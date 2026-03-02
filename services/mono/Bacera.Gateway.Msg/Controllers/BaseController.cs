using System.Text;
using Bacera.Gateway.Services.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Msg.Controllers;

[ApiController]
[Route("gateway-msg/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseController : Controller
{
    /// <summary>
    /// Gets the IP address of the remote client.
    /// </summary>
    /// <param name="allowForwarded" example="true">Whether to allow using forwarded IP addresses.</param>
    /// <returns>A string representing the IP address of the remote client, or an empty string if it cannot be obtained.</returns>
    protected string GetRemoteIpAddress(bool allowForwarded = true)
    {
        if (!allowForwarded) return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var forwardedHeader = HttpContext.Request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            var ips = forwardedHeader.ToString().Split(',');
            if (ips.Length > 0)
            {
                return ips[0];
            }
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    protected Dictionary<string, string> GetIps()
    {
        var remoteIpAddresses = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "NONE";
        var xForwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "NONE";
        var xRealIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault() ?? "NONE";
        var ips = new Dictionary<string, string>
        {
            { "RemoteIpAddress", remoteIpAddresses },
            { "X-Forwarded-For", xForwardedFor },
            { "X-Real-IP", xRealIp }
        };
        return ips;
    }

    protected async Task<string> GetRequestBody()
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var json = await reader.ReadToEndAsync();
        Request.Body.Position = 0;
        return json;
    }

    protected long GetPartyId() => User.GetPartyId();

    protected long GetTenantId() => User.GetTenantId();

    protected bool FromMobile() =>
        Request.HttpContext.GetUserAgent().Contains("dart", StringComparison.CurrentCultureIgnoreCase);
}