'use client';

import { useCallback } from 'react';
import { useToastContext } from '@/components/ui';
import type { ToastOptions } from '@/components/ui';

export type { ToastOptions };

export function useToast() {
  const { showToast, hideToast } = useToastContext();

  // 便捷方法：显示成功提示
  const showSuccess = useCallback((message: string, options?: Partial<Omit<ToastOptions, 'type' | 'message'>>) => {
    showToast({ type: 'success', message, ...options });
  }, [showToast]);

  // 便捷方法：显示错误提示
  // 始终作为 errorCode 传入，让 Toast 组件尝试翻译
  // 这样无论是 "__ERROR_CODE__" 格式还是 "Wallet address already exists" 格式
  // 都能从 errorCodes 命名空间获取翻译
  const showError = useCallback((messageOrErrorCode: string, options?: Partial<Omit<ToastOptions, 'type'>>) => {
    showToast({ type: 'error', errorCode: messageOrErrorCode, ...options });
  }, [showToast]);

  // 便捷方法：显示警告提示
  const showWarning = useCallback((message: string, options?: Partial<Omit<ToastOptions, 'type' | 'message'>>) => {
    showToast({ type: 'warning', message, ...options });
  }, [showToast]);

  // 根据 errorCode 显示错误
  const showErrorByCode = useCallback((errorCode: string, options?: Partial<Omit<ToastOptions, 'type' | 'errorCode'>>) => {
    showToast({ type: 'error', errorCode, ...options });
  }, [showToast]);

  return {
    showToast,
    hideToast,
    showSuccess,
    showError,
    showWarning,
    showErrorByCode,
  };
}

