using System.Security.Claims;
using System.Text.Json;
using Bacera.Gateway.Auth;
using Microsoft.AspNetCore;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// 处理 POST /connect/token (grant_type=password)。
/// 整合了 IS4 的 IdentityPasswordValidator + CustomTokenResponseGenerator + IdentityProfileService。
///
/// 特殊响应（复刻 IS4 行为）：
///   hasMultipleTenants  → 用户存在于多个租户，返回租户选择器
///   twoFactorRequired   → 需要邮件或 Authenticator 二次验证
///   正常登录            → 调用 context.SignIn()，由 OpenIddict 标准签发 token
/// </summary>
public class PasswordGrantHandler(
    MyDbContextPool pool,
    UserManager<User> userManager,
    ILogger<PasswordGrantHandler> logger,
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider,
    IServiceScopeFactory serviceScopeFactory,
    CentralDbContext centralDbContext,
    LoginSecurityService loginSecurityService,
    IBackgroundJobClient backgroundJobClient)
    : IOpenIddictServerHandler<HandleTokenRequestContext>
{
    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
        = OpenIddictServerHandlerDescriptor
            .CreateBuilder<HandleTokenRequestContext>()
            .UseScopedHandler<PasswordGrantHandler>()
            .SetOrder(500)
            .Build();

    public async ValueTask HandleAsync(HandleTokenRequestContext context)
    {
        if (!context.Request.IsPasswordGrantType())
            return;

        var remoteIp = httpContextAccessor.GetRemoteIpAddress();
        var email    = context.Request.Username?.Trim().ToLower() ?? string.Empty;
        var password = context.Request.Password ?? string.Empty;

        // ── 1. IP 黑名单 ──────────────────────────────────────────────────────
        if (await centralDbContext.IpBlackLists.AnyAsync(x => x.Ip == remoteIp))
        {
            logger.LogWarning("Blocked IP {IP} attempted login", remoteIp);
            context.Reject(Errors.AccessDenied, "__LOGIN_FAILED__");
            return;
        }

        // ── 2. 账号锁定检查 ───────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(email))
        {
            var (isLocked, remaining, _) = await loginSecurityService.CheckLockoutStatusAsync(email);
            if (isLocked)
            {
                logger.LogWarning("Locked account login attempt. Email: {Email}, IP: {IP}", email, remoteIp);
                context.Reject(Errors.AccessDenied,
                    $"__ACCOUNT_LOCKED__ Please try again in {remaining?.TotalMinutes:F0} minutes.");
                return;
            }
        }

        // ── 3. 按邮箱加载用户 ─────────────────────────────────────────────────
        var users = await userManager.Users
            .Where(x => x.Status == 0 && x.Email == email)
            .ToListAsync();

        var tenantIdStr = (string?)context.Request.GetParameter("tenantId");
        var tenantId    = long.TryParse(tenantIdStr, out var tid) ? tid : -1L;

        // ── 4. 多租户 → 返回租户选择器 ────────────────────────────────────────
        if (users.Count > 1 && tenantId == -1)
        {
            if (!await ValidateAnyPasswordAsync(users, password))
            {
                await loginSecurityService.RecordFailedLoginAsync(email, remoteIp);
                context.Reject(Errors.InvalidGrant, "__LOGIN_FAILED__");
                return;
            }

            var tenantIds = users.Select(u => u.TenantId).Distinct().ToList();
            await WriteCustomResponseAsync(context, new Dictionary<string, object>
            {
                { "hasMultipleTenants", true },
                { "tenantIds", tenantIds },
            });
            return;
        }

        // ── 5. 定位单个用户 ───────────────────────────────────────────────────
        var user = users.FirstOrDefault(x => tenantId == -1 || x.TenantId == tenantId);
        if (user == null)
        {
            await loginSecurityService.RecordFailedLoginAsync(email, remoteIp);
            context.Reject(Errors.InvalidGrant, "__LOGIN_FAILED__");
            return;
        }

        if (user.Status == 1)
        {
            context.Reject(Errors.AccessDenied, "__USER_UNDER_MAINTENANCE__");
            return;
        }

        if (!user.EmailConfirmed)
        {
            context.Reject(Errors.AccessDenied, "__EMAIL_UNCONFIRMED__");
            return;
        }

        if (user.PasswordHash == null || !await ValidateAnyPasswordAsync(users, password))
        {
            await loginSecurityService.RecordFailedLoginAsync(email, remoteIp);
            context.Reject(Errors.InvalidGrant, "__LOGIN_FAILED__");
            return;
        }

        // ── 6. Identity 锁定 ──────────────────────────────────────────────────
        if (await userManager.IsLockedOutAsync(user))
        {
            context.Reject(Errors.AccessDenied, "__USER_IS_LOCKED_OUT__");
            return;
        }

        // ── 7. 二次验证 ───────────────────────────────────────────────────────
        using var tenantScope = serviceProvider.CreateTenantScope(user.TenantId);
        var cfgSvc    = tenantScope.ServiceProvider.GetRequiredService<ConfigService>();
        var tenantCtx = tenantScope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var userRoles   = await userManager.GetRolesAsync(user);
        var isAdminUser = userRoles.Any(r => r is UserRoleTypesString.TenantAdmin
                                          or UserRoleTypesString.Admin
                                          or UserRoleTypesString.SuperAdmin);
        var isClientUser = userRoles.Contains(UserRoleTypesString.Client) && !isAdminUser;

        // 7a. 邮件 2FA（客户用户始终要求；管理员用户仅在新设备时要求）
        var tfSetting = await cfgSvc.GetAsync<TwoFactorAuthSetting>(
            nameof(Party), user.PartyId, ConfigKeys.TwoFactorAuthSetting);

        if (tfSetting is { LoginCodeEnabled: true })
        {
            bool requireEmail2FA = isClientUser;
            if (!requireEmail2FA)
            {
                var recentAgents = await tenantCtx.LoginLogs
                    .Where(x => x.PartyId == user.PartyId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.UserAgent)
                    .Take(3)
                    .ToListAsync();
                var currentAgentHash = Utils.Md5Hash(
                    (httpContextAccessor.HttpContext?.GetUserAgent() ?? "") + ".thebcr.com");
                requireEmail2FA = !recentAgents
                    .Select(a => Utils.Md5Hash(a + ".thebcr.com"))
                    .Contains(currentAgentHash);
            }

            if (requireEmail2FA)
            {
                var tfCode = (string?)context.Request.GetParameter("tf_code")
                          ?? (string?)context.Request.GetParameter("code");

                if (string.IsNullOrEmpty(tfCode))
                {
                    backgroundJobClient.Enqueue<IGeneralJob>(x =>
                        x.GenerateAuthCodeAndSendEmailAsync(
                            user.TenantId, email, AuthCode.EventLabel.TwoFactor));

                    await WriteCustomResponseAsync(context, new Dictionary<string, object>
                    {
                        { "twoFactorRequired", true },
                        { "emailSent", true },
                    });
                    return;
                }

                tfCode = tfCode.Replace(" ", "").Replace("-", "");
                var authCode = await tenantCtx.AuthCodes
                    .Where(x => x.Event == AuthCode.EventLabel.TwoFactor
                             && x.Method == (short)AuthCodeMethodTypes.Email
                             && x.MethodValue == email
                             && x.Status == (short)AuthCodeStatusTypes.Valid
                             && x.ExpireOn > DateTime.UtcNow
                             && x.Code == tfCode)
                    .OrderByDescending(x => x.CreatedOn)
                    .FirstOrDefaultAsync();

                if (authCode == null)
                {
                    context.Reject(Errors.InvalidGrant, "__2FA_CODE_INVALID__");
                    return;
                }

                authCode.Status = (short)AuthCodeStatusTypes.Invalid;
                await tenantCtx.SaveChangesAsync();
            }
        }

        // 7b. Authenticator (TOTP) 2FA — 仅管理员用户
        if (isAdminUser && await userManager.GetTwoFactorEnabledAsync(user))
        {
            var tfCode = (string?)context.Request.GetParameter("tf_code")
                      ?? (string?)context.Request.GetParameter("code");

            if (string.IsNullOrEmpty(tfCode))
            {
                await WriteCustomResponseAsync(context, new Dictionary<string, object>
                {
                    { "twoFactorRequired", true },
                    { "authenticator2FA", true },
                });
                return;
            }

            tfCode = tfCode.Replace(" ", "").Replace("-", "");
            if (tfCode.Length != 6)
            {
                context.Reject(Errors.InvalidGrant, "__2FA_CODE_INVALID__");
                return;
            }

            var valid = await userManager.VerifyTwoFactorTokenAsync(
                user, userManager.Options.Tokens.AuthenticatorTokenProvider, tfCode);
            if (!valid)
            {
                context.Reject(Errors.InvalidGrant, "__2FA_CODE_INVALID__");
                return;
            }
        }

        // ── 8. 登录后副作用（fire-and-forget）────────────────────────────────
        // Each task that touches DbContext gets its own DI scope to avoid
        // concurrent access on the request-scoped AuthDbContext instance.
        var userId   = user.Id;
        var tenantId2 = user.TenantId;
        var partyId  = user.PartyId;
        _ = Task.WhenAll(
            Task.Run(async () =>
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var u  = await um.FindByIdAsync(userId.ToString());
                if (u != null)
                {
                    u.LastLoginOn = DateTime.UtcNow;
                    u.LastLoginIp = remoteIp;
                    await um.UpdateAsync(u);
                }
            }),
            Task.Run(() => WriteLoginLogAsync(tenantId2, partyId, remoteIp)),
            Task.Run(() => loginSecurityService.ResetFailedAttemptsAsync(email))
        ).ContinueWith(t =>
        {
            if (t.IsFaulted)
                BcrLog.Slack($"Post-login tasks failed: {t.Exception?.GetBaseException().Message}");
        });

        // ── 9. 构建 principal 并交由 OpenIddict 签发 token ───────────────────
        var isTenantAdmin = userRoles.Any(r => r == UserRoleTypesString.TenantAdmin);
        var lifetime      = isTenantAdmin ? TimeSpan.FromDays(3650) : TimeSpan.FromHours(24);

        var principal = BuildPrincipal(user, godPartyId: 0, userRoles, lifetime);
        context.SignIn(principal);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 公共方法：供 RefreshTokenGrantHandler 复用
    // ─────────────────────────────────────────────────────────────────────────

    public static ClaimsPrincipal BuildPrincipal(
        User user,
        long godPartyId,
        IList<string> roles,
        TimeSpan accessTokenLifetime,
        string? userAgent = null)
    {
        var identity = new ClaimsIdentity(
            authenticationType: "openiddict",
            nameType: Claims.Name,
            roleType: Claims.Role);

        // 标准 OIDC claims
        identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString())
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(Claims.Name, user.GuessUserName())
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim("idp", "local")
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim("amr", user.TwoFactorEnabled ? "mfa" : "pwd")
            .SetDestinations(Destinations.AccessToken));

        // 业务自定义 claims
        identity.AddClaim(new Claim(UserClaimTypes.PartyId, Party.HashEncode(user.PartyId))
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(UserClaimTypes.TenantId, user.TenantId.ToString())
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(UserClaimTypes.GodPartyId, Party.HashEncode(godPartyId))
            .SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(UserClaimTypes.Version, UserClaimValues.Version)
            .SetDestinations(Destinations.AccessToken));

        var ua = userAgent ?? "";
        identity.AddClaim(new Claim(UserClaimTypes.UserAgent, Utils.Md5Hash(ua + ".thebcr.com"))
            .SetDestinations(Destinations.AccessToken));

        foreach (var role in roles)
            identity.AddClaim(new Claim(Claims.Role, role)
                .SetDestinations(Destinations.AccessToken));

        var principal = new ClaimsPrincipal(identity);

        // 设置 scopes 和 token 有效期
        principal.SetScopes(Scopes.OfflineAccess, "api");
        principal.SetResources("api");
        principal.SetAccessTokenLifetime(accessTokenLifetime);
        principal.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

        return principal;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 私有辅助方法
    // ─────────────────────────────────────────────────────────────────────────

    private static async ValueTask WriteCustomResponseAsync(
        HandleTokenRequestContext context,
        Dictionary<string, object> payload)
    {
        var httpResponse = context.Transaction.GetHttpRequest()?.HttpContext.Response;
        if (httpResponse != null)
        {
            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode  = 200;
            await httpResponse.WriteAsync(JsonSerializer.Serialize(payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
        context.HandleRequest();
    }

    private async Task<bool> ValidateAnyPasswordAsync(IEnumerable<User> users, string password)
    {
        foreach (var u in users)
            if (await userManager.CheckPasswordAsync(u, password))
                return true;
        return false;
    }

    private async Task WriteLoginLogAsync(long tenantId, long partyId, string ip)
    {
        if (!Startup.IsApiLogEnable()) return;
        await using var con = pool.CreateTenantDbConnection(tenantId);
        var ua      = (httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "").Replace("'", "''");
        var referer = (httpContextAccessor.HttpContext?.Request.Headers["Referer"].ToString()    ?? "").Replace("'", "''");
        await con.ExecuteAsync(
            $"""
             UPDATE core."_Party" SET "LastLoginIp" = '{ip}' WHERE "Id" = {partyId};
             INSERT INTO core."_LoginLog" ("PartyId","UserAgent","IpAddress","Referer")
             VALUES ({partyId},'{ua}','{ip}','{referer}');
             """);
    }
}
