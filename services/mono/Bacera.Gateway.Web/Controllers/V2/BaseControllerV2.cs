using Bacera.Gateway.Services.Extension;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Controllers.V2;

[ApiController]
[Route("api/" + VersionTypes.V2 + "/[controller]")]
[Produces("application/json")]
public abstract class BaseControllerV2 : Controller
{
    protected static Result ToErrorResult(string message, IEnumerable<string>? errors = null)
    {
        return errors == null
            ? Result.Error(message)
            : Result.Error(message, errors);
    }

    protected static Result ToSuccessResult(string message)
        => Result.Success(message);

    /// <summary>
    /// Gets the IP address of the remote client.
    /// </summary>
    /// <param name="allowForwarded" example="true">Whether to allow using forwarded IP addresses.</param>
    /// <returns>A string representing the IP address of the remote client, or an empty string if it cannot be obtained.</returns>
    protected string GetRemoteIpAddress(bool allowForwarded = true)
    {
        if (!allowForwarded) return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;


        // 尝试从 X-Forwarded-For 头部获取 IP 地址
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

    // get end point 

    protected string GetReferer()
    {
        var path = HttpContext.Request.Headers.TryGetValue("Referer", out var refererString)
            ? refererString.FirstOrDefault() ?? string.Empty
            : string.Empty;
        if (path.EndsWith('/')) path = path[..^1];
        return path;
    }

    protected string GetBaseUrl()
    {
        var referer = HttpContext.Request.Headers.TryGetValue("Referer", out var refererString)
            ? refererString.FirstOrDefault()
            : string.Empty;

        return referer!.Replace("portal/", "");
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

    protected long GetPartyId() => User.GetPartyId();
    // {
    // var partyId = GetUserClaim(UserClaimTypes.PartyId);
    // if (long.TryParse(partyId, out _)) throw new TokenInvalidException("TOKEN_INVALID");
    // return Party.HashDecode(partyId);
    // }

    protected long GetTenantId() => User.GetTenantId();
    // => ParseId(GetUserClaim(UserClaimTypes.TenantId));
    // private string GetUserClaim(string claimType)
    // => User.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value ?? string.Empty;

    // private static long ParseId(string idString)
    // => string.IsNullOrEmpty(idString) ? -1 : (long.TryParse(idString, out var id) ? id : -1);
}