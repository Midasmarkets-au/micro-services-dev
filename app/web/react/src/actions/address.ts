'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

// 地址内容结构
export interface AddressContent {
  address: string;
  city: string;
  postalCode: string;
  socialMediaType: string;
  socialMediaAccount: string;
  state: string;
}

// 地址信息结构
export interface AddressInfo {
  hashId: string;
  name: string;
  ccc: string;
  phone: string;
  country: string;
  content: AddressContent;
  createdOn: string;
  updatedOn: string;
}

// 地址列表响应结构
export interface AddressListResponse {
  status: number;
  data: AddressInfo[];
  criteria: {
    partyId: number;
    includeDeleted: boolean | null;
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

// 创建/更新地址的请求参数
export interface AddressPayload {
  name: string;
  ccc: number | string;
  phone: string;
  country: string;
  content: string; // JSON string of AddressContent
}

// 更新地址的请求参数（包含 hashId）
export interface UpdateAddressPayload extends AddressPayload {
  hashId: string;
  createdOn: string;
  updatedOn: string;
}

/**
 * 获取地址列表
 */
export async function getAddressList(): Promise<ActionResponse<AddressInfo[]>> {
  try {
    const response = await apiClient.v1.get<AddressListResponse>(
      '/user/address'
    );

    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch address list' };
  }
}

/**
 * 创建地址
 */
export async function createAddress(
  data: AddressPayload
): Promise<ActionResponse<AddressInfo>> {
  try {
    const response = await apiClient.v1.post<{ data: AddressInfo }>(
      '/user/address',
      data
    );

    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to create address' };
  }
}

/**
 * 更新地址
 */
export async function updateAddress(
  hashId: string,
  data: UpdateAddressPayload
): Promise<ActionResponse<AddressInfo>> {
  try {
    const response = await apiClient.v1.put<{ data: AddressInfo }>(
      `/user/address/${hashId}`,
      data
    );

    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to update address' };
  }
}
