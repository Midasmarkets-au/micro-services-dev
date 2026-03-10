import type { CurrencyTypes, TradeAccount } from './accounts';

// ============================================
// IB Agent 账户相关
// ============================================

export interface AgentAccount {
  uid: number;
  currencyId: number;
  fundType: number;
  role: number;
  hasLevelRule: boolean;
  name?: string;
  type?: number;
  siteId?: number;
  createdOn?: string;
  agentSelfGroupName?: string;
  salesGroupName?: string;
  alias?: string;
  tradeAccount?: TradeAccount;
}

// ============================================
// IB 客户相关
// ============================================

export interface IBClientUser {
  firstName?: string;
  lastName?: string;
  nativeName?: string;
  displayName?: string;
  email?: string;
  avatar?: string;
}

export interface IBClientTradeAccount {
  accountNumber?: number;
  balanceInCents?: number;
  equityInCents?: number;
  creditInCents?: number;
  currencyId?: number;
  leverage?: number;
  serviceId?: number;
}

export interface IBClientAccount {
  uid: number;
  agentUid?: number;
  role: number;
  type: number;
  group?: string;
  code?: string;
  createdOn: string;
  user: IBClientUser;
  tradeAccount?: IBClientTradeAccount;
}

export interface IBClientListResponse {
  data: IBClientAccount[];
  criteria: IBClientCriteria;
}

export interface IBClientCriteria extends IBListParams {
  total?: number;
  role?: number;
  relativeLevel?: number;
  childParentAccountUid?: number;
  isActive?: boolean;
}

// ============================================
// IB Referral 相关
// ============================================

export interface IBReferralHistory {
  id: number;
  userId: number;
  userName: string;
  email: string;
  avatar?: string;
  accountNumber?: number;
  status: number;
  createdOn: string;
  completedOn?: string;
  user?: IBClientUser;
}

export interface IBReferralHistoryResponse {
  data: IBReferralHistory[];
  criteria: {
    page: number;
    size: number;
    total?: number;
  };
}

export interface IBReferralCode {
  id: number;
  code: string;
  name?: string;
  isDefault?: boolean;
  status: number;
  type?: string;
  createdOn: string;
  url?: string;
}

// ============================================
// IB Link 相关
// ============================================

export interface IBLinkSchemaItem {
  accountType: number;
  optionName?: string;
  pips?: number;
  commission?: number;
  percentage?: number;
  items?: { cid: number; r: number }[];
}

export interface IBLinkSummary {
  name?: string;
  schema?: IBLinkSchemaItem[];
  allowAccountTypes?: IBLinkSchemaItem[];
  language?: string;
  isAutoCreatePaymentMethod?: number;
  distributionType?: number;
  percentageSchema?: {
    optionName?: string;
    percentageSetting?: number[];
  };
}

export interface IBReferralSupplement {
  code: string;
  serviceType: number;
  summary: IBLinkSummary;
}

export interface IBLink {
  id: number;
  code: string;
  name?: string;
  isDefault?: boolean | number;
  type?: string;
  status: number;
  role?: number;
  serviceType?: number;
  createdOn: string;
  url?: string;
  accountType?: number;
  rebateRuleId?: number;
  summary?: IBLinkSummary;
}

export interface IBLinkListResponse {
  data: IBLink[];
  criteria: {
    page: number;
    size: number;
    total?: number;
  };
}

export interface IBLinkDetail {
  id: number;
  code: string;
  name?: string;
  isDefault?: boolean;
  type?: string;
  role?: number;
  status: number;
  accountType?: number;
  rebateRule?: IBRebateRule;
  url?: string;
}

// ============================================
// IB Rebate 相关
// ============================================

export interface IBRebateRule {
  id: number;
  name?: string;
  rules?: IBRebateRuleItem[];
  distributionType?: number;
}

export interface IBRebateRuleItem {
  symbolCategory?: string;
  symbolCategoryId?: number;
  value?: number;
  type?: number;
}

export interface IBRebateRuleDetail {
  id: number;
  rules?: IBRebateRuleItem[];
  distributionType?: number;
  levelSetting?: {
    distributionType: number;
    levels?: number[];
  };
}

export interface IBAllowedAccount {
  accountType: number;
  optionName?: string;
  percentage?: number;
  allowPips?: number[];
  allowCommissions?: number[];
  pips?: number;
  commission?: number;
  items?: { cid: number; r: number }[];
}

export interface IBRebateRuleDetailFull {
  isRoot: boolean;
  calculatedLevelSetting: {
    allowedAccounts: IBAllowedAccount[];
  };
}

export interface IBProductCategory {
  key: number;
  value: string;
}

export interface IBDefaultLevelSettingOption {
  optionName?: string;
  OptionName?: string;
  category?: Record<number, number>;
  Category?: Record<number, number>;
  allowPipSetting?: Record<number, { items: Record<number, number> }>;
  allowCommissionSetting?: Record<number, { items: Record<number, number> }>;
}

export type IBDefaultLevelSettingMap = Record<number, IBDefaultLevelSettingOption[]>;

