using System.Security.Cryptography.X509Certificates;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Web.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupIdentityServer(this WebApplicationBuilder me)
    {
        me.Services
            .AddScoped<PasswordGrantHandler>()
            .AddScoped<RefreshTokenGrantHandler>()
            .AddSingleton<ApplyTokenResponseHandler>()
            .AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>()
            .AddHostedService<OpenIddictSeedService>();

        me.Services.AddOpenIddict()

            // ── Core: EF Core store backed by AuthDbContext ──────────────────
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AuthDbContext>();
            })

            // ── Server: token endpoint + password/refresh flows ──────────────
            .AddServer(options =>
            {
                // Token endpoint: POST /connect/token  (mirrors old IS4 path)
                options.SetTokenEndpointUris("/connect/token");

                // Grant types
                options.AllowPasswordFlow()
                       .AllowRefreshTokenFlow()
                       .AllowClientCredentialsFlow();

                // Scopes accepted by this server
                options.RegisterScopes("openid", "offline_access", "api");

                // password grant handler (replaces IdentityPasswordValidator)
                options.AddEventHandler<HandleTokenRequestContext>(
                    builder => builder
                        .UseScopedHandler<PasswordGrantHandler>()
                        .SetOrder(500)
                );

                // refresh_token grant handler
                options.AddEventHandler<HandleTokenRequestContext>(
                    builder => builder
                        .UseScopedHandler<RefreshTokenGrantHandler>()
                        .SetOrder(501)
                );

                // Cookie + custom response writer — must run before ProcessJsonResponse (500_000)
                options.AddEventHandler<ApplyTokenResponseContext>(
                    builder => builder
                        .UseSingletonHandler<ApplyTokenResponseHandler>()
                        .SetOrder(499_000)
                );

                // Signing credentials
                if (GetEnvValue("PFX", "EMPTY") == "EMPTY")
                {
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                }
                else
                {
                    var cert = new X509Certificate2(
                        Path.Combine(GetEnvValue("PFX")), "bacera");
                    options.AddSigningCertificate(cert)
                           .AddEncryptionCertificate(cert);
                }

                // Disable access token encryption — downstream services validate
                // with the public key only (same behaviour as IS4)
                options.DisableAccessTokenEncryption();

                // Default token lifetimes (overridden per-request in PasswordGrantHandler)
                options.SetAccessTokenLifetime(TimeSpan.FromHours(24));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

                options.UseAspNetCore()
                       .DisableTransportSecurityRequirement();
            })

            // ── Validation: validate tokens issued by this server ────────────
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }

    public static void SetupIdentity(this WebApplicationBuilder me)
    {
        me.Services.AddIdentity<User, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                // OpenIddict puts the user ID in the "sub" claim; tell Identity to look there
                // so that UserManager.GetUserAsync(User) can resolve the user correctly.
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
    }

    public static void SetupAuthentication(this WebApplicationBuilder me)
    {
        // Primary scheme: OpenIddict validation (replaces JwtBearer for API token validation)
        me.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            })
            // Secondary JwtBearer scheme: used only for SignalR hub query-string tokens.
            // Authority is intentionally NOT set — it would trigger an OIDC discovery-document
            // fetch that crashes due to a Microsoft.IdentityModel version conflict between
            // JwtBearer 7.x (ships IdentityModel 6.x) and OpenIddict 7.x (pulls IdentityModel 8.x).
            // Instead, IssuerSigningKeyResolver pulls the signing keys at request time directly
            // from the in-process OpenIddictServerOptions, which always has the correct keys
            // regardless of whether a PFX or the development ephemeral certificate is in use.
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience         = false,
                    ValidateIssuer           = false,
                    ValidateIssuerSigningKey  = true,
                    // Accept both "at+jwt" (OpenIddict) and "JWT" (legacy IS4) so that existing
                    // user sessions remain valid during the IS4 → OpenIddict migration window.
                    ValidTypes               = new[] { "at+jwt", "JWT" },
                    // IssuerSigningKeyResolver is injected by ConfigureJwtBearerOptions
                    // (IPostConfigureOptions) after the DI container is built, so that
                    // OpenIddictServerOptions is available without calling BuildServiceProvider here.
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (StartWithHubPath(path) || StartWithMediaPath(path)))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        me.Services.AddScoped<IUserClaimsPrincipalFactory<User>,
            AdditionalUserClaimsPrincipalFactory>();

        me.Services.AddAuthorization(options =>
        {
            options.AddPolicy(UserRoleTypes.TenantAdmin.GetDescription(), policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", UserRoleTypes.TenantAdmin.GetDescription());
            });
            options.AddPolicy(UserRoleTypes.SuperAdmin.GetDescription(), policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", UserRoleTypes.SuperAdmin.GetDescription());
            });
            options.AddPolicy(UserRoleTypes.Client.GetDescription(), policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", UserRoleTypes.Client.GetDescription());
            });
        });
    }
}
