'use client';

import { useCallback, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useToast } from '@/hooks/useToast';
import type { BrowserApiResponse } from './browserClient';

interface UseBrowserActionOptions {
  /** 是否显示错误弹窗，默认 true；aborted 永不弹 */
  showErrorToast?: boolean;
  onError?: (error: string, errorCode?: string) => void;
  onSuccess?: <T>(data: T) => void;
}

/**
 * 等价于 useServerAction 的"浏览器端 action 包装器"。
 *
 * 差异：
 *   - 不再驱动 Server Action，而是驱动 browserClient 的 fetch 调用。
 *   - execute 的第一个参数是"返回 BrowserApiResponse 的异步函数"，
 *     函数第一个参数约定为 signal；其余参数透传。
 *   - 如果响应 aborted=true，不弹 toast，不触发 onError。
 */
export function useBrowserAction(options: UseBrowserActionOptions = {}) {
  const { showErrorToast = true, onError, onSuccess } = options;
  const { showError } = useToast();
  const router = useRouter();
  const tErrors = useTranslations('errorCodes');
  const unauthorizedRedirectingRef = useRef(false);

  const translate = useCallback(
    (code: string) => {
      try {
        const t = tErrors(code);
        if (t && !t.startsWith('errorCodes.')) return t;
      } catch {
        // ignore
      }
      return code;
    },
    [tErrors]
  );

  /**
   * execute(action, ...args) 会调用 `action(...args)` 并处理：
   *   - aborted：静默返回
   *   - 401/403：跳登录页
   *   - 其他错误：可选 toast + onError
   */
  const execute = useCallback(
    async <T, Args extends unknown[]>(
      action: (...args: Args) => Promise<BrowserApiResponse<T>>,
      ...args: Args
    ): Promise<BrowserApiResponse<T>> => {
      const result = await action(...args);

      if (result.aborted) {
        return result;
      }

      if (result.success) {
        onSuccess?.(result.data as T);
        return result;
      }

      const { statusCode, errorCode } = result;
      const isUnauthorized =
        statusCode === 401 ||
        statusCode === 403 ||
        errorCode === 'Unauthorized' ||
        errorCode === 'Forbidden';

      if (isUnauthorized) {
        if (!unauthorizedRedirectingRef.current) {
          unauthorizedRedirectingRef.current = true;
          router.replace('/sign-in?expired=true');
        }
        return result;
      }

      if (showErrorToast && errorCode) {
        showError(errorCode);
      } else if (showErrorToast && result.error) {
        showError(result.error);
      }

      onError?.(
        errorCode ? translate(errorCode) : result.error || 'Request failed',
        errorCode
      );

      return result;
    },
    [onError, onSuccess, router, showError, showErrorToast, translate]
  );

  return { execute };
}
