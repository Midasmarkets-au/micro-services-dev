using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/Message Record")]
[Route("api/" + VersionTypes.V1 + "/[Area]/message-record")]
public class MessageRecordController(TenantDbContext tenantCtx)
    : TenantBaseController
{
    /// <summary>
    /// Message Record pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] MessageRecord.Criteria? criteria)
    {
        criteria ??= new MessageRecord.Criteria();
        var items = await tenantCtx.MessageRecords
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<MessageRecord.TenantPageModel>, MessageRecord.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Message Record Details
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var item = await tenantCtx.MessageRecords
            .Where(x => x.Id == id)
            .ToTenantDetailModel()
            .SingleOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }
}