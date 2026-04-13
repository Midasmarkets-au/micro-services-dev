using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using M = Bacera.Gateway.Verification;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/KYC Form")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class KycController(TenantDbContext tenantDbContext) : TenantBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<VerificationViewModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria = null)
    {
        criteria ??= new M.Criteria();
        criteria.Type = VerificationTypes.KycForm;
        var hideEmail = ShouldHideEmail();
        var items = await tenantDbContext.Verifications
            .PagedFilterBy(criteria)
            .ToTenantViewModel(hideEmail)
            .ToListAsync();
        return Ok(Result<List<VerificationViewModel>, M.Criteria>.Of(items, criteria));
    }
}
