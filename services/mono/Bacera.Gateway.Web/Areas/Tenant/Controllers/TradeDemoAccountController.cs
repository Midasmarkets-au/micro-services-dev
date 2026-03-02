using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = TradeDemoAccount;

[Area("Tenant")]
[Tags("Tenant/Trade Demo Account")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V1 + "/[Area]/trade-demo-account")]
public class TradeDemoAccountController : TenantBaseController
{
    private readonly TradingService _tradingSvc;

    public TradeDemoAccountController(TradingService tradingService)
    {
        _tradingSvc = tradingService;
    }

    /// <summary>
    /// Demo Trade Account pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DemoAccount([FromQuery] M.Criteria? criteria)
        => Ok(await _tradingSvc.TradeDemoAccountQueryAsync(criteria ?? new M.Criteria()));

    /// <summary>
    /// Delete Demo Account 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteDemoAccountAsync(long id)
    {
        var result = await _tradingSvc.DeleteDemoAccountAsync(id);
        return Ok(result);
    }
}