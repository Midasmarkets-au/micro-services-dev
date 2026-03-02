using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Permission;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Request;
using Bacera.Gateway.Web.Services;
using Hangfire;
using HashidsNet;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bacera.Gateway.Web.Controllers;

[Tags("User")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class UserController(
    UserManager<User> userManager,
    TenantDbContext tenantCtx,
    AuthDbContext authCtx,
    ConfigService cfgSvc,
    UserService userSvc,
    ITenantService tenantService,
    IBackgroundJobClient client,
    PermissionService permissionService)
    : BaseController
{
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { Message = "Pong" });


    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        long tenantId = GetTenantId(), partyId = GetPartyId();
        if (tenantId <= 0 || partyId <= 0) return Unauthorized();
        var tenant = await tenantService.GetAsync(tenantId);
        if (!tenant.IsNotEmpty()) return Unauthorized();

        // var result = UserResponseModel.Of(user);

        var user = await authCtx.Users.FirstOrDefaultAsync(x => x.PartyId == partyId && x.TenantId == tenantId);
        if (user == null) return NotFound();

        var roles = await authCtx.UserRoles.Where(x => x.UserId == user.Id)
            .Select(x => x.ApplicationRole.Name)
            .ToListAsync();

        // var userClaims = await authCtx.UserClaims.Where(x => x.UserId == user.Id).ToListAsync();

        // if (userClaims.Count == 0) return Ok(result);
        var webPermissionsTask = Task.Run(() => permissionService.GetUserWebPermissions(user.TenantId, user.PartyId));

        var configurations = await tenantCtx.Configurations
            .Where(x => x.Category == nameof(Party))
            .Where(x => x.RowId == user.PartyId)
            .ToClientMeViewModel()
            .ToListAsync();

        // if (userClaims.Count == 0) return Ok(result);
        var result = UserResponseModel.Of(user);

        result.TwoFactorEnabled = user.TwoFactorEnabled;
        result.Roles = roles.ToArray();
        result.NativeName = user.NativeName;

        result.Permissions = (await webPermissionsTask).ToArray();

        var accounts = await tenantCtx.Accounts
            .Where(x => x.PartyId == user.PartyId && x.Status == 0)
            .Select(x => new
            {
                x.Role, x.Uid, x.Group,
                Tags = x.Tags.Where(y => y.Name == AccountTagTypes.DefaultSalesAccount || y.Name == AccountTagTypes.DefaultAgentAccount)
                    .Select(y => y.Name),
            })
            .ToListAsync();

        if (accounts.Any(x => x.Group == "TMTM"))
        {
            configurations.Add(new Configuration.ClientMeViewModel
            {
                DataFormat = "bool",
                Key = "DisableTransfer",
                ValueString = "true",
            });
        }

        result.IbAccount = accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Agent)
            .Select(x => x.Uid.ToString())
            .ToArray();

        result.SalesAccount = accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .Select(x => x.Uid.ToString())
            .ToArray();

        result.RepAccount = accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Rep)
            .Select(x => x.Uid.ToString())
            .ToArray();

        var defaultAgentUid = accounts.Where(x => x.Role == (int)AccountRoleTypes.Agent)
            .Where(x => x.Tags.Contains(AccountTagTypes.DefaultAgentAccount))
            .Select(x => x.Uid)
            .SingleOrDefault();

        result.DefaultAgentAccount = defaultAgentUid == 0
            ? accounts.Where(x => x.Role == (int)AccountRoleTypes.Agent).Select(x => x.Uid.ToString()).FirstOrDefault() ?? "0"
            : defaultAgentUid.ToString();

        var defaultSalesUid = accounts.Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .Where(x => x.Tags.Contains(AccountTagTypes.DefaultSalesAccount))
            .Select(x => x.Uid)
            .SingleOrDefault();

        result.DefaultSalesAccount = defaultSalesUid == 0
            ? accounts.Where(x => x.Role == (int)AccountRoleTypes.Sales).Select(x => x.Uid.ToString()).FirstOrDefault() ?? "0"
            : defaultSalesUid.ToString();

        result.Configurations = configurations;
        result.Tenancy = user.TenantId switch
        {
            1 => "au",
            10000 => "bvi",
            10002 => "mn",
            10004 => "sea",
            10005 => "jp",
            _ => "bvi"
        };
        return Ok(result);
    }

    [HttpGet("me/refercode")]
    public async Task<IActionResult> MyReferralCode()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        return Ok(new { user.ReferCode });
    }

    [HttpPut("profile/language")]
    public async Task<IActionResult> SetLanguage(SetLanguageRequestModel spc)
    {
        long tenantId = GetTenantId(), partyId = GetPartyId();
        var task1 = Task.Run(async () =>
        {
            var email = await authCtx.Users
                .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync();

            var users = await authCtx.Users
                .Where(x => x.Email == email)
                .ToListAsync();

            users.ForEach(x =>
            {
                x.Language = spc.Language;
                x.UpdatedOn = DateTime.UtcNow;
            });

            await authCtx.SaveChangesAsync();
        });

        var task2 = Task.Run(async () =>
        {
            var party = await tenantCtx.Parties
                .Where(x => x.Id == partyId)
                .SingleAsync();

            party.Language = spc.Language;
            party.UpdatedOn = DateTime.UtcNow;
            await tenantCtx.SaveChangesAsync();
        });

        await Task.WhenAll(task1, task2);
        await userSvc.UpdateSearchAsync(new User.Criteria { TenantId = tenantId, PartyId = partyId });
        return Ok();
    }

    [HttpGet("address")]
    public async Task<IActionResult> UserAddressIndex([FromQuery] Address.Criteria? criteria)
    {
        criteria ??= new Address.Criteria();
        criteria.PartyId = GetPartyId();
        var addresses = await tenantCtx.Addresses
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return Ok(Result<List<Address.ClientPageModel>, Address.Criteria>.Of(addresses, criteria));
    }

    [HttpGet("address/{hashId}")]
    public async Task<IActionResult> GetUserAddress(string hashId)
    {
        var addressId = Address.HashDecode(hashId);
        var addresses = await tenantCtx.Addresses
            .ToClientDetailModel()
            .SingleOrDefaultAsync(x => x.Id == addressId);

        return addresses == null ? NotFound() : Ok(addresses);
    }

    [HttpPost("address")]
    public async Task<IActionResult> CreateUserAddress([FromBody] Address.CreateSpec spec)
    {
        var address = new Address
        {
            Name = spec.Name,
            CCC = spec.CCC,
            Phone = spec.Phone,
            Country = spec.Country,
            Content = spec.Content,
            PartyId = GetPartyId(),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        tenantCtx.Addresses.Add(address);
        await tenantCtx.SaveChangesAsync();
        return Ok(await tenantCtx.Addresses.ToClientDetailModel().SingleOrDefaultAsync(x => x.Id == address.Id));
    }

    [HttpPut("address/{hashId}")]
    public async Task<IActionResult> UpdateUserAddress(string hashId, [FromBody] Address.UpdateSpec spec)
    {
        var addressId = Address.HashDecode(hashId);
        var partyId = GetPartyId();
        var address = await tenantCtx.Addresses
            .SingleOrDefaultAsync(x => x.Id == addressId && x.PartyId == partyId);
        if (address == null) return NotFound();

        address.Name = spec.Name;
        address.CCC = spec.CCC;
        address.Phone = spec.Phone;
        address.Country = spec.Country;
        address.Content = spec.Content;
        address.UpdatedOn = DateTime.UtcNow;
        await tenantCtx.SaveChangesAsync();
        return Ok(await tenantCtx.Addresses.ToClientDetailModel().SingleOrDefaultAsync(x => x.Id == address.Id));
    }

    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProfileRequestModel))]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileRequestModel spec)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var task1 = Task.Run(async () =>
        {
            var partyId = GetPartyId();
            var party = await tenantCtx.Parties.SingleAsync(x => x.Id == partyId);
            party.Language = spec.Language ?? party.Language;
            party.TimeZone = spec.Timezone ?? party.TimeZone;
            party.UpdatedOn = DateTime.UtcNow;
            await tenantCtx.SaveChangesAsync();
        });

        var task2 = Task.Run(async () =>
        {
            user.Language = spec.Language ?? user.Language;
            user.TimeZone = spec.Timezone ?? user.TimeZone;
            user.UpdatedOn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        });

        await Task.WhenAll(task1, task2);

        return Ok(spec);
    }

    /// <summary>
    /// Send Enable 2fa code
    /// </summary>
    /// <returns></returns>
    [HttpPut("enable-2fa/code")]
    public async Task<IActionResult> Enable2FaCode()
    {
        var partyId = GetPartyId();
        var config = await cfgSvc.GetAsync<TwoFactorAuthSetting>(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        if (config?.LoginCodeEnabled == true) return BadRequest("2FA already enabled");
        var email = await tenantCtx.Parties.Where(x => x.Id == partyId).Select(x => x.Email).SingleAsync();
        client.Enqueue<IGeneralJob>(x => x.GenerateAuthCodeAndSendEmailAsync(GetTenantId(), email, AuthCode.EventLabel.TwoFactorEnable));
        return Ok("Auth code sent");
    }

    /// <summary>
    /// Confirm enable 2fa
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("enable-2fa/confirm")]
    public async Task<IActionResult> ConfirmEnable2Fa([FromBody] AuthCode.Update2FaSpec spec)
    {
        var partyId = GetPartyId();
        var email = await tenantCtx.Parties.Where(x => x.Id == partyId).Select(x => x.Email).SingleAsync();

        var authCode = await tenantCtx.AuthCodes
            .Where(x => x.Event == AuthCode.EventLabel.TwoFactorEnable)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.Email && x.MethodValue == email)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == spec.Code)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (authCode == null) return BadRequest("Invalid code");

        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        tenantCtx.AuthCodes.Update(authCode);
        await tenantCtx.SaveChangesAsync();

        // Get existing config to preserve other 2FA settings
        var existingConfig = await cfgSvc.GetAsync<TwoFactorAuthSetting>(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        var setting = EnsureTwoFactorAuthSettingComplete(existingConfig);
        setting.LoginCodeEnabled = true;

        await cfgSvc.SetAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting, setting);
        return Ok("2FA enabled");
    }

    /// <summary>
    /// Enable 2fa
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("enable-2fa")]
    public async Task<IActionResult> Enable2Fa([FromBody] AuthCode.Update2FaSpec spec)
    {
        var partyId = GetPartyId();
        var config = await cfgSvc.GetAsync<TwoFactorAuthSetting>(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        if (config == null)
        {
            var setting = EnsureTwoFactorAuthSettingComplete(null);
            setting.LoginCodeEnabled = true;

            await cfgSvc.SetAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting, setting);
            return Ok(Result.IsSuccessTure());
        }

        var user = await userSvc.GetPartyAsync(GetPartyId());
        if (spec.Code != null)
        {
            var codeValid = await userSvc.ValidateAuthCodeAsync(AuthCode.EventLabel.TwoFactorEnable
                , AuthCodeMethodTypes.Email
                , user.EmailRaw
                , spec.Code);

            if (!codeValid) return BadRequest(Result.IsSuccessFalse("Invalid code"));

            // Ensure all fields are present before updating
            config = EnsureTwoFactorAuthSettingComplete(config);
            config.LoginCodeEnabled = true;
            await cfgSvc.SetAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting, config);
            return Ok(Result.IsSuccessTure());
        }

        client.Enqueue<IGeneralJob>(x =>
            x.GenerateAuthCodeAndSendEmailAsync(GetTenantId(), user.EmailRaw, AuthCode.EventLabel.TwoFactorEnable));
        return Ok(Result.IsSuccessFalse("Auth code sent"));
    }

    /// <summary>
    /// Disable 2fa
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("disable-2fa")]
    public async Task<IActionResult> Disable2Fa([FromBody] AuthCode.Update2FaSpec spec)
    {
        var partyId = GetPartyId();
        var config = await cfgSvc.GetAsync<TwoFactorAuthSetting>(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        if (config == null)
        {
            var setting = new TwoFactorAuthSetting
            {
                LoginCodeEnabled = false,
            };

            await cfgSvc.SetAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting, setting);
            return Ok(Result.IsSuccessTure());
        }

        var user = await userSvc.GetPartyAsync(GetPartyId());
        if (spec.Code != null)
        {
            var codeValid = await userSvc.ValidateAuthCodeAsync(AuthCode.EventLabel.TwoFactorDisable
                , AuthCodeMethodTypes.Email
                , user.EmailRaw
                , spec.Code);

            if (!codeValid) return BadRequest(Result.IsSuccessFalse("Invalid code"));

            config.LoginCodeEnabled = false;
            await cfgSvc.SetAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting, config);
            return Ok(Result.IsSuccessTure());
        }

        client.Enqueue<IGeneralJob>(x =>
            x.GenerateAuthCodeAndSendEmailAsync(GetTenantId(), user.EmailRaw, AuthCode.EventLabel.TwoFactorDisable));
        return Ok(Result.IsSuccessFalse("Auth code sent"));
    }

    /// <summary>
    ///  Send Disable 2fa code
    /// </summary>
    /// <returns></returns>
    [HttpPut("disable-2fa/code")]
    public async Task<IActionResult> SendDisable2Fa()
    {
        var partyId = GetPartyId();
        var config = await cfgSvc.GetAsync<TwoFactorAuthSetting>(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        if (config == null) return BadRequest("2FA not set");
        var email = await tenantCtx.Parties.Where(x => x.Id == partyId).Select(x => x.Email).SingleAsync();
        client.Enqueue<IGeneralJob>(x => x.GenerateAuthCodeAndSendEmailAsync(GetTenantId(), email, AuthCode.EventLabel.TwoFactorDisable));
        return Ok("Auth code sent");
    }

    /// <summary>
    /// Confirm disable 2fa
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("disable-2fa/confirm")]
    public async Task<IActionResult> ConfirmDisable2Fa([FromBody] AuthCode.Update2FaSpec spec)
    {
        var partyId = GetPartyId();
        var config = await cfgSvc.GetAsync<TwoFactorAuthSetting>(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        if (config == null) return BadRequest("2FA not set");

        var email = await tenantCtx.Parties.Where(x => x.Id == partyId).Select(x => x.Email).SingleAsync();

        var authCode = await tenantCtx.AuthCodes
            .Where(x => x.Event == AuthCode.EventLabel.TwoFactorDisable)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.Email && x.MethodValue == email)
            .Where(x => x.Status == (short)AuthCodeStatusTypes.Valid)
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == spec.Code)
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();

        if (authCode == null) return BadRequest("Invalid code");

        authCode.Status = (short)AuthCodeStatusTypes.Invalid;
        tenantCtx.AuthCodes.Update(authCode);
        await tenantCtx.SaveChangesAsync();

        // Ensure all fields are present before updating
        config = EnsureTwoFactorAuthSettingComplete(config);
        config.LoginCodeEnabled = false;
        await cfgSvc.SetAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting, config);

        // await cfgSvc.DeleteAsync(nameof(Party), partyId, ConfigKeys.TwoFactorAuthSetting);
        return Ok("2FA disabled");
    }

    /// <summary>
    /// Ensures TwoFactorAuthSetting contains all fields with default values if missing.
    /// This helper method ensures backward compatibility and prevents data loss when updating settings.
    /// </summary>
    private static TwoFactorAuthSetting EnsureTwoFactorAuthSettingComplete(TwoFactorAuthSetting? existing)
    {
        return new TwoFactorAuthSetting
        {
            LoginCodeEnabled = existing?.LoginCodeEnabled ?? false,
            WalletToWalletTransfer = existing?.WalletToWalletTransfer ?? true,
            WalletToTradeAccount = existing?.WalletToTradeAccount ?? true,
            TradeAccountToTradeAccount = existing?.TradeAccountToTradeAccount ?? true,
            Withdrawal = existing?.Withdrawal ?? true,
        };
    }

    /// <summary>
    /// Add or Update Account Alias by uid
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("account/alias")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateAccountAlias([FromBody] Account.UpdateAliasSpec spec)
    {
        var partyId = GetPartyId();
        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == spec.Uid)
            .FirstOrDefaultAsync();
        if (account == null)
            return NotFound();

        var alias = await tenantCtx.AccountAliases
            .Where(x => x.AccountId == account.Id && x.PartyId == partyId)
            .FirstOrDefaultAsync();

        if (alias == null)
        {
            var accountAlias = new AccountAlias
            {
                Alias = spec.Alias,
                AccountId = account.Id,
                PartyId = partyId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            };
            tenantCtx.AccountAliases.Add(accountAlias);
        }
        else
        {
            alias.Alias = spec.Alias;
            alias.UpdatedOn = DateTime.UtcNow;
            tenantCtx.AccountAliases.Update(alias);
        }

        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }
}

public static class StringExtensions
{
    public static bool IsEmail(this string email) => EmailViewModel.IsValidReceiverEmail(email);
}