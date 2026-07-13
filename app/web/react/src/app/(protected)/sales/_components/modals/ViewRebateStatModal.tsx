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
import { fetchAction } from '@/lib/api/browser-client';
import type { SalesClientAccount, SalesChildStat } from '@/types/sales';
import { AccountRoleTypes } from '@/types/accounts';
import { CurrencyCodeMap } from '@/components/ui/BalanceShow';
import { convertTradeTime, cn } from '@/lib/utils';

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

// 转入类（入金/转入）复用存款配色，转出类（出金/转出）复用取款配色，跟原来的 tag 保持一致。
const AMOUNT_CATEGORY_STYLE: Record<AmountCategory, string> = {
  deposit: 'bg-(--color-tag-deposit-bg) text-(--color-tag-deposit)',
  accountTransferIn: 'bg-(--color-tag-deposit-bg) text-(--color-tag-deposit)',
  walletTransferIn: 'bg-(--color-tag-deposit-bg) text-(--color-tag-deposit)',
  rebate: 'bg-(--color-tag-rebate-bg) text-(--color-tag-rebate)',
  withdrawal: 'bg-(--color-tag-withdrawal-bg) text-(--color-tag-withdrawal)',
  accountTransferOut: 'bg-(--color-tag-withdrawal-bg) text-(--color-tag-withdrawal)',
  walletDetails: 'bg-(--color-tag-rebate-bg) text-(--color-tag-rebate)',
};

const CATEGORY_USD_TOTAL_FIELD: Partial<Record<AmountCategory, keyof SalesChildStat>> = {
  deposit: 'totalDepositAmountUsd',
  accountTransferIn: 'totalAccountTransferInAmountUsd',
  walletTransferIn: 'totalWalletTransferInAmountUsd',
  withdrawal: 'totalWithdrawalAmountUsd',
  accountTransferOut: 'totalAccountTransferOutAmountUsd',
};

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
        execute(() => fetchAction<SalesChildStat>('getSalesChildStat', salesAccount.uid, params)),
        isIb
          ? execute(() => fetchAction<Record<string, { amounts: Record<string, number>; volume: number }>>(
              'getSalesIbRebateStatBySymbol', salesAccount.uid, params))
          : isSales
            ? execute(() => fetchAction<Record<string, { symbol?: string; currencyId?: number; volume?: number; profit?: number }>>(
                'getSalesRebateStatBySymbol', salesAccount.uid, params))
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
    // 顺序即分组展示顺序：先「转入」类，再「转出」类。
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
      <DialogContent className="flex flex-col gap-10">
        {/* 内容主体 */}
        <div className="flex flex-col gap-4">
          {/* 标题 */}
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
          </DialogHeader>

          {/* 筛选栏：移动端垂直堆叠，桌面端水平排列 */}
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <DatePicker
              mode="range"
              size="sm"
              value={dateRange}
              className="w-full sm:flex-1"
              onChange={(val) => setDateRange(val as DateRange | undefined)}
            />
            <div className="flex gap-3">
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
          </div>

          {/* 统计标签 */}
          {amountSummaryRows.length > 0 && (
            <div className="border-t border-border pt-4">
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
                        'inline-flex items-center rounded px-2 py-1 text-xs',
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
