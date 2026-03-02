using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = Withdrawal;
using MSG = ResultMessage.Withdrawal;

[Tags("IB/Withdrawal")]
public class WithdrawalController : AgentBaseController
{
    private readonly AccountingService _accountSvc;

    public WithdrawalController(AccountingService accountingService)
    {
        _accountSvc = accountingService;
    }

    /// <summary>
    /// Withdrawal Pagination
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
        
        return Ok(await _accountSvc.WithdrawalQueryForParentAsync(criteria));
    }
}