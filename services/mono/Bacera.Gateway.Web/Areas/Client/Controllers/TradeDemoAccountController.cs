
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Bacera.Gateway.Web.EventHandlers;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using MSG = ResultMessage.Account;

[Area("Client")]
[Route("api/" + VersionTypes.V1 + "/[Area]/trade-demo-account")]
[Tags("Client/Trade Demo Account")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class TradeDemoAccountController(TradingService tradingService, TenantDbContext tenantCtx, IMediator mediator)
    : ClientBaseController
{
    private const int MaxDemoAccount = 5;

    /// <summary>
    /// Trade Demo Account Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    /// 
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TradeDemoAccount.ClientResponseModel>, TradeDemoAccount.Criteria>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<TradeDemoAccount.ClientResponseModel>, TradeDemoAccount.Criteria>>> DemoAccount(
        [FromQuery] TradeDemoAccount.Criteria? criteria)
    {
        criteria ??= new TradeDemoAccount.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await tradingService.TradeDemoAccountForClientQueryAsync(criteria);
        return Ok(items);
    }

    /// <summary>
    /// Create Trade Demo Account
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(TradeDemoAccount.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TradeDemoAccount>> DemoAccountCreate([FromBody] TradeDemoAccount.CreateSpec spec)
    {
        if (spec.Platform != PlatformTypes.MetaTrader4Demo && spec.Platform != PlatformTypes.MetaTrader5Demo)
            return BadRequest(ToErrorResult(MSG.ServiceNotFound));

        spec.Amount = Math.Max(100, spec.Amount);
        spec.Amount = Math.Min(50000000, spec.Amount);
        var partyId = GetPartyId();
        var user = await tenantCtx.Parties
            .Where(x => x.Id == partyId)
            .ToTenantDetailModel()
            .SingleAsync();

        var service = await tradingService.GetServiceByPlatformAsync(spec.Platform);

        if (service == null)
            return NotFound(ToErrorResult(MSG.ServiceNotFound));

        if (await tradingService.TradeDemoAccountCountAsync(GetPartyId(), service.Id) >= MaxDemoAccount)
            return BadRequest(Result.Error($"__YOU_CAN_ONLY_HAVE_{MaxDemoAccount}_DEMO_ACCOUNT__"));

        var defaultGroup = tradingService.GetDemoTradeAccountDefaultGroup(service, spec.AccountType, spec.CurrencyId);
        if (string.IsNullOrEmpty(defaultGroup))
            return BadRequest(ToErrorResult(MSG.AccountCreateFailed));

        var password = Utils.GenerateSimplePassword();

        TradeDemoAccount item;
        try
        {
            item = await tradingService.TradeDemoAccountCreateAsync(GetPartyId(), service.Id, spec.AccountType,
                $"{user.FirstName} {user.LastName}", user.EmailRaw, password, spec.Amount, spec.CurrencyId,
                spec.Leverage, defaultGroup);
        }
        catch (Exception)
        {
            return BadRequest(Result.Error(MSG.TradeServerError));
        }

        await mediator.Publish(new DemoAccountCreatedEvent(item.PartyId, password, $"{user.FirstName} {user.LastName}",
            user.EmailRaw, user.PhoneNumberRaw, item.AccountNumber, service.Name));

        return item.IsEmpty()
            ? BadRequest(ToErrorResult(MSG.AccountCreateFailed))
            : Ok(TradeDemoAccount.ClientResponseModel.Build(item));
    }
}