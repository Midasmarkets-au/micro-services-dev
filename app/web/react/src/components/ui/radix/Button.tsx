'use client';

import * as React from 'react';
import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';
import { useTranslations } from 'next-intl';
import { cn } from '@/lib/utils';

const buttonVariants = cva(
  // 基础样式不包含字体大小，由 size 变体控制
  'inline-flex items-center justify-center rounded font-medium transition-colors cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-40',
  {
    variants: {
      variant: {
        // 主要变体 - 全部使用 CSS 变量简写语法，确保 hover 不被覆盖
        primary: 'bg-(--color-primary) text-white hover:bg-(--color-primary-hover)',
        default: 'bg-(--color-primary) text-white hover:bg-(--color-primary-hover)',
        // 次要变体
        secondary: 'bg-(--color-surface-secondary) text-text-primary hover:bg-(--color-surface-secondary)/80',
        // 边框变体
        outline: 'border border-border bg-(--color-surface) hover:bg-(--color-surface-secondary) text-text-primary',
        // 幽灵变体
        ghost: 'hover:bg-(--color-surface-secondary) text-text-primary',
        // 危险变体
        destructive: 'bg-(--color-error) text-white hover:bg-(--color-error)/90',
        // 链接变体
        link: 'text-(--color-primary) underline-offset-4 hover:underline',
      },
      size: {
        // 每个 size 明确指定字体大小，避免覆盖问题
        default: 'h-11 px-5 text-sm',
        xs: 'h-auto px-2.5 py-1 text-sm gap-1',  // 紧凑型按钮 (Figma: 10px/4px padding)
        sm: 'h-9 px-3 text-sm',
        md: 'h-12 px-5 text-sm',
        lg: 'h-14 px-6 text-base',
        icon: 'size-10 text-sm',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'default',
    },
  }
);

// Loading 图标组件
const LoadingSpinner = () => (
  <svg className="size-5 animate-spin" viewBox="0 0 24 24" fill="none">
    <circle
      className="opacity-25"
      cx="12"
      cy="12"
      r="10"
      stroke="currentColor"
      strokeWidth="4"
    />
    <path
      className="opacity-75"
      fill="currentColor"
      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
    />
  </svg>
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  /** 渲染为子元素（如 Link） */
  asChild?: boolean;
  /** 加载状态 */
  loading?: boolean;
  /** 加载时显示的文本，默认使用 i18n */
  loadingText?: string;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant,
      size,
      asChild = false,
      loading = false,
      loadingText,
      disabled,
      children,
      ...props
    },
    ref
  ) => {
    const t = useTranslations('common');
    const isDisabled = disabled || loading;
    const Comp = asChild ? Slot : 'button';

    // asChild 模式下不支持 loading
    if (asChild) {
      return (
        <Comp
          className={cn(buttonVariants({ variant, size, className }))}
          ref={ref}
          {...props}
        >
          {children}
        </Comp>
      );
    }

    return (
      <button
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        disabled={isDisabled}
        {...props}
      >
        {loading ? (
          <span className="flex items-center gap-2 whitespace-nowrap">
            <LoadingSpinner />
            {loadingText ?? t('loading')}
          </span>
        ) : (
          children
        )}
      </button>
    );
  }
);

Button.displayName = 'Button';

export { Button, buttonVariants };
