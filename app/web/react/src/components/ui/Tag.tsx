'use client';

import { cn } from '@/lib/utils';

/**
 * Tag 标签组件
 *
 * 4 种语义变体 × 2 种风格：
 *
 * | variant   | 场景            | solid（默认）          | soft              |
 * |-----------|-----------------|----------------------|-------------------|
 * | info      | 中性/待定        | 灰色实底 + 白字       | 灰色半透明 + 灰字  |
 * | warning   | 警告/代理        | 橙色实底 + 白字       | 橙色半透明 + 橙字  |
 * | danger    | 危险/客户/处理中  | 主色实底 + 白字       | 主色半透明 + 主色字 |
 * | success   | 成功/已完成      | 蓝色实底 + 白字       | 蓝色半透明 + 蓝字  |
 *
 * @example
 * <Tag variant="danger">客户</Tag>
 * <Tag variant="warning">代理</Tag>
 * <Tag variant="success" soft>已完成</Tag>
 * <Tag variant="info" soft>待处理</Tag>
 */

/**
 * Tag 变体类型
 * - info:    中性/信息，灰色系，适用于待定、已取消等中性状态
 * - warning: 警告/提示，橙色系，适用于 IB/代理角色、需注意的状态
 * - danger:  危险/重要，主题色（日间酒红/夜间蓝），适用于客户角色、处理中等状态
 * - success: 成功/完成，蓝色系，适用于已完成、已发货等正向状态
 */
type TagVariant = 'info' | 'warning' | 'danger' | 'success';

interface TagProps {
  /**
   * 语义变体，决定标签颜色
   * @default 'info'
   */
  variant?: TagVariant;
  /**
   * soft 模式：半透明背景 + 彩色文字，适用于状态标签（如订单状态）
   * 默认 false 为实底背景 + 白色文字，适用于角色标签（如客户/代理）
   * @default false
   */
  soft?: boolean;
  className?: string;
  children: React.ReactNode;
}

/** 实底风格：纯色背景 + 白色文字 */
const SOLID_STYLES: Record<TagVariant, string> = {
  info: 'bg-[#999] text-white',
  warning: 'bg-[#E8811A] text-white',
  danger: 'bg-primary text-white',
  success: 'bg-[#004eff] text-white dark:bg-[#004eff]',
};

/** 半透明风格：20% 透明度背景 + 对应彩色文字 */
const SOFT_STYLES: Record<TagVariant, string> = {
  info: 'bg-[rgba(153,153,153,0.2)] text-[#999]',
  warning: 'bg-[rgba(232,129,26,0.2)] text-[#E8811A]',
  danger: 'bg-(--color-primary)/20 text-primary',
  success: 'bg-[rgba(0,78,255,0.2)] text-[#004eff]',
};

export function Tag({ variant = 'info', soft = false, className, children }: TagProps) {
  const variantCls = soft ? SOFT_STYLES[variant] : SOLID_STYLES[variant];

  return (
    <span
      className={cn(
        'inline-flex w-fit items-center rounded px-3 py-1 text-xs font-medium',
        variantCls,
        className
      )}
    >
      {children}
    </span>
  );
}

export type { TagVariant, TagProps };
