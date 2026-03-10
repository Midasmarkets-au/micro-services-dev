'use client';

import { useEffect, useCallback } from 'react';
import Image from 'next/image';
import { useTheme } from '@/hooks/useTheme';
import { useTranslations } from 'next-intl';

export type ToastType = 'success' | 'error' | 'warning';

export interface ToastProps {
  type: ToastType;
  message?: string;
  errorCode?: string;
  buttonText?: string;
  onButtonClick?: () => void;
  onClose?: () => void;
  isOpen: boolean;
}

// 图标路径映射
const getIconPath = (type: ToastType, isDark: boolean): string => {
  const suffix = isDark ? 'night' : 'day';
  const typeMap: Record<ToastType, string> = {
    success: 'success',
    error: 'error',
    warning: 'waring', // 注意：文件名是 waring 不是 warning
  };
  return `/images/icons/${typeMap[type]}-${suffix}.svg`;
};

export function Toast({
  type,
  message,
  errorCode,
  buttonText,
  onButtonClick,
  onClose,
  isOpen,
}: ToastProps) {
  const { isDark, mounted } = useTheme();
  const t = useTranslations('common');
  const tErrors = useTranslations('errorCodes');

  // 获取显示的消息
  const displayMessage = useCallback(() => {
    if (message) return message;
    if (errorCode) {
      try {
        const translated = tErrors(errorCode);
        // next-intl 在找不到翻译时返回类似 "errorCodes.xxx" 的字符串
        // 检查是否是有效翻译（不包含 namespace 前缀）
        if (translated && !translated.startsWith('errorCodes.')) {
          return translated;
        }
        // 没有找到翻译，返回原始错误信息
        return errorCode;
      } catch {
        return errorCode; // 如果翻译出错，显示 errorCode 本身
      }
    }
    return '';
  }, [message, errorCode, tErrors]);

  // ESC 键关闭
  useEffect(() => {
    const handleEsc = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose?.();
      }
    };
    window.addEventListener('keydown', handleEsc);
    return () => window.removeEventListener('keydown', handleEsc);
  }, [isOpen, onClose]);

  // 点击遮罩关闭
  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose?.();
    }
  };

  // 处理按钮点击
  const handleButtonClick = () => {
    if (onButtonClick) {
      onButtonClick();
    } else {
      onClose?.();
    }
  };

  if (!isOpen) return null;

  // 服务端渲染时使用默认图标
  const iconPath = mounted ? getIconPath(type, isDark) : getIconPath(type, false);

  return (
    <div
      className="toast-backdrop"
      onClick={handleBackdropClick}
      role="dialog"
      aria-modal="true"
    >
      <div className="toast-container">
        {/* 关闭按钮 */}
        <button
          className="toast-close-button"
          onClick={onClose}
          aria-label="Close"
        >
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path
              d="M15 5L5 15M5 5L15 15"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </button>

        {/* 内容区域 */}
        <div className="toast-content">
          {/* 图标 */}
          <div className="toast-icon">
            <Image
              src={iconPath}
              alt={type}
              width={80}
              height={80}
              priority
            />
          </div>

          {/* 消息文本 */}
          <p className="toast-message">{displayMessage()}</p>
        </div>

        {/* 按钮 */}
        <div className="toast-button-wrapper">
          <button className="toast-button" onClick={handleButtonClick}>
            {buttonText || t('confirm')}
          </button>
        </div>
      </div>
    </div>
  );
}
