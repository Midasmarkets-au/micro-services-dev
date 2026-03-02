using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Deposit;
using MSG = ResultMessage.Deposit;

[Tags("Sales/Deposit")]
public class DepositController(AccountingService accountingService)
    : SalesBaseController
{
    

    /// <summary>
    /// Deposit Pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>, M.Criteria>>> Index(long salesUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = salesUid;
        return Ok(await accountingService.DepositQueryForParentAsync(criteria));
    }
}