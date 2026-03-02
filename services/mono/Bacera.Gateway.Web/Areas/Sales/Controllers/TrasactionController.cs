using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Transaction;
using MSG = ResultMessage.Transaction;

[Tags("Sales/Transaction")]
public class TransactionController(AccountingService accountingService) : SalesBaseController
{
    /// <summary>
    /// Transaction pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Transaction.TradeAccountTransactionResponseModel>, M.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long salesUid, [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = salesUid;
        var result = await accountingService.TransactionQueryForParentAsync(criteria);
        return Ok(result);
    }
}