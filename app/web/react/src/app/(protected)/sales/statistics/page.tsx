'use client';

import dynamic from 'next/dynamic';
import Image from 'next/image';
import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import type { ComponentType, ReactNode } from 'react';
import { useTranslations } from 'next-intl';
import type { ApexOptions } from 'apexcharts';
import type { Props as ApexChartProps } from 'react-apexcharts';
import { useServerAction } from '@/hooks/useServerAction';
import { fetchAction } from '@/lib/api/browser-client';
import { useSalesStore } from '@/stores/salesStore';
import { BalanceShow, Button, CurrencyCodeMap, Input, SimpleSelect } from '@/components/ui';
import type { SalesStatistics, SalesHierarchyNode } from '@/types/sales';

const ApexChart = dynamic<ApexChartProps>(
  () => import('react-apexcharts').then((mod) => mod.default as unknown as ComponentType<ApexChartProps>),
  { ssr: false }
);

type TimeRange = '30' | '7' | 'custom';
type ChartType = 'area' | 'line';
type UserType = 'sale' | 'ib' | 'client';

interface SearchCriteria {
  userUid: string;
  userType: UserType;
  timeRange: TimeRange;
  from: string;
  to: string;
}

function formatDate(date: Date): string {
  return date.toISOString().split('T')[0];
}

function buildRange(days: number) {
  const to = new Date();
  const from = new Date();
  from.setDate(to.getDate() - days);
  return { from: formatDate(from), to: formatDate(to) };
}

function createDefaultCriteria(): SearchCriteria {
  return {
    userUid: '',
    userType: 'sale',
    timeRange: '30',
    ...buildRange(30),
  };
}

function chartMoneyFormatter(value: number): string {
  return `$${(Number(value || 0) / 100).toFixed(2)}`;
}

function SummaryMetricCard({
  title,
  value,
  iconSrc,
  loading,
}: {
  title: string;
  value: ReactNode;
  iconSrc: string;
  loading: boolean;
}) {
  return (
    <div className="flex h-full flex-col rounded-xl border border-border bg-surface shadow-sm">
      <div className="border-0 px-5 pb-0 pt-5">
        <div className="flex items-center gap-3">
          <span className="flex size-9 items-center justify-center rounded-lg bg-primary/10">
            <Image src={iconSrc} alt="" width={22} height={22} className="dark:brightness-0 dark:invert" />
          </span>
          <h4 className="text-base font-semibold text-text-primary">{title}</h4>
        </div>
      </div>
      <div className="flex flex-1 items-center px-5 py-6">
        {loading ? (
          <div className="h-9 w-32 animate-pulse rounded bg-surface-secondary" />
        ) : (
          <div className="text-3xl font-semibold leading-none text-primary">{value}</div>
        )}
      </div>
    </div>
  );
}

function ChartCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="flex h-full flex-col rounded-xl border border-border bg-surface shadow-sm">
      <div className="border-b border-border px-5 py-4">
        <h3 className="text-base font-semibold text-text-primary">{title}</h3>
      </div>
      <div className="min-h-[332px] flex-1 p-4">{children}</div>
    </div>
  );
}

function ChartEmptyState() {
  return <div className="flex h-[300px] items-center justify-center text-sm text-text-secondary">--</div>;
}

