using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSG = Bacera.Gateway.ResultMessage.RebateRule;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Rebate Client Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/rebate-client-rule")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RebateClientRuleController(TradingService tradingService) : TenantBaseController
{

    /// <summary>
    /// Pagination Client Rule
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateClientRule.ResponseModel>, RebateClientRule.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> QueryClientRules([FromQuery] RebateClientRule.Criteria? criteria)
    {
        criteria ??= new RebateClientRule.Criteria();
        var result = await tradingService.ClientRebateRuleQueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Client Rule
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateClientRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientRule(long id)
    {
        var result = await tradingService.ClientRebateRuleGetAsync(id);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Update Client Rebate Rule Distribution Type
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateClientRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] RebateClientRule.UpdateSpec spec)
    {
        var result = await tradingService.ClientRebateRuleDistributionUpdateAsync(id, spec, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok(result.ToResponseModel());
    }
}