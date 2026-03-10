'use client';

import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { requestPasswordReset } from '@/actions';
import { useToast } from '@/hooks/useToast';

interface ResetPasswordModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accountUid: number;
  accountNumber: number;
  platformName: string;
}

export function ResetPasswordModal({
  open,
  onOpenChange,
  accountUid,
  accountNumber,
  platformName,
}: ResetPasswordModalProps) {
  const t = useTranslations('accounts');
  const { showSuccess } = useToast();
  const { execute, isLoading } = useServerAction({ showErrorToast: true });

  const handleSubmit = async () => {
    // 获取当前域名作为回调 URL
    const callbackUrl = `${window.location.origin}/change-account-password`;
    
    const result = await execute(requestPasswordReset, accountUid, accountNumber, callbackUrl);
    if (result.success) {
      showSuccess(t('toast.passwordResetEmailSent'));
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent onOpenAutoFocus={(e) => e.preventDefault()}>
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold text-text-primary">
            {t('modal.resetPassword', { platform: platformName })}
          </DialogTitle>
        </DialogHeader>

        <div className="py-8">
          <p className="text-base text-text-secondary">
            {t('modal.resetPasswordDesc', { accountNumber })}
          </p>
        </div>

        <DialogFooter className="flex flex-row gap-5 justify-end pt-5 border-t border-border">
          <Button
            variant="secondary"
            onClick={() => onOpenChange(false)}
            disabled={isLoading}
            className="w-[120px]"
          >
            {t('action.close')}
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={isLoading}
            className="w-[120px]"
          >
            {t('action.submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
