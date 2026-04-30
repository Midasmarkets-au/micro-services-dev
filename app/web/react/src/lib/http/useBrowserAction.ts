'use client';

import { useCallback, useEffect, useRef } from 'react';
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

  // 用 ref 收纳所有可能在 render 间变化的依赖，让 execute 引用稳定。
  // 否则下游 `useCallback([..., execute])` / `useEffect([..., execute])`
  // 会因 execute 引用刷新而陷入死循环。
  const optionsRef = useRef({ showErrorToast, onError, onSuccess });
  const showErrorRef = useRef(showError);
  const tErrorsRef = useRef(tErrors);
  const routerRef = useRef(router);
  useEffect(() => {
    optionsRef.current = { showErrorToast, onError, onSuccess };
    showErrorRef.current = showError;
    tErrorsRef.current = tErrors;
    routerRef.current = router;
  });

  const translate = useCallback(
    (code: string) => {
      try {
        const t = tErrorsRef.current(code);
        if (t && !t.startsWith('errorCodes.')) return t;
      } catch {
        // ignore
      }
      return code;
    },
    []
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
      const { showErrorToast: showErrorToastOpt, onError: onErrorOpt, onSuccess: onSuccessOpt } = optionsRef.current;
      const showError = showErrorRef.current;
      const router = routerRef.current;

      const result = await action(...args);

      if (result.aborted) {
        return result;
      }

      if (result.success) {
        onSuccessOpt?.(result.data as T);
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

      if (showErrorToastOpt && errorCode) {
        showError(errorCode);
      } else if (showErrorToastOpt && result.error) {
        showError(result.error);
      }

      onErrorOpt?.(
        errorCode ? translate(errorCode) : result.error || 'Request failed',
        errorCode
      );

      return result;
    },
    [translate]
  );

  return { execute };
}
