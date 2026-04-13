
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

[Tags("Sales/Statistics")]
[ApiController]
[Area("Sales")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class StatisticsController : BaseController
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly ReportService _reportSvc;

    public StatisticsController(
        TenantDbContext tenantDbContext,
        ReportService reportService)
    {
        _tenantDbContext = tenantDbContext;
        _reportSvc = reportService;
    }

    /// <summary>
    /// Get sales statistics including hierarchy data, time series, summary stats, and product distribution
    /// 销售统计功能 - 包含层级数据、时间序列、汇总统计和产品分布
    /// Gets statistics for the authenticated sales user's own account (no salesUid parameter needed)
    /// </summary>
    /// <param name="from">Start date (optional, default: 30 days ago) - 开始日期（可选，默认30天前）</param>
    /// <param name="to">End date (optional, default: today) - 结束日期（可选，默认今天）</param>
    /// <returns>Complete sales statistics data</returns>
    /// <response code="200">Returns sales statistics data</response>
    /// <response code="404">Sales account not found (SALES_ACCOUNT_NOT_FOUND)</response>
    /// <response code="400">User has no sales account</response>
    [HttpGet]
    [ProducesResponseType(typeof(SalesStatistics.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            // Get sales account UID from authenticated user context
            var partyId = GetPartyId();
            if (partyId <= 0)
            {
                return BadRequest(new { error = "Invalid user context" });
            }

            // Try to get sales UID from claims first (if user has multiple sales accounts, use first one)
            var salesUidsFromClaims = User.GetAccountUidsInClaim(UserClaimTypes.SalesAccount);
            long salesUid;

            if (salesUidsFromClaims.Any())
            {
                salesUid = salesUidsFromClaims.First();
            }
            else
            {
                // Fallback: query database for sales account belonging to user's party
                var salesAccount = await _tenantDbContext.Accounts
                    .Where(x => x.PartyId == partyId && x.Role == (short)AccountRoleTypes.Sales)
                    .OrderBy(x => x.Id) // Use first sales account if multiple exist
                    .Select(x => x.Uid)
                    .FirstOrDefaultAsync();

                if (salesAccount == 0)
                {
                    return BadRequest(new { error = "User has no sales account" });
                }

                salesUid = salesAccount;
            }

            var criteria = new SalesStatistics.Criteria
            {
                SalesUid = salesUid,
                From = from,
                To = to
            };

            var result = await _reportSvc.GetSalesStatisticsAsync(criteria);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }
}

