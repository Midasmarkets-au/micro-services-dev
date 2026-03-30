using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Bacera.Gateway.ReportRequest;

[Tags("Tenant/Report")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ReportController(TenantDbContext tenantCtx) : TenantBaseController
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
}
