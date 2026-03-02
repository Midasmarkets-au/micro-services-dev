using Microsoft.AspNetCore.Authorization;
using Bacera.Gateway.Web.Response;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Context;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Controllers.V2;

[Tags("Contact")]
[Route("api/" + VersionTypes.V2 + "/contact")]
public class ContactControllerV2(MyDbContextPool myDbContextPool, IHttpClientFactory clientFactory) : BaseControllerV2
{
    private const int Timeout = 10;
    private const string Endpoint = "https://ipinfo.io/";
    private readonly string _token = Environment.GetEnvironmentVariable("IPINFO_TOKEN") ??
                                     throw new AuthenticationException("IPINFO_TOKEN is not set");
    
    /// <summary>
    /// Post Inquiry
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create([FromBody] ContactRequest.CreateSpec spec)
    {
        var ipInfo = await GetIpInfo();
        var tenantIdFromIp = Tenancy.GetTenantIdByCountryCode(ipInfo.Country);
        if (tenantIdFromIp == 0) tenantIdFromIp = 10000;
        var ctx = await myDbContextPool.BorrowTenant(tenantIdFromIp);
        try
        {
            var item = new ContactRequest
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
        client.Timeout = TimeSpan.FromSeconds(Timeout);
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