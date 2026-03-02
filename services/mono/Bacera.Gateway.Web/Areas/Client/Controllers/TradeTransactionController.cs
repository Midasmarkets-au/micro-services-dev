using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

// using M = TradeTransaction;
[Tags("Client/Trade Transaction")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class TradeTransactionController : ClientBaseController
{
    private readonly TradingService _tradingService;

    public TradeTransactionController(TradingService tradingService)
    {
        _tradingService = tradingService;
    }
    // Removed without not front-end requesting this endpoint
    // /// <summary>
    // /// Trade Transaction Pagination
    // /// </summary>
    // /// <param name="criteria"></param>
    // /// <returns></returns>
    // [HttpGet]
    // [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    // public async Task<ActionResult<Result<List<M>, M.Criteria>>> Index(
    //     [FromQuery] M.Criteria? criteria)
    // {
    //     criteria ??= new M.Criteria();
    //     criteria.PartyId = GetPartyId();
    //     return Ok(await _tradingService.TradeTransactionQueryAsync(criteria));
    // }
}