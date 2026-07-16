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
import { CurrencyCodeMap } from '@/components/ui/BalanceShow';
import { convertTradeTime, cn } from '@/lib/utils';
import type { WalletDetailsStat } from '@/types/sales';

export interface RebateStatAccount {
  uid: number;
  user?: {
    displayName?: string;
    nativeName?: string;
  } | null;
}

/** Child stat shape shared by IB / Sales APIs */
export interface RebateChildStat {
  rebateAmounts?: Record<string, number[] | number>;
  depositAmounts?: Record<string, number[] | number>;
  netAmounts?: Record<string, number[] | number>;
  profitAmounts?: Record<string, number[] | number>;
  withdrawalAmounts?: Record<string, number[] | number>;
  walletTransferInAmounts?: Record<string, number[] | number>;
  accountTransferInAmounts?: Record<string, number[] | number>;
  accountTransferOutAmounts?: Record<string, number[] | number>;
  totalDepositAmountUsd?: number;
  totalAccountTransferInAmountUsd?: number;
  totalWalletTransferInAmountUsd?: number;
  totalWithdrawalAmountUsd?: number;
  totalAccountTransferOutAmountUsd?: number;
  walletDetails?: WalletDetailsStat;
}

export type RebateStatFormat = 'ib' | 'sales';

export interface FetchActionResult<T> {
  success: boolean;
  data?: T | null;
}

export interface ViewRebateStatModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: RebateStatAccount | null;
  /** i18n namespace, e.g. 'ib' | 'sales' */
  translationNamespace: string;
  fetchChildStat: (
    uid: number,
    from?: string,
    to?: string,
  ) => Promise<FetchActionResult<RebateChildStat>>;
  fetchRebateStat: (
    uid: number,
    from?: string,
    to?: string,
  ) => Promise<FetchActionResult<unknown>>;
  /** How to parse rebate-by-symbol API response. Default `'ib'`. */
  rebateStatFormat?: RebateStatFormat;
}

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

const AMOUNT_CATEGORY_STYLE: Record<AmountCategory, string> = {
  deposit: 'bg-(--color-tag-deposit-bg) text-(--color-tag-deposit)',
  accountTransferIn: 'bg-(--color-tag-deposit-bg) text-(--color-tag-deposit)',
  walletTransferIn: 'bg-(--color-tag-deposit-bg) text-(--color-tag-deposit)',
  rebate: 'bg-(--color-tag-rebate-bg) text-(--color-tag-rebate)',
  withdrawal: 'bg-(--color-tag-withdrawal-bg) text-(--color-tag-withdrawal)',
  accountTransferOut: 'bg-(--color-tag-withdrawal-bg) text-(--color-tag-withdrawal)',
  walletDetails: 'bg-(--color-tag-rebate-bg) text-(--color-tag-rebate)',
};

const CATEGORY_USD_TOTAL_FIELD: Partial<Record<AmountCategory, keyof RebateChildStat>> = {
  deposit: 'totalDepositAmountUsd',
  accountTransferIn: 'totalAccountTransferInAmountUsd',
  walletTransferIn: 'totalWalletTransferInAmountUsd',
  withdrawal: 'totalWithdrawalAmountUsd',
  accountTransferOut: 'totalAccountTransferOutAmountUsd',
};

interface RebateStatItem {
  symbol?: string;
  currencyId?: number;
  volume?: number;
  profit?: number;
  amounts?: Record<string, number[] | number>;
}

/** Normalize API payload to list items (new array format; object map still accepted). */
function normalizeRebateStatItems(raw: unknown): RebateStatItem[] {
  if (Array.isArray(raw)) return raw as RebateStatItem[];
  if (raw && typeof raw === 'object') {
    return Object.entries(raw as Record<string, RebateStatItem>).map(([key, value]) => ({
      ...value,
      symbol: value.symbol ?? key,
    }));
  }
  return [];
}

/** IB 原逻辑：amt 取自 amounts（兼容 number / number[]） */
function resolveIbAmount(amounts?: Record<string, number[] | number>): number {
  if (!amounts) return 0;
  let total = 0;
  for (const amountVal of Object.values(amounts)) {
    total += Array.isArray(amountVal) ? amountVal[0] ?? 0 : Number(amountVal) || 0;
  }
  return total;
}

/**
 * 新接口 data 为数组，每项已是一行。
 * amt 取值与原来保持一致：ib → amounts，sales → profit。
 * 合计时金额单位一致，不按币种拆分。
 */
function parseRebateStat(
  raw: unknown,
  format: RebateStatFormat,
): { rows: RebateSymbolRow[]; totals: RebateTotal[] } {
  const items = normalizeRebateStatItems(raw);
  if (items.length === 0) return { rows: [], totals: [] };

  const rows: RebateSymbolRow[] = [];
  let totalVolume = 0;
  let totalAmount = 0;

  for (const item of items) {
    const volume = (item.volume || 0) / 100;
    const currencyId = item.currencyId ?? 0;
    const amount = format === 'ib' ? resolveIbAmount(item.amounts) : (item.profit ?? 0);

    rows.push({
      symbol: item.symbol || '',
      currencyId,
      volume,
      amount,
    });

    totalVolume += volume;
    totalAmount += amount;
  }

  return {
    rows,
    // 合计统一按 USD（840）展示
    totals: [{ volume: totalVolume, amount: totalAmount, currencyId: 840 }],
  };
}

export function ViewRebateStatModal({
  open,
  onOpenChange,
  account,
  translationNamespace,
  fetchChildStat,
  fetchRebateStat,
  rebateStatFormat = 'ib',
}: ViewRebateStatModalProps) {
  const t = useTranslations(translationNamespace);

  const [isLoading, setIsLoading] = useState(false);
  const [stats, setStats] = useState<RebateChildStat | null>(null);
  const [rebateList, setRebateList] = useState<RebateSymbolRow[]>([]);
  const [rebateTotals, setRebateTotals] = useState<RebateTotal[]>([]);
  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined);

  const fetchData = useCallback(async (uid: number, from?: string, to?: string) => {
    setIsLoading(true);
    try {
      const [statResult, rebateResult] = await Promise.all([
        fetchChildStat(uid, from, to),
        fetchRebateStat(uid, from, to),
      ]);

      if (statResult.success && statResult.data) {
        setStats(statResult.data);
      }

      if (rebateResult.success && rebateResult.data) {
        const parsed = parseRebateStat(rebateResult.data, rebateStatFormat);
        setRebateList(parsed.rows);
        setRebateTotals(parsed.totals);
      } else {
        setRebateList([]);
        setRebateTotals([]);
      }
    } finally {
      setIsLoading(false);
    }
  }, [fetchChildStat, fetchRebateStat, rebateStatFormat]);

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
          currencyId={840}
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
        <div className="flex flex-col gap-4">
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
          </DialogHeader>

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
