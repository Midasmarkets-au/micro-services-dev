'use client';

import { useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useSalesStore } from '@/stores/salesStore';
import { useUserStore } from '@/stores/userStore';
import { AccountSelectorCard } from '@/components/layout/AccountSelectorCard';

export function SalesAccountSelector() {
  const t = useTranslations('sales');
  const td = useTranslations('sales.dashboard');
  const user = useUserStore((s) => s.user);
  const { salesAccount, salesAccountList, setSalesAccount } = useSalesStore();
  const accounts = useMemo(
    () =>
      salesAccountList.map((acc) => ({
        uid: acc.uid,
        label: acc.alias ? `${acc.uid} (${acc.alias})` : String(acc.uid),
      })),
    [salesAccountList]
  );

  const handleChangeAccount = (uid: number) => {
    const found = salesAccountList.find((a) => a.uid === uid);
    if (found) {
      setSalesAccount(found);
      localStorage.setItem('sales-storage', JSON.stringify(found));
      window.location.reload();
    }
  };

  const tradeAccount = salesAccount?.tradeAccount;

  return (
    <AccountSelectorCard
      avatar={user?.avatar}
      userName={user?.name || 'Nickname'}
      balanceLabel={td('balance')}
      balanceInCents={tradeAccount?.balanceInCents ?? 0}
      currencyId={tradeAccount?.currencyId ?? 840}
      accountGroupLabel={td('bkmc')}
      accounts={accounts}
      selectedUid={salesAccount?.uid}
      onChangeAccount={handleChangeAccount}
      loading={salesAccountList.length === 0 && !salesAccount}
      noAccount={!user?.roles?.some((r) => r.toLowerCase() === 'sales')}
      noAccountMessage={t('noSalesAccount')}
    />
  );
}
