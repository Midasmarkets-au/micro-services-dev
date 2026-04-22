// ============================================
// 枚举类型定义
// ============================================

// 申请状态
export enum ApplicationStatusType {
  AwaitingApproval = 1,
  Approved = 2,
  Rejected = 3,
  Completed = 4,
}

// 申请类型
export enum ApplicationType {
  TradeAccount = 100,
  TradeAccountChangePassword = 102,
  TradeAccountChangeLeverage = 103,
}

// 账户角色
export enum AccountRoleTypes {

  Unknown = 0,
  Rep = 110,
  Broker = 200,
  System = 1,
  SuperAdmin = 2,
  TenantAdmin = 10,
  Sales = 100,
  IB = 300,
  Wholesale = 310,
  Client = 400,
  Guest = 1000
}

// 账户状态
export enum AccountStatusTypes {
  Activate = 0,
  Pause = 1,
  Inactivated = 2,
}

// 账户类型
export enum AccountTypes {
  Standard = 4,
  Raw = 6,
  Affiliate = 12,
  Elite = 22,
}

// 货币类型
export enum CurrencyTypes {
  Invalid = -1,
  Wallet = 0,
  ALL = 8,
  DZD = 12,
  ARS = 32,
  AUD = 36,
  BSD = 44,
  BHD = 48,
  BDT = 50,
  AMD = 51,
  BBD = 52,
  BMD = 60,
  BTN = 64,
  BOB = 68,
  BWP = 72,
  BZD = 84,
  SBD = 90,
  BND = 96,
  MMK = 104,
  BIF = 108,
  KHR = 116,
  CAD = 124,
  CVE = 132,
  KYD = 136,
  LKR = 144,
  CLP = 152,
  CNY = 156,
  COP = 170,
  KMF = 174,
  CRC = 188,
  HRK = 191,
  CUP = 192,
  CZK = 203,
  DKK = 208,
  DOP = 214,
  SVC = 222,
  ETB = 230,
  ERN = 232,
  FKP = 238,
  FJD = 242,
  DJF = 262,
  GMD = 270,
  GIP = 292,
  GTQ = 320,
  GNF = 324,
  GYD = 328,
  HTG = 332,
  HNL = 340,
  HKD = 344,
  HUF = 348,
  ISK = 352,
  INR = 356,
  IDR = 360,
  IRR = 364,
  IQD = 368,
  ILS = 376,
  JMD = 388,
  JPY = 392,
  KZT = 398,
  JOD = 400,
  KES = 404,
  KPW = 408,
  KRW = 410,
  KWD = 414,
  KGS = 417,
  LAK = 418,
  LBP = 422,
  LSL = 426,
  LRD = 430,
  LYD = 434,
  MOP = 446,
  MWK = 454,
  MYR = 458,
  MVR = 462,
  MUR = 480,
  MXN = 484,
  MNT = 496,
  MDL = 498,
  MAD = 504,
  OMR = 512,
  NAD = 516,
  NPR = 524,
  ANG = 532,
  AWG = 533,
  VUV = 548,
  NZD = 554,
  NIO = 558,
  NGN = 566,
  NOK = 578,
  PKR = 586,
  PAB = 590,
  PGK = 598,
  PYG = 600,
  PEN = 604,
  PHP = 608,
  QAR = 634,
  RUB = 643,
  RWF = 646,
  SHP = 654,
  SAR = 682,
  SCR = 690,
  SLL = 694,
  SGD = 702,
  VND = 704,
  SOS = 706,
  ZAR = 710,
  SSP = 728,
  SZL = 748,
  SEK = 752,
  CHF = 756,
  SYP = 760,
  THB = 764,
  TOP = 776,
  TTD = 780,
  AED = 784,
  TND = 788,
  UGX = 800,
  MKD = 807,
  EGP = 818,
  GBP = 826,
  TZS = 834,
  USD = 840,
  USC = 841,
  UYU = 858,
  UZS = 860,
  WST = 882,
  YER = 886,
  TWD = 901,
  UYW = 927,
  VES = 928,
  MRU = 929,
  STN = 930,
  CUC = 931,
  ZWL = 932,
  BYN = 933,
  TMT = 934,
  GHS = 936,
  SDG = 938,
  UYI = 940,
  RSD = 941,
  MZN = 943,
  AZN = 944,
  RON = 946,
  CHE = 947,
  CHW = 948,
  TRY = 949,
  XAF = 950,
  XCD = 951,
  XOF = 952,
  XPF = 953,
  ZMW = 967,
  SRD = 968,
  MGA = 969,
  COU = 970,
  AFN = 971,
  TJS = 972,
  AOA = 973,
  BGN = 975,
  CDF = 976,
  BAM = 977,
  EUR = 978,
  MXV = 979,
  UAH = 980,
  GEL = 981,
  BOV = 984,
  PLN = 985,
  BRL = 986,
  CLF = 990,
  USN = 997,
  PTS = 999
}

