using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using M = Bacera.Gateway.ExchangeRate;
using MSG = Bacera.Gateway.ResultMessage.Common;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Area("Client")]
[Tags("Client/Exchange Rate")]
[Route("api/" + VersionTypes.V1 + "/[Area]/exchange-rate")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class ExchangeRateController : ClientBaseController
{
    private readonly TradingService _tradeService;

    public ExchangeRateController(TradingService tradeService)
    {
        _tradeService = tradeService;
    }

    /// <summary>
    /// Pagination Exchange Rate
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var roles = await _tradeService.ExchangeRateForClientQueryAsync(criteria);
        return Ok(roles);
    }
}