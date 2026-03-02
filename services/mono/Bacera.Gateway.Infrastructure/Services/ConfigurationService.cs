using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Newtonsoft.Json;
using M = Bacera.Gateway.Configuration;

namespace Bacera.Gateway
{
    public class ConfigurationService(
        ITenantGetter tenantGetter,
        TenantDbContext tenantDbContext,
        IMyCache cache)
    {
        private readonly long _tenantId = tenantGetter.GetTenantId();
        private readonly string _allCacheKey = CacheKeys.GetConfigurationAllKey(tenantGetter.GetTenantId());

        public async Task<int> GetDefaultFundTypeAsync(int siteId = 0)
            => await GetIntValueAsync(ConfigurationTypes.DefaultFundType, siteId);

        public async Task<bool> SetDefaultFundTypeAsync(int value, int siteId, long operatorPartyId = 1)
            => await SetIntValueAsync(ConfigurationTypes.DefaultFundType, value, siteId, operatorPartyId);

        public async Task<int> GetDefaultTradeServiceAsync(int siteId = 0)
            => await GetIntValueAsync(ConfigurationTypes.DefaultTradeService, siteId);

        public async Task<bool> SetHighDollarValueAsync(int value, int siteId, long operatorPartyId = 1)
            => await SetIntValueAsync(ConfigurationTypes.HighDollarValue, value, siteId, operatorPartyId);

        public async Task<int> GetHighDollarValueAsync(int siteId = 0)
            => await GetIntValueAsync(ConfigurationTypes.HighDollarValue, siteId);

        public async Task<bool> SetDefaultTradeServiceAsync(int value, int siteId, long operatorPartyId = 1)
            => await SetIntValueAsync(ConfigurationTypes.DefaultTradeService, value, siteId, operatorPartyId);

        public async Task<bool> GetQuizFailLockEnabledToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.QuizFailLockEnabled, siteId);

        public async Task<bool> SetQuizFailLockEnabledToggleSwitchAsync(bool enabled, int siteId,
            long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.QuizFailLockEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetMultipleSiteEnabledToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.MultipleSiteIdEnabled, siteId);

        public async Task<bool> SetMultipleSiteEnabledToggleSwitchAsync(bool enabled, int siteId,
            long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.MultipleSiteIdEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetAutoConfirmEmailEnabledToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.AutoConfirmEmailEnabled, siteId);

        public async Task<bool> SetAutoConfirmEmailEnabledToggleSwitchAsync(bool enabled, int siteId,
            long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.AutoConfirmEmailEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetVerificationQuizToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.VerificationQuizEnabled, siteId);

        public async Task<bool> SetVerificationQuizToggleSwitchAsync(bool enabled, int siteId, long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.VerificationQuizEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetRebateToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.RebateEnabled, siteId);

        public async Task<bool> SetRebateToggleSwitchAsync(bool enabled, int siteId, long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.RebateEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetAccountDailyReportToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.AccountDailyReportEnabled, siteId);

        public async Task<bool> SetAccountDailyReportToggleSwitchAsync(bool enabled, int siteId,
            long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.AccountDailyReportEnabled, enabled, siteId,
                operatorPartyId);

        public async Task<DateTime> GetRebateCalculateFromAsync(int siteId = 0)
            => await GetDateTimeValueAsync(ConfigurationTypes.RebateCalculateFrom, siteId);

        public async Task<bool> SetRebateCalculateFromAsync(DateTime dateTime, int siteId, long operatorPartyId = 1)
            => await SetDateTimeValueAsync(ConfigurationTypes.RebateCalculateFrom, dateTime, siteId, operatorPartyId,
                false);

        public async Task<DateTime> GetTrackMt4TradeOpenFromAsync(int siteId = 0)
            => await GetDateTimeValueAsync(ConfigurationTypes.TrackMt4TradeOpenFrom, siteId, true);

        public async Task<bool> SetTrackMt4TradeOpenFromAsync(DateTime dateTime, int siteId, long operatorPartyId = 1)
            => await SetDateTimeValueAsync(ConfigurationTypes.TrackMt4TradeOpenFrom, dateTime, siteId, operatorPartyId,
                false);

