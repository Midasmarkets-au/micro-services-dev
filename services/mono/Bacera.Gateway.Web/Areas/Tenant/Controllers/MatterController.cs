
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Matter")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class MatterController(TenantDbContext tenantCtx) : TenantBaseController
{
    [HttpGet("{id:long}/state-detail")]
    public async Task<IActionResult> GetStateDetails(long id)
    {
        var item = await tenantCtx.Matters
            .Where(x => x.Id == id)
            .ToStateDetailModel()
            .SingleOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }
}