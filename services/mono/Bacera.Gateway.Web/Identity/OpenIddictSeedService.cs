using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// Ensures required OpenIddict applications and scopes exist in the database on startup.
/// Replaces the IS4 IdentityConfiguration / seed data that was previously in ConfigurationDbContext.
///
/// Idempotent: safe to run on every startup.
/// </summary>
public class OpenIddictSeedService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var appManager   = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // ── Scopes ────────────────────────────────────────────────────────────
        await EnsureScopeAsync(scopeManager, "api", "BCR API", cancellationToken);

        // ── Applications ──────────────────────────────────────────────────────

        // Primary client used by:
        //   - POST /connect/token (password grant, browser / mobile)
        //   - BcrTokenService.GetUserTokenAsync (email-code login, impersonation)
        //   - WsMessageProcessor (internal service-to-service password grant)
        await EnsureApplicationAsync(appManager, new OpenIddictApplicationDescriptor
        {
            ClientId   = "api",
            ClientType = ClientTypes.Public,
            DisplayName = "BCR API Client",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Revocation,
                Permissions.Endpoints.Introspection,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "api",
                Permissions.Prefixes.Scope + "openid",
                Permissions.Prefixes.Scope + "offline_access",
            },
        }, cancellationToken);

        // Mobile client (same permissions, separate client_id for analytics)
        await EnsureApplicationAsync(appManager, new OpenIddictApplicationDescriptor
        {
            ClientId   = "mobile",
            ClientType = ClientTypes.Public,
            DisplayName = "BCR Mobile Client",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.Prefixes.Scope + "api",
                Permissions.Prefixes.Scope + "openid",
                Permissions.Prefixes.Scope + "offline_access",
            },
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // ─────────────────────────────────────────────────────────────────────────

    private static async Task EnsureApplicationAsync(
        IOpenIddictApplicationManager manager,
        OpenIddictApplicationDescriptor descriptor,
        CancellationToken ct)
    {
        var existing = await manager.FindByClientIdAsync(descriptor.ClientId!, ct);
        if (existing is null)
        {
            await manager.CreateAsync(descriptor, ct);
            return;
        }

        await manager.UpdateAsync(existing, descriptor, ct);
    }

    private static async Task EnsureScopeAsync(
        IOpenIddictScopeManager manager,
        string name,
        string displayName,
        CancellationToken ct)
    {
        if (await manager.FindByNameAsync(name, ct) is not null)
            return;

        await manager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name        = name,
            DisplayName = displayName,
        }, ct);
    }
}
