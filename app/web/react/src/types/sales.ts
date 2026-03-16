import type { TradeAccount } from './accounts';

// ============================================
// Sales Account 相关
// ============================================

export interface SalesAccount {
  uid: number;
  currencyId: number;
  fundType: number;
  role: number;
  hasLevelRule: boolean;
  name?: string;
  type?: number;
  siteId?: number;
  createdOn?: string;
  salesSelfGroupName?: string;
  alias?: string;
  tradeAccount?: TradeAccount;
}

// ============================================
// Sales Client 相关
// ============================================

export interface SalesClientUser {
  firstName?: string;
  lastName?: string;
  nativeName?: string;
  displayName?: string;
  email?: string;
  avatar?: string;
}

export interface SalesClientTradeAccount {
  accountNumber?: number;
  balanceInCents?: number;
  equityInCents?: number;
  creditInCents?: number;
  currencyId?: number;
  leverage?: number;
  serviceId?: number;
}

export interface SalesClientAccount {
  uid: number;
  agentUid?: number;
  role: number;
  type: number;
  group?: string;
  code?: string;
  createdOn: string;
  user: SalesClientUser;
  tradeAccount?: SalesClientTradeAccount;
}

export interface SalesClientListResponse {
  data: SalesClientAccount[];
  criteria: SalesClientCriteria;
}

export interface SalesClientCriteria extends SalesListParams {
  total?: number;
  role?: number;
  relativeLevel?: number;
  childParentAccountUid?: number;
  isActive?: boolean;
}

// ============================================
// Sales Referral / Link 相关
// ============================================

export interface SalesReferralHistory {
  id: number;
  userId: number;
  userName: string;
  email: string;
  avatar?: string;
  accountNumber?: number;
  status: number;
  createdOn: string;
  completedOn?: string;
  user?: SalesClientUser;
}

export interface SalesReferralHistoryResponse {
  data: SalesReferralHistory[];
  criteria: {
    page: number;
    size: number;
    total?: number;
  };
}

export interface SalesLinkSchemaItem {
  cid: number;
  r: number;
}

export interface SalesLinkSchema {
  accountType: number;
  optionName?: string;
  items?: SalesLinkSchemaItem[];
  allowPips?: number[];
  allowCommissions?: number[];
  pips?: number | null;
  commission?: number | null;
  percentage?: number;
}

export interface SalesLinkSummary {
  language?: string;
  name?: string;
  schema?: SalesLinkSchema[];
  allowAccountTypes?: SalesLinkSchema[];
  isAutoCreatePaymentMethod?: number;
  distributionType?: number;
}

export interface SalesLink {
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
  isAutoCreatePaymentMethod?: number;
  displaySummary?: {
    language?: string;
    isAutoCreatePaymentMethod?: number;
    allowAccountTypes?: { accountType: number }[];
    schema?: { accountType: number }[];
  };
  summary?: SalesLinkSummary;
}

export interface SalesLinkListResponse {
  data: SalesLink[];
  criteria: {
    page: number;
    size: number;
    total?: number;
  };
}

export interface SalesLinkDetail {
  id: number;
  code: string;
  name?: string;
  isDefault?: boolean;
  type?: string;
  role?: number;
  status: number;
  serviceType?: number;
  accountType?: number;
  rebateRule?: SalesRebateRule;
  url?: string;
  summary?: SalesLinkSummary;
}

// ============================================
// Sales Rebate 相关
// ============================================

export interface SalesRebateRule {
  id: number;
  name?: string;
  rules?: SalesRebateRuleItem[];
  distributionType?: number;
}

export interface SalesRebateRuleItem {
  symbolCategory?: string;
  symbolCategoryId?: number;
  value?: number;
  type?: number;
}

export interface SalesRebateRuleDetail {
  id: number;
  rules?: SalesRebateRuleItem[];
  distributionType?: number;
  levelSetting?: {
    distributionType: number;
    levels?: number[];
  };
}

export interface SalesDefaultLevelSetting {
  distributionType: number;
  levels?: number[];
  rules?: SalesRebateRuleItem[];
}

// ============================================
// Sales 交易 / 入金 / 出金 / 转账 报表
// ============================================

