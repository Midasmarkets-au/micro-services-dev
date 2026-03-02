using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Vendor.OFAPay;
using Bacera.Gateway.Vendor.Pay247;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Withdraw;

public partial class WithdrawalService(
    TenantDbContext tenantCtx,
    MyDbContextPool pool,
    PaymentMethodService paymentMethodSvc,
    ITradingApiService tradingApiSvc,
    AccountManageService accManageSvc,
    SupplementService supplementSvc,
    IHttpClientFactory clientFactory,
    ConfigService cfgSvc,
    ILogger<WithdrawalService> logger)
{
    public async Task<Withdrawal?> CreateWithdrawalForAccountAsync(long methodId, long accountId,
        long amount, dynamic? request = null, string? note = null, long operatorPartyId = 1)
    {
        var method = await paymentMethodSvc.GetMethodByIdAsync(methodId);
        if (method == null) return null;
        // var supplement = Supplement.WithdrawalSupplementV2.Build(method.Id, amount, accountId, note, request);

        var account = await tenantCtx.Accounts.Where(x => x.Id == accountId)
            .Select(x => new { x.Id, x.CurrencyId, x.PartyId, x.FundType })
            .SingleAsync();
        
        // Use ExLink-specific exchange rate method for ExLinkGlobal (tries API first, then DB)
        // For ExLink: Use buying rate (platform buys currency from user during withdrawal)
        // For other platforms: Use standard buying rate method (DB only)
        var exchangeRage = (PaymentPlatformTypes)method.Platform == PaymentPlatformTypes.ExLinkGlobal
            ? await paymentMethodSvc.GetExLinkCashierBuyingExchangeRateAsync((CurrencyTypes)account.CurrencyId, (CurrencyTypes)method.CurrencyId)
            : await paymentMethodSvc.GetBuyingExchangeRateAsync((CurrencyTypes)account.CurrencyId, (CurrencyTypes)method.CurrencyId);
        
        if (exchangeRage == -1) return null;

        var withdrawal = Withdrawal.Build(account.PartyId, (FundTypes)account.FundType, amount,
            (CurrencyTypes)account.CurrencyId);
        withdrawal.SourceAccountId = accountId;
        withdrawal.ExchangeRate = exchangeRage;
        withdrawal.IdNavigation.AddActivity(operatorPartyId, ActionTypes.WithdrawalCreate);
        withdrawal.Payment = Payment.Build(account.PartyId, LedgerSideTypes.Debit, methodId, amount,
            (CurrencyTypes)account.CurrencyId);
        await accManageSvc.TryAddAccountingTransitionLogForAccount("WithdrawalCreated", 0,
            (int)StateTypes.WithdrawalCreated, operatorPartyId,
            account.Id);

        tenantCtx.Withdrawals.Add(withdrawal);
        await tenantCtx.SaveChangesAsync();

        var supplement = new Supplement.WithdrawalSupplement
        {
            SourcePartyId = account.PartyId,
            ExchangeRate = exchangeRage,
            TargetCurrencyId = (CurrencyTypes)method.CurrencyId,
            Reference = new { Amount = amount, Request = request }
        };

        await supplementSvc.CreateAsync(SupplementTypes.Withdraw, withdrawal.Id, supplement.ToJson());
        return withdrawal;
    }

    public async Task<Withdrawal?> CreateWithdrawalForWalletAsync(long methodId, long walletId,
        long amount, dynamic? request = null, string? note = null, long operatorPartyId = 1)
    {
        var method = await paymentMethodSvc.GetMethodByIdAsync(methodId);
        if (method == null) return null;

        var wallet = await tenantCtx.Wallets.Where(x => x.Id == walletId).SingleAsync();
        
        // Use ExLink-specific exchange rate method for ExLinkGlobal (tries API first, then DB)
        // For ExLink: Use buying rate (platform buys currency from user during withdrawal)
        // For other platforms: Use standard buying rate method (DB only)
        var exchangeRage = (PaymentPlatformTypes)method.Platform == PaymentPlatformTypes.ExLinkGlobal
            ? await paymentMethodSvc.GetExLinkCashierBuyingExchangeRateAsync((CurrencyTypes)wallet.CurrencyId, (CurrencyTypes)method.CurrencyId)
            : await paymentMethodSvc.GetBuyingExchangeRateAsync((CurrencyTypes)wallet.CurrencyId, (CurrencyTypes)method.CurrencyId);
        
        if (exchangeRage == -1) return null;

        var withdrawal = Withdrawal.Build(wallet.PartyId, (FundTypes)wallet.FundType, amount,
            (CurrencyTypes)wallet.CurrencyId);
        withdrawal.SourceAccountId = null;
        withdrawal.SourceWalletId = walletId;
        withdrawal.ExchangeRate = exchangeRage;
        withdrawal.IdNavigation.AddActivity(operatorPartyId, ActionTypes.WithdrawalCreate);
        withdrawal.Payment = Payment.Build(wallet.PartyId, LedgerSideTypes.Debit, methodId, amount,
            (CurrencyTypes)wallet.CurrencyId);

        tenantCtx.Withdrawals.Add(withdrawal);
        await tenantCtx.SaveChangesAsync();

        var supplement = new Supplement.WithdrawalSupplement
        {
            SourcePartyId = wallet.PartyId,
            ExchangeRate = exchangeRage,
            TargetCurrencyId = (CurrencyTypes)method.CurrencyId,
            Reference = new { Amount = amount, Request = request }
        };

        await supplementSvc.CreateAsync(SupplementTypes.Withdraw, withdrawal.Id, supplement.ToJson());
        return withdrawal;
    }

    public async Task<bool> IsAmountValidForAccountWithdrawal(long accountId, long amount)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == accountId)
            .Select(x => new
            {
                x.ServiceId, x.AccountNumber,
                Equity = (decimal)x.TradeAccountStatus!.Equity,
                Credit = (decimal)x.TradeAccountStatus!.Credit,
                x.TradeAccountStatus
            })
            .SingleAsync();

        var tradeAccountStatus = account.TradeAccountStatus;
        if (tradeAccountStatus == null) return false;

        if (pool.GetPlatformByServiceId(account.ServiceId) == PlatformTypes.MetaTrader5)
        {
            try
            {
                var info = await tradingApiSvc.GetAccountInfoAsync(account.ServiceId, account.AccountNumber);
                info.ApplyToTradeAccountStatus(tradeAccountStatus);
                tenantCtx.TradeAccountStatuses.Update(tradeAccountStatus);
                await tenantCtx.SaveChangesAsync();
                // *** Scale * 10000 coz amount has been scaled *** //
                return (info.Equity - info.Credit).ToScaledFromCents() >= amount / 100m;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, 
                    "Failed to get MT5 account info for withdrawal validation. AccountId: {AccountId}, AccountNumber: {AccountNumber}, ServiceId: {ServiceId}, Amount: {Amount}",
                    accountId, account.AccountNumber, account.ServiceId, amount);
                return false;
            }
        }

        var amountInDecimal = amount / 100m;
        return account.Equity - account.Credit >= amountInDecimal;
    }

    public async Task<bool> IsAmountValidForWalletWithdrawal(long walletId, long amount)
    {
        var balance = await tenantCtx.Wallets.Where(x => x.Id == walletId).Select(x => x.Balance)
            .SingleOrDefaultAsync();
        return balance >= amount;
    }

    private async Task<Supplement.BankWithdrawalRequest?> GetBankWithdrawalRequestByIdAsync(long id)
    {
        var rawData = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.Withdraw && x.RowId == id)
            .Select(x => x.Data)
            .SingleOrDefaultAsync();
        if (rawData == null) return null;

        try
        {
            var obj = JsonConvert.DeserializeObject<dynamic>(rawData)!;
            var requestRaw = (string)JsonConvert.SerializeObject(obj.Reference.Request);
            var request = JsonConvert.DeserializeObject<Supplement.BankWithdrawalRequest>(requestRaw)!;
            request.ExchangeRate = (decimal)obj.ExchangeRate;
            // request.CurrencyId = (CurrencyTypes)(int)obj.CurrencyId;
            request.TrimStrings();
            return request;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to deserialize bank withdrawal request data");
            return null;
        }
    }

    public async Task<PayoutResponseModel> CreateOfaDfByWithdrawalIdAsync(long id)
    {
        var withdrawal = await tenantCtx.Withdrawals
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Amount,
                x.PartyId,
                x.Payment.Number
            })
            .SingleAsync();

        var request = await GetBankWithdrawalRequestByIdAsync(id);
        if (request == null)
        {
            return PayoutResponseModel.Fail("Failed to get bank withdrawal request");
        }

        var methods = await paymentMethodSvc.GetMethodsAsync();

        var method = methods.FirstOrDefault(x => x is
        {
            Platform: (int)PaymentPlatformTypes.OFAPay,
            MethodType: PaymentMethodTypes.Withdrawal
        });

        if (method == null)
        {
            return PayoutResponseModel.Fail("Failed to get payment method");
        }

        var bankNoDict = await cfgSvc.GetAsync<Dictionary<string, string>>(
            nameof(Public)
            , 0
            , ConfigKeys.OfaDfBankNoDict);

        if (bankNoDict == null || !bankNoDict.TryGetValue(request.BankName, out var bankNo))
        {
            return PayoutResponseModel.Fail("Failed to get bank info");
        }

        var options = OFAPayOptions.FromJson(method.Configuration);
        if (options.SecretKey == "" || options.EndPoint == "")
        {
            return PayoutResponseModel.Fail("Failed to get OFAPay configuration");
        }

        var client = new OFAPay.DFRequestClient
        {
            Amount = Math.Floor(withdrawal.Amount * request.ExchangeRate / 100m),
            AccountName = request.Holder,
            BankNumber = request.AccountNo,
            BankName = request.BankName,
            BankCode = bankNo,
            NotifyUrl = request.ReturnUrl,
            PaymentNumber = withdrawal.Number,
            // PaymentNumber = Payment.GenerateNumber(),
            Options = options,
            Logger = logger,
            Client = clientFactory.CreateClient(),
        };

        var response = await client.RequestAsync();
        return response;
    }
}

