using Bacera.Gateway.Core.Types;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using M = Lead;
using MSG = ResultMessage.Lead;

[Tags("Rep/Lead")]
public class LeadController : RepBaseController
{
    private readonly ILeadService _leadSvc;
    private readonly TenantDbContext _tenantDbContext;

    public LeadController(
        ILeadService leadSvc
        , TenantDbContext tenantDbContext)
    {
        _leadSvc = leadSvc;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Lead Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<LeadBasicViewModel>, M.Criteria>>> Index(
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await _leadSvc.QueryViewModelAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Lead Get
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await _leadSvc.GetAsync(id);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Assign lead to sales account
    /// </summary>
    /// <returns></returns>
    [HttpPost("{id:long}/assign/{assignedAccountUid:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign(long id, long assignedAccountUid)
    {
        var result = await _leadSvc.AssignOwnerAccount(id, assignedAccountUid, GetPartyId());
        return result ? Ok() : BadRequest(Result.Error(MSG.AssignFailed));
    }

    /// <summary>
    /// Remove lead from sales account
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id:long}/assign/{assignedAccountUid:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnAssign(long repUid, long id, long assignedAccountUid)
    {
        var result = await _leadSvc.UnAssignOwnerAccount(id, assignedAccountUid, GetPartyId());
        return result ? Ok() : BadRequest(Result.Error(MSG.AssignFailed));
    }

    /// <summary>
    /// Archive or Unarchive a lead
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id:long}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Archive(long id)
    {
        var result = await _leadSvc.Archive(id, LeadIsArchivedTypes.Archived);
        return result ? Ok() : BadRequest(Result.Error(MSG.ArchivedFailed));
    }

    /// <summary>
    /// Unarchive a lead
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id:long}/unarchive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnArchive(long id)
    {
        var result = await _leadSvc.Archive(id, LeadIsArchivedTypes.Unarchived);
        return result ? Ok() : BadRequest(Result.Error(MSG.ArchivedFailed));
    }

    /// <summary>
    /// Create a lead
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var specValidator = new LeadCreateSpecValidator();
        var specValidationResult = await specValidator.ValidateAsync(spec);
        if (!specValidationResult.IsValid)
            return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));

        var result = await _leadSvc.CreateAsync(spec);
        return Ok(result);
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
        spec.Content = $"Rep-{operatorName}-Comment: " + spec.Content;
        var result = await _leadSvc.AddComment(id, spec.Content, GetPartyId());
        return result ? Ok() : BadRequest(Result.Error(MSG.AddedCommentFailed));
    }
}