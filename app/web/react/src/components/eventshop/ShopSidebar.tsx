'use client';

import { useTranslations } from 'next-intl';
import {
  ArchiveIcon,
  ClipboardIcon,
  CounterClockwiseClockIcon,
  StarIcon,
  FileTextIcon,
  RocketIcon,
} from '@radix-ui/react-icons';

export type SidebarTab =
  | 'shop'
  | 'orderHistory'
  | 'pointsHistory'
  | 'rewardDetails'
  | 'terms'
  | 'pointsRules';

interface ShopSidebarProps {
  activeTab: SidebarTab;
  onTabChange: (tab: SidebarTab) => void;
}

const sidebarItems: { key: SidebarTab; icon: React.ElementType; dividerAfter?: boolean }[] = [
  { key: 'shop', icon: ArchiveIcon },
  { key: 'orderHistory', icon: ClipboardIcon },
  { key: 'pointsHistory', icon: CounterClockwiseClockIcon, dividerAfter: true },
  { key: 'rewardDetails', icon: StarIcon },
  { key: 'terms', icon: FileTextIcon, dividerAfter: true },
  { key: 'pointsRules', icon: RocketIcon },
];

export function ShopSidebar({ activeTab, onTabChange }: ShopSidebarProps) {
  const t = useTranslations('eventshop.sidebar');

  return (
    <>
      {/* Desktop: vertical sidebar */}
      <div className="hidden md:flex bg-surface border border-border rounded p-5 flex-col gap-5 w-full">
        {sidebarItems.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.key;

          return (
            <div key={item.key} className="flex flex-col gap-5">
              <button
                onClick={() => onTabChange(item.key)}
                className={`flex items-center gap-3 px-4 py-3 rounded cursor-pointer transition-colors w-full text-left ${
                  isActive
                    ? 'bg-primary text-white'
                    : 'text-text-secondary hover:bg-(--color-surface-secondary)'
                }`}
              >
                <Icon className="size-5 shrink-0" />
                <span className="text-base leading-relaxed">
                  {t(item.key)}
                </span>
              </button>
              {item.dividerAfter && (
                <div className="h-px bg-border w-full" />
              )}
            </div>
          );
        })}
      </div>

      {/* Mobile: horizontal scrollable tabs */}
      <div className="flex md:hidden bg-surface border border-border rounded p-2 overflow-x-auto gap-1">
        {sidebarItems.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.key;

          return (
            <button
              key={item.key}
              onClick={() => onTabChange(item.key)}
              className={`flex items-center gap-1.5 px-3 py-2 rounded shrink-0 cursor-pointer transition-colors text-sm whitespace-nowrap ${
                isActive
                  ? 'bg-primary text-white'
                  : 'text-text-secondary hover:bg-(--color-surface-secondary)'
              }`}
            >
              <Icon className="size-4 shrink-0" />
              <span>{t(item.key)}</span>
            </button>
          );
        })}
      </div>
    </>
  );
}
