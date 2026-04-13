using Api.V1;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs;
using Grpc.Core;
using Hangfire;
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
    UserManager<ApplicationUser> userManager,
    ILogger<AuthCallbackGrpcService> logger)
    : AuthService.AuthServiceBase
{
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
