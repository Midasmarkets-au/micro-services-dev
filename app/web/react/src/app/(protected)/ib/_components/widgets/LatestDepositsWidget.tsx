'use client';

import { useState, useEffect, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useRouteScope } from '@/hooks/useRouteScope';
import { useBrowserAction } from '@/lib/http';
import { getIBLatestDeposits } from '@/lib/http/browserActions/ib';
import { useIBStore } from '@/stores/ibStore';
import { Avatar, BalanceShow, DataTable } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui/DataTable';
import type { IBLatestDeposit } from '@/types/ib';

export function LatestDepositsWidget() {
  const t = useTranslations('ib.dashboard');
  const { begin } = useRouteScope('/ib');
  const { execute } = useBrowserAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);

  const [deposits, setDeposits] = useState<IBLatestDeposit[]>([]);
  const [loadedUid, setLoadedUid] = useState<number | null>(null);

  const isLoading = !agentAccount || agentAccount.uid !== loadedUid;

  useEffect(() => {
    if (!agentAccount) return;
    const { signal, isActive } = begin();
    (async () => {
      const result = await execute(getIBLatestDeposits, { signal }, agentAccount.uid, 5);
      if (!isActive()) return;
      if (result.success && Array.isArray(result.data)) {
        setDeposits(result.data);
      }
      setLoadedUid(agentAccount.uid);
    })();
  }, [agentAccount, begin, execute]);

  const columns: DataTableColumn<IBLatestDeposit>[] = useMemo(() => [
    {
      key: 'no',
      title: t('no'),
      align: 'center' as const,
      skeletonWidth: 'w-6',
      render: (_item: IBLatestDeposit, index: number) => (
        <span className="text-base font-semibold text-text-primary">{index + 1}</span>
      ),
    },
    {
      key: 'user',
      title: t('user'),
      align: 'left' as const,
      skeletonWidth: 'w-24',
      skeletonRender: () => (
        <div className="flex items-center gap-3">
          <div className="size-8 animate-pulse rounded-full bg-border" />
          <div className="h-4 w-16 animate-pulse rounded bg-border" />
        </div>
      ),
      render: (item: IBLatestDeposit) => {
        const name = item.user?.nativeName || item.user?.displayName || item.userName || 'Nickname';
        return (
          <div className="flex items-center  gap-3">
            <Avatar src={item.user?.avatar} alt={name} size="xs" />
            <span className="whitespace-nowrap text-sm font-medium text-text-primary">{name}</span>
          </div>
        );
      },
    },
    {
      key: 'port',
      title: t('port'),
      align: 'center' as const,
      skeletonWidth: 'w-14',
      render: () => (
        <span className="inline-block whitespace-nowrap rounded bg-(--color-primary)/20 px-3 py-1 text-xs text-primary">
          {t('client')}
        </span>
      ),
    },
    {
      key: 'amount',
      title: t('amount'),
      align: 'right' as const,
      skeletonWidth: 'w-20',
      render: (item: IBLatestDeposit) => (
        <span className="whitespace-nowrap text-base font-bold text-text-primary">
          <BalanceShow balance={item.amount} currencyId={item.currencyId} />
        </span>
      ),
    },
  ], [t]);

  return (
    <div className="flex flex-col  h-full rounded-xl border border-border bg-surface p-5">
      <h3 className="text-xl font-semibold text-text-primary">{t('funding')}</h3>
      <div className="my-5 h-px w-full bg-border" />
      <p className="text-xl font-semibold text-text-secondary">{t('latestFiveDeposits')}</p>

      <div className="mt-5 flex flex-1 flex-col overflow-x-auto">
        <div className="flex min-w-[500px] flex-1 flex-col">
          <DataTable<IBLatestDeposit>
            columns={columns}
            data={deposits}
            rowKey={(_item, index) => index}
            loading={isLoading}
            skeletonRows={5}
            rounded="xl"
            emptyContent={t('noRecords')}
           
          />
        </div>
      </div>
    </div>
  );
}
