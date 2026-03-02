using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Lead;
using MSG = ResultMessage.Lead;
[Tags("Tenant/Lead")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class LeadController(TenantDbContext tenantDbContext, ILeadService leadSvc, ConfigService cfgSvc)
    : TenantBaseController
{
    /// <summary>
    /// Lead pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();

        var utmsParam = criteria.Utms != null
                ? criteria.Utms.ToArray()
                : [];

        var items = await tenantDbContext.Leads
          .FromSqlInterpolated($"""
            SELECT t.*
            FROM trd."_Lead" t
            WHERE 
                (
                {criteria.HasUtm == null} or "Supplement"::jsonb ->> 'utm' <> ''
                )
                AND
                (
                    ({criteria.Utms == null}) OR 
                    ("Supplement"::jsonb -> 'utm' ->> 'data' = ANY({utmsParam}))
                )
            """)
          .PagedFilterBy(criteria)
          .ToResponseModel()
          .ToListAsync();

        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Lead auto assign info
    /// </summary>
    /// <returns></returns>
    [HttpGet("auto-assign/info")]
    public async Task<IActionResult> GetAutoAssignInfo()
    {
        var info = await cfgSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        return Ok(info);
    }

    /// <summary>
    /// Enable auto assign
    /// </summary>
    /// <returns></returns>
    [HttpPut("auto-assign/enable")]
    public async Task<IActionResult> EnableAutoAssign()
    {
        var info = await cfgSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        info.Enabled = true;
        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo, info);
        return Ok(info);
    }

    /// <summary>
    /// Disable auto assign
    /// </summary>
    /// <returns></returns>
    [HttpPut("auto-assign/disable")]
    public async Task<IActionResult> DisableAutoAssign()
    {
        var info = await cfgSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        info.Enabled = false;
        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo, info);
        return Ok(info);
    }

    /// <summary>
    /// Set auto assignee
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("auto-assign/set-assignee")]
    public async Task<IActionResult> SetAutoAssignee([FromBody] M.AutoAssignInfo spec)
    {
        var info = await cfgSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        info.AutoAssignAccountUid = spec.AutoAssignAccountUid;
        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo, info);
        return Ok(info);
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
        var result = await leadSvc.GetAsync(id);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Assign lead to sales account
    /// </summary>
    /// <returns></returns>
    [HttpPost("{id:long}/assign/{assignedAccountUid:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign(long repUid, long id, long assignedAccountUid)
    {
        var result = await leadSvc.AssignOwnerAccount(id, assignedAccountUid, GetPartyId());
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
        var result = await leadSvc.UnAssignOwnerAccount(id, assignedAccountUid, GetPartyId());
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
        var result = await leadSvc.Archive(id, LeadIsArchivedTypes.Archived);
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
        var result = await leadSvc.Archive(id, LeadIsArchivedTypes.Unarchived);
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

        var result = await leadSvc.CreateAsync(spec);
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
        var operatorName = await tenantDbContext.Parties.Where(x => x.Id == partyId).Select(x => x.NativeName)
            .SingleOrDefaultAsync();
        spec.Content = $"Tenant-{operatorName}-Comment: " + spec.Content;
        var result = await leadSvc.AddComment(id, spec.Content, GetPartyId());
        return result ? Ok() : BadRequest(Result.Error(MSG.AddedCommentFailed));
    }
}