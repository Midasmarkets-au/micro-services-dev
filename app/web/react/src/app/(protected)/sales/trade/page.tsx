'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesTradeReports } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { ServiceTypes } from '@/types/accounts';
import { Pagination, DataTable } from '@/components/ui';
import type { DateRange } from '@/components/ui';
import { TradeFilter } from '@/components/TradeFilter';
import type { DataTableColumn } from '@/components/ui';
import type { SalesTradeRecord, SalesTradeListResponse } from '@/types/sales';

function formatTradePrice(price: number | undefined | null, digits?: number) {
  if (price == null || isNaN(price)) return '-';
  return parseFloat(price.toFixed(digits ?? 2)).toString();
}

function formatTradeTime(time: string | undefined | null) {
  if (!time) return '-';
  const d = new Date(time);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}

function getTradeTypeLabel(trade: SalesTradeRecord, t: (key: string) => string) {
  const cmd = trade.cmd ?? trade.type;
  if (cmd === 0) return t('trade.sell');
  if (cmd === 1) return t('trade.buy');
  return String(cmd ?? '-');
}

export default function SalesTradePage() {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [data, setData] = useState<SalesTradeRecord[]>([]);
  const [criteria, setCriteria] = useState<SalesTradeListResponse['criteria'] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 25;

  const [isClosed, setIsClosed] = useState(true);

  const todayRange = useMemo<DateRange>(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return { from: d, to: d };
  }, []);

  const [filterParams, setFilterParams] = useState<Record<string, unknown>>(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return {
      isClosed: true,
      serviceId: ServiceTypes.MetaTrader5,
      from: d.toISOString(),
      to: d.toISOString(),
    };
  });

  const fetchData = useCallback(
    async (p: number, extraParams?: Record<string, unknown>) => {
      if (!salesAccount) return;
      setIsLoading(true);
      try {
        const params = extraParams ?? filterParams;
        const result = await executeRef.current(getSalesTradeReports, salesAccount.uid, {
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
    [salesAccount, filterParams],
  );

  const salesUid = salesAccount?.uid;
  useEffect(() => {
    if (salesUid) fetchData(1);
    else setIsLoading(false);
  }, [salesUid]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSearch = (params: Record<string, unknown>) => {
    setFilterParams(params);
    setPage(1);
    fetchData(1, params);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    fetchData(p);
  };

  const columns = useMemo<DataTableColumn<SalesTradeRecord>[]>(() => {
    const cols: DataTableColumn<SalesTradeRecord>[] = [
      {
        key: 'accountNumber',
        title: t('trade.accountNumber'),
        skeletonWidth: 'w-20',
        render: (item) => item.accountNumber ?? '-',
      },
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
        render: (item) => getTradeTypeLabel(item, t),
      },
      {
        key: 'volume',
        title: t('trade.lots'),
        skeletonWidth: 'w-14',
        render: (item) => item.volume ?? 0,
      },
      {
        key: 'openTime',
        title: t('trade.openTime'),
        skeletonWidth: 'w-32',
        render: (item) => (
          <span className="whitespace-nowrap text-xs">
            {formatTradeTime(item.openAt || item.openTime)}
          </span>
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
        key: 'sl',
        title: t('trade.sl'),
        skeletonWidth: 'w-14',
        render: (item) => item.sl ?? 0,
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
          render: (item: SalesTradeRecord) => (
            <span className="whitespace-nowrap text-xs">
              {formatTradeTime(item.closeAt || item.closeTime)}
            </span>
          ),
        },
        {
          key: 'closePrice',
          title: t('trade.closePrice'),
          skeletonWidth: 'w-20',
          align: 'right' as const,
          render: (item: SalesTradeRecord) => (
            <span className="font-semibold text-text-primary">{formatTradePrice(item.closePrice, item.digits)}</span>
          ),
        },
      ] : []),
      {
        key: 'commission',
        title: t('trade.commission'),
        skeletonWidth: 'w-14',
        render: (item) => item.commission ?? 0,
      },
      {
        key: 'swaps',
        title: t('trade.swaps'),
        skeletonWidth: 'w-16',
        render: (item) => item.swaps ?? item.swap ?? 0,
      },
      {
        key: 'profit',
        title: t('trade.profit'),
        skeletonWidth: 'w-20',
        align: 'right' as const,
        render: (item: SalesTradeRecord) => {
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
  }, [t, isClosed]);

  const footerRows = useMemo(() => {
    if (!criteria || data.length === 0) return null;
    const colCount = columns.length;

    const buildSummaryRow = (label: string, vol: number, comm: number, sw: number, profit: number, highlight?: boolean) => {
      const profitPrefix = profit > 0 ? '+' : '';
      return (
        <tr key={label} className={highlight ? 'bg-surface-secondary/50 font-semibold' : 'font-medium'}>
          <td className="px-4 py-3 text-text-primary">{label}</td>
          <td className="px-4 py-3" colSpan={isClosed ? 3 : 3} />
          <td className="px-4 py-3 text-text-primary">{vol}</td>
          <td className="px-4 py-3" colSpan={isClosed ? colCount - 9 : colCount - 7} />
          <td className="px-4 py-3 text-text-primary">{comm}</td>
          <td className="px-4 py-3 text-text-primary">{sw}</td>
          <td className={`px-4 py-3 text-right ${profit < 0 ? 'text-[#800020] dark:text-[#800020]' : profit > 0 ? 'text-[#004EFF] dark:text-[#004EFF]' : 'text-text-primary'}`}>
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
  }, [criteria, data.length, columns.length, isClosed, t]);

  return (
    <div className="flex min-h-full w-full flex-col gap-5 rounded bg-surface p-5">
      <TradeFilter
        type="trade"
        translationNamespace="sales"
        filterOptions={['isClosed', 'service', 'product', 'account', 'datePicker', 'allHistory']}
        defaultDateRange={todayRange}
        onSearch={handleSearch}
        onIsClosedChange={setIsClosed}
        isLoading={isLoading}
      />

      <DataTable<SalesTradeRecord>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        footer={footerRows}
        className="flex-1"
      />

      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />
    </div>
  );
}
