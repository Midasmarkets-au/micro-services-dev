using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.IPInfo;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Response;
using Bacera.Gateway.Web.Services;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PhoneNumbers;

namespace Bacera.Gateway.Web.Controllers.V2.Auth;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/auth")]
[Tags("Auth")]
public partial class AuthControllerV2(
    ILogger<AuthControllerV2> logger,
    UserManager<User> userMgr,
    ISmsVerification smsVerification,
    CentralDbContext centralCtx,
    IServiceProvider serviceProvider,
    IOptions<IPInfoOptions> options,
    IBackgroundJobClient backgroundJobClient,
    IHttpClientFactory clientFactory,
    MyDbContextPool myDbContextPool,
    IMyCache myCache,
    UserService userService)
    : BaseControllerV2
{
    [AllowAnonymous]
    [HttpGet("/api/" + VersionTypes.V2 + "/domain")]
    public async Task<IActionResult> Index()
    {
        var value = await centralCtx.CentralConfigs
            .Where(x => x.Key == ConfigKeys.ApiDomain)
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == 0)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();

        if (value == null) return NotFound();
        return Ok(JsonConvert.DeserializeObject(value));
    }

    [AllowAnonymous]
    [HttpGet("/api/" + VersionTypes.V2 + "/welcome")]
    public async Task<IActionResult> Welcome()
    {
        var value = await centralCtx.CentralConfigs
            .Where(x => x.Key == ConfigKeys.WelcomeInfo)
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == 0)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();

        if (value == null) return NotFound();
        return Ok(JsonConvert.DeserializeObject(value));
    }

    [AllowAnonymous]
    [HttpGet("ip-info")]
    public async Task<IActionResult> IpInfo() => Ok(await GetIpInfo());

    [AllowAnonymous]
    [HttpGet("c")]
    public async Task<IActionResult> Configuration([FromQuery] string? openAt)
        => Ok(new List<object>
        {
            await GetSite(openAt),
        });

    // [Migrated to auth Rust service: POST /api/v2/auth/register]
    // [AllowAnonymous]
    // [HttpPost("register")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<IActionResult> Register([FromBody] RegistrationRequest model)
    // {
    //     var ip = GetRemoteIpAddress();
    //     var inBlackList = await centralCtx.IpBlackLists.AnyAsync(x => x.Ip == ip);
    //     if (inBlackList)
    //     {
    //         logger.LogWarning("IP {Ip} is in black list", ip);
    //         return BadRequest(ToErrorResult(ResultMessage.Register.RegisterFail));
    //     }
    //
    //     var ipInfo = await GetIpInfo(ip);
    //     var tenantIdFromIp = Tenancy.GetTenantIdByCountryCode(ipInfo.Country);
    //
    //     model.ReferCode = ReferCodeRegex().Replace(model.ReferCode.Trim().ToUpper(), "");
    //     
    //     var tenant = await centralCtx.CentralReferralCodes
    //                      .Where(x => x.Code == model.ReferCode)
    //                      .Select(x => x.Tenant)
    //                      .FirstOrDefaultAsync()
    //                  ?? await centralCtx.Tenants.FirstOrDefaultAsync(x => x.Id == model.TenantId)
    //                  ?? await centralCtx.Tenants.FirstOrDefaultAsync(x => x.Id == tenantIdFromIp)
    //                  ?? await centralCtx.Tenants.FirstAsync(x => x.Id == 10000);
    //
    //     using var scope = serviceProvider.CreateTenantScope(tenant.Id);
    //     
    //     var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
    //     var userService = scope.ServiceProvider.GetRequiredService<UserService>();
    //
    //     var scopedCfgSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
    //
    //     var email = model.Email.Trim().ToLower();
    //     var existedUsers = await userMgr.Users.Where(x => x.Email == model.Email.Trim().ToLower()).ToListAsync();
    //     if (existedUsers.Any(x => x.TenantId != tenant.Id))
    //     {
    //         logger.LogWarning("User with email already exists in another tenant, {Email}", email);
    //         return Conflict(ToErrorResult(ResultMessage.Register.AlreadyRegisterOtherSite));
    //     }
    //
    //     if (existedUsers.Any(x => x.TenantId == tenant.Id))
    //     {
    //         logger.LogWarning("User with email {Email} already exists", email);
    //         return BadRequest(ToErrorResult(ResultMessage.Register.EmailExists));
    //     }
    //
    //     var password = model.Password.Trim();
    //     var passwordValidator = new PasswordValidator<User>();
    //     var result = await passwordValidator.ValidateAsync(userMgr, new User(), password);
    //     if (!result.Succeeded)
    //     {
    //         return BadRequest(ToErrorResult(ResultMessage.Register.RegisterFail,
    //             result.Errors.Select(x => x.Description).ToList()));
    //     }
    //
    //     
    //
    //     // verify OTP
    //     var phoneNumberVerified = false;
    //     var combinedPhoneNumber = "+" + model.CCC + model.Phone;
    //     switch (model.CCC)
    //     {
    //         case "852" when model.Phone.Length != 8:
    //             return BadRequest(Result.Error(ResultMessage.Verification.PhoneNumberInvalid));
    //         case "61" when tenant.Id != 1:
    //             break;
    //         // temporary disable au phone number validation for bvi 
    //         // return BadRequest(Result.Error(ResultMessage.Verification.PhoneNumberInvalid));
    //     }
    //     
    //     //********************************************************************
    //     // Phone Number Disable on 2025/03/27 for APP Apple IOS Verification
    //     //********************************************************************
    //     //
    //     // if (!IsValidPhoneNumber(combinedPhoneNumber))
    //     //     return BadRequest(Result.Error(ResultMessage.Verification.PhoneNumberInvalid));
    //     //
    //     // if (!PhoneNumberRegionCodeTypes.All.Contains(model.CCC))
    //     //     return BadRequest(Result.Error(ResultMessage.Verification.RegionCodeInvalid));
    //     
    //     var smsEnable = await scopedCfgSvc.GetSmsVerificationToggleSwitchAsync();
    //     if (smsEnable)
    //     {
    //         try
    //         {
    //             var (otpResult, formattedPhoneNumber) = await smsVerification.VerificationCheck(combinedPhoneNumber, model.Otp);
    //             if (otpResult == false) return BadRequest(Result.Error(ResultMessage.Verification.VerificationFail));
    //             phoneNumberVerified = true;
    //         }
    //         catch (Exception ex)
    //         {
    //             logger.LogWarning("Check SMS Verification Code Error : {Message}", ex.Message);
    //             return BadRequest(Result.Error(ex.Message));
    //         }
    //     }
    //
    //     var referralCode = await tenantCtx.ReferralCodes
    //         .Where(x => x.Code == model.ReferCode)
    //         .ToResponse()
    //         .FirstOrDefaultAsync();
    //
    //     var centralParty = new CentralParty
    //     {
    //         Email = email,
    //         SiteId = referralCode?.Summary?.siteId ?? model.SiteId ?? await GetSite(),
    //         Name = model.FirstName + " " + model.LastName,
    //         NativeName = model.FirstName + " " + model.LastName,
    //         Code = "",
    //         Note = "",
    //         Uid = await userService.GeneratePartyUidAsync(),
    //         TenantId = tenant.Id,
    //         CreatedOn = DateTime.UtcNow,
    //     };
    //
    //     if (centralParty.SiteId == 0) centralParty.SiteId = (int)SiteTypes.BritishVirginIslands;
    //
    //     await centralCtx.CentralParties.AddAsync(centralParty);
    //     await centralCtx.SaveChangesAsync();
    //     //
    //     var party = centralParty.ToParty();
    //     await tenantCtx.PartyRoles.AddAsync(new PartyRole { Party = party, RoleId = (int)UserRoleTypes.Client });
    //     await tenantCtx.SaveChangesAsync();
    //     //
    //     var uid = await Utils.GenerateUniqueIdAsync(uid => userMgr.Users.AnyAsync(x => x.Uid == uid));
    //     var user = Gateway.Auth.User
    //         .Create(email)
    //         .SetUid(uid)
    //         .Party(party.Id)
    //         .Tenant(tenant.Id)
    //         .SetName(model.FirstName, model.LastName)
    //         .SetCcc(model.CCC)
    //         .SetPhoneNumber(model.Phone)
    //         .SetCurrency(model.Currency)
    //         .SetReferCode(model.ReferCode)
    //         .SetCountryCode(model.CountryCode)
    //         .Ip(ipInfo.Ip)
    //         .SetLanguage(model.Language);
    //
    //     user.PhoneNumberConfirmed = phoneNumberVerified;
    //     user.EmailConfirmed = true;
    //
    //     try
    //     {
    //         result = await userMgr.CreateAsync(user, password);
    //
    //         if (!result.Succeeded)
    //         {
    //             var descriptions = result.Errors.Select(x => x.Description).ToList();
    //             BcrLog.Slack(
    //                 $"User register with email: {user.Email!.ToLower()} failed, message: {string.Join(", ", descriptions)}");
    //             return BadRequest(ToErrorResult(ResultMessage.Register.RegisterFail,
    //                 result.Errors.Select(x => x.Description).ToList()));
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         BcrLog.Slack($"User register with email: {user.Email!.ToLower()} failed, message: {e.Message}");
    //         return BadRequest(ToErrorResult(ResultMessage.Register.RegisterFail));
    //     }
    //
    //
    //     user.ApplyToParty(ref party);
    //     tenantCtx.Parties.Update(party);
    //     await tenantCtx.SaveChangesAsync();
    //
    //     await TryAssignReferCode(user, tenantCtx);
    //     await userMgr.AddClaimsAsync(user, new List<Claim>
    //     {
    //         new(UserClaimTypes.PartyId, party.Id.ToString()),
    //         new(UserClaimTypes.TenantId, tenant.Id.ToString()),
    //     });
    //     await userMgr.AddToRoleAsync(user, UserRoleTypesString.Guest);
    //     email = await userMgr.GetEmailAsync(user);
    //     var code = await userMgr.GenerateEmailConfirmationTokenAsync(user);
    //     code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    //     var callbackUrl = $"{model.ConfirmUrl}?email={HttpUtility.UrlEncode(email)}&code={code}";
    //
    //     var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    //     await mediator.Publish(new UserRegisteredEvent(user, password, callbackUrl, model.SourceComment,
    //         utm: Request.Cookies["utm"]));
    //     return NoContent();
    // }

    [AllowAnonymous]
    [HttpPost("password/forgot")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        request.Email = request.Email.ToLower().Trim();
        if (!request.Email.IsEmail())
        {
            logger.LogWarning("Invalid email {Email}", request.Email);
            return BadRequest(ToErrorResult(ResultMessage.Register.InvalidEmail));
        }

        var users = await userMgr.Users
            .Where(x => x.Email == request.Email && x.Status == 0 && x.EmailConfirmed)
            .OrderBy(x => x.TenantId)
            .ToListAsync();

        var user = users.FirstOrDefault();
        if (user == null || !await userMgr.IsEmailConfirmedAsync(user))
        {
            // Don't reveal that the user does not exist or is not confirmed
            logger.LogWarning("User with email {Email} not found", request.Email);
            return NoContent();
        }

        var tenant = await centralCtx.Tenants.FirstOrDefaultAsync(x => x.Id == user.TenantId);
        if (tenant == null)
        {
            logger.LogWarning("Tenant with id {TenantId} not found", user.TenantId);
            return NoContent();
        }

        var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenant.Id);

        var code = await userMgr.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = $"{request.ResetUrl}?code={code}";
        var jobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        var model = new ResetPasswordViewModel(user.Email!, user.GuessUserName(), callbackUrl);
        jobClient.Enqueue<IGeneralJob>(x => x.ResetPasswordAsync(tenant.Id, model, user.Language));
        return NoContent();
    }

    [HttpPost("password/change")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel spec)
    {
        var user = await userMgr.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest(ToErrorResult(ResultMessage.Common.UserNotFound));
        }

        var result = await userMgr.CheckPasswordAsync(user, spec.CurrentPassword);
        if (!result)
        {
            return BadRequest(ToErrorResult(ResultMessage.Register.ChangePasswordFail));
        }

        var email = user.Email?.Trim().ToLower() ?? string.Empty;
        var users = await userMgr.Users.Where(x => x.Email == email).ToListAsync();
        foreach (var uu in users)
        {
            var token = await userMgr.GeneratePasswordResetTokenAsync(uu);
            var resul = await userMgr.ResetPasswordAsync(uu, token, spec.NewPassword);
            if (!resul.Succeeded)
            {
                logger.LogWarning("User {Uid} change password failed", resul.Errors.Select(x => x.Description));
            }
        }

        await userService.RecordPasswordChangeAuditAsync(user.PartyId, user.Id);
        return NoContent();
    }

    [HttpPost("password/check")]
    public async Task<IActionResult> Check([FromQuery] string email, [FromQuery] string password)
    {
        if (!AppEnvironment.IsDevelopment())
            return StatusCode(403, "Not available");

        var operatorUser = await userMgr.GetUserAsync(User);
        if (operatorUser is null || !operatorUser.UserRoles.Any(r => r.RoleId == (long)UserRoleTypes.SuperAdmin))
        {
            return BadRequest("No permission");
        }

        var users = await userMgr.Users.Where(x => x.Email == email).ToListAsync();
        var result = new List<dynamic>();
        foreach (var user in users)
        {
            var check = await userMgr.CheckPasswordAsync(user, password);
            result.Add(new { user.TenantId, check });
        }

        result = result.OrderBy(x => x.TenantId).ToList();
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("password/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordByTokenRequest data)
    {
        data.Email = data.Email.ToLower().Trim();
        if (!data.Email.IsEmail())
        {
            logger.LogWarning("Invalid email {Email}", data.Email);
            return BadRequest(ToErrorResult(ResultMessage.Register.InvalidEmail));
        }

        string code;
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(data.Code));
        }
        catch (Exception e)
        {
            logger.LogWarning("Token code invalid. Error: {@Error}. Request {@Request}", e.Message, data);
            return BadRequest(ToErrorResult(ResultMessage.Register.InvalidToken));
        }

        var users = await userMgr.Users
            .Where(x => x.Email == data.Email && x.Status == 0 && x.EmailConfirmed)
            .OrderBy(x => x.TenantId)
            .ToListAsync();
        if (!users.Any())
        {
            logger.LogWarning("User with email {Email} not found", data.Email);
            return NoContent();
        }

        var validateResult = false;
        foreach (var user in users)
        {
            validateResult = await userMgr.VerifyUserTokenAsync(user
                , userMgr.Options.Tokens.PasswordResetTokenProvider
                , "ResetPassword"
                , code);
            if (validateResult) break;
        }

        if (!validateResult)
        {
            return BadRequest(ToErrorResult(ResultMessage.Register.InvalidToken));
        }

        foreach (var user in users)
        {
            var token = await userMgr.GeneratePasswordResetTokenAsync(user);
            await userMgr.ResetPasswordAsync(user, token, data.Password);
            await userService.RecordPasswordChangeAuditAsync(user.PartyId, user.Id);
        }

        return NoContent();
    }

    private static async Task TryAssignReferCode(User user, TenantDbContext ctx)
    {
        if (string.IsNullOrEmpty(user.ReferCode)) return;

        var referCode = await ctx.ReferralCodes
            .SingleOrDefaultAsync(x => x.Code.Equals(user.ReferCode));
        if (referCode == null) return;

        var userParty = await ctx.Parties.SingleOrDefaultAsync(x => x.Id == user.PartyId);
        if (userParty == null || userParty.Pid > 0 || userParty.Id == referCode.PartyId) return;

        userParty.Pid = referCode.PartyId;
        ctx.Parties.Update(userParty);
        await ctx.SaveChangesAsync();

        var referral = new Referral
        {
            ReferralCodeId = referCode.Id,
            ReferrerPartyId = referCode.PartyId,
            ReferredPartyId = user.PartyId,
            Code = referCode.Code,
            CreatedOn = DateTime.UtcNow,
            Module = nameof(Gateway.Auth.User),
            RowId = user.Id,
        };
        await ctx.Referrals.AddAsync(referral);
        await ctx.SaveChangesAsync();
    }

    private async Task<int> GetSite(string? openAt = null) => openAt == null
        ? GetSiteId((await GetIpInfo()).Country)
        : openAt.ToUpper() switch
        {
            "BVI" => 1,
            "BA" => 2,
            "CN" => 3,
            "TW" => 4,
            "VN" => 5,
            "SEA" => 5,
            "JP" => 6,
            "MY" => 8,
            _ => 1
        };

    private IServiceScope CreateTenantScopeByTenantIdAsync(long tenantId)
    {
        var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenantId);
        return scope;
    }

    private async Task<IpInfoViewModel> GetIpInfo(string? ip = null)
    {
        ip ??= GetRemoteIpAddress();
        string endpoint = options.Value.Endpoint, token = options.Value.Token;
        endpoint = endpoint.EndsWith('/') ? endpoint : $"{endpoint}/";
        // var client = new HttpClient { BaseAddress = new Uri(endpoint), Timeout = TimeSpan.FromSeconds(5) };
        var client = clientFactory.CreateClient();
        client.BaseAddress = new Uri(endpoint);
        client.Timeout = TimeSpan.FromSeconds(5);
        var response = await client.GetAsync($"{endpoint}{ip}?token={token}");
        var data = await response.Content.ReadAsStringAsync();
        try
        {
            var result = JsonConvert.DeserializeObject<IpInfoViewModel>(data) ?? new IpInfoViewModel();
            result.Ips = GetIps();
            return result;
        }
        catch
        {
            return new IpInfoViewModel();
        }
    }

    private static int GetSiteId(string country) => country switch
    {
        "CN" => 3,
        "TW" => 4,
        "VN" => 5,
        "JP" => 6,
        "MN" => 7,
        "MY" => 8,
        "AU" => 2,
        "VG" => 1,
        _ => 0
    };

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        PhoneNumber phone;
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            phone = util.ParseAndKeepRawInput(phoneNumber, null);
        }
        catch (Exception)
        {
            return false;
        }

        return util.IsValidNumber(phone);
    }

    [GeneratedRegex("[^A-Z0-9]")]
    private static partial Regex ReferCodeRegex();
}