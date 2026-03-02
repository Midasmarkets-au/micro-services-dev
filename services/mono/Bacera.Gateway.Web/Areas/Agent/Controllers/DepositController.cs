using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = Deposit;
using MSG = ResultMessage.Deposit;

[Tags("IB/Deposit")]
public class DepositController : AgentBaseController
{
    private readonly AccountingService _accountSvc;
    private readonly TradingService _tradingSvc;

    public DepositController(
        AccountingService accountingService
        , TradingService tradingSvc)
    {
        _accountSvc = accountingService;
        _tradingSvc = tradingSvc;
    }

    /// <summary>
    /// Deposit Pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>, M.Criteria>>> Index(long agentUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = agentUid;
        return Ok(await _accountSvc.DepositQueryForParentAsync(criteria));
    }
}