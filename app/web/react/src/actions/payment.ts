'use server';

import { TronWeb } from 'tronweb';
import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

const USDT_TRC20_CONTRACT = 'TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t';

interface TronScanTransferResponse {
  code?: number;
  tokenInfo?: {
    tokenId?: string;
    tokenAbbr?: string;
    tokenType?: string;
  };
  contractMap?: Record<string, boolean>;
}

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
        bankCountry: string;
        bsb?: string;
        swiftCode?: string;
        bankName: string;
        branchName: string;
        state: string;
        city: string;
        accountNo: string;
        confirmAccountNo?: string;
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
 * 验证 USDT TRC20 钱包地址。
 * 先使用 TronWeb 校验地址格式，再通过 TronScan 确认 TRC20 USDT 网络信息。
 */
export async function validateUsdtWalletAddress(
  address: string
): Promise<ActionResponse<{ valid: boolean }>> {
  const normalizedAddress = address.trim();

  if (!TronWeb.isAddress(normalizedAddress)) {
    return { success: true, data: { valid: false } };
  }

  try {
    const params = new URLSearchParams({
      limit: '10',
      start: '0',
      trc20Id: USDT_TRC20_CONTRACT,
      direction: '2',
      address: normalizedAddress,
    });
    const response = await fetch(
      `https://apilist.tronscanapi.com/api/token_trc20/transfers-with-status?${params}`,
      {
        cache: 'no-store',
        signal: AbortSignal.timeout(10_000),
      }
    );

    if (!response.ok) {
      return { success: false, error: 'Failed to validate USDT wallet address' };
    }

    const data = (await response.json()) as TronScanTransferResponse;
    const isUsdtTrc20 =
      data.code === 200 &&
      data.tokenInfo?.tokenId === USDT_TRC20_CONTRACT &&
      data.tokenInfo?.tokenAbbr === 'USDT' &&
      data.tokenInfo?.tokenType?.toLowerCase() === 'trc20';
    const isWalletAddress = data.contractMap?.[normalizedAddress] !== true;

    return {
      success: true,
      data: { valid: isUsdtTrc20 && isWalletAddress },
    };
  } catch {
    return { success: false, error: 'Failed to validate USDT wallet address' };
  }
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
