using Api.V1;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace Bacera.Gateway.Services;

/// <summary>
/// 非密码流程的程序化 token 生成：
///   - 邮件验证码登录  (AuthControllerV2.AuthCode → ConfirmLoginAuthCode)
///   - 上帝模式 / 模拟登录  (UserController → GodMode)
///   - 多租户 token 切换  (AdminController → GetTokensForAllTenancies)
///
/// 通过 gRPC 调用 Rust auth 服务签发 token，JWT 私钥仅在 auth 服务中保存。
/// </summary>
public class BcrTokenService(
    IOpenIddictApplicationManager appManager,
    IOpenIddictTokenManager tokenManager,
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    IHttpContextAccessor httpContextAccessor,
    AuthValidationService.AuthValidationServiceClient authGrpcClient,
    ILogger<BcrTokenService> logger)
{
    private const string DefaultClientId = "api";

    public async Task<BcrTokenResult> GetUserTokenAsync(
        User user,
        long godPartyId = 0,
        string clientId = DefaultClientId)
    {
        // ── 1. 解析 OpenIddict 应用（仅用于获取 appId 存 refresh token）────
        var app = await appManager.FindByClientIdAsync(clientId)
                  ?? await appManager.FindByClientIdAsync(DefaultClientId)
                  ?? throw new InvalidOperationException($"OpenIddict client '{clientId}' not found.");

        var appId = await appManager.GetIdAsync(app)
                    ?? throw new InvalidOperationException("Application has no Id.");

        // ── 2. 收集用户信息 ──────────────────────────────────────────────────
        var roles = await userManager.GetRolesAsync(user);
        var userAgent = httpContextAccessor.HttpContext?.GetUserAgent() ?? "";

        var isTenantAdmin = await authDbContext.Users
            .Where(x => x.Id == user.Id)
            .AnyAsync(x => x.UserRoles.Any(y => y.ApplicationRole.Id <= 10));

        // ── 3. 通过 gRPC 调用 auth 服务签发 token ───────────────────────────
        var grpcReq = new IssueTokenRequest
        {
            UserId           = user.Id,
            TenantId         = user.TenantId,
            PartyIdHashed    = Party.HashEncode(user.PartyId),
            GodPartyId       = godPartyId,
            DisplayName      = user.GuessUserName(),
            Email            = user.Email ?? "",
            TwoFactorEnabled = user.TwoFactorEnabled,
            UserAgent        = userAgent,
        };
        grpcReq.Roles.AddRange(roles);

        logger.LogInformation("BcrTokenService: calling IssueToken gRPC for user {UserId}", user.Id);
        var grpcResp = await authGrpcClient.IssueTokenAsync(grpcReq);
        var accessToken = grpcResp.AccessToken;
        logger.LogInformation("BcrTokenService: IssueToken returned, token empty={IsEmpty}", string.IsNullOrEmpty(accessToken));
        var accessTokenLifetime = isTenantAdmin
            ? TimeSpan.FromDays(3650)
            : TimeSpan.FromSeconds(grpcResp.ExpiresIn > 0 ? grpcResp.ExpiresIn : 86400);

        // ── 4. 在 OpenIddict 存储中创建 refresh token 记录 ──────────────────
        var now = DateTimeOffset.UtcNow;
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
