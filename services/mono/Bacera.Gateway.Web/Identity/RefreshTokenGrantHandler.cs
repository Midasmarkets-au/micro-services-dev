using System.Security.Claims;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Extension;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// 处理 POST /connect/token (grant_type=refresh_token)。
///
/// 流程：
///   1. 从 context.RefreshTokenPrincipal 取出 sub（用户ID）
///   2. 从数据库重新加载用户（验证用户仍然有效）
///   3. 重新构建 claims principal（保持 claims 最新）
///   4. 调用 context.SignIn() — OpenIddict 自动签发新 access_token + refresh_token
/// </summary>
public class RefreshTokenGrantHandler(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor)
    : IOpenIddictServerHandler<HandleTokenRequestContext>
{
    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
        = OpenIddictServerHandlerDescriptor
            .CreateBuilder<HandleTokenRequestContext>()
            .UseScopedHandler<RefreshTokenGrantHandler>()
            .SetOrder(501)
            .Build();

    public async ValueTask HandleAsync(HandleTokenRequestContext context)
    {
        if (!context.Request.IsRefreshTokenGrantType())
            return;

        // OpenIddict 在调用此 handler 之前已验证 refresh_token 的有效性
        // RefreshTokenPrincipal 包含原始 token 签发时存入的 claims
        var principal = context.RefreshTokenPrincipal;
        if (principal is null)
        {
            context.Reject(Errors.InvalidGrant, "Refresh token principal is missing.");
            return;
        }

        var subject = principal.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(subject) || !long.TryParse(subject, out var userId))
        {
            context.Reject(Errors.InvalidGrant, "Invalid subject claim.");
            return;
        }

        // ── 重新加载用户，确保账号仍然有效 ───────────────────────────────────
        var user = await userManager.Users
            .Where(x => x.Id == userId && x.Status == 0)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            context.Reject(Errors.InvalidGrant, "__USER_NOT_FOUND__");
            return;
        }

        if (!user.EmailConfirmed)
        {
            context.Reject(Errors.InvalidGrant, "__EMAIL_UNCONFIRMED__");
            return;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            context.Reject(Errors.AccessDenied, "__USER_IS_LOCKED_OUT__");
            return;
        }

        // ── 重新构建 principal（保持 claims 最新）────────────────────────────
        var roles     = await userManager.GetRolesAsync(user);
        var isTenantAdmin = roles.Any(r => r == UserRoleTypesString.TenantAdmin);
        var lifetime  = isTenantAdmin ? TimeSpan.FromDays(3650) : TimeSpan.FromHours(24);

        // 保留原始 godPartyId（如果有）
        var godPartyIdStr = principal.GetClaim(UserClaimTypes.GodPartyId);
        var godPartyId    = Party.HashDecode(godPartyIdStr ?? "");

        var userAgent = httpContextAccessor.HttpContext?.GetUserAgent() ?? "";

        var newPrincipal = PasswordGrantHandler.BuildPrincipal(
            user, godPartyId, roles, lifetime, userAgent);

        context.SignIn(newPrincipal);
    }
}
