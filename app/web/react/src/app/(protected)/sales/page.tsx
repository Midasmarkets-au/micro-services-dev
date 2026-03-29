'use client';

// import { useState, useEffect } from 'react';
// import Image from 'next/image';
// import { useTranslations } from 'next-intl';
// import { useServerAction } from '@/hooks/useServerAction';
// import { getSalesAccountStat } from '@/actions';
// import { useSalesStore } from '@/stores/salesStore';
// import { BalanceShow } from '@/components/ui';
// import { SalesAccountSelector } from './_components/SalesAccountSelector';
// import { SalesLatestDepositsWidget } from './_components/widgets/SalesLatestDepositsWidget';
// import { SalesNewCustomerWidget } from './_components/widgets/SalesNewCustomerWidget';
// import { SalesTradingLotsWidget } from './_components/widgets/SalesTradingLotsWidget';
// import { SalesLinksWidget } from './_components/widgets/SalesLinksWidget';
// import type { SalesAccountStat } from '@/types/sales';
// import { DepositBarChart } from '@/app/(protected)/ib/_components/widgets/DepositBarChart';
import { redirect } from 'next/navigation';
function TodayStatCard({
  title,
  iconSrc,
  value,
  loading,
  children,
}: {
  title: string;
  iconSrc: string;
  value: React.ReactNode;
  loading: boolean;
  children?: React.ReactNode;
}) {
  return (
    <div className="flex flex-col overflow-hidden rounded-xl border border-border bg-surface">
      <div className="flex flex-1 flex-col items-center p-5">
        <div className="flex items-center gap-2">
          <Image src={iconSrc} alt="" width={28} height={28} className="shrink-0 dark:brightness-0 dark:invert" />
          <span className="text-xl font-semibold text-text-primary">{title}</span>
        </div>
        <div className="mt-3">
          {loading ? (
            <div className="mx-auto h-10 w-32 animate-pulse rounded bg-border" />
          ) : (
            <div className="text-responsive-3xl font-bold text-primary">{value}</div>
          )}
        </div>
      </div>
      {children && (
        <div className="flex w-full items-end justify-center">
          {children}
        </div>
      )}
    </div>
  );
}

export default function SalesDashboardPage() {
  redirect('/sales/customers');
  // const t = useTranslations('sales.dashboard');
  // const { execute } = useServerAction({ showErrorToast: true });
  // const salesAccount = useSalesStore((s) => s.salesAccount);
  // const [stat, setStat] = useState<SalesAccountStat | null>(null);
  // const [loadedUid, setLoadedUid] = useState<number | null>(null);

  // const isLoading = !salesAccount || salesAccount.uid !== loadedUid;

  // useEffect(() => {
  //   if (!salesAccount) return;
  //   let cancelled = false;

  //   const load = async () => {
  //     const now = new Date();
  //     const fmt = (d: Date) => d.toISOString().split('T')[0];
  //     const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  //     const range = { from: fmt(todayStart), to: fmt(now) };

  //     const result = await execute(getSalesAccountStat, salesAccount.uid, salesAccount.uid, range);
  //     if (cancelled) return;
  //     if (result.success && result.data) {
  //       setStat(result.data);
  //     }
  //     setLoadedUid(salesAccount.uid);
  //   };

  //   load();
  //   return () => { cancelled = true; };
  // }, [salesAccount, execute]);

  // const fmtDeposit = (v?: number) => {
  //   if (v === undefined || v === null) return <BalanceShow balance={0} currencyId={840} sign="+" />;
  //   return <BalanceShow balance={v} currencyId={840} sign="+" />;
  // };

  // return (
  //   <div className="flex w-full flex-col gap-5">
  //     {/* Row 1: 用户卡片 + 今日交易量 + 今日资金 */}
  //     <div className="grid grid-cols-1 gap-5 md:grid-cols-4">
  //       <SalesAccountSelector />
  //       <TodayStatCard
  //         title={t('todayTradeVolume')}
  //         iconSrc="/images/icons/ib/jiaoyi-stat.svg"
  //         value={isLoading ? '' : (stat?.tradeCount ?? 0)}
  //         loading={isLoading}
  //       > 
  //         <div className="trade-area-chart" />
  //       </TodayStatCard>

  //       <TodayStatCard
  //         title={t('todayFunds')}
  //         iconSrc="/images/icons/ib/zijin.svg"
  //         value={isLoading ? '' : fmtDeposit(stat?.depositAmount)}
  //         loading={isLoading}
  //       >
  //         <DepositBarChart />
  //       </TodayStatCard>
  //       <SalesNewCustomerWidget />
  //     </div>

  //     {/* Row 2: 主内容（资金表格）+ 右侧边栏 */}
  //     <div className="grid grid-cols-1 gap-5 lg:grid-cols-12">
  //       <div className="flex flex-col gap-5 lg:col-span-8">
  //         <SalesLatestDepositsWidget />
  //       </div>

  //       <div className="flex flex-col gap-5 lg:col-span-4">
          
  //         <SalesTradingLotsWidget />
  //         <SalesLinksWidget />
  //       </div>
  //     </div>
  //   </div>
  // );
}
