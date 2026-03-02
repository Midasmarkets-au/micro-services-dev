using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSG = Bacera.Gateway.ResultMessage.RebateRule;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Rebate Agent Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/rebate-agent-rule")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class RebateAgentRuleController(
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    ConfigurationService configurationService,
    ILogger<RebateAgentRuleController> logger)
    : TenantBaseController
{
    /// <summary>
    /// Pagination Agent Rule
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateAgentRule.ResponseModel>, RebateAgentRule.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> AgentRulePagination([FromQuery] RebateAgentRule.Criteria? criteria)
    {
        criteria ??= new RebateAgentRule.Criteria();
        var result = await tradingService.AgentRebateRuleQueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Agent Rule
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgentRuleGet(long id)
    {
        var result = await tradingService.AgentRebateRuleGetAsync(id);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Update Agent Rebate Rule base setting (without symbols)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgentRuleUpdate(long id, [FromBody] RebateAgentRule.UpdateSpec spec)
    {
        var result = await tradingService.AgentRebateRuleUpdateAsync(id, spec, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Update Agent Rebate Level Settings for Top Agent
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/level-setting")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgentRuleLevelSettingUpdate(long id, [FromBody] RebateLevelSchema spec)
    {
        var result = await tradingService.AgentRebateRuleLevelSettingUpdateAsync(id, spec, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok();
    }
    
    /// <summary>
    /// Clear Agent Level Setting
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/clear-level-setting")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgentRuleLevelSettingClear(long id)
    {
        var result = await tradingService.AgentRebateRuleLevelSettingClearAsync(id, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok();
    }

    /// <summary>
    /// Update Agent Rebate Rule Rule Schema for Lower Agent
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/schema")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgentRuleSchemaUpdate(long id, [FromBody] RebateLevelSchema spec)
    {
        var result = await tradingService.AgentRebateRuleSchemaUpdateAsync(id, spec, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok();
    }

    /// <summary>
    /// Create Agent Rebate Rule
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AgentRuleCreate([FromBody] RebateAgentRule.CreateSpec spec)
    {
        var agent = await tradingService.AccountGetAsync(spec.AgentAccountId);
        if (agent.IsEmpty())
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));
        if (agent.Role != (int)AccountRoleTypes.Agent)
            return BadRequest(Result.Error(ResultMessage.Account.AccountIsNotAnAgent));

        var rule = await tradingService.AgentRebateRuleGetByAccountIdAsync(spec.AgentAccountId);
        if (!rule.IsEmpty())
            return BadRequest(Result.Error(ResultMessage.Common.RecordExists));

        var result = await tradingService.AgentRebateRuleCreateAsync(spec, GetPartyId());
        return result.IsEmpty() ? BadRequest(Result.Error(MSG.CreateFailed)) : Ok(result);
    }


    /// <summary>
    /// Delete Agent Rebate Rule
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await tenantDbContext.RebateAgentRules
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null)
            return NotFound();
        try
        {
            tenantDbContext.RebateAgentRules.Remove(item);
            return await tenantDbContext.SaveChangesAsync() > 0
                ? NoContent()
                : BadRequest(ResultMessage.Common.DeleteFail);
        }
        catch (Exception e)
        {
            logger.LogWarning("Delete RebateAgentRule failed by Party Id {PartyId}: {@Message}", GetPartyId(),
                e.Message);
            return BadRequest(ResultMessage.Common.DeleteFail);
        }
    }

    /// <summary>
    /// Get Sales DefaultLevelSetting By Id
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpGet("{accountId}/default-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultLevelSetting(long accountId)
    {
        var account = await tenantDbContext.Accounts
            .Where(x => x.Id == accountId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        var item = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => x.RowId == account.Id)
            .Where(x => x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        var obj = item?.Value;
        if (obj != null) return Ok(obj);

        var result = await configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return Ok(result);
    }
}