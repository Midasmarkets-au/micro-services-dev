'use client';

import { useTranslations } from 'next-intl';
import { TransactionIcon } from './TransactionIcon';
import {
  TransactionType,
  Transaction,
  WithdrawalTransaction,
  TransferTransaction,
  RebateTransaction,
  AdjustTransaction,
  DownlineRewardTransaction,
} from '@/types/wallet';

export interface TransactionInfoProps {
  transaction: Transaction;
  type: TransactionType;
}

const ADJUST_TYPE_KEYS: Record<number, string> = {
  0: 'adjust',
  1: 'walletAdjustSalesRebate',
  2: 'eventRebate',
};

/**
 * 获取金额前缀符号（匹配旧项目 +/- 逻辑）
 */
export function getAmountSign(type: TransactionType): '+' | '-' | '' {
  if (type === TransactionType.Withdrawal) return '-';
  if (type === TransactionType.Rebate) return '+';
  return '';
}

/**
 * 交易信息组件 —— 显示图标 + 描述标签
 */
export function TransactionInfo({ transaction, type }: TransactionInfoProps) {
  const t = useTranslations('wallet');
  const flowType = type === TransactionType.DownlineReward
    ? (transaction as DownlineRewardTransaction).flowType
    : undefined;

  if (type === TransactionType.Withdrawal) {
    const wd = transaction as WithdrawalTransaction;
    const fromLabel = t('transactionSource.wallet');
    const toLabel = wd.paymentMethodName || t('transactionSource.bankAccount');
    return (
      <div className="flex flex-col gap-0.5">
        <div className="flex items-center gap-1.5">
          <span className="text-[#e02b1d] text-xs font-bold">↑</span>
          <span className="text-sm text-text-primary">
            {t('transactionSource.from')} {fromLabel}
          </span>
        </div>
        <div className="flex items-center gap-1.5">
          <span className="text-[#004eff] text-xs font-bold">↓</span>
          <span className="text-sm text-text-primary">
            {t('transactionSource.to')} {toLabel}
          </span>
        </div>
      </div>
    );
  }

  if (type === TransactionType.Transfer) {
    const tr = transaction as TransferTransaction;
    const fromLabel = t('transactionSource.wallet');
    const accountNumber = tr.targetAccountNumber ?? tr.toAccountNumber;
    const toLabel = accountNumber && accountNumber !== 0
      ? `${t('transactionSource.tradeAccount')}(${accountNumber})`
      : t('transactionSource.wallet');
    return (
      <div className="flex flex-col gap-0.5">
        <div className="flex items-center gap-1.5">
          <span className="text-[#e02b1d] text-xs font-bold">↑</span>
          <span className="text-sm text-text-primary">
            {t('transactionSource.from')} {fromLabel}
          </span>
        </div>
        <div className="flex items-center gap-1.5">
          <span className="text-[#004eff] text-xs font-bold">↓</span>
          <span className="text-sm text-text-primary">
            {t('transactionSource.to')} {toLabel}
          </span>
        </div>
      </div>
    );
  }

  const getLabel = (): string => {
    switch (type) {

      case TransactionType.Rebate: {
        const rb = transaction as RebateTransaction;
        if (rb.ticket) {
          return `${t('transactionSource.rebate')} (${t('transactionSource.ticket')} ${rb.ticket})`;
        }
        return t('transactionSource.rebate');
      }

      case TransactionType.Refund:
        return t('transactionSource.refund');

      case TransactionType.Adjust: {
        const adj = transaction as AdjustTransaction;
        const key = ADJUST_TYPE_KEYS[adj.sourceType ?? 0] || 'adjust';
        return t(`transactionSource.${key}`);
      }

      case TransactionType.DownlineReward:
        return t('transactionSource.downlineReward');

      default:
        return t('transactionSource.unknown');
    }
  };

  if (type === TransactionType.DownlineReward) {
    const dr = transaction as DownlineRewardTransaction;
    const rewardLabel = t('transactionSource.downlineReward');
    const isIn = dr.flowType === 'in';
    return (
      <div className="flex items-center gap-1.5">
        {isIn ? (
          <>
            <span className="text-[#004eff] text-xs font-bold">↓</span>
            <span className="text-sm text-text-primary">{rewardLabel}</span>
          </>
        ) : (
          <>
            <span className="text-[#e02b1d] text-xs font-bold">↑</span>
            <span className="text-sm text-text-primary">
              {t('transactionSource.from')} {rewardLabel}
            </span>
          </>
        )}
      </div>
    );
  }

  return (
    <div className="flex items-center gap-3">
      <TransactionIcon type={type} flowType={flowType} size={32} />
      <span className="text-sm text-text-primary">{getLabel()}</span>
    </div>
  );
}
