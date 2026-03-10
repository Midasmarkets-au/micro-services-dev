'use client';

import { useTranslations } from 'next-intl';
import { Tabs } from '@/components/ui';
import type { TabItem } from '@/components/ui';
import { TransactionType } from '@/types/wallet';

interface TransactionTabsProps {
  activeTab: TransactionType;
  onTabChange: (tab: TransactionType) => void;
  showRebateTab?: boolean;
}

export function TransactionTabs({
  activeTab,
  onTabChange,
  showRebateTab = false,
}: TransactionTabsProps) {
  const t = useTranslations('wallet');

  const tabs: TabItem<TransactionType>[] = [
    { key: TransactionType.Withdrawal, label: t('tabs.withdrawal') },
    { key: TransactionType.Transfer, label: t('tabs.transfer') },
    ...(showRebateTab
      ? [{ key: TransactionType.Rebate, label: t('tabs.rebate') }]
      : []),
    { key: TransactionType.Refund, label: t('tabs.refund') },
    { key: TransactionType.Adjust, label: t('tabs.adjust') },
    { key: TransactionType.DownlineReward, label: t('tabs.downlineReward') },
  ];

  return (
    <Tabs
      tabs={tabs}
      activeKey={activeTab}
      onChange={onTabChange}
      size="lg"
      showDivider={false}
    />
  );
}
