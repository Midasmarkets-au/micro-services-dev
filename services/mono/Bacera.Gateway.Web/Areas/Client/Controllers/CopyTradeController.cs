using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Vendor.MetaTrader;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = CopyTrade;

[Area("Client")]
[Tags("Client/Copy Trade")]
[Route("api/" + VersionTypes.V1 + "/[Area]/copy-trade")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class CopyTradeController : ClientBaseController
{
    private readonly ICopyTradeService _svc;
    private readonly TenantDbContext _tenantDbContext;

    public CopyTradeController(ICopyTradeService svc, TenantDbContext tenantDbContext)
    {
        _svc = svc;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Copy Trade Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Query([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        var result = await _svc.QueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get CopyTrade
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await _svc.GetForPartyAsync(id, GetPartyId());
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Create CopyTrade
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var isAccountOwner = await _tenantDbContext.Accounts
            .Where(x => x.Id == spec.Target)
            .Where(x => x.PartyId == GetPartyId())
            .AnyAsync();
        if (false == isAccountOwner)
        {
            return BadRequest(Result.Error(ResultMessage.CopyTrade.AccountIsNotOwnedByCurrentParty));
        }

        var result = await _svc.CreateAsync(spec.Source, spec.Target, spec.Mode, spec.Value);
        return result.IsSuccess() ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete CopyTrade
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _svc.DeleteForPartyAsync(id, GetPartyId());
        return result.IsSuccess() ? NoContent() : BadRequest();
    }
}