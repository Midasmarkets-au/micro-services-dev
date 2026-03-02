using System.Security.Claims;
using Bacera.Gateway.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Web;

public class AdditionalUserClaimsPrincipalFactory :
    UserClaimsPrincipalFactory<User, ApplicationRole>
{
    public AdditionalUserClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        var claims = new List<Claim> { user.TwoFactorEnabled ? new Claim("amr", "mfa") : new Claim("amr", "pwd") };

        identity.AddClaims(claims);
        return principal;
    }
}