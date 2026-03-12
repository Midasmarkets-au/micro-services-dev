'use client';

import { useState, useCallback, useMemo, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, BalanceShow, DatePicker, DataTable, Icon } from '@/components/ui';
import type { DateRange, DataTableColumn } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { getSalesChildStat, getSalesRebateStatBySymbol } from '@/actions';
import type { SalesClientAccount, SalesChildStat } from '@/types/sales';
import { AccountRoleTypes, getCurrencySymbol } from '@/types/accounts';

interface RebateSymbolRow {
  symbol: string;
  currencyId: number;
  volume: number;
  amount: number;
}

interface RebateTotal {
  volume: number;
  amount: number;
  currencyId: number;
}

interface ViewRebateStatModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}


export function ViewRebateStatModal({ open, onOpenChange, account }: ViewRebateStatModalProps) {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [isLoading, setIsLoading] = useState(false);
  const [stats, setStats] = useState<SalesChildStat | null>(null);
  const [rebateList, setRebateList] = useState<RebateSymbolRow[]>([]);
  const [rebateTotal, setRebateTotal] = useState<RebateTotal | null>(null);
  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined);

  const fetchData = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!salesAccount) return;
    setIsLoading(true);
    try {
      const params: Record<string, unknown> = { uid };
      if (from) params.from = from;
      if (to) params.to = to;

      const isIbOrSales = account?.role === AccountRoleTypes.IB || account?.role === AccountRoleTypes.Sales;

      const [statResult, rebateResult] = await Promise.all([
        execute(getSalesChildStat, salesAccount.uid, params),
        isIbOrSales
          ? execute(getSalesRebateStatBySymbol, salesAccount.uid, params)
          : Promise.resolve({ success: false, data: null }),
      ]);

      if (statResult.success && statResult.data) {
        setStats(statResult.data);
      }

      if (rebateResult.success && rebateResult.data) {
        const raw = rebateResult.data as Record<string, { amounts: Record<string, number[]>; volume?: number }>;
        const entries = Object.entries(raw);
        if (entries.length > 0) {
          const rows: RebateSymbolRow[] = [];
          let totalVolume = 0;
          let totalAmount = 0;
          let firstCurrencyId = 0;

          for (const [symbol, symbolData] of entries) {
            const amountEntries = Object.entries(symbolData.amounts || {});
            for (const [currencyIdStr, amountArr] of amountEntries) {
              const currencyId = Number(currencyIdStr);
              const amount = Array.isArray(amountArr) ? amountArr[0] ?? 0 : Number(amountArr) || 0;
              const volume = (symbolData.volume || 0) / 100;
              rows.push({ symbol, currencyId, volume, amount });
              totalVolume += volume;
              totalAmount += amount;
              if (!firstCurrencyId) firstCurrencyId = currencyId;
            }
          }

          setRebateList(rows);
          setRebateTotal({ volume: totalVolume, amount: totalAmount, currencyId: firstCurrencyId });
        } else {
          setRebateList([]);
          setRebateTotal(null);
        }
      }
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute]);

  useEffect(() => {
    if (open && account) {
      setStats(null);
      setRebateList([]);
      setRebateTotal(null);
      setDateRange(undefined);
      fetchData(account.uid);
    }
  }, [open, account, fetchData]);

  const handleSearch = () => {
    if (!account) return;
    const from = dateRange?.from ? dateRange.from.toISOString() : undefined;
    const to = dateRange?.to ? dateRange.to.toISOString() : undefined;
    fetchData(account.uid, from, to);
  };

  const handleClear = () => {
    if (!account) return;
    setDateRange(undefined);
    fetchData(account.uid);
  };

  const title = account?.user?.displayName || account?.user?.nativeName || '';

  const columns = useMemo<DataTableColumn<RebateSymbolRow>[]>(() => [
    {
      key: 'symbol',
      title: t('fields.symbol'),
      skeletonWidth: 'w-20',
      render: (row) => row.symbol,
    },
    {
      key: 'currency',
      title: t('fields.currency'),
      skeletonWidth: 'w-16',
      render: (row) => getCurrencySymbol(row.currencyId),
    },
    {
      key: 'volume',
      title: t('fields.volume'),
      skeletonWidth: 'w-16',
      render: (row) => row.volume.toFixed(2),
    },
    {
      key: 'amount',
      title: t('fields.amount'),
      skeletonWidth: 'w-24',
      render: (row) => (
        <BalanceShow
          balance={row.amount}
          currencyId={row.currencyId}
          className={row.amount <= 0 ? 'error-text' : ''}
        />
      ),
    },
  ], [t]);

  const footerRow = rebateTotal ? (
    <tr className="border-t-2 border-border font-bold text-(--color-success)">
      <td className="px-5 py-4 uppercase">{t('dashboard.rebateAmount')}</td>
      <td className="px-5 py-4">{getCurrencySymbol(rebateTotal.currencyId)}</td>
      <td className="px-5 py-4">{rebateTotal.volume.toFixed(2)}</td>
      <td className="px-5 py-4">
        <BalanceShow
          balance={rebateTotal.amount}
          currencyId={rebateTotal.currencyId}
          className={rebateTotal.amount <= 0 ? 'error-text' : ''}
        />
      </td>
    </tr>
  ) : null;

  const renderAmountTags = () => {
    if (!stats) return null;
    const tags: React.ReactNode[] = [];

    if (stats.depositAmounts) {
      for (const [currencyId, amounts] of Object.entries(stats.depositAmounts)) {
        const val = Array.isArray(amounts) ? amounts[0] ?? 0 : Number(amounts) || 0;
        tags.push(
          <span
            key={`dep-${currencyId}`}
            className="inline-flex items-center gap-1 rounded bg-(--color-tag-deposit-bg) px-3 py-1 text-xs text-(--color-tag-deposit)"
          >
            <span>{t('menu.deposit')}:</span>
            <BalanceShow balance={val} currencyId={Number(currencyId)} />
          </span>
        );
      }
    }
    if (stats.withdrawalAmounts) {
      for (const [currencyId, amounts] of Object.entries(stats.withdrawalAmounts)) {
        const val = Array.isArray(amounts) ? amounts[0] ?? 0 : Number(amounts) || 0;
        tags.push(
          <span
            key={`wd-${currencyId}`}
            className="inline-flex items-center gap-1 rounded bg-(--color-tag-withdrawal-bg) px-3 py-1 text-xs text-(--color-tag-withdrawal)"
          >
            <span>{t('menu.withdrawal')}:</span>
            <BalanceShow balance={val} currencyId={Number(currencyId)} />
          </span>
        );
      }
    }
    if (stats.rebateAmounts) {
      for (const [currencyId, amounts] of Object.entries(stats.rebateAmounts)) {
        const val = Array.isArray(amounts) ? amounts[0] ?? 0 : Number(amounts) || 0;
        tags.push(
          <span
            key={`rb-${currencyId}`}
            className="inline-flex items-center gap-1 rounded bg-(--color-tag-rebate-bg) px-3 py-1 text-xs text-(--color-tag-rebate)"
          >
            <span>{t('menu.rebate')}:</span>
            <BalanceShow balance={val} currencyId={Number(currencyId)} />
          </span>
        );
      }
    }
    return tags.length > 0 ? <div className="flex flex-wrap gap-5">{tags}</div> : null;
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="flex flex-col gap-10">
        {/* 内容主体 */}
        <div className="flex flex-col flex-wrap gap-10">
          {/* 头部行：标题+日期选择器 | 清除+搜索 */}
          <DialogHeader className="flex-col items-stretch gap-3 space-y-0 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex flex-col items-stretch gap-3 sm:flex-row sm:items-center sm:gap-5">
              <DialogTitle className="shrink-0">{title}</DialogTitle>
              <DatePicker
                mode="range"
                size="sm"
                value={dateRange}
                className="w-full sm:w-auto"
                onChange={(val) => setDateRange(val as DateRange | undefined)}
              />
            </div>
            <div className="flex w-full items-center gap-3 sm:w-auto sm:gap-5">
              <Button
                size="sm"
                className="flex-1 bg-(--color-btn-dark) text-white hover:bg-(--color-btn-dark)/80 whitespace-nowrap sm:flex-none"
                onClick={handleClear}
              >
                <Icon name="reset-line" />
                {t('action.clear')}
              </Button>
              <Button
                variant="primary"
                size="sm"
                className="flex-1 whitespace-nowrap sm:flex-none"
                onClick={handleSearch}
              >
                <Icon name="search-line" />
                {t('action.search')}
              </Button>
            </div>
          </DialogHeader>

          {/* 统计标签 */}
          {renderAmountTags()}

          {/* 数据表格 */}
          <div className="max-h-[50vh] overflow-auto">
            <DataTable<RebateSymbolRow>
              columns={columns}
              data={rebateList}
              rowKey={(_, idx) => idx}
              loading={isLoading}
              skeletonRows={5}
              footer={footerRow}
            />
          </div>
        </div>

        {/* 底部关闭按钮 */}
        <div className="flex justify-end">
          <Button
            variant="secondary"
            size="sm"
            className="w-[120px] border border-border"
            onClick={() => onOpenChange(false)}
          >
            {t('action.close')}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
