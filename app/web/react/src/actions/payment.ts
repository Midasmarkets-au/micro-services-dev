'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

export interface PaymentInfo {
  id: number;
  paymentPlatform: 100 | 240;
  paymentServiceId: number | null;
  createdOn: string;
  updatedOn: string;
  name: string;
  info:
    | {
        // Bank account (platform 100)
        name: string;
        holder: string;
        bankName: string;
        branchName: string;
        state: string;
        city: string;
        accountNo: string;
        confirmAccountNo?: string;
        bankCountry: string;
      }
    | {
        // USDT wallet (platform 240)
        name: string;
        walletAddress: string;
      };
}

export interface PaymentInfoListResponse {
  status: number;
  data: PaymentInfo[];
  criteria: {
    platform: number | null;
    partyId: number;
    keyword: string | null;
    infoKey: string | null;
    id: number;
    ids: number[];
    page: number;
    size: number;
    total: number;
    pageCount: number;
    hasMore: boolean;
    sortField: string;
    sortFlag: boolean;
  };
  message: string;
}

/**
 * 获取支付信息列表
 */
export async function getPaymentInfoList(): Promise<
  ActionResponse<PaymentInfo[]>
> {
  try {
    const response = await apiClient.v1.get<PaymentInfoListResponse>(
      '/client/payment-info'
    );

    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch payment info' };
  }
}

/**
 * 删除支付信息
 */
export async function deletePaymentInfo(
  id: number
): Promise<ActionResponse> {
  try {
    await apiClient.v1.delete(`/client/payment-info/${id}`);

    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to delete payment info' };
  }
}

/**
 * 添加支付信息
 */
export async function createPaymentInfo(
  data: Omit<PaymentInfo, 'id' | 'createdOn' | 'updatedOn' | 'paymentServiceId'>
): Promise<ActionResponse<PaymentInfo>> {
  try {
    const response = await apiClient.v1.post<{ data: PaymentInfo }>(
      '/client/payment-info',
      data
    );

    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to create payment info' };
  }
}

/**
 * 更新支付信息
 */
export async function updatePaymentInfo(
  id: number,
  data: Omit<PaymentInfo, 'id' | 'createdOn' | 'updatedOn' | 'paymentServiceId'>
): Promise<ActionResponse<PaymentInfo>> {
  try {
    const response = await apiClient.v1.put<{ data: PaymentInfo }>(
      `/client/payment-info/${id}`,
      data
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to update payment info' };
  }
}
