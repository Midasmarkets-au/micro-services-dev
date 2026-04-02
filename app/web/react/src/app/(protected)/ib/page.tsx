'use client';

import { useState, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { usePathname } from 'next/navigation';
import { useServerAction } from '@/hooks/useServerAction';
import {
  getIBRebateTodayValue,
  getIBRebateTotalValue,
  getIBTradeTodayVolume,
  getIBTodayAccountCreation,
  getIBDepositTodayValue,
} from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { BalanceShow } from '@/components/ui';
import { IBAccountSelector } from './_components/IBAccountSelector';
import { StatCard } from './_components/widgets/StatCard';
import { DepositBarChart } from './_components/widgets/DepositBarChart';
import { RebateChartWidget } from './_components/widgets/RebateChartWidget';
import { LatestDepositsWidget } from './_components/widgets/LatestDepositsWidget';
import { IBLinksWidget } from './_components/widgets/IBLinksWidget';
import { NewCustomerListWidget } from './_components/widgets/NewCustomerListWidget';
import { TradingLotsWidget } from './_components/widgets/TradingLotsWidget';
import type { IBReportValue } from '@/types/ib';

function ReportValueDisplay({ values, sign = '' }: { values: IBReportValue[]; sign?: '+' | '-' | '' }) {
  if (!values || values.length === 0) {
    return <BalanceShow balance={0} currencyId={840} sign={sign} />;
  }
  return (
    <>
      {values.map((v, i) => (
        <span key={i}>
          {i > 0 && ' / '}
          <BalanceShow balance={v.amount } currencyId={v.currencyId} sign={sign} />
        </span>
      ))}
    </>
  );
}

export default function IBDashboardPage() {
  const t = useTranslations('ib.dashboard');
  const pathname = usePathname();
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const requestIdRef = useRef(0);
  const pathnameRef = useRef(pathname);

  const [todayRebate, setTodayRebate] = useState<IBReportValue[]>([]);
  const [totalRebate, setTotalRebate] = useState<IBReportValue[]>([]);
  const [todayVolume, setTodayVolume] = useState<number>(0);
  const [todayNewCustomers, setTodayNewCustomers] = useState<number>(0);
  const [todayDeposit, setTodayDeposit] = useState<IBReportValue[]>([]);
  const [loadedUid, setLoadedUid] = useState<number | null>(null);

  const isLoading = !agentAccount || agentAccount.uid !== loadedUid;

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
      const tz = -(new Date().getTimezoneOffset() / 60);

      const [rebateToday, rebateTotal, volume, newCustomers, depositToday] =
        await Promise.all([
          execute(getIBRebateTodayValue, agentAccount.uid, tz),
          execute(getIBRebateTotalValue, agentAccount.uid),
          execute(getIBTradeTodayVolume, agentAccount.uid, tz),
          execute(getIBTodayAccountCreation, agentAccount.uid),
          execute(getIBDepositTodayValue, agentAccount.uid),
        ]);
      if (isStaleRequest()) return;

      if (rebateToday.success) setTodayRebate(rebateToday.data || []);
      if (rebateTotal.success) setTotalRebate(rebateTotal.data || []);
      if (volume.success) setTodayVolume(Number(volume.data) || 0);
      if (newCustomers.success) setTodayNewCustomers(Number(newCustomers.data) || 0);
      if (depositToday.success) setTodayDeposit(depositToday.data || []);

      setLoadedUid(agentAccount.uid);
    };

    load();
    return () => {
      cancelled = true;
    };
  }, [agentAccount, execute, pathname]);

  return (
    <div className="flex w-full flex-col gap-5">
      {/* ===== Row 1: Account Selector + Trade Volume + Today's Deposit ===== */}
      <div className="grid grid-cols-1 gap-5 md:grid-cols-3">
        <IBAccountSelector />
        <StatCard
          title={t('todayTradeVolume')}
          value={isLoading ? '' : (typeof todayVolume === 'number' ? todayVolume : Number(todayVolume) || 0)}
          loading={isLoading}
          iconSrc="/images/icons/ib/jiaoyi-stat.svg"
          centered
        >
          <div className="trade-area-chart" />
        </StatCard>

        <StatCard
          title={t('todayDeposit')}
          value={isLoading ? '' : <ReportValueDisplay values={todayDeposit} />}
          loading={isLoading}
          iconSrc="/images/icons/ib/zijin.svg"
          centered
        >
          <DepositBarChart />
        </StatCard>
      </div>

      {/* ===== Row 2: New Customers + Rebate (with charts) ===== */}
      <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
        <StatCard
          title={t('todayNewCustomers')}
          value={isLoading ? '' : `+${todayNewCustomers}`}
          loading={isLoading}
          iconSrc="/images/icons/ib/xinkehu-stat.svg"
          centered
          viewMoreHref="/ib/new-customers"
          viewMoreLabel={t('viewMore')}
        >
          <DepositBarChart />
        </StatCard>

        <StatCard
          title={t('rebate')}
          value={isLoading ? '' : <ReportValueDisplay values={todayRebate} sign="+" />}
          loading={isLoading}
          iconSrc="/images/icons/ib/fanyong-stat.svg"
          centered
          extra={
            <span className="text-xs text-text-secondary">
              {t('all')} ≡
            </span>
          }
        >
          <div className="trade-area-chart" />
        </StatCard>
      </div>

      {/* ===== Row 3: Main content + Sidebar ===== */}
      <div className="grid grid-cols-1 gap-5 lg:grid-cols-12">
        <div className="flex flex-col gap-5 lg:col-span-8">
          <RebateChartWidget totalRebate={totalRebate} totalLoading={isLoading} />
          <LatestDepositsWidget />
        </div>

        <div className="flex flex-col gap-5 lg:col-span-4">
          <NewCustomerListWidget />
          <TradingLotsWidget />
          <IBLinksWidget />
        </div>
      </div>
    </div>
  );
}
