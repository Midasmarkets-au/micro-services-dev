// ============================================
// 枚举类型定义 (状态码与旧项目 TransactionStateType 一致)
// ============================================

import { CurrencyTypes } from './accounts';

export enum TransactionType {
  Withdrawal = 'withdrawal',
  Transfer = 'transfer',
  Rebate = 'rebate',
  Refund = 'refund',
  Adjust = 'adjust',
  DownlineReward = 'downline-reward-transfer',
}

export enum TransferState {
  Created = 200,
  Canceled = 205,
  Failed = 206,
  AwaitingApproval = 210,
  Rejected = 215,
  Approved = 220,
  Completed = 250,
}

export enum WithdrawalState {
  Created = 400,
  Canceled = 405,
  Failed = 406,
  TenantApproved = 420,
  TenantRejected = 425,
  PaymentCompleted = 430,
  Completed = 450,
}

export enum RebateState {
  Created = 500,
  Canceled = 505,
  OnHold = 510,
  Released = 520,
  Completed = 550,
}

export enum RefundState {
  Created = 600,
  Completed = 650,
}

export enum WalletAdjustState {
  Created = 700,
  Completed = 750,
}

export enum TransferRewardsState {
  Created = 800,
  Completed = 850,
}

// ============================================
// API 响应类型
// ============================================

export interface Wallet {
  hashId: string;
  id?: number;
  userId: number;
  currencyId: number;
  balanceInCents: number;
  balance: number;
  isPrimary: boolean;
  fundType?: number;
  createdAt: string;
  updatedAt: string;
}

export interface WalletListResponse {
  data: Wallet[];
}

export interface TradeAccount {
  uid: number;
  serviceId: number;
  accountNumber: number;
  currencyId: number;
  leverage: number;
  balance: number;
  balanceInCents: number;
  email?: string;
  name?: string;
  lastSyncedOn?: string;
}

export interface BaseTransaction {
  id?: number;
  hashId?: string;
  walletHashId?: string;
  matterId?: number;
  amount: number;
  currencyId?: number;
  stateId: number;
  createdOn: string;
  updatedOn: string;
  comment?: string;
}

export interface WithdrawalTransaction extends BaseTransaction {
  paymentInfoId?: number;
  paymentMethodName?: string;
  paymentStatus?: number;
  note?: string;
}

export interface TransferTransaction extends BaseTransaction {
  fromWalletHashId?: string;
  toWalletHashId?: string;
  toAccountNumber?: number;
  fromAccountNumber?: number;
  targetAccountNumber?: number;
  senderAccountType?: number;
  receiverAccountType?: number;
  ledgerSide?: number;
  note?: string;
}

export interface AdjustTransaction extends BaseTransaction {
  sourceType?: number;
  reason?: string;
  adminNote?: string;
}

export interface RefundTransaction extends BaseTransaction {
  originalTransactionId?: number;
  reason?: string;
}

export interface RebateTransaction extends BaseTransaction {
  sourceAccountNumber?: number;
  tradeVolume?: number;
  rebateRate?: number;
  ticket?: string;
}

export interface DownlineRewardTransaction extends BaseTransaction {
  flowType?: string;
  sourceType?: number;
  targetType?: number;
  sourceAccountNumber?: number;
  targetAccountNumber?: number;
}

export type Transaction =
  | WithdrawalTransaction
  | TransferTransaction
  | AdjustTransaction
  | RefundTransaction
  | RebateTransaction
  | DownlineRewardTransaction;

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}

// ============================================
// 请求参数类型 (与后端 API 参数名一致)
// ============================================

export interface GetTransactionsParams {
  page?: number;
  size?: number;
  from?: string;
  to?: string;
}

// ============================================
// 出金/转账相关类型
// ============================================

export interface PaymentMethodGroup {
  hashId: string;
  name: string;
  category?: string;
  description?: string;
  icon?: string;
  logo?: string;
  isActive?: boolean;
  range?: [number, number];
}

export interface PaymentMethodGroupInfo {
  hashId: string;
  name: string;
  category?: string;
  notes?: string[];
  policy?: string;
  range?: [number, number];
  fee?: number;
  feeType?: string;
  paymentInfoType?: string;
  fields?: Record<string, unknown>[];
}

export interface UserPaymentInfo {
  id: number;
  type: string;
  paymentPlatform?: number;
  bankName?: string;
  branchName?: string;
  accountName?: string;
  accountNumber?: string;
  bsbCode?: string;
  swiftCode?: string;
  walletAddress?: string;
  state?: string;
  city?: string;
  isDefault?: boolean;
}

export interface WithdrawalSubmitData {
  hashId: string;
  amount: number;
  verificationCode?: string;
  request?: Record<string, unknown>;
}

export interface TransferSubmitData {
  walletId?: number;
  tradeAccountUid?: number;
  amount: number;
  verificationCode?: string;
}

