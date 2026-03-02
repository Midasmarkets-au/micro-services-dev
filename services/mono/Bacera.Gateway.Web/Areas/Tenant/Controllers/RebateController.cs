using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Rebate;

[Tags("Tenant/Rebate")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RebateController(
    AccountingService accountingService,
    TradingService tradingSvc,
    IMyCache myCache,
    TenantDbContext tenantCtx)
    : TenantBaseController
{
    private readonly TradingService _tradingSvc = tradingSvc;

    [HttpPut("disable-release")]
    public async Task<IActionResult> DisableReleaseRebateAsync([FromQuery] long? timeInMinutes = 10)
    {
        var key = CacheKeys.GetReleaseDisabledKey();
        await myCache.SetStringAsync(key, "1", TimeSpan.FromMinutes(timeInMinutes!.Value));
        return Ok();
    }

    [HttpPut("enable-release")]
    public async Task<IActionResult> EnableReleaseRebateAsync()
    {
        var key = CacheKeys.GetReleaseDisabledKey();
        await myCache.KeyDeleteAsync(key);
        return Ok();
    }

    /// <summary>
    /// Rebate pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)

    {
        criteria ??= new M.Criteria();
        var hideEmail = ShouldHideEmail();
        return Ok(await accountingService.RebateQueryAsync(criteria, hideEmail));
    }

    /// <summary>
    /// Get Rebate
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await accountingService.RebateGetAsync(id);
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get All rebate symbols
    /// </summary>
    /// <returns></returns>
    [HttpGet("symbol/all")]
    [ProducesResponseType(typeof(List<RebateSymbol>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllRebateSymbol() =>
        Ok(await tenantCtx.Symbols.ToRebateSymbols(400).ToListAsync());

    /// <summary>
    /// Get rebate symbol categories
    /// </summary>
    /// <returns></returns>
    [HttpGet("symbol/category")]
    [ProducesResponseType(typeof(List<RebateSymbol>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateSymbolCategory() =>
        Ok(await tenantCtx.Symbols.ToRebateCategories(300).ToListAsync());

    /// <summary>
    /// resend rebate
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("resend")]
    public async Task<IActionResult> ResendRebate([FromBody] M.PkModel spec)
    {
        var matter = await tenantCtx.Matters.FindAsync(spec.Id);
        if (matter == null) return NotFound();
        if (matter.StateId != (int)StateTypes.RebateCanceled) return BadRequest("State is not RebateCanceled");
        matter.StateId = (int)StateTypes.RebateOnHold;
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }
}