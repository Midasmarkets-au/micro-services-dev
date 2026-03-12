'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesWithdrawals } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { AccountRoleTypes } from '@/types/accounts';
import {
  Avatar,
  BalanceShow,
  Pagination,
  DataTable,
  Tag,
  Icon,
  Button,
} from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import type { TagVariant } from '@/components/ui';
import type { SalesWithdrawalRecord, SalesWithdrawalListResponse } from '@/types/sales';
import { CurrencyCodeMap } from '@/components/ui';
import { TradeFilter } from '@/components/TradeFilter';
import type { StatusOption } from '@/components/TradeFilter';

const DEFAULT_WITHDRAWAL_STATE_IDS = [450, 430];

function getUserName(item: SalesWithdrawalRecord): string {
  const u = item.user;
  if (u?.nativeName?.trim()) return u.nativeName;
  if (u?.displayName?.trim()) return u.displayName;
  if (u?.firstName?.trim() && u?.lastName?.trim()) return `${u.firstName} ${u.lastName}`;
  return item.userName || '-';
}

function getWithdrawalStateVariant(stateId: number): TagVariant {
  if ([430, 450].includes(stateId)) return 'success';
  if ([405, 406, 425].includes(stateId)) return 'danger';
  if ([400, 420].includes(stateId)) return 'warning';
  return 'info';
}

function formatTime(time: string | null | undefined) {
  if (!time) return '-';
  const d = new Date(time);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}

export default function SalesWithdrawalPage() {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [data, setData] = useState<SalesWithdrawalRecord[]>([]);
  const [criteria, setCriteria] = useState<SalesWithdrawalListResponse['criteria'] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isClient, setIsClient] = useState(true);
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({
    StateIds: DEFAULT_WITHDRAWAL_STATE_IDS,
  });
  const pageSize = 25;

  const fetchData = useCallback(
    async (p: number, extraParams?: Record<string, unknown>) => {
      if (!salesAccount) return;
      setIsLoading(true);
      try {
        const params = extraParams ?? filterParams;
        const result = await executeRef.current(getSalesWithdrawals, salesAccount.uid, {
          page: p,
          size: pageSize,
          role: isClient ? AccountRoleTypes.Client : AccountRoleTypes.IB,
          ...params,
        });
        if (result.success && result.data) {
          setData(Array.isArray(result.data.data) ? result.data.data : []);
          setCriteria(result.data.criteria || null);
          setTotal(result.data.criteria?.total || 0);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [salesAccount, filterParams, isClient],
  );

  const salesUid = salesAccount?.uid;
  useEffect(() => {
    if (salesUid) fetchData(1);
    else setIsLoading(false);
  }, [salesUid, isClient]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSearch = (params: Record<string, unknown>) => {
    setFilterParams(params);
    setPage(1);
    fetchData(1, params);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    fetchData(p);
  };

  const handleToggleRole = () => {
    setIsClient((prev) => !prev);
    setPage(1);
  };

  const statusOptions = useMemo<StatusOption[]>(() => [
    { value: '400', label: t('withdrawal.incompleteWithdrawal') },
    { value: '450', label: t('withdrawal.completedWithdrawal') },
  ], [t]);

  const columns = useMemo<DataTableColumn<SalesWithdrawalRecord>[]>(() => [
    {
      key: 'user',
      title: t('withdrawal.user'),
      skeletonWidth: 'w-28',
      skeletonRender: () => (
        <div className="flex items-center gap-2">
          <div className="size-10 shrink-0 rounded-full bg-gray-200 dark:bg-gray-700" />
          <div className="h-4 w-20 rounded bg-gray-200 dark:bg-gray-700" />
        </div>
      ),
      render: (item) => (
        <div className="flex items-center gap-3">
          <Avatar src={item.user?.avatar} alt={getUserName(item)} size="sm" />
          <span className="text-sm text-text-primary">{getUserName(item)}</span>
        </div>
      ),
    },
    {
      key: 'email',
      title: t('withdrawal.email'),
      skeletonWidth: 'w-36',
      render: (item) => item.user?.email || item.userEmail || '-',
    },
    {
      key: 'currency',
      title: t('withdrawal.currency'),
      skeletonWidth: 'w-12',
      render: (item) => CurrencyCodeMap[item.currencyId] || 'USD',
    },
    ...(isClient
      ? [
          {
            key: 'sourceAccount' as const,
            title: t('withdrawal.sourceAccount'),
            skeletonWidth: 'w-28',
            align: 'center' as const,
            render: (item: SalesWithdrawalRecord) => {
              const src = (item as unknown as Record<string, unknown>).source as
                | { displayNumber?: number; currencyId?: number; agentGroupName?: string }
                | undefined;
              if (!src) return item.accountNumber ? `No.${item.accountNumber}` : '-';
              return (
                <div className="flex flex-col items-center">
                  <span className="text-sm">
                    No.{src.displayNumber} ({CurrencyCodeMap[src.currencyId ?? 0] || '-'})
                  </span>
                  <span className="text-xs">
                    Group: {src.agentGroupName || '***'}
                  </span>
                </div>
              );
            },
          },
        ]
      : []),
    {
      key: 'exchangeRate',
      title: t('withdrawal.exchangeRate'),
      skeletonWidth: 'w-16',
      render: (item) => (item as unknown as Record<string, unknown>).exchangeRate as string ?? '-',
    },
    {
      key: 'amount',
      title: t('withdrawal.amount'),
      skeletonWidth: 'w-24',
      render: (item) => (
        <BalanceShow balance={item.amount} currencyId={item.currencyId} className="font-semibold text-text-primary" />
      ),
    },
    {
      key: 'status',
      title: t('withdrawal.status'),
      skeletonWidth: 'w-20',
      render: (item) => (
        <Tag variant={getWithdrawalStateVariant(item.stateId)} soft>
          {tAccount(`transactionState.${item.stateId}`).replace(/^(withdrawal|出金)\s*/i, '')}
        </Tag>
      ),
    },
    {
      key: 'createdOn',
      title: t('withdrawal.createdOn'),
      skeletonWidth: 'w-32',
      render: (item) => (
        <span className="whitespace-nowrap text-sm">
          {formatTime(item.createdOn)}
        </span>
      ),
    },
  ], [t, tAccount, isClient]);

  const totalAmount = criteria?.totalAmount ?? 0;
  const firstCurrencyId = data[0]?.currencyId;

  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 rounded bg-surface p-5">
      <TradeFilter
        type="withdrawal"
        translationNamespace="sales"
        statusOptions={statusOptions}
        statusLabel={t('withdrawal.status')}
        filterOptions={['status', 'account', 'datePicker', 'allHistory']}
        onSearch={handleSearch}
        isLoading={isLoading}
      />

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span className="text-lg font-semibold text-text-secondary">
            {t.rich('withdrawal.showResults', {
              count: String(total),
              num: (chunks) => <span className="text-text-primary">{chunks}</span>,
            })}
          </span>
          <span className="text-lg font-semibold text-text-secondary">{t('withdrawal.total')}：</span>
          <BalanceShow balance={totalAmount} currencyId={firstCurrencyId} className="text-lg font-bold text-text-primary" />
        </div>

        <Button
          variant="outline"
          size="sm"
          onClick={handleToggleRole}
          className="flex shrink-0 items-center gap-1"
        >
          {isClient ? t('withdrawal.client') : t('withdrawal.ib')}
          <Icon name="reset-line" size={14} />
        </Button>
      </div>

      <DataTable<SalesWithdrawalRecord>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
      />

      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />
    </div>
  );
}
