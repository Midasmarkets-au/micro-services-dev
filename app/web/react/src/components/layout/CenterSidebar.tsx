'use client';

import { useMemo } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';

export interface CenterMenuItem {
  id: string;
  path: string;
  labelKey: string;
  icon?: string;
  group?: number;
}

interface CenterSidebarProps {
  items: CenterMenuItem[];
  basePath: string;
  translationNamespace: string;
}

export function CenterSidebar({ items, basePath, translationNamespace }: CenterSidebarProps) {
  const t = useTranslations(translationNamespace);
  const pathname = usePathname();

  const activeId = useMemo(() => {
    for (const item of items) {
      if (item.path === basePath) {
        if (pathname === basePath) return item.id;
        continue;
      }
      if (pathname === item.path || pathname.startsWith(item.path + '/')) {
        return item.id;
      }
    }
    return items[0]?.id ?? '';
  }, [items, basePath, pathname]);

  const dividerIndices = useMemo(() => {
    const set = new Set<number>();
    for (let i = 1; i < items.length; i++) {
      if (items[i].group !== undefined && items[i].group !== items[i - 1].group) {
        set.add(i);
      }
    }
    return set;
  }, [items]);

  const hasIcons = items.some((i) => i.icon);

  return (
    <>
      {/* 桌面端：竖向侧栏 */}
      <aside className="hidden w-52 md:block">
        <div className="sticky top-24 flex h-full flex-col gap-5 rounded bg-surface p-5">
          {items.map((item, idx) => {
            const isActive = activeId === item.id;
            const showDivider = dividerIndices.has(idx);

            return (
              <div key={item.id} className="flex flex-col gap-5">
                {showDivider && <div className="h-px bg-border" />}
                <Link
                  href={item.path}
                  className={`flex items-center gap-3 rounded px-4 py-3 text-base transition-colors ${
                    isActive
                      ? 'bg-primary text-white'
                      : 'text-text-secondary hover:text-text-primary'
                  }`}
                >
                  {item.icon && (
                    <Image
                      src={item.icon}
                      alt=""
                      width={20}
                      height={20}
                      className={isActive ? 'brightness-0 invert' : 'opacity-60'}
                    />
                  )}
                  <span>{t(`menu.${item.labelKey}`)}</span>
                </Link>
              </div>
            );
          })}
        </div>
      </aside>

      {/* 移动端：横向可滑动 tabs */}
      <div className="flex gap-1 overflow-x-auto rounded border border-border bg-surface p-2 md:hidden">
        {items.map((item) => {
          const isActive = activeId === item.id;
          return (
            <Link
              key={item.id}
              href={item.path}
              className={`flex shrink-0 items-center gap-1.5 whitespace-nowrap rounded px-3 py-2 text-sm transition-colors ${
                isActive
                  ? 'bg-primary text-white'
                  : 'text-text-secondary hover:bg-(--color-surface-secondary)'
              }`}
            >
              {hasIcons && item.icon && (
                <Image
                  src={item.icon}
                  alt=""
                  width={16}
                  height={16}
                  className={isActive ? 'brightness-0 invert' : 'opacity-60'}
                />
              )}
              <span>{t(`menu.${item.labelKey}`)}</span>
            </Link>
          );
        })}
      </div>
    </>
  );
}
