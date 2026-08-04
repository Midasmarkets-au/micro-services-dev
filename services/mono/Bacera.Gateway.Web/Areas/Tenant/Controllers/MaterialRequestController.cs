using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = MaterialRequest;

[Tags("Tenant/MaterialRequest")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class MaterialRequestController(TenantDbContext tenantDbContext) : TenantBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<MaterialRequestDTO>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();

        var hideEmail = ShouldHideEmail();
        var items = await tenantDbContext.MaterialRequests
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.PartyTags)
            .PagedFilterBy(criteria)
            .ToListAsync();

        var data = items.Select(x =>
        {
            var dto = MaterialRequestDTO.FromEntity(x);
            dto.User = x.Party.ToTenantBasicViewModel(hideEmail);
            return dto;
        }).ToList();

        return Ok(Result<List<MaterialRequestDTO>, M.Criteria>.Of(data, criteria));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(MaterialRequestDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tenantDbContext.MaterialRequests
            .Include(x => x.Party)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();

        var dto = MaterialRequestDTO.FromEntity(item);
        dto.User = item.Party.ToTenantBasicViewModel(ShouldHideEmail());
        return Ok(dto);
    }

    [HttpPut("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(long id)
    {
        var item = await GetMaterialRequest(id);
        if (item == null) return NotFound();

        item.Status = (int)VerificationStatusTypes.Approved;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.MaterialRequests.Update(item);
        await tenantDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(long id, [FromBody] MaterialRequestRejectModel spec)
    {
        var item = await GetMaterialRequest(id);
        if (item == null) return NotFound();

        item.Status = (int)VerificationStatusTypes.Rejected;
        item.Note = spec.Note;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDbContext.MaterialRequests.Update(item);
        await tenantDbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<MaterialRequest?> GetMaterialRequest(long id)
        => await tenantDbContext.MaterialRequests.SingleOrDefaultAsync(x => x.Id == id);
}
