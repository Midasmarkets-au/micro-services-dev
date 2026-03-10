'use client';

import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useState } from 'react';
import { Input, Button, AuthSuccessState } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { forgotPassword } from '@/actions';

export default function ForgotPasswordPage() {
  const t = useTranslations('auth');
  const router = useRouter();
  const { execute, isLoading } = useServerAction();
  const { showSuccess } = useToast();

  // 状态管理
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [submittedEmail, setSubmittedEmail] = useState('');

  // 表单验证 schema
  const forgotPasswordSchema = z.object({
    email: z
      .string()
      .min(1, t('emailRequired'))
      .email(t('emailInvalid')),
  });

  type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

  // 表单
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  // 提交处理
  const onSubmit = async (data: ForgotPasswordFormData) => {
    const resetUrl = `${window.location.protocol}//${window.location.host}/reset-password`;

    // 使用 Server Action
    const result = await execute(forgotPassword, {
      email: data.email,
      resetUrl,
    });

    if (result.success) {
      // 成功发送重置链接
      setSubmittedEmail(data.email);
      setIsSubmitted(true);
      showSuccess(t('passwordResetLinkSent'));
    }
  };

  // 返回登录
  const handleBackToLogin = () => router.push('/sign-in');

  // 渲染表单
  const renderForm = () => (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      {/* 描述文本 */}
      <p className="text-text-secondary">
        {t('enterEmailToResetPassword')}
      </p>

      {/* 邮箱输入 */}
      <Input
        label={t('email')}
        type="email"
        placeholder={t('pleaseInput')}
        error={errors.email?.message}
        autoComplete="email"
        autoFocus
        inputSize="md"
        {...register('email')}
      />

      {/* 提交按钮 */}
      <div className="mt-4">
        <Button type="submit" loading={isSubmitting || isLoading}>
          {t('send')}
        </Button>
      </div>

      {/* 返回登录 */}
      <div className="text-center">
        <Link
          href="/sign-in"
          className="text-sm text-text-link transition-colors hover:underline"
        >
          {t('backToLogin')}
        </Link>
      </div>
    </form>
  );

  return (
    <div className="card auth-card">
      {/* 左侧插图 */}
      <div className="auth-illustration-container">
        <AuthIllustration />
      </div>

      {/* 右侧表单 */}
      <div className="auth-card-form flex flex-col">
        {/* 标题 */}
        <h1 className="mb-8 text-responsive-2xl font-semibold text-text-primary">
          {t('resetPasswordTitle')}
        </h1>

        {/* 动态内容 */}
        {isSubmitted ? (
          <AuthSuccessState
            title={t('passwordResetLinkSent')}
            description={t('checkEmailForResetLink', { email: submittedEmail })}
            onButtonClick={handleBackToLogin}
          >
            {/* 重新发送链接 */}
            <p className="text-center text-sm text-text-secondary">
              {t('didNotReceiveEmail')}{' '}
              <button
                type="button"
                onClick={() => setIsSubmitted(false)}
                className="text-text-link transition-colors hover:underline"
              >
                {t('resendEmail')}
              </button>
            </p>
          </AuthSuccessState>
        ) : renderForm()}
      </div>
    </div>
  );
}
