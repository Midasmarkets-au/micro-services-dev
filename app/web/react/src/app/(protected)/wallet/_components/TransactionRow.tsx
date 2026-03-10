'use client';

import { useTranslations } from 'next-intl';
import {
  Button,
  DateDisplay,
  BalanceShow,
  CurrencyCodeMap,
  TransactionInfo,
  getAmountSign,
} from '@/components/ui';
import {
  TransactionType,
  Transaction,
  getStatusStyle,
  getStatusText,
  canCancelWithdrawal,
} from '@/types/wallet';

interface TransactionRowProps {
  transaction: Transaction;
  type: TransactionType;
  currencyId: number;
  onCancel?: (hashId: string) => void;
}

export function TransactionRow({
  transaction,
  type,
  currencyId,
  onCancel,
}: TransactionRowProps) {
  const t = useTranslations('wallet');
  const statusStyle = getStatusStyle(type, transaction.stateId);
  const statusText = getStatusText(type, transaction.stateId);
  const effectiveCurrencyId = transaction.currencyId ?? currencyId;
  const currencyCode = CurrencyCodeMap[effectiveCurrencyId] || 'USD';
  const amountSign = getAmountSign(type);

  const showCancelButton =
    type === TransactionType.Withdrawal &&
    !!transaction.hashId &&
    canCancelWithdrawal(transaction.stateId);

  const hideCurrency =
    type === TransactionType.Adjust || type === TransactionType.DownlineReward;

  return (
    <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
      {/* 交易描述（图标 + 标签）—— 使用公共 TransactionInfo 组件 */}
      <div className="flex-1">
        <TransactionInfo transaction={transaction} type={type} />
      </div>

      {/* 状态 */}
      <div className="flex-1 flex justify-start md:justify-center">
        <span
          className={`inline-flex items-center px-3 py-1 rounded text-xs font-normal ${statusStyle.bgColor} ${statusStyle.textColor}`}
        >
          {t(`status.${statusText}`)}
        </span>
      </div>

      {/* 货币（调整和推广奖励不显示） */}
      {!hideCurrency && (
        <div className="flex-1 flex justify-start md:justify-center">
          <span className="text-base font-semibold text-text-primary">
            {currencyCode}
          </span>
        </div>
      )}

      {/* 金额 */}
      <div className="flex-1 flex justify-start md:justify-end">
        <BalanceShow
          balance={Math.abs(transaction.amount)}
          currencyId={effectiveCurrencyId}
          sign={amountSign}
          className="text-base font-bold text-text-primary font-['DIN',sans-serif]"
        />
      </div>

      {/* 时间 + 取消按钮 */}
      <div className="flex-1 flex items-center justify-start md:justify-end gap-3">
        <DateDisplay
          value={transaction.createdOn}
          format="datetime"
          className="text-sm text-text-secondary"
        />

        {showCancelButton && onCancel && transaction.hashId && (
          <Button
            variant="outline"
            size="xs"
            onClick={() => onCancel(transaction.hashId!)}
            className="text-xs"
          >
            {t('action.cancel')}
          </Button>
        )}
      </div>
    </div>
  );
}
