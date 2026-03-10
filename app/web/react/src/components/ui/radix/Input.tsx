'use client';

import * as React from 'react';
import * as Label from '@radix-ui/react-label';
import { EyeOpenIcon, EyeClosedIcon } from '@radix-ui/react-icons';
import { cn } from '@/lib/utils';

/**
 * 输入框尺寸：
 * - sm: 36px (h-9) — 页面内搜索框、筛选栏等紧凑场景
 * - normal: 44px (h-11) — 通用表单（默认）
 * - md: 48px — 登录/注册等全屏表单页面
 */
export type InputSize = 'sm' | 'normal' | 'md';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  /** 标签文本 */
  label?: string;
  /** 标签自定义样式类 */
  labelClassName?: string;
  /** 错误信息 */
  error?: string;
  /** 错误提示位置：top=label右侧，bottom=输入框下方 */
  errorPosition?: 'top' | 'bottom';
  /** 是否必填（显示红色 * 号） */
  required?: boolean;
  /** 是否显示密码切换按钮（type=password 时默认 true） */
  showPasswordToggle?: boolean;
  /** 输入框尺寸：sm=36px（搜索框），normal=44px（默认），md=48px（登录等页面） */
  inputSize?: InputSize;
}

const INPUT_SIZE_CLASS: Record<InputSize, string> = {
  sm: 'h-9! px-3!',
  normal: 'h-11!',
  md: '',
};

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      type = 'text',
      label,
      labelClassName,
      error,
      errorPosition = 'top',
      required,
      showPasswordToggle,
      inputSize = 'normal',
      ...props
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = React.useState(false);
    const isPassword = type === 'password';
    const shouldShowToggle = isPassword && (showPasswordToggle !== false);
    const inputType = isPassword ? (showPassword ? 'text' : 'password') : type;
    const sizeClass = INPUT_SIZE_CLASS[inputSize];

    return (
      <div className="w-full">
        {(label || (error && errorPosition === 'top')) && (
          <div className="mb-2 flex items-center justify-between">
            {label && (
              <Label.Root className={`flex items-center ${labelClassName || "text-sm font-normal text-text-secondary"}`}>
                {required && <span className="mr-0.5 text-primary">*</span>}
                {label}
              </Label.Root>
            )}
            {error && errorPosition === 'top' && (
              <span className="error-text text-sm font-normal">{error}</span>
            )}
          </div>
        )}

        <div className="relative">
          <input
            type={inputType}
            className={cn(
              'input-field',
              sizeClass,
              shouldShowToggle && 'pr-12',
              error && 'error-border',
              className
            )}
            ref={ref}
            {...props}
          />

          {/* 密码显示/隐藏按钮 */}
          {shouldShowToggle && (
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-text-secondary hover:text-text-primary transition-colors"
              tabIndex={-1}
              aria-label={showPassword ? 'Hide password' : 'Show password'}
            >
              {showPassword ? (
                <EyeOpenIcon className="size-5" />
              ) : (
                <EyeClosedIcon className="size-5" />
              )}
            </button>
          )}
        </div>

        {/* 错误提示（底部） */}
        {error && errorPosition === 'bottom' && (
          <p className="error-text mt-1 text-sm">{error}</p>
        )}
      </div>
    );
  }
);
Input.displayName = 'Input';

export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  /** 标签文本 */
  label?: string;
  /** 错误信息 */
  error?: string;
  /** 错误提示位置：top=label右侧，bottom=输入框下方 */
  errorPosition?: 'top' | 'bottom';
  /** 是否必填（显示红色 * 号） */
  required?: boolean;
}

const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ className, label, error, errorPosition = 'top', required, ...props }, ref) => {
    return (
      <div className="w-full">
        {/* Label 和错误提示（顶部） */}
        {(label || (error && errorPosition === 'top')) && (
          <div className="mb-2 flex items-center justify-between">
            {label && (
              <Label.Root className="flex items-center text-sm font-medium text-text-secondary">
                {required && <span className="mr-0.5 text-primary">*</span>}
                {label}
              </Label.Root>
            )}
            {error && errorPosition === 'top' && (
              <span className="error-text text-sm font-normal">{error}</span>
            )}
          </div>
        )}

        <textarea
          className={cn(
            'input-field h-auto! min-h-[120px] resize-none py-3',
            error && 'error-border',
            className
          )}
          ref={ref}
          {...props}
        />

        {/* 错误提示（底部） */}
        {error && errorPosition === 'bottom' && (
          <p className="error-text mt-1 text-sm">{error}</p>
        )}
      </div>
    );
  }
);
Textarea.displayName = 'Textarea';

export { Input, Textarea };
