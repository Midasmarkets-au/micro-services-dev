'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useState, useEffect } from 'react';
import { Input, Button, AuthSuccessState, AuthErrorState } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { useServerAction } from '@/hooks/useServerAction';
import { resetPassword } from '@/actions';

export default function ResetPasswordPage() {
  const t = useTranslations('auth');
  const router = useRouter();
  const searchParams = useSearchParams();
  const { execute, isLoading } = useServerAction();

  // 从 URL 获取 code 和 email 参数
  const code = searchParams.get('code') || '';
  const emailFromUrl = searchParams.get('email') || '';

  // 状态管理
  const [isSuccess, setIsSuccess] = useState(false);

  // 表单验证 schema
  const resetPasswordSchema = z.object({
    email: z
      .string()
      .min(1, t('emailRequired'))
      .email(t('emailInvalid')),
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
      .min(1, t('confirmPasswordRequired')),
  }).refine((data) => data.password === data.confirmPassword, {
    message: t('passwordNotMatch'),
    path: ['confirmPassword'],
  });

  type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

  // 表单
  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: emailFromUrl,
    },
  });

  // 处理 URL 中的 email（可能需要解码）
  useEffect(() => {
    if (emailFromUrl) {
      // 处理 URL 编码的 + 号（邮箱中的 + 会被编码为 %2B 或空格）
      const decodedEmail = emailFromUrl.includes('%2B')
        ? decodeURIComponent(emailFromUrl)
        : emailFromUrl.replace(/\s/g, '+');
      setValue('email', decodedEmail);
    }
  }, [emailFromUrl, setValue]);

  // 返回登录
  const handleBackToLogin = () => router.push('/sign-in');

  // 检查 code 是否存在
  if (!code) {
    return (
      <div className="card auth-card">
        <div className="auth-illustration-container">
          <AuthIllustration />
        </div>
        <div className="auth-card-form flex flex-col">
          <h1 className="mb-8 text-responsive-2xl font-semibold text-text-primary">
            {t('resetPasswordTitle')}
          </h1>
          <AuthErrorState
            title={t('verificationFailed')}
            description={t('invalidResetLink')}
            buttonText={t('requestNewResetLink')}
            onButtonClick={() => router.push('/forgot-password')}
          />
        </div>
      </div>
    );
  }

  // 提交处理
  const onSubmit = async (data: ResetPasswordFormData) => {
    // 使用 Server Action
    const result = await execute(resetPassword, {
      email: data.email,
      password: data.password,
      code,
    });

    if (result.success) {
      setIsSuccess(true);
    }
  };

  // 渲染表单
  const renderForm = () => (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      {/* 邮箱输入 */}
      <Input
        label={t('email')}
        type="email"
        placeholder={t('pleaseInput')}
        error={errors.email?.message}
        autoComplete="email"
        inputSize="md"
        {...register('email')}
      />

      {/* 新密码 */}
      <div>
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
        <p className="mt-2 text-xs text-text-secondary">
          {t('passwordHint')}
        </p>
      </div>

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

      {/* 提交按钮 */}
      <div className="mt-4">
        <Button type="submit" loading={isSubmitting || isLoading}>
          {t('resetPassword')}
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
        {isSuccess ? (
          <AuthSuccessState
            title={t('passwordResetSuccess')}
            description={t('passwordResetSuccessDesc')}
            onButtonClick={handleBackToLogin}
          />
        ) : renderForm()}
      </div>
    </div>
  );
}
