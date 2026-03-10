'use client';

import { useState, useEffect, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesDeposits } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { Avatar, BalanceShow, DataTable } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui/DataTable';

interface DepositItem {
  amount: number;
  currencyId: number;
  userName?: string;
  user?: {
    avatar?: string;
    displayName?: string;
    nativeName?: string;
  };
}

export function SalesLatestDepositsWidget() {
  const t = useTranslations('sales.dashboard');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [deposits, setDeposits] = useState<DepositItem[]>([]);
  const [loadedUid, setLoadedUid] = useState<number | null>(null);

  const isLoading = !salesAccount || salesAccount.uid !== loadedUid;

  useEffect(() => {
    if (!salesAccount) return;
    let cancelled = false;

    const load = async () => {
      const result = await execute(getSalesDeposits, salesAccount.uid, { page: 1, size: 5 });
      if (cancelled) return;
      if (result.success && result.data?.data) {
        setDeposits(Array.isArray(result.data.data) ? result.data.data : []);
      }
      setLoadedUid(salesAccount.uid);
    };

    load();
    return () => { cancelled = true; };
  }, [salesAccount, execute]);

  const columns: DataTableColumn<DepositItem>[] = useMemo(() => [
    {
      key: 'no',
      title: t('no'),
      align: 'center' as const,
      skeletonWidth: 'w-6',
      render: (_item: DepositItem, index: number) => (
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
      render: (item: DepositItem) => {
        const name = (item.user?.nativeName || item.user?.displayName || item.userName || 'Nickname') as string;
        return (
          <div className="flex items-center gap-3">
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
      render: (item: DepositItem) => (
        <span className="whitespace-nowrap text-base font-bold text-text-primary">
          <BalanceShow balance={item.amount} currencyId={item.currencyId} />
        </span>
      ),
    },
  ], [t]);

  return (
    <div className="flex h-full flex-col rounded-xl border border-border bg-surface p-5">
      <h3 className="text-xl font-semibold text-text-primary">{t('funding')}</h3>
      <div className="my-5 h-px w-full bg-border" />
      <p className="text-xl font-semibold text-text-secondary">{t('latestFiveDeposits')}</p>

      <div className="mt-5 flex flex-1 flex-col overflow-x-auto">
        <div className="flex min-w-[500px] flex-1 flex-col">
          <DataTable<DepositItem>
            columns={columns}
            data={deposits}
            rowKey={(_item, index) => index}
            loading={isLoading}
            skeletonRows={5}
            rounded="xl"
            emptyContent={t('noRecords')}
            className="flex-1"
          />
        </div>
      </div>
    </div>
  );
}
