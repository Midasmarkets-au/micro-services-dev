using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = Rebate;

[Tags("IB/Rebate")]
public class RebateController : AgentBaseController
{
    private readonly AccountingService _accountSvc;

    public RebateController(AccountingService accountingService)
    {
        _accountSvc = accountingService;
    }

    /// <summary>
    /// Rebate pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M>, M.Criteria>>> Index(long agentUid,
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.AccountUid = agentUid;
        criteria.PartyId = GetPartyId();
        criteria.StateIds = [StateTypes.RebateCompleted, StateTypes.RebateOnHold];
        var result = await _accountSvc.RebateQueryForClientAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Rebate
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> Get(long agentUid, long id)
    {
        var result = await _accountSvc.RebateGetForAgentAsync(id, agentUid);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }
}