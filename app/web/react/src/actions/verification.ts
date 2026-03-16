'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

// ==================== Types ====================

interface VerificationStatus {
  step?: number;
  status?: string;
  data?: unknown;
  settings?: unknown[];
  [key: string]: unknown;
}

interface PersonalInfoData {
  [key: string]: unknown;
}

interface StartedInfoData {
  [key: string]: unknown;
}

interface FinancialInfoData {
  [key: string]: unknown;
}

interface AgreementData {
  [key: string]: unknown;
}

interface DocumentSubmitData {
  [key: string]: unknown;
}

interface QuizAnswerData {
  [key: string]: unknown;
}

interface MyReferralCodeResponse {
  referCode?: string;
}

interface ReferralCodeSummary {
  allowAccountTypes?: { accountType: number }[];
  schema?: { accountType: number }[];
}

interface ReferralCodeInfoResponse {
  code?: string;
  serviceType?: number;
  summary?: ReferralCodeSummary;
  data?: {
    code?: string;
    serviceType?: number;
    summary?: ReferralCodeSummary;
  };
}

interface UploadResponse {
  guid?: string;
  id?: string;
}

interface MergeResponse {
  guid?: string;
  id?: string;
}

// ==================== Actions ====================

/**
 * 获取验证状态
 */
export async function getVerificationStatus(): Promise<ActionResponse<VerificationStatus>> {
  try {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const response = await apiClient.v1.get<any>('/client/verification');

    // 后端返回 { data: ..., settings: ... }，已有 data 字段，apiClient 不会再包装
    // 提取 data 和 settings 组合返回
    return {
      success: true,
      data: {
        ...(response.data || {}),
        settings: response.settings,
      },
    };
  } catch (error) {
    console.error('[getVerificationStatus] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 保存开户信息（Started）
 */
export async function saveStartedInfo(data: StartedInfoData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v1.post<{ data: unknown }>('/client/verification/started', data);

    return {
      success: true,
      data: result.data,
    };
  } catch (error) {
    console.error('[saveStartedInfo] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 校验开始页问卷答案（step1）
 */
export async function checkClientAnswer(data: QuizAnswerData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v1.post<{ data: unknown }>('/client/quiz/verification/step1', data);

    return {
      success: true,
      data: result.data,
      // 旧版逻辑这里无论接口成功都阻断继续并提示，避免出现成功 toast
      skipToast: true,
    };
  } catch (error) {
    console.error('[checkClientAnswer] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 校验财务页问卷答案（step2）
 */
export async function checkClientProfessionalAnswer(data: QuizAnswerData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v1.post<{ data: unknown }>('/client/quiz/verification/step2', data);

    return {
      success: true,
      data: result.data,
      skipToast: true,
    };
  } catch (error) {
    console.error('[checkClientProfessionalAnswer] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 获取我的推荐码
 */
export async function getMyReferralCode(): Promise<ActionResponse<MyReferralCodeResponse>> {
  try {
    const result = await apiClient.v1.get<MyReferralCodeResponse>('/user/me/refercode');

    return {
      success: true,
      data: result,
    };
  } catch (error) {
    console.error('[getMyReferralCode] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 根据推荐码获取补充信息
 */
export async function getReferralInfoByReferralCode(code: string): Promise<ActionResponse<ReferralCodeInfoResponse>> {
  try {
    const result = await apiClient.v1.get<ReferralCodeInfoResponse>(`/referralcode/${code}`);

    return {
      success: true,
      data: result,
    };
  } catch (error) {
    console.error('[getReferralInfoByReferralCode] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 保存个人信息
 */
export async function savePersonalInfo(data: PersonalInfoData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v1.post<{ data: unknown }>('/client/verification/info', data);

    return {
      success: true,
      data: result.data,
    };
  } catch (error) {
    console.error('[savePersonalInfo] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 保存财务信息
 */
export async function saveFinancialInfo(data: FinancialInfoData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v1.post<{ data: unknown }>('/client/verification/financial', data);

    return {
      success: true,
      data: result.data,
    };
  } catch (error) {
    console.error('[saveFinancialInfo] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 保存协议信息
 */
export async function saveAgreement(data: AgreementData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v1.post<{ data: unknown }>('/client/verification/agreement', data);

    return {
      success: true,
      data: result.data,
    };
  } catch (error) {
    console.error('[saveAgreement] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 提交文档
 */
export async function submitDocument(data: DocumentSubmitData): Promise<ActionResponse> {
  try {
    const result = await apiClient.v2.post<{ data: unknown }>(
      '/client/verification/document/media/submit',
      data
    );

    return {
      success: true,
      data: result.data,
    };
  } catch (error) {
    console.error('[submitDocument] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 上传文件（FormData）
 * 注意：Server Actions 不直接支持 FormData，需要将文件转换为 base64 或使用其他方式
 */
export async function uploadFile(formDataEntries: {
  file: { name: string; type: string; data: string }; // base64 encoded
  [key: string]: unknown;
}): Promise<ActionResponse<{ guid: string }>> {
  try {
    // 将 base64 转换为 FormData
    const formData = new FormData();
    
    // 解码 base64 文件数据
    const { file, ...otherFields } = formDataEntries;
    const binaryData = Buffer.from(file.data, 'base64');
    const blob = new Blob([binaryData], { type: file.type });
    formData.append('file', blob, file.name);

    // 添加其他字段
    Object.entries(otherFields).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        formData.append(key, String(value));
      }
    });

    const response = await apiClient.v2.postFormData<{ data: UploadResponse }>(
      '/client/media/upload/merge',
      formData
    );

    return {
      success: true,
      data: { guid: response.data.guid || response.data.id || '' },
    };
  } catch (error) {
    console.error('[uploadFile] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 上传验证文档
 */
export async function uploadVerificationDocument(
  file: File,
  documentType: string
): Promise<ActionResponse<{ guid: string }>> {
  try {
    // 创建 FormData
    const formData = new FormData();
    formData.append('file', file);

    // 调用上传接口
    const response = await apiClient.v1.postFormData<{ data: { guid: string } }>(
      `/client/verification/document/upload?type=${documentType}`,
      formData
    );

    return {
      success: true,
      data: { guid: response.data.guid || '' },
    };
  } catch (error) {
    console.error('[uploadVerificationDocument] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 上传文件切片
 */
export async function uploadChunk(formDataEntries: {
  file: { name: string; type: string; data: string }; // base64 encoded chunk
  [key: string]: unknown;
}): Promise<ActionResponse> {
  try {
    const formData = new FormData();
    
    const { file, ...otherFields } = formDataEntries;
    const binaryData = Buffer.from(file.data, 'base64');
    const blob = new Blob([binaryData], { type: file.type });
    formData.append('file', blob, file.name);

    Object.entries(otherFields).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        formData.append(key, String(value));
      }
    });

    const response = await apiClient.v2.postFormData<{ data: unknown }>('/client/media/upload/chunk', formData);

    return {
      success: true,
      data: response.data,
    };
  } catch (error) {
    console.error('[uploadChunk] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}

/**
 * 合并文件切片
 */
export async function mergeChunks(formDataEntries: {
  [key: string]: unknown;
}): Promise<ActionResponse<{ guid: string }>> {
  try {
    const formData = new FormData();

    Object.entries(formDataEntries).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        formData.append(key, String(value));
      }
    });

    const response = await apiClient.v2.postFormData<{ data: MergeResponse }>(
      '/client/media/upload/merge',
      formData
    );

    return {
      success: true,
      data: { guid: response.data.guid || response.data.id || '' },
    };
  } catch (error) {
    console.error('[mergeChunks] Error:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message,
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}
