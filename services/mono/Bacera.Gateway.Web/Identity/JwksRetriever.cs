using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// Fetches a raw JWKS JSON document and wraps it into an OpenIdConnectConfiguration
/// so it can be used with ConfigurationManager for automatic key refresh.
/// Used when the auth server exposes only /.well-known/jwks.json (no OIDC discovery).
/// </summary>
public class JwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address,
        IDocumentRetriever retriever,
        CancellationToken cancel)
    {
        var json = await retriever.GetDocumentAsync(address, cancel);
        var jwks = new JsonWebKeySet(json);
        return new OpenIdConnectConfiguration { JsonWebKeySet = jwks };
    }
}