// 平台类型
export enum PlatformTypes {
  MetaTrader4 = 20,
  MetaTrader4Demo = 21,
  MetaTrader5 = 30,
  MetaTrader5Demo = 31,
}

// 服务类型 (用于判断 WebTrader 链接)
export enum ServiceTypes {
  MetaTrader4Co = 10,
  MetaTrader4CoDemo = 11,
  MetaTrader4 = 20,
  MetaTrader4Demo = 21,
  MetaTrader5 = 30,
  MetaTrader5Demo = 31,
  CTrader = 40,
}

// ============================================
// API 响应类型
// ============================================

// 交易账户信息
export interface TradeAccount {
  accountNumber: number;
  serviceId: number;
  currencyId: number;
  leverage: number;
  balanceInCents: number;
  equityInCents: number;
  creditInCents: number;
}

// 真实账户
export interface Account {
  id: number;
  uid: number;
  hasTradeAccount: boolean;
  type: number;
  fundType: number;
  currencyId: number;
  role?: number;
  name?: string;
  siteId?: number;
  hasLevelRule?: boolean;
  code?: string;
  group?: string;
  alias?: string;
  createdOn?: string;
  tradeAccount?: TradeAccount;
}

// 账户列表响应
export interface AccountResponse {
  data: Account[];
  total: number;
  page: number;
  pageSize: number;
}

// 申请补充信息
export interface ApplicationSupplement {
  serviceId: number;
  accountType: number;
  currencyId: number;
  leverage: number;
}

// 待审核申请
export interface Application {
  id: number;
  status: number;
  type: number;
  supplement: ApplicationSupplement;
}

// 申请列表响应
export interface ApplicationResponse {
  data: Application[];
}

// 模拟账户
export interface DemoAccount {
  id: number;
  accountNumber: number;
  serviceId: number;
  currencyId: number;
  leverage: number;
  balance: number;
  balanceInCents: number;
  expireOn: string | null;
}

// 模拟账户列表响应
export interface DemoAccountResponse {
  data: DemoAccount[];
}

// 服务/平台信息
export interface Service {
  id: number;
  name: string;
  platform: number;
  priority: number;
  description: string;
}

// 服务映射
export interface ServiceMapItem {
  serverName: string;
  platform: number;
  platformName: string;
}

export type ServiceMap = Record<number, ServiceMapItem>;

// 交易平台配置
export interface TradingPlatformConfig {
  serviceId: number;
  platform: number;
}

// 账户配置
export interface AccountConfig {
  tradingPlatformAvailable: TradingPlatformConfig[];
  referCode: string;
  currencyAvailable: number[];
  accountTypeAvailable: number[];
  leverageAvailable: number[];
}

// ============================================
// 请求参数类型
// ============================================

// 获取账户列表参数
export interface GetAccountsParams {
  hasTradeAccount?: boolean;
  status?: number;
  roles?: number[];
  uids?: number[];
}

// 获取申请列表参数
export interface GetApplicationsParams {
  statuses?: number[];
  type?: number;
}