export interface SalesTradeRecord {
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

export interface SalesTradeListResponse {
  data: SalesTradeRecord[];
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

export interface SalesTargetTradeAccount {
  accountNumber?: number;
  currencyId?: number;
  group?: string;
}

export interface SalesDepositRecord {
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
  user?: SalesClientUser;
  targetTradeAccount?: SalesTargetTradeAccount;
}

export interface SalesDepositListResponse {
  data: SalesDepositRecord[];
  criteria: {
    page: number;
    size: number;
    total?: number;
    totalAmount?: number;
  };
}

export interface SalesWithdrawalRecord {
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
  user?: SalesClientUser;
  targetTradeAccount?: SalesTargetTradeAccount;
}

export interface SalesWithdrawalListResponse {
  data: SalesWithdrawalRecord[];
  criteria: {
    page: number;
    size: number;
    total?: number;
    totalAmount?: number;
  };
}

export interface SalesTransactionAccount {
  accountNumber?: number;
  currencyId?: number;
  group?: string;
}

export interface SalesTransactionRecord {
  id: number;
  hashId?: string;
  amount: number;
  currencyId: number;
  stateId: number;
  type?: number;
  userName?: string;
  userEmail?: string;
  accountNumber?: number;
  fromAccountNumber?: number;
  toAccountNumber?: number;
  sourceAccount?: SalesTransactionAccount;
  targetAccount?: SalesTransactionAccount;
  createdOn: string;
  updatedOn?: string;
  user?: SalesClientUser;
}

export interface SalesTransactionListResponse {
  data: SalesTransactionRecord[];
  criteria: {
    page: number;
    size: number;
    total?: number;
    totalAmount?: number;
  };
}

// ============================================
// Sales Lead 相关
// ============================================

export interface SalesLead {
  id: number;
  name?: string;
  email?: string;
  phoneNumber?: string;
  status?: number;
  hasAssignedToSales?: boolean;
  createdOn: string;
  updatedOn?: string;
  user?: {
    avatar?: string;
    displayName?: string;
    nativeName?: string;
  };
}

export interface SalesLeadListResponse {
  data: SalesLead[];
  criteria: {
    page: number;
    size: number;
    total?: number;
  };
}

export interface SalesLeadDetail extends SalesLead {
  comments?: SalesLeadComment[];
}

export interface SalesLeadComment {
  id: number;
  content: string;
  createdBy?: string;
  createdOn: string;
}

// ============================================
// Sales Statistics 相关
// ============================================

export interface SalesAccountStat {
  newAccountCount?: number;
  newAgentCount?: number;
  rebateCount?: number;
  rebateAmount?: number;
  depositCount?: number;
  depositAmount?: number;
  withdrawCount?: number;
  withdrawAmount?: number;
  tradeCount?: number;
  tradeVolume?: number;
  tradeProfit?: number;
  tradeBySymbol?: Array<{
    symbol: string;
    totalTrade: number;
    totalVolume: number;
    totalProfit: number;
  }>;
}

export interface SalesStatistics {
  summaryStats?: {
    totalTrades?: number;
    totalNetDeposit?: number;
    totalRebate?: number;
    totalDeposit?: number;
    totalWithdrawal?: number;
    totalLots?: number;
  };
  timeSeriesData?: Array<{
    date: string;
    trades: number;
    deposit: number;
    withdrawal: number;
    netDeposit: number;
    rebate: number;
  }>;
  productDistribution?: Array<{
    symbol: string;
    count: number;
    percentage: number;
  }>;
  hierarchyData?: SalesHierarchyNode[];
}

export interface SalesHierarchyNode {
  id: number;
  name?: string;
  type?: string;
  groupCode?: string;
  trades?: number;
  netDeposit?: number;
  deposit?: number;
  withdrawal?: number;
  rebate?: number;
  lots?: number;
  products?: string;
  children?: SalesHierarchyNode[];
}

// ============================================
// Sales Child Stat
// ============================================

export interface SalesChildStat {
  rebateAmounts?: Record<string, number[]>;
  depositAmounts?: Record<string, number[]>;
  netAmounts?: Record<string, number[]>;
  profitAmounts?: Record<string, number[]>;
  withdrawalAmounts?: Record<string, number[]>;
}

// ============================================
// 查询参数类型
// ============================================

export interface SalesListParams {
  page?: number;
  size?: number;
  from?: string;
  to?: string;
  searchText?: string;
  sortField?: string;
  sortFlag?: boolean;
  [key: string]: unknown;
}

export interface SalesFilterParams extends SalesListParams {
  symbol?: string;
  accountNumber?: number;
  tradeAccountUid?: number;
  stateId?: number;
}

// ============================================
// Symbol Category (shared)
// ============================================

export interface SymbolCategory {
  id: number;
  name: string;
  description?: string;
}
