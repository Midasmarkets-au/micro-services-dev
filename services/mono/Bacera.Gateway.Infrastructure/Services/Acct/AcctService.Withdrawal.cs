using Bacera.Gateway.MyException;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public async Task<(bool, string)> CancelWithdrawalAsync(long id, long operatorPartyId = 1, string comment = "")
        => await ProcessMatterAsync(id, async () =>
        {
            var item = await tenantCtx.Withdrawals
                .Include(x => x.IdNavigation)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (item == null) throw new ProcessMatterException("Withdrawal not found");
            if (item.IdNavigation.StateId != (int)StateTypes.WithdrawalCreated) throw new ProcessMatterException("Withdrawal not in created state");

            TransitRaw(item, StateTypes.WithdrawalCanceled, operatorPartyId);
            item.ApprovedOn = DateTime.UtcNow;
        });

    public async Task<(bool, string)> ApproveWithdrawalAsync(long id, long operatorPartyId = 1, string comment = "")
        => await ProcessMatterAsync(id, async () =>
        {
            var item = await tenantCtx.Withdrawals
                .Include(x => x.IdNavigation)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (item == null) throw new ProcessMatterException("Withdrawal not found");
            if (item.IdNavigation.StateId != (int)StateTypes.WithdrawalCreated) throw new ProcessMatterException("Withdrawal not in created state");

            if (item.SourceAccountId != null)
            {
                await WithdrawFromAccountAsync(item, comment);
            }
            else
            {
                await WithdrawalFromWalletAsync(item, comment);
            }

            TransitRaw(item, StateTypes.WithdrawalTenantApproved, operatorPartyId);
            item.ApprovedOn = DateTime.UtcNow;
        });

    private async Task WithdrawalFromWalletAsync(Withdrawal item, string comment)
    {
        var wallet = await GetWalletAsync(item.PartyId, (CurrencyTypes)item.CurrencyId, (FundTypes)item.FundType);
        if (wallet == null) throw new ProcessMatterException("Wallet not found");

        if (wallet.Balance < item.Amount) throw new ProcessMatterException("__BALANCE_NOT_ENOUGH__");
        var amountForWallet = -1 * Math.Abs(item.Amount);
        await WalletChangeBalanceRawAsync(wallet.Id, item.Id, amountForWallet);
    }

    private async Task WithdrawFromAccountAsync(Withdrawal item, string comment = "")
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == item.SourceAccountId && x.Status == 0)
            .Where(x => x.TradeAccountStatus != null)
            .Select(x => new { x.ServiceId, x.AccountNumber, x.TradeAccountStatus!.Equity, x.CurrencyId })
            .SingleOrDefaultAsync();

        if (account == null) throw new ProcessMatterException("Account not found or status not valid");

        // Compare equity with the amount with the same currency
        // Try call CurrencyConversionHelper to get the conversion rate 
        // Convert withdrawal amount to account currency for comparison
        var accountCurrency = (CurrencyTypes)account.CurrencyId;
        var withdrawalCurrency = (CurrencyTypes)item.CurrencyId;
        decimal withdrawalAmountInAccountCurrency = 0;
        if (accountCurrency == withdrawalCurrency)
        {
            // Same currency, no conversion needed
            withdrawalAmountInAccountCurrency = item.Amount / 100m; // Convert from scaled to units
        }
        else
        {
            // Different currencies, need exchange rate
            var exchangeRate = await GetExchangeRateAsync(withdrawalCurrency, accountCurrency);
            if (exchangeRate <= 0)
                throw new ProcessMatterException($"Exchange rate not available from {withdrawalCurrency} to {accountCurrency}");

            // Convert: withdrawal amount (scaled) -> units -> account currency units
            var withdrawalAmountInUnits = item.Amount / 100m; // Convert from scaled to withdrawal currency units
            withdrawalAmountInAccountCurrency = withdrawalAmountInUnits * exchangeRate;
        }

        // Convert both to long for precise comparison (avoiding floating-point precision issues)
        var equityScaled = (long)Math.Round(account.Equity.ToScaledFromCents(), 0, MidpointRounding.AwayFromZero);
        if (equityScaled < withdrawalAmountInAccountCurrency) throw new ProcessMatterException("__SOURCE_ACCOUNT_BALANCE_NOT_ENOUGH__");
        var amountForAccount = -1 * (decimal)item.Amount / 100;

        var (result, ticket) = await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amountForAccount, comment);
        if (!result)
        {
            BcrLog.Slack($"WithdrawFromAccountAsync_id:{item.Id}_api_change_balance_error:{ticket}");
            throw new ProcessMatterException("__CHANGE_TRADE_ACCOUNT_BALANCE_FAILED__");
        }

        item.ReferenceNumber = ticket;
        await TradeAccountUpdateBalanceAndLeverageAsync(item.SourceAccountId!.Value);
    }

    public async Task<bool> IsUSDTWalletExistAsync(string walletAddress)
    {
        var item = await tenantCon.FirstOrDefaultAsync<bool>($"""
                                                              SELECT EXISTS (SELECT 1
                                                              FROM acct."_PaymentInfo"
                                                              WHERE "PaymentPlatform" = {(int)PaymentPlatformTypes.USDT}
                                                                AND "Info"::jsonb ->> 'walletAddress' = '{walletAddress}') AS "Exists";
                                                              """);

        return item;
    }

    public Task<(bool, string)> WithdrawalRestoreToInitialAsync(long id, long operatorId = 1, string comment = "")
    => ProcessMatterAsync(id, async () =>
    {
        var item = await tenantCtx.Withdrawals
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) throw new ProcessMatterException("Withdrawal not found");

        comment = $"{comment}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
        TransitRaw(item, StateTypes.WithdrawalCreated, operatorId, comment);
        item.Payment.Status = (int)PaymentStatusTypes.Pending;
        item.Payment.UpdatedOn = DateTime.UtcNow;
    });

}