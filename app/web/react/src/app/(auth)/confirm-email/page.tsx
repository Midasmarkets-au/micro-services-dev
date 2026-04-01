'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useMemo } from 'react';
import { Input, Button } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { confirmEmail, logout } from '@/actions';

export default function ConfirmEmailPage() {
  const t = useTranslations('auth');
  const router = useRouter();
  const searchParams = useSearchParams();
  const { execute, isLoading } = useServerAction();

  const code = searchParams.get('code') || '';
  const emailFromUrl = searchParams.get('email') || '';

  const { showSuccess } = useToast();

  const email = useMemo(() => {
    if (!emailFromUrl) return '';
    return emailFromUrl.includes('%2B')
      ? decodeURIComponent(emailFromUrl)
      : emailFromUrl.replace(/\s/g, '+');
  }, [emailFromUrl]);

  const handleConfirm = async () => {
    // Vue 原逻辑：确认前先登出
    await logout();

    const result = await execute(confirmEmail, { code, email });
    if (result.success) {
      // Vue 原逻辑：成功后弹提示，然后登出并跳转登录页
      showSuccess(t('confirmEmailSuccess'));
      await logout();
      router.push('/sign-in');
    }
  };

  return (
    <div className="card auth-card">
      <div className="auth-illustration-container">
        <AuthIllustration />
      </div>

      <div className="auth-card-form flex flex-col">
        <h1 className="mb-8 text-center text-responsive-2xl font-semibold text-text-primary">
          {t('emailConfirmation')}
        </h1>

        <div className="mb-6 flex justify-center md:hidden">
          <AuthIllustration size={140} />
        </div>

        <div className="flex flex-col gap-6">
          <Input
            label={t('email')}
            type="email"
            value={email}
            disabled
            autoComplete="email"
            inputSize="md"
          />

          <div className="mt-4">
            <Button
              type="button"
              className="w-full"
              loading={isLoading}
              onClick={handleConfirm}
            >
              {t('confirm')}
            </Button>
          </div>

          <div className="text-center text-sm text-text-secondary">
            {t('backTo')}{' '}
            <Link
              href="/sign-in"
              className="text-text-link transition-colors hover:underline"
            >
              {t('login')}
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
