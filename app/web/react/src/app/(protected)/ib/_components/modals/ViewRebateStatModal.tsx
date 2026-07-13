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
import { CurrencyCodeMap } from '@/components/ui/BalanceShow';
import type { DateRange, DataTableColumn } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useIBStore } from '@/stores/ibStore';
import { fetchAction } from '@/lib/api/browser-client';
import { cn } from '@/lib/utils';
import type { IBChildStat, IBClientAccount } from '@/types/ib';

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

type AmountCategory =
  | 'deposit'
  | 'accountTransferIn'
  | 'walletTransferIn'
  | 'rebate'
  | 'withdrawal'
  | 'accountTransferOut'
  | 'walletDetails';

type WalletDetailKey = 'refund' | 'rebate' | 'rewards' | 'adjust';

interface AmountSummaryRow {
  category: AmountCategory;
  currencyId: number;
  amount: number;
  detailLabel?: string;
  isUsdSummary?: boolean;
}

const WALLET_DETAIL_ORDER: WalletDetailKey[] = ['refund', 'rebate', 'rewards', 'adjust'];

const CATEGORY_USD_TOTAL_FIELD: Partial<Record<AmountCategory, keyof IBChildStat>> = {
  deposit: 'totalDepositAmountUsd',
  accountTransferIn: 'totalAccountTransferInAmountUsd',
  walletTransferIn: 'totalWalletTransferInAmountUsd',
  withdrawal: 'totalWithdrawalAmountUsd',
  accountTransferOut: 'totalAccountTransferOutAmountUsd',
};

