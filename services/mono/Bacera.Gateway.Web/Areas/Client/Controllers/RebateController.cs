
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Rebate;

[Tags("Client/Rebate")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class RebateController(AccountingService accountingService, TenantDbContext tenantCtx)
    : ClientBaseController
{
    /// <summary>
    /// Rebate pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Rebate.ClientResponseModel>, Rebate.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<Rebate.ClientResponseModel>, Rebate.Criteria>>> Index(
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        var result = await accountingService.RebateQueryForClientAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M>> Get(long id)
    {
        var result = await accountingService.RebateGetForPartyAsync(id, GetPartyId());
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get All rebate symbols
    /// </summary>
    /// <returns></returns>
    [HttpGet("symbol/all")]
    [ProducesResponseType(typeof(List<RebateSymbol>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllRebateSymbol()
    {
        var result = await tenantCtx.Symbols
            .ToRebateSymbols(400)
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Get rebate symbol categories
    /// </summary>
    /// <returns></returns>
    [HttpGet("symbol/category")]
    [ProducesResponseType(typeof(List<RebateSymbol>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateSymbolCategory() =>
        Ok(await tenantCtx.Symbols.ToRebateCategories(300).ToListAsync());
}