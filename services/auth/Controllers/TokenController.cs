using Bacera.Gateway.Auth.Db;
using Bacera.Gateway.Auth.Security;
using Bacera.Gateway.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Bacera.Gateway.Auth.Controllers;

/// <summary>
/// POST /connect/token — OAuth2 token endpoint.
///
/// Mirrors mono's IdentityPasswordValidator + CustomTokenResponseGenerator logic.
///
/// Supported grant types:
///   - "password"           (resource owner password credentials)
///   - "client_credentials" (service-to-service)
/// </summary>
[ApiController]
public class TokenController(
    AuthDbContext authDb,
    CentralDbContext centralDb,
    TenantDbContextFactory tenantDbFactory,
    LoginSecurityService loginSecurity,
    EmailService emailService,
    IConfiguration config,
    ILogger<TokenController> logger) : ControllerBase
{
    [HttpPost("connect/token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ConnectToken([FromForm] TokenRequest req, CancellationToken ct)
    {
        return req.grant_type switch
        {
            "password" => await HandlePasswordGrant(req, ct),
            "client_credentials" => HandleClientCredentials(req),
            _ => ErrorResponse("unsupported_grant_type", $"grant_type '{req.grant_type}' is not supported")
        };
    }

    // ── client_credentials ──────────────────────────────────────────────────

    private IActionResult HandleClientCredentials(TokenRequest req)
    {
        var clientId = req.client_id ?? string.Empty;
        var lifetime = config.GetValue<long>("ACCESS_TOKEN_LIFETIME", 86400);
        var secret = config["JWT_SECRET"] ?? "dev-secret-change-in-production";

        var result = TokenService.GenerateClientCredentialsToken(clientId, lifetime, secret);
        SetTokenCookie(result.AccessToken, result.ExpiresIn);
        return Ok(new
        {
            access_token = result.AccessToken,
            token_type = "Bearer",
            expires_in = result.ExpiresIn,
        });
    }

    // ── password grant ───────────────────────────────────────────────────────

    private async Task<IActionResult> HandlePasswordGrant(TokenRequest req, CancellationToken ct)
    {
        var email = req.username?.Trim().ToLower();
        if (string.IsNullOrEmpty(email))
            return ErrorResponse("invalid_request", "username is required");

        var password = req.password;
        if (string.IsNullOrEmpty(password))
            return ErrorResponse("invalid_request", "password is required");

        // 1. IP blacklist check
        var clientIp = GetClientIp();
        if (await DbQueries.IsIpBlockedAsync(centralDb, clientIp, ct))
        {
            logger.LogWarning("Blocked IP {Ip} attempted login", clientIp);
            return ErrorResponse("__LOGIN_FAILED__", "Login failed.");
        }

        // 2. Account lockout check (Redis-based, mirrors LoginSecurityService)
        var (isLocked, lockoutRemaining, _) = await loginSecurity.CheckLockoutStatusAsync(email);
        if (isLocked)
        {
            logger.LogWarning("Login attempt for locked account. Email: {Email}, IP: {IP}, Remaining: {Min:F1}min",
                email, clientIp, lockoutRemaining?.TotalMinutes ?? 0);
            return ErrorResponse("__ACCOUNT_LOCKED__",
                $"Account is locked due to too many failed login attempts. Please try again in {lockoutRemaining?.TotalMinutes:F0} minutes or contact customer service.");
        }

        // 3. Find users by email
        var users = await DbQueries.FindUsersByEmailAsync(authDb, email, ct);
        if (users.Count == 0)
        {
            await loginSecurity.RecordFailedLoginAsync(email, clientIp);
            return ErrorResponse("__LOGIN_FAILED__", "Login failed.");
        }

        var tenantId = long.TryParse(req.tenantId, out var tid) ? (long?)tid : null;

        // 4. Multiple tenants — validate password then return tenant list
        if (users.Count > 1 && tenantId is null)
        {
            if (!ValidatePasswordAny(users, password))
            {
                await loginSecurity.RecordFailedLoginAsync(email, clientIp);
                return ErrorResponse("__LOGIN_FAILED__", "Login failed.");
            }

            var tenantIds = users.Select(u => u.TenantId).Distinct().ToList();
            return Ok(new { tenantIds, hasMultipleTenants = true });
        }

        var user = tenantId.HasValue
            ? users.FirstOrDefault(u => u.TenantId == tenantId.Value)
            : users.FirstOrDefault();

        if (user is null)
        {
            await loginSecurity.RecordFailedLoginAsync(email, clientIp);
            return ErrorResponse("__LOGIN_FAILED__", "Login failed.");
        }

        if (user.Status == 1)
            return ErrorResponse("__USER_UNDER_MAINTENANCE__", "Our system is under maintenance.");

        if (!user.EmailConfirmed)
            return ErrorResponse("__EMAIL_UNCONFIRMED__", "Please confirm your email before login.");

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            await loginSecurity.RecordFailedLoginAsync(email, clientIp);
            return ErrorResponse("__LOGIN_FAILED__", "Login failed.");
        }

        if (!PasswordVerifier.VerifyHashedPasswordV3(user.PasswordHash, password))
        {
            await loginSecurity.RecordFailedLoginAsync(email, clientIp);
            return ErrorResponse("__LOGIN_FAILED__", "Login failed.");
        }

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            return ErrorResponse("__USER_IS_LOCKED_OUT__", "Please contact customer service.");

        // 5. Determine user roles
        var roles = await DbQueries.GetUserRolesAsync(authDb, user.Id, ct);
        var isAdminUser = roles.Contains("TenantAdmin") || roles.Contains("Admin") || roles.Contains("SuperAdmin");
        var isClientUser = roles.Contains("Client") && !isAdminUser;

        // 6. Open tenant DB context for 2FA and config checks
        await using var tenantCtx = await tenantDbFactory.CreateAsync(user.TenantId, ct);

        // 7. Client Email 2FA check
        if (tenantCtx != null)
        {
            var tfSetting = await GetTwoFactorAuthSettingAsync(tenantCtx, user.PartyId, ct);
            if (tfSetting is { LoginCodeEnabled: true })
            {
                bool shouldRequireEmail2FA;
                if (isClientUser)
                {
                    shouldRequireEmail2FA = true;
                }
                else
                {
                    // Admin users: only require email 2FA for new devices
                    var recentAgents = await tenantCtx.LoginLogs
                        .Where(x => x.PartyId == user.PartyId)
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.UserAgent)
                        .Take(3)
                        .ToListAsync(ct);

                    var currentAgent = GetUserAgent();
                    var recentHashed = recentAgents.Select(a => Md5Hash(a + ".thebcr.com")).ToList();
                    shouldRequireEmail2FA = !recentHashed.Contains(Md5Hash(currentAgent + ".thebcr.com"));
                }

                if (shouldRequireEmail2FA)
                {
                    var tfCode = req.tf_code ?? req.code;

                    if (string.IsNullOrEmpty(tfCode))
                    {
                        // Trigger email send (fire-and-forget) — create a fresh TenantDbContext
                        // because tenantCtx (await using) will be disposed when this method returns
                        var capturedTenantId = user.TenantId;
                        var capturedPartyId = user.PartyId;
                        _ = Task.Run(() => GenerateAndSendAuthCodeAsync(capturedTenantId, capturedPartyId, email), CancellationToken.None);
                        return Ok(new { twoFactorRequired = true, emailSent = true });
                    }

                    tfCode = tfCode.Replace(" ", "").Replace("-", "");
                    var authCode = await tenantCtx.AuthCodes
                        .Where(x => x.Event == AuthCode.TwoFactorEvent)
                        .Where(x => x.Method == AuthCode.MethodEmail && x.MethodValue == email)
                        .Where(x => x.Status == AuthCode.StatusValid)
                        .Where(x => x.ExpireOn > DateTime.UtcNow)
                        .Where(x => x.Code == tfCode)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync(ct);

                    if (authCode == null)
                        return ErrorResponse("__2FA_CODE_INVALID__", "Please provide a valid email verification code.");

                    authCode.Status = AuthCode.StatusInvalid;
                    await tenantCtx.SaveChangesAsync(ct);
                }
            }
        }

        // 8. Admin Authenticator (TOTP) 2FA check
        if (isAdminUser && user.TwoFactorEnabled)
        {
            var tfCode = req.tf_code ?? req.code;

            if (string.IsNullOrEmpty(tfCode))
            {
                return Ok(new { twoFactorRequired = true, authenticator2FA = true });
            }

            tfCode = tfCode.Replace(" ", "").Replace("-", "");
            if (tfCode.Length != 6)
                return ErrorResponse("__2FA_CODE_INVALID__", "Please provide a valid 6-digit authenticator code.");

            var totpSecret = await DbQueries.GetAuthenticatorKeyAsync(authDb, user.Id, ct);
            if (string.IsNullOrEmpty(totpSecret) || !VerifyTotp(totpSecret, tfCode))
                return ErrorResponse("__2FA_CODE_INVALID__", "Please provide a valid authenticator code.");
        }

        // 9. Generate tokens
        var partyIdHashed = HashIds.EncodePartyId(user.PartyId);
        var lifetime = config.GetValue<long>("ACCESS_TOKEN_LIFETIME", 86400);
        var jwtSecret = config["JWT_SECRET"] ?? "dev-secret-change-in-production";

        var tokenResult = TokenService.GenerateAccessToken(
            user.Id, user.TenantId, partyIdHashed, roles,
            user.TwoFactorEnabled, lifetime, jwtSecret);

        var refreshToken = TokenService.GenerateRefreshToken();

        // Capture request-scoped values before fire-and-forget (HttpContext becomes invalid after response)
        var capturedUserAgent = GetUserAgent();
        var capturedReferer = HttpContext.Request.Headers["Referer"].ToString();
        var authConnStr = authDb.Database.GetConnectionString() ?? string.Empty;

        // 10. Post-login tasks (fire-and-forget) — use fresh DB connections, not scoped instances
        _ = Task.Run(() => PostLoginTasksAsync(
            user, clientIp, capturedUserAgent, capturedReferer,
            authConnStr, user.TenantId), CancellationToken.None);
        _ = Task.Run(() => loginSecurity.ResetFailedAttemptsAsync(email), CancellationToken.None);

        logger.LogInformation("Successful login. Email: {Email}, IP: {IP}", email, clientIp);

        SetTokenCookie(tokenResult.AccessToken, tokenResult.ExpiresIn);
        return Ok(new
        {
            access_token = tokenResult.AccessToken,
            token_type = "Bearer",
            expires_in = tokenResult.ExpiresIn,
            refresh_token = refreshToken,
        });
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task GenerateAndSendAuthCodeAsync(long tenantId, long partyId, string email)
    {
        try
        {
            await using var freshCtx = await tenantDbFactory.CreateAsync(tenantId);
            if (freshCtx == null)
            {
                logger.LogError("Cannot create TenantDbContext for tenantId {TenantId} to send 2FA email", tenantId);
                return;
            }

            var code = new Random().Next(100000, 999999).ToString();
            freshCtx.AuthCodes.Add(new AuthCode
            {
                PartyId = partyId,
                Code = code,
                Event = AuthCode.TwoFactorEvent,
                Method = AuthCode.MethodEmail,
                MethodValue = email,
                Status = AuthCode.StatusValid,
                CreatedOn = DateTime.UtcNow,
                ExpireOn = DateTime.UtcNow.AddMinutes(11),
            });
            await freshCtx.SaveChangesAsync();
            await emailService.SendTwoFactorCodeAsync(email, code);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate/send 2FA auth code for {Email}", email);
        }
    }

    private async Task PostLoginTasksAsync(
        Db.User user, string clientIp, string userAgent, string referer,
        string authConnStr, long tenantId)
    {
        // Create a fresh AuthDbContext — the scoped one from DI is disposed after the HTTP response
        try
        {
            var opts = new DbContextOptionsBuilder<AuthDbContext>()
                .UseNpgsql(authConnStr)
                .Options;
            await using var freshAuthDb = new AuthDbContext(opts);
            await DbQueries.UpdateLastLoginAsync(freshAuthDb, user.Id, clientIp);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update last login for user {UserId}", user.Id);
        }

        // Create a fresh TenantDbContext for login log
        try
        {
            await using var freshTenantCtx = await tenantDbFactory.CreateAsync(tenantId);
            if (freshTenantCtx == null) return;

            freshTenantCtx.LoginLogs.Add(new LoginLog
            {
                PartyId = user.PartyId,
                UserAgent = userAgent,
                Referer = referer,
                IpAddress = clientIp,
                CreatedOn = DateTime.UtcNow,
            });
            await freshTenantCtx.Database.ExecuteSqlAsync(
                $"""UPDATE core."_Party" SET "LastLoginIp" = {clientIp} WHERE "Id" = {user.PartyId}""");
            await freshTenantCtx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write login log for partyId {PartyId}", user.PartyId);
        }
    }

    private static async Task<TwoFactorAuthSetting?> GetTwoFactorAuthSettingAsync(
        TenantDbContext ctx, long partyId, CancellationToken ct)
    {
        var value = await ctx.Configurations
            .Where(x => x.Category == "Party" && x.RowId == partyId && x.Key == "TwoFactorAuthSetting")
            .OrderByDescending(x => x.Id)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(ct);

        if (value == null) return null;
        try { return Newtonsoft.Json.JsonConvert.DeserializeObject<TwoFactorAuthSetting>(value); }
        catch { return null; }
    }

    private static bool VerifyTotp(string base32Secret, string code)
    {
        try
        {
            var key = Base32Encoding.ToBytes(base32Secret);
            var totp = new Totp(key);
            return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
        }
        catch { return false; }
    }

    private string GetClientIp()
    {
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            var first = forwarded.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(first)) return first;
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    private string GetUserAgent()
        => HttpContext.Request.Headers["User-Agent"].ToString();

    private static bool ValidatePasswordAny(IEnumerable<Db.User> users, string password)
        => users.Any(u => !string.IsNullOrEmpty(u.PasswordHash)
                          && PasswordVerifier.VerifyHashedPasswordV3(u.PasswordHash, password));

    private static string Md5Hash(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = md5.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }

    private void SetTokenCookie(string token, long expiresIn)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn),
            Path = "/",
        };
        HttpContext.Response.Cookies.Append("access_token", token, options);
    }

    private IActionResult ErrorResponse(string error, string description)
        => BadRequest(new { error, error_description = description });
}

public class TwoFactorAuthSetting
{
    public bool LoginCodeEnabled { get; set; }
}

public class TokenRequest
{
    public string? grant_type { get; set; }
    public string? client_id { get; set; }
    public string? client_secret { get; set; }
    public string? username { get; set; }
    public string? password { get; set; }
    public string? scope { get; set; }
    public string? refresh_token { get; set; }
    public string? tenantId { get; set; }
    public string? code { get; set; }
    public string? tf_code { get; set; }
}
