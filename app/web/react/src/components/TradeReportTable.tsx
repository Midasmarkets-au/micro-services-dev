'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { Pagination, DataTable } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import { TradeFilter } from '@/components/TradeFilter';
import type { FilterField, TradeFilterValues } from '@/components/TradeFilter';
import { TimeShow } from '@/components/TimeShow';
import { handleTradeBuySellDisplay } from '@/lib/trade';

// ====================================================================
// Types
// ====================================================================

export interface TradeRecord {
  id: number;
  ticket?: number;
  symbol?: string;
  type?: number;
  cmd?: number;
  volume?: number;
  openTime?: string;
  openAt?: string;
  openPrice?: number;
  closeTime?: string;
  closeAt?: string;
  closePrice?: number;
  sl?: number;
  tp?: number;
  digits?: number;
  commission?: number;
  swap?: number;
  swaps?: number;
  profit?: number;
  accountNumber?: number;
  serviceId?: number;
}

export interface TradeCriteria {
  page?: number;
  size?: number;
  total?: number;
  isClosed?: boolean;
  pageTotalVolume?: number;
  pageTotalCommission?: number;
  pageTotalSwap?: number;
  pageTotalProfit?: number;
  totalVolume?: number;
  totalCommission?: number;
  totalSwap?: number;
  totalProfit?: number;
}

export interface TradeReportTableProps {
  /** 数据获取函数，返回 null 表示暂时无法获取（如账户未就绪） */
  fetchData: (params: Record<string, unknown>) => Promise<{
    data: TradeRecord[];
    criteria: TradeCriteria;
  } | null>;
  /** TradeFilter 显示的筛选字段 */
  filterOptions?: FilterField[];
  /** TradeFilter 各字段的默认值 */
  defaultParam?: TradeFilterValues;
  /** 每页条数，默认 25 */
  pageSize?: number;
  /** 是否显示账号列（sales/trade 和 ib/trade 需要） */
  showAccountNumber?: boolean;
  /** 变化时自动重新获取第一页数据（如 accountUid） */
  autoFetchKey?: unknown;
}

// ====================================================================
// Utilities
// ====================================================================

function toFixedSafe(value: number | undefined | null, digits: number, fallback = '--') {
  if (value == null || Number.isNaN(value)) return fallback;
  return value.toFixed(digits);
}

function getProfitColorClass(value: number): string {
  if (value < 0) return 'text-[#800020] dark:text-[#800020]';
  if (value > 0) return 'text-[#004EFF] dark:text-[#004EFF]';
  return 'text-text-primary';
}

function getTodayDateRange() {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return { from: d, to: d };
}

// ====================================================================
// Component
// ====================================================================

