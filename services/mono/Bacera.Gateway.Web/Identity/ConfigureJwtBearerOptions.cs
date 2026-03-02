using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;

namespace Bacera.Gateway.Web;

/// <summary>
/// Post-configures the secondary JwtBearer scheme used for SignalR hub query-string tokens.
/// Runs after the DI container is built, so OpenIddictServerOptions is safely resolvable.
///
/// Only IssuerSigningKeyResolver is wired here — it pulls signing keys directly from the
/// in-process OpenIddict server at validation time, avoiding any file I/O or OIDC
/// discovery-document fetch. JwtBearer 8.x uses JsonWebTokenHandler by default, which
/// shares the same Microsoft.IdentityModel 8.x assembly as OpenIddict 7.x, so the keys
/// are type-compatible and signature verification works correctly.
/// </summary>
public class ConfigureJwtBearerOptions(
    IOptionsMonitor<OpenIddictServerOptions> oidcServerOptions)
    : IPostConfigureOptions<JwtBearerOptions>
{
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme) return;

        // Resolve signing keys from the live OpenIddict server options at validation time.
        // This works for both PFX-backed certificates and the development ephemeral key.
        options.TokenValidationParameters.IssuerSigningKeyResolver =
            (_, _, _, _) => oidcServerOptions.CurrentValue.SigningCredentials.Select(c => c.Key);
    }
}
