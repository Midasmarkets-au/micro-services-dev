using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Sales Rebate")]
[Route("api/" + VersionTypes.V1 + "/[Area]/sales-rebate")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]

public class SalesRebateController(
    TenantDbContext tenantDbContext
    ) : TenantBaseController
{
    /// <summary>
    /// Get Sales Rebate Schema List
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<SalesRebate.TenantPageModel>, SalesRebate.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] SalesRebate.Criteria? criteria)
    {
        criteria ??= new SalesRebate.Criteria();
        var items = await tenantDbContext.SalesRebates
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<SalesRebate.TenantPageModel>, SalesRebate.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Update Sales Rebate Status
    /// </summary>
    /// <returns></returns>
    [HttpPut("{recordId:long}")]
    [ProducesResponseType(typeof(SalesRebateSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SalesRebateSchemaUpdate(long recordId, [FromBody] SalesRebate.UpdateSpec spec)
    {
        if (spec.Status != SalesRebateStatusTypes.Pause && spec.Status != SalesRebateStatusTypes.Pending)
        {
            return BadRequest("Invalid status.");
        }

        var currentStatus = spec.Status == SalesRebateStatusTypes.Pause 
            ? SalesRebateStatusTypes.Pending 
            : SalesRebateStatusTypes.Pause;

        var item = await tenantDbContext.SalesRebates
            .Where(x => x.Id == recordId)
            .Where(x => x.Status == (short)currentStatus)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }

        item.Status = (short)spec.Status;
        tenantDbContext.SalesRebates.Update(item);
        await tenantDbContext.SaveChangesAsync();

        return Ok(item);
    }
}