'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBTradeReports } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { ServiceTypes } from '@/types/accounts';
import { Pagination, DataTable } from '@/components/ui';
import type { DateRange } from '@/components/ui';
import { TradeFilter } from '@/components/TradeFilter';
import { TimeShow } from '@/components/TimeShow';
import type { DataTableColumn } from '@/components/ui';
import type { IBTradeRecord, IBTradeListResponse } from '@/types/ib';
import { handleTradeBuySellDisplay } from '@/lib/trade';

function formatTradePrice(price: number | undefined | null, digits?: number) {
  if (price == null || isNaN(price)) return '-';
  return parseFloat(price.toFixed(digits ?? 2)).toString();
}

export default function IBTradePage() {
  const t = useTranslations('ib');
  const tType = useTranslations();
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const agentAccount = useIBStore((s) => s.agentAccount);

  const [data, setData] = useState<IBTradeRecord[]>([]);
  const [criteria, setCriteria] = useState<IBTradeListResponse['criteria'] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 25;

  const [isClosed, setIsClosed] = useState(false);

  const todayRange = useMemo<DateRange>(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return { from: d, to: d };
  }, []);

  const [filterParams, setFilterParams] = useState<Record<string, unknown>>(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return {
      isClosed: false,
      serviceId: ServiceTypes.MetaTrader5,
      from: d.toISOString(),
      to: d.toISOString(),
    };
  });

  const fetchData = useCallback(
    async (p: number, extraParams?: Record<string, unknown>) => {
      if (!agentAccount) return;
      setIsLoading(true);
      try {
        const params = extraParams ?? filterParams;
        const result = await executeRef.current(getIBTradeReports, agentAccount.uid, {
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

  const columns = useMemo<DataTableColumn<IBTradeRecord>[]>(() => {
    const cols: DataTableColumn<IBTradeRecord>[] = [
      {
        key: 'ticket',
        title: t('trade.ticket'),
        skeletonWidth: 'w-20',
        render: (item) => item.ticket ?? '-',
      },
      {
        key: 'symbol',
        title: t('trade.symbol'),
        skeletonWidth: 'w-20',
        render: (item) => item.symbol ?? '-',
      },
      {
        key: 'type',
        title: t('trade.type'),
        skeletonWidth: 'w-14',
        render: (item) => tType(`type.cmd.${handleTradeBuySellDisplay(item)}`),
      },
      {
        key: 'volume',
        title: t('trade.lots'),
        skeletonWidth: 'w-14',
        align: 'right' as const,
        render: (item) => (item.volume ?? 0) / 100,
      },
      {
        key: 'openTime',
        title: t('trade.openTime'),
        skeletonWidth: 'w-32',
        render: (item) => (
          <TimeShow type="inFields" dateIsoString={item.openAt || item.openTime} />
        ),
      },
      {
        key: 'openPrice',
        title: t('trade.openPrice'),
        skeletonWidth: 'w-20',
        align: 'right' as const,
        render: (item) => (
          <span className="font-semibold text-text-primary">
            {formatTradePrice(item.openPrice, item.digits)}
          </span>
        ),
      },
      {
        key: 'slTp',
        title: t('trade.slTp'),
        skeletonWidth: 'w-14',
        render: (item) => `${item.sl ?? 0}`,
      },
      {
        key: 'tp',
        title: t('trade.tp'),
        skeletonWidth: 'w-14',
        render: (item) => item.tp ?? 0,
      },
      ...(isClosed ? [
        {
          key: 'closeTime',
          title: t('trade.closeTime'),
          skeletonWidth: 'w-32',
          render: (item: IBTradeRecord) => (
            <TimeShow type="inFields" dateIsoString={item.closeAt || item.closeTime} />
          ),
        },
        {
          key: 'closePrice',
          title: t('trade.closePrice'),
          skeletonWidth: 'w-20',
          align: 'right' as const,
          render: (item: IBTradeRecord) => (
            <span className="font-semibold text-text-primary">{formatTradePrice(item.closePrice, item.digits)}</span>
          ),
        },
      ] : []),
      {
        key: 'commission',
        title: t('trade.commission'),
        skeletonWidth: 'w-14',
        align: 'right' as const,
        render: (item) => item.commission ?? 0,
      },
      {
        key: 'swaps',
        title: t('trade.swaps'),
        skeletonWidth: 'w-16',
        align: 'right' as const,
        render: (item) => item.swaps ?? item.swap ?? 0,
      },
      {
        key: 'profit',
        title: t('trade.profit'),
        skeletonWidth: 'w-20',
        align: 'right' as const,
        render: (item: IBTradeRecord) => {
          const val = item.profit ?? 0;
          const formatted = formatTradePrice(val, 2);
          const prefix = val > 0 ? '+' : '';
          return (
            <span className={val < 0 ? 'font-semibold text-[#800020] dark:text-[#800020]' : val > 0 ? 'font-semibold text-[#004EFF] dark:text-[#004EFF]' : 'text-text-primary'}>
              {prefix}{formatted}
            </span>
          );
        },
      },
    ];

    return cols;
  }, [t, tType, isClosed]);

  const footerRows = useMemo(() => {
    if (!criteria || data.length === 0) return null;
    const volumeColIndex = columns.findIndex((col) => col.key === 'volume');
    const commissionColIndex = columns.findIndex((col) => col.key === 'commission');
    const swapColIndex = columns.findIndex((col) => col.key === 'swaps');
    const profitColIndex = columns.findIndex((col) => col.key === 'profit');

    if (volumeColIndex < 0 || commissionColIndex < 0 || swapColIndex < 0 || profitColIndex < 0) return null;

    const beforeVolumeColSpan = Math.max(volumeColIndex - 1, 0);
    const middleColSpan = Math.max(commissionColIndex - volumeColIndex - 1, 0);

    const buildSummaryRow = (label: string, vol: number, comm: number, sw: number, profit: number, highlight?: boolean) => {
      const profitPrefix = profit > 0 ? '+' : '';
      return (
        <tr key={label} className={highlight ? 'bg-surface-secondary/50 font-semibold' : 'font-medium'}>
          <td className="px-5 py-3 text-text-primary">{label}</td>
          {beforeVolumeColSpan > 0 && <td className="px-5 py-3" colSpan={beforeVolumeColSpan} />}
          <td className="px-5 py-3 text-right text-text-primary">{vol}</td>
          {middleColSpan > 0 && <td className="px-5 py-3" colSpan={middleColSpan} />}
          <td className="px-5 py-3 text-right text-text-primary">{comm}</td>
          <td className="px-5 py-3 text-right text-text-primary">{sw}</td>
          <td className={`px-5 py-3 text-right ${profit < 0 ? 'text-[#800020] dark:text-[#800020]' : profit > 0 ? 'text-[#004EFF] dark:text-[#004EFF]' : 'text-text-primary'}`}>
            {profitPrefix}{formatTradePrice(profit, 2)}
          </td>
        </tr>
      );
    };

    return (
      <>
        {buildSummaryRow(
          t('trade.subTotal'),
          criteria.pageTotalVolume ?? 0,
          criteria.pageTotalCommission ?? 0,
          criteria.pageTotalSwap ?? 0,
          criteria.pageTotalProfit ?? 0,
        )}
        {buildSummaryRow(
          t('trade.total'),
          criteria.totalVolume ?? 0,
          criteria.totalCommission ?? 0,
          criteria.totalSwap ?? 0,
          criteria.totalProfit ?? 0,
          true,
        )}
      </>
    );
  }, [criteria, data.length, columns, t]);

  return (
    <div className="flex min-h-full w-full min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      {/* Filter Bar */}
      <TradeFilter
        type="trade"
        filterOptions={['isClosed', 'service', 'product', 'account', 'datePicker', 'allHistory']}
        defaultDateRange={todayRange}
        onSearch={handleSearch}
        onIsClosedChange={setIsClosed}
        isLoading={isLoading}
      />

      {/* DataTable */}
      <DataTable<IBTradeRecord>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        footer={footerRows}
      />

      {/* Pagination */}
      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />
    </div>
  );
}
