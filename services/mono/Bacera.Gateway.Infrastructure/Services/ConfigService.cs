using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Report.Models;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class ConfigService(TenantDbContext tenantCtx, ITenantGetter getter, IMyCache cache)
{
    private readonly long _tenantId = getter.GetTenantId();
    private readonly string _hkey = CacheKeys.GetConfigHashKey(getter.GetTenantId());

    public async Task<int> GetHighDollarValueAsync(int siteId = 0)
    {
        var item = await GetAsync<ApplicationConfigure.IntValue>(nameof(Public), siteId, ConfigKeys.HighDollarValue);
        if (item is { Value: > 0 }) return item.Value;
        var defaultVal = await GetAsync<ApplicationConfigure.IntValue>(nameof(Public), 0, ConfigKeys.HighDollarValue);
        return defaultVal?.Value ?? 0;
    }

    /// <summary>
    /// According to LA DST, arrange the gap hours, if true use 3, otherwise use 2.
    /// Uses <paramref name="referenceDate"/> to determine DST; falls back to <see cref="DateTime.UtcNow"/> when null.
    /// </summary>
    /// <returns>2 or 3</returns>
    public Task<int> GetHoursGapForMT5Async(DateTime? referenceDate = null)
    {
        var hoursGap = Utils.IsCurrentDSTLosAngeles(referenceDate ?? DateTime.UtcNow) ? 3 : 2;
        return Task.FromResult(hoursGap);
    }

    public async Task<OfficeMergeMapping?> GetDailyEquityOfficeMergeMappingAsync()
    {
        return await GetAsync<OfficeMergeMapping>("Public", 0, ConfigKeys.DailyEquityOfficeMergeMapping);
    }

    public async Task<string?> GetRawValueAsync(string category, long rowId, string key)
    {
        var field = $"{category}:{rowId}:{key}";
        var value = await cache.HGetStringAsync(_hkey, field);
        if (value != null) return value;

        value = await tenantCtx.Configurations
            .Where(x => x.Category == category)
            .Where(x => x.RowId == rowId)
            .Where(x => x.Key == key)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();

        if (value != null)
        {
            await cache.HSetStringAsync(_hkey, field, value, TimeSpan.FromHours(12));
        }

        return value;
    }


    public async Task<T?> GetAsync<T>(string category, long rowId, string key) where T : class, new()
    {
        var value = await GetRawValueAsync(category, rowId, key);
        if (value == null) return null;
        try
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        catch
        {
            var field = $"{category}:{rowId}:{key}";
            await cache.HSetDeleteByKeyFieldAsync(_hkey, field);
            return null;
        }
    }

    public async Task DeleteAsync(string category, long rowId, string key)
    {
        var field = $"{category}:{rowId}:{key}";
        await cache.HSetDeleteByKeyFieldAsync(_hkey, field);

        var item = await tenantCtx.Configurations
            .Where(x => x.Category == category)
            .Where(x => x.RowId == rowId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.Key == key);
        if (item == null) return;

        tenantCtx.Configurations.Remove(item);
        await tenantCtx.SaveChangesAsync();
    }

    public Task ResetCacheAsync() => cache.KeyDeleteAsync(_hkey);

    public async Task SetAsync<T>(string category, long rowId, string key, T value, long partyId = 1, long? id = null)
        where T : class, new()
    {
        var field = $"{category}:{rowId}:{key}";
        await cache.HSetStringAsync(_hkey, field, JsonConvert.SerializeObject(value), TimeSpan.FromHours(12));
        var item = await tenantCtx.Configurations
            .Where(x => x.Category == category)
            .Where(x => x.RowId == rowId)
            .FirstOrDefaultAsync(x => x.Key == key);
        if (item == null)
        {
            item = new Configuration
            {
                Category = category,
                RowId = rowId,
                Key = key,
                Value = JsonConvert.SerializeObject(value),
                Name = key[..Math.Min(key.Length, 50)],
                DataFormat = "json",
                Description = "",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = partyId
            };
            if (id != null) item.Id = id.Value;
            tenantCtx.Configurations.Add(item);
        }
        else
        {
            item.Value = JsonConvert.SerializeObject(value);
        }

        await tenantCtx.SaveChangesAsync();
    }

    public async Task<ApplicationConfigure.PublicSetting> GetPartyConfigurationBySiteAsync(long partyId)
    {
        var partySiteId = await tenantCtx.Parties
            .Where(x => x.Id == partyId)
            .Select(x => x.SiteId)
            .SingleOrDefaultAsync();

        var keys = ConfigKeys.PublicKeys;

        var partyConfigs = await tenantCtx.Configurations
            .Where(x => keys.Contains(x.Key))
            .Where(x => x.Category == nameof(Party))
            .Where(x => x.RowId == partyId)
            .ToDictionaryAsync(x => x.Key, x => x.Value);

        var siteConfigs = await tenantCtx.Configurations
            .Where(x => keys.Contains(x.Key))
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == partySiteId)
            .ToDictionaryAsync(x => x.Key, x => x.Value);

        var defaultConfigs = await tenantCtx.Configurations
            .Where(x => keys.Contains(x.Key))
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.RowId == 0)
            .ToDictionaryAsync(x => x.Key, x => x.Value);

        var dict = new Dictionary<string, string>();
        foreach (var key in keys)
        {
            if (partyConfigs.TryGetValue(key, out var config))
            {
                dict[key] = config;
            }
            else if (siteConfigs.TryGetValue(key, out var siteConfig))
            {
                dict[key] = siteConfig;
            }
            else if (defaultConfigs.TryGetValue(key, out var defaultConfig))
            {
                dict[key] = defaultConfig;
            }
            else
            {
                dict[key] = "";
            }
        }

        var isSales = await tenantCtx.Accounts
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .AnyAsync();
        if (!isSales && dict[ConfigKeys.TwoFactorAuthSetting] == "")
        {
            dict[ConfigKeys.TwoFactorAuthSetting] =
                JsonConvert.SerializeObject(new TwoFactorAuthSetting 
                { 
                    LoginCodeEnabled = false,
                    WalletToWalletTransfer = true,
                    WalletToTradeAccount = true,
                    TradeAccountToTradeAccount = true,
                    Withdrawal = true,
                });
        }

        var result = new ApplicationConfigure.PublicSetting
        {
            SiteId = partySiteId,
            DefaultFundType =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.IntValue>(dict[ConfigKeys.DefaultFundType])
                    .Value,
            DefaultTradeService =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.IntValue>(
                    dict[ConfigKeys.DefaultTradeService]).Value,
            IbEnabled = Utils
                .JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(dict[ConfigKeys.IbEnabled]).Value,
            RebateEnabled =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(dict[ConfigKeys.RebateEnabled])
                    .Value,
            AccountDailyReportEnabled =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(
                    dict[ConfigKeys.AccountDailyReportEnabled]).Value,
            WebTraderEnabled =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(
                    dict[ConfigKeys.WebTraderEnabled]).Value,
            SmsValidationEnabled =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(
                    dict[ConfigKeys.SmsValidationEnabled]).Value,
            VerificationQuizEnabled = Utils
                .JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(
                    dict[ConfigKeys.VerificationQuizEnabled])
                .Value,
            AccountTypeAvailable =
                Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.AccountTypeAvailable]),
            CurrencyAvailable = Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.CurrencyAvailable]),
            FundTypeAvailable = Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.FundTypeAvailable]),
            LeverageAvailable = Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.LeverageAvailable]),
            TradingPlatformAvailable =
                Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.TradingPlatformAvailable]),
            DemoTradingPlatformAvailable =
                Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.DemoTradingPlatformAvailable]),
            LeverageForWholesaleAvailable =
                Utils.JsonDeserializeObjectWithDefault<List<int>>(dict[ConfigKeys.LeverageForWholesaleAvailable]),
            ContactInfo =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.DictionaryValue>(
                    dict[ConfigKeys.ContactInfo]).Value,
            NewYearEvent =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(dict[ConfigKeys.NewYearEvent])
                    .Value,
            WholesaleEnabled =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(
                    dict[ConfigKeys.WholesaleEnabled]).Value,
            TwoFactorAuthRaw = dict[ConfigKeys.TwoFactorAuthSetting],
            UTCEnabled =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.BoolValue>(
                    dict[ConfigKeys.UTCEnabled]).Value,
            HoursGapForMT5 =
                Utils.JsonDeserializeObjectWithDefault<ApplicationConfigure.IntValue>(
                    dict[ConfigKeys.HoursGapForMT5]).Value,
        };

        return result;
    }

    // =>
    // new()
    // {
    //     SiteId = siteId,
    //     DefaultFundType = await GetDefaultFundTypeAsync(siteId),
    //     DefaultTradeService = await GetDefaultTradeServiceAsync(siteId),
    //     IbEnabled = await GetIbToggleSwitchAsync(siteId),
    //     RebateEnabled = await GetRebateToggleSwitchAsync(siteId),
    //     AccountDailyReportEnabled = await GetAccountDailyReportToggleSwitchAsync(siteId),
    //     WebTraderEnabled = await GetWebTraderToggleSwitchAsync(siteId),
    //     SmsValidationEnabled = await GetSmsVerificationToggleSwitchAsync(siteId),
    //     VerificationQuizEnabled = await GetVerificationQuizToggleSwitchAsync(siteId),
    //     AccountTypeAvailable = await GetAccountTypeAvailableAsync(siteId),
    //     CurrencyAvailable = await GetCurrencyAvailableAsync(siteId),
    //     FundTypeAvailable = await GetFundTypeAvailableAsync(siteId),
    //     LeverageAvailable = await GetLeverageAvailableAsync(siteId),
    //     TradingPlatformAvailable = await GetTradingPlatformAvailableAsync(siteId),
    //     DemoTradingPlatformAvailable = await GetDemoTradingPlatformAvailableAsync(siteId),
    //     LeverageForWholesaleAvailable = await GetLeverageForWholesaleAvailableAsync(siteId),
    //     ContactInfo = await GetContactInfoAsync(siteId),
    //     NewYearEvent = await GetNewYearEventToggleSwitchAsync(siteId),
    //     WholesaleEnabled = await GetWholesaleEnabledToggleSwitchAsync(siteId),
    // };
}