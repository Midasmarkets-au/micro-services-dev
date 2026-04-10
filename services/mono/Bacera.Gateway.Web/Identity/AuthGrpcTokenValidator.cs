using System.Security.Claims;
using Api.V1;
using Bacera.Gateway.Services.Extension;
using OpenIddict.Validation;
using static OpenIddict.Validation.OpenIddictValidationEvents;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// Replaces OpenIddict's local JWT validation with a gRPC call to the Rust auth service.
/// Registered as an event handler for <see cref="ValidateTokenContext"/>.
///
/// When AUTH_GRPC_ADDR is set and the Rust auth service is reachable, this handler
/// validates the token remotely and builds the ClaimsPrincipal from the gRPC response.
/// If the gRPC call fails (service unavailable), it falls through to OpenIddict's
/// built-in local validation as a fallback — ensuring zero downtime during migration.
/// </summary>
public class AuthGrpcTokenValidator(
    AuthValidationService.AuthValidationServiceClient authClient,
    ILogger<AuthGrpcTokenValidator> logger)
    : IOpenIddictValidationHandler<ValidateTokenContext>
{
    public static OpenIddictValidationHandlerDescriptor Descriptor { get; }
        = OpenIddictValidationHandlerDescriptor.CreateBuilder<ValidateTokenContext>()
            .UseSingletonHandler<AuthGrpcTokenValidator>()
            // Run before OpenIddict's built-in ValidateIdentityModelToken (order 500)
            .SetOrder(200)
            .Build();

    public async ValueTask HandleAsync(ValidateTokenContext context)
    {
        var token = context.Token;
        if (string.IsNullOrEmpty(token))
            return;

        ValidateTokenResponse resp;
        try
        {
            resp = await authClient.ValidateTokenAsync(
                new ValidateTokenRequest { Token = token },
                deadline: DateTime.UtcNow.AddSeconds(3));
        }
        catch (Exception ex)
        {
            // gRPC unavailable — fall through to local OpenIddict validation
            logger.LogWarning("AuthGrpcTokenValidator: gRPC call failed ({Msg}), falling back to local validation", ex.Message);
            return;
        }

        if (!resp.Valid)
        {
            logger.LogDebug("AuthGrpcTokenValidator: token rejected by auth service: {Error}", resp.Error);
            context.Reject(
                error: OpenIddict.Abstractions.OpenIddictConstants.Errors.InvalidToken,
                description: resp.Error);
            return;
        }

        // Build ClaimsPrincipal from gRPC response claims
        var identity = new ClaimsIdentity(
            authenticationType: "AuthGrpc",
            nameType: OpenIddict.Abstractions.OpenIddictConstants.Claims.Name,
            roleType: OpenIddict.Abstractions.OpenIddictConstants.Claims.Role);

        identity.AddClaim(new Claim(OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject, resp.UserId.ToString()));
        identity.AddClaim(new Claim(UserClaimTypes.TenantId, resp.TenantId.ToString()));
        identity.AddClaim(new Claim(UserClaimTypes.PartyId, resp.PartyId));

        foreach (var role in resp.Roles)
            identity.AddClaim(new Claim(OpenIddict.Abstractions.OpenIddictConstants.Claims.Role, role));

        context.Principal = new ClaimsPrincipal(identity);
    }
}
