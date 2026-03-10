'use client';

import * as React from 'react';
import Image from 'next/image';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const DEFAULT_AVATAR = '/images/default-avatar.svg';

const avatarVariants = cva(
  'relative overflow-hidden rounded-full border-2 border-primary',
  {
    variants: {
      size: {
        xs: 'size-8',      // 32px
        sm: 'size-10',     // 40px
        md: 'size-[60px]', // 60px
        lg: 'size-15',     // 60px (tailwind size-15)
        xl: 'size-20',     // 80px
        '2xl': 'size-24',  // 96px
      },
    },
    defaultVariants: {
      size: 'md',
    },
  }
);

export interface AvatarProps extends VariantProps<typeof avatarVariants> {
  /** 头像 URL */
  src?: string | null;
  /** alt 文本 */
  alt?: string;
  /** 是否显示加载状态（上传中） */
  loading?: boolean;
  /** 是否显示骨架屏（数据加载中） */
  skeleton?: boolean;
  /** 点击事件 */
  onClick?: () => void;
  /** 自定义类名 */
  className?: string;
  /** 是否可点击（显示 cursor-pointer） */
  clickable?: boolean;
}

/**
 * 用户头像组件
 * 
 * @example
 * // 基础用法
 * <Avatar src={user.avatar} alt={user.name} />
 * 
 * // 数据加载中显示骨架屏
 * <Avatar src={user?.avatar} skeleton={!user} />
 * 
 * // 不同大小
 * <Avatar src={user.avatar} size="sm" />
 * <Avatar src={user.avatar} size="lg" />
 * <Avatar src={user.avatar} size="xl" />
 * 
 * // 可点击上传
 * <Avatar 
 *   src={user.avatar} 
 *   onClick={handleUpload} 
 *   loading={isUploading}
 *   clickable 
 * />
 */
export function Avatar({
  src,
  alt = 'User',
  size,
  loading = false,
  skeleton = false,
  onClick,
  className,
  clickable = false,
}: AvatarProps) {
  const isClickable = clickable || !!onClick;

  // 骨架屏状态
  if (skeleton) {
    return (
      <div
        className={cn(
          avatarVariants({ size }),
          'animate-pulse bg-surface-secondary border-border',
          className
        )}
      />
    );
  }

  return (
    <div
      className={cn(
        avatarVariants({ size }),
        isClickable && 'cursor-pointer',
        className
      )}
      onClick={onClick}
      role={isClickable ? 'button' : undefined}
      tabIndex={isClickable ? 0 : undefined}
      onKeyDown={
        isClickable
          ? (e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                onClick?.();
              }
            }
          : undefined
      }
    >
      <Image
        src={src || DEFAULT_AVATAR}
        alt={alt}
        fill
        className="object-cover"
      />

      {/* 上传中遮罩 */}
      {loading && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/50">
          <div className="size-6 border-2 border-white border-t-transparent rounded-full animate-spin" />
        </div>
      )}
    </div>
  );
}
