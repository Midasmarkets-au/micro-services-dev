using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

[Tags("Sales/Rebate")]
public class RebateController : SalesBaseController
{
    private readonly AccountingService _accountSvc;

    public RebateController(AccountingService accountingService)
    {
        _accountSvc = accountingService;
    }

    /// <summary>
    /// Rebate pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<Rebate>, Rebate.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<Rebate>, Rebate.Criteria>>> Index(long salesUid,
        [FromQuery] Rebate.Criteria? criteria)
    {
        criteria ??= new Rebate.Criteria();
        criteria.SalesUid = salesUid;
        var hideEmail = ShouldHideEmail();
        var result = await _accountSvc.RebateQueryAsync(criteria, hideEmail);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(Rebate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Rebate>> Get(long salesUid, long id)
    {
        var result = await _accountSvc.RebateGetForAgentAsync(id, salesUid);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }
}