// 创建真实账户参数
export interface CreateLiveAccountParams {
  serviceId: number;
  platform: number;
  accountType: number;
  currencyId: number;
  leverage: number;
  referCode?: string;
}

// 创建模拟账户参数
export interface CreateDemoAccountParams {
  platform: number;
  accountType: number;
  currencyId: number;
  leverage: number;
  amount: number;
}

// ============================================
// 账户详情相关类型
// ============================================

// 交易账户交易记录 (Transaction/Transfer)
export interface AccountTransaction {
  id: number;
  hashId: string;
  stateId: number;
  amount: number;
  amountInCents: number;
  sourceTradeAccountUid: number;
  sourceAccountNumber: number;
  targetAccountNumber: number;
  sourceTradeAccountNumber: number;
  targetTradeAccountUid: number;
  targetTradeAccountNumber: number;
  walletId?: number;
  createdOn: string;
  updatedOn: string;
  statedOn: string;
}

// 转账状态 (matches old project TransactionStateType)
export enum TransferState {
  TransferCreated = 200,
  TransferCanceled = 205,
  TransferFailed = 206,
  TransferAwaitingApproval = 210,
  TransferRejected = 215,
  TransferApproved = 220,
  TransferCompleted = 250,
}

export enum TransactionAccountType {
  Wallet = 1,
  TradeAccount = 2,
}
// 入金记录
export interface AccountDeposit {
  id: number;
  hashId: string;
  stateId: number;
  paymentMethodName: string;
  paymentStatus: number;
  amount: number;
  amountInCents: number;
  currencyId: number;
  createdOn: string;
  updatedOn: string;
}

// 入金状态 (matches old project TransactionStateType)
export enum DepositState {
  DepositCreated = 300,
  DepositCanceled = 305,
  DepositFailed = 306,
  DepositPaymentCompleted = 310,
  DepositTenantApproved = 330,
  DepositTenantRejected = 335,
  DepositCallbackComplete = 345,
  DepositCompleted = 350,
}

// 出金记录
export interface AccountWithdrawal {
  id: number;
  hashId: string;
  stateId: number;
  paymentMethodName: string;
  paymentStatus: number;
  amount: number;
  amountInCents: number;
  currencyId: number;
  createdOn: string;
  updatedOn: string;
  statedOn?: string;
}

// 出金状态 (matches old project TransactionStateType)
export enum WithdrawalState {
  WithdrawalCreated = 400,
  WithdrawalCanceled = 405,
  WithdrawalFailed = 406,
  WithdrawalTenantApproved = 420,
  WithdrawalTenantRejected = 425,
  WithdrawalPaymentCompleted = 430,
  WithdrawalCompleted = 450,
}

// 交易报告 (Trade)
export interface AccountTrade {
  id: number;
  ticket: number;
  symbol: string;
  type: number;
  volume: number;
  openTime: string;
  openPrice: number;
  sl: number;
  tp: number;
  closeTime: string;
  closeAt: string;
  openAt: string;
  closePrice: number;
  commission: number;
  swap: number;
  profit: number;
}

// 交易类型
export enum TradeType {
  Buy = 0,
  Sell = 1,
}

// 分页参数
export interface PaginationParams {
  page?: number;
  size?: number;
}

// 交易记录查询参数
export interface TransactionQueryParams extends PaginationParams {
  period?: string;
  type?: number;
  stateIds?: number[];
}

// 入金查询参数
export interface DepositQueryParams extends PaginationParams {
  period?: string;
  stateIds?: number[];
}

// 出金查询参数
export interface WithdrawalQueryParams extends PaginationParams {
  period?: string;
  stateIds?: number[];
}

// 交易报告查询参数
export interface TradeQueryParams extends PaginationParams {
  period?: string;
  from?: string;
  to?: string;
  symbol?: string;
  isClosed?: boolean;
}

// 分页响应
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  size: number;
  criteria?: {
    total?: number;
    page?: number;
    size?: number;
  };
}

