using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

[Area("Sales")]
[Tags("Sales/Rebate Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{salesUid:long}/rebate-rule")]
public class RebateRuleController : SalesBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly TenantDbContext _tenantDbContext;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<RebateRuleController> _logger;

    public RebateRuleController(
        TradingService tradingService
        , TenantDbContext tenantDbContext
        , ConfigurationService configurationService
        , ILogger<RebateRuleController> logger)
    {
        _logger = logger;
        _tradingSvc = tradingService;
        _tenantDbContext = tenantDbContext;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Agent Rebate Rule Pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("agent")]
    [ProducesResponseType(typeof(Result<List<RebateAgentRule.ResponseModel>, RebateAgentRule.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long salesUid, [FromQuery] RebateAgentRule.Criteria? criteria)
    {
        criteria ??= new RebateAgentRule.Criteria();
        criteria.SalesUid = salesUid;
        criteria.ViewAllLevel = true;
     
        var result = await _tradingSvc.AgentRebateRuleQueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Agent Rebate Rule
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("agent/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long salesUid, long id)
    {
        var result = await _tradingSvc.AgentRebateRuleGetForSalesAsync(salesUid, id);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get Agent Rebate Rule
    /// </summary>
    /// <param name="salesUid"></param>
    /// <returns></returns>
    [HttpGet("default-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultLevelSetting(long salesUid)
    {
        var account = await _tenantDbContext.Accounts
            .Where(x => x.Uid == salesUid)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        var item = await _tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => x.RowId == account.Id)
            .Where(x => x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        var obj = item?.Value;
        if (obj != null) return Ok(obj);

        var result = await _configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate Rule Level Settings
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("agent/{agentUid:long}/detail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRebateRuleDetail(long salesUid, long agentUid)
    {
        var rebateRule = await _tenantDbContext.RebateAgentRules
            .Where(x => x.AgentAccount.ReferPath.Contains(salesUid.ToString()))
            .Where(x => x.AgentAccount.Uid == agentUid)
            .ToResponseModel()
            .FirstOrDefaultAsync();
        if (rebateRule == null)
            return NotFound();
        
        rebateRule.CalculatedLevelSetting =
            await _tradingSvc.GetCalculatedRebateLevelSettingById(rebateRule.AgentAccountId);
        
        return Ok(rebateRule);
    }

    /// <summary>
    /// Update RebateAgentRule for child agent
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="agentUid"></param>
    /// <param name="rebateRuleId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("agent/{agentUid:long}/{rebateRuleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long salesUid, long agentUid, long rebateRuleId,
        List<RebateLevelSchema> spec)
    {
        var agentAccount = await _tenantDbContext.Accounts
            .Where(x => x.ReferPath.Contains(salesUid.ToString()))
            .Where(x => x.Uid == agentUid)
            .FirstOrDefaultAsync();

        if (agentAccount == null) // || agentAccount.IsMySales(salesUid)
            return NotFound(ResultMessage.Common.AgentUidNotFound);

        var rebateRule = await _tenantDbContext.RebateAgentRules
            .Where(x => x.Id == rebateRuleId)
            .Where(x => x.AgentAccount.ReferPath.StartsWith(agentAccount.ReferPath))
            .FirstOrDefaultAsync();
        if (rebateRule == null)
            return NotFound();

        var selfRebateRule = await _tenantDbContext.RebateAgentRules
            .Where(x => x.AgentAccount.Uid == agentUid)
            .ToResponseModel()
            .FirstOrDefaultAsync();
        if (selfRebateRule == null)
            return BadRequest(Result.Error(ResultMessage.RebateRule.RebateRuleSettingNotExists));

        // var (isValid, result) =
        //     TradingService.IsRebateLevelSchemasValid(spec, selfRebateRule.LevelSetting.AllowedAccounts);
        // if (!isValid) return BadRequest(result);

        // TODO: Parent agent takes less rebate rate from child agent, the decreased amount of rate should be added to children agents' rebate rules setting for their agents. 
        var subAgentRebateRules = await _tenantDbContext.RebateAgentRules
            .Where(x => x.ParentId == rebateRule.Id)
            .ToListAsync();

        var levelSchemaFromRebateRule = rebateRule.GetSchema();
        foreach (var subAgentRebateRule in subAgentRebateRules)
        {
            var subLevelSchemas = subAgentRebateRule.GetSchema();
            var language = subAgentRebateRule.GetLevelSetting().Language;

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

            var subAgentRebateRuleSettings = new RebateAgentRule.RebateLevelSetting
            {
                Language = language,
                AllowedAccounts = subLevelSchemas,
            };

            subAgentRebateRule.Schema = JsonConvert.SerializeObject(subLevelSchemas);
            // subAgentRebateRule.LevelSetting = JsonConvert.SerializeObject(subAgentRebateRuleSettings);
            subAgentRebateRule.UpdatedOn = DateTime.UtcNow;

            _tenantDbContext.RebateAgentRules.Update(subAgentRebateRule);
            await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());
        }
        // END

        var levelSetting = JsonConvert.DeserializeObject<ReferralCode.AgentCreateSpec>(rebateRule.LevelSetting);
        var settings = new RebateAgentRule.RebateLevelSetting
        {
            Language = levelSetting?.Language,
            AllowedAccounts = spec,
        };

        rebateRule.Schema = JsonConvert.SerializeObject(spec);
        // rebateRule.LevelSetting = JsonConvert.SerializeObject(settings);
        rebateRule.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.RebateAgentRules.Update(rebateRule);
        await _tenantDbContext.SaveChangesWithAuditAsync(GetPartyId());

        return Ok(rebateRule.ToResponseModel());
    }

    /// <summary>
    /// Update RebateAgentRule for child agent
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="agentUid"></param>
    /// <param name="rebateRuleId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("top-agent/{agentUid:long}/{rebateRuleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgentRebateRuleUpdateAsync(long salesUid, long agentUid, long rebateRuleId,
        RebateAgentRule.UpdateSpec spec)
    {
        foreach (var schema in spec.Schema)
        {
            // sorting and add default zero value
            // if (schema.AllowPips.Count == 0) schema.AllowPips.Add(0);
            // if (schema.AllowCommissions.Count == 0) schema.AllowCommissions.Add(0);
            schema.AllowPips = schema.AllowPips.OrderBy(x => x).Distinct().ToList();
            schema.AllowCommissions = schema.AllowCommissions.OrderBy(x => x).Distinct().ToList();
        }

        var result = await _tradingSvc.AgentRebateRuleUpdateAsync(rebateRuleId, spec, GetPartyId());
        return result.IsEmpty() ? NotFound() : Ok(result);
    }
}