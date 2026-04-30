'use client';

import Image from 'next/image';
import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { useTheme } from '@/hooks/useTheme';
import {
  getPrimaryWallet,
  getWithdrawalTransactions,
  getTransferTransactions,
  getAdjustTransactions,
  getRefundTransactions,
  getRebateTransactions,
  getDownlineRewardTransactions,
  cancelWithdrawal,
} from '@/actions';
import {
  TransactionType,
  Transaction,
  Wallet,
  GetTransactionsParams,
  getStatusStyle,
  getStatusText,
  canCancelWithdrawal,
} from '@/types/wallet';
import {
  WalletBanner,
  TransactionTabs,
  DateFilter,
  TransferModal,
} from './_components';
import { WithdrawalModal } from '@/components/dashboard/modals/WithdrawalModal';
import {
  DataTable,
  BalanceShow,
  CurrencyCodeMap,
  TransactionInfo,
  Button,
  Pagination,
  getAmountSign,
} from '@/components/ui';
import { TimeShow } from '@/components/TimeShow';
import type { DataTableColumn } from '@/components/ui';

export default function WalletPage() {
  const t = useTranslations('wallet');
  const { showSuccess } = useToast();
  const { execute } = useServerAction({ showErrorToast: true });
  const { theme } = useTheme();

  const [wallet, setWallet] = useState<Wallet | null>(null);
  const [isLoadingWallet, setIsLoadingWallet] = useState(true);
  const [currentTab, setCurrentTab] = useState<TransactionType>(
    TransactionType.Withdrawal
  );
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [isLoadingTransactions, setIsLoadingTransactions] = useState(false);
  const [startDate, setStartDate] = useState<string | null>(null);
  const [endDate, setEndDate] = useState<string | null>(null);

  const [showWithdrawModal, setShowWithdrawModal] = useState(false);
  const [showTransferModal, setShowTransferModal] = useState(false);

  const isLoadedRef = useRef(false);

  const loadWallet = useCallback(async () => {
    setIsLoadingWallet(true);
    try {
      const result = await execute(getPrimaryWallet);
      if (result.success && result.data) {
        setWallet(result.data);
      }
    } finally {
      setIsLoadingWallet(false);
    }
  }, [execute]);

  const loadTransactions = useCallback(
    async (tab: TransactionType, params?: GetTransactionsParams) => {
      if (!wallet?.hashId) return;

      setIsLoadingTransactions(true);
      try {
        const from = startDate
          ? new Date(`${startDate}T00:00:00`).toISOString()
          : undefined;
        const to = endDate
          ? new Date(`${endDate}T23:59:59`).toISOString()
          : undefined;
        const queryParams: GetTransactionsParams = {
          ...params,
          page: params?.page ?? page,
          size: params?.size ?? pageSize,
          from,
          to,
        };

        let result;
        switch (tab) {
          case TransactionType.Withdrawal:
            result = await execute(getWithdrawalTransactions, wallet.hashId, queryParams);
            break;
          case TransactionType.Transfer:
            result = await execute(getTransferTransactions, wallet.hashId, queryParams);
            break;
          case TransactionType.Adjust:
            result = await execute(getAdjustTransactions, wallet.hashId, queryParams);
            break;
          case TransactionType.Refund:
            result = await execute(getRefundTransactions, wallet.hashId, queryParams);
            break;
          case TransactionType.Rebate:
            result = await execute(getRebateTransactions, wallet.hashId, queryParams);
            break;
          case TransactionType.DownlineReward:
            result = await execute(getDownlineRewardTransactions, wallet.hashId, queryParams);
            break;
        }
        if (result?.success && result.data) {
          setTransactions(result.data.data || []);
          setTotal(result.data.total || 0);
        }
      } finally {
        setIsLoadingTransactions(false);
      }
    },
    [wallet?.hashId, execute, startDate, endDate, page, pageSize]
  );

  const handleCancelWithdrawal = useCallback(
    async (hashId: string) => {
      const result = await execute(cancelWithdrawal, hashId);
      if (result.success) {
        showSuccess(t('toast.withdrawalCancelled'));
        loadTransactions(TransactionType.Withdrawal);
      }
    },
    [execute, loadTransactions, showSuccess, t]
  );

  const handleTabChange = useCallback(
    (tab: TransactionType) => {
      setCurrentTab(tab);
      setTransactions([]);
      setTotal(0);
      setPage(1);
      loadTransactions(tab, { page: 1, size: pageSize });
    },
    [loadTransactions, pageSize]
  );

  const handleDateChange = useCallback(
    (newStartDate: string | null, newEndDate: string | null) => {
      setStartDate(newStartDate);
      setEndDate(newEndDate);
    },
    []
  );

  const handleReset = useCallback(() => {
    setStartDate(null);
    setEndDate(null);
    setPage(1);
    loadTransactions(currentTab, {
      page: 1,
      size: pageSize,
      from: undefined,
      to: undefined,
    });
  }, [currentTab, loadTransactions, pageSize]);

  const handleSearch = useCallback(() => {
    setPage(1);
    loadTransactions(currentTab, { page: 1, size: pageSize });
  }, [currentTab, loadTransactions, pageSize]);

  const handlePageChange = useCallback(
    (nextPage: number) => {
      setPage(nextPage);
      loadTransactions(currentTab, { page: nextPage, size: pageSize });
    },
    [currentTab, loadTransactions, pageSize]
  );

  const refreshAfterAction = useCallback(async () => {
    await loadWallet();
    loadTransactions(currentTab);
  }, [loadWallet, currentTab, loadTransactions]);

  const handleWithdraw = useCallback(() => {
    setShowWithdrawModal(true);
  }, []);

  const handleTransfer = useCallback(() => {
    setShowTransferModal(true);
  }, []);

  const tableColumns = useMemo<DataTableColumn<Transaction>[]>(() => {
    const hideCurrency =
      currentTab === TransactionType.Adjust ||
      currentTab === TransactionType.DownlineReward;

    const baseColumns: DataTableColumn<Transaction>[] = [
      {
        key: 'transaction',
        title: t(
          `tabs.${currentTab === TransactionType.DownlineReward ? 'downlineReward' : currentTab}`
        ),
        render: (item) => <TransactionInfo transaction={item} type={currentTab} />,
      },
      {
        key: 'status',
        title: t('table.status'),
        align: 'center',
        render: (item) => {
          const statusStyle = getStatusStyle(currentTab, item.stateId);
          const statusText = getStatusText(currentTab, item.stateId);
          return (
            <span
              className={`inline-flex items-center px-3 py-1 rounded text-xs font-normal ${statusStyle.bgColor} ${statusStyle.textColor}`}
            >
              {t(`status.${statusText}`)}
            </span>
          );
        },
      },
    ];

    if (!hideCurrency) {
      baseColumns.push({
        key: 'currency',
        title: t('table.currency'),
        align: 'center',
        render: (item) => {
          const effectiveCurrencyId = item.currencyId ?? wallet?.currencyId ?? 840;
          return (
            <span className="text-base font-semibold text-text-primary">
              {CurrencyCodeMap[effectiveCurrencyId] || 'USD'}
            </span>
          );
        },
      });
    }

    baseColumns.push(
      {
        key: 'amount',
        title: t('table.amount'),
        align: 'right',
        render: (item) => {
          const effectiveCurrencyId = item.currencyId ?? wallet?.currencyId ?? 840;
          return (
            <BalanceShow
              balance={Math.abs(item.amount)}
              currencyId={effectiveCurrencyId}
              sign={getAmountSign(currentTab)}
              className="text-base font-bold text-text-primary font-['DIN',sans-serif]"
            />
          );
        },
      },
      {
        key: 'time',
        title: t('table.time'),
        align: 'right',
        render: (item) => {
          const showCancelButton =
            currentTab === TransactionType.Withdrawal &&
            !!item.hashId &&
            canCancelWithdrawal(item.stateId);

          return (
            <div className="flex items-center justify-end gap-3">
              <TimeShow
                  dateIsoString={item.createdOn}
                  format="h:mm a"
                />
              {showCancelButton && item.hashId && (
                <Button
                  variant="outline"
                  size="xs"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleCancelWithdrawal(item.hashId!);
                  }}
                  className="text-xs"
                >
                  {t('action.cancel')}
                </Button>
              )}
            </div>
          );
        },
      }
    );

    return baseColumns;
  }, [currentTab, handleCancelWithdrawal, t, wallet?.currencyId]);

  const noDataImage =
    theme === 'dark'
      ? '/images/data/no-data-night.svg'
      : '/images/data/no-data-day.svg';

  useEffect(() => {
    if (isLoadedRef.current) return;
    isLoadedRef.current = true;
    loadWallet();
  }, [loadWallet]);

  useEffect(() => {
    if (wallet?.hashId) {
      loadTransactions(currentTab);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [wallet?.hashId]);

  return (
    <div className="flex flex-col gap-2 w-full">
      <div className="bg-surface rounded border border-border p-5">
        {/* 标题行 */}
        <div className="flex items-center gap-3 mb-5">
          <Image
            src="/images/banner/wallet-day.svg"
            alt="wallet"
            width={26}
            height={26}
          />
          <h1 className="text-xl font-semibold text-text-primary">
            {t('title')}
          </h1>
        </div>
        {/* 分隔线 */}
        <div className="w-full h-px bg-border mb-5" />
        {/* Banner */}
        {isLoadingWallet ? (
          <div className="h-[170px] bg-muted rounded animate-pulse" />
        ) : (
          <WalletBanner
            balance={wallet?.balance ?? 0}
            currencyId={wallet?.currencyId ?? 840}
            onWithdraw={handleWithdraw}
            onTransfer={handleTransfer}
            fractionDigits={2}
          />
        )}
      </div>

      <div className="bg-surface rounded border border-border p-4 md:p-6">
        <div className="flex flex-col gap-0 mb-6">
          <div className="flex flex-col md:flex-row md:items-start md:justify-between gap-4">
            <TransactionTabs
              activeTab={currentTab}
              onTabChange={handleTabChange}
              showRebateTab={true}
            />
            <DateFilter
              startDate={startDate}
              endDate={endDate}
              onDateChange={handleDateChange}
              onReset={handleReset}
              onSearch={handleSearch}
            />
          </div>
          <div className="w-full h-px bg-border" />
        </div>
        <div className="text-lg font-semibold mb-5">
          <span className="text-text-secondary">{t('table.showing')}</span>
          <span className="text-text-primary">{total || transactions.length}</span>
          <span className="text-text-secondary">{t('table.results')}</span>
        </div>
        <DataTable<Transaction>
          columns={tableColumns}
          data={transactions}
          rowKey={(item, idx) => item.hashId || `${item.createdOn}-${idx}`}
          loading={isLoadingTransactions}
          groupConfig={{
            groupBy: (item) => new Date(item.createdOn).toDateString(),
            headerWidth: 'w-[120px]',
            renderGroupHeader: (_groupKey, items) => {
              const firstDate = items[0]?.createdOn
                ? new Date(items[0].createdOn)
                : new Date();
              const weekday = firstDate.toLocaleDateString('en-US', {
                weekday: 'long',
              });
              const dayMonthYear = firstDate.toLocaleDateString('en-US', {
                day: 'numeric',
                month: 'short',
                year: 'numeric',
              });

              return (
                <div className="flex flex-col gap-2.5">
                  <span className="text-xl font-bold text-text-primary font-['DIN',sans-serif]">
                    {weekday}
                  </span>
                  <span className="text-sm text-text-secondary font-['DIN',sans-serif]">
                    {dayMonthYear}
                  </span>
                </div>
              );
            },
          }}
          emptyContent={(
            <div className="flex flex-col items-center justify-center py-16">
              <Image
                src={noDataImage}
                alt="No data"
                width={120}
                height={120}
                className="mb-4"
              />
              <p className="text-base text-text-secondary">
                {t('noTransactions')}
              </p>
            </div>
          )}
        />
        <div className="mt-5">
          <Pagination
            page={page}
            total={total}
            size={pageSize}
            onPageChange={handlePageChange}
          />
        </div>
      </div>

      {wallet && (
        <>
          <WithdrawalModal
            open={showWithdrawModal}
            onOpenChange={setShowWithdrawModal}
            wallet={wallet}
            type='wallet'
            onSuccess={refreshAfterAction}
             
          />
          <TransferModal
            open={showTransferModal}
            onOpenChange={setShowTransferModal}
            wallet={wallet}
            onSuccess={refreshAfterAction}
          />
        </>
      )}
    </div>
  );
}
