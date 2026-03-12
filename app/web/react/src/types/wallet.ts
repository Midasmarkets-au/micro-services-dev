// ============================================
// 枚举类型定义 (状态码与旧项目 TransactionStateType 一致)
// ============================================

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

export const CURRENCY_CODE_MAP: Record<number, string> = {
  8: 'ALL', 12: 'DZD', 32: 'ARS', 36: 'AUD', 44: 'BSD', 48: 'BHD',
  50: 'BDT', 51: 'AMD', 52: 'BBD', 60: 'BMD', 64: 'BTN', 68: 'BOB',
  72: 'BWP', 84: 'BZD', 90: 'SBD', 96: 'BND', 104: 'MMK', 108: 'BIF',
  116: 'KHR', 124: 'CAD', 132: 'CVE', 136: 'KYD', 144: 'LKR', 152: 'CLP',
  156: 'CNY', 170: 'COP', 174: 'KMF', 188: 'CRC', 191: 'HRK', 192: 'CUP',
  203: 'CZK', 208: 'DKK', 214: 'DOP', 222: 'SVC', 230: 'ETB', 232: 'ERN',
  238: 'FKP', 242: 'FJD', 262: 'DJF', 270: 'GMD', 292: 'GIP', 320: 'GTQ',
  324: 'GNF', 328: 'GYD', 332: 'HTG', 340: 'HNL', 344: 'HKD', 348: 'HUF',
  352: 'ISK', 356: 'INR', 360: 'IDR', 364: 'IRR', 368: 'IQD', 376: 'ILS',
  388: 'JMD', 392: 'JPY', 398: 'KZT', 400: 'JOD', 404: 'KES', 408: 'KPW',
  410: 'KRW', 414: 'KWD', 417: 'KGS', 418: 'LAK', 422: 'LBP', 426: 'LSL',
  430: 'LRD', 434: 'LYD', 446: 'MOP', 454: 'MWK', 458: 'MYR', 462: 'MVR',
  480: 'MUR', 484: 'MXN', 496: 'MNT', 498: 'MDL', 504: 'MAD', 512: 'OMR',
  516: 'NAD', 524: 'NPR', 532: 'ANG', 533: 'AWG', 548: 'VUV', 554: 'NZD',
  558: 'NIO', 566: 'NGN', 578: 'NOK', 586: 'PKR', 590: 'PAB', 598: 'PGK',
  600: 'PYG', 604: 'PEN', 608: 'PHP', 634: 'QAR', 643: 'RUB', 646: 'RWF',
  654: 'SHP', 682: 'SAR', 690: 'SCR', 694: 'SLL', 702: 'SGD', 704: 'VND',
  706: 'SOS', 710: 'ZAR', 728: 'SSP', 748: 'SZL', 752: 'SEK', 756: 'CHF',
  760: 'SYP', 764: 'THB', 776: 'TOP', 780: 'TTD', 784: 'AED', 788: 'TND',
  800: 'UGX', 807: 'MKD', 818: 'EGP', 826: 'GBP', 834: 'TZS', 840: 'USD',
  841: 'USC', 858: 'UYU', 860: 'UZS', 882: 'WST', 886: 'YER', 901: 'TWD',
  927: 'UYW', 928: 'VES', 929: 'MRU', 930: 'STN', 931: 'CUC', 932: 'ZWL',
  933: 'BYN', 934: 'TMT', 936: 'GHS', 938: 'SDG', 940: 'UYI', 941: 'RSD',
  943: 'MZN', 944: 'AZN', 946: 'RON', 947: 'CHE', 948: 'CHW', 949: 'TRY',
  950: 'XAF', 951: 'XCD', 952: 'XOF', 953: 'XPF', 967: 'ZMW', 968: 'SRD',
  969: 'MGA', 970: 'COU', 971: 'AFN', 972: 'TJS', 973: 'AOA', 975: 'BGN',
  976: 'CDF', 977: 'BAM', 978: 'EUR', 979: 'MXV', 980: 'UAH', 981: 'GEL',
  984: 'BOV', 985: 'PLN', 986: 'BRL', 990: 'CLF', 997: 'USN', 999: 'PTS',
};

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
  const code = CURRENCY_CODE_MAP[currencyId] || 'USD';

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
