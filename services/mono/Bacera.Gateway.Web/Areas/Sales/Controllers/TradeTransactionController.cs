using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = TradeViewModel;

[Tags("Sales/Trade Transaction")]
public class TradeTransactionController(TradingService tradingService, ConfigurationService configurationService)
    : SalesBaseController
{
    /// <summary>
    /// Trade Transaction Pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M>, M.Criteria>>> Index(long salesUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = salesUid;
        criteria.Commands = [0, 1];
        criteria.ServiceId ??= await configurationService.GetDefaultTradeServiceAsync();
        return Ok(await tradingService.QueryTrade(criteria, true));
    }
}