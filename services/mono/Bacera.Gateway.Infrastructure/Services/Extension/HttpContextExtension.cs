using System.Net;
using System.Security.Claims;
using Bacera.Gateway.MyException;
using Microsoft.AspNetCore.Http;

namespace Bacera.Gateway.Services.Extension;

public static class HttpContextExtension
{
    private static string? GetUserClaim(this ClaimsPrincipal user, string claimType)
        => user.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value;

    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.IsInRole(UserRoleTypesString.SuperAdmin);
    public static bool IsTenantAdmin(this ClaimsPrincipal user) => user.IsInRole(UserRoleTypesString.TenantAdmin);
    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(UserRoleTypesString.Admin);

    public static long GetPartyId(this ClaimsPrincipal user)
    {
        var partyId = user.GetUserClaim(UserClaimTypes.PartyId);
        if (partyId == null) return -1;
        return Party.HashDecode(partyId);
    }

    public static long GetGodPartyId(this ClaimsPrincipal user)
    {
        var partyId = user.GetUserClaim(UserClaimTypes.GodPartyId);
        if (partyId == null) return -1;
        return Party.HashDecode(partyId);
    }

    public static string GetTokenUserAgent(this ClaimsPrincipal user)
    {
        var userAgent = user.GetUserClaim(UserClaimTypes.UserAgent);
        if (userAgent == null) throw new TokenInvalidException("Token invalid");
        return userAgent;
    }

    public static long GetTenantId(this ClaimsPrincipal user)
    {
        var tenantId = user.GetUserClaim(UserClaimTypes.TenantId);
        if (tenantId == null) return -1;
        return long.Parse(tenantId);
    }

    public static List<long> GetAccountUidsInClaim(this ClaimsPrincipal user, string userClaimType)
        => user.Claims.Where(x => x.Type == userClaimType)
            .Select(x => long.TryParse(x.Value, out var id) ? id : -1)
            .Where(x => x > 0)
            .ToList();

    public static string GetRemoteIpAddress(this IHttpContextAccessor httpContextAccessor, bool allowForwarded = true)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return string.Empty;
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

    public static string GetUserAgent(this HttpContext ctx)
    {
        var userAgent = ctx.Request.Headers.TryGetValue("User-Agent", out var userAgentString)
            ? userAgentString.FirstOrDefault()
            : string.Empty;

        return userAgent ?? string.Empty;
    }
}