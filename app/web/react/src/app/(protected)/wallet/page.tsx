'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
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
} from '@/types/wallet';
import {
  WalletBanner,
  TransactionTabs,
  TransactionTable,
  DateFilter,
  TransferModal,
} from './_components';
import { WithdrawalModal } from '@/components/dashboard/modals/WithdrawalModal';

export default function WalletPage() {
  const t = useTranslations('wallet');
  const { showSuccess } = useToast();
  const { execute } = useServerAction({ showErrorToast: true });

  const [wallet, setWallet] = useState<Wallet | null>(null);
  const [isLoadingWallet, setIsLoadingWallet] = useState(true);
  const [currentTab, setCurrentTab] = useState<TransactionType>(
    TransactionType.Withdrawal
  );
  const [transactions, setTransactions] = useState<Transaction[]>([]);
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
      console.log('result', result);
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
        }
      } finally {
        setIsLoadingTransactions(false);
      }
    },
    [wallet?.hashId, execute, startDate, endDate]
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
      loadTransactions(tab);
    },
    [loadTransactions]
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
    loadTransactions(currentTab);
  }, [currentTab, loadTransactions]);

  const handleSearch = useCallback(() => {
    loadTransactions(currentTab);
  }, [currentTab, loadTransactions]);

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
    <div className="flex flex-col gap-6 w-full">
      <h1 className="text-2xl font-semibold text-text-primary">
        {t('title')}
      </h1>

      {isLoadingWallet ? (
        <div className="h-[170px] bg-surface rounded animate-pulse" />
      ) : (
        <WalletBanner
          balance={wallet?.balance ?? 0}
          currencyId={wallet?.currencyId ?? 840}
          onWithdraw={handleWithdraw}
          onTransfer={handleTransfer}
        />
      )}

      <div className="bg-surface rounded border border-border p-5 md:p-8">
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

        <TransactionTable
          transactions={transactions}
          type={currentTab}
          currencyId={wallet?.currencyId || 840}
          isLoading={isLoadingTransactions}
          onCancelWithdrawal={
            currentTab === TransactionType.Withdrawal
              ? handleCancelWithdrawal
              : undefined
          }
        />
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
