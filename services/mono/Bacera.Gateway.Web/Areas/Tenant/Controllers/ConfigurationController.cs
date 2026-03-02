using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Configuration")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ConfigurationController(
    ConfigurationService configurationService,
    ConfigService configSvc,
    TenantDbContext tenantDbContext,
    IMyCache myCache,
    TradingService tradingSvc)
    : TenantBaseController
{
    [HttpGet("sites")]
    public async Task<IActionResult> Sites()
    {
        var results = await tenantDbContext.Sites
            .ToListAsync();
        return Ok(results);
    }

    private IQueryable<Configuration> GetQuery(string category)
        => tenantDbContext.Configurations.Where(x => x.Category.ToLower() == category.ToLower());

    private Task<Configuration.TenantViewModel?> GetTenantViewModelAsync(string category, long rowId, string key)
        => GetQuery(category)
            .Where(x => x.RowId == rowId && x.Key == key)
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

    [HttpGet]
    public async Task<IActionResult> Query([FromQuery] Configuration.Criteria? criteria = null)
    {
        criteria ??= new Configuration.Criteria();
        var items = await tenantDbContext.Configurations
            .PagedFilterBy(criteria)
            .ToTenantViewModel()
            .ToListAsync();

        return Ok(Result<List<Configuration.TenantViewModel>, Configuration.Criteria>.Of(items, criteria));
    }

    [ServiceFilter(typeof(ConfigurationFilter))]
    [HttpGet("{category}/{rowId:long}")]
    public async Task<IActionResult> Index(string category, long rowId)
    {
        if (category.Equals(ConfigCategoryTypes.Account, StringComparison.CurrentCultureIgnoreCase))
            return Ok(await GetAccountConfigurations(rowId));

        if (category.Equals(ConfigCategoryTypes.Party, StringComparison.CurrentCultureIgnoreCase))
            return Ok(await GetPartyConfigurations(rowId));

        if (category.Equals(ConfigCategoryTypes.Public, StringComparison.CurrentCultureIgnoreCase))
            return Ok(await GetPublicConfigurations(rowId));

        return Ok(new List<Configuration.TenantViewModel>());
    }
    // => category.ToUpper() switch
    // {
    //     nameof(Account) => Ok(await GetAccountConfigurations(rowId)),
    //     nameof(Party) => Ok(await GetPartyConfigurations(rowId)),
    //     nameof(Public) => Ok(await GetPublicConfigurations(rowId)),
    //     _ => Ok(new List<Configuration.TenantViewModel>()),
    // };

    [ServiceFilter(typeof(ConfigurationFilter))]
    [HttpGet("{category}/{rowId:long}/{key}")]
    public async Task<IActionResult> Detail(string category, long rowId, string key, [FromQuery] bool? isInherit = false)
    {
        var result = await GetTenantViewModelAsync(category, rowId, key);

        if (isInherit == true && result == null)
        {
            if (category.Equals(nameof(Public), StringComparison.CurrentCultureIgnoreCase) ||
                category.Equals(nameof(Party), StringComparison.CurrentCultureIgnoreCase))
                return Ok(await GetTenantViewModelAsync(nameof(Public), 0, key));

            if (string.Equals(category, nameof(Account), StringComparison.CurrentCultureIgnoreCase))
            {
                var account = await tenantDbContext.Accounts
                    .Where(x => x.Id == rowId)
                    .Select(x => new { x.PartyId, x.SiteId })
                    .SingleOrDefaultAsync();

                if (account == null) return Ok(await GetTenantViewModelAsync(nameof(Public), 0, key));

                result = await GetTenantViewModelAsync(nameof(Party), account.PartyId, key);
                if (result != null) return Ok(result);

                result = await GetTenantViewModelAsync(nameof(Public), account.SiteId, key);
                if (result != null) return Ok(result);

                return Ok(await GetTenantViewModelAsync(nameof(Public), 0, key));
            }
        }

        return Ok(result);
    }

    [ServiceFilter(typeof(ConfigurationFilter))]
    [HttpPut("{category}/{rowId:long}/{key}")]
    public async Task<IActionResult> Update(string category, long rowId, string key,
        [FromBody] Configuration.CreateSpec spec)
    {
        await configSvc.SetAsync(ConfigCategoryTypes.ParseCategory(category), rowId, key, spec.Value, GetPartyId());
        var item = await configSvc.GetAsync<object>(ConfigCategoryTypes.ParseCategory(category), rowId, key);
        return Ok(item);
        // var item = await GetQuery(category)
        //     .Where(x => x.RowId == rowId)
        //     .Where(x => x.Key == key)
        //     .FirstOrDefaultAsync() ?? new Configuration();
        //
        // item.Category = ConfigCategoryTypes.ParseCategory(category);
        // item.RowId = rowId;
        // item.Key = key;
        //
        // item.Name = spec.Name;
        // item.Value = JsonConvert.SerializeObject(spec.Value);
        // item.DataFormat = spec.DataFormat;
        // item.Description = spec.Description;
        // item.UpdatedOn = DateTime.UtcNow;
        // item.UpdatedBy = GetPartyId();
        //
        // if (!item.Validate())
        //     return BadRequest(ResultMessage.Common.InvalidDataFormat);
        //
        // if (item.Id == 0) tenantDbContext.Configurations.Add(item);
        // else tenantDbContext.Configurations.Update(item);
        //
        // await tenantDbContext.SaveChangesAsync();
        // await configurationService.ResetCacheAsync();
        // if (category.Equals("account", StringComparison.CurrentCultureIgnoreCase))
        // {
        //     await tradingSvc.AddAccountLogAsync(rowId, "UpdateAccountConfig", "",
        //         $"{spec}", GetPartyId());
        // }
        //
        // var hkey = CacheKeys.GetConfigHashKey(GetTenantId());
        // var field = $"{category}:{rowId}:{key}";
        // await myCache.HSetStringAsync(hkey, field, JsonConvert.SerializeObject(spec.Value), TimeSpan.FromHours(12));

        // return Ok(item);
    }

    [ServiceFilter(typeof(ConfigurationFilter))]
    [HttpDelete("{category}/{rowId:long}/{key}")]
    public async Task<IActionResult> Delete(string category, long rowId, string key)
    {
        await Task.Delay(0);
        return BadRequest("Delete is not supported right now");
    }

    private async Task<List<Configuration.TenantViewModel>> GetAccountConfigurations(long id)
    {
        var account = await tenantDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (account == null)
            return [];

        var keysConfig = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == 0)
            .Where(x => x.Key == "AccountConfigurationList")
            .FirstOrDefaultAsync();
        if (keysConfig == null)
            return [];

        List<string>? keys;
        try
        {
            keys = JsonConvert.DeserializeObject<List<string>>(keysConfig.Value);
            if (keys == null)
                return [];
        }
        catch
        {
            return [];
        }

        var results = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => x.RowId == id)
            .Where(x => keys.Contains(x.Key))
            .ToTenantViewModel()
            .ToListAsync();

        if (keys.All(x => results.Any(r => r.Key == x)))
            return results;

        var missingKeysInAccount = keys.Where(x => results.All(r => r.Key != x)).ToList();

        var partyResults = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Party))
            .Where(x => x.RowId == account.PartyId)
            .Where(x => missingKeysInAccount.Contains(x.Key))
            .ToTenantViewModel()
            .ToListAsync();
        results.AddRange(partyResults);

        if (keys.All(x => results.Any(r => r.Key == x)))
            return results;

        var missingKeysInParty = keys.Where(x => results.All(r => r.Key != x)).ToList();
        var publicResults = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == 0)
            .Where(x => missingKeysInParty.Contains(x.Key))
            .ToTenantViewModel()
            .ToListAsync();

        results.AddRange(publicResults);
        return results;
    }

    private async Task<List<Configuration.TenantViewModel>> GetPartyConfigurations(long id)
    {
        return await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Party))
            .Where(x => x.RowId == id)
            .ToTenantViewModel()
            .ToListAsync();
    }

    private async Task<List<Configuration.TenantViewModel>> GetPublicConfigurations(long id)
    {
        return await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == id)
            .ToTenantViewModel()
            .ToListAsync();
    }

    // Old!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Do not touch !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // Old!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Do not touch !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// <summary>
    /// Update default email
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/default-email-address")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.IntValue))]
    public async Task<IActionResult> SetDefaultEmailAddress(int siteId,
        [FromBody] ApplicationConfigure.StringValue spec)
    {
        var result = await configurationService.SetDefaultEmailAddressAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update default email display name
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/default-email-display-name")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.IntValue))]
    public async Task<IActionResult> SetDefaultEmailDisplayName(int siteId,
        [FromBody] ApplicationConfigure.StringValue spec)
    {
        var result = await configurationService.SetDefaultEmailDisplayNameAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update default fund type
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/default-fund-type")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.IntValue))]
    public async Task<IActionResult> SetDefaultFundType(int siteId, [FromBody] ApplicationConfigure.IntValue spec)
    {
        var exists = await tenantDbContext.FundTypes.AnyAsync(x => x.Id == spec.Value);
        if (!exists)
            return BadRequest(Result.Error(ResultMessage.Transaction.FundTypeNotMatched));

        var result = await configurationService.SetDefaultFundTypeAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update default trading service
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/default-trade-service")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.IntValue))]
    public async Task<IActionResult> SetDefaultTradeService(int siteId, [FromBody] ApplicationConfigure.IntValue spec)
    {
        var exists = await tenantDbContext.TradeServices.AnyAsync(x => x.Id == spec.Value);
        if (!exists)
            return BadRequest(Result.Error(ResultMessage.Application.TradeServiceNotFount));

        var result = await configurationService.SetDefaultTradeServiceAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update high dollar value
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/high-dollar-value")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.IntValue))]
    public async Task<IActionResult> SetHighDollarValue(int siteId, [FromBody] ApplicationConfigure.IntValue spec)
    {
        var result = await configurationService.SetHighDollarValueAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Get high dollar value
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet("site/{siteId:int}/high-dollar-value")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.IntValue))]
    public async Task<IActionResult> GetHighDollarValue(int siteId)
    {
        var result = await configurationService.GetHighDollarValueAsync(siteId);
        return Ok(new { value = result });
    }

    /// <summary>
    /// Update Verification Quiz enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/verification-quiz-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> VerificationQuiz(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetVerificationQuizToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Quiz Fail Lock Enabled
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/quiz-fail-lock-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> QuizFailLockEnabled(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetQuizFailLockEnabledToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update SMS Verification enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/sms-Validation-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> SmsVerification(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetSmsValidationToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update IB enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/ib-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> Ib(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetIbToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Rebate enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/rebate-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> Rebate(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetRebateToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Account Daily Report
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/account-daily-report")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> AccountDailyReport(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetAccountDailyReportToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Rebate Calculate From
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/rebate-calculate-from")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> RebateCalculateFrom(int siteId, [FromBody] ApplicationConfigure.DateTimeValue spec)
    {
        var result = await configurationService.SetRebateCalculateFromAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Multiple Site enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/multiple-site-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> MultipleSite(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetMultipleSiteEnabledToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Auto Confirm Email enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/auto-confirm-email-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> AutoConfirmEmail(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetAutoConfirmEmailEnabledToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update Web Trader enable
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/web-trader-enabled")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.BoolValue))]
    public async Task<IActionResult> WebTraderEnable(int siteId, [FromBody] ApplicationConfigure.BoolValue spec)
    {
        var result = await configurationService.SetWebTraderToggleSwitchAsync(spec.Value, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update account type available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/account-type-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> AccountTypeAvailable(int siteId, [FromBody] List<int> spec)
    {
        var result = await configurationService.SetAccountTypeAvailableAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update currency type available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/currency-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> CurrencyAvailable(int siteId, [FromBody] List<int> spec)
    {
        var exists = await tenantDbContext.Currencies.Where(x => spec.Contains(x.Id)).Select(x => x.Id).ToListAsync();
        var result = await configurationService.SetCurrencyAvailableAsync(exists, siteId, GetPartyId());
        return result ? Ok(exists) : BadRequest();
    }

    /// <summary>
    /// Update trading platform available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/trading-platform-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> TradingPlatformAvailable(int siteId, [FromBody] List<int> spec)
    {
        var result = await configurationService.SetTradingPlatformAvailableAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update trading platform available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/demo-trading-platform-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> DemoTradingPlatformAvailable(int siteId, [FromBody] List<int> spec)
    {
        var result = await configurationService.SetDemoTradingPlatformAvailableAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update leverage available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/leverage-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> LeverageAvailable(int siteId, [FromBody] List<int> spec)
    {
        var result = await configurationService.SetLeverageAvailableAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update leverage for wholesale available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/leverage-for-wholesale-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> LeverageForWholesaleAvailable(int siteId, [FromBody] List<int> spec)
    {
        var result = await configurationService.SetLeverageForWholesaleAvailableAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Update fund type available
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/fund-type-available")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> FundAvailable(int siteId, [FromBody] List<int> spec)
    {
        spec = spec.Distinct().ToList();
        var exists = await tenantDbContext.FundTypes.Where(x => spec.Contains(x.Id)).Select(x => x.Id).ToListAsync();
        if (exists.Count != spec.Count)
            return BadRequest(Result.Error(ResultMessage.Transaction.FundTypeNotMatched));

        var result = await configurationService.SetFundTypeAvailableAsync(spec, siteId, GetPartyId());
        return result ? Ok(exists) : BadRequest();
    }

    /// <summary>
    /// Update contact info
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/contact-info")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> UpdateContactInfo(int siteId, [FromBody] Dictionary<string, string> spec)
    {
        var result = await configurationService.SetContactInfoAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Add or Update cheater ip
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/cheater-ip")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> CreateCheaterIp(int siteId, [FromBody] KeyValuePair<string, string> spec)
    {
        var items = await configurationService.GetCheaterIpAsync(siteId);
        foreach (var item in items.Where(item => item.Key == spec.Key))
        {
            items.Remove(item.Key);
        }

        items.Add(spec.Key, spec.Value);

        var result = await configurationService.SetCheaterIpAsync(items, siteId, GetPartyId());
        return result ? Ok(items) : BadRequest();
    }

    /// <summary>
    /// Get Default Rebate Level Setting
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet("site/{siteId:int}/default-rebate-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>))]
    public async Task<IActionResult> GetDefaultLevelSetting(int siteId)
    {
        var result = await configurationService.GetDefaultRebateLevelSettingAsync(siteId);
        return Ok(result);
    }

    /// <summary>
    /// Add or Update Default Rebate Level Setting
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/default-rebate-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<IActionResult> UpdateDefaultLevelSetting(int siteId,
        [FromBody] Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>> spec)
    {
        var result = await configurationService.SetDefaultRebateLevelSettingAsync(spec, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// remove cheater ip
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    [HttpDelete("site/{siteId:int}/cheater-ip/{ip}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> RemoveCheaterIp(int siteId, string ip)
    {
        var items = await configurationService.GetCheaterIpAsync(siteId);
        var count = items.Count;
        foreach (var item in items.Where(item => item.Key == ip))
        {
            items.Remove(item.Key);
        }

        if (count == items.Count)
        {
            return BadRequest(ResultMessage.Common.RecordNotFound);
        }

        var result = await configurationService.SetCheaterIpAsync(items, siteId, GetPartyId());
        return result ? Ok(items) : BadRequest();
    }

    /// <summary>
    /// Add or Update ip setting
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/ip-setting")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> CreateIpSetting(int siteId, [FromBody] IpSetting spec)
    {
        var items = await configurationService.GetIpSettingAsync(siteId);
        foreach (var item in items
                     .Where(item => item.Ip == spec.Ip && item.Type == spec.Type)
                     .ToList())
        {
            items.Remove(item);
        }

        items.Add(spec);

        var result = await configurationService.SetIpSettingAsync(items, siteId, GetPartyId());
        return result ? Ok(items) : BadRequest();
    }

    /// <summary>
    /// remove ip setting
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="ip"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpDelete("site/{siteId:int}/ip-setting/{ip}/type/{type:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
    public async Task<IActionResult> RemoveIpSetting(int siteId, string ip, int type)
    {
        var items = await configurationService.GetIpSettingAsync(siteId);
        var count = items.Count;
        foreach (var item in items
                     .Where(item => item.Ip == ip && item.Type == (IpSettingTypes)type)
                     .ToList())
        {
            items.Remove(item);
        }

        if (count == items.Count)
        {
            return BadRequest(ResultMessage.Common.RecordNotFound);
        }

        var result = await configurationService.SetIpSettingAsync(items, siteId, GetPartyId());
        return result ? Ok(items) : BadRequest();
    }

    /// <summary>
    /// Offset check full list
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet("site/{siteId:int}/offset-check")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LegacyOffsetCheck>))]
    public async Task<IActionResult> OffsetCheckList(int siteId)
    {
        var result = await configurationService.GetOffsetCheckListAsync(siteId);
        return Ok(result);
    }

    /// <summary>
    /// Add an offset check 
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("site/{siteId:int}/offset-check")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegacyOffsetCheck))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOffsetCheck(int siteId, LegacyOffsetCheck spec)
    {
        var offsetChecks = await configurationService.GetOffsetCheckListAsync(siteId);
        var maxId = offsetChecks.Any() ? offsetChecks.Max(x => x.Id) : 0;
        spec.Id = maxId + 1;
        offsetChecks.Add(spec);
        var result = await configurationService.UpdateOffsetChecksAsync(offsetChecks, siteId, GetPartyId());
        return result ? Ok(spec) : BadRequest();
    }

    /// <summary>
    /// Delete an offset check 
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("site/{siteId:int}/offset-check/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteOffsetCheck(int siteId, long id)
    {
        var offsetChecks = await configurationService.GetOffsetCheckListAsync(siteId);
        var item = offsetChecks.FirstOrDefault(x => x.Id == id);
        if (item == null) return NotFound();
        offsetChecks.Remove(item);
        var result = await configurationService.UpdateOffsetChecksAsync(offsetChecks, siteId, GetPartyId());
        return result ? Ok(result) : BadRequest();
    }

    /// <summary>
    /// Update an offset check 
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("site/{siteId:int}/offset-check/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOffsetCheck(int siteId, long id, LegacyOffsetCheck spec)
    {
        var offsetChecks = await configurationService.GetOffsetCheckListAsync(siteId);
        var item = offsetChecks.FirstOrDefault(x => x.Id == id);
        if (item == null) return NotFound();
        item.Name = spec.Name;
        item.IsActive = spec.IsActive;
        item.AccountNumbers = spec.AccountNumbers;
        var result = await configurationService.UpdateOffsetChecksAsync(offsetChecks, siteId, GetPartyId());
        return result ? Ok(result) : BadRequest();
    }

    // /// <summary>
    // /// Activate an offset check 
    // /// </summary>
    // /// <param name="siteId"></param>
    // /// <param name="id"></param>
    // /// <returns></returns>
    // [HttpPut("site/{siteId:int}/offset-check/{id:long}/activate")]
    // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<IActionResult> ActivateOffsetCheck(int siteId, long id)
    // {
    //     var offsetChecks = await _configSvc.GetOffsetCheckListAsync(siteId);
    //     var item = offsetChecks.FirstOrDefault(x => x.Id == id);
    //     if (item == null) return NotFound();
    //     item.IsActive = true;
    //     var result = await _configSvc.UpdateOffsetChecksAsync(offsetChecks, siteId, GetPartyId());
    //     return result ? Ok(result) : BadRequest();
    // }
    //
    // /// <summary>
    // /// Inactivate an offset check 
    // /// </summary>
    // /// <param name="siteId"></param>
    // /// <param name="id"></param>
    // /// <returns></returns>
    // [HttpPut("site/{siteId:int}/offset-check/{id:long}/inactivate")]
    // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> InactivateOffsetCheck(int siteId, long id)
    // {
    //     var offsetChecks = await _configSvc.GetOffsetCheckListAsync(siteId);
    //     var item = offsetChecks.FirstOrDefault(x => x.Id == id);
    //     if (item == null) return NotFound();
    //     item.IsActive = false;
    //     var result = await _configSvc.UpdateOffsetChecksAsync(offsetChecks, siteId, GetPartyId());
    //     return result ? Ok(result) : BadRequest();
    // }

    /// <summary>
    /// Get all Configuration by site
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet("site/{siteId:int}/all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.AllSetting))]
    public async Task<IActionResult> SiteAll(int siteId)
    {
        var items = await configurationService.GetAllConfigurationBySiteAsync(siteId);
        return Ok(items);
    }

    /// <summary>
    /// Get all Configuration
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.AllSetting))]
    public async Task<IActionResult> All()
    {
        var items = await configurationService.GetAllConfigurationAsync();
        return Ok(items);
    }

    /// <summary>
    /// Get public Configuration
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet("site/{siteId:int}/public")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationConfigure.PublicSetting))]
    public async Task<IActionResult> PublicConfiguration(int siteId)
    {
        var items = await configurationService.GetPublicConfigurationBySiteAsync(siteId);
        return Ok(items);
    }

    /// <summary>
    /// Get public Configuration
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet("site/{siteId:int}/raw")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Configuration>))]
    public async Task<IActionResult> GetAllRecords(int siteId)
    {
        var items = await configurationService.GetAllConfigurationAsync();
        return Ok(items.Where(x => x.RowId == siteId).ToList());
    }

    /// <summary>
    /// Reload Configuration
    /// </summary>
    /// <returns></returns>
    [HttpPut("reload")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reload()
    {
        await configurationService.ResetCacheAsync();
        await configSvc.ResetCacheAsync();
        return NoContent();
    }
}