using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using M = TradeViewModel;

[Tags("Rep/Trade Transaction")]
public class TradeTransactionController : RepBaseController
{
    private readonly TradingService _tradingService;
    private readonly ConfigurationService _configurationService;

    public TradeTransactionController(TradingService tradingService, ConfigurationService configurationService)
    {
        _tradingService = tradingService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Trade Transaction pagination (trade history)
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TradeViewModel>, TradeViewModel.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long repUid, [FromQuery] TradeViewModel.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = repUid;
        criteria.Commands = new List<int> { 0, 1 };
        criteria.ServiceId ??= await _configurationService.GetDefaultTradeServiceAsync();
        return Ok(await _tradingService.QueryTrade(criteria));
    }
}