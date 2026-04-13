
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Application")]
[Area("Client")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
[Route("api/" + VersionTypes.V2 + "/[Area]/application")]
public class ApplicationControllerV2(ApplicationService applicationService, IMediator mediator)
    : ClientBaseControllerV2
{
    [HttpPost("trade-account")]
    public async Task<ActionResult> Create([FromBody] Account.TradeAccountCreateSpec spec)
    {
        var supplement = spec.ToApplicationSupplement();
        var result =
            await applicationService.CreateApplication(GetPartyId(), ApplicationTypes.TradeAccount, supplement);
        
        if (result.Id == 0)
        {
            // If Id is 0, validation failed. RejectedReason contains the error message
            var errorMessage = string.IsNullOrWhiteSpace(result.RejectedReason) 
                ? "Failed to create application" 
                : result.RejectedReason;
            return BadRequest(Result.Error(errorMessage));
        }
        
        await mediator.Publish(new ApplicationCreatedEvent(result));
        return Ok();
    }
}