export function TradeReportTable({
  fetchData,
  filterOptions = ['isClosed', 'product', 'datePicker', 'allHistory'],
  defaultParam,
  pageSize = 25,
  showAccountNumber = false,
  autoFetchKey,
}: TradeReportTableProps) {
  const t = useTranslations('accounts');
  const tType = useTranslations();

  const [data, setData] = useState<TradeRecord[]>([]);
  const [criteria, setCriteria] = useState<TradeCriteria | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isClosed, setIsClosed] = useState(defaultParam?.isClosed ?? false);
  const [activeDefaultParam, setActiveDefaultParam] = useState(defaultParam);
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({});

  const fetchDataRef = useRef(fetchData);
  fetchDataRef.current = fetchData;
  const filterParamsRef = useRef(filterParams);
  filterParamsRef.current = filterParams;

  const loadData = useCallback(
    async (p: number, extraParams?: Record<string, unknown>) => {
      setIsLoading(true);
      try {
        const params = extraParams ?? filterParamsRef.current;
        const result = await fetchDataRef.current({
          page: p,
          size: pageSize,
          ...params,
        });
        if (result) {
          setData(Array.isArray(result.data) ? result.data : []);
          setCriteria(result.criteria || null);
          setTotal(result.criteria?.total || 0);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [pageSize],
  );
  const prevAutoFetchKeyRef = useRef(autoFetchKey);
  useEffect(() => {
    if (prevAutoFetchKeyRef.current === autoFetchKey) return;
    prevAutoFetchKeyRef.current = autoFetchKey;
    loadData(1);
  }, [autoFetchKey]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSearch = useCallback(
    (params: Record<string, unknown>) => {
      setFilterParams(params);
      setPage(1);
      loadData(1, params);
    },
    [loadData],
  );

  const handleFilterChange = useCallback((values: TradeFilterValues) => {
    if (values.isClosed !== undefined) {
      setIsClosed(values.isClosed);
    }
  }, []);

  const handlePageChange = useCallback(
    (p: number) => {
      setPage(p);
      loadData(p);
    },
    [loadData],
  );

  // ---- 列定义（以 account/[accountNumber] 为标准） ----
  const columns = useMemo<DataTableColumn<TradeRecord>[]>(() => {
    const cols: DataTableColumn<TradeRecord>[] = [];

    if (showAccountNumber) {
      cols.push({
        key: 'accountNumber',
        title: t('detail.table.accountNumber'),
        skeletonWidth: 'w-20',
        render: (item) => <span className="text-text-primary">{item.accountNumber ?? '-'}</span>,
      });
    }

    cols.push(
      {
        key: 'ticket',
        title: t('detail.table.ticket'),
        skeletonWidth: 'w-16',
        render: (item) => <span className="text-text-primary">{item.ticket}</span>,
      },
      {
        key: 'symbol',
        title: t('detail.table.symbol'),
        skeletonWidth: 'w-16',
        render: (item) => <span className="text-text-primary">{item.symbol}</span>,
      },
      {
        key: 'type',
        title: t('fields.type'),
        skeletonWidth: 'w-12',
        render: (item) => {
          const typeCmd = handleTradeBuySellDisplay(item);
          return <span>{tType(`type.cmd.${typeCmd}`)}</span>;
        },
      },
      {
        key: 'volume',
        title: t('detail.table.volume'),
        align: 'right',
        skeletonWidth: 'w-12',
        render: (item) => (
          <span className="text-text-primary">{toFixedSafe(item.volume, 2)}</span>
        ),
      },
      {
        key: 'openAt',
        title: t('detail.table.openTime'),
        skeletonWidth: 'w-24',
        render: (item) => (
          <TimeShow type="inFields" dateIsoString={item.openAt || item.openTime} />
        ),
      },
      {
        key: 'openPrice',
        title: t('detail.table.openPrice'),
        align: 'right',
        skeletonWidth: 'w-20',
        render: (item) => (
          <span className="text-text-primary">{toFixedSafe(item.openPrice, 2)}</span>
        ),
      },
      {
        key: 'sl',
        title: t('detail.table.sl'),
        align: 'right',
        skeletonWidth: 'w-16',
        render: (item) => (
          <span className="text-text-secondary">
            {item.sl != null && item.sl > 0 ? toFixedSafe(item.sl, 5) : '--'}
          </span>
        ),
      },
      {
        key: 'tp',
        title: t('detail.table.tp'),
        align: 'right',
        skeletonWidth: 'w-16',
        render: (item) => (
          <span className="text-text-secondary">
            {item.tp != null && item.tp > 0 ? toFixedSafe(item.tp, 5) : '--'}
          </span>
        ),
      },
    );

    if (isClosed) {
      cols.push(
        {
          key: 'closeAt',
          title: t('detail.table.closeTime'),
          skeletonWidth: 'w-24',
          render: (item: TradeRecord) => (
            <TimeShow type="inFields" dateIsoString={item.closeAt || item.closeTime} />
          ),
        },
        {
          key: 'closePrice',
          title: t('detail.table.closePrice'),
          align: 'right' as const,
          skeletonWidth: 'w-20',
          render: (item: TradeRecord) => (
            <span className="text-text-primary">
              {item.closePrice != null && item.closePrice > 0
                ? toFixedSafe(item.closePrice, 2)
                : '--'}
            </span>
          ),
        },
      );
    }

    cols.push(
      {
        key: 'commission',
        title: t('detail.table.commission'),
        align: 'right',
        skeletonWidth: 'w-16',
        render: (item) => (
          <span className="text-text-secondary">{toFixedSafe(item.commission, 2)}</span>
        ),
      },
      {
        key: 'swap',
        title: t('detail.table.swap'),
        align: 'right',
        skeletonWidth: 'w-12',
        render: (item) => (
          <span className="text-text-secondary">
            {toFixedSafe(item.swap ?? item.swaps, 2)}
          </span>
        ),
      },
      {
        key: 'profit',
        title: t('detail.table.pl'),
        align: 'right',
        skeletonWidth: 'w-16',
        render: (item) => {
          const profit = item.profit ?? 0;
          return (
            <span className={`font-semibold ${getProfitColorClass(profit)}`}>
              {profit > 0 ? '+' : ''}
              {toFixedSafe(profit, 2, '0.00')}
            </span>
          );
        },
      },
    );

    return cols;
  }, [t, tType, isClosed, showAccountNumber]);

  // ---- 汇总行 ----
  const footerRows = useMemo(() => {
    if (!criteria || data.length === 0) return null;
    const volumeColIndex = columns.findIndex((col) => col.key === 'volume');
    const commissionColIndex = columns.findIndex((col) => col.key === 'commission');
    const swapColIndex = columns.findIndex((col) => col.key === 'swap');
    const profitColIndex = columns.findIndex((col) => col.key === 'profit');

    if (volumeColIndex < 0 || commissionColIndex < 0 || swapColIndex < 0 || profitColIndex < 0)
      return null;

    const beforeVolumeColSpan = Math.max(volumeColIndex - 1, 0);
    const middleColSpan = Math.max(commissionColIndex - volumeColIndex - 1, 0);

    const buildSummaryRow = (
      label: string,
      volume: number,
      commission: number,
      swap: number,
      profit: number,
      highlight?: boolean,
    ) => (
      <tr
        key={label}
        className={highlight ? 'bg-surface-secondary/50 font-semibold' : 'font-medium'}
      >
        <td className="px-5 py-3 text-text-primary">{label}</td>
        {beforeVolumeColSpan > 0 && (
          <td className="px-5 py-3" colSpan={beforeVolumeColSpan} />
        )}
        <td className="px-5 py-3 text-right text-text-primary">
          {toFixedSafe(volume, 2, '0.00')}
        </td>
        {middleColSpan > 0 && <td className="px-5 py-3" colSpan={middleColSpan} />}
        <td className="px-5 py-3 text-right text-text-primary">
          {toFixedSafe(commission, 2, '0.00')}
        </td>
        <td className="px-5 py-3 text-right text-text-primary">
          {toFixedSafe(swap, 2, '0.00')}
        </td>
        <td
          className={`px-5 py-3 text-right font-semibold ${getProfitColorClass(profit)}`}
        >
          {profit > 0 ? '+' : ''}
          {toFixedSafe(profit, 2, '0.00')}
        </td>
      </tr>
    );

    return (
      <>
        {buildSummaryRow(
          t('detail.table.subTotal'),
          criteria.pageTotalVolume ?? 0,
          criteria.pageTotalCommission ?? 0,
          criteria.pageTotalSwap ?? 0,
          criteria.pageTotalProfit ?? 0,
        )}
        {buildSummaryRow(
          t('detail.table.total'),
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
    <>
      <TradeFilter
        type="trade"
        filterOptions={filterOptions}
        defaultParam={activeDefaultParam}
        onSearch={handleSearch}
        onChange={handleFilterChange}
        isLoading={isLoading}
      />

      <DataTable<TradeRecord>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        skeletonRows={5}
        footer={footerRows}
      />

      <Pagination
        page={page}
        total={total}
        size={pageSize}
        onPageChange={handlePageChange}
      />
    </>
  );
}
