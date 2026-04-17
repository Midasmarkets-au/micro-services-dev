using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Symbol")]
[Route("api/" + VersionTypes.V1 + "/[Area]/symbol")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class SymbolCategoryController(TenantDbContext tenantDbContext) : TenantBaseController
{
    /// <summary>
    /// Get distinct symbol categories grouped by Type, with symbols in each category.
    /// Returns the same format as the legacy HTTP endpoint.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] int? type = null)
    {
        var query = tenantDbContext.Symbols.AsQueryable();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        var categories = await query
            .GroupBy(x => new { x.CategoryId, x.Category, x.Type })
            .Select(g => new
            {
                categoryId = g.Key.CategoryId,
                category   = g.Key.Category,
                type       = g.Key.Type,
                symbols    = g.OrderByDescending(x => x.Id)
                              .Select(x => new { id = x.Id, code = x.Code })
                              .ToArray(),
            })
            .OrderByDescending(x => x.categoryId)
            .ToListAsync();

        return Ok(categories);
    }
}
