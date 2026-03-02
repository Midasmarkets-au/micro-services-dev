using Bacera.Gateway.Agent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = TradeAccount;

[Area("IB")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{agentUid:long}/trade-account")]
[Tags("IB/Trade Account")]
public class TradeAccountController : AgentBaseController
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
    /// TradeAccount Pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>>>> Index(long agentUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.AgentUid = agentUid;
        var items = await _tradingSvc.TradeAccountForClientQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Get TradeAccount by uid
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> TradeAccount(long agentUid, long uid)
    {
        var clientAccount = await _tradingSvc.AccountLookupForParentAsync(agentUid, uid);
        if (clientAccount.IsEmpty()) return NotFound();

        var item = await _tradingSvc.TradeAccountGetForPartyAsync(uid, clientAccount.PartyId);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// TradeTransaction pagination (trade history)
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="uid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/trade")]
    [ProducesResponseType(typeof(Result<List<TradeViewModel>, TradeViewModel.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> TradeTransaction(long agentUid, long uid,
        [FromQuery] TradeViewModel.Criteria? criteria)
    {
        var clientAccount = await _tradingSvc.AccountLookupForParentAsync(agentUid, uid);
        if (clientAccount.IsEmpty())
            return NotFound();
        criteria ??= new TradeViewModel.Criteria();
        criteria.Commands = new List<int> { 0, 1 };
        criteria.ServiceId = clientAccount.ServiceId;
        criteria.AccountNumber = clientAccount.AccountNumber;
        return Ok(await _tradingSvc.QueryTrade(criteria));
    }

    /// <summary>
    /// Transaction pagination (accounting history)
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="uid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/transaction")]
    [ProducesResponseType(typeof(Result<List<TransactionForAgentViewModel>, Transaction.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<TransactionForAgentViewModel>, Transaction.Criteria>>>
        AccountingTransaction(long agentUid, long uid, [FromQuery] Transaction.Criteria? criteria)
    {
        var clientAccount = await _tradingSvc.AccountLookupForParentAsync(agentUid, uid);
        if (clientAccount.IsEmpty())
            return NotFound();

        criteria ??= new Transaction.Criteria();
        criteria.ParentAccountUid = agentUid;
        criteria.PartyId = clientAccount.PartyId;
        criteria.AccountId = clientAccount.Id;

        var result = await _acctSvc.TransactionQueryForParentAsync(criteria);
        var res = Result<List<TransactionForAgentViewModel>, Transaction.Criteria>.Of(result.Data, criteria);
        return Ok(res);
    }
}