function HierarchyRow({ node, depth = 0 }: { node: SalesHierarchyNode; depth?: number }) {
  const [expanded, setExpanded] = useState(false);
  const hasChildren = node.children && node.children.length > 0;
  const currencyId = node.currencyId ?? 840;

  return (
    <>
      <tr className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
        <td className="px-4 py-3 text-text-primary" style={{ paddingLeft: `${16 + depth * 24}px` }}>
          <div className="flex items-center gap-2">
            {hasChildren && (
              <button
                type="button"
                onClick={() => setExpanded(!expanded)}
                className="text-text-secondary hover:text-text-primary"
              >
                {expanded ? '▼' : '►'}
              </button>
            )}
            <span>{node.name || '--'}</span>
            {node.type && (
              <span className="rounded-full bg-surface-secondary px-2 py-0.5 text-xs text-text-secondary">{node.type}</span>
            )}
          </div>
        </td>
        <td className="px-4 py-3 text-text-secondary">{node.groupCode || '-'}</td>
        <td className="px-4 py-3 text-text-secondary">{CurrencyCodeMap[node.currencyId ?? 840] || 'USD'}</td>
        <td className="px-4 py-3 text-right text-text-primary">{node.trades ?? 0}</td>
        <td className="px-4 py-3 text-right text-text-primary">
          <BalanceShow balance={Number(node.netDeposit ?? 0)} currencyId={currencyId} />
        </td>
        <td className="px-4 py-3 text-right text-text-primary">
          <BalanceShow balance={Number(node.deposit ?? 0)} currencyId={currencyId} />
        </td>
        <td className="px-4 py-3 text-right text-text-primary">
          <BalanceShow balance={Number(node.withdrawal ?? 0)} currencyId={currencyId} />
        </td>
        <td className="px-4 py-3 text-right text-text-primary">
          <BalanceShow balance={Number(node.rebate ?? 0)} currencyId={currencyId} />
        </td>
        <td className="px-4 py-3 text-right text-text-primary">{Number(node.lots ?? 0).toFixed(2)}</td>
      </tr>
      {expanded && hasChildren && node.children!.map((child) => (
        <HierarchyRow key={child.id} node={child} depth={depth + 1} />
      ))}
    </>
  );
}

