using Newtonsoft.Json;

namespace Bacera.Gateway;

public class ApplicationConfigure
{
    public class BoolValue
    {
        public bool Value { get; set; }
        public static BoolValue Of(bool value = false) => new() { Value = value };
    }

    public class StringValue
    {
        public string Value { get; set; } = string.Empty;
        public static StringValue Of(string value = "") => new() { Value = value };
    }

    public class IntValue
    {
        public int Value { get; set; }
        public static IntValue Of(int value = 0) => new() { Value = value };
    }

    public class LongValue
    {
        public long Value { get; set; }
        public static LongValue Of(long value = 0) => new() { Value = value };
    }

    public class DictionaryValue
    {
        public Dictionary<string, string> Value { get; set; } = new();

        public static DictionaryValue Of(Dictionary<string, string> value)
            => new() { Value = value };
    }

    public class DateTimeValue
    {
        public DateTime Value { get; set; }
        public static DateTimeValue Of(DateTime value) => new() { Value = value };
    }

    public class ObjectValue
    {
        public object Value { get; set; } = new();

        public static ObjectValue Of(object value)
            => new() { Value = value };
    }

    public class AutoCompleteTransactionAmountValue
    {
        public bool Enabled { get; set; }
        public long Amount { get; set; }

        public static AutoCompleteTransactionAmountValue Of(bool enabled = false, long amount = 0)
            => new() { Enabled = enabled, Amount = amount };
    }

    public class PublicSettingV2
    {
        public List<AccountTypes> AccountTypeAvailable { get; set; } = [];
        public dynamic ContactInfo { get; set; } = new { };
        public List<int> CurrencyAvailable { get; set; } = [];
        public int DefaultTradeService { get; set; }
        public List<int> DemoTradingPlatformAvailable { get; set; } = [];
        public List<int> LeverageAvailable { get; set; } = [];
        public int SiteId { get; set; }
        public bool VerificationQuizEnabled { get; set; }
        public bool SmsValidationEnabled { get; set; }
    }

    public class PublicSetting
    {
        public int SiteId { get; set; }
        public int DefaultFundType { get; set; }
        public int DefaultTradeService { get; set; }
        public int HoursGapForMT5 { get; set; }

        public List<int> FundTypeAvailable { get; set; } = new();
        public List<int> AccountTypeAvailable { get; set; } = new();
        public List<int> CurrencyAvailable { get; set; } = new();
        public List<int> LeverageAvailable { get; set; } = new();
        public List<int> LeverageForWholesaleAvailable { get; set; } = new();
        public List<int> TradingPlatformAvailable { get; set; } = new();
        public List<int> DemoTradingPlatformAvailable { get; set; } = new();
        public bool IbEnabled { get; set; }
        public bool RebateEnabled { get; set; }
        public bool WholesaleEnabled { get; set; }
        public bool AccountDailyReportEnabled { get; set; }
        public bool SmsValidationEnabled { get; set; }
        public bool WebTraderEnabled { get; set; }
        public bool VerificationQuizEnabled { get; set; }
        public bool NewYearEvent { get; set; }
        public bool UTCEnabled { get; set; }

        public Dictionary<string, string> ContactInfo { get; set; } = new();
        public DateTime RebateCalculateFrom { get; set; }
        [JsonIgnore] public string? TwoFactorAuthRaw { get; set; } = string.Empty;

        public bool? TwoFactorAuth
        {
            get
            {
                if (string.IsNullOrEmpty(TwoFactorAuthRaw)) return null;
                var config = Utils.JsonDeserializeObjectWithDefault<TwoFactorAuthSetting>(TwoFactorAuthRaw);
                return config.LoginCodeEnabled;
            }
        }

        public TwoFactorAuthTransactionSetting? TwoFactorAuthForTransactions
        {  
            get
            {
                if (string.IsNullOrEmpty(TwoFactorAuthRaw)) return null;
                var config = Utils.JsonDeserializeObjectWithDefault<TwoFactorAuthSetting>(TwoFactorAuthRaw);

                // Return only transaction-related properties
                return new TwoFactorAuthTransactionSetting
                {
                    WalletToWalletTransfer = config.WalletToWalletTransfer,
                    WalletToTradeAccount = config.WalletToTradeAccount,
                    TradeAccountToTradeAccount = config.TradeAccountToTradeAccount,
                    Withdrawal = config.Withdrawal
                };
            }
        }
    }

    public class AllSetting : PublicSetting
    {
        public string DefaultEmailAddress { get; set; } = string.Empty;
        public string DefaultEmailDisplayName { get; set; } = string.Empty;
        public bool QuizFailLockEnabled { get; set; }
        public bool MultipleSiteEnabled { get; set; }
        public bool AutoConfirmEmailEnabled { get; set; }

        public int HighDollarValue { get; set; }
        public List<IpSetting> IpSetting { get; set; } = new();
        public Dictionary<string, string> CheaterIp { get; set; } = new();
    }
}