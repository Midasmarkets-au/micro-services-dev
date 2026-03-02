using Bacera.Gateway.Context;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Response;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Controllers;

using M = ContactRequest;

[Tags("Public/Contact")]
public class ContactController(MyDbContextPool myDbContextPool, IHttpClientFactory clientFactory) : BaseController
{
    private const int Timeout = 10;
    private const string Endpoint = "https://ipinfo.io/";

    // private readonly HttpClient _client = CreateClient();

    private readonly string _token = Environment.GetEnvironmentVariable("IPINFO_TOKEN") ??
                                     throw new AuthenticationException("IPINFO_TOKEN is not set");

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var ipInfo = await GetIpInfo();
        var tenantIdFromIp = Tenancy.GetTenantIdByCountryCode(ipInfo.Country);
        if (tenantIdFromIp == 0) tenantIdFromIp = 10000;
        var ctx = await myDbContextPool.BorrowTenant(tenantIdFromIp);
        try
        {
            var item = new M
            {
                Content = spec.Content,
                Email = spec.Email,
                Name = spec.Name,
                PhoneNumber = spec.PhoneNumber,
                CreatedOn = DateTime.UtcNow,
                Ip = GetRemoteIpAddress(),
                PartyId = GetPartyId()
            };
            await ctx.ContactRequests.AddAsync(item);
            await ctx.SaveChangesAsync();
            return NoContent();
        }
        finally
        {
            myDbContextPool.ReturnTenant(ctx);
        }
    }

    private async Task<IpInfoViewModel> GetIpInfo()
    {
        var ip = GetRemoteIpAddress();
        var client = clientFactory.CreateClient();

        var response = await client.GetAsync($"{Endpoint}{ip}?token={_token}");
        var data = await response.Content.ReadAsStringAsync();
        try
        {
            var result = JsonConvert.DeserializeObject<IpInfoViewModel>(data) ?? new IpInfoViewModel();
            result.Ips = GetIps();
            return result;
        }
        catch
        {
            return new IpInfoViewModel();
        }
    }
    //
    // private static HttpClient CreateClient()
    // {
    //     var handler = new HttpClientHandler();
    //     handler.ClientCertificateOptions = ClientCertificateOption.Manual;
    //     handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
    //
    //     var client = new HttpClient(handler)
    //     {
    //         Timeout = TimeSpan.FromSeconds(Timeout),
    //         DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
    //         BaseAddress = new Uri(Endpoint.EndsWith("/") ? Endpoint : Endpoint + "/"),
    //     };
    //     return client;
    // }
}