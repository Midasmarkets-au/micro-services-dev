'use client';

import { useRouter, useSearchParams, useParams } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useState, useMemo } from 'react';
import { Input, Button, AuthSuccessState, AuthErrorState } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { useServerAction } from '@/hooks/useServerAction';
import { changeTradeAccountPassword } from '@/actions';

// 页面状态类型
type PageStatus = 'idle' | 'loading' | 'success' | 'error';

export default function ChangeAccountPasswordPage() {
  const t = useTranslations('auth');
  const tCommon = useTranslations('common');
  const router = useRouter();
  const params = useParams();
  const searchParams = useSearchParams();
  const { execute, isLoading } = useServerAction({ showErrorToast: false });

  // 从 URL 获取参数
  const tenantId = params.tenantId as string;
  const accountNumber = searchParams.get('an') || '';
  const uid = searchParams.get('uid') || '';
  const pid = searchParams.get('pid') || '';
  const token = searchParams.get('token') || '';

  // 页面状态
  const [status, setStatus] = useState<PageStatus>('idle');
  const [errorMessage, setErrorMessage] = useState<string>('');

  // 表单验证 schema
  const changePasswordSchema = z
    .object({
      password: z
        .string()
        .min(1, t('passwordRequired'))
        .min(8, t('passwordMin8'))
        .regex(/[a-z]/, t('passwordLowercase'))
        .regex(/[A-Z]/, t('passwordUppercase'))
        .regex(/\d/, t('passwordNumber'))
        .regex(/[!@#$%^&*(),.?":{}|<>]/, t('passwordSymbol')),
      confirmPassword: z
        .string()
        .min(1, t('confirmPasswordRequiredShort')),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('passwordNotMatch'),
      path: ['confirmPassword'],
    });

  type ChangePasswordFormData = z.infer<typeof changePasswordSchema>;

  // 表单
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
  });

  // 检查必要参数是否缺失
  const isMissingParams = useMemo(() => !uid || !pid || !token, [uid, pid, token]);

  // 提交表单
  const onSubmit = async (data: ChangePasswordFormData) => {
    setStatus('loading');

    // 使用 Server Action
    const result = await execute(changeTradeAccountPassword, {
      referenceId: uid,
      partyId: pid,
      token,
      password: data.password,
      tenantId,
    });

    if (result.success) {
      setStatus('success');
    } else {
      setStatus('error');
      setErrorMessage(result.error || t('verificationFailed'));
    }
  };

  // 返回登录
  const handleBackToLogin = () => router.push('/sign-in');

  // 渲染表单
  const renderForm = () => (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      {/* 新密码 */}
      <Input
        label={t('newPassword')}
        type="password"
        placeholder={t('pleaseInput')}
        error={errors.password?.message}
        showPasswordToggle
        autoComplete="new-password"
        inputSize="md"
        {...register('password')}
      />

      {/* 确认密码 */}
      <Input
        label={t('confirmPassword')}
        type="password"
        placeholder={t('pleaseInput')}
        error={errors.confirmPassword?.message}
        showPasswordToggle
        autoComplete="new-password"
        inputSize="md"
        {...register('confirmPassword')}
      />

      {/* 密码提示 */}
      <p className="text-sm text-text-secondary">
        {t('passwordHint')}
      </p>

      {/* 提交按钮 */}
      <div className="mt-4">
        <Button type="submit" loading={isSubmitting || isLoading || status === 'loading'}>
          {isSubmitting || isLoading || status === 'loading' ? tCommon('loading') : tCommon('submit')}
        </Button>
      </div>
    </form>
  );

  // 根据状态渲染内容
  const renderContent = () => {
    // 缺少必要参数，显示错误
    if (isMissingParams) {
      return (
        <AuthErrorState
          title={t('verificationFailed')}
          description={t('invalidResetLink')}
          onButtonClick={handleBackToLogin}
        />
      );
    }
    if (status === 'success') {
      return (
        <AuthSuccessState
          title={t('passwordResetSuccess')}
          onButtonClick={handleBackToLogin}
        />
      );
    }
    if (status === 'error') {
      return (
        <AuthErrorState
          title={errorMessage || t('verificationFailed')}
          onButtonClick={handleBackToLogin}
        />
      );
    }
    return renderForm();
  };

  return (
    <div className="card auth-card">
      {/* 左侧插图 - flex: 1 */}
      <div className="auth-illustration-container">
        <AuthIllustration />
      </div>

      {/* 右侧表单 */}
      <div className="auth-card-form flex flex-col">
        {/* 标题 */}
        <h1 className="mb-2 text-responsive-2xl font-semibold text-text-primary">
          {t('changeAccountPassword')}
        </h1>
        {accountNumber && (
          <p className="mb-8 text-lg text-text-secondary">
            {t('account')}: {accountNumber}
          </p>
        )}

        {/* 动态内容区域 */}
        {renderContent()}
      </div>
    </div>
  );
}