export interface IBAccountLevelSetting {
  accountType: number;
  optionName?: string;
  percentage?: number;
  allowPips: number[];
  allowCommissions: number[];
  pips?: number;
  commission?: number;
  items: Record<number, number>;
  selected: boolean;
}

export interface IBRebateTradeInfo {
  accountName?: string;
  accountNumber?: number;
  symbol?: string;
  ticket?: string;
  currencyId?: number;
  volume?: number;
  closeAt?: string;
}

export interface IBRebateRecord {
  id: number;
  hashId?: string;
  amount: number;
  currencyId: number;
  stateId: number;
  rebateRate?: number;
  sourceAccountNumber?: number;
  createdOn: string;
  updatedOn?: string;
  trade?: IBRebateTradeInfo;
}

export interface IBRebateListResponse {
  data: IBRebateRecord[];
  criteria: {
    page: number;
    size: number;
    total?: number;
    totalAmount?: number;
    pageTotalAmount?: number;
    pageTotalVolume?: number;
    totalVolume?: number;
  };
}

export interface IBRebateStatItem {
  amounts: Record<string, number[]>;
}

export interface IBRebateDistribution {
  id: number;
  symbolCategory?: string;
  value?: number;
  type?: number;
}

// ============================================
// IB 交易/入金/出金报表相关
// ============================================

export interface IBTradeRecord {
  id: number;
  ticket?: number;
  symbol?: string;
  type?: number;
  cmd?: number;
  volume?: number;
  openTime?: string;
  openAt?: string;
  openPrice?: number;
  closeTime?: string;
  closeAt?: string;
  closePrice?: number;
  sl?: number;
  tp?: number;
  digits?: number;
  commission?: number;
  swap?: number;
  swaps?: number;
  profit?: number;
  accountNumber?: number;
  serviceId?: number;
}

export interface IBTradeListResponse {
  data: IBTradeRecord[];
  criteria: {
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
  };
}

export interface IBTargetTradeAccount {
  accountNumber?: number;
  currencyId?: number;
  group?: string;
}

export interface IBDepositRecord {
  id: number;
  hashId?: string;
  amount: number;
  currencyId: number;
  stateId: number;
  paymentMethodName?: string;
  userName?: string;
  userEmail?: string;
  accountNumber?: number;
  createdOn: string;
  updatedOn?: string;
  user?: IBClientUser;
  targetTradeAccount?: IBTargetTradeAccount;
}

export interface IBDepositListResponse {
  data: IBDepositRecord[];
  criteria: {
    page: number;
    size: number;
    total?: number;
    totalAmount?: number;
  };
}

export interface IBWithdrawalRecord {
  id: number;
  hashId?: string;
  amount: number;
  currencyId: number;
  stateId: number;
  paymentMethodName?: string;
  userName?: string;
  userEmail?: string;
  accountNumber?: number;
  createdOn: string;
  updatedOn?: string;
  user?: IBClientUser;
  targetTradeAccount?: IBTargetTradeAccount;
}

export interface IBWithdrawalListResponse {
  data: IBWithdrawalRecord[];
  criteria: {
    page: number;
    size: number;
    total?: number;
    totalAmount?: number;
  };
}

// ============================================
// IB Report Dashboard 相关
// ============================================

export interface IBReportValue {
  currencyId: number;
  amount: number;
}

export interface IBRebateDailySeries {
  date: string;
  totalValue: number;
}

export interface IBLatestDeposit {
  id: number;
  amount: number;
  currencyId: number;
  userName?: string;
  userEmail?: string;
  createdOn: string;
  user?: IBClientUser;
}

export interface IBChildStat {
  rebateAmounts?: Record<string, number[]>;
  depositAmounts?: Record<string, number[]>;
  netAmounts?: Record<string, number[]>;
  profitAmounts?: Record<string, number[]>;
  withdrawalAmounts?: Record<string, number[]>;
}

// ============================================
// IB Report 请求相关
// ============================================

export interface IBReportRequest {
  id: number;
  status: number;
  month?: string;
  year?: number;
  downloadLink?: string;
  createdOn: string;
}

export interface IBReportRequestResponse {
  data: IBReportRequest[];
  criteria: {
    page: number;
    size: number;
    total?: number;
  };
}

// ============================================
// 符号分类相关
// ============================================

export interface SymbolCategory {
  id: number;
  name: string;
  description?: string;
}

export interface SymbolCategoryItem {
  key: number;
  value: string;
}

// ============================================
// IB 默认 Level 设置
// ============================================

export interface IBDefaultLevelSetting {
  distributionType: number;
  levels?: number[];
  rules?: IBRebateRuleItem[];
}

// ============================================
// 查询参数类型
// ============================================

export interface IBListParams {
  page?: number;
  size?: number;
  from?: string;
  to?: string;
  searchText?: string;
  sortField?: string;
  sortFlag?: boolean;
  [key: string]: unknown;
}

export interface IBTradeFilterParams extends IBListParams {
  symbol?: string;
  accountNumber?: number;
  tradeAccountUid?: number;
}
