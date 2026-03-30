using Bacera.Gateway.Web.BackgroundJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Bacera.Gateway.ReportRequest;

[Tags("Tenant/Report")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ReportController(TenantDbContext tenantCtx, ITenantGetter tenantGetter, IReportJob reportJob) : TenantBaseController
{
    [HttpGet("request")]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPagination([FromQuery] M.Criteria? criteria = null)
    {
        criteria ??= new M.Criteria();

        var query = tenantCtx.ReportRequests.FilterBy(criteria);
        query = query
            .OrderByDescending(x => x.GeneratedOn.HasValue)
            .ThenByDescending(x => x.GeneratedOn)
            .ThenByDescending(x => x.Id);

        var total = query.Count();
        criteria.Total = total;
        criteria.Page = criteria.Page < 1 ? 1 : criteria.Page;
        criteria.Size = criteria.Size < 1 ? 20 : criteria.Size;
        criteria.PageCount = (int)Math.Ceiling(total / (decimal)criteria.Size);
        criteria.HasMore = criteria.PageCount > criteria.Page;

        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync();

        return Ok(Result<List<M>, M.Criteria>.Of(items, criteria));
    }

    [HttpPost("request/{id:long}/regen")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenRequest(long id)
    {
        var item = await tenantCtx.ReportRequests.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();

        item.PartyId = GetPartyId();
        item.FileName = "";
        item.GeneratedOn = null;

        if (item.Type == (int)ReportRequestTypes.Rebate ||
            item.Type == (int)ReportRequestTypes.WalletTransactionForTenant ||
            item.Type == (int)ReportRequestTypes.DailyEquity ||
            item.Type == (int)ReportRequestTypes.DailyEquityMonthly)
        {
            bool? useClosingTime = null;
            try
            {
                var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Query);
                if (queryDict != null && queryDict.ContainsKey("UseClosingTime"))
                    useClosingTime = Convert.ToBoolean(queryDict["UseClosingTime"]);
            }
            catch { /* use name-based detection */ }

            var currentName = item.Name;
            var isClosingTimeBased = useClosingTime == true
                || (useClosingTime == null && currentName.Contains("MT5 ClosingTime Based"));
            var isReleasedTimeBased = useClosingTime == false
                || (useClosingTime == null && currentName.Contains("ReleasedTime Based"));

            if (!isClosingTimeBased && !isReleasedTimeBased)
            {
                if (item.IsFromApi == 0) isClosingTimeBased = true;
                else isReleasedTimeBased = useClosingTime != true;
            }

            DateTime? reportDate = null;
            try
            {
                var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Query);
                if (queryDict != null && queryDict.ContainsKey("to"))
                    reportDate = DateTime.Parse(queryDict["to"].ToString()!);
            }
            catch
            {
                var m = System.Text.RegularExpressions.Regex.Match(item.Name, @"(\d{4}-\d{2}-\d{2})");
                if (m.Success) reportDate = DateTime.Parse(m.Groups[1].Value);
            }

            if (reportDate.HasValue)
            {
                var dateStr = reportDate.Value.ToString("yyyy-MM-dd");
                var fromDate = reportDate.Value.AddDays(-1);
                var reportType = item.Type;
                var isPerOfficeReport = currentName.Contains("Per Office");
                var is3DayReport = currentName.Contains("(Sat-Mon)");

                string closingTimeNamePattern, releasedTimeNamePattern;

                if (reportType == (int)ReportRequestTypes.Rebate)
                {
                    closingTimeNamePattern  = $"Rebate Daily Record (MT5 ClosingTime Based) {dateStr}";
                    releasedTimeNamePattern = $"Rebate Daily Record (ReleasedTime Based) {dateStr}";
                }
                else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                {
                    closingTimeNamePattern  = $"Wallet Daily Transaction (MT5 ClosingTime Based) {dateStr}";
                    releasedTimeNamePattern = $"Wallet Daily Transaction (ReleasedTime Based) {dateStr}";
                }
                else if (reportType == (int)ReportRequestTypes.DailyEquity)
                {
                    var prefix = isPerOfficeReport ? "Daily Equity Per Office" : "Daily Equity Report";
                    if (is3DayReport) fromDate = reportDate.Value.AddDays(-4);
                    closingTimeNamePattern  = $"{prefix}{(is3DayReport ? " (Sat-Mon)" : "")} (MT5 ClosingTime Based) {dateStr}";
                    releasedTimeNamePattern = $"{prefix}{(is3DayReport ? " (Sat-Mon)" : "")} (ReleasedTime Based) {dateStr}";
                }
                else // DailyEquityMonthly
                {
                    var prefix = isPerOfficeReport ? "Daily Equity Per Office Monthly" : "Daily Equity Monthly Report";
                    var monthStr = reportDate.Value.ToString("yyyy-MM");
                    dateStr = monthStr;
                    try
                    {
                        var qd = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Query);
                        if (qd != null && qd.ContainsKey("from")) fromDate = DateTime.Parse(qd["from"].ToString()!);
                    }
                    catch { /* keep default */ }
                    closingTimeNamePattern  = $"{prefix} (MT5 ClosingTime Based) {monthStr}";
                    releasedTimeNamePattern = $"{prefix} (ReleasedTime Based) {monthStr}";
                }

                if (!isClosingTimeBased && !isReleasedTimeBased && item.IsFromApi == 0)
                {
                    item.Name = closingTimeNamePattern;
                    isClosingTimeBased = true;
                }

                var fromUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
                var toUtc   = DateTime.SpecifyKind(reportDate.Value, DateTimeKind.Utc);
                var queryJson = isPerOfficeReport
                    ? JsonConvert.SerializeObject(new { from = fromUtc, to = toUtc, aggregateByOffice = true }, Utils.AppJsonSerializerSettings)
                    : JsonConvert.SerializeObject(new { from = fromUtc, to = toUtc }, Utils.AppJsonSerializerSettings);

                M? pairedReport = null;

                if (isClosingTimeBased)
                {
                    pairedReport = await tenantCtx.ReportRequests.FirstOrDefaultAsync(x =>
                        x.Type == reportType && x.IsFromApi == 1
                        && x.Name.Contains("ReleasedTime Based") && x.Name.Contains(dateStr)
                        && (is3DayReport ? x.Name.Contains("(Sat-Mon)") : !x.Name.Contains("(Sat-Mon)"))
                        && (isPerOfficeReport ? x.Name.Contains("Per Office") : !x.Name.Contains("Per Office"))
                        && x.PartyId == item.PartyId && x.Id != item.Id);

                    if (pairedReport == null)
                    {
                        pairedReport = new M { PartyId = item.PartyId, Type = reportType, Name = releasedTimeNamePattern, Query = queryJson, IsFromApi = 1 };
                        tenantCtx.ReportRequests.Add(pairedReport);
                        await tenantCtx.SaveChangesAsync();
                    }
                }
                else if (isReleasedTimeBased)
                {
                    pairedReport = await tenantCtx.ReportRequests.FirstOrDefaultAsync(x =>
                        x.Type == reportType && x.IsFromApi == 0
                        && x.Name.Contains("ClosingTime Based") && x.Name.Contains(dateStr)
                        && (is3DayReport ? x.Name.Contains("(Sat-Mon)") : !x.Name.Contains("(Sat-Mon)"))
                        && (isPerOfficeReport ? x.Name.Contains("Per Office") : !x.Name.Contains("Per Office"))
                        && x.PartyId == item.PartyId && x.Id != item.Id);

                    if (pairedReport == null)
                    {
                        pairedReport = new M { PartyId = item.PartyId, Type = reportType, Name = closingTimeNamePattern, Query = queryJson, IsFromApi = 0 };
                        tenantCtx.ReportRequests.Add(pairedReport);
                        await tenantCtx.SaveChangesAsync();
                    }
                }

                if (pairedReport != null)
                {
                    pairedReport.FileName = "";
                    pairedReport.GeneratedOn = null;
                    await tenantCtx.SaveChangesAsync();
                    var pairedTenantId = tenantGetter.GetTenantId();
                    var pairedRequestId = pairedReport.Id;
                    _ = Task.Run(async () =>
                    {
                        try { await reportJob.ProcessReportRequest(pairedTenantId, pairedRequestId); }
                        catch (Exception ex) { Console.Error.WriteLine($"Paired report processing failed for request {pairedRequestId}: {ex.Message}"); }
                    });
                }
            }
        }

        await tenantCtx.SaveChangesAsync();

        var tenantId = tenantGetter.GetTenantId();
        var requestId = item.Id;
        _ = Task.Run(async () =>
        {
            try { await reportJob.ProcessReportRequest(tenantId, requestId); }
            catch (Exception ex) { Console.Error.WriteLine($"Report processing failed for request {requestId}: {ex.Message}"); }
        });

        return Ok(item);
    }
}
