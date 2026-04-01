import type { TradeAccount } from './accounts';

// ============================================
// Rep Account 相关
// ============================================

export interface RepAccount {
  uid: number;
  currencyId: number;
  fundType: number;
  role: number;
  hasLevelRule: boolean;
  name?: string;
  type?: number;
  siteId?: number;
  createdOn?: string;
  alias?: string;
  group?: string;
  tradeAccount?: TradeAccount;
}

// ============================================
// Rep Client 相关
// ============================================

export interface RepClientUser {
  firstName?: string;
  lastName?: string;
  nativeName?: string;
  displayName?: string;
  email?: string;
  avatar?: string;
}

export interface RepClientTradeAccount {
  accountNumber?: number;
  balanceInCents?: number;
  equityInCents?: number;
  creditInCents?: number;
  currencyId?: number;
  leverage?: number;
  serviceId?: number;
}

export interface RepClientAccount {
  uid: number;
  agentUid?: number;
  role: number;
  type: number;
  group?: string;
  code?: string;
  createdOn: string;
  currencyId?: number;
  user: RepClientUser;
  tradeAccount?: RepClientTradeAccount;
}

export interface RepClientListResponse {
  data: RepClientAccount[];
  criteria: RepClientCriteria;
}

export interface RepClientCriteria extends RepListParams {
  total?: number;
  role?: number;
  childParentAccountUid?: number;
  uids?: number[];
}

// ============================================
// 查询参数类型
// ============================================

export interface RepListParams {
  page?: number;
  size?: number;
  keywords?: string;
  searchText?: string;
  sortField?: string;
  sortFlag?: boolean;
  [key: string]: unknown;
}
