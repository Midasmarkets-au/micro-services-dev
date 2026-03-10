'use client';

import { useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useToast } from './useToast';

interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  errorCode?: string;
  message?: string;
}

interface FetchOptions extends Omit<RequestInit, 'body'> {
  body?: unknown;
  showErrorToast?: boolean; // 是否显示错误弹窗，默认 true
}

interface UploadOptions extends Omit<RequestInit, 'body' | 'method'> {
  showErrorToast?: boolean;
}

// 定义错误码类型（用于 next-intl 的类型安全）
type ErrorCodeKey = 
  | '__USER_IS_LOCKED_OUT__'
  | 'invalid_grant'
  | '__EMAIL_UNCONFIRMED__'
  | '__BALANCE_NOT_ENOUGH__'
  | '__INVALID_TOKEN__'
  | '__ACCOUNT_NOT_EXISTS__'
  | '__INVALID_EMAIL__'
  | '__INVALID_PASSWORD__'
  | '__INVALID_VERIFICATION_CODE__'
  | '__VERIFICATION_CODE_EXPIRED__'
  | '__VERIFICATION_CODE_REQUIRED__'
  | '__FAILED_SEND_VERIFICATION_CODE__'
  | '__EMAIL_EXISTS__'
  | '__EMAIL_IS_REQUIRED__'
  | '__PASSWORD_NOT_MATCH_REQUIREMENT__'
  | '__ACCOUNT_INACTIVATED__'
  | '__INVALID_DATA__'
  | '__INVALID_PARAMETERS__'
  | '__INVALID_INPUT__'
  | '__CREATE_FAILED__'
  | '__UPDATE_FAILED__'
  | '__DELETE_ITEM_FAILED__'
  | '__ACTION_FAIL__'
  | '__ACTION_NOT_ALLOW__'
  | '__CANCEL_FAILED__'
  | '__APPROVE_FAILED__'
  | '__PAYMENT_EXECUTE_FAILED__'
  | '__ACCOUNT_CREATE_FAILED__'
  | '__CHANGE_PASSWORD_FAIL__'
  | '__SYSTEM_MAINTENANCE__'
  | 'networkError'
  | 'unknownError';

// 错误码映射表（运行时使用，避免 next-intl 动态 key 问题）
const ERROR_CODE_MAP: Record<string, ErrorCodeKey> = {
  '__USER_IS_LOCKED_OUT__': '__USER_IS_LOCKED_OUT__',
  'invalid_grant': 'invalid_grant',
  '__EMAIL_UNCONFIRMED__': '__EMAIL_UNCONFIRMED__',
  '__BALANCE_NOT_ENOUGH__': '__BALANCE_NOT_ENOUGH__',
  '__INVALID_TOKEN__': '__INVALID_TOKEN__',
  '__ACCOUNT_NOT_EXISTS__': '__ACCOUNT_NOT_EXISTS__',
  '__INVALID_EMAIL__': '__INVALID_EMAIL__',
  '__INVALID_PASSWORD__': '__INVALID_PASSWORD__',
  '__INVALID_VERIFICATION_CODE__': '__INVALID_VERIFICATION_CODE__',
  '__VERIFICATION_CODE_EXPIRED__': '__VERIFICATION_CODE_EXPIRED__',
  '__VERIFICATION_CODE_REQUIRED__': '__VERIFICATION_CODE_REQUIRED__',
  '__FAILED_SEND_VERIFICATION_CODE__': '__FAILED_SEND_VERIFICATION_CODE__',
  '__EMAIL_EXISTS__': '__EMAIL_EXISTS__',
  '__EMAIL_IS_REQUIRED__': '__EMAIL_IS_REQUIRED__',
  '__PASSWORD_NOT_MATCH_REQUIREMENT__': '__PASSWORD_NOT_MATCH_REQUIREMENT__',
  '__ACCOUNT_INACTIVATED__': '__ACCOUNT_INACTIVATED__',
  '__INVALID_DATA__': '__INVALID_DATA__',
  '__INVALID_PARAMETERS__': '__INVALID_PARAMETERS__',
  '__INVALID_INPUT__': '__INVALID_INPUT__',
  '__CREATE_FAILED__': '__CREATE_FAILED__',
  '__UPDATE_FAILED__': '__UPDATE_FAILED__',
  '__DELETE_ITEM_FAILED__': '__DELETE_ITEM_FAILED__',
  '__ACTION_FAIL__': '__ACTION_FAIL__',
  '__ACTION_NOT_ALLOW__': '__ACTION_NOT_ALLOW__',
  '__CANCEL_FAILED__': '__CANCEL_FAILED__',
  '__APPROVE_FAILED__': '__APPROVE_FAILED__',
  '__PAYMENT_EXECUTE_FAILED__': '__PAYMENT_EXECUTE_FAILED__',
  '__ACCOUNT_CREATE_FAILED__': '__ACCOUNT_CREATE_FAILED__',
  '__CHANGE_PASSWORD_FAIL__': '__CHANGE_PASSWORD_FAIL__',
  '__SYSTEM_MAINTENANCE__': '__SYSTEM_MAINTENANCE__',
  'networkError': 'networkError',
  'unknownError': 'unknownError',
};

/**
 * API 客户端 Hook
 * 封装 fetch，当 API 返回非 200 状态码时自动显示错误弹窗
 */
