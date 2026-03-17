'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { disable2FA, enable2FA, getConfiguration } from '@/actions';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { useUserStore } from '@/stores/userStore';
import type { SiteConfiguration } from '@/types/user';
import {
  Button,
  Checkbox,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Input,
} from '@/components/ui';

type TwoFactorChoice = 'enable' | 'disable';

export function TwoFactorAuthDialog() {
  const tCommon = useTranslations('common');
  const tAuth = useTranslations('auth');
  const tSecurity = useTranslations('profile.security');
  const tSecurityMessages = useTranslations('profile.security.messages');
  const { execute } = useServerAction();
  const { showSuccess } = useToast();
  const { siteConfig, setSiteConfig, setTwoFactorAuth } = useUserStore();

  const [isDialogOpen, setIsDialogOpen] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showCodeInput, setShowCodeInput] = useState(false);
  const [choice, setChoice] = useState<TwoFactorChoice>('enable');
  const [code, setCode] = useState('');

  const twoFactorAuth = (siteConfig as { twoFactorAuth?: boolean | null } | null)?.twoFactorAuth;
  const isTwoFactorRequired = twoFactorAuth === null;
  const isOpen = isTwoFactorRequired && isDialogOpen;

  const refreshConfig = async () => {
    const configResult = await execute(getConfiguration);
    if (configResult.success && configResult.data) {
      setSiteConfig(configResult.data as unknown as SiteConfiguration);
    }
  };

  const handleSelect = (value: TwoFactorChoice) => {
    if (isSubmitting) return;
    setChoice(value);
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    const action = choice === 'enable' ? enable2FA : disable2FA;
    const result = await execute(action, showCodeInput ? code.trim() : '');

    if (result.success) {
      if (!showCodeInput) {
        setShowCodeInput(true);
        setCode('');
        showSuccess(tSecurityMessages('codeSent'));
      } else {
        await refreshConfig();
        setTwoFactorAuth(choice === 'enable');
        showSuccess(
          choice === 'enable'
            ? tSecurityMessages('twoFactorEnabled')
            : tSecurityMessages('twoFactorDisabled')
        );
        setShowCodeInput(false);
        setCode('');
        setIsDialogOpen(false);
      }
    } else if (!showCodeInput) {
      // 后端要求输入验证码时，进入第二步
      setShowCodeInput(true);
      setCode('');
    }

    setIsSubmitting(false);
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={(open) => {
        if (!open) setIsDialogOpen(false);
      }}
    >
      <DialogContent>
        <DialogHeader className="space-y-4">
          <DialogTitle className="text-center text-2xl  font-semibold text-primary">
            {tAuth('twoFactorAuthenticationSetupReminder')}
          </DialogTitle>
          <DialogDescription asChild>
            <div className="space-y-2 text-2xl text-text-primary py-4">
              <p>{tAuth('twoFaDesc')}</p>
              <ul className="list-disc pl-5">
                <li>{tAuth('enableTwoFa')}</li>
                <li>{tAuth('disableTwoFa')}</li>
              </ul>
            </div>
          </DialogDescription>
        </DialogHeader>

        {!showCodeInput ? (
          <div className="mt-2 flex flex-wrap items-center gap-8">
            <div
              onClick={() => handleSelect('enable')}
              className="cursor-pointer rounded border border-border bg-surface px-4 py-2 text-base text-text-primary transition hover:bg-surface-secondary"
            >
              <Checkbox
                checked={choice === 'enable'}
                variant="radio"
                disabled={isSubmitting}
                onCheckedChange={(checked) => {
                  if (checked) handleSelect('enable');
                }}
                label={tAuth('enable')}
              />
            </div>
            <div
              onClick={() => handleSelect('disable')}
              className="cursor-pointer rounded border border-border bg-surface px-4 py-2 text-base text-text-primary transition hover:bg-surface-secondary"
            >
              <Checkbox
                checked={choice === 'disable'}
                variant="radio"
                disabled={isSubmitting}
                onCheckedChange={(checked) => {
                  if (checked) handleSelect('disable');
                }}
                label={tAuth('disable')}
              />
            </div>
          </div>
        ) : (
          <div className="mt-2 space-y-2">
            <Input
              value={code}
              onChange={(event) => setCode(event.target.value)}
              placeholder={tSecurity('twoFactorCodePlaceholder')}
              disabled={isSubmitting}
              className="w-full sm:w-[220px]"
            />
            <p className="text-sm text-text-secondary">
              {tAuth('pleaseEnterTwoFactorCode')}
            </p>
          </div>
        )}

        <DialogFooter className="mt-6 flex w-full flex-row items-center justify-center gap-3">
          <Button
            variant="outline"
            onClick={() => {
              if (showCodeInput) {
                setShowCodeInput(false);
                setCode('');
                return;
              }
              setIsDialogOpen(false);
            }}
            disabled={isSubmitting}
            className="w-[120px]"
          >
            {showCodeInput ? tCommon('back') : tCommon('cancel')}
          </Button>
          <Button
            onClick={handleSubmit}
            loading={isSubmitting}
            disabled={showCodeInput && !code.trim()}
            className="w-[120px]"
          >
            {showCodeInput ? tSecurity('verify') : tCommon('submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

