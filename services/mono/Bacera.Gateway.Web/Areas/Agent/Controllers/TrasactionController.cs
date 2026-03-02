using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = Transaction;
using MSG = ResultMessage.Transaction;

[Tags("IB/Transaction")]
public class TransactionController : AgentBaseController
{
    private readonly TradingService _tradeSvc;
    private readonly AccountingService _accountingSvc;

    public TransactionController(
        AccountingService accountingService
        , TradingService tradingService
    )
    {
        _tradeSvc = tradingService;
        _accountingSvc = accountingService;
    }

    /// <summary>
    /// Transaction pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.TradeAccountTransactionResponseModel>, M.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long agentUid, [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = agentUid;
        var result = await _accountingSvc.TransactionQueryForParentAsync(criteria);
        return Ok(result);
    }
}