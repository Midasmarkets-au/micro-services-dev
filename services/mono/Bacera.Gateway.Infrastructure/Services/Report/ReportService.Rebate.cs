using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    public async Task<List<Rebate.ReportViewModel>> RebateReportQueryAsync(Rebate.ReportCriteria criteria, double timeZoneOffset = 0)
    {
        var query = tenantDbContext.Rebates.FilterBy(criteria);
        var items = criteria.PeriodType switch
        {
            ReportPeriodTypes.Daily => query.ToDailyResponseModel(timeZoneOffset),
            ReportPeriodTypes.Monthly => query.ToMonthlyResponseModel(timeZoneOffset),
            ReportPeriodTypes.Yearly => query.ToYearlyResponseModel(timeZoneOffset),
            ReportPeriodTypes.Hourly => query.ToHourlyResponseModel(timeZoneOffset),
            _ => throw new ArgumentOutOfRangeException()
        };
        var result = await items.ToListAsync();
        return result;
    }

    public async Task<List<KeyValuePair<int, long>>> RebateTodayValueOfAccountAsync(long agentUid)
    {
        var start = Utils.GetTodayCloseTradeTime().AddDays(-1);

        var result = await tenantDbContext.Rebates
            .Where(x => x.Account.Uid == agentUid)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.RebateCompleted)
            .Where(x => x.IdNavigation.StatedOn >= start)
            .GroupBy(x => x.CurrencyId)
            .Select(x => new KeyValuePair<int, long>(x.Key, x.Sum(g => g.Amount)))
            .ToListAsync();

        return result;
    }

    public async Task<List<KeyValuePair<int, long>>> RebateSumUpByCurrencyValueAsync(Rebate.Criteria criteria)
    {
        // var key = $"report-rebate-sum-by-currency-t:{GetTenantId}-c:{criteria.Hash()}";
        // var cached = await _cacheRepo.GetAsync<List<KeyValuePair<int, long>>>(key);
        // if (cached != null)
        //     return cached;

        var result = await tenantDbContext.Rebates
            .FilterBy(criteria)
            .GroupBy(x => x.CurrencyId)
            .Select(x => new KeyValuePair<int, long>(x.Key, x.Sum(g => g.Amount)))
            .ToListAsync();

        // await _cacheRepo.SetAsync(key, result, TimeSpan.FromHours(1));
        return result;
    }
}