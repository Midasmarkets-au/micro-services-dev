using Api.V1;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.EventHandlers;
using Grpc.Core;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Bacera.Gateway.Web.HttpServices.Auth;

/// <summary>
/// gRPC server implementation of AuthService (defined in auth.proto).
/// Called by the Rust auth service during the login flow for operations
/// that require access to TenantDbContext (2FA codes, login logs, config).
/// </summary>
public class AuthCallbackGrpcService(
    IServiceProvider serviceProvider,
    IBackgroundJobClient backgroundJobClient,
    UserManager<Bacera.Gateway.Auth.User> userManager,
    ILogger<AuthCallbackGrpcService> logger)
    : AuthService.AuthServiceBase
{
    public override async Task<RegisterTenantUserResponse> RegisterTenantUser(
        RegisterTenantUserRequest request, ServerCallContext context)
    {
        try
        {
            using var scope = serviceProvider.CreateTenantScope(request.TenantId);
            var tenantCtx  = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            var mediator   = scope.ServiceProvider.GetRequiredService<IMediator>();

            // ── Build Party from CentralParty data ──────────────────────────
            var centralParty = new CentralParty
            {
                Id         = request.PartyId,
                Uid        = request.Uid,
                Email      = request.Email,
                Name       = $"{request.FirstName} {request.LastName}",
                NativeName = request.NativeName,
                Code       = "",
                Note       = "",
                SiteId     = request.SiteId,
                TenantId   = request.TenantId,
                CreatedOn  = DateTime.UtcNow,
                UpdatedOn  = DateTime.UtcNow,
            };
            var party = centralParty.ToParty();

            // Apply user fields to party (mirrors User.ApplyToParty)
            party.NativeName      = request.NativeName;
            party.PhoneNumber     = request.Phone;
            party.Email           = request.Email;
            party.FirstName       = request.FirstName;
            party.LastName        = request.LastName;
            party.Language        = request.Language;
            party.EmailConfirmed  = true;
            party.ReferCode       = request.ReferCode;
            party.CountryCode     = request.CountryCode;
            party.Currency        = request.Currency;
            party.CCC             = request.Ccc;
            party.RegisteredIp    = request.RegisterIp;

            // ── Party + PartyRole ────────────────────────────────────────────
            await tenantCtx.PartyRoles.AddAsync(new PartyRole
            {
                Party  = party,
                RoleId = (int)UserRoleTypes.Client,
            });
            await tenantCtx.SaveChangesAsync();

            // ── TryAssignReferCode ───────────────────────────────────────────
            await TryAssignReferCodeAsync(request.ReferCode, party, tenantCtx);

            // ── Reconstruct User object for the event ────────────────────────
            var user = new Bacera.Gateway.Auth.User
            {
                Id          = request.UserId,
                PartyId     = request.PartyId,
                TenantId    = request.TenantId,
                Email       = request.Email,
                FirstName   = request.FirstName,
                LastName    = request.LastName,
                NativeName  = request.NativeName,
                PhoneNumber = request.Phone,
                Language    = request.Language,
            };

            await mediator.Publish(new UserRegisteredEvent(
                user,
                password: "",        // password not needed post-creation
                callbackUrl: "",
                sourceComment: request.SourceComment,
                utm: request.Utm));

            return new RegisterTenantUserResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterTenantUser failed for party_id={PartyId}", request.PartyId);
            return new RegisterTenantUserResponse { Success = false, Error = ex.Message };
        }
    }

    private static async Task TryAssignReferCodeAsync(
        string referCode, Party party, TenantDbContext ctx)
    {
        if (string.IsNullOrEmpty(referCode)) return;

        var rc = await ctx.ReferralCodes
            .SingleOrDefaultAsync(x => x.Code == referCode);
        if (rc == null) return;
        if (party.Pid > 0 || party.Id == rc.PartyId) return;

        party.Pid = rc.PartyId;
        ctx.Parties.Update(party);
        await ctx.SaveChangesAsync();

        await ctx.Referrals.AddAsync(new Referral
        {
            ReferralCodeId  = rc.Id,
            ReferrerPartyId = rc.PartyId,
            ReferredPartyId = party.Id,
            Code            = rc.Code,
            CreatedOn       = DateTime.UtcNow,
            Module          = nameof(Bacera.Gateway.Auth.User),
            RowId           = party.Id,
        });
        await ctx.SaveChangesAsync();
    }

    public override async Task<VerifyAuthCodeResponse> VerifyAuthCode(
        VerifyAuthCodeRequest request, ServerCallContext context)
    {
        using var scope = serviceProvider.CreateTenantScope(request.TenantId);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var code = request.Code.Replace(" ", "").Replace("-", "");
        var email = request.Email.Trim().ToLower();
        var authCode = await tenantCtx.AuthCodes
            .Where(x => x.Event == AuthCode.EventLabel.TwoFactor
                     && x.Method == (short)AuthCodeMethodTypes.Email
                     && x.MethodValue.ToLower() == email
                     && x.Status == (short)AuthCodeStatusTypes.Valid
                     && x.ExpireOn > DateTime.UtcNow
                     && x.Code == code)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (authCode == null)
        {
            return new VerifyAuthCodeResponse
            {
                Valid = false,
                ErrorCode = "__2FA_CODE_INVALID__",
            };
        }

        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        await tenantCtx.SaveChangesAsync();

        return new VerifyAuthCodeResponse { Valid = true };
    }

    public override Task<SendAuthCodeResponse> SendAuthCode(
        SendAuthCodeRequest request, ServerCallContext context)
    {
        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.GenerateAuthCodeAndSendEmailAsync(request.TenantId, request.Email, request.EventLabel));

        return Task.FromResult(new SendAuthCodeResponse { Sent = true });
    }

    public override async Task<GetTwoFactorSettingResponse> GetTwoFactorSetting(
        GetTwoFactorSettingRequest request, ServerCallContext context)
    {
        using var scope = serviceProvider.CreateTenantScope(request.TenantId);
        var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();

        var setting = await cfgSvc.GetAsync<TwoFactorAuthSetting>(
            nameof(Party), request.PartyId, ConfigKeys.TwoFactorAuthSetting);

        return new GetTwoFactorSettingResponse
        {
            LoginCodeEnabled = setting?.LoginCodeEnabled ?? false,
        };
    }

    public override async Task<GetRecentUserAgentsResponse> GetRecentUserAgents(
        GetRecentUserAgentsRequest request, ServerCallContext context)
    {
        using var scope = serviceProvider.CreateTenantScope(request.TenantId);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var agents = await tenantCtx.LoginLogs
            .Where(x => x.PartyId == request.PartyId)
            .OrderByDescending(x => x.Id)
            .Select(x => x.UserAgent)
            .Take(request.Limit > 0 ? request.Limit : 3)
            .ToListAsync();

        var resp = new GetRecentUserAgentsResponse();
        resp.UserAgents.AddRange(agents.Where(a => a != null)!);
        return resp;
    }

    public override async Task<WriteLoginLogResponse> WriteLoginLog(
        WriteLoginLogRequest request, ServerCallContext context)
    {
        try
        {
            using var scope = serviceProvider.CreateTenantScope(request.TenantId);
            var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            tenantCtx.LoginLogs.Add(new LoginLog
            {
                PartyId   = request.PartyId,
                IpAddress = request.Ip,
                UserAgent = request.UserAgent,
                Referer   = request.Referer,
                CreatedOn = DateTime.UtcNow,
            });

            var party = await tenantCtx.Parties.FirstOrDefaultAsync(p => p.Id == request.PartyId);
            if (party != null)
                party.LastLoginIp = request.Ip;

            await tenantCtx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "WriteLoginLog failed for PartyId={PartyId}", request.PartyId);
        }

        return new WriteLoginLogResponse { Ok = true };
    }

    public override async Task<SendPasswordResetEmailResponse> SendPasswordResetEmail(
        SendPasswordResetEmailRequest request, ServerCallContext context)
    {
        try
        {
            var email = request.Email.Trim().ToLower();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return new SendPasswordResetEmailResponse { Sent = false };

            var callbackUrl = $"{request.ResetUrl.TrimEnd('/')}?code={request.ResetToken}";
            var model = new ResetPasswordViewModel(user.Email!, user.GuessUserName(), callbackUrl);
            backgroundJobClient.Enqueue<IGeneralJob>(x =>
                x.ResetPasswordAsync(request.TenantId, model, user.Language));

            return new SendPasswordResetEmailResponse { Sent = true };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SendPasswordResetEmail failed for email={Email}", request.Email);
            return new SendPasswordResetEmailResponse { Sent = false };
        }
    }
}
