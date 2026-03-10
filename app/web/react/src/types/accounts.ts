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
  SuperAdmin = 2,
  TenantAdmin = 10,
  Sales = 100,
  IB = 300,
  Wholesale = 310,
  Client = 400,
  Guest = 1000,
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
  AUD = 36,
  CNY = 156,
  USD = 840,
  USC = 841,
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
  sourceTradeAccountNumber: number;
  targetTradeAccountUid: number;
  targetTradeAccountNumber: number;
  walletId?: number;
  createdOn: string;
  updatedOn: string;
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
}

// 入金查询参数
export interface DepositQueryParams extends PaginationParams {
  period?: string;
  state?: number;
}

// 出金查询参数
export interface WithdrawalQueryParams extends PaginationParams {
  period?: string;
  state?: number;
}

// 交易报告查询参数
export interface TradeQueryParams extends PaginationParams {
  period?: string;
  symbol?: string;
  isClosed?: boolean;
}

// 分页响应
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  size: number;
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
  [CurrencyTypes.USC]: 'US$',
};

// 平台名称映射
export const PlatformNames: Record<number, string> = {
  [PlatformTypes.MetaTrader4]: 'MT4',
  [PlatformTypes.MetaTrader4Demo]: 'MT4 Demo',
  [PlatformTypes.MetaTrader5]: 'MT5',
  [PlatformTypes.MetaTrader5Demo]: 'MT5 Demo',
};


// 获取货币符号
export const getCurrencySymbol = (currencyId: number): string => {
  return CurrencySymbols[currencyId] || '$';
};

// 格式化余额（分转元）
export const formatBalance = (balanceInCents: number, currencyId: number): string => {
  const amount = balanceInCents / 100;
  const symbol = getCurrencySymbol(currencyId);
  return `${symbol} ${amount.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
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
