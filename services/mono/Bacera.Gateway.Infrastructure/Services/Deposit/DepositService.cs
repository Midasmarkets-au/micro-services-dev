using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Services;

public partial class DepositService(
    TenantDbContext tenantCtx,
    PaymentMethodService paymentMethodSvc,
    IMyCache cache,
    UserService userSvc,
    ISendMessageService messageSvc,
    ConfigService cfgSvc,
    CryptoService cryptoSvc,
    ITenantGetter getter,
    IHttpClientFactory clientFactory,
    ILogger<DepositService> logger,
    SupplementService supplementService)
{
    private readonly long _tenantId = getter.GetTenantId();

    public async Task<DepositCreatedResponseModel> CreateDepositForAccountAsync(long methodId, long accountId,
        long amount, Dictionary<string, string> request, string? note = null, long operatorPartyId = 1)
    {
        
        var method = await paymentMethodSvc.GetMethodByIdAsync(methodId);
        if (method == null) return DepositCreatedResponseModel.Fail("Payment method not found");
        var supplement = Supplement.DepositSupplementV2.Build(method.Id, amount, accountId, note, request);
        var account = await tenantCtx.Accounts.Where(x => x.Id == accountId).SingleAsync();

        var targetCurrency = request.TryGetValue("currencyId", out var valRaw) && int.TryParse(valRaw, out var val)
            ? (CurrencyTypes)val
            : (CurrencyTypes)method.CurrencyId;

        // Use ExLink-specific exchange rate method for ExLinkCashier (tries API first, then DB)
        // Use standard method for all other platforms (DB only)
        var exchangeRage = (PaymentPlatformTypes)method.Platform == PaymentPlatformTypes.ExLinkGlobal
            ? await paymentMethodSvc.GetExLinkCashierExchangeRateAsync((CurrencyTypes)account.CurrencyId, targetCurrency)
            : await paymentMethodSvc.GetSellingExchangeRateAsync((CurrencyTypes)account.CurrencyId, targetCurrency);

        if (exchangeRage == -1) return DepositCreatedResponseModel.Fail("Exchange rate not found");
        var exchangedAmount = RoundUp(amount * exchangeRage);

        Func<PaymentMethod, Account, long, Dictionary<string, string>, Task<DepositCreatedResponseModel>> handler =
            (PaymentPlatformTypes)method.Platform switch
            {
                PaymentPlatformTypes.Wire => ProcessWireAsync,
                PaymentPlatformTypes.Ocex => ProcessOcexAsync,
                PaymentPlatformTypes.NPay => ProcessNPayAsync,
                PaymentPlatformTypes.GPay => ProcessGPayAsync,
                PaymentPlatformTypes.EuPay => ProcessEuPayAsync,
                PaymentPlatformTypes.Poli => ProcessPoliPayAsync,
                PaymentPlatformTypes.Manual => ProcessManualAsync,
                PaymentPlatformTypes.BigPay => ProcessBigPayAsync,
                PaymentPlatformTypes.UEnjoy => ProcessUEnjoyAsync,
                PaymentPlatformTypes.ExLink => ProcessExLinkAsync,
                PaymentPlatformTypes.UsePay => ProcessUsePayAsync,
                PaymentPlatformTypes.Bakong => ProcessBakongAsync,
                PaymentPlatformTypes.Pay247 => ProcessPay247Async,
                PaymentPlatformTypes.OFAPay => ProcessOFAPayAsync,
                PaymentPlatformTypes.Crypto => ProcessCryptoAsync,
                PaymentPlatformTypes.PayPal => ProcessPayPalAsync,
                PaymentPlatformTypes.ChipPay => ProcessChipPayAsync,
                PaymentPlatformTypes.BipiPay => ProcessBipiPayAsync,
                PaymentPlatformTypes.Monetix => ProcessMonetixAsync,
                PaymentPlatformTypes.Help2Pay => ProcessHelp2PayAsync,
                PaymentPlatformTypes.Long77Pay => ProcessLong77PayAsync,
                PaymentPlatformTypes.ExLinkGlobal => ProcessExLinkCashierAsync,
                PaymentPlatformTypes.FivePayVA => ProcessFivePayVAAsync,
                PaymentPlatformTypes.DragonPay => ProcessDragonPayAsync,
                PaymentPlatformTypes.ChinaPay => ProcessChinaPayPayAsync,
                PaymentPlatformTypes.Help2PayCNY => ProcessHelp2PayAsync,
                PaymentPlatformTypes.UnionePayX => ProcessUnionePayXAsync,
                PaymentPlatformTypes.FivePayF2F => ProcessFivePayF2FAsync,
                PaymentPlatformTypes.Long77PayUsdt => ProcessLong77PayUsdtAsync,
                PaymentPlatformTypes.PaymentAsiaRMB => ProcessPaymentAsiaRMBAsync,
                PaymentPlatformTypes.Buzipay => ProcessBuzipayAsync,
                PaymentPlatformTypes.QrCodeTunnel => ProcessQrCodeTunnelAsync,

                PaymentPlatformTypes.Undefined =>
                    throw new Exception($"Invalid payment method {PaymentPlatformTypes.Undefined}"),
                PaymentPlatformTypes.System =>
                    throw new Exception($"Invalid payment method {PaymentPlatformTypes.System}"),
                PaymentPlatformTypes.Cash =>
                    throw new Exception($"Invalid payment method {PaymentPlatformTypes.Cash}"),
                PaymentPlatformTypes.Check =>
                    throw new Exception($"Invalid payment method {PaymentPlatformTypes.Check}"),
                PaymentPlatformTypes.USDT =>
                    throw new Exception($"Invalid payment method {PaymentPlatformTypes.USDT}"),
                _ =>
                    throw new Exception("Invalid payment method, Not Found")
            };

        await using var transaction = await tenantCtx.Database.BeginTransactionAsync();
        try
        {
            var result = await handler(method, account, exchangedAmount.ToCentsFromScaled(), request);
            if (!result.IsSuccess)
            {
                var message = $"Failed to process {method.Name}: {result.Message}; Login: {account.AccountNumber}";
                var notice = MessagePopupDTO.BuildWarning($"{method.Name} Process Failed", message);
                await messageSvc.SendPopupToManagerAsync(_tenantId, notice);
                return result;
            }

            var payment = await CreatePaymentAsync(account.PartyId
                , methodId
                , exchangedAmount
                , result.PaymentNumber
                , result.Reference);

            if ((PaymentPlatformTypes)method.Platform == PaymentPlatformTypes.QrCodeTunnel
                && !string.IsNullOrWhiteSpace(result.TransactionId))
            {
                var snap = new JObject
                {
                    ["qrTunnelTransactionId"] = result.TransactionId,
                    ["qrTunnelCreateResponse"] = result.Info != null ? JToken.FromObject(result.Info) : null
                };
                payment.CallbackBody = snap.ToString(Formatting.None);
                payment.UpdatedOn = DateTime.UtcNow;
                await tenantCtx.SaveChangesAsync();
            }

            var deposit = await CreateDepositAsync(account
                , payment.Id
                , amount
                , operatorPartyId
                , JsonConvert.SerializeObject(supplement));

            await result.CreatedCbHandler(deposit);
            await transaction.CommitAsync();
            var user = await userSvc.GetPartyAsync(account.PartyId);
            result.Instruction = await paymentMethodSvc.GetInstructionAsync(method, user.Language);
            result.DepositId = deposit.Id;
            return result;
        }
        catch (Exception e)
        {
            BcrLog.Slack(
                $"CreateDepositForAccountAsync_Error_Execute_ThirdParty_Payment: platform: {method.Platform}, msg: {e.Message}");
            await transaction.RollbackAsync();
            return DepositCreatedResponseModel.Fail("Failed to process payment");
        }
    }

    private async Task<Payment> CreatePaymentAsync(long partyId, long methodId, long exchangedAmount,
        string paymentNumber, string? referenceNumber = null, string note = "Prepare for Deposit")
    {
        var method = await paymentMethodSvc.GetMethodByIdAsync(methodId);
        var payment = Payment.BuildForDeposit(partyId
            , methodId
            , exchangedAmount
            , (CurrencyTypes)method!.CurrencyId
            , paymentNumber
            , referenceNumber);

        tenantCtx.Payments.Add(payment);
        await tenantCtx.SaveChangesAsync();

        await cache.SetStringAsync(Payment.GetPaymentNumberTenantIdKey(paymentNumber)
            , _tenantId.ToString()
            , TimeSpan.FromDays(2));

        if (referenceNumber != null)
        {
            await cache.SetStringAsync(Payment.GetReferenceNumberTenantIdKey(referenceNumber)
                , _tenantId.ToString()
                , TimeSpan.FromDays(2));
        }

        var supplementObj = new Supplement.PaymentSupplement { Note = note };
        var supplementEntity = new Supplement
        {
            Type = (short)SupplementTypes.Payment,
            RowId = payment.Id,
            Data = Utils.JsonSerializeObject(supplementObj),
        };
        tenantCtx.Supplements.Add(supplementEntity);
        await tenantCtx.SaveChangesAsync();
        return payment;
    }

    private async Task<Deposit> CreateDepositAsync(Account account, long paymentId, long amount,
        long operatorPartyId = 1,
        string? supplement = null)
    {
        var deposit = Deposit.Build(account.PartyId
            , (FundTypes)account.FundType
            , (CurrencyTypes)account.CurrencyId
            , amount);

        deposit.PaymentId = paymentId;
        deposit.TargetAccountId = account.Id;
        deposit.IdNavigation.AddActivity(operatorPartyId, ActionTypes.DepositCreate);
        tenantCtx.Deposits.Add(deposit);
        await tenantCtx.SaveChangesAsync();
        if (supplement != null) await supplementService.CreateAsync(SupplementTypes.Deposit, deposit.Id, supplement);
        return deposit;
    }

    private static long RoundUp(decimal value)
    {
        var rounded = (long)Math.Round(value, 0, MidpointRounding.AwayFromZero);
        if (rounded < value) rounded++;
        return rounded;
    }
}