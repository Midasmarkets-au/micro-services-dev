'use server';
import logger from '@/lib/logger';

import { z } from 'zod';
import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

// ==================== Schema Definitions ====================

const materialRequestSchema = z.object({
  materialType: z.string().min(1, 'materialTypeRequired'),
  description: z.string().min(1, 'descriptionRequired'),
  quantity: z.number().int().positive().optional(),
});

// ==================== Types ====================

export interface MaterialAttachment {
  guid: string;
  url: string;
  fileName: string;
}

export interface MaterialRequestContent {
  materialType: string;
  description: string;
  quantity?: number;
  attachments: MaterialAttachment[];
}

export interface MaterialRequestItem {
  id: number;
  status: number;
  createdOn: string;
  updatedOn: string;
  note: string;
  content: MaterialRequestContent;
}

interface SubmitMaterialRequestData {
  materialType: string;
  description: string;
  quantity?: number;
}

// ==================== Actions ====================

/**
 * 提交物料申请
 */
export async function submitMaterialRequest(
  data: SubmitMaterialRequestData
): Promise<ActionResponse<MaterialRequestItem>> {
  try {
    const validationResult = materialRequestSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: validationResult.error.issues[0]?.message || 'Validation failed',
      };
    }

    const result = await apiClient.v1.post<{ data: MaterialRequestItem }>(
      '/client/materialrequest',
      validationResult.data
    );

    return { success: true, data: result.data };
  } catch (error) {
    logger.error('[submitMaterialRequest] Error:', error);

    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }

    return { success: false, error: 'Submit failed' };
  }
}

/**
 * 获取我的物料申请列表
 */
export async function getMyMaterialRequests(): Promise<ActionResponse<MaterialRequestItem[]>> {
  try {
    const response = await apiClient.v1.get<{ data: MaterialRequestItem[] }>(
      '/client/materialrequest'
    );

    return { success: true, data: response.data || [] };
  } catch (error) {
    logger.error('[getMyMaterialRequests] Error:', error);

    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }

    return { success: false, error: 'Failed to fetch material requests' };
  }
}

/**
 * 获取单条物料申请详情
 */
export async function getMaterialRequestDetail(
  id: number
): Promise<ActionResponse<MaterialRequestItem>> {
  try {
    const result = await apiClient.v1.get<{ data: MaterialRequestItem }>(
      `/client/materialrequest/${id}`
    );

    return { success: true, data: result.data };
  } catch (error) {
    logger.error('[getMaterialRequestDetail] Error:', error);

    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }

    return { success: false, error: 'Failed to fetch material request' };
  }
}

/**
 * 上传物料申请的参考图 / 设计稿
 */
export async function uploadMaterialAttachment(
  id: number,
  file: File
): Promise<ActionResponse<MaterialRequestItem>> {
  try {
    const formData = new FormData();
    formData.append('file', file);

    const result = await apiClient.v1.postFormData<{ data: MaterialRequestItem }>(
      `/client/materialrequest/${id}/attachment`,
      formData
    );

    return { success: true, data: result.data };
  } catch (error) {
    logger.error('[uploadMaterialAttachment] Error:', error);

    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }

    return { success: false, error: 'Upload failed' };
  }
}

/**
 * 撤回尚未审核的物料申请
 */
export async function cancelMaterialRequest(id: number): Promise<ActionResponse> {
  try {
    await apiClient.v1.put(`/client/materialrequest/${id}/cancel`, {});
    return { success: true };
  } catch (error) {
    logger.error('[cancelMaterialRequest] Error:', error);

    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }

    return { success: false, error: 'Cancel failed' };
  }
}
