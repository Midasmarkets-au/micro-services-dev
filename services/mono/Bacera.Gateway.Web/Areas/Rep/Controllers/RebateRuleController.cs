using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

[Area("Rep")]
[Tags("Rep/Rebate Rule")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{agentUid:long}/rebate-rule")]
public class RebateRuleController : RepBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly ILogger<RebateRuleController> _logger;

    public RebateRuleController(TradingService tradingService, ILogger<RebateRuleController> logger)
    {
        _logger = logger;
        _tradingSvc = tradingService;
    }

    /// <summary>
    /// Rebate Rule Pagination
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<RebateAgentRule.ResponseModel>, RebateAgentRule.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long repUid, [FromQuery] RebateAgentRule.Criteria? criteria)
    {
        criteria ??= new RebateAgentRule.Criteria();
        criteria.RepUid = repUid;
        var result = await _tradingSvc.AgentRebateRuleQueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate Rule
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RebateAgentRule.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long repUid, long id)
    {
        var result = await _tradingSvc.AgentRebateRuleGetForRepAsync(id, repUid);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }
}