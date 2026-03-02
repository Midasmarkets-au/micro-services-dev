using Bacera.Gateway.Auth;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Bacera.Gateway.Services;

/// <summary>
/// 非密码流程的程序化 token 生成：
///   - 邮件验证码登录  (AuthControllerV2.AuthCode → ConfirmLoginAuthCode)
///   - 上帝模式 / 模拟登录  (UserController → GodMode)
///   - 多租户 token 切换  (AdminController → GetTokensForAllTenancies)
///
/// 使用 OpenIddict 的签名凭证直接签发 JWT，
/// claims 构建逻辑复用 PasswordGrantHandler.BuildPrincipal。
/// </summary>
public class BcrTokenService(
    IOpenIddictApplicationManager appManager,
    IOpenIddictTokenManager tokenManager,
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<OpenIddictServerOptions> serverOptions)
{
    private const string DefaultClientId = "api";

    public async Task<BcrTokenResult> GetUserTokenAsync(
        User user,
        long godPartyId = 0,
        string clientId = DefaultClientId)
    {
        // ── 1. 解析 OpenIddict 应用 ──────────────────────────────────────────
        var app = await appManager.FindByClientIdAsync(clientId)
                  ?? await appManager.FindByClientIdAsync(DefaultClientId)
                  ?? throw new InvalidOperationException($"OpenIddict client '{clientId}' not found.");

        var appId = await appManager.GetIdAsync(app)
                    ?? throw new InvalidOperationException("Application has no Id.");

        // ── 2. 确定 token 有效期 ─────────────────────────────────────────────
        var isTenantAdmin = await authDbContext.Users
            .Where(x => x.Id == user.Id)
            .AnyAsync(x => x.UserRoles.Any(y => y.ApplicationRole.Id <= 10));

        var accessTokenLifetime = isTenantAdmin
            ? TimeSpan.FromDays(3650)
            : TimeSpan.FromHours(24);

        // ── 3. 构建 principal（复用 PasswordGrantHandler 的逻辑）────────────
        var roles     = await userManager.GetRolesAsync(user);
        var userAgent = httpContextAccessor.HttpContext?.GetUserAgent() ?? "";
        var principal = PasswordGrantHandler.BuildPrincipal(user, godPartyId, roles, accessTokenLifetime, userAgent);

        // ── 4. 签发 access token ─────────────────────────────────────────────
        var options = serverOptions.CurrentValue;
        var signingCredentials = options.SigningCredentials.FirstOrDefault()
            ?? throw new InvalidOperationException("No signing credentials configured in OpenIddict.");

        var now    = DateTimeOffset.UtcNow;
        var issuer = options.Issuer?.AbsoluteUri ?? "https://localhost";

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer             = issuer,
            Audience           = appId,
            Subject            = principal.Identity as System.Security.Claims.ClaimsIdentity,
            IssuedAt           = now.UtcDateTime,
            NotBefore          = now.UtcDateTime,
            Expires            = now.Add(accessTokenLifetime).UtcDateTime,
            SigningCredentials = signingCredentials,
            TokenType          = "at+jwt",
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        // ── 5. 在 OpenIddict 存储中创建 refresh token 记录 ──────────────────
        var referenceId = Guid.NewGuid().ToString("N");
        await tokenManager.CreateAsync(new OpenIddictTokenDescriptor
        {
            ApplicationId  = appId,
            Subject        = user.Id.ToString(),
            Status         = OpenIddictConstants.Statuses.Valid,
            Type           = OpenIddictConstants.TokenTypeHints.RefreshToken,
            ReferenceId    = referenceId,
            CreationDate   = now,
            ExpirationDate = now.AddDays(30),
        });

        return new BcrTokenResult
        {
            AccessToken         = accessToken,
            RefreshToken        = referenceId,
            AccessTokenLifetime = (int)accessTokenLifetime.TotalSeconds,
        };
    }
}

public class BcrTokenResult
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int AccessTokenLifetime { get; set; }
}
