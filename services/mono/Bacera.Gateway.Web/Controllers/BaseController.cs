using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Services.Extension;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;

namespace Bacera.Gateway.Web.Controllers;

[ApiController]
[Route("api/" + VersionTypes.V1 + "/[controller]")]
[Produces("application/json")]
public abstract class BaseController : Controller
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
    // {
    // var partyId = GetUserClaim(UserClaimTypes.PartyId);
    // if (long.TryParse(partyId, out _)) throw new TokenInvalidException("TOKEN_INVALID");
    // return Party.HashDecode(partyId);
    // }

    protected long GetTenantId() => User.GetTenantId();

    /// <summary>
    /// Gets the current user's email from JWT claims (no database query needed)
    /// </summary>
    protected string GetUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    /// <summary>
    /// Gets the current user's language from JWT claims, defaults to English
    /// </summary>
    protected string GetUserLanguage() => User.FindFirst("Language")?.Value ?? LanguageTypes.English;

    protected bool ShouldHideEmail() => false;

    protected bool FromMobile() =>
        Request.HttpContext.GetUserAgent().Contains("dart", StringComparison.CurrentCultureIgnoreCase);


    // => ParseId(GetUserClaim(UserClaimTypes.TenantId));

    // private string GetUserClaim(string claimType)
    // => User.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value ?? string.Empty;

    // private static long ParseId(string idString)
    // => string.IsNullOrEmpty(idString) ? -1 : (long.TryParse(idString, out var id) ? id : -1);
}