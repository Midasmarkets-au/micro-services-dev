'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  DepositGroup,
  DepositGroupInfo,
  DepositRequest,
  DepositResponse,
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
  group: string
): Promise<ActionResponse<DepositGroupInfo>> {
  try {
    const response = await apiClient.v2.get<{ data: DepositGroupInfo }>(
      `/client/payment-method/account/${uid}/deposit-group-info?group=${encodeURIComponent(group)}`
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
 * 通知服务端 QrCode 支付已完成
 */
export async function postQrCodePaid(
  transactionId: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.post(`/payment/${transactionId}/paid`);
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to confirm payment' };
  }
}
