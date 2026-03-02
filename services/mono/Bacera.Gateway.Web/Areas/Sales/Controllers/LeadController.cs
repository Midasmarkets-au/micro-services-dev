using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Lead;
using MSG = ResultMessage.Lead;

[Tags("Sales/Lead")]
public class LeadController : SalesBaseController
{
    private readonly ILeadService _leadSvc;
    private readonly TenantDbContext _tenantDbContext;

    public LeadController(ILeadService leadSvc,  TenantDbContext tenantDbContext)
    {
        _leadSvc = leadSvc;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Lead Pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<LeadBasicViewModel>, M.Criteria>>> Index(long salesUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.AssignedAccountUid = salesUid;
        var items = await _leadSvc.QueryViewModelAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Lead Get
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long salesUid, long id)
    {
        var result = await _leadSvc.LookUpUnderAssignedAccountUid(salesUid, id);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Add a comment for a lead
    /// </summary>
    /// <returns></returns>
    [HttpPost("{id:long}/comment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(long id, [FromBody] M.AddCommentSpec spec)
    {
        var partyId = GetPartyId();
        var operatorName = await _tenantDbContext.Parties.Where(x => x.Id == partyId).Select(x => x.NativeName).SingleOrDefaultAsync(); 
        spec.Content = $"Sales-{operatorName}-Comment: " + spec.Content;
        var result = await _leadSvc.AddComment(id, spec.Content, GetPartyId());
        return result ? Ok() : BadRequest(Result.Error(MSG.AddedCommentFailed));
    }
}