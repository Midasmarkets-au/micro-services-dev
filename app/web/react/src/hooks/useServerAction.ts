'use client';

import { useCallback, useEffect, useRef, useState, useTransition } from 'react';
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

  // 把所有可能在 render 间变化的依赖收进 ref，
  // 让 execute 自己保持稳定引用（useCallback 空依赖）。
  // 否则下游 `useCallback([..., execute])` / `useEffect([..., execute])`
  // 会因为 execute 引用刷新而陷入死循环（典型表现：一打开 Modal/进入页面 API 不停发）。
  const optionsRef = useRef({ showErrorToast, onSuccess, onError });
  const showErrorRef = useRef(showError);
  const tErrorsRef = useRef(tErrors);
  const routerRef = useRef(router);
  useEffect(() => {
    optionsRef.current = { showErrorToast, onSuccess, onError };
    showErrorRef.current = showError;
    tErrorsRef.current = tErrors;
    routerRef.current = router;
  });

  const normalizeErrorPayload = useCallback(
    (payload?: { errorCode?: string; error?: string; statusCode?: number } | null) => {
      if (!payload) return payload;
      if (payload.statusCode) return payload;

      const code = payload.errorCode?.toLowerCase();
      if (code === 'unauthorized' || code === '__unauthorized__') {
        return { ...payload, statusCode: 401 };
      }
      if (code === 'forbidden' || code === '__forbidden__') {
        return { ...payload, statusCode: 403 };
      }
      return payload;
    },
    []
  );

  /**
   * 统一判断是否为 401 / Unauthorized
   */
  const isUnauthorizedError = useCallback(
    (payload?: { errorCode?: string; error?: string; statusCode?: number } | null): boolean => {
      if (!payload) return false;
      if (payload.statusCode === 401 || payload.statusCode === 403) return true;

      const errorCode = payload.errorCode?.toLowerCase();
      if (
        errorCode === 'unauthorized' ||
        errorCode === '__unauthorized__' ||
        errorCode === 'forbidden' ||
        errorCode === '__forbidden__'
      ) {
        return true;
      }
      return false;
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
        // 通过 ref 拿到最新的 t 函数，保持本回调引用稳定
        const translated = tErrorsRef.current(errorCode);
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
    []
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

      const { showErrorToast: showErrorToastOpt, onSuccess: onSuccessOpt, onError: onErrorOpt } = optionsRef.current;
      const showError = showErrorRef.current;
      const router = routerRef.current;

      try {
        const result = await action(...args);
        if (!result.success) {
          if (unauthorizedRedirectingRef.current) {
            return result;
          }
          const errorPayload = normalizeErrorPayload({
            errorCode: result.errorCode,
            error: result.error || result.message || 'Request failed',
            statusCode: result.statusCode,
          });
          const errorCode = errorPayload?.errorCode;
          const rawError = errorPayload?.error || 'Request failed';
          // 401/403：静默跳转登录，不弹错误 Toast
          if (isUnauthorizedError(errorPayload)) {
            unauthorizedRedirectingRef.current = true;
            router.replace('/sign-in?expired=true');
            const unauthorizedStatus = errorPayload?.statusCode === 403 ? 403 : 401;
            const unauthorizedCode = unauthorizedStatus === 403 ? 'Forbidden' : 'Unauthorized';
            return {
              success: false,
              error: unauthorizedCode,
              errorCode: unauthorizedCode,
              statusCode: unauthorizedStatus,
            };
          }
          
          // 优先翻译 errorCode，其次尝试翻译 error 信息
          // 这样即使后端返回的是英文错误信息（如 "Wallet address already exists"），
          // 也能从国际化文件中找到对应的翻译
          const errorMessage = errorCode
            ? translateError(errorCode)
            : translateError(rawError);
          
          // 由业务 action 决定是否跳过 Toast（通过 skipToast 字段）
          if (showErrorToastOpt && !result.skipToast) {
            // 传入原始 key 给 showError，让 Toast 组件也能尝试翻译
            showError(errorCode || rawError);
          }

          onErrorOpt?.(errorMessage, errorCode);

          return {
            ...result,
            error: errorMessage,
          };
        }

        onSuccessOpt?.(result.data as T);
        return result;
      } catch (error) {

        console.log('error---catch', error);
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
        const errorPayload = normalizeErrorPayload({
          errorCode: unknownError?.errorCode,
          error: errorMessage,
          statusCode: unknownError?.statusCode,
        });
        const statusCode = errorPayload?.statusCode;
        const errorCode = errorPayload?.errorCode;
        // 异常场景下也拦截 401/403：不弹框，直接跳转登录
        if (isUnauthorizedError(errorPayload)) {
          unauthorizedRedirectingRef.current = true;
          router.push('/sign-in?expired=true');
          const unauthorizedStatus = statusCode === 403 ? 403 : 401;
          const unauthorizedCode = unauthorizedStatus === 403 ? 'Forbidden' : 'Unauthorized';
          return {
            success: false,
            error: unauthorizedCode,
            errorCode: unauthorizedCode,
            statusCode: unauthorizedStatus,
          };
        }

        if (showErrorToastOpt) {
          showError('networkError');
        }

        onErrorOpt?.(errorMessage, 'networkError');

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
    [translateError, isUnauthorizedError, normalizeErrorPayload]
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
