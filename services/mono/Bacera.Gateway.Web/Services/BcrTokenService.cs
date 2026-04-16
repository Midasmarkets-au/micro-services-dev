using Api.V1;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

/// <summary>
/// 非密码流程的程序化 token 生成：
///   - 邮件验证码登录  (AuthControllerV2.AuthCode → ConfirmLoginAuthCode)
///   - 上帝模式 / 模拟登录  (UserController → GodMode)
///   - 多租户 token 切换  (AdminController → GetTokensForAllTenancies)
///
/// 通过 gRPC 调用 Rust auth 服务签发 token，JWT 私钥仅在 auth 服务中保存。
/// Refresh token 由 auth 服务生成并存入 Redis（30 天），通过 IssueTokenResponse.RefreshToken 返回。
/// </summary>
public class BcrTokenService(
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    IHttpContextAccessor httpContextAccessor,
    AuthValidationService.AuthValidationServiceClient authGrpcClient,
    ILogger<BcrTokenService> logger)
{
    public async Task<BcrTokenResult> GetUserTokenAsync(
        User user,
        long godPartyId = 0)
    {
        // ── 1. 收集用户信息 ──────────────────────────────────────────────────
        var roles = await userManager.GetRolesAsync(user);
        var userAgent = httpContextAccessor.HttpContext?.GetUserAgent() ?? "";

        var isTenantAdmin = await authDbContext.Users
            .Where(x => x.Id == user.Id)
            .AnyAsync(x => x.UserRoles.Any(y => y.ApplicationRole.Id <= 10));

        // ── 2. 通过 gRPC 调用 auth 服务签发 token + refresh token ───────────
        // auth 服务在 IssueToken handler 内生成 refresh token 并存入 Redis（30 天）
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

        // ── NOTE: OpenIddict refresh token storage removed ───────────────────
        // Refresh tokens are now managed exclusively by the Rust auth service in Redis.
        // The token is returned via grpcResp.RefreshToken (UUID, 30-day TTL, single-use).

        return new BcrTokenResult
        {
            AccessToken         = accessToken,
            RefreshToken        = grpcResp.RefreshToken,
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
