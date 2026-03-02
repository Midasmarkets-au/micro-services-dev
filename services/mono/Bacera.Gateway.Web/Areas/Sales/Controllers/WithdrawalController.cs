using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Withdrawal;
using MSG = ResultMessage.Withdrawal;

[Tags("Sales/Withdrawal")]
public class WithdrawalController(AccountingService accountingService, TenantDbContext tenantCtx) : SalesBaseController
{
    /// <summary>
    /// Withdrawal Pagination
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
        return Ok(await accountingService.WithdrawalQueryForParentAsync(criteria));
    }
}