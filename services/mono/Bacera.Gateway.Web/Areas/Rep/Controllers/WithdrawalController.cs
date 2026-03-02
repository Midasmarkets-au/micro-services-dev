using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using M = Withdrawal;
using MSG = ResultMessage.Withdrawal;

[Tags("Rep/Withdrawal")]
public class WithdrawalController : RepBaseController
{
    private readonly AccountingService _accountSvc;

    public WithdrawalController(AccountingService accountingService)
    {
        _accountSvc = accountingService;
    }

    /// <summary>
    /// Withdrawal Pagination
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>, M.Criteria>>> Index(long repUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = repUid;
        return Ok(await _accountSvc.WithdrawalQueryForParentAsync(criteria));
    }
}