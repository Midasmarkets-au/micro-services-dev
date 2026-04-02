'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesTransactionReports } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { AccountRoleTypes, CurrencyTypes } from '@/types/accounts';
import {
  Avatar,
  BalanceShow,
  Pagination,
  DataTable,
  Tag,
  Switch,
} from '@/components/ui';
import type { DataTableColumn, TagVariant } from '@/components/ui';
import { CurrencyCodeMap } from '@/components/ui';
import type { SalesTransactionRecord, SalesTransactionListResponse } from '@/types/sales';
import { TradeFilter } from '@/components/TradeFilter';
import type { TradeFilterValues } from '@/components/TradeFilter';
import { TimeShow } from '@/components/TimeShow';

const FIXED_PARAMS: Record<string, unknown> = { isClosed: false };

function getUserName(item: SalesTransactionRecord): string {
  const u = item.user;
  if (u?.nativeName?.trim()) return u.nativeName;
  if (u?.displayName?.trim()) return u.displayName;
  if (u?.firstName?.trim() && u?.lastName?.trim()) return `${u.firstName} ${u.lastName}`;
  return item.userName || '-';
}

function getTransferStateVariant(stateId: number): TagVariant {
  if ([250].includes(stateId)) return 'success';
  if ([205, 206, 215].includes(stateId)) return 'danger';
  if ([200, 210, 220].includes(stateId)) return 'warning';
  return 'info';
}

export default function SalesTransactionPage() {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const salesAccount = useSalesStore((s) => s.salesAccount);

  const defaultFilterParam = useMemo<TradeFilterValues>(() => ({
    pageSize: 25,
  }), []);

  const [data, setData] = useState<SalesTransactionRecord[]>([]);
  const [criteria, setCriteria] = useState<SalesTransactionListResponse['criteria'] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isClient, setIsClient] = useState(true);
  const [pageSize, setPageSize] = useState(25);
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({
    size: 25,
    ...FIXED_PARAMS,
  });

  const fetchData = useCallback(
    async (p: number, extraParams?: Record<string, unknown>) => {
      if (!salesAccount) return;
      setIsLoading(true);
      try {
        const params = { ...filterParams, ...extraParams, ...FIXED_PARAMS };
        const result = await executeRef.current(getSalesTransactionReports, salesAccount.uid, {
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
    [salesAccount, filterParams, isClient, pageSize],
  );

  const salesUid = salesAccount?.uid;
  useEffect(() => {
    if (salesUid) fetchData(1);
    else setIsLoading(false);
  }, [salesUid, isClient]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSearch = useCallback(
    (params: Record<string, unknown>) => {
      const mergedParams = { ...params, ...FIXED_PARAMS };
      if (typeof params.size === 'number') setPageSize(params.size);
      setFilterParams(mergedParams);
      setPage(1);
      fetchData(1, mergedParams);
    },
    [fetchData],
  );

  const handlePageChange = useCallback(
    (p: number) => {
      setPage(p);
      fetchData(p);
    },
    [fetchData],
  );

  const handleRoleToggle = useCallback((val: boolean) => {
    setIsClient(val);
  }, []);

  const columns = useMemo<DataTableColumn<SalesTransactionRecord>[]>(() => [
    {
      key: 'user',
      title: t('transaction.user'),
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
      title: t('transaction.email'),
      skeletonWidth: 'w-36',
      render: (item) => item.user?.email || item.userEmail || '-',
    },
    {
      key: 'currency',
      title: t('transaction.currency'),
      skeletonWidth: 'w-12',
      render: (item) => CurrencyCodeMap[item.currencyId] || 'USD',
    },
    {
      key: 'sourceAccount',
      title: t('transaction.sourceAccount'),
      skeletonWidth: 'w-28',
      align: 'center',
      render: (item) => {
        const acc = item.targetAccount;
        if (!acc) return item.fromAccountNumber ?? '-';
        return (
          <div className="flex flex-col items-center">
            <span className="text-sm font-semibold text-text-primary">
              No.{acc.accountNumber} ({CurrencyCodeMap[acc.currencyId ?? 0] || '***'})
            </span>
            <span className="text-xs text-text-secondary">Group: {acc.group || '***'}</span>
          </div>
        );
      },
    },
    {
      key: 'targetAccount',
      title: t('transaction.targetAccount'),
      skeletonWidth: 'w-28',
      align: 'center',
      render: (item) => {
        const acc = item.sourceAccount;
        if (!acc) return item.toAccountNumber ?? '-';
        return (
          <div className="flex flex-col items-center">
            <span className="text-sm font-semibold text-text-primary">
              No.{acc.accountNumber} ({CurrencyCodeMap[acc.currencyId ?? 0] || '***'})
            </span>
            <span className="text-xs text-text-secondary">Group: {acc.group || '***'}</span>
          </div>
        );
      },
    },
    {
      key: 'amount',
      title: t('transaction.amount'),
      skeletonWidth: 'w-24',
      align: 'right',
      render: (item) => (
        <BalanceShow balance={item.amount} currencyId={item.currencyId} className="font-semibold text-text-primary" />
      ),
    },
    {
      key: 'status',
      title: t('transaction.status'),
      skeletonWidth: 'w-20',
      render: (item) => (
        <Tag variant={getTransferStateVariant(item.stateId)} soft>
          {tAccount(`transactionState.${item.stateId}`).replace(/^(transfer|转账)\s*/i, '')}
        </Tag>
      ),
    },
    {
      key: 'createdOn',
      title: t('transaction.createdOn'),
      skeletonWidth: 'w-32',
      render: (item) => (
        <TimeShow type="inFields" dateIsoString={item.createdOn} />
      ),
    },
  ], [t, tAccount]);

  const totalAmount = criteria?.totalAmount ?? 0;
  const firstCurrencyId = CurrencyTypes.USD;// data[0]?.currencyId;

  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 rounded bg-surface p-5">
      <TradeFilter
        type="transaction"
        filterOptions={['datePicker', 'target', 'allHistory']}
        defaultParam={defaultFilterParam}
        fixedParams={FIXED_PARAMS}
        onSearch={handleSearch}
        isLoading={isLoading}
      />

      <div className="flex items-center justify-between flex-wrap gap-2">
        <div className="flex items-center gap-3">
          <span className="text-lg font-semibold text-text-secondary">
            {t.rich('transaction.showResults', {
              count: String(total),
              num: (chunks) => <span className="text-text-primary">{chunks}</span>,
            })}
          </span>
          <span className="text-lg font-semibold text-text-secondary">{t('transaction.total')}：</span>
          <BalanceShow balance={totalAmount} currencyId={firstCurrencyId} className="text-lg font-bold text-text-primary" />
        </div>

        <div className="flex items-center gap-2">
          <span className="text-sm text-text-secondary">
            {isClient ? t('transaction.client') : t('transaction.ib')}
          </span>
          <Switch checked={isClient} onChange={handleRoleToggle} />
        </div>
      </div>

      <DataTable<SalesTransactionRecord>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        skeletonRows={5}
      />

      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />
    </div>
  );
}
