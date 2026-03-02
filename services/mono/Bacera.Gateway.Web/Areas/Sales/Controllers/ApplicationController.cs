using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Deposit;
using MSG = ResultMessage.Deposit;

[Tags("Sales/Application")]
public class ApplicationController : SalesBaseController
{
    private readonly ApplicationService _applicationService;
    private readonly TenantDbContext _tenantDbContext;
    private readonly IMediator _mediator;

    public ApplicationController(ApplicationService applicationService, TenantDbContext tenantDbContext,
        IMediator mediator)
    {
        _applicationService = applicationService;
        _tenantDbContext = tenantDbContext;
        _mediator = mediator;
    }

    [HttpPost("for-user/{accountUid:long}/trade-account")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<ActionResult<M>> Create(long salesUid, long accountUid,
        [FromBody] ApplicationSupplement supplement)
    {
        var partyId = await _tenantDbContext.Accounts
            .Where(a => a.Uid == accountUid && a.ReferPath.Contains(salesUid.ToString()))
            .Select(a => a.PartyId)
            .FirstOrDefaultAsync();
        if (partyId == 0) return NotFound();

        var result = await _applicationService.CreateApplication(partyId, ApplicationTypes.TradeAccount, supplement);
        if (result.Id == 0)
            return BadRequest(result);

        await _mediator.Publish(new ApplicationCreatedEvent(result));
        return Ok(result);
    }
}