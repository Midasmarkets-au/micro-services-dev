'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBRebates } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import {
  BalanceShow,
  Pagination,
  DataTable,
  Tag,
} from '@/components/ui';
import type { DataTableColumn, TagVariant } from '@/components/ui';
import { CurrencyCodeMap } from '@/components/ui';
import type { IBRebateRecord, IBRebateListResponse } from '@/types/ib';
import { TradeFilter } from '@/components/TradeFilter';

function formatTime(time: string | null | undefined) {
  if (!time) return '-';
  const d = new Date(time);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}

function getRebateStateVariant(stateId: number): TagVariant {
  if (stateId >= 550) return 'success';
  if (stateId >= 505 && stateId < 520) return 'danger';
  return 'warning';
}

function getRebateStateKey(stateId: number): string {
  if (stateId >= 550) return 'completedRebate';
  if (stateId === 505) return 'cancelledRebate';
  return 'pendingRebate';
}

export default function IBRebatePage() {
  const t = useTranslations('ib');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const agentAccount = useIBStore((s) => s.agentAccount);

  const [data, setData] = useState<IBRebateRecord[]>([]);
  const [criteria, setCriteria] = useState<IBRebateListResponse['criteria'] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({});
  const pageSize = 15;

  const fetchData = useCallback(
    async (p: number, extraParams?: Record<string, unknown>) => {
      if (!agentAccount) return;
      setIsLoading(true);
      try {
        const params = { ...filterParams, ...extraParams };
        const result = await executeRef.current(getIBRebates, agentAccount.uid, {
          page: p,
          size: pageSize,
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
    [agentAccount, filterParams],
  );

  const agentUid = agentAccount?.uid;
  useEffect(() => {
    if (agentUid) fetchData(1);
    else setIsLoading(false);
  }, [agentUid]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSearch = (params: Record<string, unknown>) => {
    setFilterParams(params);
    setPage(1);
    fetchData(1, params);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    fetchData(p);
  };

  const columns = useMemo<DataTableColumn<IBRebateRecord>[]>(() => [
    {
      key: 'account',
      title: t('rebate.account'),
      skeletonWidth: 'w-28',
      render: (item) => (
        <div className="flex flex-col">
          <span className="text-sm">{item.trade?.accountName || '-'}</span>
          <span className="text-xs">{item.trade?.accountNumber || ''}</span>
        </div>
      ),
    },
    {
      key: 'symbol',
      title: t('rebate.product'),
      skeletonWidth: 'w-20',
      render: (item) => item.trade?.symbol || '-',
    },
    {
      key: 'ticket',
      title: t('rebate.ticket'),
      skeletonWidth: 'w-20',
      render: (item) => item.trade?.ticket || '-',
    },
    {
      key: 'currency',
      title: t('rebate.currency'),
      skeletonWidth: 'w-12',
      render: (item) => (
        <span  className="font-semibold text-text-primary">{CurrencyCodeMap[item.trade?.currencyId ?? item.currencyId] || 'USD'}</span>
      ),
    },
    {
      key: 'volume',
      title: t('rebate.volume'),
      skeletonWidth: 'w-14',
      render: (item) => item.trade?.volume != null ? item.trade.volume / 100 : '-',
    },
    {
      key: 'amount',
      title: t('rebate.amount'),
      skeletonWidth: 'w-24',
      align: 'right',
      render: (item) => (
        <BalanceShow balance={item.amount} currencyId={item.currencyId} className="font-semibold text-text-primary" />
      ),
    },
    {
      key: 'status',
      title: t('rebate.status'),
      skeletonWidth: 'w-20',
      render: (item) => (
        <Tag variant={getRebateStateVariant(item.stateId)} soft>
          {t(`rebate.${getRebateStateKey(item.stateId)}`)}
        </Tag>
      ),
    },
    {
      key: 'createdOn',
      title: t('rebate.createdOn'),
      skeletonWidth: 'w-32',
      render: (item) => (
        <span className="whitespace-nowrap text-sm">
          {formatTime(item.createdOn)}
        </span>
      ),
    },
    {
      key: 'closeAt',
      title: t('rebate.closeTime'),
      skeletonWidth: 'w-32',
      render: (item) => (
        <span className="whitespace-nowrap text-sm">
          {formatTime(item.trade?.closeAt)}
        </span>
      ),
    },  
  ], [t]);

  const agentCurrencyId = agentAccount?.currencyId;
  const totalAmount = criteria?.totalAmount ?? 0;

  const footerRows = useMemo(() => {
    if (!criteria || data.length === 0) return null;
    const colCount = columns.length;

    const buildRow = (label: string, vol: number, amount: number, highlight?: boolean) => (
      <tr key={label} className={highlight ? 'bg-surface-secondary/50 font-semibold' : 'font-medium'}>
        <td className="px-4 py-3 text-text-primary">{label}</td>
        <td className="px-4 py-3" colSpan={3} />
        <td className="px-4 py-3 text-text-primary">{vol}</td>
        <td className="px-4 py-3">
          <BalanceShow balance={amount} currencyId={agentCurrencyId} className="font-semibold text-text-primary" />
        </td>
        <td className="px-4 py-3" colSpan={colCount - 6} />
      </tr>
    );

    return (
      <>
        {buildRow(
          t('rebate.subTotal'),
          (criteria.pageTotalVolume ?? 0) / 100,
          criteria.pageTotalAmount ?? 0,
        )}
        {buildRow(
          t('rebate.total'),
          (criteria.totalVolume ?? 0) / 100,
          criteria.totalAmount ?? 0,
          true,
        )}
      </>
    );
  }, [criteria, data.length, columns.length, agentCurrencyId, t]);

  return (
    <div className="flex min-h-full w-full min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      <TradeFilter
        type="trade"
        filterOptions={['product', 'account', 'datePicker', 'allHistory']}
        onSearch={handleSearch}
        isLoading={isLoading}
      />

      <div className="flex flex-wrap items-center gap-3">
        <span className="whitespace-nowrap text-lg font-semibold text-text-secondary">
          {t.rich('rebate.showResults', {
            count: String(total),
            num: (chunks) => <span className="text-text-primary">{chunks}</span>,
          })}
        </span>
        <span className="whitespace-nowrap text-lg font-semibold text-text-secondary">{t('rebate.total')}：</span>
        <BalanceShow balance={totalAmount} currencyId={agentCurrencyId} className="whitespace-nowrap text-lg font-bold text-text-primary" />
      </div>

      <DataTable<IBRebateRecord>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        footer={footerRows}
      />

      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />
    </div>
  );
}