// public async Task<Dictionary<string, string>> GetBankNoDictAsync(long paymentMethodId, CurrencyTypes? currencyId)
// {
//     var item = await tenantCtx.Supplements
//         .Where(x => x.Type == (int)SupplementTypes.WithdrawBackCode)
//         .Where(x => x.RowId == paymentMethodId)
//         .OrderByDescending(x => x.Id)
//         .Select(x => new { x.Data })
//         .FirstOrDefaultAsync();
//
//     var data = JsonConvert.DeserializeObject<Dictionary<CurrencyTypes, Dictionary<string, string>>>(item!.Data);
//     return data![currencyId!.Value];
// }

// public async Task<DepositCreatedResponseModel> CreatePay247DfByWithdrawalIdAsync(Pay247.CreatePayoutSpec spec)
// {
//     var withdrawal = await tenantCtx.Withdrawals
//         .Where(x => x.Id == spec.WithdrawalId)
//         .Select(x => new
//         {
//             x.Amount,
//             x.PartyId,
//             x.Payment.Number
//         })
//         .SingleAsync();
//
//     var request = await GetBankWithdrawalRequestByIdAsync(spec.WithdrawalId);
//     if (request == null)
//         return DepositCreatedResponseModel.Fail("Failed to get bank withdrawal request");
//
//     var methods = await paymentMethodSvc.GetMethodsAsync();
//
//     var method = methods.FirstOrDefault(x => x is
//     {
//         Platform: (int)PaymentPlatformTypes.Pay247,
//         MethodType: PaymentMethodTypes.Withdrawal
//     });
//
//     if (method == null)
//         return DepositCreatedResponseModel.Fail("Failed to get payment method");
//
//     var options = Pay247Options.FromJson(method.Configuration);
//     var exchangedAmount = Math.Floor(withdrawal.Amount * request.ExchangeRate / 100m);
//     var client = new Pay247.PayoutRequestClient
//     {
//         Amount = exchangedAmount,
//         AccountName = request.Name,
//         AccountNumber = request.AccountNo,
//         PaymentNumber = withdrawal.Number,
//         BankCode = spec.BankCode,
//         CurrencyId = request.CurrencyId,
//         Logger = logger,
//         Options = options,
//         Client = clientFactory.CreateClient(),
//     };
//
//     var response = await client.RequestAsync();
//     return response;
// }