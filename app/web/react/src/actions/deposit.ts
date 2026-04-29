'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  DepositGroup,
  DepositGroupInfo,
  DepositRequest,
  DepositResponse,
  ExLinkCurrency,
  ExLinkExchangeRatesResponse,
} from '@/types/deposit';

/**
 * 获取入金支付渠道列表
 */
export async function getDepositGroups(
  uid: number
): Promise<ActionResponse<DepositGroup[]>> {
  try {
    const response = await apiClient.v2.get<{ data: DepositGroup[] }>(
      `/client/payment-method/account/${uid}/deposit-group`
    );
    return { success: true, data: response.data || [] };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch deposit groups' };
  }
}

/**
 * 获取支付渠道详情（政策说明 + 配置）
 */
export async function getDepositGroupInfo(
  uid: number,
  group: string,
  type?: string
): Promise<ActionResponse<DepositGroupInfo>> {
  try {
    const params = new URLSearchParams({ group });
    if (type) params.set('type', type);
    const response = await apiClient.v2.get<{ data: DepositGroupInfo }>(
      `/client/payment-method/account/${uid}/deposit-group-info?${params.toString()}`
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch deposit group info' };
  }
}

/**
 * 提交入金请求
 */
export async function postAccountDeposit(
  uid: number,
  payload: DepositRequest
): Promise<ActionResponse<DepositResponse>> {
  try {
    const response = await apiClient.v2.post<{ data: DepositResponse }>(
      `/client/account/${uid}/deposit`,
      payload
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to submit deposit' };
  }
}

/**
 * ExLink Global - 获取支持的币种列表
 */
export async function getExLinkCurrencies(): Promise<ActionResponse<ExLinkCurrency[]>> {
  try {
    const response = await apiClient.v1.get<{ data: ExLinkCurrency[] }>(
      '/client/deposit/exlink/currencies'
    );
    return { success: true, data: response.data || [] };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch ExLink currencies' };
  }
}

/**
 * ExLink Global - 获取实时汇率列表
 */
export async function getExLinkExchangeRates(): Promise<ActionResponse<ExLinkExchangeRatesResponse>> {
  try {
    const response = await apiClient.v1.get<{ data: ExLinkExchangeRatesResponse }>(
      '/client/deposit/exlink/exchange-rates'
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch ExLink exchange rates' };
  }
}

/**
 * 通知服务端 QrCode 支付已完成
 */
export async function postQrCodePaid(
  transactionId: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.post(`/payment/${transactionId}/paid`, null);
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to confirm payment' };
  }
}
