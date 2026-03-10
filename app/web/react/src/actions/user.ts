'use server';

import { cookies } from 'next/headers';
import { apiClient, ApiError, API_BASE_URL } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

// ==================== Types ====================

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type UserInfo = any;

interface Configuration {
  [key: string]: unknown;
}

interface AvatarUploadResponse {
  id: number;
  url: string;
  party: {
    avatar: string;
    [key: string]: unknown;
  };
  [key: string]: unknown;
}

// ==================== Actions ====================

/**
 * 获取当前用户信息
 */
export async function getUserInfo(): Promise<ActionResponse<UserInfo>> {
  try {
    const response = await apiClient.v1.get<{ data: UserInfo }>('/user/me');

    return {
      success: true,
      data: response.data,
    };
  } catch (error) {
    console.error('[getUserInfo] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '获取用户信息失败',
    };
  }
}

/**
 * 获取站点配置（需要认证）
 */
export async function getConfiguration(): Promise<ActionResponse<Configuration>> {
  try {
    const response = await apiClient.v1.get<{ data: Configuration }>('/configuration/public');

    return {
      success: true,
      data: response.data,
    };
    console.log('response', response);
  } catch (error) {
    console.error('[getConfiguration] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '获取配置失败',
    };
  }
}

/**
 * 上传用户头像
 * POST api/v1/user/profile/avatar
 */
export async function uploadAvatar(file: File): Promise<ActionResponse<AvatarUploadResponse>> {
  try {
    // 创建 FormData
    const formData = new FormData();
    formData.append('avatar', file);

    // 使用 postFormData 方法上传
    const response = await apiClient.v1.postFormData<{ data: AvatarUploadResponse }>(
      '/user/profile/avatar',
      formData
    );

    return {
      success: true,
      data: response.data,
    };
  } catch (error) {
    console.error('[uploadAvatar] Error:', error);

    if (error instanceof ApiError) {
      // 打印完整的错误详情
      console.error('[uploadAvatar] ApiError details:', {
        message: error.message,
        statusCode: error.statusCode,
        errors: JSON.stringify(error.errors, null, 2),
        errorCode: error.errorCode,
      });
      
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '上传头像失败',
    };
  }
}

/**
 * 发送手机号验证码
 */
export async function sendPhoneVerificationCode(
  regionCode: string,
  phone: string
): Promise<ActionResponse<void>> {
  try {
    // 移除 + 号
    const cleanRegionCode = regionCode.replace('+', '');

    await apiClient.v1.post(
      `/user/verification/mobile/${cleanRegionCode}/${phone}/send`,
      {}
    );

    return {
      success: true,
    };
  } catch (error) {
    console.error('[sendPhoneVerificationCode] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '发送验证码失败',
    };
  }
}

/**
 * 更新手机号
 * PUT api/v1/user/verification/mobile/${regionCode}/${phone}/${code}
 * code 为可选参数，如果不传则不包含在 URL 中
 */
export async function updatePhoneNumber(
  regionCode: string,
  phone: string,
  code?: string
): Promise<ActionResponse<void>> {
  try {
    // 构建 URL：如果有验证码则包含，否则不包含
    const url = code
      ? `/user/verification/mobile/${regionCode}/${phone}/${code}`
      : `/user/verification/mobile/${regionCode}/${phone}/undefined`;

    await apiClient.v1.put(url, {});

    return {
      success: true,
    };
  } catch (error) {
    console.error('[updatePhoneNumber] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '更新手机号失败',
    };
  }
}

/**
 * 启用双重认证
 * PUT api/v1/user/enable-2fa
 * @param code 验证码，为空时触发发送验证码邮件
 */
export async function enable2FA(code: string): Promise<ActionResponse<void>> {
  try {
    // 如果 code 为空，发送空对象触发发送验证码邮件；否则发送验证码
    const body = code ? { code } : {};
    await apiClient.v1.put<void>('/user/enable-2fa', body);

    return {
      success: true,
    };
  } catch (error) {
    console.error('[enable2FA] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '启用双重认证失败',
    };
  }
}

/**
 * 禁用双重认证
 * PUT api/v1/user/disable-2fa
 * @param code 验证码，为空时触发发送验证码邮件
 */
export async function disable2FA(code: string): Promise<ActionResponse<void>> {
  try {
    // 如果 code 为空，发送空对象触发发送验证码邮件；否则发送验证码
    const body = code ? { code } : {};
    await apiClient.v1.put<void>('/user/disable-2fa', body);

    return {
      success: true,
    };
  } catch (error) {
    console.error('[disable2FA] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '禁用双重认证失败',
    };
  }
}

/**
 * 更新用户语言偏好
 * PUT api/v1/user/profile/language
 */
export async function updateUserLanguage(language: string): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.put<void>('/user/profile/language', { language });
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to update language' };
  }
}

/**
 * 修改登录密码
 * POST api/v1/auth/password/change
 */
export async function changePassword(
  currentPassword: string,
  newPassword: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.post<void>(
      '/auth/password/change',
      { currentPassword, newPassword }
    );

    return {
      success: true,
    };
  } catch (error) {
    console.error('[changePassword] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: '修改密码失败',
    };
  }
}

/**
 * 获取媒体文件 URL（带认证 token）
 * @param guid 文件 GUID
 * @returns 带 access_token 的完整 URL
 */
export async function getMediaUrl(guid: string): Promise<ActionResponse<string>> {
  try {
    if (!guid) {
      return {
        success: false,
        error: '缺少文件 GUID',
      };
    }

    // 使用本地 API 代理路由，由 route handler 处理鉴权和转发
    const mediaUrl = `/api/media/${guid}`;

    return {
      success: true,
      data: mediaUrl,
    };
  } catch (error) {
    console.error('[getMediaUrl] Error:', error);
    return {
      success: false,
      error: '获取文件 URL 失败',
    };
  }
}