// 转入类（入金/转入）复用蓝色，转出类（出金/转出）复用黄色，返佣单独用绿色，跟原来的 tag 配色保持一致。
const AMOUNT_CATEGORY_STYLE: Record<AmountCategory, string> = {
  deposit: 'bg-primary',
  accountTransferIn: 'bg-primary',
  walletTransferIn: 'bg-primary',
  rebate: 'bg-green-600',
  withdrawal: 'bg-[#E6A700]',
  accountTransferOut: 'bg-[#E6A700]',
  walletDetails: 'bg-green-600',
};

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
        execute(() => fetchAction<IBChildStat>('getIBChildStat', agentAccount.uid, params)),
        execute(() => fetchAction<Record<string, { amounts: Record<string, number[]> }>>(
          'getIBRebateStatBySymbol', agentAccount.uid, params)),
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

  // 受控打开时 Radix 不会触发 onOpenChange，改用 effect 监听 open/account 加载数据
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
          className={row.amount <= 0 ? 'text-red-500' : ''}
        />
      ),
    },
  ], [t]);

  const footerRow = rebateTotal ? (
    <tr className="border-t-2 border-border font-bold text-green-600">
      <td className="px-5 py-4 uppercase">{t('trade.total')}</td>
      <td className="px-5 py-4">{CurrencyCodeMap[rebateTotal.currencyId] || 'USD'}</td>
      <td className="px-5 py-4 text-right">{rebateTotal.volume.toFixed(2)}</td>
      <td className="px-5 py-4 text-right">
        <BalanceShow
          balance={rebateTotal.amount}
          currencyId={rebateTotal.currencyId}
          className={rebateTotal.amount <= 0 ? 'text-red-500' : ''}
        />
      </td>
    </tr>
  ) : null;

  const amountSummaryRows = useMemo<AmountSummaryRow[]>(() => {
    if (!stats) return [];
    const rows: AmountSummaryRow[] = [];
    const pushCategoryRows = (
      category: AmountCategory,
      amounts?: Record<string, number[] | number>,
    ) => {
      if (!amounts || Object.keys(amounts).length === 0) return;
      for (const [currencyId, amountVal] of Object.entries(amounts)) {
        const amount = Array.isArray(amountVal) ? amountVal[0] ?? 0 : Number(amountVal) || 0;
        rows.push({ category, currencyId: Number(currencyId), amount });
      }
      const usdField = CATEGORY_USD_TOTAL_FIELD[category];
      if (usdField) {
        const usdTotal = stats[usdField];
        if (typeof usdTotal === 'number') {
          rows.push({ category, currencyId: 840, amount: usdTotal, isUsdSummary: true });
        }
      }
    };
    // 顺序即分组展示顺序：先「转入」类，再「转出」类，最后「钱包明细」。
    pushCategoryRows('deposit', stats.depositAmounts);
    pushCategoryRows('accountTransferIn', stats.accountTransferInAmounts);
    pushCategoryRows('walletTransferIn', stats.walletTransferInAmounts);
    pushCategoryRows('rebate', stats.rebateAmounts);
    pushCategoryRows('withdrawal', stats.withdrawalAmounts);
    pushCategoryRows('accountTransferOut', stats.accountTransferOutAmounts);

    const walletDetailLabel: Record<WalletDetailKey, string> = {
      refund: t('menu.walletDetailRefund'),
      rebate: t('menu.walletDetailRebate'),
      rewards: t('menu.walletDetailRewards'),
      adjust: t('menu.walletDetailAdjust'),
    };
    // 钱包明细：各子类只展示币种明细，不单独出 Total；最后汇总全部 totalUsd。
    let walletDetailsUsdTotal = 0;
    let hasWalletDetailRows = false;
    for (const key of WALLET_DETAIL_ORDER) {
      const detail = stats.walletDetails?.[key];
      if (!detail?.amounts || Object.keys(detail.amounts).length === 0) continue;
      hasWalletDetailRows = true;
      for (const [currencyId, amountVal] of Object.entries(detail.amounts)) {
        rows.push({
          category: 'walletDetails',
          detailLabel: walletDetailLabel[key],
          currencyId: Number(currencyId),
          amount: Number(amountVal) || 0,
        });
      }
      if (typeof detail.totalUsd === 'number') {
        walletDetailsUsdTotal += detail.totalUsd;
      }
    }
    if (hasWalletDetailRows && walletDetailsUsdTotal !== 0) {
      rows.push({
        category: 'walletDetails',
        currencyId: 840,
        amount: walletDetailsUsdTotal,
        isUsdSummary: true,
      });
    }
    return rows;
  }, [stats, t]);

  const amountCategoryLabel: Record<AmountCategory, string> = {
    deposit: t('menu.deposit'),
    accountTransferIn: t('menu.accountTransferIn'),
    walletTransferIn: t('menu.walletTransferIn'),
    rebate: t('menu.rebate'),
    withdrawal: t('menu.withdrawal'),
    accountTransferOut: t('menu.accountTransferOut'),
    walletDetails: t('menu.walletDetails'),
  };

  const amountSummaryColumns = useMemo<DataTableColumn<AmountSummaryRow>[]>(() => [
    {
      key: 'detail',
      title: t('fields.detail'),
      skeletonWidth: 'w-20',
      render: (row) => (
        row.detailLabel
          ? <span className="text-sm text-text-primary">{row.detailLabel}</span>
          : <span className="text-sm text-text-secondary">-</span>
      ),
    },
    {
      key: 'currency',
      title: t('fields.currency'),
      skeletonWidth: 'w-16',
      render: (row) => (
        row.isUsdSummary
          ? <span className="text-sm font-medium text-text-secondary">USD {t('trade.total')}</span>
          : (CurrencyCodeMap[row.currencyId] || 'USD')
      ),
    },
    {
      key: 'amount',
      title: t('fields.amount'),
      skeletonWidth: 'w-24',
      align: 'right',
      render: (row) => (
        <BalanceShow
          balance={row.amount}
          currencyId={row.isUsdSummary ? 840 : row.currencyId}
          className={row.isUsdSummary ? 'text-sm font-semibold text-text-primary' : undefined}
        />
      ),
    },
  ], [t]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[900px]">
        <DialogHeader className="flex-col items-stretch gap-3 space-y-0 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-col items-stretch gap-3 mb-4 sm:flex-row sm:items-center sm:gap-5">
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
              className="flex-1 whitespace-nowrap bg-(--color-btn-dark) text-white hover:bg-(--color-btn-dark)/80 sm:flex-none"
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

        <div className="max-h-[60vh] overflow-auto border-t border-border pt-4">
          {amountSummaryRows.length > 0 && (
            <div className="mb-5">
              <DataTable<AmountSummaryRow>
                columns={amountSummaryColumns}
                data={amountSummaryRows}
                rowKey={(row, idx) => `${row.category}-${row.detailLabel ?? 'none'}-${row.isUsdSummary ? 'usd-total' : row.currencyId}-${idx}`}
                groupConfig={{
                  groupBy: (row) => row.category,
                  headerWidth: 'w-32',
                  headerTitle: t('fields.category'),
                  renderGroupHeader: (key) => (
                    <span
                      className={cn(
                        'inline-flex items-center rounded px-2 py-1 text-xs text-white',
                        AMOUNT_CATEGORY_STYLE[key as AmountCategory]
                      )}
                    >
                      {amountCategoryLabel[key as AmountCategory]}
                    </span>
                  ),
                }}
              />
            </div>
          )}

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
