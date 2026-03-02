using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Trade Account")]
[Area("Client")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
[Route("api/" + VersionTypes.V2 + "/[Area]/trade-account")]
public class TradeAccountControllerV2(TradingService tradingSvc): ClientBaseControllerV2
{
    /// <summary>
    /// TradeAccount Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TradeAccount.ClientResponseModel>, TradeAccount.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<TradeAccount.ClientResponseModel>>>> Index([FromQuery] TradeAccount.Criteria? criteria)
    {
        criteria ??= new TradeAccount.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await tradingSvc.TradeAccountForClientQueryAsync(criteria);
        return Ok(items);
    }
}