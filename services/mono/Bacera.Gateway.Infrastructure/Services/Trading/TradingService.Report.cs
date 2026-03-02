using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

/**
 * Reports Module
 */
partial class TradingService
{
    public async Task<List<int>> GetAllTradeServiceIdsAsync()
    {
        var items = await dbContext.TradeServices
            .Where(x => x.Platform == (int)PlatformTypes.MetaTrader4
                        || x.Platform == (int)PlatformTypes.MetaTrader5)
            .Select(x => x.Id)
            .ToListAsync();
        return items;
    }

    public async Task<List<(string, int)>> TradeVolumeTopSymbolTodayFromTradeRebate(long agentUid, double timezoneOffset = 0, int count = 5)
    {
        var start = Utils.GetTodayCloseTradeTime().AddDays(-1);
        var items = await dbContext.TradeRebates
            .Where(x => x.Rebates.Any(r => r.Account.Uid == agentUid))
            .Where(x => x.CreatedOn >= start)
            .GroupBy(x => x.Symbol)
            .Select(x => new { Symbol = x.Key, Volume = x.Sum(g => g.Volume) })
            .OrderByDescending(x => x.Volume)
            .Take(count)
            .ToListAsync();
        return items.Select(x => (x.Symbol, x.Volume)).ToList();
    }

    public async Task<List<KeyValuePair<string, double>>> TradeVolumeTopSymbolToday(int serviceId,
        AccountGroupTypes groupType,
        long agentUid, int count = 5)
    {
        var key = GetReportCacheKey(serviceId, groupType, agentUid, "TradeVolumeTopSymbolToday");
        // var cached = await _cache.GetAsync<List<KeyValuePair<string, double>>>(key);
        // if (cached != null)
        //     return cached;

        var criteria = new TradeViewModel.ReportCriteria
        {
            ServiceId = serviceId,
            From = DateTime.UtcNow.AddDays(-1)
        };
        switch (groupType)
        {
            case AccountGroupTypes.Agent:
                criteria.AgentUid = agentUid;
                break;
            case AccountGroupTypes.Sales:
                criteria.SalesUid = agentUid;
                break;
            case AccountGroupTypes.Rep:
                criteria.RepUid = agentUid;
                break;
        }

        var query = await TradeReportQuery(criteria);
        var items = await query
            .Where(x => x.Cmd >= 0 && x.Cmd <= 5 && x.Symbol != "")
            .GroupBy(x => x.Symbol)
            .Select(x => new { Symbol = x.Key, Volume = x.Sum(g => g.Volume) })
            .OrderByDescending(x => x.Volume)
            .Take(count)
            .ToListAsync();
        var result = items.Select(x => new KeyValuePair<string, double>(x.Symbol, x.Volume)).ToList();
        await myCache.SetAsync(key, result, ReportCacheTimeSpan);
        return result;
    }

    public async Task<double> TradeVolumeTodayValueFromTradeRebate(long agentUid, double timezoneOffset = 0)
    {
        var from = Utils.GetTodayCloseTradeTime().AddDays(-1);
        var result = await dbContext.TradeRebates
            .Where(x => x.CreatedOn >= from)
            .Where(x => x.Rebates.Any(r => r.Account.Uid == agentUid))
            .SumAsync(x => x.Volume);
        return result;
    }

    // public async Task<double> TradeVolumeTodayValue(int serviceId, AccountGroupTypes groupType, long agentUid)
    // {
    //     var key = GetReportCacheKey(serviceId, groupType, agentUid, "TradeVolumeTodayValue");
    //     var cached = await myCache.GetStringAsync(key);
    //     if (cached != null)
    //         return long.Parse(cached);
    //
    //     var criteria = new TradeViewModel.ReportCriteria
    //     {
    //         ServiceId = serviceId,
    //         From = DateTime.UtcNow.AddDays(-1)
    //     };
    //     switch (groupType)
    //     {
    //         case AccountGroupTypes.Agent:
    //             criteria.AgentUid = agentUid;
    //             break;
    //         case AccountGroupTypes.Sales:
    //             criteria.SalesUid = agentUid;
    //             break;
    //         case AccountGroupTypes.Rep:
    //             criteria.RepUid = agentUid;
    //             break;
    //     }
    //
    //     var query = await TradeReportQuery(criteria);
    //     var result = await query
    //         .Where(x => x.Cmd >= 0 && x.Cmd <= 5 && x.Symbol != "")
    //         .Select(x => x.Volume)
    //         .SumAsync();
    //     await myCache.SetStringAsync(key, result.ToString(), ReportCacheTimeSpan);
    //     return result;
    // }

    public async Task<List<TradeViewModel>> GetLastTradeTransactionForSalesAsync(long salesUid, int serviceId,
        int count = 5)
    {
        var criteria = new TradeViewModel.Criteria
        {
            Size = count,
            SalesUid = salesUid,
            ServiceId = serviceId,
        };
        var result = await QueryTrade(criteria);
        return result.Data;
    }

    public async Task<List<Transaction>> GetLastTransferForSalesAsync(long salesUid,
        int count = 5)
    {
        var query = GetLatestTransactionsForSalesQuery(salesUid);
        return await query
            .Include(x => x.IdNavigation)
            .Take(count).ToListAsync();
    }

    private IQueryable<Transaction> GetLatestTransactionsForSalesQuery(long salesUid)
    {
        var query =
            from client in dbContext.Accounts
            join trans in dbContext.Transactions
                on client.PartyId equals trans.PartyId
            join m in dbContext.Matters
                on trans.Id equals m.Id
            where client.SalesAccount != null && client.SalesAccount.Uid == salesUid
            where m.StateId == (int)StateTypes.TransferCompleted
            orderby m.StatedOn descending
            select trans;
        return query;
    }

    private static string GetReportCacheKey(int serviceId, AccountGroupTypes groupTypes, long agentUid, string name)
        => $"trade_report_agent:{agentUid}_group:{groupTypes}_service:{serviceId}_name:{name}";

    private static TimeSpan ReportCacheTimeSpan => TimeSpan.FromHours(1);
}