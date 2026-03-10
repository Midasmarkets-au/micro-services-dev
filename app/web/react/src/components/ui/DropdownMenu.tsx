'use client';

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';

/**
 * DropdownMenu 下拉菜单组件
 *
 * 纯自定义实现，不依赖第三方组件库。
 * 点击触发器打开/关闭菜单，点击外部自动关闭。
 * 使用 Portal 渲染到 body，避免被 overflow 容器裁剪。
 *
 * @example
 * <DropdownMenu
 *   trigger={<Button>操作 <ChevronDown /></Button>}
 *   items={[
 *     { key: 'view', label: '查看账户', onClick: () => {} },
 *     { key: 'rebate', label: '查看返佣统计', onClick: () => {} },
 *   ]}
 * />
 */

export interface DropdownMenuItem {
  key: string;
  label: React.ReactNode;
  onClick: () => void;
  disabled?: boolean;
  hidden?: boolean;
}

export interface DropdownMenuProps {
  /** 触发器元素 */
  trigger: React.ReactNode;
  /** 菜单项列表 */
  items: DropdownMenuItem[];
  /** 菜单对齐方式，默认 right */
  align?: 'left' | 'right';
  /** 外层 className */
  className?: string;
}

export function DropdownMenu({ trigger, items, align = 'right', className }: DropdownMenuProps) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLDivElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const [pos, setPos] = useState({ top: 0, left: 0 });

  const updatePosition = useCallback(() => {
    if (!triggerRef.current) return;
    const rect = triggerRef.current.getBoundingClientRect();
    setPos({
      top: rect.bottom + window.scrollY + 4,
      left: align === 'right'
        ? rect.right + window.scrollX
        : rect.left + window.scrollX,
    });
  }, [align]);

  useEffect(() => {
    if (!open) return;
    updatePosition();

    const handleClickOutside = (e: MouseEvent) => {
      const target = e.target as Node;
      if (
        triggerRef.current && !triggerRef.current.contains(target) &&
        menuRef.current && !menuRef.current.contains(target)
      ) {
        setOpen(false);
      }
    };

    const handleScroll = () => setOpen(false);

    document.addEventListener('mousedown', handleClickOutside);
    window.addEventListener('scroll', handleScroll, true);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      window.removeEventListener('scroll', handleScroll, true);
    };
  }, [open, updatePosition]);

  const visibleItems = items.filter((item) => !item.hidden);

  if (visibleItems.length === 0) return null;

  return (
    <div ref={triggerRef} className={cn('relative inline-block', className)}>
      <div
        onClick={(e) => { e.stopPropagation(); setOpen((prev) => !prev); }}
        className="cursor-pointer"
      >
        {trigger}
      </div>

      {open && createPortal(
        <div
          ref={menuRef}
          style={{
            position: 'absolute',
            top: pos.top,
            ...(align === 'right'
              ? { right: `${document.documentElement.clientWidth - pos.left}px`, left: 'auto' }
              : { left: pos.left }),
            zIndex: 9999,
          }}
          className="min-w-[160px] rounded-sm border border-border bg-surface py-1 shadow-dropdown"
        >
          {visibleItems.map((item) => (
            <button
              key={item.key}
              type="button"
              disabled={item.disabled}
              onClick={(e) => {
                e.stopPropagation();
                if (!item.disabled) {
                  item.onClick();
                  setOpen(false);
                }
              }}
              className={cn(
                'flex w-full items-center px-4 py-2 text-left text-sm text-text-primary transition-colors',
                item.disabled
                  ? 'cursor-not-allowed opacity-50'
                  : 'hover:bg-surface-secondary cursor-pointer'
              )}
            >
              {item.label}
            </button>
          ))}
        </div>,
        document.body
      )}
    </div>
  );
}
