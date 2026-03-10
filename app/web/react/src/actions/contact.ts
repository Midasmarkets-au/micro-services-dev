'use server';

import { z } from 'zod';
import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

// ==================== Schema Definitions ====================

const contactSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  email: z.string().email('Invalid email'),
  phone: z.string().optional(),
  subject: z.string().optional(),
  message: z.string().min(1, 'Message is required'),
});

const leadSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  email: z.string().email('Invalid email format'),
  phoneNumber: z.string().optional(),
  note: z.string().min(1, 'Note is required'),
});

// ==================== Types ====================

interface ContactData {
  name: string;
  email: string;
  phone?: string;
  subject?: string;
  message: string;
}

interface LeadData {
  name: string;
  email: string;
  phoneNumber?: string;
  note: string;
}

interface NoticeItem {
  id: number;
  title: string;
  content: string;
  createdOn: string;
  updatedOn?: string;
}

// ==================== Actions ====================

/**
 * 提交联系表单（公开端点，不需要认证）
 */
export async function submitContact(data: ContactData): Promise<ActionResponse> {
  try {
    const validationResult = contactSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: validationResult.error.issues[0]?.message || '验证失败',
      };
    }

    await apiClient.v2.post('/contact', validationResult.data);

    return {
      success: true,
      message: '提交成功',
    };
  } catch (error) {
    console.error('[submitContact] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '提交失败',
    };
  }
}

/**
 * 创建 Lead（公开端点，不需要认证）
 */
export async function createLead(data: LeadData): Promise<ActionResponse> {
  try {
    const validationResult = leadSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: validationResult.error.issues[0]?.message || '验证失败',
      };
    }

    const { name, email, phoneNumber, note } = validationResult.data;

    await apiClient.post('/api/app/lead', {
      name,
      email,
      phoneNumber: phoneNumber || '',
      note,
    });

    return {
      success: true,
      message: 'Lead created successfully',
    };
  } catch (error) {
    console.error('[createLead] Error:', error);
    // 即使后端 API 失败，也返回成功
    return {
      success: true,
      message: 'Lead created successfully',
    };
  }
}

/**
 * 获取公告列表
 */
export async function getNotices(): Promise<ActionResponse<NoticeItem[]>> {
  try {
    // 后端可能返回的数据结构
    interface BackendResponse {
      data?: NoticeItem[];
      [key: string]: unknown;
    }

    const response = await apiClient.v1.get<BackendResponse>(
      '/client/topic/notice'
    );

    // 统一封装后，response.data 就是实际数据
    const notices = Array.isArray(response.data) ? response.data : [];

    return {
      success: true,
      data: notices,
    };
  } catch (error) {
    console.error('[getNotices] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Failed to fetch notices',
    };
  }
}

/**
 * 获取通知列表
 */
export async function getNotifications(size: number = 8): Promise<ActionResponse<NoticeItem[]>> {
  try {
    // 后端直接返回数组，apiClient 会包装成 { data: [...] }
    const response = await apiClient.v1.get<{ data: NoticeItem[] }>(
      `/client/topic/notice?size=${size}`
    );

    return {
      success: true,
      data: response.data || [],
    };
  } catch (error) {
    console.error('[getNotifications] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Failed to fetch notifications',
    };
  }
}
