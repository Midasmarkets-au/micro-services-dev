'use client';

import React, { useState, useRef, useEffect, useLayoutEffect, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';

/**
 * DropdownMenu 下拉菜单组件
 *
 * 纯自定义实现，不依赖第三方组件库。
 * 点击触发器打开/关闭菜单，点击外部自动关闭。
 * 使用 Portal 渲染到 body，避免被 overflow 容器裁剪。
 * 自动检测底部剩余空间，不足时向上弹出。
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

interface MenuStyle {
  top?: number;
  bottom?: number;
  left?: number;
  right?: number;
  maxHeight?: number;
  visibility: 'hidden' | 'visible';
}

const GAP = 4;

export function DropdownMenu({ trigger, items, align = 'right', className }: DropdownMenuProps) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLDivElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const [menuStyle, setMenuStyle] = useState<MenuStyle>({ visibility: 'hidden' });

  // 第一步：打开时先以 visibility:hidden 渲染菜单，让 DOM 存在但不可见
  const computePosition = useCallback(() => {
    if (!triggerRef.current || !menuRef.current) return;

    const triggerRect = triggerRef.current.getBoundingClientRect();
    const menuHeight = menuRef.current.offsetHeight;
    const viewportHeight = window.innerHeight;
    const scrollY = window.scrollY;
    const scrollX = window.scrollX;
    const clientWidth = document.documentElement.clientWidth;

    // 计算下方可用空间和上方可用空间
    const spaceBelow = viewportHeight - triggerRect.bottom - GAP;
    const spaceAbove = triggerRect.top - GAP;

    // 水平定位
    const horizontalStyle: Partial<MenuStyle> = align === 'right'
      ? { right: clientWidth - (triggerRect.right + scrollX), left: undefined }
      : { left: triggerRect.left + scrollX, right: undefined };

    if (spaceBelow >= menuHeight) {
      // 下方空间足够，向下弹出
      setMenuStyle({
        top: triggerRect.bottom + scrollY + GAP,
        ...horizontalStyle,
        visibility: 'visible',
      });
    } else if (spaceAbove >= menuHeight) {
      // 上方空间足够，向上弹出
      setMenuStyle({
        top: triggerRect.top + scrollY - menuHeight - GAP,
        ...horizontalStyle,
        visibility: 'visible',
      });
    } else {
      // 上下都不够，取空间较大的一侧并限制 maxHeight
      const useBelow = spaceBelow >= spaceAbove;
      setMenuStyle({
        top: useBelow ? triggerRect.bottom + scrollY + GAP : undefined,
        bottom: !useBelow ? viewportHeight - triggerRect.top + scrollY + GAP : undefined,
        maxHeight: useBelow ? spaceBelow : spaceAbove,
        ...horizontalStyle,
        visibility: 'visible',
      });
    }
  }, [align]);

  // 菜单打开后 DOM 渲染完成，立即测量并定位
  useLayoutEffect(() => {
    if (!open) return;
    computePosition();
  }, [open, computePosition]);

  useEffect(() => {
    if (!open) return;

    const handleClickOutside = (e: MouseEvent) => {
      const target = e.target as Node;
      if (
        triggerRef.current && !triggerRef.current.contains(target) &&
        menuRef.current && !menuRef.current.contains(target)
      ) {
        setOpen(false);
      }
    };

    // scroll 监听用捕获阶段是为了感知页面内任意可滚动容器（比如表格自身的
    // overflow-auto）产生的滚动，而不仅仅是 window 的滚动。但不能不分青红皂白
    // 直接关闭菜单，否则会有两个问题：
    // 1. 菜单项较多时自身会变成可滚动（见 maxHeight 那段逻辑），用户在菜单内部
    //    往下滚动查看更多选项时，这次滚动也会被这里捕获到，菜单立刻被关掉。
    // 2. 点击菜单项时，浏览器会在 mousedown 之后自动给该按钮设置焦点；如果按钮
    //    所在的可滚动祖先容器判断它不完全可见，会自动"滚动到可视区域"，这一步
    //    发生在真正的 click 事件之前。如果这里直接 setOpen(false)，菜单（含
    //    该按钮）会立刻从 DOM 里卸载，click 事件根本来不及派发到按钮上，
    //    item.onClick 永远不会执行，表现为"点一下菜单项，下拉框就消失了"。
    // 所以：菜单自身内部的滚动直接忽略；其余滚动只重新定位（跟 resize 一样），
    // 只有触发器彻底滚出视口才真正关闭。用 rAF 节流，避免高频 scroll 触发过多计算。
    let rafId: number | null = null;
    const handleScroll = (e: Event) => {
      const target = e.target as Node | null;
      if (menuRef.current && target && menuRef.current.contains(target)) return;

      if (rafId !== null) return;
      rafId = requestAnimationFrame(() => {
        rafId = null;
        if (!triggerRef.current) return;
        const rect = triggerRef.current.getBoundingClientRect();
        const outOfView = rect.bottom < 0 || rect.top > window.innerHeight;
        if (outOfView) {
          setOpen(false);
        } else {
          computePosition();
        }
      });
    };
    const handleResize = () => computePosition();

    document.addEventListener('mousedown', handleClickOutside);
    window.addEventListener('scroll', handleScroll, true);
    window.addEventListener('resize', handleResize);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      window.removeEventListener('scroll', handleScroll, true);
      window.removeEventListener('resize', handleResize);
      if (rafId !== null) cancelAnimationFrame(rafId);
    };
  }, [open, computePosition]);

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
            top: menuStyle.top,
            bottom: menuStyle.bottom,
            left: menuStyle.left,
            right: menuStyle.right !== undefined ? `${menuStyle.right}px` : undefined,
            maxHeight: menuStyle.maxHeight,
            zIndex: 9999,
            visibility: menuStyle.visibility,
            overflowY: menuStyle.maxHeight ? 'auto' : undefined,
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
                  : 'hover:bg-(--color-surface-secondary) cursor-pointer'
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
