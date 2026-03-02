using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Extension;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    public async Task DuplicateRebate()
    {
        var from = Utils.ParseToUTC("2024-04-15 00:00:00.000000 +00:00");
        // var to = Utils.ParseToUTC("2024-04-16 13:00:34.000000 +00:00");
        // using var scope = CreateTenantScopeByTenantIdAsync(10000);
        // var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var affectedWalletIds = new HashSet<long>();
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var items = await ctx.Rebates
            .Where(x => x.TradeRebate != null)
            .Where(x => x.IdNavigation.PostedOn >= from)
            .GroupBy(x => new { x.AccountId, x.TradeRebate!.Ticket, x.Amount })
            .Select(x => new
            {
                x.Key.AccountId,
                x.Key.Ticket,
                x.Key.Amount,
                Count = x.Count()
            })
            .Where(x => x.Count > 1)
            .ToListAsync();
        _logger.LogInformation("DuplicateRebate: {items}", items);

        foreach (var item in items)
        {
            await using var tran = await ctx.Database.BeginTransactionAsync();
            try
            {
                var iis = await ctx.Rebates
                    .Where(x => x.AccountId == item.AccountId)
                    .Where(x => x.TradeRebate!.Ticket == item.Ticket)
                    .Where(x => x.Amount == item.Amount)
                    .OrderBy(x => x.IdNavigation.PostedOn)
                    .ToListAsync();

                var kept = iis.First();
                var toBeDeleted = iis.Last();
                var toBeDeletedMatter = await ctx.Matters.SingleAsync(x => x.Id == toBeDeleted.Id);
                var activities = await ctx.Activities
                    .Where(x => x.MatterId == toBeDeletedMatter.Id)
                    .ToListAsync();

                _logger.LogInformation("Removed_Rebate: {id}", toBeDeleted.Id);
                var wt = await ctx.WalletTransactions.SingleOrDefaultAsync(x => x.MatterId == toBeDeletedMatter.Id);
                if (wt != null)
                {
                    affectedWalletIds.Add(wt.WalletId);
                    ctx.WalletTransactions.Remove(wt);
                    await ctx.SaveChangesAsync();
                    var prevWt = await ctx.WalletTransactions
                        .Where(x => x.WalletId == wt.WalletId)
                        .Where(x => x.CreatedOn < wt.CreatedOn)
                        .OrderByDescending(x => x.CreatedOn)
                        .FirstOrDefaultAsync();
                    // var wt
                    if (prevWt != null)
                    {
                        var subsequentWts = await ctx.WalletTransactions
                            .Where(x => x.WalletId == prevWt.WalletId)
                            .Where(x => x.CreatedOn > prevWt.CreatedOn)
                            .OrderBy(x => x.CreatedOn)
                            .ToListAsync();

                        var prev = prevWt;
                        var finalAmount = prev.PrevBalance + prev.Amount;
                        foreach (var cur in subsequentWts)
                        {
                            cur.PrevBalance = prev.PrevBalance + prev.Amount;
                            finalAmount = cur.PrevBalance + cur.Amount;
                            prev = cur;
                        }

                        var wallet = await ctx.Wallets.SingleAsync(x => x.Id == prev.WalletId);
                        wallet.Balance = finalAmount;
                        await ctx.SaveChangesAsync();
                    }
                }

                ctx.Activities.RemoveRange(activities);
                ctx.Rebates.Remove(toBeDeleted);
                ctx.Matters.Remove(toBeDeletedMatter);
                await ctx.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch (Exception e)
            {
                await tran.RollbackAsync();
                _logger.LogError(e, "DuplicateRebate");
                throw;
            }
        }

        _logger.LogInformation("AffectedWalletIds: {ids}", string.Join(",", affectedWalletIds));
    }


    private async Task Test2()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10004);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        // await using var transaction = await ctx.Database.BeginTransactionAsync();
        try
        {
            // await ctx.Database.ExecuteSqlRawAsync("LOCK TABLE acct.\"_WalletTransaction\" IN ACCESS EXCLUSIVE MODE;");

            var matterIds = await ctx.WalletTransactions
                .OrderBy(x => x.CreatedOn)
                .Where(x => x.Matter.StateId == 550)
                .GroupBy(x => x.MatterId)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToListAsync();
            _logger.LogInformation("Test2: {count}", matterIds.Count);

            foreach (var matterId in matterIds)
            {
                var wts = await ctx.WalletTransactions
                    .Where(x => x.MatterId == matterId)
                    .OrderBy(x => x.CreatedOn)
                    .ToListAsync();

                var kept = wts.First();
                var toBeDeleted = wts.Last();

                var subsequentWts = await ctx.WalletTransactions
                    .Where(x => x.WalletId == toBeDeleted.WalletId)
                    .Where(x => x.CreatedOn > toBeDeleted.CreatedOn)
                    .OrderBy(x => x.CreatedOn)
                    .ToListAsync();

                var prev = kept;
                var finalAmount = kept.Amount + kept.PrevBalance;
                foreach (var cur in subsequentWts)
                {
                    cur.PrevBalance = prev.PrevBalance + prev.Amount;
                    finalAmount = cur.PrevBalance + cur.Amount;
                    prev = cur;
                }

                _logger.LogInformation("Adjusted count: {count}, finalAmount: {finalAmount}", subsequentWts.Count,
                    finalAmount);

                var wallet = await ctx.Wallets.SingleAsync(x => x.Id == kept.WalletId);
                wallet.Balance = finalAmount;
                ctx.WalletTransactions.Remove(toBeDeleted);
                await ctx.SaveChangesAsync();
            }

            // await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            // await transaction.RollbackAsync();
            _logger.LogError(e, "Test2");
        }
    }

    private async Task UpdateWalletByWalletTransactionId()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10004);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        // await using var transaction = await ctx.Database.BeginTransactionAsync();
        var wrongs = new[] { 4736160, }; // wallet transaction Id
        foreach (var id in wrongs)
        {
            var wt = await ctx.WalletTransactions.SingleAsync(x => x.Id == id);
            var wallet = await ctx.Wallets.SingleAsync(x => x.Id == wt.WalletId);

            var subsequentWts = await ctx.WalletTransactions
                .Where(x => x.WalletId == wt.WalletId)
                .Where(x => x.CreatedOn > wt.CreatedOn)
                .OrderBy(x => x.CreatedOn).ThenBy(x => x.Id)
                .ToListAsync();

            var prev = wt;
            var finalAmount = wt.PrevBalance + wt.Amount;
            foreach (var cur in subsequentWts)
            {
                cur.PrevBalance = prev.PrevBalance + prev.Amount;
                finalAmount = cur.PrevBalance + cur.Amount;
                prev = cur;
            }

            _logger.LogInformation("Adjusted count: {count}, finalAmount: {finalAmount}", subsequentWts.Count,
                finalAmount);

            wallet.Balance = finalAmount;
            await ctx.SaveChangesAsync();
        }
    }

    private async Task BatchUpdateWalletByWalletTransactionIdByWalletId()
    {
        var walletIds = new long[]
        {
            10260
        }; // wallet Id
        var bar = CreateBar(walletIds.Length, "Wallets");

        using var scope = CreateTenantScopeByTenantIdAsync(10004);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var wallets = await ctx.Wallets
            .Where(x => walletIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .ToListAsync();

        foreach (var wallet in wallets)
        {
            bar.Tick();
            // var wtid = await CheckWalletTransaction(ctx, wallet);
            // if (wtid == 0) continue;
            var fromDateTime = Utils.ParseToUTC("2023-12-12");
            var wtid = await ctx.WalletTransactions
                .Where(x => x.WalletId == wallet.Id)
                .Where(x => x.CreatedOn >= fromDateTime)
                .OrderBy(x => x.CreatedOn).ThenBy(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
            await UpdateWalletTransactionByTransactionId(ctx, wtid);
        }

        // const int threadCount = 5;
        // var walletsPerThread = (int)Math.Ceiling(walletIds.Length / (decimal)threadCount);
        //
        // await Task.WhenAll(Enumerable.Range(0, 5).Select(async i =>
        // {
        //     using var scope = CreateTenantScopeByTenantIdAsync(10004);
        //     var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        //     var ids = walletIds.Skip(i * walletsPerThread).Take(walletsPerThread).ToArray();
        //     var wallets = await ctx.Wallets
        //         .Where(x => ids.Contains(x.Id))
        //         .ToListAsync();
        //
        //     foreach (var wallet in wallets)
        //     {
        //         bar.Tick();
        //         // var wtid = await CheckWalletTransaction(ctx, wallet);
        //         // if (wtid == 0) continue;
        //         var fromDateTime = Utils.ParseToUTC("2025-02-10");
        //         var wtid = await ctx.WalletTransactions
        //             .Where(x => x.WalletId == wallet.Id)
        //             .Where(x => x.CreatedOn >= fromDateTime)
        //             .OrderBy(x => x.CreatedOn).ThenBy(x => x.Id)
        //             .Select(x => x.Id)
        //             .FirstOrDefaultAsync();
        //         await UpdateWalletTransactionByTransactionId(ctx, wtid);
        //     }
        // }));
    }

    private async Task UpdateWalletTransactionByTransactionId(TenantDbContext ctx, long wtId = 1)
    {
        var wrongs = new[] { wtId }; // wallet transaction Id
        foreach (var id in wrongs)
        {
            var wt = await ctx.WalletTransactions.SingleAsync(x => x.Id == id);
            var wallet = await ctx.Wallets.SingleAsync(x => x.Id == wt.WalletId);

            var subsequentWts = await ctx.WalletTransactions
                .Where(x => x.WalletId == wt.WalletId)
                .Where(x => x.CreatedOn > wt.CreatedOn)
                .OrderBy(x => x.CreatedOn).ThenBy(x => x.Id)
                .ToListAsync();

            var prev = wt;
            var finalAmount = wt.PrevBalance + wt.Amount;
            foreach (var cur in subsequentWts)
            {
                cur.PrevBalance = prev.PrevBalance + prev.Amount;
                finalAmount = cur.PrevBalance + cur.Amount;
                prev = cur;
            }

            _logger.LogInformation("Adjusted count: {count}, finalAmount: {finalAmount}", subsequentWts.Count,
                finalAmount);

            wallet.Balance = finalAmount;
            await ctx.SaveChangesAsync();
        }
    }


    private async Task<long> BatchCheckWalletTransaction()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10004);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var walletIds = new long[]
        {
            10260
        };
        var wallets = await ctx.Wallets
            .Where(x => walletIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .ToListAsync();
        foreach (var wallet in wallets)
        {
            DateTime? fromDateTime = null;

            var lists = await ctx.WalletTransactions
                .Where(x => x.WalletId == wallet!.Id)
                .Where(x => fromDateTime == null || x.CreatedOn >= fromDateTime)
                .OrderByDescending(x => x.CreatedOn).ThenByDescending(x => x.Id)
                .Select(x => new { x.Id, x.PrevBalance, x.Amount, x.CreatedOn })
                .ToListAsync();
            // _logger.LogInformation("Test: {count}", lists.Count);

            var latest = lists.FirstOrDefault();
            if (latest != null)
            {
                var flag = wallet.Balance == latest.PrevBalance + latest.Amount;
                if (!flag)
                {
                    _logger.LogInformation(
                        "CheckWalletTransaction => Wallet Balance not match with latest wt: {balance} = {prev} + {amount}, LastWtId = {id}",
                        wallet.Balance, latest.PrevBalance, latest.Amount, latest.Id);
                    // return latest.Id;
                    continue;
                }
            }

            var valid = true;
            for (var i = 0; i < lists.Count - 1; i++)
            {
                var cur = lists[i];
                var prev = lists[i + 1];
                var flag = prev.PrevBalance + prev.Amount == cur.PrevBalance;
                if (!flag)
                {
                    _logger.LogInformation(
                        "CheckWalletTransaction => Wallet {walletId} Balance not match with prev wt: {balance} = {prev} + {amount}, PrevWtId = {id}, createdOn = {createdOn}",
                        wallet.Id, cur.PrevBalance, prev.PrevBalance, prev.Amount, prev.Id, prev.CreatedOn);
                    // return prev.Id;
                    valid = false;
                    break;
                }
            }

            if (valid)
                _logger.LogInformation("CheckWalletTransaction => Wallet {walletId} OK", wallet.Id);
        }

        return 0;
    }

    private async Task<long> CheckWalletTransaction(TenantDbContext? ctx = null, Wallet? wallet = null)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10004);
        if (ctx == null)
        {
            ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            wallet = await ctx.Wallets.SingleAsync(x => x.Id == 10260);
        }

        // var fromDateTime = Utils.ParseToUTC("2025-02-11");
        DateTime? fromDateTime = null;

        var lists = await ctx.WalletTransactions
            .Where(x => x.WalletId == wallet!.Id)
            .Where(x => fromDateTime == null || x.CreatedOn >= fromDateTime)
            .OrderByDescending(x => x.CreatedOn).ThenByDescending(x => x.Id)
            .Select(x => new { x.Id, x.PrevBalance, x.Amount, x.CreatedOn })
            .ToListAsync();
        // _logger.LogInformation("Test: {count}", lists.Count);

        var latest = lists.FirstOrDefault();
        if (latest != null)
        {
            var flag = wallet.Balance == latest.PrevBalance + latest.Amount;
            if (!flag)
            {
                _logger.LogInformation(
                    "CheckWalletTransaction => Wallet Balance not match with latest wt: {balance} = {prev} + {amount}, LastWtId = {id}",
                    wallet.Balance, latest.PrevBalance, latest.Amount, latest.Id);
                return latest.Id;
            }
        }

        for (var i = 0; i < lists.Count - 1; i++)
        {
            var cur = lists[i];
            var prev = lists[i + 1];
            var flag = prev.PrevBalance + prev.Amount == cur.PrevBalance;
            if (!flag)
            {
                _logger.LogInformation(
                    "CheckWalletTransaction => Wallet {walletId} Balance not match with prev wt: {balance} = {prev} + {amount}, PrevWtId = {id}, createdOn = {createdOn}",
                    wallet.Id, cur.PrevBalance, prev.PrevBalance, prev.Amount, prev.Id, prev.CreatedOn);
                return prev.Id;
            }
        }

        return 0;
    }


    private async Task SetWithdrawalApprovalTime()
    {
        using var scope = _serviceProvider.CreateTenantScope(10004);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        // current month parse to date
        // var currentMonth = DateTime.Now;
        var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.AddDays(-1).Day);
        var nextMonth = currentMonth.AddMonths(1);

        // make it utc time
        currentMonth = DateTime.SpecifyKind(currentMonth, DateTimeKind.Utc);
        nextMonth = DateTime.SpecifyKind(nextMonth, DateTimeKind.Utc);

        var withdrawals = await tenantCtx.Withdrawals
            // .Where(x => x.IdNavigation.PostedOn >= currentMonth && x.IdNavigation.PostedOn < nextMonth)
            .Where(x => x.IdNavigation.StateId >= 420 && x.IdNavigation.StateId != 425)
            .Include(x => x.IdNavigation.Activities.Where(y => y.ToStateId == (int)StateTypes.WithdrawalTenantApproved))
            .ToListAsync();
        using var bar = CreateBar(withdrawals.Count, "Withdrawals");

        foreach (var item in withdrawals)
        {
            bar.Tick();
            var approveActivity =
                item.IdNavigation.Activities.FirstOrDefault(
                    x => x.ToStateId == (int)StateTypes.WithdrawalTenantApproved);
            if (approveActivity == null) continue;
            item.ApprovedOn = approveActivity.PerformedOn;
            tenantCtx.Withdrawals.Update(item);
            await tenantCtx.SaveChangesAsync();
        }
    }
}