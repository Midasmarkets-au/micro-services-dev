using Bacera.Gateway.Agent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using M = TradeAccount;

[Area("Rep")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{repUid:long}/trade-account")]
[Tags("Rep/Trade Account")]
public class TradeAccountController : RepBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly AccountingService _acctSvc;

    public TradeAccountController(
        TradingService tradingService
        , AccountingService accountingService
    )
    {
        _tradingSvc = tradingService;
        _acctSvc = accountingService;
    }

    /// <summary>
    /// TradeTransaction pagination (trade history)
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="uid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/trade")]
    [ProducesResponseType(typeof(Result<List<TradeViewModel>, TradeViewModel.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> TradeTransaction(long repUid, long uid,
        [FromQuery] TradeViewModel.Criteria? criteria)
    {
        var clientAccount = await _tradingSvc.AccountLookupForParentAsync(repUid, uid);
        if (clientAccount.IsEmpty())
            return NotFound();

        criteria ??= new TradeViewModel.Criteria();
        criteria.ServiceId = clientAccount.ServiceId;
        criteria.AccountNumber = clientAccount.AccountNumber;
        criteria.Commands = new List<int> { 0, 1 };
        return Ok(await _tradingSvc.QueryTrade(criteria));
    }

    /// <summary>
    /// Transaction pagination (accounting history)
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="uid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/transaction")]
    [ProducesResponseType(typeof(Result<List<TransactionForAgentViewModel>, Transaction.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<TransactionForAgentViewModel>, Transaction.Criteria>>>
        AccountingTransaction(long repUid, long uid, [FromQuery] Transaction.Criteria? criteria)
    {
        var clientAccount = await _tradingSvc.AccountLookupForParentAsync(repUid, uid);
        if (clientAccount.IsEmpty())
            return NotFound();

        criteria ??= new Transaction.Criteria();
        criteria.ParentAccountUid = repUid;
        criteria.PartyId = clientAccount.PartyId;
        criteria.AccountId = clientAccount.Id;

        var items = await _acctSvc.TransactionQueryForParentAsync(criteria);
        return items;
    }
}