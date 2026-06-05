'use client';

import { useState, useCallback, useMemo, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import moment from 'moment';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, BalanceShow, DatePicker, DataTable, Icon } from '@/components/ui';
import type { DateRange, DataTableColumn } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { getSalesChildStat, getSalesRebateStatBySymbol, getSalesIbRebateStatBySymbol } from '@/actions';
import type { SalesClientAccount, SalesChildStat } from '@/types/sales';
import { AccountRoleTypes } from '@/types/accounts';
import { CurrencyCodeMap } from '@/components/ui/BalanceShow';
import { convertTradeTime } from '@/lib/utils';

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
  const [rebateTotals, setRebateTotals] = useState<RebateTotal[]>([]);
  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined);

  const processIbRebate = useCallback((raw: Record<string, { amounts: Record<string, number>; volume: number }>) => {
    const entries = Object.entries(raw);
    if (entries.length === 0) { setRebateList([]); setRebateTotals([]); return; }

    const firstAmounts = entries[0][1].amounts || {};
    const currency = Number(Object.keys(firstAmounts)[0]) || 0;

    const rows: RebateSymbolRow[] = [];
    let totalVolume = 0;
    let totalAmount = 0;

    for (const [symbol, symbolData] of entries) {
      const amountEntries = Object.entries(symbolData.amounts || {});
      for (const [currencyIdStr, amount] of amountEntries) {
        const currencyId = Number(currencyIdStr);
        const vol = (symbolData.volume || 0) / 100;
        const amt = Number(amount) || 0;
        rows.push({ symbol, currencyId, volume: vol, amount: amt });
        totalVolume += vol;
        totalAmount += amt;
      }
    }

    setRebateList(rows);
    setRebateTotals([{ volume: totalVolume, amount: totalAmount, currencyId: currency }]);
  }, []);

  const processSalesRebate = useCallback((raw: Record<string, { symbol?: string; currencyId?: number; volume?: number; profit?: number; amounts?: Record<string, number[]> }>) => {
    const entries = Object.values(raw);
    if (entries.length === 0) { setRebateList([]); setRebateTotals([]); return; }

    const rows: RebateSymbolRow[] = [];
    const totalByCurrency: Record<number, RebateTotal> = {};

    for (const item of entries) {
      const currencyId = item.currencyId ?? 0;
      const volume = (item.volume || 0) / 100;
      const amount = item.profit ?? 0;
      const symbol = item.symbol || '';

      rows.push({ symbol, currencyId, volume, amount });

      if (!totalByCurrency[currencyId]) {
        totalByCurrency[currencyId] = { volume: 0, amount: 0, currencyId };
      }
      totalByCurrency[currencyId].volume += volume;
      totalByCurrency[currencyId].amount += amount;
    }

    setRebateList(rows);
    setRebateTotals(Object.values(totalByCurrency));
  }, []);

  const fetchData = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!salesAccount) return;
    setIsLoading(true);
    try {
      const params: Record<string, unknown> = { uid };
      if (from) params.from = from;
      if (to) params.to = to;

      const isIb = account?.role === AccountRoleTypes.IB;
      const isSales = account?.role === AccountRoleTypes.Sales;

      const [statResult, rebateResult] = await Promise.all([
        execute(getSalesChildStat, salesAccount.uid, params),
        isIb
          ? execute(getSalesIbRebateStatBySymbol, salesAccount.uid, params)
          : isSales
            ? execute(getSalesRebateStatBySymbol, salesAccount.uid, params)
            : Promise.resolve({ success: false, data: null }),
      ]);

      if (statResult.success && statResult.data) {
        setStats(statResult.data);
      }

      if (rebateResult.success && rebateResult.data) {
        if (isIb) {
          processIbRebate(rebateResult.data as Record<string, { amounts: Record<string, number>; volume: number }>);
        } else if (isSales) {
          processSalesRebate(rebateResult.data as Record<string, { symbol?: string; currencyId?: number; volume?: number; profit?: number }>);
        }
      } else {
        setRebateList([]);
        setRebateTotals([]);
      }
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute, processIbRebate, processSalesRebate]);

  useEffect(() => {
    if (open && account) {
      setStats(null);
      setRebateList([]);
      setRebateTotals([]);
      setDateRange(undefined);
      fetchData(account.uid);
    }
  }, [open, account, fetchData]);

  const handleSearch = () => {
    if (!account) return;
    const fromRaw = dateRange?.from ? moment(dateRange.from).format('YYYY-MM-DD') : null;
    const toRaw = dateRange?.to ? moment(dateRange.to).format('YYYY-MM-DD') : null;
    const [from, to] = convertTradeTime(fromRaw, toRaw);
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
      render: (row) => CurrencyCodeMap[row.currencyId] || 'USD',
    },
    {
      key: 'volume',
      title: t('fields.volume'),
      skeletonWidth: 'w-16',
      align: 'right',
      render: (row) => row.volume.toFixed(2),
    },
    {
      key: 'amount',
      title: t('fields.amount'),
      skeletonWidth: 'w-24',
      align: 'right',
      render: (row) => (
        <BalanceShow
          balance={row.amount}
          currencyId={row.currencyId}
          className={row.amount <= 0 ? 'error-text' : ''}
        />
      ),
    },
  ], [t]);

  const footerRow = rebateTotals.length > 0 ? (
    <>
      {rebateTotals.map((total) => (
        <tr key={total.currencyId} className="border-t-2 border-border font-bold text-(--color-success)">
          <td className="px-5 py-4 uppercase">{t('trade.total')}</td>
          <td className="px-5 py-4">{CurrencyCodeMap[total.currencyId] || 'USD'}</td>
          <td className="px-5 py-4 text-right">{total.volume.toFixed(2)}</td>
          <td className="px-5 py-4 text-right">
            <BalanceShow
              balance={total.amount}
              currencyId={total.currencyId}
              className={total.amount <= 0 ? 'error-text' : ''}
            />
          </td>
        </tr>
      ))}
    </>
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
          <div className="max-h-[50vh] overflow-x-auto overflow-y-auto">
            <DataTable<RebateSymbolRow>
              columns={columns}
              data={rebateList}
              rowKey={(_, idx) => idx}
              loading={isLoading}
              skeletonRows={5}
              footer={footerRow}
              stickyHeader
            />
          </div>
        </div>

        {/* 底部关闭按钮 */}
        <DialogFooter>
          <div className="flex justify-end">
            <Button
              variant="outline"
              size="sm"
              className="w-[120px] border border-border"
              onClick={() => onOpenChange(false)}
            >
              {t('action.close')}
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
