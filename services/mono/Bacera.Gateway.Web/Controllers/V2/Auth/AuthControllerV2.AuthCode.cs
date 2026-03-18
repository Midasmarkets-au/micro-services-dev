using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Identity;
using Bacera.Gateway.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Controllers.V2.Auth;

public partial class AuthControllerV2
{
    /// <summary>
    /// Generate reset password code for user
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("password-reset/code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateResetPasswordCode([FromBody] AuthCode.CreateSpec spec)
    {
        var email = spec.Email.ToLower().Trim();
        if (!await IsEmailValidToSendAsync(email))
            return BadRequest(Result.Error("Too many requests, please try again in an hour."));

        var user = await userMgr.Users
            .Where(x => x.Email == email && x.Status == 0 && x.EmailConfirmed)
            .OrderBy(x => x.TenantId)
            .Select(x => new { x.TenantId })
            .FirstOrDefaultAsync();
        if (user == null) return NotFound(Result.Error("User not found."));
        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.GenerateAuthCodeAndSendEmailAsync(user.TenantId, email, AuthCode.EventLabel.ResetPassword));
        return Ok(Result.Success("Reset password code sent."));
    }


    /// <summary>
    ///  Confirm reset password code and reset password for all users
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("password-reset/code/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmResetPasswordCode([FromBody] AuthCode.ConfirmPasswordSpec spec)
    {
        var email = spec.Email.ToLower().Trim();
        var users = await userMgr.Users
            .Where(x => x.Email == email && x.Status == 0 && x.EmailConfirmed)
            .OrderBy(x => x.TenantId)
            .ToListAsync();

        if (users.Count == 0)
            return NotFound(Result.Error("User not found."));

        var tenantId = users.First().TenantId;
        var password = spec.NewPassword;
        var passwordValidator = new PasswordValidator<User>();
        var validatePwsResult = await passwordValidator.ValidateAsync(userMgr, new User(), password);
        if (!validatePwsResult.Succeeded)
        {
            var descp = validatePwsResult.Errors.Select(x => x.Description).ToList();
            return BadRequest(ToErrorResult(ResultMessage.Register.RegisterFail, descp));
        }

        await using var ctx = myDbContextPool.CreateTenantDbContext(tenantId);
        var authCode = await ctx.AuthCodes
            .Where(x => x.Event == AuthCode.EventLabel.ResetPassword)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.Email && x.MethodValue == email)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == spec.Code)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (authCode == null) return BadRequest(Result.Error("Invalid Code"));

        foreach (var user in users)
        {
            var token = await userMgr.GeneratePasswordResetTokenAsync(user);
            var result = await userMgr.ResetPasswordAsync(user, token, password);
            if (result.Succeeded) continue;

            BcrLog.Slack($"Reset password failed for user {user.Email} with error: {result.Errors}");
            return BadRequest(Result.Error("Reset password failed."));
        }

        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        await ctx.SaveChangesAsync();
        return Ok(Result.Success("Reset password code succeed."));
    }

    /// <summary>
    /// Generate login auth code for user
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("token/code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoginAuthCode([FromBody] AuthCode.CreateSpec spec)
    {
        var email = spec.Email.ToLower().Trim();
        if (!await IsEmailValidToSendAsync(email))
            return BadRequest(Result.Error("Too many requests, please try again in an hour."));

        var users = await userMgr.Users
            .Where(x => x.Email == email && x.Status == 0 && x.EmailConfirmed)
            .OrderBy(x => x.TenantId)
            .Select(x => new { x.TenantId })
            .ToListAsync();

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (users.Count == 0) return NotFound(Result.Error("User not found."));
        if (users.Count > 1) return BadRequest(Result.Error("Email login not allowed"));
        var user = users.First();

        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.GenerateAuthCodeAndSendEmailAsync(user.TenantId, email, AuthCode.EventLabel.Login));

        return Ok(Result.Success("Login code sent."));
    }

    /// <summary>
    /// Confirm login auth code and return token
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("token/code/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmLoginAuthCode([FromBody] AuthCode.ConfirmEmailLoginSpec spec)
    {
        var email = spec.Email.ToLower().Trim();

        var users = await userMgr.Users
            .Where(x => x.Email == email && x.Status == 0 && x.EmailConfirmed)
            .OrderBy(x => x.TenantId)
            .ToListAsync();

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (users.Count == 0) return NotFound(Result.Error("User not found."));
        if (users.Count > 1) return BadRequest(Result.Error("Email login not allowed"));
        var user = users.First();

        using var scope = CreateTenantScopeByTenantIdAsync(user.TenantId);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var authCode = await ctx.AuthCodes
            .Where(x => x.Event == AuthCode.EventLabel.Login)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.Email && x.MethodValue == email)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == spec.Code)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (authCode == null) return BadRequest(Result.Error("Invalid Code"));

        var tokenSvc = scope.ServiceProvider.GetRequiredService<BcrTokenService>();
        BcrTokenResult token;
        try
        {
            token = await tokenSvc.GetUserTokenAsync(user, clientId: spec.GrantType);
        }
        catch (Exception e)
        {
            BcrLog.Slack($"GenerateTokenByAuthCode failed for user {user.Email} with error: {e.Message}");
            return BadRequest(Result.Error("Generate token failed."));
        }

        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        await ctx.SaveChangesAsync();

        if (!string.IsNullOrEmpty(token.AccessToken))
            ApplyTokenResponseHandler.AppendAccessTokenCookie(Response, token.AccessToken, token.AccessTokenLifetime, Request.IsHttps);

        return Ok(token);
    }


    private const int MaxSendEmailCount = 60;

    private async Task<bool> IsEmailValidToSendAsync(string email)
    {
        var key = $"auth_code_send_email:{email}";
        var value = await myCache.GetStringAsync(key);
        if (value == null)
        {
            await myCache.SetStringAsync(key, "1", TimeSpan.FromHours(1));
            return true;
        }

        if (int.TryParse(value, out var count) && count < MaxSendEmailCount)
        {
            await myCache.SetStringAsync(key, (count + 1).ToString(), TimeSpan.FromHours(1));
            return true;
        }

        return false;
    }
}