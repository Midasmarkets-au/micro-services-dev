using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Bacera.Gateway.Msg.MyOptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bacera.Gateway.Msg.Identity;

public class WsSchemeHandler(IOptionsMonitor<WsSchemeOption> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<WsSchemeOption>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.Delay(0);
        var path = Context.Request.Path.ToString();
        if (Context.Request.Method == "OPTIONS" || path.Contains("symbol-group-hub"))
        {
            var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "anonymous")], Scheme.Name);
            return AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name));
        }

        var token = Context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(token)) token = Context.Request.Query["access_token"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("Missing token");
        }


        var claimsPrincipal = VerifyTokenAndGetPrincipal(token, Options.PfxPath, Options.Password);
        if (claimsPrincipal == null)
        {
            return AuthenticateResult.Fail("Invalid token");
        }

        // 使用从令牌中提取的Claims创建认证票据
        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    public static ClaimsPrincipal? VerifyTokenAndGetPrincipal(string tokenString, string pfxPath, string pwd)
    {
        var data = File.ReadAllBytes(pfxPath);
        var cert = new X509Certificate2(data, pwd, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
        var publicKey = cert.GetRSAPublicKey();
        if (publicKey == null) throw new Exception("Failed to extract RSA public key");

        if (tokenString.StartsWith("Bearer "))
            tokenString = tokenString[7..];

        var handler = new JwtSecurityTokenHandler();

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(publicKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal =
                handler.ValidateToken(tokenString, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.StartsWith("RS"))
            {
                throw new Exception($"Unexpected signing method: {(validatedToken as JwtSecurityToken)?.Header.Alg}");
            }

            return principal;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse JWT: {ex.Message}");
            return null;
        }
    }
}