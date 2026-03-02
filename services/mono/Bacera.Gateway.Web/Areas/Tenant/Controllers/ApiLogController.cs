using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Web.Areas.Tenant.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant;

[Tags("Tenant/Api Log")]
[Area("Tenant")]
[Route("api/" + VersionTypes.V1 + "/[Area]/api-log")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ApiLogController(TenantDbContext tenantCtx) : TenantBaseController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ApiLog.Criteria? criteria = null)
    {
        criteria ??= new ApiLog.Criteria();
        var hideEmail = ShouldHideEmail();

        var items = await tenantCtx.ApiLogs
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();

        var partyIds = items.Select(x => x.PartyId).Distinct().ToList();
        var parties = await tenantCtx.Parties.Where(x => partyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Email);
        items.ForEach(x => x.Email = parties.GetValueOrDefault(x.PartyId, string.Empty));
        return Ok(Result.Of(items, criteria));
    }
}