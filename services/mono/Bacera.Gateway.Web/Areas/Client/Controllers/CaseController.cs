using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using HashidsNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Case")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class CaseController : ClientBaseController
{
    private readonly TenantDbContext _ctx;

    public CaseController(TenantDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Case.Criteria? criteria)
    {
        criteria ??= new Case.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await _ctx.Cases
            .Where(x => x.ReplyId == null)
            .PagedFilterBy(criteria)
            .ToClientBasicModel()
            .ToListAsync();
        return Ok(Result<List<Case.ClientBasicModel>, Case.Criteria>.Of(items, criteria));
    }

    [HttpGet("{caseId}")]
    public async Task<IActionResult> Get(string caseId)
    {
        var partyId = GetPartyId();
        var item = await _ctx.Cases
            .Where(x => x.CaseId == caseId && x.PartyId == partyId)
            .Include(x => x.InverseReply)
            .ThenInclude(x => x.Category)
            .ToClientResponseModel()
            .FirstOrDefaultAsync();
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        item.InverseReply = item.InverseReply.OrderBy(x => x.CreatedOn).ToList();
        return Ok(item);
    }

    [HttpPost("{caseId}/reply")]
    public async Task<IActionResult> Reply(string caseId, [FromBody] Case.ReplySpec spec)
    {
        var partyId = GetPartyId();
        var item = await _ctx.Cases.FirstOrDefaultAsync(x => x.CaseId == caseId && x.PartyId == partyId);
        if (item == null) return NotFound("__CASE_NOT_FOUND__");
        if (!item.CanReply()) return BadRequest("__CASE_CANNOT_REPLY__");

        var operatorPartyId = GetPartyId();
        var reply = spec.ToEntity(operatorPartyId, false).ReplyTo(item, (CaseStatusTypes)item.Status);
        _ctx.Cases.Add(reply);
        _ctx.Cases.Update(item);
        await _ctx.SaveChangesAsync();

        return Ok(reply);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Case.ClientCreateSpec spec)
    {
        var partyId = GetPartyId();
        var item = spec.ToEntity();
        item.PartyId = partyId;
        item.CreatedOn = DateTime.UtcNow;
        item.UpdatedOn = DateTime.UtcNow;
        _ctx.Cases.Add(item);
        await _ctx.SaveChangesAsync();

        var hashIds = new Hashids("BCRCaseId", 6, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        item.CaseId = hashIds.Encode((int)item.Id);
        await _ctx.SaveChangesAsync();
        return Ok(item);
    }

    [HttpGet("category")]
    public async Task<IActionResult> GetCategories([FromQuery] long? parentCategoryId)
        => Ok(await _ctx.CaseCategories
            .Where(x => x.ParentId == parentCategoryId)
            .ToCategoryResponseModel()
            .ToListAsync());
}