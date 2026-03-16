'use client';

import { useCallback, useRef, useState, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useToast } from './useToast';

/**
 * Server Action 统一响应格式
 */
export interface ActionResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  errorCode?: string;
  statusCode?: number;
  message?: string;
  /** 是否跳过 Toast 提示，由业务 action 决定 */
  skipToast?: boolean;
  // 特殊响应
  twoFactorRequired?: boolean;
  hasMultipleTenants?: boolean;
  tenantIds?: number[];
}

interface UseServerActionOptions {
  /** 是否显示错误弹窗，默认 true */
  showErrorToast?: boolean;
  /** 成功回调 */
  onSuccess?: <T>(data: T) => void;
  /** 错误回调 */
  onError?: (error: string, errorCode?: string) => void;
}

// 错误码直接使用 errorCodes 命名空间的国际化文件
// 新增错误码只需在 messages/*.json 的 errorCodes 中添加即可

/**
 * Server Actions Hook
 * 统一包装 Server Actions 调用，保留 Toast 错误处理和国际化
 */
export function useServerAction(options: UseServerActionOptions = {}) {
  const { showErrorToast = true, onSuccess, onError } = options;
  const { showError } = useToast();
  const router = useRouter();
  const tErrors = useTranslations('errorCodes');
  const [isPending, startTransition] = useTransition();
  const [isLoading, setIsLoading] = useState(false);
  const unauthorizedRedirectingRef = useRef(false);

  /**
   * 统一判断是否为 401 / Unauthorized
   */
  const isUnauthorizedError = useCallback(
    (payload?: { errorCode?: string; error?: string; statusCode?: number } | null): boolean => {
      if (!payload) return false;
      if (payload.statusCode === 401) return true;

      const errorCode = payload.errorCode?.toLowerCase();
      if (errorCode === 'unauthorized' || errorCode === '__unauthorized__') {
        return true;
      }

      const errorText = payload.error?.toLowerCase();
      if (!errorText) return false;
      return errorText.includes('unauthorized') || errorText.includes('401');
    },
    []
  );

  /**
   * 将 errorCode 转换为国际化文本
   * 直接从 errorCodes 命名空间查找翻译，找不到则返回原文
   */
  const translateError = useCallback(
    (errorCode: string): string => {
      try {
        // 尝试从国际化文件获取翻译
        const translated = tErrors(errorCode);
        // next-intl 在找不到翻译时返回类似 "errorCodes.xxx" 的字符串
        // 检查是否是有效翻译（不包含 namespace 前缀）
        if (translated && !translated.startsWith('errorCodes.')) {
          return translated;
        }
        // 没有找到翻译，返回原始错误信息
        return errorCode;
      } catch {
        // 没有对应翻译时返回原始错误码/错误信息
        return errorCode;
      }
    },
    [tErrors]
  );

  /**
   * 执行 Server Action
   * @param action Server Action 函数
   * @param args Action 参数
   */
  const execute = useCallback(
    async <T, Args extends unknown[]>(
      action: (...args: Args) => Promise<ActionResponse<T>>,
      ...args: Args
    ): Promise<ActionResponse<T>> => {
      setIsLoading(true);

      try {
        const result = await action(...args);
        if (!result.success) {
          if (unauthorizedRedirectingRef.current) {
            return result;
          }

          const errorCode = result.errorCode;
          const rawError = result.error || result.message || 'Request failed';
          
          // 401 / Unauthorized：静默跳转登录，不弹错误 Toast
          if (isUnauthorizedError({ errorCode, error: rawError, statusCode: result.statusCode })) {
            unauthorizedRedirectingRef.current = true;
            router.replace('/sign-in');
            return {
              success: false,
              error: 'Unauthorized',
              errorCode: 'Unauthorized',
              statusCode: 401,
            };
          }
          
          // 优先翻译 errorCode，其次尝试翻译 error 信息
          // 这样即使后端返回的是英文错误信息（如 "Wallet address already exists"），
          // 也能从国际化文件中找到对应的翻译
          const errorMessage = errorCode
            ? translateError(errorCode)
            : translateError(rawError);
          
          // 由业务 action 决定是否跳过 Toast（通过 skipToast 字段）
          if (showErrorToast && !result.skipToast) {
            // 传入原始 key 给 showError，让 Toast 组件也能尝试翻译
            showError(errorCode || rawError);
          }

          onError?.(errorMessage, errorCode);

          return {
            ...result,
            error: errorMessage,
          };
        }

        onSuccess?.(result.data as T);
        return result;
      } catch (error) {
        if (unauthorizedRedirectingRef.current) {
          return {
            success: false,
            error: 'Unauthorized',
            errorCode: 'Unauthorized',
            statusCode: 401,
          };
        }

        const errorMessage = error instanceof Error ? error.message : 'Network error';
        const unknownError = error as { statusCode?: number; errorCode?: string; message?: string } | null;
        const statusCode = unknownError?.statusCode;
        const errorCode = unknownError?.errorCode;

        // 异常场景下也拦截 401：不弹框，直接跳转登录
        if (isUnauthorizedError({ errorCode, error: errorMessage, statusCode })) {
          unauthorizedRedirectingRef.current = true;
          //router.replace('/sign-in');
          router.push('/sign-in');
          return {
            success: false,
            error: 'Unauthorized',
            errorCode: 'Unauthorized',
            statusCode: 401,
          };
        }

        if (showErrorToast) {
          showError('networkError');
        }

        onError?.(errorMessage, 'networkError');

        return {
          success: false,
          error: errorMessage,
          errorCode: errorCode || 'networkError',
          statusCode,
        };
      } finally {
        setIsLoading(false);
      }
    },
    [showErrorToast, showError, translateError, onSuccess, onError, router, isUnauthorizedError]
  );

  /**
   * 使用 React Transition 执行 Server Action（不阻塞 UI）
   */
  const executeWithTransition = useCallback(
    <T, Args extends unknown[]>(
      action: (...args: Args) => Promise<ActionResponse<T>>,
      ...args: Args
    ) => {
      startTransition(async () => {
        await execute(action, ...args);
      });
    },
    [execute]
  );

  return {
    execute,
    executeWithTransition,
    isLoading,
    isPending,
  };
}

export type { UseServerActionOptions };
