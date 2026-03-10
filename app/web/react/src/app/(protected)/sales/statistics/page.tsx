'use client';

import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesStatistics } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { Button } from '@/components/ui';
import type { SalesStatistics, SalesHierarchyNode } from '@/types/sales';

type TimeRange = '30' | '7' | 'custom';

function StatSummaryCard({ title, value, loading }: { title: string; value: string | number; loading: boolean }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <p className="text-xs text-text-secondary">{title}</p>
      {loading ? (
        <div className="mt-2 h-6 w-20 animate-pulse rounded bg-surface-secondary" />
      ) : (
        <p className="mt-1 text-xl font-semibold text-text-primary">{value}</p>
      )}
    </div>
  );
}

function HierarchyRow({ node, depth = 0 }: { node: SalesHierarchyNode; depth?: number }) {
  const [expanded, setExpanded] = useState(false);
  const hasChildren = node.children && node.children.length > 0;

  return (
    <>
      <tr className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
        <td className="px-4 py-3 text-text-primary" style={{ paddingLeft: `${16 + depth * 24}px` }}>
          <div className="flex items-center gap-2">
            {hasChildren && (
              <button type="button" onClick={() => setExpanded(!expanded)} className="text-text-secondary hover:text-text-primary">
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
        <td className="px-4 py-3 text-text-primary">{node.trades ?? 0}</td>
        <td className="px-4 py-3 text-text-primary">{Number(node.netDeposit ?? 0).toFixed(2)}</td>
        <td className="px-4 py-3 text-text-primary">{Number(node.deposit ?? 0).toFixed(2)}</td>
        <td className="px-4 py-3 text-text-primary">{Number(node.withdrawal ?? 0).toFixed(2)}</td>
        <td className="px-4 py-3 text-text-primary">{Number(node.rebate ?? 0).toFixed(2)}</td>
        <td className="px-4 py-3 text-text-primary">{Number(node.lots ?? 0).toFixed(2)}</td>
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
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [statistics, setStatistics] = useState<SalesStatistics | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [timeRange, setTimeRange] = useState<TimeRange>('30');
  const [customFrom, setCustomFrom] = useState('');
  const [customTo, setCustomTo] = useState('');

  const buildParams = useMemo(() => {
    if (timeRange === 'custom') {
      return { from: customFrom || undefined, to: customTo || undefined };
    }
    const now = new Date();
    const from = new Date();
    from.setDate(now.getDate() - Number(timeRange));
    return {
      timeRange,
      from: from.toISOString().split('T')[0],
      to: now.toISOString().split('T')[0],
    };
  }, [timeRange, customFrom, customTo]);

  const loadStatistics = useCallback(async () => {
    setIsLoading(true);
    const result = await execute(getSalesStatistics, buildParams);
    if (result.success && result.data) {
      setStatistics(result.data);
    }
    setIsLoading(false);
  }, [buildParams, execute]);

  useEffect(() => {
    loadStatistics();
  }, [loadStatistics]);

  const summary = statistics?.summaryStats;

  return (
    <div className="flex w-full flex-col gap-5">
      {salesAccount && (
        <>
          <div className="flex flex-wrap items-center gap-3">
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => setTimeRange('7')}
                className={`rounded-lg px-3 py-1.5 text-sm ${timeRange === '7' ? 'bg-primary text-white' : 'bg-surface-secondary text-text-secondary'}`}
              >
                {t('days7')}
              </button>
              <button
                type="button"
                onClick={() => setTimeRange('30')}
                className={`rounded-lg px-3 py-1.5 text-sm ${timeRange === '30' ? 'bg-primary text-white' : 'bg-surface-secondary text-text-secondary'}`}
              >
                {t('days30')}
              </button>
              <button
                type="button"
                onClick={() => setTimeRange('custom')}
                className={`rounded-lg px-3 py-1.5 text-sm ${timeRange === 'custom' ? 'bg-primary text-white' : 'bg-surface-secondary text-text-secondary'}`}
              >
                {tSales('dashboard.custom')}
              </button>
            </div>
            {timeRange === 'custom' && (
              <div className="flex items-center gap-2">
                <input
                  type="date"
                  value={customFrom}
                  onChange={(e) => setCustomFrom(e.target.value)}
                  className="h-9 rounded-lg border border-border bg-input-bg px-2 text-sm text-text-primary"
                />
                <span className="text-text-secondary">-</span>
                <input
                  type="date"
                  value={customTo}
                  onChange={(e) => setCustomTo(e.target.value)}
                  className="h-9 rounded-lg border border-border bg-input-bg px-2 text-sm text-text-primary"
                />
                <Button size="sm" onClick={loadStatistics}>
                  {tSales('action.search')}
                </Button>
              </div>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
            <StatSummaryCard title={t('totalTrades')} value={summary?.totalTrades ?? 0} loading={isLoading} />
            <StatSummaryCard title={t('totalNetDeposit')} value={Number(summary?.totalNetDeposit ?? 0).toFixed(2)} loading={isLoading} />
            <StatSummaryCard title={t('totalRebate')} value={Number(summary?.totalRebate ?? 0).toFixed(2)} loading={isLoading} />
            <StatSummaryCard title={t('totalDeposit')} value={Number(summary?.totalDeposit ?? 0).toFixed(2)} loading={isLoading} />
            <StatSummaryCard title={t('totalWithdrawal')} value={Number(summary?.totalWithdrawal ?? 0).toFixed(2)} loading={isLoading} />
            <StatSummaryCard title={t('totalLots')} value={Number(summary?.totalLots ?? 0).toFixed(2)} loading={isLoading} />
          </div>

          {statistics?.timeSeriesData && statistics.timeSeriesData.length > 0 && (
            <div className="rounded-xl border border-border bg-surface">
              <div className="border-b border-border p-4">
                <h3 className="text-sm font-semibold text-text-primary">{t('tradeTrend')}</h3>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-border text-xs text-text-secondary">
                      <th className="px-4 py-3">Date</th>
                      <th className="px-4 py-3">Trades</th>
                      <th className="px-4 py-3">Deposit</th>
                      <th className="px-4 py-3">Withdrawal</th>
                      <th className="px-4 py-3">Net Deposit</th>
                      <th className="px-4 py-3">Rebate</th>
                    </tr>
                  </thead>
                  <tbody>
                    {statistics.timeSeriesData.map((row, idx) => (
                      <tr key={idx} className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
                        <td className="px-4 py-3 text-text-primary">{row.date}</td>
                        <td className="px-4 py-3">{row.trades}</td>
                        <td className="px-4 py-3">{Number(row.deposit).toFixed(2)}</td>
                        <td className="px-4 py-3">{Number(row.withdrawal).toFixed(2)}</td>
                        <td className="px-4 py-3">{Number(row.netDeposit).toFixed(2)}</td>
                        <td className="px-4 py-3">{Number(row.rebate).toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {statistics?.productDistribution && statistics.productDistribution.length > 0 && (
            <div className="rounded-xl border border-border bg-surface">
              <div className="border-b border-border p-4">
                <h3 className="text-sm font-semibold text-text-primary">{t('productDistribution')}</h3>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-border text-xs text-text-secondary">
                      <th className="px-4 py-3">Symbol</th>
                      <th className="px-4 py-3">Count</th>
                      <th className="px-4 py-3">Percentage</th>
                    </tr>
                  </thead>
                  <tbody>
                    {statistics.productDistribution.map((row, idx) => (
                      <tr key={idx} className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
                        <td className="px-4 py-3 text-text-primary">{row.symbol}</td>
                        <td className="px-4 py-3">{row.count}</td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-2">
                            <div className="h-2 w-24 rounded-full bg-surface-secondary">
                              <div className="h-2 rounded-full bg-primary" style={{ width: `${Math.min(row.percentage, 100)}%` }} />
                            </div>
                            <span className="text-xs text-text-secondary">{Number(row.percentage).toFixed(1)}%</span>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {statistics?.hierarchyData && statistics.hierarchyData.length > 0 && (
            <div className="rounded-xl border border-border bg-surface">
              <div className="border-b border-border p-4">
                <h3 className="text-sm font-semibold text-text-primary">{t('hierarchy')}</h3>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-border text-xs text-text-secondary">
                      <th className="px-4 py-3">Name</th>
                      <th className="px-4 py-3">Group</th>
                      <th className="px-4 py-3">Trades</th>
                      <th className="px-4 py-3">Net Deposit</th>
                      <th className="px-4 py-3">Deposit</th>
                      <th className="px-4 py-3">Withdrawal</th>
                      <th className="px-4 py-3">Rebate</th>
                      <th className="px-4 py-3">Lots</th>
                    </tr>
                  </thead>
                  <tbody>
                    {statistics.hierarchyData.map((node) => (
                      <HierarchyRow key={node.id} node={node} />
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
