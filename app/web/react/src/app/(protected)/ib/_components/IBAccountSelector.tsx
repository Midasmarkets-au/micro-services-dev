'use client';

import { useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useIBStore } from '@/stores/ibStore';
import { useUserStore } from '@/stores/userStore';
import { AccountSelectorCard } from '@/components/layout/AccountSelectorCard';

export function IBAccountSelector() {
  const t = useTranslations('ib');
  const user = useUserStore((s) => s.user);
  const { agentAccount, agentAccountList, setAgentAccount } = useIBStore();

  const accounts = useMemo(
    () =>
      agentAccountList.map((acc) => ({
        uid: acc.uid,
        label: acc.alias ? `${acc.uid} (${acc.alias})` : String(acc.uid),
      })),
    [agentAccountList]
  );

  const handleChangeAccount = (uid: number) => {
    const found = agentAccountList.find((a) => a.uid === uid);
    if (found) {
      setAgentAccount(found);
      localStorage.setItem('ib-storage', JSON.stringify(found));
      window.location.reload();
    }
  };

  const tradeAccount = agentAccount?.tradeAccount;

  return (
    <AccountSelectorCard
      avatar={user?.avatar}
      userName={user?.name || 'Nickname'}
      balanceLabel={t('dashboard.balance')}
      balanceInCents={tradeAccount?.balanceInCents ?? 0}
      currencyId={tradeAccount?.currencyId ?? 840}
      accountGroupLabel={agentAccount?.agentSelfGroupName || 'BKMC'}
      accounts={accounts}
      selectedUid={agentAccount?.uid}
      onChangeAccount={handleChangeAccount}
      loading={agentAccountList.length === 0 && !agentAccount}
      noAccount={!user?.ibAccount?.length}
      noAccountMessage={t('noIBAccount')}
    />
  );
}