export interface AccountTradeCriteria {
  page: number;
  size: number;
  total?: number;
  isClosed?: boolean;
  pageTotalVolume?: number;
  pageTotalCommission?: number;
  pageTotalSwap?: number;
  pageTotalProfit?: number;
  totalVolume?: number;
  totalCommission?: number;
  totalSwap?: number;
  totalProfit?: number;
}

export interface AccountTradeListResponse {
  data: AccountTrade[];
  criteria: AccountTradeCriteria;
}

// ============================================
// UI 组件 Props 类型
// ============================================

// 卡片类型
export type CardType = 'account' | 'application' | 'demo';

// TradeAccountCard Props
export interface TradeAccountCardProps {
  item: Account | Application | DemoAccount;
  type: CardType;
  serviceMap: ServiceMap;
  showWebtrader?: boolean;
  buttonText?: string;
  buttonHandler?: () => void;
  disableSetting?: boolean;
  wholesaleEnable?: boolean;
  onResetPassword?: () => void;
  onChangeLeverage?: () => void;
  onDeposit?: () => void;
  onRefresh?: () => void;
}

// 页面状态
export interface AccountsPageState {
  isLoading: boolean;
  currentTab: 'RealAccounts' | 'DemoAccounts';
  liveAccounts: Account[];
  pendingApplications: Application[];
  demoAccounts: DemoAccount[];
  serviceMap: ServiceMap;
  wholesaleEnable: boolean;
}

// ============================================
// 工具函数类型
// ============================================

// 货币符号映射
export const CurrencySymbols: Record<number, string> = {
  [CurrencyTypes.AUD]: 'A$',
  [CurrencyTypes.CNY]: '¥',
  [CurrencyTypes.USD]: 'US$',
  [CurrencyTypes.USC]: 'USC$',
};

export const getCurrencyCode = (currencyId: number): string => {
  const code = CurrencyTypes[currencyId as CurrencyTypes];
  return typeof code === 'string' ? code : 'USD';
};

// 平台名称映射
export const PlatformNames: Record<number, string> = {
  [PlatformTypes.MetaTrader4]: 'MT4',
  [PlatformTypes.MetaTrader4Demo]: 'MT4 Demo',
  [PlatformTypes.MetaTrader5]: 'MT5',
  [PlatformTypes.MetaTrader5Demo]: 'MT5 Demo',
};


// 获取货币符号
export const getCurrencySymbol = (currencyId: number, locale = 'en-US'): string => {
  const code = getCurrencyCode(currencyId);

  // USC 不是标准 ISO 货币码，Intl 无法直接格式化，回退到既有显示。
  if (code === 'USC') {
    return CurrencySymbols[currencyId] || 'USC$';
  }

  try {
    const parts = new Intl.NumberFormat(locale, {
      style: 'currency',
      currency: code,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).formatToParts(0);

    const symbol = parts.find((part) => part.type === 'currency')?.value;
    return symbol || CurrencySymbols[currencyId] || '$';
  } catch {
    return CurrencySymbols[currencyId] || '$';
  }
};

// 格式化余额（分转元）
export const formatBalance = (balanceInCents: number, currencyId: number, locale = 'en-US'): string => {
  const amount = balanceInCents / 100;
  const symbol = getCurrencySymbol(currencyId, locale);
  return `${symbol} ${amount.toLocaleString(locale, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
};

// 获取货币图标
export const getCurrencyFlag = (currencyId: number): string => {
  return `/images/currency/${currencyId}.svg`;
};

// 获取平台名称
export const getPlatformName = (platform: number): string => {
  return PlatformNames[platform] || 'Unknown';
};

// 是否显示 WebTrader
export const showWebTrader = (serviceId: number): boolean => {
  return Object.values(ServiceTypes).includes(serviceId as ServiceTypes);
};

// 获取 WebTrader 链接
export const getWebTraderLink = (
  serviceId: number,
  accountNumber: number
): string => {
  if (
    serviceId === ServiceTypes.MetaTrader5 ||
    serviceId === ServiceTypes.MetaTrader5Demo
  ) {
    return `/webTrader5/${accountNumber}`;
  }
  return `/webTrader/${accountNumber}/${serviceId}`;
};