export interface TransferTargetResult {
  walletId: number;
  email: string;
  name: string;
  currencyId: number;
}

// ============================================
// UI 组件 Props 类型
// ============================================

export interface TabConfig {
  key: TransactionType;
  label: string;
  requiresIB?: boolean;
}

export interface TransactionRowProps {
  transaction: Transaction;
  type: TransactionType;
  currencyId: number;
}

export interface WalletPageState {
  isLoading: boolean;
  wallet: Wallet | null;
  currentTab: TransactionType;
  transactions: Transaction[];
  total: number;
  page: number;
  pageSize: number;
  startDate: string | null;
  endDate: string | null;
}

// ============================================
// 状态映射 (基于旧项目 TransactionStateType)
// ============================================

export interface StatusStyle {
  bgColor: string;
  textColor: string;
}

const COMPLETED_STATES = new Set([
  TransferState.Completed,
  WithdrawalState.Completed,
  WithdrawalState.PaymentCompleted,
  RebateState.Completed,
  RebateState.Released,
  RefundState.Completed,
  WalletAdjustState.Completed,
  TransferRewardsState.Completed,
]);

const FAILED_STATES = new Set([
  TransferState.Canceled,
  TransferState.Failed,
  TransferState.Rejected,
  WithdrawalState.Canceled,
  WithdrawalState.Failed,
  WithdrawalState.TenantRejected,
  RebateState.Canceled,
]);

const STYLE_COMPLETED: StatusStyle = {
  bgColor: 'bg-[rgba(0,78,255,0.2)]',
  textColor: 'text-[#004eff]',
};

const STYLE_FAILED: StatusStyle = {
  bgColor: 'bg-[rgba(128,0,32,0.2)]',
  textColor: 'text-[#800020]',
};

const STYLE_PENDING: StatusStyle = {
  bgColor: 'bg-[rgba(255,165,0,0.2)]',
  textColor: 'text-[#ffa500]',
};

export function getStatusStyle(_type: TransactionType, stateId: number): StatusStyle {
  if (COMPLETED_STATES.has(stateId)) return STYLE_COMPLETED;
  if (FAILED_STATES.has(stateId)) return STYLE_FAILED;
  return STYLE_PENDING;
}

const STATUS_TEXT_MAP: Record<number, string> = {
  [TransferState.Created]: 'created',
  [TransferState.Canceled]: 'cancelled',
  [TransferState.Failed]: 'failed',
  [TransferState.AwaitingApproval]: 'pending',
  [TransferState.Rejected]: 'declined',
  [TransferState.Approved]: 'processing',
  [TransferState.Completed]: 'completed',
  [WithdrawalState.Created]: 'created',
  [WithdrawalState.Canceled]: 'cancelled',
  [WithdrawalState.Failed]: 'failed',
  [WithdrawalState.TenantApproved]: 'processing',
  [WithdrawalState.TenantRejected]: 'declined',
  [WithdrawalState.PaymentCompleted]: 'completed',
  [WithdrawalState.Completed]: 'completed',
  [RebateState.Created]: 'created',
  [RebateState.Canceled]: 'cancelled',
  [RebateState.OnHold]: 'pending',
  [RebateState.Released]: 'completed',
  [RebateState.Completed]: 'completed',
  [RefundState.Created]: 'created',
  [RefundState.Completed]: 'completed',
  [WalletAdjustState.Created]: 'created',
  [WalletAdjustState.Completed]: 'completed',
  [TransferRewardsState.Created]: 'created',
  [TransferRewardsState.Completed]: 'completed',
};

export function getStatusText(_type: TransactionType, stateId: number): string {
  return STATUS_TEXT_MAP[stateId] || 'unknown';
}

export function canCancelWithdrawal(stateId: number): boolean {
  return stateId === WithdrawalState.Created;
}

/**
 * 格式化金额 —— 匹配旧项目 toCurrency 逻辑：
 * balance 已经过 normalizeAmountList(÷10000) 处理，单位为"分"
 * 本函数再 ÷100 转换为实际货币单位，使用 Intl.NumberFormat 格式化
 * 有小数部分时显示 4 位，否则 2 位
 */
export function formatWalletAmount(
  balance: number | null | undefined,
  currencyId: number,
  locale: string = 'en-US'
): string {
  const raw = balance ?? 0;
  const value = raw / 100;
  const hasDecimal = value % 1 !== 0;
  const fractionDigits = hasDecimal ? 4 : 2;
  const code = CurrencyTypes[currencyId] || 'USD';

  try {
    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency: code,
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    }).format(value);
  } catch {
    return `${code} ${value.toFixed(fractionDigits)}`;
  }
}

export function formatDateGroup(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

export function groupTransactionsByDate(
  transactions: Transaction[]
): Map<string, Transaction[]> {
  const groups = new Map<string, Transaction[]>();
  transactions.forEach((tx) => {
    const key = formatDateGroup(tx.createdOn);
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key)!.push(tx);
  });
  return groups;
}
