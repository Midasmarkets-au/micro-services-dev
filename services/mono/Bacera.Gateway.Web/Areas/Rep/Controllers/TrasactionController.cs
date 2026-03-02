namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using Microsoft.AspNetCore.Mvc;
using M = Transaction;

[Tags("Rep/Transaction")]
public class TransactionController : RepBaseController
{
    private readonly AccountingService _accountingSvc;

    public TransactionController(
        AccountingService accountingService
    )
    {
        _accountingSvc = accountingService;
    }

    /// <summary>
    /// Transaction pagination
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Transaction.TradeAccountTransactionResponseModel>, M.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long repUid, [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = repUid;
        var result = await _accountingSvc.TransactionQueryForParentAsync(criteria);
        return Ok(result);
    }
}