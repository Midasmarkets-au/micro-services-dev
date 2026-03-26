'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import { buildQuery, normalizeAmountList } from '@/lib/utils';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  Wallet,
  TradeAccount,
  WithdrawalTransaction,
  TransferTransaction,
  AdjustTransaction,
  RefundTransaction,
  RebateTransaction,
  DownlineRewardTransaction,
  GetTransactionsParams,
  PaginatedResponse,
  PaymentMethodGroup,
  PaymentMethodGroupInfo,
  UserPaymentInfo,
  WithdrawalSubmitData,
  TransferSubmitData,
  TransferTargetResult,
} from '@/types/wallet';

// ============================================
// 工具函数
// ============================================

const AMOUNT_SUBMIT_MULTIPLIER = 100;

function buildQueryString(params?: GetTransactionsParams): string {
  const query = buildQuery(params);
  return query.startsWith('?') ? query.slice(1) : query;
}

function handleApiError(error: unknown, fallbackMessage: string): ActionResponse<never> {
  if (error instanceof ApiError) {
    return { success: false, error: error.message, errorCode: error.errorCode };
  }
  return { success: false, error: fallbackMessage };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function parseTransactionResponse<T>(responseData: any): PaginatedResponse<T> {
  const raw = responseData?.data || responseData || [];
  const items: T[] = Array.isArray(raw) ? raw : (raw.data || []);
  const normalized = normalizeAmountList(
    items as Record<string, unknown>[],
    ['amount', 'balance', 'prevBalance']
  ) as T[];
  return {
    data: normalized,
    total: responseData?.total ?? items.length,
    page: responseData?.page ?? 1,
    pageSize: responseData?.size ?? responseData?.pageSize ?? 10,
  };
}

// ============================================
// 钱包查询
// ============================================

async function mergeWalletIds(wallets: Wallet[]): Promise<Wallet[]> {
  try {
    const v1Response = await apiClient.v1.get<{ data: { id: number; currencyId: number }[] }>('/client/wallet');
    const v1Wallets = v1Response.data || [];
    const idMap = new Map(v1Wallets.map((w) => [w.currencyId, w.id]));
    return wallets.map((w) => ({ ...w, id: w.id || idMap.get(w.currencyId) || 0 }));
  } catch {
    return wallets;
  }
}

export async function getWalletList(): Promise<ActionResponse<Wallet[]>> {
  try {
    const response = await apiClient.v2.get<{ data: Wallet[] }>('/client/wallet');
    const wallets = response.data || [];
    const normalized = normalizeAmountList(wallets, ['balance', 'balanceInCents']) as Wallet[];
    const withIds = await mergeWalletIds(normalized);
    return { success: true, data: withIds };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch wallet list');
  }
}

export async function getPrimaryWallet(): Promise<ActionResponse<Wallet | null>> {
  try {
    const response = await apiClient.v2.get<{ data: Wallet[] }>('/client/wallet');
    const wallets = response.data || [];
    const primary = wallets.find((w) => w.isPrimary) || null;
    if (primary) {
      const normalized = normalizeAmountList(primary, ['balance', 'balanceInCents']) as Wallet;
      Object.assign(primary, normalized);
    }
    const withIds = await mergeWalletIds(wallets);
    const result = primary ? withIds.find((w) => w.hashId === primary.hashId) || primary : null;
    return { success: true, data: result };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch primary wallet');
  }
}

// ============================================
// 交易记录查询（所有 6 个 Tab 统一处理）
// ============================================

export async function getWithdrawalTransactions(
  walletHashId: string,
  params?: GetTransactionsParams
): Promise<ActionResponse<PaginatedResponse<WithdrawalTransaction>>> {
  try {
    const url = `/client/wallet/${walletHashId}/withdrawal${buildQuery(params)}`;
    const response = await apiClient.v2.get(url);
    return { success: true, data: parseTransactionResponse<WithdrawalTransaction>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch withdrawal transactions');
  }
}

export async function getTransferTransactions(
  walletHashId: string,
  params?: GetTransactionsParams
): Promise<ActionResponse<PaginatedResponse<TransferTransaction>>> {
  try {
    const url = `/client/wallet/${walletHashId}/transfer${buildQuery(params)}`;
    const response = await apiClient.v2.get(url);
    return { success: true, data: parseTransactionResponse<TransferTransaction>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch transfer transactions');
  }
}

export async function getAdjustTransactions(
  walletHashId: string,
  params?: GetTransactionsParams
): Promise<ActionResponse<PaginatedResponse<AdjustTransaction>>> {
  try {
    const url = `/client/wallet/${walletHashId}/adjust${buildQuery(params)}`;
    const response = await apiClient.v2.get(url);
    return { success: true, data: parseTransactionResponse<AdjustTransaction>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch adjust transactions');
  }
}

export async function getRefundTransactions(
  walletHashId: string,
  params?: GetTransactionsParams
): Promise<ActionResponse<PaginatedResponse<RefundTransaction>>> {
  try {
    const url = `/client/wallet/${walletHashId}/refund${buildQuery(params)}`;
    const response = await apiClient.v2.get(url);
    return { success: true, data: parseTransactionResponse<RefundTransaction>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch refund transactions');
  }
}

export async function getRebateTransactions(
  walletHashId: string,
  params?: GetTransactionsParams
): Promise<ActionResponse<PaginatedResponse<RebateTransaction>>> {
  try {
    const url = `/client/wallet/${walletHashId}/rebate${buildQuery(params)}`;
    const response = await apiClient.v2.get(url);
    return { success: true, data: parseTransactionResponse<RebateTransaction>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate transactions');
  }
}

export async function getDownlineRewardTransactions(
  walletHashId: string,
  params?: GetTransactionsParams
): Promise<ActionResponse<PaginatedResponse<DownlineRewardTransaction>>> {
  try {
    const url = `/client/wallet/${walletHashId}/downline-reward-transfer${buildQuery(params)}`;
    const response = await apiClient.v2.get(url);
    return { success: true, data: parseTransactionResponse<DownlineRewardTransaction>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch downline reward transactions');
  }
}

// ============================================
// 取消提现
// ============================================

export async function cancelWithdrawal(
  hashId: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.put<void>(`/client/withdrawal/${hashId}/cancel`, {});
    return { success: true };
  } catch (error) {
    return handleApiError(error, 'Failed to cancel withdrawal');
  }
}

// ============================================
// 出金 (Withdrawal) 相关
// ============================================

export async function getWalletWithdrawGroups(
  targetId: string | number,
  type: 'wallet' | 'account' = 'wallet'
): Promise<ActionResponse<PaymentMethodGroup[]>> {
  try {
    const path =
      type === 'account'
        ? `/client/payment-method/account/${targetId}/withdrawal`
        : `/client/payment-method/wallet/${targetId}/withdrawal`;
    const response = await apiClient.v2.get<{ data: PaymentMethodGroup[] }>(
      path
    );
    console.log('response', response);
    return { success: true, data: response.data || [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch withdrawal payment methods');
  }
}

export async function getWalletWithdrawGroupInfo(
  serviceHashId: string,
  targetId: string | number,
  type: 'wallet' | 'account' = 'wallet'
): Promise<ActionResponse<PaymentMethodGroupInfo>> {
  try {
    const path =
      type === 'account'
        ? `/client/payment-method/${serviceHashId}/account/${targetId}/withdrawal-info`
        : `/client/payment-method/${serviceHashId}/wallet/${targetId}/withdrawal-info`;
    const response = await apiClient.v2.get<{ data: PaymentMethodGroupInfo }>(
      path
    );
    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch withdrawal group info');
  }
}

export async function getUserPaymentInfoList(): Promise<ActionResponse<UserPaymentInfo[]>> {
  try {
    const response = await apiClient.v1.get<{ data: UserPaymentInfo[] }>('/client/payment-info');
    return { success: true, data: response.data || [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch payment info');
  }
}

export async function createUserPaymentInfo(
  data: Record<string, unknown>
): Promise<ActionResponse<UserPaymentInfo>> {
  try {
    const response = await apiClient.v1.post<{ data: UserPaymentInfo }>('/client/payment-info', data);
    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'Failed to create payment info');
  }
}

export async function submitWalletWithdrawal(
  targetId: string | number,
  data: WithdrawalSubmitData,
  type: 'wallet' | 'account' = 'wallet'
): Promise<ActionResponse<unknown>> {
  try {
    const path =
      type === 'account'
        ? `/client/account/${targetId}/withdrawal`
        : `/client/wallet/${targetId}/withdrawal`;
    const response = await apiClient.v2.post(
      path,
      { ...data, amount: Math.round(data.amount * AMOUNT_SUBMIT_MULTIPLIER) }
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to submit withdrawal');
  }
}

// ============================================
// 转账 (Transfer) 相关
// ============================================

export async function getTradeAccounts(
  currencyId?: number,
  fundType?: number
): Promise<ActionResponse<TradeAccount[]>> {
  try {
    const response = await apiClient.v1.get<{ data: TradeAccount[] }>(
      `/client/trade-account${buildQuery({
        CurrencyId: currencyId,
        FundType: fundType,
        includeClosed: false,
      })}`
    );
    return { success: true, data: response.data || [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch trade accounts');
  }
}

export async function searchTransferTarget(
  email: string
): Promise<ActionResponse<TransferTargetResult>> {
  try {
    const response = await apiClient.v1.get<{ data: TransferTargetResult }>(
      `/client/trade-account/check-target-email?email=${encodeURIComponent(email)}`
    );
    return { success: true, data: response.data };
  } catch (error) {
    const result = handleApiError(error, 'Failed to search transfer target');
    return { ...result, skipToast: true };
  }
}

export async function sendTransferVerificationCode(
  authType: string
): Promise<ActionResponse<{ expiresIn: number }>> {
  try {
    const response = await apiClient.v1.post<{ data: { expiresIn: number } }>(
      '/client/transaction/to/downsidewallet/request-code',
      { authType }
    );
    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'Failed to send verification code');
  }
}

export async function transferToTradeAccount(
  data: TransferSubmitData
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post('/client/transaction/to/trade-account', {
      ...data,
      amount: Math.round(data.amount * AMOUNT_SUBMIT_MULTIPLIER),
    });
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to transfer to trade account');
  }
}

export async function transferToWallet(
  data: TransferSubmitData
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post('/client/transaction/to/downsidewallet', {
      ...data,
      amount: Math.round(data.amount * AMOUNT_SUBMIT_MULTIPLIER),
    });
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to transfer to wallet');
  }
}

export async function transferBetweenTradeAccounts(
  data: {
    sourceTradeAccountUid: number;
    targetTradeAccountUid: number;
    amount: number;
    verificationCode?: string;
  }
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post('/client/transaction/between-trade-account', {
      ...data,
      amount: Math.round(data.amount * AMOUNT_SUBMIT_MULTIPLIER),
    });
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to transfer between trade accounts');
  }
}
