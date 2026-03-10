'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBReferralHistory } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { Avatar, DataTable, Tabs, Pagination } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import type { IBReferralHistory } from '@/types/ib';

type TabFilter = 'all' | 'deposited' | 'notDeposited';

function formatDateTime(dateStr: string): string {
  const d = new Date(dateStr);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}

export default function IBNewCustomersPage() {
  const t = useTranslations('ib.newCustomers');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  useEffect(() => { executeRef.current = execute; });

  const agentAccount = useIBStore((s) => s.agentAccount);
  const agentUid = agentAccount?.uid;

  const [data, setData] = useState<IBReferralHistory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [activeTab, setActiveTab] = useState<TabFilter>('all');
  const size = 10;

  const getStatusFilter = (tab: TabFilter): Record<string, unknown> => {
    if (tab === 'deposited') return { status: 2 };
    if (tab === 'notDeposited') return { status: 1 };
    return {};
  };

  const fetchData = useCallback(
    async (p: number, tab: TabFilter) => {
      if (!agentUid) return;
      setIsLoading(true);
      try {
        const result = await executeRef.current(getIBReferralHistory, agentUid, {
          page: p,
          size,
          IsUnverified: true,
          ...getStatusFilter(tab),
        });
        if (result.success && result.data) {
          setData(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } catch {
        // ignore
      } finally {
        setIsLoading(false);
      }
    },
    [agentUid]
  );

  useEffect(() => {
    fetchData(1, activeTab);
  }, [fetchData, activeTab]);

  const handleTabChange = (tab: TabFilter) => {
    setActiveTab(tab);
    setPage(1);
  };

  const tabs: { key: TabFilter; label: string }[] = [
    { key: 'all', label: t('all') },
    { key: 'deposited', label: t('hasDeposit') },
    { key: 'notDeposited', label: t('noDeposit') },
  ];

  const columns = useMemo<DataTableColumn<IBReferralHistory>[]>(() => [
    {
      key: 'customer',
      title: t('customer'),
      align: 'center',
      skeletonRender: () => (
        <div className="flex items-center justify-center gap-3">
          <div className="size-6 animate-pulse rounded-full bg-border" />
          <div className="h-4 w-20 animate-pulse rounded bg-border" />
        </div>
      ),
      render: (item) => (
        <div className="flex items-center justify-center gap-3">
          <Avatar
            src={item.avatar || item.user?.avatar}
            alt={item.userName || item.user?.displayName}
            size="xs"
            className="size-6! border-0!"
          />
          <span className="text-sm font-medium text-text-primary">
            {item.user?.displayName || item.userName || 'Nickname'}
          </span>
        </div>
      ),
    },
    {
      key: 'email',
      title: t('email'),
      align: 'center',
      skeletonWidth: 'w-36',
      render: (item) => (
        <span className="text-sm text-text-secondary">
          {item.email || item.user?.email || '-'}
        </span>
      ),
    },
    {
      key: 'createdTime',
      title: t('createdTime'),
      align: 'center',
      skeletonWidth: 'w-32',
      render: (item) => (
        <span className="text-sm text-text-secondary">
          {formatDateTime(item.createdOn)}
        </span>
      ),
    },
    {
      key: 'status',
      title: t('status'),
      align: 'center',
      skeletonWidth: 'w-20',
      skeletonHeight: 'h-5',
      render: (item) =>
        item.status === 2 ? (
          <span className="rounded bg-[rgba(0,78,255,0.2)] px-3 py-1 text-xs text-[#004eff]">
            {t('statusDeposited')}
          </span>
        ) : (
          <span className="rounded bg-[rgba(128,0,32,0.2)] px-3 py-1 text-xs text-[#800020]">
            {t('statusNotDeposited')}
          </span>
        ),
    },
  ], [t]);

  return (
    <div className="flex flex-1 flex-col gap-5 rounded bg-surface p-5">
      <Tabs tabs={tabs} activeKey={activeTab} onChange={handleTabChange} />

      <DataTable<IBReferralHistory>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        skeletonRows={5}
      />

      {!isLoading && total > size && (
        <Pagination
          page={page}
          total={total}
          size={size}
          onPageChange={(p) => {
            setPage(p);
            fetchData(p, activeTab);
          }}
        />
      )}
    </div>
  );
}
