'use client';

import { useState, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { usePathname } from 'next/navigation';
import { useServerAction } from '@/hooks/useServerAction';
import { Button, BalanceShow, EmptyState } from '@/components/ui';
import {
  getIBRebateDailySeries,
  getIBRebateHourlySeries,
  getIBRebateMonthlySeries,
} from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import type { IBRebateDailySeries, IBReportValue } from '@/types/ib';

type Period = 'hourly' | 'daily' | 'monthly';

const fetchers: Record<Period, typeof getIBRebateHourlySeries> = {
  hourly: getIBRebateHourlySeries,
  daily: getIBRebateDailySeries,
  monthly: getIBRebateMonthlySeries,
};

interface RebateChartWidgetProps {
  totalRebate?: IBReportValue[];
  totalLoading?: boolean;
}

function ReportValueInline({ values }: { values: IBReportValue[] | undefined }) {
  if (!values || values.length === 0) {
    return <BalanceShow balance={0} currencyId={840} />;
  }
  return (
    <>
      {values.map((v, i) => (
        <span key={i}>
          {i > 0 && ' / '}
          <BalanceShow balance={v.amount} currencyId={v.currencyId} />
        </span>
      ))}
    </>
  );
}

function formatXLabel(dateStr: string | undefined, period: Period): string {
  if (!dateStr) return '';
  if (period === 'hourly') {
    const d = new Date(dateStr);
    const h = d.getHours();
    const ampm = h >= 12 ? 'pm' : 'am';
    const h12 = h % 12 || 12;
    return `${h12}${ampm}`;
  }
  if (period === 'monthly') {
    return dateStr.split('T')?.[0]?.slice(0, 7) || '';
  }
  const parts = dateStr.split('T')?.[0]?.split('-');
  if (parts?.length === 3) return `${parts[1]}/${parts[2]}`;
  return dateStr.slice(-5);
}

export function RebateChartWidget({ totalRebate, totalLoading }: RebateChartWidgetProps) {
  const t = useTranslations('ib.dashboard');
  const pathname = usePathname();
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const requestIdRef = useRef(0);
  const pathnameRef = useRef(pathname);

  const [period, setPeriod] = useState<Period>('hourly');
  const [data, setData] = useState<IBRebateDailySeries[]>([]);
  const [loadKey, setLoadKey] = useState<string | null>(null);

  const currentKey = `${agentAccount?.uid}-${period}`;
  const isLoading = !agentAccount || currentKey !== loadKey;

  useEffect(() => {
    pathnameRef.current = pathname;
  }, [pathname]);

  useEffect(() => {
    if (!agentAccount || pathname !== '/ib') return;
    let cancelled = false;
    const currentRequestId = ++requestIdRef.current;
    const isStaleRequest = () =>
      cancelled || requestIdRef.current !== currentRequestId || pathnameRef.current !== '/ib';

    const load = async () => {
      try {
        const tz = -(new Date().getTimezoneOffset() / 60);
        const result = await execute(fetchers[period], agentAccount.uid, tz);
        if (isStaleRequest()) return;
        if (result.success && Array.isArray(result.data)) {
          setData(result.data);
        }
      } catch {
        // ignore
      } finally {
        if (!isStaleRequest()) {
          setLoadKey(`${agentAccount.uid}-${period}`);
        }
      }
    };

    load();
    return () => {
      cancelled = true;
    };
  }, [agentAccount, period, execute, pathname]);

  const maxValue = data.length > 0 ? Math.max(...data.map((d) => d.totalValue || 0), 0.01) : 0.05;
  const chartHeight = 200;
  const ySteps = 6;
  const yLabels = Array.from({ length: ySteps }, (_, i) =>
    `$${(maxValue / 100 * (ySteps - 1 - i) / (ySteps - 1)).toFixed(2)}`
  );

  return (
    <div className="flex flex-col rounded-xl border border-border bg-surface p-5">
      {/* Title */}
      <h3 className="text-xl font-semibold text-text-primary">{t('rebateChart')}</h3>

      {/* Total stat + Period tabs */}
      <div className="mt-3 flex items-center justify-between">
        <div className="text-sm text-text-secondary">
          {t('currentTotal')}：{' '}
          {totalLoading ? (
            <span className="inline-block h-4 w-20 animate-pulse rounded bg-border align-middle" />
          ) : (
            <span className="text-base font-semibold text-text-primary"><ReportValueInline values={totalRebate} /></span>
          )}
        </div>
        <div className="flex gap-1 rounded-lg bg-surface-secondary p-0.5">
          {(['hourly', 'daily', 'monthly'] as Period[]).map((p) => (
            <Button
              key={p}
              variant={period === p ? 'primary' : 'ghost'}
              size="xs"
              className="min-w-14 text-xs"
              onClick={() => setPeriod(p)}
            >
              {t(p)}
            </Button>
          ))}
        </div>
      </div>

      {/* Chart area */}
      <div className="mt-4 flex" style={{ height: chartHeight }}>
        {isLoading ? (
          <div className="flex h-full flex-1 gap-2">
            {[45, 70, 30, 55, 80, 40, 65, 35, 75, 50, 60, 38].map((h, i) => (
              <div key={i} className="flex flex-1 flex-col justify-end">
                <div className="w-full animate-pulse rounded-t bg-border" style={{ height: `${h}%` }} />
              </div>
            ))}
          </div>
        ) : data.length === 0 ? (
          <EmptyState className="flex-1 py-0" />
        ) : (
          <>
            <div className="flex flex-col justify-between pb-6 pr-3">
              {yLabels.map((label, i) => (
                <span key={i} className="text-responsive-2xs leading-none text-text-secondary">{label}</span>
              ))}
            </div>
            <div className="flex flex-1 flex-col">
              <div className="relative flex-1">
                {yLabels.map((_, i) => (
                  <div
                    key={i}
                    className="absolute left-0 w-full border-t border-border"
                    style={{ top: `${(i / (ySteps - 1)) * 100}%` }}
                  />
                ))}
                <div className="relative flex h-full w-full items-end gap-px px-1">
                  {data.map((item, idx) => {
                    const height = maxValue > 0 ? Math.max((item.totalValue / maxValue) * 100, 0.5) : 0;
                    return (
                      <div key={idx} className="group relative z-10 flex flex-1 flex-col items-center justify-end" style={{ height: '100%' }}>
                        <div
                          className="w-full max-w-4 rounded-t bg-(--color-primary)/80 transition-colors group-hover:bg-primary"
                          style={{ height: `${Math.max(height, 1)}%`, minHeight: '2px' }}
                        />
                        <div className="absolute -top-6 left-1/2 z-20 hidden -translate-x-1/2 whitespace-nowrap rounded bg-surface-secondary px-2 py-0.5 text-xs text-text-primary shadow-card group-hover:block">
                          ${((item.totalValue ?? 0) / 100).toFixed(2)}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
              <div className="mt-1 flex h-6">
                {data.map((item, idx) => {
                  const skipEvery = data.length > 12 ? Math.ceil(data.length / 12) : 1;
                  const show = idx % skipEvery === 0;
                  return (
                    <div key={idx} className="flex-1 text-center">
                      {show && (
                        <span className="text-responsive-2xs text-text-secondary">
                          {formatXLabel(item.date, period)}
                        </span>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
