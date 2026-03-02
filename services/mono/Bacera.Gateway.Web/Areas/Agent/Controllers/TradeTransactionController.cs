using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = TradeViewModel;

[Tags("IB/Trade Transaction")]
public class TradeTransactionController : AgentBaseController
{
    private readonly TradingService _tradingService;
    private readonly ConfigurationService _configurationService;

    public TradeTransactionController(TradingService tradingService, ConfigurationService configurationService)
    {
        _tradingService = tradingService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Trade Transaction Pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M>, M.Criteria>>> Index(long agentUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = agentUid;
        criteria.Commands = new List<int> { 0, 1 };
        criteria.ServiceId ??= await _configurationService.GetDefaultTradeServiceAsync();
        return Ok(await _tradingService.QueryTrade(criteria));
    }
}