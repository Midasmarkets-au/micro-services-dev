using System.Net;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.IPInfo;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Middlewares;

public class MultiTenantServiceMiddleware(
    ITenantSetter setter,
    CentralDbContext centralDb,
    // ITenantService tenantService,
    // IMyCache cache,
    IHttpClientFactory clientFactory,
    IOptions<IPInfoOptions> ipInfoOptions)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = context.User.GetTenantId();
        // var partyId = context.User.GetPartyId();
        // var tenant = await tenantService.GetAsync(tenantId);
        if (tenantId > 0)
        {
            setter.SetTenantId(tenantId);
            await next(context);
            return;
        }

        // Unauthenticated request — resolve tenant from Host header via central Domain table
        var host = context.Request.Host.Host; // strips port
        if (!string.IsNullOrEmpty(host))
        {
            var domain = await centralDb.Domains
                .Where(d => d.DomainName == host)
                .Select(d => new { d.TenantId })
                .FirstOrDefaultAsync();
            if (domain?.TenantId > 0)
            {
                setter.SetTenantId(domain.TenantId);
                await next(context);
                return;
            }
        }

        // /api/v1/payment/[callback]/:tenantId/
        var requestPath = context.Request.Path.Value ?? "/none";
        var paths = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? controller = null;
        if (paths.Length >= 3)
        {
            controller = paths[2];
        }

        switch (controller)
        {
            case "contact":
            case "lead":
                // case "trade-demo-account":
            {
                var ipInfo = await GetIpInfo(context);
                tenantId = Tenancy.GetTenantIdByCountryCode(ipInfo.Country, 10000);
                // setter.SetTenant(await tenantService.GetAsync(tenantId));
                setter.SetTenantId(tenantId);
                await next(context);
                return;
            }
        }

        await next(context);

    }

    private async Task<IpInfoViewModel> GetIpInfo(HttpContext context)
    {
        var ip = GetRemoteIpAddress(context);
        var endPoint = ipInfoOptions.Value.Endpoint;
        endPoint = endPoint.EndsWith('/') ? endPoint : $"{endPoint}/";
        var client = clientFactory.CreateClient();
        // var client = new HttpClient { BaseAddress = new Uri(endPoint), Timeout = TimeSpan.FromSeconds(5) };
        var response = await client.GetAsync($"{endPoint}{ip}?token={ipInfoOptions.Value.Token}");
        var data = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<IpInfoViewModel>(data) ?? new IpInfoViewModel();
        }
        catch
        {
            return new IpInfoViewModel();
        }
    }

    private static string GetRemoteIpAddress(HttpContext context, bool allowForwarded = true)
    {
        if (!allowForwarded) return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        var forwardedHeader = context.Request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            var ips = forwardedHeader.ToString().Split(',');
            if (ips.Length > 0 && IPAddress.TryParse(ips[0], out _))
            {
                return ips[0];
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}