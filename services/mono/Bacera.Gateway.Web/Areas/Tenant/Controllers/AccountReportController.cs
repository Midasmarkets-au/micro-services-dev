using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Background;
using Bacera.Gateway.Web.BackgroundJobs;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Tags("Tenant/Account Report")]
[Area("Tenant")]
[Route("api/" + VersionTypes.V1 + "/[Area]/account-report")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class AccountReportController : TenantBaseController
{
    private readonly TenantDbContext _tenantDbCtx;
    private readonly ReportService _reportService;
    private readonly IMyCache _myCache;

    public AccountReportController(TenantDbContext tenantDbCtx, ReportService reportService, IMyCache myCache)
    {
        _tenantDbCtx = tenantDbCtx;
        _reportService = reportService;
        _myCache = myCache;
    }

    /// <summary>
    /// Account Report List
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<AccountReport>, AccountReport.Criteria>), StatusCodes.Status200OK)]
    public async Task<Result<List<AccountReport>, AccountReport.Criteria>> Index(
        [FromQuery] AccountReport.Criteria? criteria)
    {
        criteria ??= new AccountReport.Criteria();
        var items = await _tenantDbCtx.AccountReports
            .PagedFilterBy(criteria)
            .ToListAsync();

        return Result<List<AccountReport>, AccountReport.Criteria>.Of(items, criteria);
    }

    /// <summary>
    ///  Send Email
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/send-report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendEmail(long id, [FromBody] AccountReport.SendEmailSpec spec)
    {
        var (result, msg) = await _reportService.SendAccountReportEmailById(id, receiverEmail: spec.Email);
        if (!result)
            return BadRequest(msg);
        return Ok();
    }

    /// <summary>
    /// Preview
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/preview")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview(long id)
    {
        var result = await _reportService.TryGetAccountDailyReportModelPreviewHtml(id);
        return Ok(result);
    }

    [HttpGet("today-status")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> TodayStatus()
    {
        var tenantId = GetTenantId();
        var date = DateTime.UtcNow.Date;
        var list = new[]
        {
            await _myCache.GetStringAsync(ReportJob.GenerateAccountTaskKey(tenantId, date)) ?? "Unknown",
            await _myCache.GetStringAsync(ReportJob.ProcessAccountReportTaskKey(tenantId, date)) ?? "Unknown",
            await _myCache.GetStringAsync(ReportJob.SendMailTaskKey(tenantId, date)) ?? "Unknown",
        };

        return Ok(list);
    }
}