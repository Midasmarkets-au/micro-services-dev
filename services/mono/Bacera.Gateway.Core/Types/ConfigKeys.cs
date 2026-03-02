namespace Bacera.Gateway.Core.Types;

public static class ConfigKeys
{
    public static string TradingPlatformAvailable => "TradingPlatformAvailable";
    public static string DemoTradingPlatformAvailable => "DemoTradingPlatformAvailable";
    public static string DemoAccountTypeAvailable => "DemoAccountTypeAvailable";
    public static string DemoCurrencyAvailable => "DemoCurrencyAvailable";
    public static string AccountTypeAvailable => "AccountTypeAvailable";
    public static string CurrencyAvailable => "CurrencyAvailable";
    public static string LeverageAvailable => "LeverageAvailable";
    public static string HighDollarValue => "HighDollarValue";
    public static string AutoCompleteTransactionAmount => "AutoCompleteTransactionAmount";
    public static string Mt4GroupAndSchemaNameForAutoOpenAccount => "Mt4GroupAndSchemaNameForAutoOpenAccount";
    public static string Mt5GroupAndSchemaNameForAutoOpenAccount => "Mt5GroupAndSchemaNameForAutoOpenAccount";
    public static string PaymentAccessNamesForAutoOpenAccount => "PaymentAccessNamesForAutoOpenAccount";
    public static string AutoCreateTradeAccountEnabled => "AutoCreateTradeAccountEnabled";
    public static string UserLastCheckedEventId => "UserLastCheckedEventId";
    public static string PaymentServiceCallbackSetting => "PaymentServiceCallbackSetting";
    public static string CryptoSetting => "CryptoSetting";
    public static string ExcludedFromEquityBelowCredit => "ExcludedFromEquityBelowCredit";
    public static string FundTypeAvailable => "FundTypeAvailable";
    public static string DefaultFundType => "DefaultFundType";
    public static string DefaultEmailAddress => "DefaultEmailAddress";
    public static string DefaultEmailDisplayName => "DefaultEmailDisplayName";
    public static string MultipleSiteIdEnabled => "MultipleSiteIdEnabled";
    public static string AutoConfirmEmailEnabled => "AutoConfirmEmailEnabled";
    public static string RebateEnabled => "RebateEnabled";
    public static string IbEnabled => "IbEnabled";
    public static string LeverageForWholesaleAvailable => "LeverageForWholesaleAvailable";
    public static string WebTraderEnabled => "WebTraderEnabled";
    public static string Logo => "Logo";
    public static string SmsValidationEnabled => "SmsValidationEnabled";
    public static string VerificationQuizEnabled => "VerificationQuizEnabled";
    public static string QuizFailLockEnabled => "QuizFailLockEnabled";
    public static string ContactInfo => "ContactInfo";
    public static string CheaterIp => "CheaterIp";
    public static string DefaultTradeService => "DefaultTradeService";
    public static string IpSetting => "IpSetting";
    public static string RebateCalculateFrom => "RebateCalculateFrom";
    public static string OffsetCheck => "OffsetCheck";
    public static string TrackMt4TradeOpenFrom => "TrackMt4TradeOpenFrom";
    public static string TrackMt5TradeOpenFrom => "TrackMt5TradeOpenFrom";
    public static string AccountDailyReportEnabled => "AccountDailyReportEnabled";
    public static string DefaultRebateLevelSetting => "DefaultRebateLevelSetting";
    public static string NewYearEvent => "NewYearEvent";
    public static string WholesaleEnabled => "WholesaleEnabled";
    public static string TwoFactorAuthSetting => "TwoFactorAuthSetting";
    public static string OfaDfBankNoDict => "OfaDfBankNoDict";
    public static string PaymentMethodBankNoDict => "PaymentMethodBankNoDict";
    public static string SalesWithdrawalCreatedEmailConfig => "SalesWithdrawalCreatedEmailConfig";
    public static string WelcomeInfo => "WelcomeInfo";
    public static string AutoAssignLeadInfo => "AutoAssignLeadInfo";
    public static string ApiDomain => "ApiDomain";
    public static string EuPayDesensitizedRequestData => "EuPayDesensitizedRequestData";
    public static string SendBatchEmailSpecKey => "SendBatchEmailSpecKey";
    public static string EventShopItemCategoryKey => "EventShopItemCategoryKey";
    public static string SymbolsMM => "SymbolsMM";
    public static string SymbolsOCEX => "SymbolsOCEX";
    public static string UTCEnabled => "UTCEnabled";
    public static string MaxTradeAccountCount => "MaxTradeAccountCount";
    public static string DefaultAutoCreatePaymentMethod => "DefaultAutoCreatePaymentMethod";
    public static string DefaultAutoCreateWithdrawalPaymentMethod => "DefaultAutoCreateWithdrawalPaymentMethod";
    public static string HoursGapForMT5 => "HoursGapForMT5";
    public static string PaymentCallbackToken => "PaymentCallbackToken";
    public static string EnforcePaymentCallbackToken => "EnforcePaymentCallbackToken";

    public static List<string> PublicKeys =>
    [
        DefaultFundType, DefaultTradeService, FundTypeAvailable, AccountTypeAvailable, CurrencyAvailable, LeverageAvailable,
        LeverageForWholesaleAvailable, TradingPlatformAvailable, DemoTradingPlatformAvailable, IbEnabled, RebateEnabled, WholesaleEnabled,
        AccountDailyReportEnabled, SmsValidationEnabled, WebTraderEnabled, VerificationQuizEnabled, NewYearEvent, ContactInfo,
        TwoFactorAuthSetting, EventShopItemCategoryKey,SymbolsMM,SymbolsOCEX,
        WelcomeInfo, AutoAssignLeadInfo, ApiDomain, UTCEnabled, DefaultAutoCreatePaymentMethod, DefaultAutoCreateWithdrawalPaymentMethod,HoursGapForMT5
    ];

}