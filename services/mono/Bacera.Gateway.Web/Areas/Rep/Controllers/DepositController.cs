using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using M = Deposit;
using MSG = ResultMessage.Deposit;

[Tags("Rep/Deposit")]
public class DepositController(AccountingService accountingService) : RepBaseController
{
    /// <summary>
    /// Deposit Pagination
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
        return Ok(await accountingService.DepositQueryForParentAsync(criteria));
    }
}