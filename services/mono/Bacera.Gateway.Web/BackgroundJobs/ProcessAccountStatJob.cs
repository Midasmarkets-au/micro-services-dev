using Bacera.Gateway.Context;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

// Later we may use Message Queue to process the job, but for now we use Hangfire
public class ProcessAccountStatJob : IProcessAccountStatJob
{
    private readonly MyDbContextPool _myDbContextPool;
    private readonly ILogger<ProcessAccountStatJob> _logger;

    public ProcessAccountStatJob(MyDbContextPool myDbContextPool, ILogger<ProcessAccountStatJob> logger)
    {
        _logger = logger;
        _myDbContextPool = myDbContextPool;
    }

    public async Task<(bool, string)> ClientAccountAddedAsync(long tenantId, long accountId)
    {
        var parentIds = await GetParentAccountIdsAsync(tenantId, accountId);
        var tasks = parentIds.Select(parentId =>
            ConcurrentProcessTodayAccountStatAsync(tenantId, parentId, x => x.NewAccountCount++));
        await Task.WhenAll(tasks);
        return (true, "Client account added");
    }


    public async Task<(bool, string)> AgentAccountAddedAsync(long tenantId, long accountId)
    {
        var isAgent = await CheckIfAgentAccountAsync(tenantId, accountId);
        if (!isAgent) return (false, "Account is not an agent");
        var parentIds = await GetParentAccountIdsAsync(tenantId, accountId);
        var tasks = parentIds.Select(parentId =>
            ConcurrentProcessTodayAccountStatAsync(tenantId, parentId, x => x.NewAgentCount++));
        await Task.WhenAll(tasks);
        return (true, "Agent account added");
    }

    public async Task<(bool, string)> DepositApprovedAsync(long tenantId, long depositId)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            var deposit = await ctx.Deposits
                .Where(x => x.Id == depositId)
                .Select(x => new { x.Amount, x.TargetAccountId })
                .SingleOrDefaultAsync();
            if (deposit?.TargetAccountId == null) return (false, "Deposit not found");
            var parentIds = await GetParentAccountIdsAsync(tenantId, deposit.TargetAccountId.Value);
            var tasks = parentIds.Select(parentId => ConcurrentProcessTodayAccountStatAsync(tenantId, parentId,
                x =>
                {
                    x.DepositAmount += deposit.Amount;
                    x.DepositCount++;
                }));
            await Task.WhenAll(tasks);
            return (true, "Deposit approved");
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }

    public async Task<(bool, string)> WithdrawalApprovedAsync(long tenantId, long withdrawalId)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            var withdrawal = await ctx.Withdrawals
                .Where(x => x.Id == withdrawalId)
                .Select(x => new { x.Amount, x.SourceAccountId })
                .SingleOrDefaultAsync();
            if (withdrawal?.SourceAccountId == null) return (false, "Withdrawal not found");
            var parentIds = await GetParentAccountIdsAsync(tenantId, withdrawal.SourceAccountId.Value);
            var tasks = parentIds.Select(parentId => ConcurrentProcessTodayAccountStatAsync(tenantId, parentId,
                x =>
                {
                    x.WithdrawAmount += withdrawal.Amount;
                    x.WithdrawCount++;
                }));
            await Task.WhenAll(tasks);
            return (true, "Withdrawal approved");
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }

    public async Task<(bool, string)> RebateReleasedAsync(long tenantId, long rebateId)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            var rebate = await ctx.Rebates
                .Where(x => x.Id == rebateId)
                .Select(x => new { x.Amount, x.AccountId })
                .SingleOrDefaultAsync();
            if (rebate == null) return (false, "Rebate not found");
            var parentIds = await GetParentAccountIdsAsync(tenantId, rebate.AccountId);
            var tasks = parentIds.Select(parentId => ConcurrentProcessTodayAccountStatAsync(tenantId, parentId,
                x =>
                {
                    x.RebateAmount += rebate.Amount;
                    x.RebateCount++;
                }));
            await Task.WhenAll(tasks);
            return (true, "Rebate received");
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }

    public async Task<(bool, string)> TradeClosedAsync(long tenantId, long tradeRebateId)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            var tradeRebate = await ctx.TradeRebates
                .Where(x => x.Id == tradeRebateId)
                .Select(x => new { x.Volume, x.Symbol, x.Profit, x.AccountId })
                .SingleOrDefaultAsync();
            if (tradeRebate?.AccountId == null) return (false, "Trade rebate not found");
            var parentIds = await GetParentAccountIdsAsync(tenantId, tradeRebate.AccountId.Value);

            var tasks = parentIds.Select(parentId => ConcurrentProcessTodayAccountStatAsync(tenantId, parentId,
                x =>
                {
                    var profit = (long)(tradeRebate.Profit * 100);
                    x.TradeVolume += tradeRebate.Volume;
                    x.TradeProfit += profit;
                    x.TradeCount++;
                    var tradeSymbolStat = AccountStat.SymbolStatModel.FromJson(x.TradeSymbol);
                    var symbolStat = tradeSymbolStat
                        .GetValueOrDefault(tradeRebate.Symbol, AccountStat.SymbolStatModel.Build());
                    symbolStat.TotalTrades++;
                    symbolStat.TotalProfit += profit;
                    symbolStat.TotalVolume += tradeRebate.Volume;
                    tradeSymbolStat[tradeRebate.Symbol] = symbolStat;
                    x.TradeSymbol = JsonConvert.SerializeObject(tradeSymbolStat);
                }));
            await Task.WhenAll(tasks);
            return (true, "Trade closed");
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }

    private async Task<List<long>> GetParentAccountIdsAsync(long tenantId, long accountId)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            var path = await ctx.Accounts
                .Where(x => x.Id == accountId)
                .Select(x => x.ReferPath)
                .SingleOrDefaultAsync();
            if (path == null) return new List<long>();
            var uids = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
            var ids = await ctx.Accounts
                .Where(x => uids.Contains(x.Uid))
                .Select(x => x.Id)
                .ToListAsync();
            return ids;
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }

    private async Task<bool> CheckIfAgentAccountAsync(long tenantId, long accountId)
    {
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        try
        {
            var result = await ctx.Accounts
                .AnyAsync(x => x.Id == accountId && x.Role == (short)AccountRoleTypes.Agent);
            return result;
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }

    private async Task ConcurrentProcessTodayAccountStatAsync(long tenantId, long accountId,
        Action<AccountStat> processHandler)
    {
        var today = DateTime.UtcNow.Date;
        var ctx = await _myDbContextPool.BorrowTenant(tenantId);
        await using var transaction = await ctx.Database.BeginTransactionAsync();
        try
        {
            var item = await ctx.AccountStats
                .FromSqlInterpolated(
                    $"select * from trd.\"_AccountStat\" t where \"AccountId\" = {accountId} and \"Date\" = {today} for update"
                )
                .SingleOrDefaultAsync();
            if (item == null)
            {
                item = AccountStat.BuildToday(accountId, today);
                try
                {
                    ctx.AccountStats.Add(item);
                    await ctx.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    item = await ctx.AccountStats
                        .FromSqlInterpolated(
                            $"select * from trd.\"_AccountStat\" t where \"AccountId\" = {accountId} and \"Date\" = {today} for update"
                        )
                        .SingleAsync();
                }
            }

            processHandler(item);
            ctx.AccountStats.Update(item);
            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error processing account stat");
        }
        finally
        {
            _myDbContextPool.ReturnTenant(ctx);
        }
    }
}