        public async Task<DateTime> GetTrackMt5TradeOpenFromAsync(int siteId = 0)
            => await GetDateTimeValueAsync(ConfigurationTypes.TrackMt5TradeOpenFrom, siteId, true);

        public async Task<bool> SetTrackMt5TradeOpenFromAsync(DateTime dateTime, int siteId, long operatorPartyId = 1)
            => await SetDateTimeValueAsync(ConfigurationTypes.TrackMt5TradeOpenFrom, dateTime, siteId, operatorPartyId,
                false);

        public async Task<bool> GetWebTraderToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.WebTraderEnabled, siteId);

        public async Task<bool> GetNewYearEventToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.NewYearEvent, siteId);

        public async Task<bool> GetWholesaleEnabledToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.WholesaleEnabled, siteId);

        public async Task<bool> SetWebTraderToggleSwitchAsync(bool enabled, int siteId, long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.WebTraderEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetSmsVerificationToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.SmsValidationEnabled, siteId);

        public async Task<bool> SetSmsValidationToggleSwitchAsync(bool enabled, int siteId, long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.SmsValidationEnabled, enabled, siteId, operatorPartyId);

        public async Task<bool> GetIbToggleSwitchAsync(int siteId = 0)
            => await GetToggleSwitchAsync(ConfigurationTypes.IbEnabled, siteId);

        public async Task<bool> SetIbToggleSwitchAsync(bool enabled, int siteId, long operatorPartyId = 1)
            => await SetToggleSwitchAsync(ConfigurationTypes.IbEnabled, enabled, siteId, operatorPartyId);

        public async Task<List<int>> GetLeverageAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.LeverageAvailable, siteId);

        public async Task<List<int>> GetLeverageForWholesaleAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.LeverageForWholesaleAvailable, siteId);

        public async Task<bool> SetLeverageAvailableAsync(List<int> list, int siteId, long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.LeverageAvailable, list, siteId, operatorPartyId);

        public async Task<bool> SetLeverageForWholesaleAvailableAsync(List<int> list, int siteId,
            long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.LeverageForWholesaleAvailable, list, siteId,
                operatorPartyId);

        public async Task<List<int>> GetAccountTypeAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.AccountTypeAvailable, siteId);

        public async Task<bool> SetAccountTypeAvailableAsync(List<int> list, int siteId, long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.AccountTypeAvailable, list, siteId, operatorPartyId);

        public async Task<List<int>> GetCurrencyAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.CurrencyAvailable, siteId);

        public async Task<bool> SetCurrencyAvailableAsync(List<int> list, int siteId, long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.CurrencyAvailable, list, siteId, operatorPartyId);

        public async Task<List<int>> GetTradingPlatformAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.TradingPlatformAvailable, siteId);

        public async Task<bool> SetTradingPlatformAvailableAsync(List<int> list, int siteId, long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.TradingPlatformAvailable, list, siteId,
                operatorPartyId);

        public async Task<List<int>> GetDemoTradingPlatformAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.DemoTradingPlatformAvailable, siteId);

        public async Task<bool> SetDemoTradingPlatformAvailableAsync(List<int> list, int siteId,
            long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.DemoTradingPlatformAvailable, list, siteId,
                operatorPartyId);

        public async Task<List<int>> GetFundTypeAvailableAsync(int siteId = 0)
            => await GetAsync<List<int>>(ConfigurationTypes.FundTypeAvailable, siteId);

        public async Task<bool> SetFundTypeAvailableAsync(List<int> list, int siteId, long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.FundTypeAvailable, list, siteId, operatorPartyId);

        public async Task<string> GetDefaultEmailAddressAsync(int siteId = 0)
            => await GetStringValueAsync(ConfigurationTypes.DefaultEmailAddress, siteId);

        public async Task<bool> SetDefaultEmailAddressAsync(string email, int siteId, long operatorPartyId = 1)
            => await SetStringValueAsync(ConfigurationTypes.DefaultEmailAddress, email, siteId, operatorPartyId);

        public async Task<string> GetDefaultEmailDisplayNameAsync(int siteId = 0)
            => await GetStringValueAsync(ConfigurationTypes.DefaultEmailDisplayName, siteId);

        public async Task<bool> SetDefaultEmailDisplayNameAsync(string name, int siteId, long operatorPartyId = 1)
            => await SetStringValueAsync(ConfigurationTypes.DefaultEmailDisplayName, name, siteId, operatorPartyId);

        public async Task<Dictionary<string, string>> GetContactInfoAsync(int siteId = 0)
            => await GetDictionaryValueAsync(ConfigurationTypes.ContactInfo, siteId);

        public async Task<bool> SetContactInfoAsync(Dictionary<string, string> contactInfo, int siteId = 0,
            long operatorPartyId = 1)
            => await SetDictionaryValueAsync(ConfigurationTypes.ContactInfo, contactInfo, siteId, operatorPartyId);

        public async Task<Dictionary<string, string>> GetCheaterIpAsync(int siteId = 0)
            => await GetAsync<Dictionary<string, string>>(ConfigurationTypes.CheaterIp, siteId);

        public async Task<bool> SetCheaterIpAsync(Dictionary<string, string> cheaterIps, int siteId = 0,
            long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.CheaterIp, cheaterIps, siteId, operatorPartyId);

        public async Task<List<IpSetting>> GetIpSettingAsync(int siteId = 0)
            => await GetAsync<List<IpSetting>>(ConfigurationTypes.IpSetting, siteId);

        public async Task<bool> SetIpSettingAsync(List<IpSetting> ipSettings, int siteId = 0,
            long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.IpSetting, ipSettings, siteId, operatorPartyId);

        public async Task<bool> SetDefaultRebateLevelSettingAsync(
            Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>> defaultLevelSetting
            , int siteId = 0
            , long operatorPartyId = 1)
            => await UpdateAsync(ConfigurationTypes.DefaultRebateLevelSetting
                , defaultLevelSetting
                , siteId
                , operatorPartyId);

        public async Task<Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>> GetDefaultRebateLevelSettingAsync(
            int siteId = 0)
            => await GetAsync<Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>>(
                ConfigurationTypes.DefaultRebateLevelSetting, siteId);

        // public async Task<bool> SetOffsetChecksAsync(List<OffsetCheck> offsetChecks, int siteId = 0,
        //     long operatorPartyId = 1)
        // {
        //     var cacheKey = GetItemCacheKey(siteId, ConfigurationTypes.OffsetCheck);
        //     await _cache.RemoveAsync(cacheKey);
        //
        //     var item = new M
        //     {
        //         RowId = siteId,
        //         Key = Enum.GetName(typeof(ConfigurationTypes), ConfigurationTypes.OffsetCheck) ?? string.Empty,
        //         Name = ConfigurationTypes.OffsetCheck.ToString(),
        //         Value = JsonConvert.SerializeObject(offsetChecks),
        //         UpdatedOn = DateTime.UtcNow,
        //         UpdatedBy = operatorPartyId
        //     };
        //     await _tenantDbContext.Configurations.AddAsync(item);
        //     await _tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
        //     return true;
        // }

        public async Task<bool> UpdateOffsetChecksAsync(List<LegacyOffsetCheck> offsetChecks,
            int siteId = 0,
            long operatorPartyId = 1)
        {
            var cacheKey = GetItemCacheKey(siteId, ConfigurationTypes.OffsetCheck);
            await cache.KeyDeleteAsync(cacheKey);

            var configuration = await tenantDbContext.Configurations
                .Where(x => x.Category == nameof(Public))
                .Where(x => x.RowId == siteId)
                .Where(x => x.Key == Enum.GetName(typeof(ConfigurationTypes), ConfigurationTypes.OffsetCheck))
                .SingleOrDefaultAsync();

            if (configuration == null)
            {
                var item = new M
                {
                    RowId = siteId,
                    Key = Enum.GetName(typeof(ConfigurationTypes), ConfigurationTypes.OffsetCheck) ?? string.Empty,
                    Name = ConfigurationTypes.OffsetCheck.ToString(),
                    Value = JsonConvert.SerializeObject(offsetChecks),
                    UpdatedOn = DateTime.UtcNow,
                    UpdatedBy = operatorPartyId
                };
                await tenantDbContext.Configurations.AddAsync(item);
                await tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
                return true;
            }

            configuration.Value = JsonConvert.SerializeObject(offsetChecks);
            configuration.UpdatedOn = DateTime.UtcNow;
            configuration.UpdatedBy = operatorPartyId;
            tenantDbContext.Configurations.Update(configuration);
            await tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
            return true;
        }

        public async Task<List<LegacyOffsetCheck>> GetOffsetCheckListAsync(int siteId = 0)
        {
            var cacheKey = GetItemCacheKey(siteId, ConfigurationTypes.OffsetCheck);
            var cached = await cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonConvert.DeserializeObject<List<LegacyOffsetCheck>>(cached) ?? [];

            var data = await tenantDbContext.Configurations
                .Where(x => x.Category == nameof(Public))
                .Where(x => x.RowId == siteId)
                .Where(x => x.Key == Enum.GetName(typeof(ConfigurationTypes), ConfigurationTypes.OffsetCheck))
                .Select(x => x.Value)
                .SingleOrDefaultAsync();
            if (data == null) return [];

            var offsetChecks = JsonConvert.DeserializeObject<List<LegacyOffsetCheck>>(data) ?? [];

            await cache.SetStringAsync(cacheKey, data, TimeSpan.FromDays(7));
            return offsetChecks;
        }

        public async Task<bool> IsIpSettingContains(IpSettingTypes type, string ip, int siteId = 0)
        {
            var ipSettings = await GetIpSettingAsync(siteId);
            return ipSettings.Any()
                   && ipSettings
                       .Where(x => x.Type == type)
                       .Any(x => x.Ip == ip);
        }

        public async Task<ApplicationConfigure.PublicSetting> GetPublicConfigurationBySiteAsync(int siteId = 0) =>
            new()
            {
                SiteId = siteId,
                DefaultFundType = await GetDefaultFundTypeAsync(siteId),
                DefaultTradeService = await GetDefaultTradeServiceAsync(siteId),
                IbEnabled = await GetIbToggleSwitchAsync(siteId),
                RebateEnabled = await GetRebateToggleSwitchAsync(siteId),
                AccountDailyReportEnabled = await GetAccountDailyReportToggleSwitchAsync(siteId),
                WebTraderEnabled = await GetWebTraderToggleSwitchAsync(siteId),
                SmsValidationEnabled = await GetSmsVerificationToggleSwitchAsync(siteId),
                VerificationQuizEnabled = await GetVerificationQuizToggleSwitchAsync(siteId),
                AccountTypeAvailable = await GetAccountTypeAvailableAsync(siteId),
                CurrencyAvailable = await GetCurrencyAvailableAsync(siteId),
                FundTypeAvailable = await GetFundTypeAvailableAsync(siteId),
                LeverageAvailable = await GetLeverageAvailableAsync(siteId),
                TradingPlatformAvailable = await GetTradingPlatformAvailableAsync(siteId),
                DemoTradingPlatformAvailable = await GetDemoTradingPlatformAvailableAsync(siteId),
                LeverageForWholesaleAvailable = await GetLeverageForWholesaleAvailableAsync(siteId),
                ContactInfo = await GetContactInfoAsync(siteId),
                NewYearEvent = await GetNewYearEventToggleSwitchAsync(siteId),
                WholesaleEnabled = await GetWholesaleEnabledToggleSwitchAsync(siteId),
            };

       

        public async Task<ApplicationConfigure.AllSetting> GetAllConfigurationBySiteAsync(int siteId = 0) =>
            new()
            {
                SiteId = siteId,
                DefaultFundType = await GetDefaultFundTypeAsync(siteId),
                DefaultTradeService = await GetDefaultTradeServiceAsync(siteId),
                DefaultEmailAddress = await GetDefaultEmailAddressAsync(siteId),
                DefaultEmailDisplayName = await GetDefaultEmailDisplayNameAsync(siteId),
                MultipleSiteEnabled = await GetMultipleSiteEnabledToggleSwitchAsync(siteId),
                AutoConfirmEmailEnabled = await GetAutoConfirmEmailEnabledToggleSwitchAsync(siteId),

                IbEnabled = await GetIbToggleSwitchAsync(siteId),
                RebateEnabled = await GetRebateToggleSwitchAsync(siteId),
                AccountDailyReportEnabled = await GetAccountDailyReportToggleSwitchAsync(siteId),
                RebateCalculateFrom = await GetRebateCalculateFromAsync(siteId),
                WebTraderEnabled = await GetWebTraderToggleSwitchAsync(siteId),
                SmsValidationEnabled = await GetSmsVerificationToggleSwitchAsync(siteId),
                VerificationQuizEnabled = await GetVerificationQuizToggleSwitchAsync(siteId),
                QuizFailLockEnabled = await GetQuizFailLockEnabledToggleSwitchAsync(siteId),

                AccountTypeAvailable = await GetAccountTypeAvailableAsync(siteId),
                CurrencyAvailable = await GetCurrencyAvailableAsync(siteId),
                FundTypeAvailable = await GetFundTypeAvailableAsync(siteId),
                LeverageAvailable = await GetLeverageAvailableAsync(siteId),
                TradingPlatformAvailable = await GetTradingPlatformAvailableAsync(siteId),
                DemoTradingPlatformAvailable = await GetDemoTradingPlatformAvailableAsync(siteId),
                LeverageForWholesaleAvailable = await GetLeverageForWholesaleAvailableAsync(siteId),
                ContactInfo = await GetContactInfoAsync(siteId),
                CheaterIp = await GetCheaterIpAsync(siteId),
                HighDollarValue = await GetHighDollarValueAsync(siteId),
                IpSetting = await GetIpSettingAsync(siteId),
                NewYearEvent = await GetNewYearEventToggleSwitchAsync(siteId),
                WholesaleEnabled = await GetWholesaleEnabledToggleSwitchAsync(siteId),
            };

        public async Task<bool> UpdateAsync(ConfigurationTypes type, object value, int siteId, long operatorPartyId = 1,
            bool clearCache = true)
        {
            var key = Enum.GetName(typeof(ConfigurationTypes), type) ?? string.Empty;
            var item = await tenantDbContext.Configurations
                           .Where(x => x.Category == nameof(Public))
                           .Where(x => x.RowId == siteId)
                           .FirstOrDefaultAsync(x => x.Key == key)
                       ?? new M();

            item.RowId = siteId;
            item.Key = key;
            item.Category = nameof(Public);
            item.DataFormat = "json";
            item.Description = "";
            item.Name = type.ToString();
            item.Value = JsonConvert.SerializeObject(value);
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = operatorPartyId;
            if (item.Id == 0)
                await tenantDbContext.Configurations.AddAsync(item);
            else
                tenantDbContext.Configurations.Update(item);
            var result = await tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);

            if (clearCache)
            {
                var hkey = CacheKeys.GetConfigHashKey(_tenantId);
                var field = $"{item.Category}:{item.RowId}:{item.Key}";
                await cache.HSetStringAsync(hkey, field, item.Value, TimeSpan.FromDays(1));
                await ResetCacheAsync();
            }

            return result > 0;
        }

        public async Task<T> GetAsync<T>(ConfigurationTypes type, int siteId, bool fromDb = false)
            where T : class, new()
        {
            Configuration? item;
            var key = Enum.GetName(typeof(ConfigurationTypes), type) ?? string.Empty;
            if (fromDb)
            {
                item = await tenantDbContext.Configurations
                    .Where(x => x.Category == nameof(Public))
                    .Where(x => x.RowId == siteId)
                    .Where(x => x.Key == key)
                    .FirstOrDefaultAsync();
            }
            else
            {
                var data = await GetAllConfigurationAsync();
                item = data
                           .Where(x => x.Category == nameof(Public))
                           .Where(x => x.RowId == siteId)
                           .FirstOrDefault(x => x.Key == key)
                       ?? data
                           .Where(x => x.Category == nameof(Public))
                           .Where(x => x.RowId == 0)
                           .FirstOrDefault(x => x.Key == key);
            }

            if (item == null)
                return new T();

            try
            {
                return JsonConvert.DeserializeObject<T>(item.Value) ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        private async Task<bool> GetToggleSwitchAsync(ConfigurationTypes type, int siteId)
        {
            var data = await GetAsync<ApplicationConfigure.BoolValue>(type, siteId);
            return data.Value;
        }

        private async Task<bool> SetToggleSwitchAsync(ConfigurationTypes type, bool enabled, int siteId,
            long operatorPartyId = 1)
        {
            var data = await GetAsync<ApplicationConfigure.BoolValue>(type, siteId);
            if (enabled == data.Value)
                return true;
            data.Value = enabled;
            return await UpdateAsync(type, data, siteId, operatorPartyId);
        }

        private async Task<int> GetIntValueAsync(ConfigurationTypes type, int siteId)
        {
            var data = await GetAsync<ApplicationConfigure.IntValue>(type, siteId);
            return data.Value;
        }

        private async Task<bool> SetIntValueAsync(ConfigurationTypes type, int value, int siteId,
            long operatorPartyId = 1)
        {
            var data = await GetAsync<ApplicationConfigure.IntValue>(type, siteId);
            if (value == data.Value)
                return true;
            data.Value = value;
            return await UpdateAsync(type, data, siteId, operatorPartyId);
        }

        private async Task<string> GetStringValueAsync(ConfigurationTypes type, int siteId)
        {
            var data = await GetAsync<ApplicationConfigure.StringValue>(type, siteId);
            return data.Value;
        }

        private async Task<bool> SetStringValueAsync(ConfigurationTypes type, string value, int siteId,
            long operatorPartyId = 1)
        {
            var data = await GetAsync<ApplicationConfigure.StringValue>(type, siteId);
            if (value == data.Value)
                return true;
            data.Value = value;
            return await UpdateAsync(type, data, siteId, operatorPartyId);
        }

        private async Task<Dictionary<string, string>> GetDictionaryValueAsync(ConfigurationTypes type, int siteId)
        {
            var data = await GetAsync<ApplicationConfigure.DictionaryValue>(type, siteId);
            return data.Value;
        }

        private async Task<bool> SetDictionaryValueAsync(ConfigurationTypes type, Dictionary<string, string> value
            , int siteId
            , long operatorPartyId = 1)
        {
            var data = await GetAsync<ApplicationConfigure.DictionaryValue>(type, siteId);
            data.Value = value;
            return await UpdateAsync(type, data, siteId, operatorPartyId);
        }

        private async Task<DateTime> GetDateTimeValueAsync(ConfigurationTypes type, int siteId, bool fromDb = false)
        {
            var data = await GetAsync<ApplicationConfigure.DateTimeValue>(type, siteId, fromDb);
            return DateTime.SpecifyKind(data.Value, DateTimeKind.Utc);
        }

        private async Task<bool> SetDateTimeValueAsync(ConfigurationTypes type, DateTime value, int siteId,
            long operatorPartyId = 1, bool clearCache = true)
        {
            var data = await GetAsync<ApplicationConfigure.DateTimeValue>(type, siteId);
            data.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return await UpdateAsync(type, data, siteId, operatorPartyId, clearCache);
        }

        public Task<List<M>> GetAllConfigurationAsync()
            => cache.GetOrSetAsync(_allCacheKey, () => tenantDbContext.Configurations.ToListAsync(), TimeSpan.FromDays(1));

        public Task ResetCacheAsync() => cache.KeyDeleteAsync(_allCacheKey);

        private string GetItemCacheKey(int siteId, ConfigurationTypes type) =>
            $"configuration_site:{siteId}_type:{type}_tenant:{_tenantId}";
    }
}