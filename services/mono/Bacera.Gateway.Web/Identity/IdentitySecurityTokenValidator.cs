using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bacera.Gateway.Auth;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Bacera.Gateway.Web;

public class IdentitySecurityTokenValidator(

    // IMyCache cache
) : ISecurityTokenValidator
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public bool CanReadToken(string securityToken) => _tokenHandler.CanReadToken(securityToken);

    public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
    {
        try
        {
            var payload = _tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);
            return payload;
            // var (tenantId, partyId) = (payload.GetTenantId(), payload.GetPartyId());
            // var key = CacheKeys.UserTokenInvalidKey(tenantId, partyId);
            // var value = cache.GetStringAsync(key).Result;
            // if (value == null) return payload;
            //
            // Task.Run(() => cache.KeyDeleteAsync(key));
            // throw new TokenInvalidException("Token invalid");
            // var requestUserAgent = httpContextAccessor.GetUserAgent();
            // var tokenUserAgent = payload.GetTokenUserAgent();
            // if (Utils.Md5Hash(requestUserAgent + ".thebcr.com") != tokenUserAgent)
            // {
            //     throw new TokenInvalidException("Token invalid");
            // }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    public bool CanValidateToken => true;
    public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;
}