'use client';

import { useState, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, Input } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import type { ActionResponse } from '@/hooks/useServerAction';

interface UnlockEmailAddressModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  uid: number | null;
  email: string | undefined;
  sendCodeAction: (ownerUid: number, accountUid: number) => Promise<ActionResponse<number>>;
  verifyCodeAction: (ownerUid: number, accountUid: number, code: number) => Promise<ActionResponse<string>>;
  ownerUid: number;
}

export function UnlockEmailAddressModal({
  open,
  onOpenChange,
  uid,
  email,
  sendCodeAction,
  verifyCodeAction,
  ownerUid,
}: UnlockEmailAddressModalProps) {
  const t = useTranslations('common');
  const tUnlock = useTranslations('unlockEmail');
  // 关闭 toast：弹窗内已有 inline 消息区域展示错误（含 CODE_NOT_FOUND/CODE_ALREADY_SENT 等），避免双重提示
  const { execute } = useServerAction({ showErrorToast: false });

  const [emailAddress, setEmailAddress] = useState(email ?? '');
  const [verificationCode, setVerificationCode] = useState('');
  const [ableToCheck, setAbleToCheck] = useState(false);
  const [message, setMessage] = useState(tUnlock('needVerificationCode'));
  const [messageType, setMessageType] = useState<'info' | 'success' | 'error'>('info');
  const [isSending, setIsSending] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const sendCode = useCallback(async () => {
    if (uid === null) return;
    setIsSending(true);
    try {
      const result = await execute(sendCodeAction, ownerUid, uid);
      if (result.success) {
        setAbleToCheck(true);
        setMessage(tUnlock('codeSentToEmail'));
        setMessageType('success');
      } else {
        const errorCode = result.errorCode || '';
        if (errorCode === 'CODE_ALREADY_SENT') {
          setAbleToCheck(true);
        }
        setMessage(result.error || tUnlock('sendFailed'));
        setMessageType('error');
      }
    } catch {
      setMessage(tUnlock('sendFailed'));
      setMessageType('error');
    }
    setVerificationCode('');
    setIsSending(false);
  }, [uid, ownerUid, execute, sendCodeAction, tUnlock]);

  const handleSubmit = useCallback(async () => {
    if (uid === null) return;
    if (!verificationCode.trim()) {
      setMessage(tUnlock('codeRequired'));
      setMessageType('error');
      return;
    }
    setIsSubmitting(true);
    try {
      const result = await execute(verifyCodeAction, ownerUid, uid, Number(verificationCode));
      if (result.success && result.data) {
        setEmailAddress(result.data);
        setMessage(t('success'));
        setMessageType('success');
      } else {
        setMessage(result.error || tUnlock('verifyFailed'));
        setMessageType('error');
      }
    } catch {
      setMessage(tUnlock('verifyFailed'));
      setMessageType('error');
    }
    setVerificationCode('');
    setIsSubmitting(false);
  }, [uid, ownerUid, verificationCode, execute, verifyCodeAction, tUnlock, t]);
  const messageColor =
    messageType === 'success'
      ? 'bg-green-50 text-green-700 border-green-200 dark:bg-green-900/20 dark:text-green-400 dark:border-green-800'
      : messageType === 'error'
        ? 'bg-red-50 text-red-700 border-red-200 dark:bg-red-900/20 dark:text-red-400 dark:border-red-800'
        : 'bg-yellow-50 text-yellow-700 border-yellow-200 dark:bg-yellow-900/20 dark:text-yellow-400 dark:border-yellow-800';

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{tUnlock('title')}</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col items-center gap-4 pt-4 pb-2">
          <div className="w-full max-w-[350px]">
            <Input
              value={emailAddress}
              disabled
              className="w-full text-center text-base"
            />
          </div>

          {message && (
            <div className={`w-full max-w-[350px] rounded-full border px-4 py-1.5 text-center text-sm ${messageColor}`}>
              {message}
            </div>
          )}

          {!ableToCheck ? (
            <Button
              variant="primary"
              size="sm"
              onClick={sendCode}
              loading={isSending} className="whitespace-nowrap"
            >
              {tUnlock('sendCode')}
            </Button>
          ) : (
            <div className="flex items-center gap-3">
              <Input
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSubmit()}
                placeholder={tUnlock('codePlaceholder')}
                disabled={isSubmitting}
                className="w-[150px]"
                inputSize="sm"
              />
              <Button
                variant="primary"
                size="sm"
                onClick={handleSubmit}
                loading={isSubmitting}
                className="whitespace-nowrap"
              >
                {t('confirm')}
              </Button>
              <Button
                variant="secondary"
                size="sm"
                onClick={sendCode}
                loading={isSending}
                disabled={isSubmitting}
                className="whitespace-nowrap"
              >
                {tUnlock('resendCode')}
              </Button>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
