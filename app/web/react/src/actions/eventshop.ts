'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  EventDetail,
  EventUserDetail,
  ShopItem,
  ShopOrder,
  PointTransaction,
  RewardRebate,
  CriteriaParams,
} from '@/types/eventshop';
import { normalizeAmountList, buildQuery } from '@/lib/utils';
import Decimal from 'decimal.js';

function normalizeUserPoint(user: EventUserDetail): EventUserDetail {
  // 第一步：normalizeAmountList 对 point 和 totalPoint 除 10000（与 Vue ShopService 一致）
  const step1 = normalizeAmountList(user, ['point', 'totalPoint']) as EventUserDetail;
  // 第二步：再除 10000，拆分可用/不可用积分（与 Vue EventshopIndex 一致）
  const raw = new Decimal(step1.point).div(10000);
  const val2 = new Decimal(raw.toFixed(2));
  const val4 = new Decimal(raw.toFixed(4));
  const diff = val4.minus(val2);
  return {
    ...step1,
    point: val2.toNumber(),
    notavailable: diff.toNumber(),
  };
}

export async function getEventDetail(
  key: string = 'EventShop'
): Promise<ActionResponse<EventDetail>> {
  try {
    const response = await apiClient.v1.get<{ data: EventDetail }>(
      `/client/event/${key}`
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch event detail' };
  }
}

export async function getEventUserDetail(): Promise<ActionResponse<EventUserDetail>> {
  try {
    const response = await apiClient.v1.get<{ data: EventUserDetail }>(
      '/client/event/EventShop/user'
    );
    const normalized = normalizeUserPoint(response.data);
    return { success: true, data: normalized };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch event user detail' };
  }
}

export async function registerEvent(
  key: string = 'EventShop'
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<{ data: unknown }>(
      `/client/event/${key}/apply`,
      {}
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to register for event' };
  }
}

export async function getShopCategories(): Promise<ActionResponse<Record<string, string>>> {
  try {
    const response = await apiClient.v1.get<{ data: Record<string, string> }>(
      '/client/event/shop/item/category-name'
    );
    return { success: true, data: response.data || {} };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch categories' };
  }
}

export async function getShopItems(
  criteria?: CriteriaParams
): Promise<ActionResponse<{ items: ShopItem[]; total: number; page: number; size: number }>> {
  try {
    const query = buildQuery(criteria);
    const response = await apiClient.v1.get<{
      status: number;
      data: ShopItem[];
      criteria: { total: number; page: number; size: number; [key: string]: unknown };
      message: string;
    }>(`/client/event/shop/item${query}`);
    const items = normalizeAmountList(response.data || [], 'point') as ShopItem[];
    return {
      success: true,
      data: {
        items,
        total: response.criteria?.total || 0,
        page: response.criteria?.page || 1,
        size: response.criteria?.size || 16,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch shop items' };
  }
}

export async function getShopItemDetail(
  hashId: string
): Promise<ActionResponse<ShopItem>> {
  try {
    const response = await apiClient.v1.get<{ data: ShopItem }>(
      `/client/event/shop/item/${hashId}`
    );
    const item = normalizeAmountList(response.data, 'point') as ShopItem;
    return { success: true, data: item };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch item detail' };
  }
}

export async function getOrderDetail(
  hashId: string
): Promise<ActionResponse<ShopOrder>> {
  try {
    const response = await apiClient.v1.get<{ data: ShopOrder }>(
      `/client/event/shop/order/${hashId}`
    );
    const order = normalizeAmountList(response.data, 'totalPoint') as ShopOrder;
    return { success: true, data: order };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch order detail' };
  }
}

export async function confirmDelivery(
  hashId: string
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<{ data: unknown }>(
      `/client/event/shop/order/${hashId}/succeed`,
      {}
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to confirm delivery' };
  }
}

export async function updateOrderAddress(
  orderHashId: string,
  addressHashId: string
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<{ data: unknown }>(
      `/client/event/shop/order/${orderHashId}/address/${addressHashId}`,
      {}
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to update order address' };
  }
}

export async function purchaseItem(
  formData: Record<string, string | number>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<{ data: unknown }>(
      '/client/event/shop/order',
      formData
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to purchase item' };
  }
}

interface ListApiResponse<T> {
  status: number;
  data: T[];
  criteria: { total: number; page: number; size: number; [key: string]: unknown };
  message: string;
}


export async function getShopOrderList(
  criteria?: CriteriaParams
): Promise<ActionResponse<{ items: ShopOrder[]; total: number; page: number; size: number }>> {
  try {
    const query = buildQuery(criteria);
    const response = await apiClient.v1.get<ListApiResponse<ShopOrder>>(
      `/client/event/shop/order${query}`
    );
    const items = normalizeAmountList(response.data || [], 'totalPoint') as ShopOrder[];
    return {
      success: true,
      data: {
        items,
        total: response.criteria?.total || 0,
        page: response.criteria?.page || 1,
        size: response.criteria?.size || 10,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch orders' };
  }
}

export async function getPointsHistory(
  criteria?: CriteriaParams
): Promise<ActionResponse<{ items: PointTransaction[]; total: number; page: number; size: number }>> {
  try {
    const query = buildQuery(criteria);
    const response = await apiClient.v1.get<ListApiResponse<PointTransaction>>(
      `/client/event/shop/point/transaction${query}`
    );
    const items = normalizeAmountList(response.data || [], 'point') as PointTransaction[];
    return {
      success: true,
      data: {
        items,
        total: response.criteria?.total || 0,
        page: response.criteria?.page || 1,
        size: response.criteria?.size || 10,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch points history' };
  }
}

export async function getRewardRebateList(
  criteria?: CriteriaParams
): Promise<ActionResponse<{ items: RewardRebate[]; total: number; page: number; size: number }>> {
  try {
    const query = buildQuery(criteria);
    const response = await apiClient.v1.get<ListApiResponse<RewardRebate>>(
      `/client/event/shop/reward/rebate${query}`
    );
    return {
      success: true,
      data: {
        items: response.data || [],
        total: response.criteria?.total || 0,
        page: response.criteria?.page || 1,
        size: response.criteria?.size || 10,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch reward rebates' };
  }
}
