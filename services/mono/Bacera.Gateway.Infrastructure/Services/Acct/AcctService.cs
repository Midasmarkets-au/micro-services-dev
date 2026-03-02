using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService(
    TenantDbContext tenantCtx,
    TenantDbConnection tenantCon,
    ITradingApiService apiService,
    IMyCache myCache,
    ConfigService cfgSvc,
    MyDbContextPool pool,
    ILogger<AcctService> logger,
    ITenantGetter tenantGetter)
{
    private readonly long _tenantId = tenantGetter.GetTenantId();

    private void TransitRaw(IHasMatter matter, StateTypes toStateId, long operatorPartyId, string note = "")
    {
        var now = DateTime.UtcNow;
        var activity = new Activity
        {
            PartyId = operatorPartyId,
            MatterId = matter.Id,
            PerformedOn = now,
            OnStateId = matter.IdNavigation.StateId,
            ActionId = 0,
            ToStateId = (int)toStateId,
            Data = note
        };
        matter.IdNavigation.StateId = (int)toStateId;
        if (matter.IdNavigation.StateId != (int)StateTypes.DepositCallbackCompleted)
        {
            matter.IdNavigation.StatedOn = now;
        }

        matter.IdNavigation.Activities.Add(activity);
        tenantCtx.Matters.Update(matter.IdNavigation);
    }

    private async Task<(bool, string)> ProcessMatterAsync(long matterId, Func<Task> handler)
    {
        var lockKey = DistributedLockKeys.GetProcessMatterKey(_tenantId, matterId);
        var lockValue = Guid.NewGuid().ToString();
        if (!myCache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromMinutes(1)))
            return (false, "Matter is processing");

        var transaction = await tenantCtx.Database.BeginTransactionAsync();
        try
        {
            await handler();
            await tenantCtx.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (ProcessMatterException e)
        {
            logger.LogInformation($"ProcessMatterAsync_ProcessMatterException_id:{matterId}_tid:{_tenantId}_error:{e.Message}");
            await transaction.RollbackAsync();
            return (false, e.Message);
        }
        // catch (InvalidOperationException e)
        // {
        //     logger.LogInformation($"ProcessMatterAsync_InvalidOperationException_id:{matterId}_tid:{_tenantId}_error:{e.Message}");
        //     await transaction.RollbackAsync();
        //     return (false, e.Message);
        // }
        catch (Exception e)
        {
            BcrLog.Slack($"ProcessMatterAsync_id_{matterId}_tid_{_tenantId}_error_{e.Message}");
            await transaction.RollbackAsync();
            return (false, e.Message);
        }
        finally
        {
            myCache.ReleaseDistributedLock(lockKey, lockValue);
        }

        return (true, "");
    }

    public async Task<(bool, int, double)> TradeAccountUpdateBalanceAndLeverageAsync(long accountId)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == accountId)
            .Select(x => new { x.ServiceId, x.AccountNumber })
            .SingleOrDefaultAsync();
        if (account == null) return (false, 0, 0);
        try
        {
            var (res, leverage, balance) = await apiService.GetAccountBalanceAndLeverage(account.ServiceId, account.AccountNumber);
            if (!res) return (true, 0, 0);

            var status = await tenantCtx.TradeAccountStatuses.SingleAsync(x => x.Id == accountId);
            status.Leverage = leverage;
            status.Balance = balance;
            return (true, status.Leverage, status.Balance);
        }
        catch (Exception e)
        {
            // ignored
            return (false, 0, 0);
        }
    }

    private Task<Wallet?> GetWalletAsync(long partyId, CurrencyTypes currencyId, FundTypes fundType)
        => tenantCtx.Wallets
            .Where(x => x.PartyId == partyId && x.CurrencyId == (int)currencyId && x.FundType == (int)fundType)
            .SingleOrDefaultAsync();

    /// <summary>
    /// Change wallet balance without transaction, the caller should handle transaction
    /// </summary>
    /// <param name="walletId"></param>
    /// <param name="matterId"></param>
    /// <param name="amount"></param>
    private async Task WalletChangeBalanceRawAsync(long walletId, long matterId, long amount)
    {
        // Always use FOR UPDATE lock for wallet balance changes to prevent race conditions
        var wallet = await tenantCtx.Wallets
            .FromSqlRaw("SELECT * FROM acct.\"_Wallet\" WHERE \"Id\" = {0} FOR UPDATE", walletId)
            .SingleOrDefaultAsync();
            
        if (wallet == null)
        {
            throw new ProcessMatterException($"Wallet {walletId} not found");
        }

        var prevBalance = wallet.Balance;
        wallet.Balance += amount;
        tenantCtx.WalletTransactions.Add(wallet.TransactionByMatter(matterId, prevBalance, amount));
    }
}