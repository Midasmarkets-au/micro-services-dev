
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Auth Code")]
[Area("Tenant")]
[Route("api/" + VersionTypes.V1 + "/[Area]/auto-code")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class AuthCodeController(TenantDbContext tenantCtx, IBackgroundJobClient client) : TenantBaseController
{
    /// <summary>
    /// Page of AuthCode
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AuthCode.Criteria? criteria = null)
    {
        criteria ??= new AuthCode.Criteria();
        var hideEmail = ShouldHideEmail();
        var items = await tenantCtx.AuthCodes
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Resend
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/resend")]
    public async Task<IActionResult> Resend(long id)
    {
        if (!await tenantCtx.AuthCodes.AnyAsync(x => x.Id == id))
            return NotFound(Result.Error("Auth code not found."));
        client.Enqueue<IGeneralJob>(x => x.ResetAuthCodeAndSendAsync(GetTenantId(), id));
        return Ok();
    }
}