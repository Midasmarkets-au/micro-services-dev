'use client';

import { useTranslations } from 'next-intl';
import { Tabs } from '@/components/ui';
import type { TabItem } from '@/components/ui';

type TabType = 'RealAccounts' | 'DemoAccounts';

interface AccountTabsProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
  disableRealAccounts?: boolean;
}

export function AccountTabs({ activeTab, onTabChange, disableRealAccounts = false }: AccountTabsProps) {
  const t = useTranslations('accounts');

  const tabs: TabItem<TabType>[] = [
    { key: 'RealAccounts', label: t('title.realAccount'), disabled: disableRealAccounts },
    { key: 'DemoAccounts', label: t('title.demoAccounts') },
  ];

  return (
    <Tabs
      tabs={tabs}
      activeKey={activeTab}
      onChange={onTabChange}
      size="xl"
      showDivider={false}
    />
  );
}
