using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

using M = Rebate;

[Tags("Rep/Rebate")]
public class RebateController : RepBaseController
{
    private readonly AccountingService _accountSvc;

    public RebateController(AccountingService accountingService)
    {
        _accountSvc = accountingService;
    }

    /// <summary>
    /// Rebate pagination
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M>, M.Criteria>>> Index(long repUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.RepUid = repUid;
        var result = await _accountSvc.RebateQueryForClientAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> Get(long repUid, long id)
    {
        var result = await _accountSvc.RebateGetForRepAsync(id, repUid);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }
}