'use client';

import { useState, useCallback, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, BalanceShow, DatePicker, DataTable } from '@/components/ui';
import type { DateRange, DataTableColumn } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useIBStore } from '@/stores/ibStore';
import { getIBChildStat, getIBRebateStatBySymbol } from '@/actions';
import type { IBChildStat, IBClientAccount } from '@/types/ib';
import { getCurrencySymbol } from '@/types/accounts';

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
  account: IBClientAccount | null;
}

export function ViewRebateStatModal({ open, onOpenChange, account }: ViewRebateStatModalProps) {
  const t = useTranslations('ib');
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);

  const [isLoading, setIsLoading] = useState(false);
  const [stats, setStats] = useState<IBChildStat | null>(null);
  const [rebateList, setRebateList] = useState<RebateSymbolRow[]>([]);
  const [rebateTotal, setRebateTotal] = useState<RebateTotal | null>(null);
  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined);

  const fetchData = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!agentAccount) return;
    setIsLoading(true);
    try {
      const params: Record<string, unknown> = { uid };
      if (from) params.from = from;
      if (to) params.to = to;

      const [statResult, rebateResult] = await Promise.all([
        execute(getIBChildStat, agentAccount.uid, params),
        execute(getIBRebateStatBySymbol, agentAccount.uid, params),
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
  }, [agentAccount, execute]);

  const handleOpenChange = useCallback((nextOpen: boolean) => {
    if (nextOpen && account) {
      setStats(null);
      setRebateList([]);
      setRebateTotal(null);
      setDateRange(undefined);
      fetchData(account.uid);
    }
    onOpenChange(nextOpen);
  }, [account, fetchData, onOpenChange]);

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
          className={row.amount <= 0 ? 'text-red-500' : ''}
        />
      ),
    },
  ], [t]);

  const footerRow = rebateTotal ? (
    <tr className="border-t-2 border-border font-bold text-green-600">
      <td className="px-5 py-4 uppercase">{t('dashboard.totalRebate')}</td>
      <td className="px-5 py-4">{getCurrencySymbol(rebateTotal.currencyId)}</td>
      <td className="px-5 py-4">{rebateTotal.volume.toFixed(2)}</td>
      <td className="px-5 py-4">
        <BalanceShow
          balance={rebateTotal.amount}
          currencyId={rebateTotal.currencyId}
          className={rebateTotal.amount <= 0 ? 'text-red-500' : ''}
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
          <span key={`dep-${currencyId}`} className="inline-flex items-center gap-1 rounded bg-primary px-3 py-1 text-xs text-white">
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
          <span key={`wd-${currencyId}`} className="inline-flex items-center gap-1 rounded bg-[#E6A700] px-3 py-1 text-xs text-white">
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
          <span key={`rb-${currencyId}`} className="inline-flex items-center gap-1 rounded bg-green-600 px-3 py-1 text-xs text-white">
            <span>{t('menu.rebate')}:</span>
            <BalanceShow balance={val} currencyId={Number(currencyId)} />
          </span>
        );
      }
    }
    return tags.length > 0 ? <div className="mb-4 flex flex-wrap gap-2">{tags}</div> : null;
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="max-w-[900px]">
        <DialogHeader>
          <div className="flex flex-wrap items-center gap-4">
            <DialogTitle className="shrink-0">{title}</DialogTitle>
            <DatePicker
              mode="range"
              value={dateRange}
              onChange={(val) => setDateRange(val as DateRange | undefined)}
            />
            <Button variant="outline" size="sm" onClick={handleSearch}>
              {t('action.search')}
            </Button>
            <Button variant="ghost" size="sm" onClick={handleClear}>
              {t('action.clear')}
            </Button>
          </div>
        </DialogHeader>

        <div className="max-h-[60vh] overflow-auto">
          {renderAmountTags()}

          <DataTable<RebateSymbolRow>
            columns={columns}
            data={rebateList}
            rowKey={(_, idx) => idx}
            loading={isLoading}
            skeletonRows={5}
            footer={footerRow}
          />
        </div>
      </DialogContent>
    </Dialog>
  );
}
