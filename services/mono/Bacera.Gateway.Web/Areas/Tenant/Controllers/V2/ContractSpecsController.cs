using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[Area("Tenant")]
[Tags("Tenant/Documents")]
[Route("api/" + VersionTypes.V2 + "/[Area]/documents/contractspecs")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ContractSpecsController(TenantDbContext tenantCtx, ILogger<ContractSpecsController> logger)
    : TenantBaseControllerV2
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int size = 20,
        [FromQuery] string? site = null, [FromQuery] string? category = null, [FromQuery] string? symbol = null)
    {
        if (string.IsNullOrWhiteSpace(site))
            return BadRequest(new { message = "Site is required" });

        if (page < 1) page = 1;
        if (size < 1) size = 20;
        if (size > 200) size = 200;

        var q = tenantCtx.ContractSpecs.AsNoTracking().Where(x => x.Site == site);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category);
        if (!string.IsNullOrWhiteSpace(symbol)) q = q.Where(x => x.Symbol == symbol);

        q = q.OrderBy(x => x.Id);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync();

        var categories = await tenantCtx.ContractSpecs.AsNoTracking()
            .Where(x => x.Site == site)
            .Select(x => x.Category)
            .Distinct()
            .ToListAsync();

        var result = new
        {
            data = items,
            criteria = new { size, page, total, category, site, symbol },
            categories
        };
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] Core.Models.Tenant.ContractSpec payload)
    {
        var entity = await tenantCtx.ContractSpecs.SingleOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound(new { message = "ContractSpec not found" });

        // simple field mapping
        entity.Site = payload.Site;
        entity.Category = payload.Category;
        entity.Description = payload.Description;
        entity.Symbol = payload.Symbol;
        entity.ContractSize = payload.ContractSize;
        entity.ContractUnit = payload.ContractUnit;
        entity.TradingStartTime = payload.TradingStartTime;
        entity.TradingEndTime = payload.TradingEndTime;
        entity.TradingStartWeekday = payload.TradingStartWeekday;
        entity.TradingEndWeekday = payload.TradingEndWeekday;
        entity.BreakStartTime = payload.BreakStartTime;
        entity.BreakEndTime = payload.BreakEndTime;
        entity.MoreBreakStartTime = payload.MoreBreakStartTime;
        entity.MoreBreakEndTime = payload.MoreBreakEndTime;
        entity.MarginRequirements = payload.MarginRequirements;
        entity.Commission = payload.Commission;
        entity.RolloverTime = payload.RolloverTime;
        entity.Comment = payload.Comment;
        entity.OperatorInfo = payload.OperatorInfo;
        entity.DescriptionLangs = payload.DescriptionLangs;
        entity.IsEnabled = payload.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;

        await tenantCtx.SaveChangesAsync();
        return Ok(new { message = "ContractSpec updated successfully", data = entity });
    }

    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> SwitchStatus(long id)
    {
        var entity = await tenantCtx.ContractSpecs.SingleOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound(new { message = "ContractSpec not found" });
        entity.IsEnabled = !entity.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        await tenantCtx.SaveChangesAsync();
        return Ok(new { message = "ContractSpec status updated successfully", data = entity });
    }
}


