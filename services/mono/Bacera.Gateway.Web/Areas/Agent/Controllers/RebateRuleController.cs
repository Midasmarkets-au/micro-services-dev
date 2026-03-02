using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

[Area("IB")]
[Tags("IB/Rebate Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{agentUid:long}/rebate-rule")]
public class RebateRuleController(TradingService tradingService, TenantDbContext tenantDbContext, ConfigurationService configurationService)
    : AgentBaseController
{
    /// <summary>
    /// Rebate Rule Pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateAgentRule.ResponseModel>, RebateAgentRule.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long agentUid, [FromQuery] RebateAgentRule.Criteria? criteria)
    {
        criteria ??= new RebateAgentRule.Criteria();
        criteria.AgentUid = agentUid;
        var result = await tradingService.AgentRebateRuleQueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate Rule
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="rebateRuleId"></param>
    /// <returns></returns>
    [HttpGet("{rebateRuleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long agentUid, long rebateRuleId)
    {
        var result = await tradingService.AgentRebateRuleGetForAgentAsync(rebateRuleId, agentUid);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get Rebate Rule for child agent
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="accountUid"></param>
    /// <returns></returns>
    [HttpGet("account/{accountUid:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForAccount(long agentUid, long accountUid)
    {
        var rebateRule = await tenantDbContext.RebateAgentRules
            .Where(x => x.AgentAccount.ReferPath.Contains(agentUid.ToString()))
            .Where(x => x.AgentAccount.Uid == accountUid)
            .ToResponseModel()
            .FirstOrDefaultAsync();
        if (rebateRule == null)
            return NotFound();
        
        rebateRule.CalculatedLevelSetting =
            await tradingService.GetCalculatedRebateLevelSettingById(rebateRule.AgentAccountId);
        
        return Ok(rebateRule);
    }

    /// <summary>
    /// Update RebateAgentRule for child agent
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="rebateRuleId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{rebateRuleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long agentUid, long rebateRuleId, List<RebateLevelSchema> spec)
    {
        var rebateRule = await tenantDbContext.RebateAgentRules
            .Where(x => x.Id == rebateRuleId)
            .Where(x => x.AgentAccount.ReferPath.Contains(agentUid.ToString()))
            .FirstOrDefaultAsync();
        if (rebateRule == null)
            return NotFound();

        var selfRebateRule = await tradingService.GetCalculatedRebateLevelSettingByUid(agentUid);
        if (selfRebateRule.IsEmpty())
            return BadRequest(Result.Error(ResultMessage.RebateRule.RebateRuleSettingNotExists));
        
        // TODO: Parent agent takes less rebate rate from child agent, the decreased amount of rate should be added to children agents' rebate rules setting for their agents. 
        var rebateRuleAgentAccount = await tenantDbContext.Accounts
            .Where(x => x.Id == rebateRule.AgentAccountId)
            .FirstOrDefaultAsync();
        if (rebateRuleAgentAccount == null) return NotFound();

        var subAgentIds = await tenantDbContext.Accounts
            .Where(x => x.ReferPath.Contains(rebateRuleAgentAccount.Uid.ToString()))
            .Where(x => x.Role == (short)AccountRoleTypes.Agent)
            .Where(x => x.Level == rebateRuleAgentAccount.Level+1)
            .Select(x => x.Id)
            .ToListAsync();

        var subAgentRebateRules = await tenantDbContext.RebateAgentRules
            .Where(x => subAgentIds.Any(id => id == x.AgentAccountId))
            .ToListAsync(); 
        
        var levelSchemaFromRebateRule = rebateRule.GetSchema();
        foreach (var subAgentRebateRule in subAgentRebateRules)
        {
            var subLevelSchemas = subAgentRebateRule.GetSchema();
            foreach (var schemaItem in subLevelSchemas)
            {
                var schemaFromSpec = spec.FirstOrDefault(x => x.AccountType == schemaItem.AccountType);
                var schemaFromRebateRule = levelSchemaFromRebateRule
                    .FirstOrDefault(x => x.AccountType == schemaItem.AccountType);
                if (schemaFromSpec == null || schemaFromRebateRule == null)
                    continue;

                foreach (var item in schemaItem.Items)
                {
                    var categoryFromSpec = schemaFromSpec.Items
                        .FirstOrDefault(x => x.CategoryId == item.CategoryId);
                    var categoryFromRule = schemaFromRebateRule.Items
                        .FirstOrDefault(x => x.CategoryId == item.CategoryId);
                    if (categoryFromSpec == null || categoryFromRule == null)
                        continue;

                    var diff = categoryFromRule.Rate - categoryFromSpec.Rate;
                    item.Rate += diff;
                }
            }

            subAgentRebateRule.Schema = JsonConvert.SerializeObject(subLevelSchemas);
            subAgentRebateRule.UpdatedOn = DateTime.UtcNow;

            tenantDbContext.RebateAgentRules.Update(subAgentRebateRule);
            await tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        }

        rebateRule.Schema = JsonConvert.SerializeObject(spec);
        rebateRule.UpdatedOn = DateTime.UtcNow;

        tenantDbContext.RebateAgentRules.Update(rebateRule);
        await tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());

        return Ok(rebateRule.ToResponseModel());
    }

    /// <summary>
    /// Get Rebate Rule Level Settings
    /// </summary>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("detail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRebateRuleDetail(long agentUid)
    {
        var rebateRule = await tenantDbContext.RebateAgentRules
            .Where(x => x.AgentAccount.Uid == agentUid)
            .ToResponseModel()
            .FirstOrDefaultAsync();
        if (rebateRule == null) return NotFound();

        rebateRule.CalculatedLevelSetting = await tradingService.GetCalculatedRebateLevelSettingById(rebateRule.AgentAccountId);

        // hide percentage setting if not top ib
        // ReSharper disable once InvertIf
        if (rebateRule.LevelSetting.DistributionType == RebateDistributionTypes.LevelPercentage && !rebateRule.IsRoot)
        {
            rebateRule.LevelSetting.PercentageSetting = new Dictionary<string, List<decimal>>();
            rebateRule.CalculatedLevelSetting.PercentageSetting = new Dictionary<string, List<decimal>>();
        }
        return Ok(rebateRule);
    }

    /// <summary>
    /// Get Agent Rebate Rule
    /// </summary>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("default-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultLevelSetting(long agentUid)
    {
        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == agentUid)
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