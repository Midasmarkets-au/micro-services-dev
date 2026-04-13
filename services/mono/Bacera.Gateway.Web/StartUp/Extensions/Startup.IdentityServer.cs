using System.Security.Cryptography.X509Certificates;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Web.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupIdentityServer(this WebApplicationBuilder me)
    {
        // connect/token is now handled entirely by the Rust auth service.
        // OpenIddict server is kept only for refresh token DB management via BcrTokenService.
        me.Services
            .AddScoped<PasswordGrantHandler>()
            .AddScoped<RefreshTokenGrantHandler>()
            .AddSingleton<ApplyTokenResponseHandler>()
            .AddHostedService<OpenIddictSeedService>();

        me.Services.AddOpenIddict()

            // ── Core: EF Core store (refresh token persistence) ──────────────
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AuthDbContext>();
            })

            // ── Server: kept for connect/token fallback + refresh token store ─
            .AddServer(options =>
            {
                var issuerEnv = GetEnvValue("OPENIDDICT_ISSUER", "");
                if (!string.IsNullOrEmpty(issuerEnv))
                    options.SetIssuer(new Uri(issuerEnv));

                options.SetTokenEndpointUris("/connect/token");
                options.AllowPasswordFlow()
                       .AllowRefreshTokenFlow()
                       .AllowClientCredentialsFlow();
                options.RegisterScopes("openid", "offline_access", "api");

                options.AddEventHandler<HandleTokenRequestContext>(
                    builder => builder.UseScopedHandler<PasswordGrantHandler>().SetOrder(500));
                options.AddEventHandler<HandleTokenRequestContext>(
                    builder => builder.UseScopedHandler<RefreshTokenGrantHandler>().SetOrder(501));
                options.AddEventHandler<ApplyTokenResponseContext>(
                    builder => builder.UseSingletonHandler<ApplyTokenResponseHandler>().SetOrder(499_000));

                if (GetEnvValue("PFX", "EMPTY") == "EMPTY")
                    options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
                else
                {
                    var cert = new X509Certificate2(Path.Combine(GetEnvValue("PFX")), "bacera");
                    options.AddSigningCertificate(cert).AddEncryptionCertificate(cert);
                }

                options.DisableAccessTokenEncryption();
                options.SetAccessTokenLifetime(TimeSpan.FromHours(24));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
                options.UseAspNetCore().DisableTransportSecurityRequirement();
            });
        // NOTE: .AddValidation() removed — token validation is now done by JwtBearer
        // using the Rust auth service's RS256 public key from /.well-known/jwks.json.
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
                // Rust auth service puts user ID in "sub" and roles in "role" (short form).
                options.ClaimsIdentity.UserIdClaimType = "sub";
                options.ClaimsIdentity.RoleClaimType   = "role";
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
    }

    public static void SetupAuthentication(this WebApplicationBuilder me)
    {
        // Auth tokens are issued by the Rust auth service (RS256, JWKS at /.well-known/jwks.json).
        // JwtBearer validates the token directly using the public key fetched from JWKS.
        // Tokens are delivered via HttpOnly cookie (set by the Rust auth service on login).
        var authGrpcAddr = GetEnvValue("AUTH_GRPC_ADDR", "http://auth:50002");
        // Derive HTTP base URL from gRPC addr: http://auth:50002 → http://auth:9002
        var authHttpAddr = authGrpcAddr.Replace(":50002", ":9002");

        // JWKS: cache keys fetched from Rust auth service, refresh every 12 hours.
        var jwksUrl = $"{authHttpAddr}/.well-known/jwks.json";
        var jwksManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
            jwksUrl,
            new JwksRetriever(),
            new Microsoft.IdentityModel.Protocols.HttpDocumentRetriever { RequireHttps = false })
        {
            AutomaticRefreshInterval = TimeSpan.FromHours(12),
            RefreshInterval          = TimeSpan.FromMinutes(5),
        };

        // Disable JwtBearer's default claim type mapping so that "sub" stays as "sub"
        // (not remapped to ClaimTypes.NameIdentifier). UserManager.GetUserAsync looks
        // for UserIdClaimType = "sub" set in SetupIdentity.
        System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        me.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience        = false,
                    ValidateIssuer          = false,
                    ValidateIssuerSigningKey = true,
                    ValidTypes              = new[] { "at+jwt", "JWT" },
                    NameClaimType           = "name",
                    RoleClaimType           = "role",
                    IssuerSigningKeyResolver = (token, secToken, kid, parameters) =>
                    {
                        var config = jwksManager.GetConfigurationAsync(CancellationToken.None)
                                                .GetAwaiter().GetResult();
                        return config.JsonWebKeySet?.Keys
                               ?? Enumerable.Empty<Microsoft.IdentityModel.Tokens.SecurityKey>();
                    }
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // All paths: read token from HttpOnly cookie set by Rust auth service.
                        var cookieToken = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrEmpty(cookieToken))
                            context.Token = cookieToken;

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