export function useApiClient() {
  const { showError } = useToast();
  const tErrors = useTranslations('errorCodes');

  /**
   * 将 errorCode 转换为国际化文本
   * 如果找不到翻译，返回原始 errorCode
   */
  const translateError = useCallback((errorCode: string): string => {
    try {
      // 检查错误码是否在已知映射中
      const mappedKey = ERROR_CODE_MAP[errorCode];
      if (mappedKey) {
        // 使用映射后的 key 进行翻译（类型安全）
        return tErrors(mappedKey);
      }
      // 未知错误码，返回原始值
      return errorCode;
    } catch {
      return errorCode;
    }
  }, [tErrors]);

  /**
   * 发送 API 请求
   * @param url API 路径，如 '/api/auth/forgot-password'
   * @param options 请求选项
   * @returns API 响应数据
   */
  const fetchApi = useCallback(async <T = unknown>(
    url: string,
    options: FetchOptions = {}
  ): Promise<ApiResponse<T>> => {
    const { body, showErrorToast = true, ...fetchOptions } = options;

    try {
      const response = await fetch(url, {
        ...fetchOptions,
        headers: {
          'Content-Type': 'application/json',
          ...fetchOptions.headers,
        },
        body: body ? JSON.stringify(body) : undefined,
      });

      const data = await response.json();
      // 如果响应不是 2xx，显示错误弹窗
      if (!response.ok) {
        // 提取错误码：优先使用 errorCode，其次 error，最后检查 message 是否是错误码格式
        let errorCode = data.errorCode || data.error;
        const rawErrorMessage = data.message || data.error || 'Request failed';
        
        // 如果 message 是错误码格式（以 __ 开头和结尾），也作为 errorCode
        if (!errorCode && data.message && /^__[A-Z_]+__$/.test(data.message)) {
          errorCode = data.message;
        }
        
        // 自动国际化错误消息：优先使用 errorCode 的翻译，否则使用原始消息
        const errorMessage = errorCode ? translateError(errorCode) : rawErrorMessage;

        if (showErrorToast) {
          // Toast 组件内部也会尝试国际化，但传入已翻译的消息可以作为后备
          if (errorCode) {
            showError(errorCode);
          } else {
            showError(errorMessage);
          }
        }

        return {
          success: false,
          error: errorMessage, // 返回已国际化的错误消息
          errorCode,
        };
      }

      // 如果响应数据已经包含 success 字段（API route 已包装），直接返回，避免二次包装
      if (data && typeof data === 'object' && 'success' in data) {
        return data as ApiResponse<T>;
      }
      
      // 否则包装成统一格式
      return {
        success: true,
        data: data as T,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Network error';
      console.error('error', error);
      if (showErrorToast) {
        showError('networkError');
      }

      return {
        success: false,
        error: errorMessage,
        errorCode: 'networkError',
      };
    }
  }, [showError, translateError]);

  /**
   * GET 请求
   */
  const get = useCallback(<T = unknown>(url: string, options?: Omit<FetchOptions, 'method' | 'body'>) => {
    return fetchApi<T>(url, { ...options, method: 'GET' });
  }, [fetchApi]);

  /**
   * POST 请求
   */
  const post = useCallback(<T = unknown>(url: string, body?: unknown, options?: Omit<FetchOptions, 'method' | 'body'>) => {
    return fetchApi<T>(url, { ...options, method: 'POST', body });
  }, [fetchApi]);

  /**
   * PUT 请求
   */
  const put = useCallback(<T = unknown>(url: string, body?: unknown, options?: Omit<FetchOptions, 'method' | 'body'>) => {
    return fetchApi<T>(url, { ...options, method: 'PUT', body });
  }, [fetchApi]);

  /**
   * DELETE 请求
   */
  const del = useCallback(<T = unknown>(url: string, options?: Omit<FetchOptions, 'method' | 'body'>) => {
    return fetchApi<T>(url, { ...options, method: 'DELETE' });
  }, [fetchApi]);

  /**
   * 文件上传请求 (FormData)
   * 注意：不设置 Content-Type，让浏览器自动设置
   */
  const upload = useCallback(async <T = unknown>(
    url: string, 
    formData: FormData,
    options: UploadOptions = {}
  ): Promise<ApiResponse<T>> => {
    const { showErrorToast = true, ...fetchOptions } = options;

    try {
      const response = await fetch(url, {
        ...fetchOptions,
        method: 'POST',
        body: formData,
        // 不设置 Content-Type，让浏览器自动处理 multipart/form-data
      });

      const data = await response.json();

      if (!response.ok) {
        // 提取错误码：优先使用 errorCode，其次 error，最后检查 message 是否是错误码格式
        let errorCode = data.errorCode || data.error;
        const rawErrorMessage = data.message || data.error || 'Upload failed';
        
        // 如果 message 是错误码格式（以 __ 开头和结尾），也作为 errorCode
        if (!errorCode && data.message && /^__[A-Z_]+__$/.test(data.message)) {
          errorCode = data.message;
        }
        
        // 自动国际化错误消息
        const errorMessage = errorCode ? translateError(errorCode) : rawErrorMessage;

        if (showErrorToast) {
          if (errorCode) {
            showError(errorCode);
          } else {
            showError(errorMessage);
          }
        }

        return {
          success: false,
          error: errorMessage, // 返回已国际化的错误消息
          errorCode,
        };
      }

      // 如果响应数据已经包含 success 字段（API route 已包装），直接返回，避免二次包装
      if (data && typeof data === 'object' && 'success' in data) {
        return data as ApiResponse<T>;
      }
      
      // 否则包装成统一格式
      return {
        success: true,
        data: data as T,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Upload failed';
      
      if (showErrorToast) {
        showError('networkError');
      }

      return {
        success: false,
        error: errorMessage,
        errorCode: 'networkError',
      };
    }
  }, [showError, translateError]);

  return {
    fetchApi,
    get,
    post,
    put,
    delete: del,
    upload,
  };
}

