'use client';

import { createContext, useContext, useState, useCallback, useMemo, useSyncExternalStore, useEffect, useRef, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { Toast, type ToastType } from './radix/Toast';

export interface ToastOptions {
  type: ToastType;
  message?: string;
  errorCode?: string;
  buttonText?: string;
  onButtonClick?: () => void;
  onClose?: () => void;
}

interface ToastContextValue {
  showToast: (options: ToastOptions) => void;
  hideToast: () => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

// Toast 容器 ID
const TOAST_CONTAINER_ID = 'toast-portal-root';

// 获取或创建 Toast 容器
function getToastContainer(): HTMLElement {
  let container = document.getElementById(TOAST_CONTAINER_ID);
  if (!container) {
    container = document.createElement('div');
    container.id = TOAST_CONTAINER_ID;
    // 确保容器在所有其他元素之上
    container.style.cssText = 'position: fixed; inset: 0; z-index: 99999; pointer-events: none;';
    document.body.appendChild(container);
  }
  return container;
}

// 用于检测客户端挂载的工具函数
const emptySubscribe = () => () => {};
const getClientSnapshot = () => true;
const getServerSnapshot = () => false;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false);
  const [toastOptions, setToastOptions] = useState<ToastOptions | null>(null);
  const containerRef = useRef<HTMLElement | null>(null);
  
  // 使用 useSyncExternalStore 检测客户端挂载，避免 lint 错误
  const isClient = useSyncExternalStore(emptySubscribe, getClientSnapshot, getServerSnapshot);

  // 初始化 Toast 容器
  useEffect(() => {
    if (isClient) {
      containerRef.current = getToastContainer();
    }
  }, [isClient]);

  // 当 Toast 显示时，禁用 Radix Dialog 的模态阻止
  useEffect(() => {
    if (isOpen && isClient) {
      // 添加 class 让 CSS 禁用 Dialog overlay 的 pointer-events
      document.body.classList.add('toast-active');
      
      // 移除所有 inert 属性（Radix Dialog 会设置这个属性来阻止外部交互）
      const inertElements = document.querySelectorAll('[inert]');
      inertElements.forEach((el) => {
        el.removeAttribute('inert');
        el.setAttribute('data-was-inert', 'true');
      });

      return () => {
        document.body.classList.remove('toast-active');
        // 恢复 inert 属性
        const wasInertElements = document.querySelectorAll('[data-was-inert]');
        wasInertElements.forEach((el) => {
          el.setAttribute('inert', '');
          el.removeAttribute('data-was-inert');
        });
      };
    }
  }, [isOpen, isClient]);

  const showToast = useCallback((options: ToastOptions) => {
    setToastOptions(options);
    setIsOpen(true);
  }, []);

  const hideToast = useCallback(() => {
    setIsOpen(false);
    // 延迟清除选项，让动画完成
    setTimeout(() => {
      setToastOptions(null);
    }, 300);
  }, []);

  // context value 用 useMemo 固定引用，避免 ToastProvider 自身重渲染
  // （isOpen / toastOptions 变化）时让所有 useToast() 消费者无谓地重渲染
  const contextValue = useMemo(() => ({ showToast, hideToast }), [showToast, hideToast]);

  const handleClose = useCallback(() => {
    toastOptions?.onClose?.();
    hideToast();
  }, [toastOptions, hideToast]);

  const handleButtonClick = useCallback(() => {
    if (toastOptions?.onButtonClick) {
      toastOptions.onButtonClick();
    }
    hideToast();
  }, [toastOptions, hideToast]);

  // Toast 组件内容
  const toastContent = toastOptions && (
    <Toast
      type={toastOptions.type}
      message={toastOptions.message}
      errorCode={toastOptions.errorCode}
      buttonText={toastOptions.buttonText}
      onButtonClick={handleButtonClick}
      onClose={handleClose}
      isOpen={isOpen}
    />
  );

  return (
    <ToastContext.Provider value={contextValue}>
      {children}
      {/* 使用 Portal 渲染到专用 Toast 容器，确保在所有其他元素之上 */}
      {isClient && toastContent && containerRef.current && createPortal(toastContent, containerRef.current)}
    </ToastContext.Provider>
  );
}

export function useToastContext() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToastContext must be used within a ToastProvider');
  }
  return context;
}

