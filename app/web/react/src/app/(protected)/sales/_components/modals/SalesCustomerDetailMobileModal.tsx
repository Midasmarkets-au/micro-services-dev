'use client';

import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { useTranslations } from 'next-intl';
import { Cross2Icon } from '@radix-ui/react-icons';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { BalanceShow, Button, Icon, Input } from '@/components/ui';
import { TimeShow } from '@/components/TimeShow';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { getSalesEmailByCode, getSalesViewEmailCode } from '@/actions';
import { AccountRoleTypes } from '@/types/accounts';
import type { SalesClientAccount } from '@/types/sales';

interface SalesCustomerDetailMobileModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
  onEmailRevealed?: (uid: number, email: string) => void;
}

function getUserName(item: SalesClientAccount | null): string {
  const u = item?.user;
  if (u?.nativeName && u.nativeName.trim()) return u.nativeName;
  if (u?.displayName && u.displayName.trim()) return u.displayName;
  if (u?.firstName && u?.lastName && u.firstName.trim() && u.lastName.trim()) {
    return `${u.firstName} ${u.lastName}`;
  }
  return 'No Name';
}

export function SalesCustomerDetailMobileModal({
  open,
  onOpenChange,
  account,
  onEmailRevealed,
}: SalesCustomerDetailMobileModalProps) {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const tUnlock = useTranslations('unlockEmail');
  const tCommon = useTranslations('common');
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const { execute } = useServerAction({ showErrorToast: false });

  const [activeUid, setActiveUid] = useState<number | null>(null);
  const [emailAddress, setEmailAddress] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [ableToCheck, setAbleToCheck] = useState(false);
  const [message, setMessage] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (!open || !account) return;
    if (account.uid === activeUid) return;
    setActiveUid(account.uid);
    setEmailAddress(account.user?.email ?? '');
    setVerificationCode('');
    setAbleToCheck(false);
    setMessage('');
    setIsSending(false);
    setIsSubmitting(false);
    setCopied(false);
  }, [open, account, activeUid]);

  const sendCode = useCallback(async () => {
    if (!account || !salesAccount) return;
    setIsSending(true);
    try {
      const result = await execute(getSalesViewEmailCode, salesAccount.uid, account.uid);
      if (result.success) {
        setAbleToCheck(true);
        setMessage(tUnlock('codeSentToEmail'));
      } else {
        const errorCode = result.errorCode || '';
        if (errorCode === 'CODE_ALREADY_SENT') {
          setAbleToCheck(true);
        }
        setMessage(result.error || tUnlock('sendFailed'));
      }
    } catch {
      setMessage(tUnlock('sendFailed'));
    }
    setVerificationCode('');
    setIsSending(false);
  }, [account, salesAccount, execute, tUnlock]);

  const handleSubmit = useCallback(async () => {
    if (!account || !salesAccount) return;
    if (!verificationCode.trim()) {
      setMessage(tUnlock('codeRequired'));
      return;
    }
    setIsSubmitting(true);
    try {
      const result = await execute(
        getSalesEmailByCode,
        salesAccount.uid,
        account.uid,
        Number(verificationCode),
      );
      if (result.success && result.data) {
        setEmailAddress(result.data);
        setMessage(t('action.verifySuccess'));
        onEmailRevealed?.(account.uid, result.data);
      } else {
        setMessage(result.error || tUnlock('verifyFailed'));
      }
    } catch {
      setMessage(tUnlock('verifyFailed'));
    }
    setVerificationCode('');
    setIsSubmitting(false);
  }, [account, salesAccount, verificationCode, execute, tUnlock, t, onEmailRevealed]);

  const markCopied = useCallback(() => {
    setCopied(true);
    window.setTimeout(() => setCopied(false), 1000);
  }, []);

  const handleCopy = useCallback((value: string, anchor: HTMLElement) => {
    if (!value) return;

    // 弹窗会把焦点留在对话框里，异步 clipboard 经常直接失败。复制必须在这次点击里同步完成。
    const dialog = anchor.closest('[role="dialog"]') ?? document.body;
    const textarea = document.createElement('textarea');
    textarea.value = value;
    textarea.setAttribute('readonly', '');
    textarea.style.position = 'fixed';
    textarea.style.top = '0';
    textarea.style.left = '0';
    textarea.style.width = '1px';
    textarea.style.height = '1px';
    textarea.style.opacity = '0';
    dialog.appendChild(textarea);
    textarea.focus();
    textarea.select();
    textarea.setSelectionRange(0, textarea.value.length);
    let ok = false;
    try {
      ok = document.execCommand('copy');
    } catch {
      ok = false;
    }
    dialog.removeChild(textarea);

    if (ok) {
      markCopied();
      return;
    }

    if (navigator.clipboard?.writeText) {
      void navigator.clipboard.writeText(value).then(markCopied).catch(() => undefined);
    }
  }, [markCopied]);

  if (!account) return null;

  const isUidAccount =
    account.role === AccountRoleTypes.IB || account.role === AccountRoleTypes.Sales;
  const accountNo = account.accountNumber || account.tradeAccount?.accountNumber;
  const copyValue = isUidAccount ? String(account.uid) : accountNo ? String(accountNo) : '';

  let roleLabel = t('customers.clientType');
  let roleClass = 'bg-[#212529] text-white';
  if (account.role === AccountRoleTypes.IB) {
    roleLabel = t('customers.ibType');
    roleClass = 'bg-[#50cd89] text-white';
  } else if (account.role === AccountRoleTypes.Sales) {
    roleLabel = t('customers.salesType');
    roleClass = 'bg-[#f1416c] text-white';
  }

  const typeLabel = tAccount.has(`accountTypes.${account.type}`)
    ? tAccount(`accountTypes.${account.type}`)
    : account.type === 0
      ? t('fields.default')
      : String(account.type);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[500px]">
        <DialogHeader className="flex-row items-start justify-between space-y-0">
          <DialogTitle className="text-base font-medium">{getUserName(account)}</DialogTitle>
          <DialogClose className="text-text-secondary hover:text-text-primary">
            <Cross2Icon className="size-4" />
            <span className="sr-only">{t('action.close')}</span>
          </DialogClose>
        </DialogHeader>

        <div className="mt-4 flex items-center justify-between gap-3">
          <span className="min-w-0 truncate text-sm text-text-primary">{emailAddress}</span>
          <Button
            size="sm"
            loading={isSending}
            disabled={isSubmitting}
            onClick={sendCode}
            className="shrink-0 border-0 bg-[#e6a23c] text-white hover:bg-[#cf9236]"
          >
            {t('action.sendCode')}
          </Button>
        </div>

        {ableToCheck && (
          <div className="mt-3 flex flex-col gap-2">
            {message && (
              <div className="rounded-full border border-[#e0e0e0] bg-[#fff8dc] px-3 py-1 text-center text-sm text-[#900000]">
                {message}
              </div>
            )}
            <div className="flex items-center justify-end gap-3">
              <div className="min-w-0 w-[150px] max-w-full flex-1">
                <Input
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSubmit()}
                  placeholder={t('action.oneTimeCode')}
                  disabled={isSending || isSubmitting}
                  inputSize="sm"
                />
              </div>
              <Button
                variant="primary"
                size="sm"
                onClick={handleSubmit}
                loading={isSubmitting}
                disabled={isSending}
                className="shrink-0 whitespace-nowrap"
              >
                {tCommon('confirm')}
              </Button>
            </div>
          </div>
        )}

        {!ableToCheck && message && (
          <div className="mt-3 rounded-full border border-[#e0e0e0] bg-[#fff8dc] px-3 py-1 text-center text-sm text-[#900000]">
            {message}
          </div>
        )}

        <div className="mt-3 flex flex-col gap-2 rounded-md border border-border bg-(--color-surface-secondary) px-4 py-3 text-sm">
          <DetailRow label={isUidAccount ? t('fields.accountUid') : t('fields.accountNo')}>
            <span>{copyValue || t('customers.noTradeAccount')}</span>
            {copyValue && (
              <button
                type="button"
                className="relative inline-flex shrink-0 items-center text-text-secondary hover:text-text-primary"
                aria-label={t('action.copy')}
                onClick={(event) => {
                  event.stopPropagation();
                  handleCopy(copyValue, event.currentTarget);
                }}
              >
                <Icon name="copy" size={14} className="pointer-events-none" />
                {copied && (
                  <span className="pointer-events-none absolute bottom-full left-1/2 mb-1 -translate-x-1/2 whitespace-nowrap rounded bg-surface px-1.5 py-0.5 text-xs text-text-primary shadow-card">
                    {t('dashboard.copied')}
                  </span>
                )}
              </button>
            )}
          </DetailRow>
          <DetailRow label={t('fields.type')}>{typeLabel}</DetailRow>
          <DetailRow label={t('fields.role')}>
            <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs ${roleClass}`}>
              {roleLabel}
            </span>
          </DetailRow>
          <DetailRow label={t('fields.group')}>{account.group || ''}</DetailRow>
          <DetailRow label={t('fields.code')}>{account.code || ''}</DetailRow>
          {account.role !== AccountRoleTypes.IB && (
            <DetailRow label={t('fields.balance')}>
              <BalanceShow
                balance={account.tradeAccount?.balanceInCents || 0}
                currencyId={account.tradeAccount?.currencyId || 840}
              />
            </DetailRow>
          )}
          <DetailRow label={t('fields.createdOn')}>
            <TimeShow
              dateIsoString={account.createdOn}
              className="text-xs text-[#717171]"
            />
          </DetailRow>
        </div>

        <DialogFooter className="mt-4">
          <Button variant="outline" size="sm" onClick={() => onOpenChange(false)} disabled={isSending || isSubmitting}>
            {t('action.close')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function DetailRow({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-4">
      <span className="shrink-0 text-text-secondary">{label}</span>
      <span className="flex items-center gap-1 text-right text-text-primary">{children}</span>
    </div>
  );
}
