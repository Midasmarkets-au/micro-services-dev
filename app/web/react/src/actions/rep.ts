'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import { normalizeAmountList, buildQuery } from '@/lib/utils';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  RepClientListResponse,
  RepListParams,
} from '@/types/rep';
import type {
  SalesTradeListResponse,
  SalesDepositListResponse,
  SalesWithdrawalListResponse,
  SalesTransactionListResponse,
} from '@/types/sales';

function handleApiError(error: unknown, fallbackMessage: string): ActionResponse<never> {
  if (error instanceof ApiError) {
    return { success: false, error: error.message, errorCode: error.errorCode };
  }
  return { success: false, error: fallbackMessage };
}

function unwrapData<T>(response: unknown): T {
  if (
    response !== null &&
    typeof response === 'object' &&
    !Array.isArray(response) &&
    'data' in (response as Record<string, unknown>)
  ) {
    return (response as Record<string, unknown>).data as T;
  }
  return response as T;
}

// ============================================
// Rep Account API
// ============================================

export async function getRepClients(
  repUid: number,
  params?: RepListParams
): Promise<ActionResponse<RepClientListResponse>> {
  try {
    const response = await apiClient.v1.get<RepClientListResponse>(
      `/rep/${repUid}/account${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rep clients');
  }
}

export async function repFuzzySearchAccount(
  repUid: number,
  params?: RepListParams
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/rep/${repUid}/search/account${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to search rep accounts');
  }
}

export async function getRepViewEmailCode(
  repUid: number,
  accountUid: number
): Promise<ActionResponse<number>> {
  try {
    const response = await apiClient.v1.get<number>(
      `/rep/${repUid}/account/${accountUid}/view-email-code`
    );
    return { success: true, data: unwrapData<number>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to get email view code');
  }
}

export async function getRepEmailByCode(
  repUid: number,
  accountUid: number,
  code: number
): Promise<ActionResponse<string>> {
  try {
    const response = await apiClient.v1.get<string>(
      `/rep/${repUid}/account/${accountUid}/view-email/${code}`
    );
    return { success: true, data: unwrapData<string>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to get email');
  }
}

// ============================================
// Rep Client Detail API
// ============================================

export async function getRepClientTransactions(
  repUid: number,
  tradeAccountUid: number,
  params?: RepListParams
): Promise<ActionResponse<{ data: unknown[]; criteria: unknown }>> {
  try {
    const response = await apiClient.v1.get<{ data: unknown[]; criteria: unknown }>(
      `/rep/${repUid}/trade-account/${tradeAccountUid}/transaction${buildQuery(params)}`
    );
    const normalized = {
      ...response,
      data: normalizeAmountList(response.data || []) as unknown[],
    };
    return { success: true, data: normalized };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch client transactions');
  }
}

export async function getRepClientTrades(
  repUid: number,
  tradeAccountUid: number,
  params?: RepListParams
): Promise<ActionResponse<SalesTradeListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesTradeListResponse>(
      `/rep/${repUid}/trade-account/${tradeAccountUid}/trade${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch client trades');
  }
}

export async function getRepTradeReports(
  repUid: number,
  params?: RepListParams
): Promise<ActionResponse<SalesTradeListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesTradeListResponse>(
      `/rep/${repUid}/tradetransaction${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rep trade reports');
  }
}

export async function getRepTransactionReports(
  repUid: number,
  params?: RepListParams
): Promise<ActionResponse<SalesTransactionListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: SalesTransactionListResponse['data'];
      criteria: SalesTransactionListResponse['criteria'];
    }>(`/rep/${repUid}/transaction${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as SalesTransactionListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as SalesTransactionListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rep transaction reports');
  }
}

export async function getRepDeposits(
  repUid: number,
  params?: RepListParams
): Promise<ActionResponse<SalesDepositListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: SalesDepositListResponse['data'];
      criteria: SalesDepositListResponse['criteria'];
    }>(`/rep/${repUid}/deposit${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as SalesDepositListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as SalesDepositListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rep deposits');
  }
}

export async function getRepWithdrawals(
  repUid: number,
  params?: RepListParams
): Promise<ActionResponse<SalesWithdrawalListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: SalesWithdrawalListResponse['data'];
      criteria: SalesWithdrawalListResponse['criteria'];
    }>(`/rep/${repUid}/withdrawal${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as SalesWithdrawalListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as SalesWithdrawalListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rep withdrawals');
  }
}
