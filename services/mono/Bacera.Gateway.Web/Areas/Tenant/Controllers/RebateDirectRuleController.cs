using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSG = Bacera.Gateway.ResultMessage.RebateRule;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Rebate Direct Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/rebate-direct-rule")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RebateDirectRuleController(
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    ILogger<RebateDirectRuleController> logger)
    : TenantBaseController
{
    private readonly ILogger<RebateDirectRuleController> _logger = logger;

    /// <summary>
    /// Direct rule pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DirectRulePagination([FromQuery] RebateDirectRule.Criteria? criteria)
    {
        criteria ??= new RebateDirectRule.Criteria();
        var result = await tradingService.DirectRebateRuleQuery(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Create Direct Rule
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateDirectRule))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DirectRuleCreate(RebateDirectRule.CreateSpec spec)
    {
        var tradeAccount =
            await tenantDbContext.TradeAccounts.SingleOrDefaultAsync(x => x.IdNavigation.Uid == spec.SourceAccountUid);
        if (tradeAccount == null)
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        var agentAccount = await tradingService.AccountGetByUidAsync(spec.TargetAccountUid);
        if (agentAccount.IsEmpty())
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        if (!await tenantDbContext.RebateDirectSchemas.AnyAsync(x => x.Id == spec.RebateRuleId))
            return BadRequest(Result.Error(MSG.RebateRuleNotExists));

        var rule = await tradingService.DirectRebateRuleLookupAsync(tradeAccount.Id, agentAccount.Id);
        if (!rule.IsEmpty())
            return BadRequest(Result.Error(MSG.DirectRuleExists));

        var item = await tradingService.DirectRebateRuleCreate(tradeAccount.Id, agentAccount.Id,
            spec.RebateRuleId, GetPartyId());
        return Ok(item);
    }

    /// <summary>
    /// Update Direct Rule
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DirectRuleUpdate(long id, [FromBody] RebateDirectRule.UpdateSpec spec)
    {
        try
        {
            var item = await tradingService.DirectRebateRuleUpdateRuleId(id, spec.RebateRuleId, GetPartyId());
            return item.IsEmpty() ? NotFound() : NoContent();
        }
        catch
        {
            return BadRequest();
        }
    }

    /// <summary>
    /// Get Direct Rule
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateDirectRule))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DirectRuleGet(long id)
    {
        var item = await tradingService.DirectRebateRuleGet(id);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Confirm Direct Rule
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id:long}/confirm")]
    [ProducesResponseType(typeof(RebateDirectSchema), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DirectRuleConfirm(long id)
    {
        var item = await tenantDbContext.RebateDirectRules
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();

        if (item.CreatedBy == GetPartyId())
            return BadRequest(Result.Error(ResultMessage.RebateRule.CanNotConfirmByCreator));

        item.ConfirmedOn = DateTime.UtcNow;
        item.ConfirmedBy = GetPartyId();
        item.UpdatedOn = DateTime.UtcNow;

        tenantDbContext.RebateDirectRules.Update(item);
        await tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        return Ok(item);
    }

    /// <summary>
    /// Delete Direct Rule
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DirectRuleDelete(long id)
    {
        try
        {
            var result = await tradingService.DirectRebateRuleDelete(id, GetPartyId());
            return result ? NoContent() : BadRequest();
        }
        catch
        {
            return BadRequest(Result.Error(ResultMessage.Common.DeleteFail));
        }
    }
}