using System.Collections.Immutable;
using Bacera.Gateway.Services;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Web.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

[Tags("IB/Report")]
public class ReportController(
    TradingService tradingService,
    ReportService reportService,
    TenantDbContext tenantCtx)
    : AgentBaseController
{
    /// <summary>
    /// Recent accounts
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpGet("account/latest")]
    [ProducesResponseType(typeof(List<ReportService.UserAccountTransaction>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LastAccount(long agentUid, [FromQuery] int? count)
    {
        var limit = Math.Min(count ?? 5, 10);
        var items = await reportService.AccountRecentReportAsync(agentUid, limit);
        return Ok(items);
    }

    /// <summary>
    /// Recent deposit
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpGet("deposit/latest")]
    [ProducesResponseType(typeof(List<ReportService.UserAccountTransaction>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LastDeposit(long agentUid, [FromQuery] int? count)
    {
        var limit = Math.Min(count ?? 5, 10);
        var items = await reportService.DepositRecentReportAsync(agentUid, limit);
        return Ok(items);
    }

    /// <summary>
    /// Today deposit value
    /// </summary>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("deposit/today-value")]
    [ProducesResponseType(typeof(List<MonetaryResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DepositTodayValue(long agentUid)
    {
        var items = await reportService.DepositTodayValueAsync(agentUid);
        return Ok(items.Select(MonetaryResponseModel.Of).ToList());
    }

    /// <summary>
    /// Total rebate value(
    /// TODO: This API should be called carefully, it may not able to return result if the rows are too many. Should added timeout and state change check @FrontEnd
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("rebate/total-value")]
    [ProducesResponseType(typeof(List<MonetaryResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateTotalValue(long agentUid, [FromQuery] Rebate.Criteria? criteria)
    {
        criteria ??= new Rebate.Criteria();
        criteria.AccountUid = agentUid;
        criteria.StateId = StateTypes.RebateCompleted;
        var items = await reportService.RebateSumUpByCurrencyValueAsync(criteria);
        return Ok(items.Select(MonetaryResponseModel.Of).ToList());
    }

    /// <summary>
    /// Today rebate value
    /// </summary>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("rebate/today-value")]
    [ProducesResponseType(typeof(List<MonetaryResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateTodayValue(long agentUid)
    {
        var items = await reportService.RebateTodayValueOfAccountAsync(agentUid);
        return Ok(items.Select(MonetaryResponseModel.Of).ToList());
    }

    /// <summary>
    /// Today rebate
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <returns></returns>
    [HttpGet("rebate/today")]
    [ProducesResponseType(typeof(List<Rebate.ReportViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateToday(long agentUid, [FromQuery] double timezoneOffset = 0)
    {
        var start = Utils.GetTodayCloseTradeTime().AddDays(-1);
        var criteria = Rebate.ReportCriteria.Build(start, DateTime.UtcNow, ReportPeriodTypes.Daily, timezoneOffset);
        criteria.AccountUid = agentUid;
        var items = await reportService.RebateReportQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Hourly rebate
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <returns></returns>
    [HttpGet("rebate/hourly")]
    [ProducesResponseType(typeof(List<Rebate.ReportViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateHourly(long agentUid, [FromQuery] double timezoneOffset = 0)
    {
        var clientStartTime = DateTime.UtcNow.Date;
        var clientEndTime = DateTime.UtcNow;
        var criteria = Rebate.ReportCriteria.Build(clientStartTime, clientEndTime, ReportPeriodTypes.Hourly);
        criteria.AccountUid = agentUid;
        var items = await reportService.RebateReportQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Daily rebate
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <returns></returns>
    [HttpGet("rebate/daily")]
    [ProducesResponseType(typeof(List<Rebate.ReportCriteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateDaily(long agentUid, [FromQuery] double timezoneOffset = 0)
    {
        var criteria = Rebate.ReportCriteria.Build(
            DateTime.UtcNow.AddMonths(-1).AddHours(-timezoneOffset), DateTime.UtcNow, ReportPeriodTypes.Daily,
            timezoneOffset);
        criteria.AccountUid = agentUid;
        var items = await reportService.RebateReportQueryAsync(criteria, timezoneOffset);
        return Ok(items);
    }

    /// <summary>
    /// Monthly rebate
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <returns></returns>
    [HttpGet("rebate/monthly")]
    [ProducesResponseType(typeof(List<Rebate.ReportViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateMonthly(long agentUid, [FromQuery] double timezoneOffset = 0)
    {
        var criteria = Rebate.ReportCriteria.Build(
            DateTime.UtcNow.AddYears(-1), DateTime.UtcNow,
            ReportPeriodTypes.Monthly, timezoneOffset);
        criteria.AccountUid = agentUid;
        var items = await reportService.RebateReportQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Yearly rebate
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <returns></returns>
    [HttpGet("rebate/yearly")]
    [ProducesResponseType(typeof(List<Rebate.ReportViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateYearly(long agentUid, [FromQuery] double timezoneOffset = 0)
    {
        var criteria = Rebate.ReportCriteria.Build(
            DateTime.UtcNow.AddYears(-10), DateTime.UtcNow,
            ReportPeriodTypes.Yearly,
            timezoneOffset);
        criteria.AccountUid = agentUid;
        var items = await reportService.RebateReportQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Today symbol trade volume
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpGet("trade/today-symbol-volume")]
    [ProducesResponseType(typeof(List<SymbolVolumeResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SymbolTodayTop(long agentUid, [FromQuery] double timezoneOffset = 0,
        [FromQuery] int count = 5)
    {
        var start = Utils.GetTodayCloseTradeTime().AddDays(-1);
        var items = await tenantCtx.TradeRebates
            .Where(x => x.Rebates.Any(r => r.Account.Uid == agentUid))
            .Where(x => x.CreatedOn >= start)
            .GroupBy(x => x.Symbol)
            .Select(x => new { Symbol = x.Key, Volume = x.Sum(g => g.Volume) })
            .OrderByDescending(x => x.Volume)
            .Select(x => SymbolVolumeResponseModel.Of(x.Symbol, x.Volume))
            .Take(count)
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Today trade volume
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="timezoneOffset"></param>
    /// <returns></returns>
    [HttpGet("trade/today-volume")]
    [ProducesResponseType(typeof(VolumeResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> TradeTodayValue(long agentUid, [FromQuery] double timezoneOffset = 0)
    {
        var volume = await tradingService.TradeVolumeTodayValueFromTradeRebate(agentUid, timezoneOffset);
        return Ok(VolumeResponseModel.Of(volume));
    }

    /// <summary>
    /// Today account creation count
    /// </summary>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("account/today-creation")]
    [ProducesResponseType(typeof(CountResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> AccountTodayCreationCount(long agentUid)
    {
        var count = await reportService.AccountTodayCreation(agentUid);
        return Ok(CountResponseModel.Of(count));
    }
    
    /// <summary>
    /// Report Request pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("request")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<List<AccountReport.ClientPageModel>, AccountReport.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPagination(long agentUid, [FromQuery] AccountReport.Criteria? criteria)
    {
        criteria ??= new AccountReport.Criteria();

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == agentUid)
            .Select(x => new { x.Id, x.CreatedOn })
            .FirstOrDefaultAsync();

        if (account == null) return NotFound();
        
        criteria.AccountId = account.Id;
        criteria.Type = AccountReportTypes.IbMonthlyReport;

        var items = await tenantCtx.AccountReports
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();

        var (year, endMonth, _) = DateTime.UtcNow;
        var startMonth = account.CreatedOn.Year == year ? account.CreatedOn.Month : 1;

        var reports = items.ToDictionary(x => x.Date, x => x);

        var results = Enumerable.Range(startMonth, endMonth - startMonth + 1)
            .Select(month => $"{year}-{month:D2}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToImmutableSortedDictionary(x => x, x => reports.GetValueOrDefault(x) ?? new AccountReport.ClientPageModel());

        return Ok(Result.Of(results, criteria));
    }
}