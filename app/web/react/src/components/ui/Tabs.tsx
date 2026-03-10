'use client';

import Link from 'next/link';

export interface TabItem<T extends string = string> {
  key: T;
  label: string;
  disabled?: boolean;
  href?: string;
}

type TabSize = 'sm' | 'base' | 'lg' | 'xl';

const sizeConfig: Record<TabSize, { text: string; gap: string; font: string }> = {
  sm: { text: 'text-sm', gap: 'gap-6', font: 'font-medium' },
  base: { text: 'text-base', gap: 'gap-8', font: 'font-medium' },
  lg: { text: 'text-lg', gap: 'gap-8 md:gap-10', font: 'font-semibold' },
  xl: { text: 'text-xl', gap: 'gap-10', font: 'font-semibold' },
};

export interface TabsProps<T extends string = string> {
  tabs: TabItem<T>[];
  activeKey: T;
  onChange: (key: T) => void;
  size?: TabSize;
  showDivider?: boolean;
  className?: string;
}

export function Tabs<T extends string = string>({
  tabs,
  activeKey,
  onChange,
  size = 'xl',
  showDivider = true,
  className = '',
}: TabsProps<T>) {
  const cfg = sizeConfig[size];

  return (
    <div className={`flex flex-col ${className}`}>
      <div className={`relative flex ${cfg.gap} overflow-x-auto overflow-y-hidden ${showDivider ? 'border-b border-border' : ''}`}>
        {tabs.map((tab) => {
          const isActive = activeKey === tab.key;
          const isDisabled = tab.disabled;

          const inner = (
            <>
              <span
                className={`${cfg.text} ${cfg.font} whitespace-nowrap transition-colors ${
                  isDisabled
                    ? 'text-text-secondary'
                    : isActive
                      ? 'text-primary'
                      : 'text-text-secondary hover:text-text-primary'
                }`}
              >
                {tab.label}
              </span>
              <span className={`h-0.5 w-full ${isActive && !isDisabled ? 'bg-primary' : 'bg-transparent'}`} />
            </>
          );

          const wrapperCls = `relative -mb-px flex flex-col items-center gap-4 transition-colors ${
            isDisabled ? 'cursor-not-allowed opacity-50' : 'cursor-pointer'
          }`;

          if (tab.href) {
            return (
              <Link
                key={tab.key}
                href={tab.href}
                className={wrapperCls}
                aria-disabled={isDisabled}
              >
                {inner}
              </Link>
            );
          }

          return (
            <button
              key={tab.key}
              onClick={() => !isDisabled && onChange(tab.key)}
              disabled={isDisabled}
              className={wrapperCls}
            >
              {inner}
            </button>
          );
        })}
      </div>
    </div>
  );
}
