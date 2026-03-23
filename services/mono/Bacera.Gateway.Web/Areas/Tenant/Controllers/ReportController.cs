using OpenIddict.Validation.AspNetCore;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = ReportRequest;

[Tags("Tenant/Report")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ReportController(
    TenantDbContext tenantCtx,
    ITenantGetter tenantGetter,
    IReportServiceClient reportServiceClient,
    ReportService reportService,
    IReportJob reportJob,
    MyDbContextPool myDbContextPool)
    : TenantBaseController
{
    /// <summary>
    /// Request pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("request")]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPagination([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        
        // Override sorting to handle NULL GeneratedOn values (put them last)
        var query = tenantCtx.ReportRequests.FilterBy(criteria);
        query = query
            .OrderByDescending(x => x.GeneratedOn.HasValue) // Non-null first
            .ThenByDescending(x => x.GeneratedOn)           // Then by GeneratedOn DESC
            .ThenByDescending(x => x.Id);                   // Then by Id DESC
        
        // Apply pagination
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

    /// <summary>
    /// Get a query sample for a report
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("query/{type:int}/sample")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuerySample(int type)
    {
        await Task.Delay(0);
        switch ((ReportRequestTypes)type)
        {
            case ReportRequestTypes.Rebate:
                return Ok(new Rebate.Criteria());
            default:
                return NotFound();
        }
    }

    /// <summary>
    /// Query preview
    /// </summary>
    /// <param name="type"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpPost("query/{type:int}/preview")]
    [ProducesResponseType(typeof(Result<List<object>, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QueryPreview(int type, [FromBody] dynamic query)
    {
        await Task.Delay(0);
        switch ((ReportRequestTypes)type)
        {
            case ReportRequestTypes.Rebate:
                Rebate.Criteria criteria =
                    JsonConvert.DeserializeObject<Rebate.Criteria>(JsonConvert.SerializeObject(query));
                criteria.Page = 1;
                criteria.Size = 20;
                var items = await tenantCtx.Rebates
                    .PagedFilterBy(criteria)
                    .ToListAsync();
                return Ok(Result<List<Rebate>, Rebate.Criteria>.Of(items, criteria));
            default:
                return NotFound();
        }
    }

    /// <summary>
    /// Create a request
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("request")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRequest([FromBody] ReportRequest.CreateSpec spec)
    {
        var item = M.Build(GetPartyId(), spec.Type, spec.Name, Utils.JsonSerializeObject(spec.Query, true));

        item.IsFromApi = 1; // Mark as from API, differentiate from scheduled Job

        if (!ReportService.ValidateQueryString(item))
        {
            return BadRequest(Result.Error("__INVALID_QUERY__"));
        }

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);

        return Ok(item);
    }

    /// <summary>
    /// Create a request
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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
        
        // 如果是 Rebate 或 WalletTransactionForTenant 类型，需要同时重新生成配对报告（双向支持）
        if (item.Type == (int)ReportRequestTypes.Rebate || 
            item.Type == (int)ReportRequestTypes.WalletTransactionForTenant ||
            item.Type == (int)ReportRequestTypes.DailyEquity)
        {
            // 优先从 Query 中读取 UseClosingTime，如果不存在则从名称判断
            bool? useClosingTime = null;
            try
            {
                var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Query);
                if (queryDict != null && queryDict.ContainsKey("UseClosingTime"))
                {
                    useClosingTime = Convert.ToBoolean(queryDict["UseClosingTime"]);
                }
            }
            catch
            {
                // 如果解析失败，继续使用名称判断
            }

            var currentName = item.Name;
            var isClosingTimeBased = useClosingTime == true 
                || (useClosingTime == null && currentName.Contains("MT5 ClosingTime Based"));
            var isReleasedTimeBased = useClosingTime == false 
                || (useClosingTime == null && currentName.Contains("ReleasedTime Based"));

            // 如果既不是 ClosingTime 也不是 ReleasedTime，根据 IsFromApi 判断
            if (!isClosingTimeBased && !isReleasedTimeBased)
            {
                if (item.IsFromApi == 0)
                {
                    isClosingTimeBased = true; // Job入口默认使用ClosingTime
                }
                else
                {
                    isReleasedTimeBased = useClosingTime != true; // API入口：如果 UseClosingTime 未设置或为 false，则使用 ReleasedTime
                }
            }
            
            // 从 query 中提取日期
            DateTime? reportDate = null;
            try
            {
                var queryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Query);
                if (queryDict != null && queryDict.ContainsKey("to"))
                {
                    reportDate = DateTime.Parse(queryDict["to"].ToString()!);
                }
            }
            catch
            {
                // 如果从 query 解析失败，尝试从名称中提取日期
                var dateMatchResult = System.Text.RegularExpressions.Regex.Match(item.Name, @"(\d{4}-\d{2}-\d{2})");
                if (dateMatchResult.Success)
                {
                    reportDate = DateTime.Parse(dateMatchResult.Groups[1].Value);
                }
            }
            
            if (reportDate.HasValue)
            {
                var dateStr = reportDate.Value.ToString("yyyy-MM-dd");
                var fromDate = reportDate.Value.AddDays(-1);
                
                // 确定报告类型和名称模式
                var reportType = item.Type;
                string baseNamePattern;
                string closingTimeNamePattern;
                string releasedTimeNamePattern;
                
                if (reportType == (int)ReportRequestTypes.Rebate)
                {
                    baseNamePattern = "Rebate Daily Record";
                    closingTimeNamePattern = $"Rebate Daily Record (MT5 ClosingTime Based) {dateStr}";
                    releasedTimeNamePattern = $"Rebate Daily Record (ReleasedTime Based) {dateStr}";
                }
                else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                {
                    baseNamePattern = "Wallet Daily Transaction";
                    closingTimeNamePattern = $"Wallet Daily Transaction (MT5 ClosingTime Based) {dateStr}";
                    releasedTimeNamePattern = $"Wallet Daily Transaction (ReleasedTime Based) {dateStr}";
                }
                else if (reportType == (int)ReportRequestTypes.DailyEquity)
                {
                    // Check if this is a 3-day report (Sat-Mon)
                    var is3DayReport = currentName.Contains("(Sat-Mon)");
                    
                    baseNamePattern = "Daily Equity Report";
                    if (is3DayReport)
                    {
                        // For 3-day reports, adjust date parameters to cover Fri->Mon
                        fromDate = reportDate.Value.AddDays(-4); // Friday (4 days before Tuesday)
                        closingTimeNamePattern = $"Daily Equity Report (Sat-Mon) (MT5 ClosingTime Based) {dateStr}";
                        releasedTimeNamePattern = $"Daily Equity Report (Sat-Mon) (ReleasedTime Based) {dateStr}";
                    }
                    else
                    {
                        closingTimeNamePattern = $"Daily Equity Report (MT5 ClosingTime Based) {dateStr}";
                        releasedTimeNamePattern = $"Daily Equity Report (ReleasedTime Based) {dateStr}";
                    }
                }
                else
                {
                    baseNamePattern = "";
                    closingTimeNamePattern = "";
                    releasedTimeNamePattern = "";
                }
                
                // 更新报告名称为新格式（如果还是旧格式且是Job生成的）
                if (!isClosingTimeBased && !isReleasedTimeBased && !string.IsNullOrEmpty(baseNamePattern) && item.IsFromApi == 0)
                {
                    // 仅对Job入口更新名称，API入口保持用户自定义名称
                    item.Name = closingTimeNamePattern;
                    isClosingTimeBased = true;
                }
                
                ReportRequest? pairedReport = null;
                
                if (isClosingTimeBased)
                {
                    // 当前是 MT5 ClosingTime Based，查找 ReleasedTime Based 配对报告
                    // For 3-day reports, also search for (Sat-Mon) pattern
                    var is3DayReport = currentName.Contains("(Sat-Mon)");
                    pairedReport = await tenantCtx.ReportRequests
                        .FirstOrDefaultAsync(x => 
                            x.Type == reportType
                            && x.IsFromApi == 1
                            && x.Name.Contains("ReleasedTime Based")
                            && x.Name.Contains(dateStr)
                            && (is3DayReport ? x.Name.Contains("(Sat-Mon)") : !x.Name.Contains("(Sat-Mon)"))
                            && x.PartyId == item.PartyId
                            && x.Id != item.Id);
                    
                    // 如果没有配对报告，创建一个新的
                    if (pairedReport == null && !string.IsNullOrEmpty(releasedTimeNamePattern))
                    {
                        // 确保日期时间为 UTC 格式
                        var fromDateUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
                        var toDateUtc = DateTime.SpecifyKind(reportDate.Value, DateTimeKind.Utc);
                        
                        if (reportType == (int)ReportRequestTypes.Rebate)
                        {
                            pairedReport = ReportRequest.Build(
                                item.PartyId,
                                ReportRequestTypes.Rebate,
                                releasedTimeNamePattern,
                                JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings));
                        }
                        else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                        {
                            pairedReport = new ReportRequest
                            {
                                PartyId = item.PartyId,
                                Type = (int)ReportRequestTypes.WalletTransactionForTenant,
                                Name = releasedTimeNamePattern,
                                Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                                IsFromApi = 1
                            };
                        }
                        else if (reportType == (int)ReportRequestTypes.DailyEquity)
                        {
                            pairedReport = new ReportRequest
                            {
                                PartyId = item.PartyId,
                                Type = (int)ReportRequestTypes.DailyEquity,
                                Name = releasedTimeNamePattern,
                                Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                                IsFromApi = 1
                            };
                        }

                        if (pairedReport != null)
                        {
                            pairedReport.IsFromApi = 1; // 标记为API入口
                            tenantCtx.ReportRequests.Add(pairedReport);
                            await tenantCtx.SaveChangesAsync();
                        }
                    }
                }
                else if (isReleasedTimeBased)
                {
                    // 当前是 ReleasedTime Based，查找 MT5 ClosingTime Based 配对报告
                    // For 3-day reports, also search for (Sat-Mon) pattern
                    var is3DayReport = currentName.Contains("(Sat-Mon)");
                    pairedReport = await tenantCtx.ReportRequests
                        .FirstOrDefaultAsync(x => 
                            x.Type == reportType
                            && x.IsFromApi == 0
                            && x.Name.Contains("ClosingTime Based")
                            && x.Name.Contains(dateStr)
                            && (is3DayReport ? x.Name.Contains("(Sat-Mon)") : !x.Name.Contains("(Sat-Mon)"))
                            && x.PartyId == item.PartyId
                            && x.Id != item.Id);
                    
                    // 如果没有配对报告，创建一个新的
                    if (pairedReport == null && !string.IsNullOrEmpty(closingTimeNamePattern))
                    {
                        // 确保日期时间为 UTC 格式
                        var fromDateUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
                        var toDateUtc = DateTime.SpecifyKind(reportDate.Value, DateTimeKind.Utc);
                        
                        if (reportType == (int)ReportRequestTypes.Rebate)
                        {
                            pairedReport = ReportRequest.Build(
                                item.PartyId,
                                ReportRequestTypes.Rebate,
                                closingTimeNamePattern,
                                JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings));
                        }
                        else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                        {
                            pairedReport = new ReportRequest
                            {
                                PartyId = item.PartyId,
                                Type = (int)ReportRequestTypes.WalletTransactionForTenant,
                                Name = closingTimeNamePattern,
                                Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                                IsFromApi = 0
                            };
                        }
                        else if (reportType == (int)ReportRequestTypes.DailyEquity)
                        {
                            pairedReport = new ReportRequest
                            {
                                PartyId = item.PartyId,
                                Type = (int)ReportRequestTypes.DailyEquity,
                                Name = closingTimeNamePattern,
                                Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                                IsFromApi = 0
                            };
                        }

                        if (pairedReport != null)
                        {
                            pairedReport.IsFromApi = 0; // 标记为Job入口（默认值）
                            tenantCtx.ReportRequests.Add(pairedReport);
                            await tenantCtx.SaveChangesAsync();
                        }
                    }
                }
                
                // 如果找到或创建了配对报告，同时重新生成配对报告csv
                if (pairedReport != null)
                {
                    pairedReport.FileName = "";
                    pairedReport.GeneratedOn = null;
                    await tenantCtx.SaveChangesAsync();
                    await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), pairedReport.Id);
                }
            }
        }
        
        await tenantCtx.SaveChangesAsync();

        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);
        return Ok(item);
    }

    /// <summary>
    /// Wait until the report is generated rather than queueing a background job @Jim
    /// 
    /// Create a request and generate the report immediately
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("request/download")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDownloadRequest([FromBody] ReportRequest.CreateSpec spec)
    {
        var item = M.Build(GetPartyId(), spec.Type, spec.Name, Utils.JsonSerializeObject(spec.Query, true), spec.IsFromApi);
        if (!ReportService.ValidateQueryString(item))
        {
            return BadRequest(Result.Error("__INVALID_QUERY__"));
        }

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        var (result, medium) = await reportService.ProcessRequestAsync(item);
        if (!result) return BadRequest(result);

        return Ok(medium);
    }

    /// <summary>
    /// [TEST ONLY] Simulate Hangfire Job execution for Daily Equity Report
    /// This endpoint mimics the exact execution path of the Hangfire background job
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("request/download/simulate-job")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimulateHangfireJob([FromBody] ReportRequest.CreateSpec spec)
    {
        var item = M.Build(GetPartyId(), spec.Type, spec.Name, Utils.JsonSerializeObject(spec.Query, true), spec.IsFromApi);

        if (!ReportService.ValidateQueryString(item))
        {
            return BadRequest(Result.Error("__INVALID_QUERY__"));
        }

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        // ✅ This calls the EXACT SAME method that Hangfire uses
        var result = await reportJob.ProcessReportRequest(tenantGetter.GetTenantId(), item.Id);

        // Retrieve the updated request to get the file name
        var updatedItem = await tenantCtx.ReportRequests.SingleOrDefaultAsync(x => x.Id == item.Id);

        return Ok(new
        {
            success = result,
            requestId = item.Id,
            fileName = updatedItem?.FileName,
            generatedOn = updatedItem?.GeneratedOn,
            message = result 
                ? "Report generated successfully via simulated Hangfire job path" 
                : "Report generation failed",
            note = "This endpoint uses the same execution path as Hangfire background job (ReportJob.ProcessReportRequest)"
        });
    }

    /// <summary>
    /// Get Report Request
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("request/{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M>> GetRequest(long id)
    {
        var item = await tenantCtx.ReportRequests
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Request equity report by scheduler
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("request/equity")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> QueueEquityReport([FromBody] EquityReportRequest spec)
    {
        if (!spec.Emails.Any())
        {
            return BadRequest(Result.Error("__EMAILS_REQUIRED__"));
        }

        var date = (spec.Date ?? DateTime.UtcNow).Date;
        var query = new { from = date, to = date.AddHours(23).AddMinutes(59).AddSeconds(59), emails = spec.Emails };
        var name = $"Daily Equity Report (ClosingTime Based) {date:yyyy-MM-dd}";

        var item = M.Build(GetPartyId(), ReportRequestTypes.DailyEquity, name,
            Utils.JsonSerializeObject(query, true));
        item.IsFromApi = 1;

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);

        return Ok(item);
    }

    [HttpPost("request/equity/html")]
    public async Task<ActionResult> CreateEquityReportHtml([FromBody] ReportConfiguration reportCfg)
    {
        var html = await ReportJob.GenerateEquityReportHtmlAsync(reportService, reportCfg);
        return Ok(html);
    }

    [HttpPost("request/equity/logins")]
    public async Task<ActionResult> GetLogins([FromBody] ReportConfiguration reportCfg)
    {
        var dict = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<long>>>();

        var tasks = reportCfg.Items.Select(async item =>
        {
            dict.GetOrAdd(item.Group, new ConcurrentDictionary<string, List<long>>())[item.Name] =
                await reportService.GetLogins(item, reportCfg.ServiceId);
        });

        await Task.WhenAll(tasks);

        var result = dict.OrderBy(x => x.Key)
            .ToDictionary(x => x.Key
                , x => x.Value.OrderBy(y => y.Key)
                    .ToDictionary(y => y.Key, y => y.Value));

        return Ok(result);
    }


    public class EquityReportRequest
    {
        [Required] public List<string> Emails { get; set; } = new();
        public DateTime? Date { get; set; }
    }

    public class EquityReportHtmlRequest
    {
        public string ConfigJson { get; set; } = "{}";
    }
}