// 完整的用户信息（来自 /user/me 接口）
export interface UserInfo {
  uid: number;
  lastCheckedEventId: number;
  name: string;
  nativeName: string;
  avatar: string;
  createdOn: string;
  email: string;
  twoFactorEnabled: boolean;
  roles: string[];
  permissions: string[];
  ibAccount: string[];
  salesAccount: string[];
  repAccount: string[];
  brokerAccount: string[];
  configurations: string[];
  firstName: string;
  lastName: string;
  language: string;
  timezone: string;
  countryCode: string;
  currency: string;
  ccc: string;
  phoneNumber: string;
  phoneNumberConfirmed: boolean;
  tenancy: string;
  defaultAgentAccount: string;
  defaultSalesAccount: string;
}

// 联系信息
export interface ContactInfo {
  googleMap: string;
  phone: string;
  department: {
    generalInformation: string;
    marketingDepartment: string;
    complianceDepartment: string;
  };
  offices: {
    address: string;
  };
  socialMedia: {
    facebook: { url: string; icon: string };
    twitter: { url: string; icon: string };
    instagram: { url: string; icon: string };
  };
}

// 两步验证交易配置
export interface TwoFactorAuthForTransactions {
  walletToWalletTransfer: boolean;
  walletToTradeAccount: boolean;
  tradeAccountToTradeAccount: boolean;
  withdrawal: boolean;
}

// 站点配置信息（来自 /configuration/public 接口）
export interface SiteConfiguration {
  siteId: number;
  defaultFundType: number;
  defaultTradeService: number;
  fundTypeAvailable: number[];
  accountTypeAvailable: number[];
  currencyAvailable: number[];
  leverageAvailable: number[];
  leverageForWholesaleAvailable: number[];
  tradingPlatformAvailable: number[];
  demoTradingPlatformAvailable: number[];
  ibEnabled: boolean;
  rebateEnabled: boolean;
  wholesaleEnabled: boolean;
  accountDailyReportEnabled: boolean;
  smsValidationEnabled: boolean;
  webTraderEnabled: boolean;
  verificationQuizEnabled: boolean;
  newYearEvent: boolean;
  utcEnabled: boolean;
  contactInfo: {
    value: string; // JSON string, 需要解析为 ContactInfo
  };
  rebateCalculateFrom: string;
  twoFactorAuth: boolean;
  passwordChangedWithinLast24h?: boolean;
  twoFactorAuthForTransactions: TwoFactorAuthForTransactions;
  HoursGapForMT5: number;
}

// 解析后的站点配置（contactInfo 已解析）
export interface ParsedSiteConfiguration extends Omit<SiteConfiguration, 'contactInfo'> {
  contactInfo: ContactInfo;
}