export default function SalesStatisticsPage() {
  const t = useTranslations('sales.statistics');
  const tSales = useTranslations('sales');
  const tCommon = useTranslations('common');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const isSalesInitialized = useSalesStore((s) => s.isInitialized);
  const didLoadRef = useRef(false);

  const [statistics, setStatistics] = useState<SalesStatistics | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [criteria, setCriteria] = useState<SearchCriteria>(() => createDefaultCriteria());
  const [chartType, setChartType] = useState<ChartType>('area');

  const timeRangeError = useMemo(() => {
    if (criteria.timeRange !== 'custom') return '';
    if (!criteria.from || !criteria.to) return t('pleaseSelectTime');
    const from = new Date(criteria.from);
    const to = new Date(criteria.to);
    if (to < from) return t('endTimeBeforeStart');
    const daysDiff = Math.floor((to.getTime() - from.getTime()) / 86400000);
    if (daysDiff > 30) return t('timeRangeExceeds30Days');
    return '';
  }, [criteria.from, criteria.timeRange, criteria.to, t]);

  const loadStatistics = useCallback(
    async (nextCriteria: SearchCriteria) => {
      setIsLoading(true);
      try {
        const params = {
          userUid: nextCriteria.userUid || undefined,
          userType: nextCriteria.userType,
          timeRange: nextCriteria.timeRange,
          from: nextCriteria.from || undefined,
          to: nextCriteria.to || undefined,
        };
        const result = await execute(async () => fetchAction<SalesStatistics>('getSalesStatistics', params));
        if (result.success && result.data) {
          setStatistics(result.data);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [execute]
  );

  useEffect(() => {
    if (!isSalesInitialized || !salesAccount) return;
    if (didLoadRef.current) return;
    const timer = window.setTimeout(() => {
      didLoadRef.current = true;
      void loadStatistics(criteria);
    }, 0);
    return () => window.clearTimeout(timer);
  }, [criteria, isSalesInitialized, loadStatistics, salesAccount]);

  const handleTimeRangeChange = (value: string) => {
    const timeRange = value as TimeRange;
    if (timeRange === 'custom') {
      setCriteria((prev) => ({ ...prev, timeRange }));
      return;
    }
    const nextCriteria = {
      ...criteria,
      timeRange,
      ...buildRange(Number(timeRange)),
    };
    setCriteria(nextCriteria);
    void loadStatistics(nextCriteria);
  };

  const handleSearch = () => {
    if (timeRangeError) return;
    void loadStatistics(criteria);
  };

  const handleReset = () => {
    const nextCriteria = createDefaultCriteria();
    setCriteria(nextCriteria);
    setChartType('area');
    void loadStatistics(nextCriteria);
  };

  const commonTimeSeriesOptions = useCallback(
    (colors: string[], moneyAxis = false): ApexOptions => ({
      chart: {
        type: chartType,
        toolbar: { show: false },
        zoom: { enabled: false },
        fontFamily: 'inherit',
      },
      colors,
      dataLabels: { enabled: false },
      stroke: {
        curve: 'smooth',
        width: chartType === 'area' ? 2 : 3,
      },
      fill: {
        type: chartType === 'area' ? 'gradient' : 'solid',
        gradient: {
          opacityFrom: 0.6,
          opacityTo: 0.1,
        },
      },
      grid: {
        borderColor: '#EFF2F5',
        strokeDashArray: 4,
      },
      xaxis: {
        categories: statistics?.timeSeriesData?.map((item) => item.date) ?? [],
        labels: { style: { fontSize: '12px', colors: '#7E8299' } },
        axisBorder: { show: false },
        axisTicks: { show: false },
      },
      yaxis: {
        labels: {
          style: { fontSize: '12px', colors: '#7E8299' },
          formatter: moneyAxis ? chartMoneyFormatter : (value) => String(Math.round(value)),
        },
      },
      legend: {
        position: 'top',
        horizontalAlign: 'right',
        labels: { colors: '#7E8299' },
      },
      tooltip: {
        y: {
          formatter: moneyAxis ? chartMoneyFormatter : (value) => String(value),
        },
      },
    }),
    [chartType, statistics?.timeSeriesData]
  );

  const tradeChartSeries = useMemo(
    () => [
      {
        name: t('tradesCount'),
        data: statistics?.timeSeriesData?.map((item) => item.trades) ?? [],
      },
    ],
    [statistics?.timeSeriesData, t]
  );

  const fundFlowChartSeries = useMemo(
    () => [
      {
        name: t('deposit'),
        data: statistics?.timeSeriesData?.map((item) => item.deposit) ?? [],
      },
      {
        name: t('withdrawal'),
        data: statistics?.timeSeriesData?.map((item) => item.withdrawal) ?? [],
      },
      {
        name: t('netDeposit'),
        data: statistics?.timeSeriesData?.map((item) => item.netDeposit) ?? [],
      },
    ],
    [statistics?.timeSeriesData, t]
  );

  const rebateChartSeries = useMemo(
    () => [
      {
        name: t('rebate'),
        data: statistics?.timeSeriesData?.map((item) => item.rebate) ?? [],
      },
    ],
    [statistics?.timeSeriesData, t]
  );

  const productChartOptions = useMemo<ApexOptions>(
    () => ({
      chart: {
        type: 'pie',
        fontFamily: 'inherit',
      },
      labels: statistics?.productDistribution?.map((item) => item.symbol) ?? [],
      colors: ['#0095FF', '#50CD89', '#FFC700', '#F1416C', '#7239EA'],
      legend: {
        position: 'bottom',
        labels: { colors: '#7E8299' },
      },
      tooltip: {
        y: {
          formatter: (value, opts) => {
            const seriesIndex = opts?.seriesIndex ?? -1;
            const percentage = statistics?.productDistribution?.[seriesIndex]?.percentage ?? 0;
            return `${value} (${Number(percentage).toFixed(1)}%)`;
          },
        },
      },
    }),
    [statistics?.productDistribution]
  );

  const productChartSeries = useMemo(
    () => statistics?.productDistribution?.map((item) => item.count) ?? [],
    [statistics?.productDistribution]
  );

  const summary = statistics?.summaryStats;
  const hasTimeSeries = Boolean(statistics?.timeSeriesData?.length);
  const hasProductDistribution = Boolean(statistics?.productDistribution?.length);

  return (
    <div className="flex w-full flex-col gap-3">
      {!isSalesInitialized ? (
        <div className="rounded-xl border border-border bg-surface p-8 text-center text-sm text-text-secondary">
          {tCommon('loading')}
        </div>
      ) : !salesAccount ? (
        <div className="rounded-xl border border-border bg-surface p-8 text-center text-sm text-text-secondary">
          {tSales('noSalesAccount')}
        </div>
      ) : (
        <>
          <div className="rounded-xl border border-border bg-surface p-5 shadow-sm">
            <div className="grid grid-cols-1 items-end gap-3 lg:grid-cols-12">
              <div className="lg:col-span-6">
                <Input
                  inputSize="sm"
                  value={criteria.userUid}
                  placeholder={t('searchUserUid')}
                  onChange={(event) => setCriteria((prev) => ({ ...prev, userUid: event.target.value }))}
                  onClear={() => setCriteria((prev) => ({ ...prev, userUid: '' }))}
                />
              </div>
              <div className="grid grid-cols-1 gap-2 sm:grid-cols-4 lg:col-span-6">
                <SimpleSelect
                  value={criteria.userType}
                  onChange={(value) => setCriteria((prev) => ({ ...prev, userType: value as UserType }))}
                  placeholder={t('selectIdentity')}
                  triggerSize="sm"
                  options={[
                    { value: 'sale', label: t('roleSale') },
                    { value: 'ib', label: t('roleIb') },
                    { value: 'client', label: t('roleClient') },
                  ]}
                />
                <SimpleSelect
                  value={criteria.timeRange}
                  onChange={handleTimeRangeChange}
                  placeholder={t('selectTimeRange')}
                  triggerSize="sm"
                  options={[
                    { value: '30', label: t('last30Days') },
                    { value: '7', label: t('last7Days') },
                    { value: 'custom', label: t('customTime') },
                  ]}
                />
                <SimpleSelect
                  value={chartType}
                  onChange={(value) => setChartType(value as ChartType)}
                  placeholder={t('selectChartType')}
                  triggerSize="sm"
                  options={[
                    { value: 'area', label: t('areaChart') },
                    { value: 'line', label: t('lineChart') },
                  ]}
                />
                <div className="flex gap-1">
                  <Button type="button" variant="outline" size="sm" className="flex-1" onClick={handleReset}>
                    {tCommon('reset')}
                  </Button>
                  <Button
                    type="button"
                    size="sm"
                    className="flex-1"
                    loading={isLoading}
                    disabled={Boolean(timeRangeError)}
                    onClick={handleSearch}
                  >
                    {tCommon('search')}
                  </Button>
                </div>
              </div>
            </div>
            {criteria.timeRange === 'custom' && (
              <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)] lg:ml-auto lg:w-1/2">
                <input
                  type="date"
                  value={criteria.from}
                  onChange={(event) => setCriteria((prev) => ({ ...prev, from: event.target.value }))}
                  className="input-field h-9! px-3! text-sm"
                />
                <input
                  type="date"
                  value={criteria.to}
                  onChange={(event) => setCriteria((prev) => ({ ...prev, to: event.target.value }))}
                  className="input-field h-9! px-3! text-sm"
                />
                {timeRangeError && <p className="text-sm text-error sm:col-span-2">{timeRangeError}</p>}
              </div>
            )}
          </div>

          <div className="grid grid-cols-1 gap-2 md:grid-cols-3">
            <SummaryMetricCard
              title={t('totalTrades')}
              iconSrc="/images/icons/ib/jiaoyi.svg"
              value={summary?.totalTrades ?? 0}
              loading={isLoading}
            />
            <SummaryMetricCard
              title={t('totalNetDeposit')}
              iconSrc="/images/icons/ib/zijin.svg"
              value={<BalanceShow balance={Number(summary?.totalNetDeposit ?? 0)} currencyId={840} />}
              loading={isLoading}
            />
            <SummaryMetricCard
              title={t('totalRebate')}
              iconSrc="/images/icons/ib/fanyong.svg"
              value={<BalanceShow balance={Number(summary?.totalRebate ?? 0)} currencyId={840} />}
              loading={isLoading}
            />
            <SummaryMetricCard
              title={t('totalDeposit')}
              iconSrc="/images/icons/ib/zijin.svg"
              value={<BalanceShow balance={Number(summary?.totalDeposit ?? 0)} currencyId={840} />}
              loading={isLoading}
            />
            <SummaryMetricCard
              title={t('totalWithdrawal')}
              iconSrc="/images/icons/ib/zijin.svg"
              value={<BalanceShow balance={Number(summary?.totalWithdrawal ?? 0)} currencyId={840} />}
              loading={isLoading}
            />
            <SummaryMetricCard
              title={t('totalLots')}
              iconSrc="/images/icons/ib/jiaoyi.svg"
              value={Number(summary?.totalLots ?? 0).toFixed(2)}
              loading={isLoading}
            />
          </div>

          <div className="grid grid-cols-1 gap-2 lg:grid-cols-2">
            <ChartCard title={t('tradeTrend')}>
              {hasTimeSeries ? (
                <ApexChart
                  type={chartType}
                  height={300}
                  options={commonTimeSeriesOptions(['#0095FF'])}
                  series={tradeChartSeries}
                />
              ) : (
                <ChartEmptyState />
              )}
            </ChartCard>

            <ChartCard title={t('fundFlow')}>
              {hasTimeSeries ? (
                <ApexChart
                  type={chartType}
                  height={300}
                  options={commonTimeSeriesOptions(['#50CD89', '#F1416C', '#FFC700'], true)}
                  series={fundFlowChartSeries}
                />
              ) : (
                <ChartEmptyState />
              )}
            </ChartCard>

            <ChartCard title={t('rebateTrend')}>
              {hasTimeSeries ? (
                <ApexChart
                  type={chartType}
                  height={300}
                  options={commonTimeSeriesOptions(['#7239EA'], true)}
                  series={rebateChartSeries}
                />
              ) : (
                <ChartEmptyState />
              )}
            </ChartCard>

            <ChartCard title={t('productDistribution')}>
              {hasProductDistribution ? (
                <ApexChart type="pie" height={300} options={productChartOptions} series={productChartSeries} />
              ) : (
                <ChartEmptyState />
              )}
            </ChartCard>
          </div>

          <div className="rounded-xl border border-border bg-surface shadow-sm">
            <div className="border-b border-border px-5 py-4">
              <h3 className="text-base font-semibold text-text-primary">{t('hierarchyData')}</h3>
            </div>
            <div className="overflow-x-auto p-4">
              <table className="w-full whitespace-nowrap text-left text-sm">
                <thead>
                  <tr className="border-b border-border text-xs text-text-secondary">
                    <th className="px-4 py-3">{t('name')}</th>
                    <th className="px-4 py-3">{t('groupCode')}</th>
                    <th className="px-4 py-3">{tSales('deposit.currency')}</th>
                    <th className="px-4 py-3 text-right">{t('tradesCount')}</th>
                    <th className="px-4 py-3 text-right">{t('netDeposit')}</th>
                    <th className="px-4 py-3 text-right">{t('deposit')}</th>
                    <th className="px-4 py-3 text-right">{t('withdrawal')}</th>
                    <th className="px-4 py-3 text-right">{t('rebate')}</th>
                    <th className="px-4 py-3 text-right">{t('lots')}</th>
                  </tr>
                </thead>
                <tbody>
                  {statistics?.hierarchyData?.length ? (
                    statistics.hierarchyData.map((node) => <HierarchyRow key={node.id} node={node} />)
                  ) : (
                    <tr>
                      <td colSpan={9} className="px-4 py-12 text-center text-text-secondary">
                        --
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
