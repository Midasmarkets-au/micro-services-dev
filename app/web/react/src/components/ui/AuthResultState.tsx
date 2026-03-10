'use client';

import Image from 'next/image';
import { useTheme } from '@/hooks/useTheme';
import { useTranslations } from 'next-intl';
import { Button } from './radix/Button';

export type ResultType = 'success' | 'error' | 'warning';

export interface AuthResultStateProps {
  /** 结果类型 */
  type: ResultType;
  /** 标题文本 */
  title: string;
  /** 描述文本（可选） */
  description?: string;
  /** 按钮文本（默认根据类型自动选择） */
  buttonText?: string;
  /** 按钮点击回调 */
  onButtonClick?: () => void;
  /** 是否显示按钮（默认 true） */
  showButton?: boolean;
  /** 自定义类名 */
  className?: string;
  /** 额外内容（显示在按钮下方） */
  children?: React.ReactNode;
}

// 图标路径映射
const getIconPath = (type: ResultType, isDark: boolean): string => {
  const suffix = isDark ? 'night' : 'day';
  const typeMap: Record<ResultType, string> = {
    success: 'success',
    error: 'error',
    warning: 'waring', // 注意：文件名是 waring 不是 warning
  };
  return `/images/icons/${typeMap[type]}-${suffix}.svg`;
};

/**
 * 统一的认证结果状态组件
 * 用于显示成功、错误、警告等状态
 * UI 风格与 Toast 组件保持一致
 */
export function AuthResultState({
  type,
  title,
  description,
  buttonText,
  onButtonClick,
  showButton = true,
  className = '',
  children,
}: AuthResultStateProps) {
  const { isDark, mounted } = useTheme();
  const t = useTranslations('auth');

  // 服务端渲染时使用默认图标（日间模式）
  const iconPath = mounted ? getIconPath(type, isDark) : getIconPath(type, false);

  // 默认按钮文本
  const defaultButtonText = t('backToLogin');

  return (
    <div className={`flex flex-col items-center gap-6 py-8 ${className}`}>
      {/* 图标 */}
      <div className="flex justify-center">
        <Image
          src={iconPath}
          alt={type}
          width={80}
          height={80}
          priority
        />
      </div>

      {/* 文本内容 */}
      <div className="text-center">
        <h2 className="text-xl font-semibold text-text-primary">
          {title}
        </h2>
        {description && (
          <p className="mt-2 text-text-secondary">
            {description}
          </p>
        )}
      </div>

      {/* 按钮 */}
      {showButton && onButtonClick && (
        <Button
          type="button"
          onClick={onButtonClick}
          className="mt-4"
        >
          {buttonText || defaultButtonText}
        </Button>
      )}

      {/* 额外内容 */}
      {children}
    </div>
  );
}

/**
 * 成功状态组件（便捷封装）
 */
export function AuthSuccessState({
  title,
  description,
  buttonText,
  onButtonClick,
  showButton = true,
  className,
  children,
}: Omit<AuthResultStateProps, 'type'>) {
  return (
    <AuthResultState
      type="success"
      title={title}
      description={description}
      buttonText={buttonText}
      onButtonClick={onButtonClick}
      showButton={showButton}
      className={className}
    >
      {children}
    </AuthResultState>
  );
}

/**
 * 错误状态组件（便捷封装）
 */
export function AuthErrorState({
  title,
  description,
  buttonText,
  onButtonClick,
  showButton = true,
  className,
  children,
}: Omit<AuthResultStateProps, 'type'>) {
  return (
    <AuthResultState
      type="error"
      title={title}
      description={description}
      buttonText={buttonText}
      onButtonClick={onButtonClick}
      showButton={showButton}
      className={className}
    >
      {children}
    </AuthResultState>
  );
}
