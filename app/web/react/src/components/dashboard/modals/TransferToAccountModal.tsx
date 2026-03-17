'use client';

import { useState, useCallback, useRef, useEffect, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/radix/Select';
import { Button, Checkbox, BalanceShow, Input } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import {
  transferBetweenTradeAccounts,
  sendTransferVerificationCode,
} from '@/actions';
import type { Account } from '@/types/accounts';

interface TransferToAccountModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sourceAccount: Account;
  allAccounts: Account[];
  onSuccess?: () => void;
}

export function TransferToAccountModal({
  open,
  onOpenChange,
  sourceAccount,
  allAccounts,
  onSuccess,
}: TransferToAccountModalProps) {
  const t = useTranslations('wallet.transfer.betweenAccounts');
  const tCommon = useTranslations('common');
  const tTransfer = useTranslations('wallet.transfer');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess, showError } = useToast();

  const [targetUid, setTargetUid] = useState('');
  const [amount, setAmount] = useState('');
  const [amountError, setAmountError] = useState('');
  const [agreed, setAgreed] = useState(false);
  const [verificationCode, setVerificationCode] = useState('');
  const [isSendingCode, setIsSendingCode] = useState(false);
  const [countdown, setCountdown] = useState(0);
  const countdownRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const targetAccounts = useMemo(
    () =>
      allAccounts.filter(
        (a) => a.tradeAccount && a.uid !== sourceAccount.uid
      ),
    [allAccounts, sourceAccount.uid]
  );

  const maxBalance = useMemo(() => {
    const ta = sourceAccount.tradeAccount;
    if (!ta) return 0;
    const available = (ta.equityInCents - ta.creditInCents) / 100;
    return Math.max(available, 0);
  }, [sourceAccount]);

  const handleClose = useCallback(() => {
    onOpenChange(false);
    setTimeout(() => {
      setTargetUid('');
      setAmount('');
      setAmountError('');
      setAgreed(false);
      setVerificationCode('');
      setCountdown(0);
      if (countdownRef.current) {
        clearInterval(countdownRef.current);
        countdownRef.current = null;
      }
    }, 200);
  }, [onOpenChange]);

  useEffect(() => {
    return () => {
      if (countdownRef.current) clearInterval(countdownRef.current);
    };
  }, []);

  const validateAmount = useCallback(
    (val: string): boolean => {
      if (!val || val.trim() === '') {
        setAmountError(t('pleaseInput'));
        return false;
      }
      const num = Number(val);
      if (isNaN(num) || num <= 0) {
        setAmountError(t('amountGreaterThanZero'));
        return false;
      }
      if (num > maxBalance) {
        setAmountError(t('amountExceedsBalance'));
        return false;
      }
      if (!/^\d+(\.\d{1,2})?$/.test(val)) {
        setAmountError(t('upToTwoDecimals'));
        return false;
      }
      setAmountError('');
      return true;
    },
    [maxBalance, t]
  );

  const handleSendCode = useCallback(async () => {
    if (countdown > 0) return;
    setIsSendingCode(true);
    try {
      const result = await execute(
        sendTransferVerificationCode,
        'TradeAccountToTradeAccount'
      );
      if (result.success && result.data) {
        setCountdown(result.data.expiresIn);
        countdownRef.current = setInterval(() => {
          setCountdown((prev) => {
            if (prev <= 1) {
              if (countdownRef.current) {
                clearInterval(countdownRef.current);
                countdownRef.current = null;
              }
              return 0;
            }
            return prev - 1;
          });
        }, 1000);
        showSuccess(t('codeSentToEmail'));
      }
    } finally {
      setIsSendingCode(false);
    }
  }, [countdown, execute, showSuccess, t]);

  const handleSubmit = useCallback(async () => {
    if (!validateAmount(amount)) return;
    if (!targetUid) {
      showError(t('selectTargetAccount'));
      return;
    }
    if (!agreed) {
      showError(tTransfer('confirmRequired'));
      return;
    }

    const result = await execute(transferBetweenTradeAccounts, {
      sourceTradeAccountUid: sourceAccount.uid,
      targetTradeAccountUid: Number(targetUid),
      amount: Number(amount),
      verificationCode: verificationCode || undefined,
    });

    if (result.success) {
      showSuccess(t('submitSuccess'));
      handleClose();
      onSuccess?.();
    }
  }, [
    amount,
    targetUid,
    agreed,
    verificationCode,
    sourceAccount.uid,
    execute,
    validateAmount,
    showError,
    showSuccess,
    handleClose,
    onSuccess,
    t,
    tTransfer,
  ]);

  return (
    <Dialog
      open={open}
      onOpenChange={(v) => {
        if (!v) handleClose();
        else onOpenChange(v);
      }}
    >
      <DialogContent className="sm:max-w-[520px]">
        <DialogHeader>
          <DialogTitle>{t('title')}</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-5 py-4">
          {/* Max Transfer Out */}
          <div className="flex items-center gap-2 rounded-lg bg-surface-secondary p-4">
            <span className="text-sm text-text-secondary">
              {t('maxTransferOut')}:
            </span>
            <BalanceShow
              balance={Math.round(maxBalance * 100)}
              currencyId={sourceAccount.currencyId}
              className="text-sm font-semibold text-primary"
            />
          </div>

          {/* Target Account */}
          <div className="flex flex-col gap-2">
            <label className="flex items-center text-sm font-medium text-text-secondary">
              <span className="mr-1 text-primary">*</span>
              {t('targetAccount')}
            </label>
            <Select value={targetUid} onValueChange={setTargetUid}>
              <SelectTrigger className="h-12 w-full bg-input-bg">
                <SelectValue placeholder={t('selectTargetAccount')} />
              </SelectTrigger>
              <SelectContent>
                {targetAccounts.map((acc) => (
                  <SelectItem key={acc.uid} value={String(acc.uid)}>
                    <span className="flex items-center gap-2">
                      {acc.tradeAccount!.accountNumber}
                      <BalanceShow
                        balance={acc.tradeAccount!.balanceInCents}
                        currencyId={acc.tradeAccount!.currencyId}
                        className="text-xs text-text-secondary"
                      />
                    </span>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Amount */}
          <div className="flex flex-col gap-2">
            <label className="flex items-center text-sm font-medium text-text-secondary">
              <span className="mr-1 text-primary">*</span>
              {t('transferAmount')}
            </label>
            <input
              type="number"
              value={amount}
              onChange={(e) => {
                setAmount(e.target.value);
                if (amountError) validateAmount(e.target.value);
              }}
              onBlur={() => amount && validateAmount(amount)}
              className="h-12 w-full rounded bg-input-bg px-3 text-sm font-medium text-text-primary outline-none"
              placeholder={t('pleaseInput')}
            />
            {amountError && (
              <span className="text-xs text-error">{amountError}</span>
            )}
          </div>

          {/* Verification Code */}
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium text-text-secondary">
              {tTransfer('verificationCode')}
            </label>
            <div className="flex items-center gap-3">
              <input
                type="text"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                className="h-12 flex-1 rounded bg-input-bg px-3 text-sm text-text-primary outline-none"
                placeholder={tTransfer('codePlaceholder')}
              />
              <Button
                variant="primary"
                size="sm"
                className="shrink-0"
                onClick={handleSendCode}
                disabled={countdown > 0 || isSendingCode}
                loading={isSendingCode}
              >
                {countdown > 0
                  ? `${Math.floor(countdown / 60)}:${String(countdown % 60).padStart(2, '0')}`
                  : tTransfer('sendCode')}
              </Button>
            </div>
          </div>

          {/* Agreement Checkbox */}
          <Checkbox
            variant="radio"
            checked={agreed}
            onCheckedChange={(checked) => setAgreed(checked === true)}
            label={t('transferAgreement')}
          />
        </div>

        <DialogFooter className="flex flex-row justify-end gap-3">
          <Button
            variant="outline"
            onClick={handleClose}
            disabled={isLoading}
          >
            {tCommon('cancel')}
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={isLoading}
            disabled={!amount || !targetUid || !agreed}
          >
            {tCommon('submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
