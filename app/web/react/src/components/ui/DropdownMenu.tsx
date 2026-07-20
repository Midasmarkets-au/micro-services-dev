'use client';

import React, { useState, useRef, useEffect, useLayoutEffect, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';
import { useDataTableRowContext } from './DataTableRowContext';

/**
 * DropdownMenu 下拉菜单组件
 *
 * 纯自定义实现，不依赖第三方组件库。
 * 点击触发器打开/关闭菜单，点击外部自动关闭。
 * 使用 Portal 渲染到 body，避免被 overflow 容器裁剪。
 * 使用 fixed 定位，自动检测底部剩余空间，不足时向上弹出；
 * 空间不足时限制 maxHeight 并显示滚动条。
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
/** scroll 定位更新节流间隔 */
const SCROLL_THROTTLE_MS = 50;
/** scroll 停止后最终校正的防抖间隔 */
const SCROLL_DEBOUNCE_MS = 100;

export function DropdownMenu({ trigger, items, align = 'right', className }: DropdownMenuProps) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLDivElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const [menuStyle, setMenuStyle] = useState<MenuStyle>({ visibility: 'hidden' });
  const rowCtx = useDataTableRowContext();

  // 下拉菜单通过 Portal 渲染到 body，鼠标移入菜单时表格行的 :hover 会失效。
  // 打开菜单时锁定当前行高亮，关闭后保持（点击菜单项后高亮也不消失）。
  useEffect(() => {
    if (open) {
      rowCtx?.activateRow();
    }
  }, [open, rowCtx]);

  const computePosition = useCallback(() => {
    if (!triggerRef.current || !menuRef.current) return;

    const triggerRect = triggerRef.current.getBoundingClientRect();
    const menuEl = menuRef.current;

    // 临时解除高度限制，测量菜单自然高度，避免沿用上次 maxHeight 导致误判方向/闪动
    const prevMaxHeight = menuEl.style.maxHeight;
    const prevOverflowY = menuEl.style.overflowY;
    menuEl.style.maxHeight = 'none';
    menuEl.style.overflowY = 'visible';
    const menuHeight = menuEl.scrollHeight;
    menuEl.style.maxHeight = prevMaxHeight;
    menuEl.style.overflowY = prevOverflowY;

    const viewportHeight = window.innerHeight;
    const clientWidth = document.documentElement.clientWidth;

    const spaceBelow = viewportHeight - triggerRect.bottom - GAP;
    const spaceAbove = triggerRect.top - GAP;

    // fixed 定位使用视口坐标，无需叠加 scrollY/scrollX
    const horizontalStyle: Partial<MenuStyle> = align === 'right'
      ? { right: clientWidth - triggerRect.right, left: undefined }
      : { left: triggerRect.left, right: undefined };

    let top: number | undefined;
    let bottom: number | undefined;
    let maxHeight: number | undefined;

    if (spaceBelow >= menuHeight) {
      // 下方空间足够，向下弹出
      top = triggerRect.bottom + GAP;
    } else if (spaceAbove >= menuHeight) {
      // 上方空间足够，向上弹出（顶边对齐到 trigger 上方）
      top = triggerRect.top - menuHeight - GAP;
    } else if (spaceBelow >= spaceAbove) {
      // 上下都不够，下方空间更大：向下展开并限制高度，出现滚动条
      top = triggerRect.bottom + GAP;
      maxHeight = Math.max(spaceBelow, 0);
    } else {
      // 上下都不够，上方空间更大：用 bottom 锚定在 trigger 上方，避免偏移
      bottom = viewportHeight - triggerRect.top + GAP;
      maxHeight = Math.max(spaceAbove, 0);
    }

    setMenuStyle({
      top,
      bottom,
      maxHeight,
      ...horizontalStyle,
      visibility: 'visible',
    });
  }, [align]);

  // 打开时先重置样式再测量定位，避免继承上次的 maxHeight/bottom
  useLayoutEffect(() => {
    if (!open) {
      setMenuStyle({ visibility: 'hidden' });
      return;
    }
    setMenuStyle({ visibility: 'hidden' });
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
    // 只有触发器彻底滚出视口才真正关闭。
    let lastThrottleAt = 0;
    let throttleTimer: ReturnType<typeof setTimeout> | null = null;
    let debounceTimer: ReturnType<typeof setTimeout> | null = null;

    const updateFromScroll = () => {
      if (!triggerRef.current) return;
      const rect = triggerRef.current.getBoundingClientRect();
      const outOfView = rect.bottom < 0 || rect.top > window.innerHeight;
      if (outOfView) {
        setOpen(false);
      } else {
        computePosition();
      }
    };

    const handleScroll = (e: Event) => {
      const target = e.target as Node | null;
      if (menuRef.current && target && menuRef.current.contains(target)) return;

      const now = Date.now();
      const elapsed = now - lastThrottleAt;

      // 节流：滚动过程中降低定位频率，减轻闪动
      if (elapsed >= SCROLL_THROTTLE_MS) {
        lastThrottleAt = now;
        updateFromScroll();
      } else if (throttleTimer === null) {
        throttleTimer = setTimeout(() => {
          throttleTimer = null;
          lastThrottleAt = Date.now();
          updateFromScroll();
        }, SCROLL_THROTTLE_MS - elapsed);
      }

      // 防抖：滚动停止后再做一次最终校正，避免位置残留偏移
      if (debounceTimer !== null) clearTimeout(debounceTimer);
      debounceTimer = setTimeout(() => {
        debounceTimer = null;
        updateFromScroll();
      }, SCROLL_DEBOUNCE_MS);
    };

    const handleResize = () => computePosition();

    document.addEventListener('mousedown', handleClickOutside);
    window.addEventListener('scroll', handleScroll, true);
    window.addEventListener('resize', handleResize);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      window.removeEventListener('scroll', handleScroll, true);
      window.removeEventListener('resize', handleResize);
      if (throttleTimer !== null) clearTimeout(throttleTimer);
      if (debounceTimer !== null) clearTimeout(debounceTimer);
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
            position: 'fixed',
            top: menuStyle.top,
            bottom: menuStyle.bottom,
            left: menuStyle.left,
            right: menuStyle.right !== undefined ? `${menuStyle.right}px` : undefined,
            maxHeight: menuStyle.maxHeight,
            zIndex: 9999,
            visibility: menuStyle.visibility,
            overscrollBehavior: 'contain',
          }}
          className={cn(
            'min-w-[160px] rounded-sm border border-border bg-surface py-1 shadow-dropdown',
            // macOS Chrome 默认叠加滚动条会隐藏，需专用 class 强制常显
            menuStyle.maxHeight !== undefined && 'dropdown-menu-scroll',
          )}
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
