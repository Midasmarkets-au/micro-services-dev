using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Bacera.Gateway.Auth.Security;

public record TokenResult(string AccessToken, long ExpiresIn);

/// <summary>
/// Mirrors token.rs — generates JWT access tokens with the same claims structure as mono's IdentityServer4.
/// </summary>
public static class TokenService
{
    private const string Version = "08-23-24";

    /// <summary>
    /// TenantAdmin tokens get a 30-day lifetime; all others use the configured lifetime.
    /// </summary>
    public static TokenResult GenerateAccessToken(
        long userId,
        long tenantId,
        string partyIdHashed,
        IReadOnlyList<string> roles,
        bool twoFactorEnabled,
        long lifetimeSecs,
        string jwtSecret)
    {
        var now = DateTimeOffset.UtcNow;
        var isTenantAdmin = roles.Contains("TenantAdmin");
        var effectiveLifetime = isTenantAdmin ? 2_592_000L : lifetimeSecs;

        var role = roles.FirstOrDefault();
        var amr = twoFactorEnabled ? "mfa" : "pwd";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("TenantId", tenantId.ToString()),
            new("PartyId", partyIdHashed),
            new("Version", Version),
            new("amr", amr),
        };

        if (role != null)
            claims.Add(new Claim("role", role));

        var key = BuildSigningKey(jwtSecret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.UtcDateTime.AddSeconds(effectiveLifetime),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenResult(accessToken, effectiveLifetime);
    }

    public static TokenResult GenerateClientCredentialsToken(
        string clientId,
        long lifetimeSecs,
        string jwtSecret)
    {
        var now = DateTimeOffset.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("TenantId", "0"),
            new("PartyId", "0"),
            new("Version", Version),
        };

        var key = BuildSigningKey(jwtSecret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.UtcDateTime.AddSeconds(lifetimeSecs),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenResult(accessToken, lifetimeSecs);
    }

    public static string GenerateRefreshToken() => Guid.NewGuid().ToString();

    /// HS256 requires at least 32 bytes (256 bits). Pad short keys with zeros.
    private static SymmetricSecurityKey BuildSigningKey(string secret)
    {
        var bytes = Encoding.UTF8.GetBytes(secret);
        if (bytes.Length < 32)
        {
            var padded = new byte[32];
            bytes.CopyTo(padded, 0);
            bytes = padded;
        }
        return new SymmetricSecurityKey(bytes);
    }
}
