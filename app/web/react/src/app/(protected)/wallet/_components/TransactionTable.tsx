'use client';

import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { TransactionRow } from './TransactionRow';
import {
  TransactionType,
  Transaction,
  groupTransactionsByDate,
} from '@/types/wallet';

interface TransactionTableProps {
  transactions: Transaction[];
  type: TransactionType;
  currencyId: number;
  isLoading?: boolean;
  onCancelWithdrawal?: (hashId: string) => void;
}

export function TransactionTable({
  transactions,
  type,
  currencyId,
  isLoading,
  onCancelWithdrawal,
}: TransactionTableProps) {
  const t = useTranslations('wallet');
  const { theme } = useTheme();

  const noDataImage =
    theme === 'dark'
      ? '/images/data/no-data-night.svg'
      : '/images/data/no-data-day.svg';

  const hideCurrency =
    type === TransactionType.Adjust || type === TransactionType.DownlineReward;

  if (isLoading) {
    return (
      <div className="flex flex-col gap-5">
        <div className="h-7 w-32 bg-surface-secondary rounded animate-pulse" />
        <div className="h-6 bg-surface-secondary rounded animate-pulse" />
        {[1, 2].map((i) => (
          <div
            key={i}
            className="h-40 bg-surface-secondary rounded-xl animate-pulse"
          />
        ))}
      </div>
    );
  }

  if (!transactions || transactions.length === 0) {
    return (
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
    );
  }

  const groupedTransactions = groupTransactionsByDate(transactions);

  return (
    <div className="flex flex-col gap-5">
      <div className="text-lg font-semibold">
        <span className="text-text-secondary">{t('table.showing')}</span>
        <span className="text-text-primary">{transactions.length}</span>
        <span className="text-text-secondary">{t('table.results')}</span>
      </div>

      <div className="hidden md:flex items-center px-5 gap-[200px]">
        <div className="w-[100px] shrink-0 opacity-0">
          <span>Date</span>
        </div>
        <div className="flex-1 flex items-center justify-between">
          <div className="flex-1 pl-[43px]">
            <span className="text-sm font-medium text-text-secondary">
              {t(`tabs.${type === TransactionType.DownlineReward ? 'downlineReward' : type}`)}
            </span>
          </div>
          <div className="flex-1 text-center">
            <span className="text-sm font-medium text-text-secondary">
              {t('table.status')}
            </span>
          </div>
          {!hideCurrency && (
            <div className="flex-1 text-center">
              <span className="text-sm font-medium text-text-secondary">
                {t('table.currency')}
              </span>
            </div>
          )}
          <div className="flex-1 text-right">
            <span className="text-sm font-medium text-text-secondary">
              {t('table.amount')}
            </span>
          </div>
          <div className="flex-1 text-right pr-[53px]">
            <span className="text-sm font-medium text-text-secondary">
              {t('table.time')}
            </span>
          </div>
        </div>
      </div>

      <div className="flex flex-col gap-5">
        {Array.from(groupedTransactions.entries()).map(([dateKey, items]) => {
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
            <div
              key={dateKey}
              className="flex flex-col md:flex-row md:items-start gap-4 md:gap-[200px] border border-border rounded-xl p-5 overflow-hidden"
            >
              <div className="shrink-0 w-[100px]">
                <div className="flex flex-col gap-2.5">
                  <span className="text-xl font-bold text-text-primary font-['DIN',sans-serif]">
                    {weekday}
                  </span>
                  <span className="text-sm text-text-secondary font-['DIN',sans-serif]">
                    {dayMonthYear}
                  </span>
                </div>
              </div>

              <div className="flex-1 flex flex-col gap-5">
                {items.map((transaction, index) => (
                  <div key={transaction.hashId || `${transaction.createdOn}-${index}`}>
                    <TransactionRow
                      transaction={transaction}
                      type={type}
                      currencyId={currencyId}
                      onCancel={onCancelWithdrawal}
                    />
                    {index < items.length - 1 && (
                      <div className="w-full h-px bg-border mt-5" />
                    )}
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
