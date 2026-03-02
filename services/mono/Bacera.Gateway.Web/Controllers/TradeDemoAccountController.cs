using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.IPInfo;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Response;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Controllers;

using MSG = ResultMessage.Account;

[Tags("Public/Trade Demo Account")]
[Route("api/" + VersionTypes.V1 + "/trade-demo-account")]
public class TradeDemoAccountController(
    // TradingService tradingService,
    // TenantDbContext tenantDbContext,
    IDistributedCache cacheSvc,
    IHttpClientFactory clientFactory,
    // IMediator mediator
    IServiceProvider provider,
    IOptions<IPInfoOptions> ipInfoOptions
)
    : BaseController
{
    private const long DefaultPartyIdForDemoAccount = 1; // for system

    private const int RequestPerMin = 2;

    /// <summary>
    /// Create Trade Demo Account
    /// </summary>
    /// <example>
    ///  {
    ///  "leverage": 100,
    ///  "password": "Pass!1111",
    ///  "platform": 21,
    ///  "accountType": 1,
    ///  "currencyId": 840
    ///  }
    /// </example>
    /// <param name="spec"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(TradeDemoAccount.ClientResponseModel), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TradeDemoAccount>> DemoAccountCreate([FromBody] TradeDemoAccount.DemoCreateSpec spec)
    {
        // if (token.ToLower() != HashToken)
        //     return Forbid();
        var ip = GetRemoteIpAddress();
        if (_isReachedRequestLimit(ip))
            return new StatusCodeResult(StatusCodes.Status429TooManyRequests);

        using var scope = provider.CreateScope();
        var tenantSetter = scope.ServiceProvider.GetRequiredService<ITenantSetter>();

        var tenantId = spec.TenantId;
        if (tenantId == 0)
        {
            var ipInfo = await GetIpInfo();
            tenantId = Tenancy.GetTenantIdByCountryCode(ipInfo.Country, 10000);
        }

        if (tenantId == 0)
            return BadRequest(ToErrorResult("Invalid tenant"));

        tenantSetter.SetTenantId(tenantId);

        var tradingService = scope.ServiceProvider.GetRequiredService<TradingService>();

        if (spec.Platform != PlatformTypes.MetaTrader4Demo && spec.Platform != PlatformTypes.MetaTrader5Demo)
            return BadRequest(ToErrorResult(MSG.ServiceNotFound));

        var service = await tradingService.GetServiceByPlatformAsync(spec.Platform);

        if (service == null)
            return NotFound(ToErrorResult(MSG.ServiceNotFound));

        var defaultGroup = tradingService.GetDemoTradeAccountDefaultGroup(service, spec.AccountType, spec.CurrencyId);
        if (string.IsNullOrEmpty(defaultGroup))
            return BadRequest(ToErrorResult(MSG.AccountCreateFailed));

        // initial demo account
        spec.Leverage = 100;
        const double defaultBalance = 10000;
        var initialAmount = spec.Amount == 0 ? defaultBalance : spec.Amount;
        var password = Utils.GenerateSimplePassword();

        TradeDemoAccount item;
        try
        {
            item = await tradingService.TradeDemoAccountCreateAsync(DefaultPartyIdForDemoAccount, service.Id,
                spec.AccountType, !string.IsNullOrEmpty(spec.Name) ? spec.Name : "Demo Account", spec.Email,
                password, initialAmount, spec.CurrencyId, spec.Leverage, defaultGroup);
        }
        catch (Exception)
        {
            return BadRequest(Result.Error(MSG.TradeServerError));
        }

        var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var demoAccount = await tenantDbContext.TradeDemoAccounts.SingleAsync(x => x.Id == item.Id);
        demoAccount.Name = TrimAndLimitLength(spec.Name, 128);
        demoAccount.Email = TrimAndLimitLength(spec.Email, 128);
        demoAccount.PhoneNumber = TrimAndLimitLength(spec.PhoneNumber, 128);
        demoAccount.CountryCode = TrimAndLimitLength(spec.CountryCode, 32);
        demoAccount.ReferralCode = TrimAndLimitLength(spec.ReferralCode, 512);
        tenantDbContext.TradeDemoAccounts.Update(demoAccount);
        await tenantDbContext.SaveChangesAsync();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(new DemoAccountCreatedEvent(
            demoAccount.PartyId,
            password,
            demoAccount.Name,
            demoAccount.Email,
            demoAccount.PhoneNumber,
            demoAccount.AccountNumber,
            service.Name
        ));

        _updateRequestLimit(ip);
        return item.IsEmpty()
            ? BadRequest(ToErrorResult(MSG.AccountCreateFailed))
            : Ok(TradeDemoAccount.ClientResponseModel.Build(item));
    }

    private static string TrimAndLimitLength(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input; // Return the original input if it's null or empty
        }

        var trimmedName = input.Trim();
        return trimmedName.Length > maxLength ? trimmedName[..maxLength] : trimmedName;
    }

    private bool _isReachedRequestLimit(string limitKey)
    {
        var key = "request_demo_account:" + limitKey.Trim();
        List<Tuple<string, DateTime>> items;
        try
        {
            var cachedString = cacheSvc.GetString(key);
            if (!string.IsNullOrEmpty(cachedString))
            {
                items = JsonConvert.DeserializeObject<List<Tuple<string, DateTime>>>(cachedString) ??
                        new List<Tuple<string, DateTime>>();
            }
            else items = new List<Tuple<string, DateTime>>();
        }
        catch
        {
            items = new List<Tuple<string, DateTime>>();
        }

        var current = DateTime.UtcNow;
        var offset = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
        items.RemoveAll(x => x.Item2 < offset);
        return items.Count(x => x.Item1 == limitKey) >= RequestPerMin;
    }

    private void _updateRequestLimit(string limitKey)
    {
        var key = "request_demo_account:" + limitKey.Trim();
        List<Tuple<string, DateTime>> items;
        try
        {
            var cachedString = cacheSvc.GetString(key);
            if (!string.IsNullOrEmpty(cachedString))
            {
                items = JsonConvert.DeserializeObject<List<Tuple<string, DateTime>>>(cachedString) ??
                        new List<Tuple<string, DateTime>>();
            }
            else items = new List<Tuple<string, DateTime>>();
        }
        catch
        {
            items = new List<Tuple<string, DateTime>>();
        }

        items.Add(Tuple.Create(limitKey, DateTime.UtcNow));
        cacheSvc.SetString(key, JsonConvert.SerializeObject(items));
    }

    private async Task<IpInfoViewModel> GetIpInfo(string? ip = null)
    {
        ip ??= GetRemoteIpAddress();
        string endpoint = ipInfoOptions.Value.Endpoint, token = ipInfoOptions.Value.Token;
        endpoint = endpoint.EndsWith('/') ? endpoint : $"{endpoint}/";
        try
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(endpoint);
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync($"{endpoint}{ip}?token={token}");
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IpInfoViewModel>(data) ?? new IpInfoViewModel();
            result.Ips = GetIps();
            return result;
        }
        catch
        {
            return new IpInfoViewModel();
        }
    }
}