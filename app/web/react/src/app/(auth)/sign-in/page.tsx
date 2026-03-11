'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useState, useCallback } from 'react';
import Image from 'next/image';
import { Input, Button } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { useServerAction } from '@/hooks/useServerAction';
import { login, forgotPassword, resendConfirmation } from '@/actions';

// 页面状态类型
type PageState = 'LoginPage' | 'ForgetPasswordPage' | 'EmailVerifyPage' | 'SelectTenantPage';
type TabType = 'login' | 'register';

// 租户选项接口
interface TenantOption {
  id: string | number;
  name: string;
  flag?: string;
}

// 租户国旗图片映射
const tenantFlagMap: Record<number, string> = {
  1: '/images/flags/au.svg',
  10000: '/images/flags/earth.svg',
  10002: '/images/flags/mn.svg',
  10004: '/images/flags/vn.svg',
  10005: '/images/flags/jp.svg',
};

export default function SignInPage() {
  const t = useTranslations('auth');
  const router = useRouter();
  const searchParams = useSearchParams();
  const { execute, isLoading } = useServerAction();

  // 页面状态管理
  const [showPage, setShowPage] = useState<PageState>('LoginPage');
  const [twoFaRequired, setTwoFaRequired] = useState(false);
  const [twoFaCode, setTwoFaCode] = useState('');
  const [tenantsOptions, setTenantsOptions] = useState<TenantOption[]>([]);
  const [selectedTenant, setSelectedTenant] = useState<string | number | null>(null);
  const [formEmail, setFormEmail] = useState('');
  const [formPassword, setFormPassword] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // 登录表单验证 schema
  const loginSchema = z.object({
    email: z
      .string()
      .min(1, t('emailRequired'))
      .email(t('emailInvalid')),
    password: z
      .string()
      .min(1, t('passwordRequired'))
      .min(6, t('passwordMinLength')),
  });

  // 忘记密码表单验证 schema
  const forgotPasswordSchema = z.object({
    email: z
      .string()
      .min(1, t('emailRequired'))
      .email(t('emailInvalid')),
  });

  type LoginFormData = z.infer<typeof loginSchema>;
  type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

  // 登录表单
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    clearErrors,
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  // 忘记密码表单
  const {
    register: registerForgot,
    handleSubmit: handleSubmitForgot,
    formState: { errors: forgotErrors, isSubmitting: isForgotSubmitting },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  // 处理登录
  const handleLogin = useCallback(async (
    email: string,
    password: string,
    tenantId?: string | number | null,
    tfCode?: string
  ) => {
    clearErrors();
    
    // 使用 Server Action
    const result = await execute(login, {
      email,
      password,
      rememberMe: true,
      tenantId: tenantId || undefined,
      twoFaCode: tfCode || undefined,
    });

    // 处理双因素认证
    if (result.twoFactorRequired) {
      setTwoFaRequired(true);
      setFormEmail(email);
      setFormPassword(password);
      return;
    }

    // 处理多租户
    if (result.hasMultipleTenants && result.tenantIds) {
      setTenantsOptions(result.tenantIds.map((id: number) => ({
        id,
        name: t(`tenants.${id}`) || `Tenant ${id}`,
        flag: tenantFlagMap[Number(id)],
      })));
      setFormEmail(email);
      setFormPassword(password);
      setShowPage('SelectTenantPage');
      return;
    }

    if (!result.success) {
      // 处理特殊错误码（需要页面跳转或特殊 UI）
      if (result.errorCode === '__EMAIL_UNCONFIRMED__') {
        setFormEmail(email);
        setFormPassword(password);
        setShowPage('EmailVerifyPage');
        return;
      }
      // 其他错误已通过 Toast 弹窗显示
      return;
    }

    // 登录成功，跳转到仪表盘
    router.push('/dashboard');
    router.refresh();
  }, [clearErrors, execute, router, t]);

  // 表单提交
  const onSubmit = async (data: LoginFormData) => {
    await handleLogin(data.email, data.password, selectedTenant, twoFaCode || undefined);
  };

  // 处理2FA提交
  const handleTwoFaSubmit = async () => {
    if (!twoFaCode) return;
    await handleLogin(formEmail, formPassword, selectedTenant, twoFaCode);
  };

  // 处理租户选择
  const handleTenantSelect = async (tenantId: string | number) => {
    setSelectedTenant(tenantId);
    await handleLogin(formEmail, formPassword, tenantId, twoFaCode || undefined);
  };

  // 处理忘记密码提交
  const onForgotPasswordSubmit = async (data: ForgotPasswordFormData) => {
    const resetUrl = `${window.location.protocol}//${window.location.host}/reset-password`;
    
    // 使用 Server Action
    const result = await execute(forgotPassword, {
      email: data.email,
      resetUrl,
    });

    if (!result.success) {
      return;
    }

    // 成功发送重置链接
    setSuccessMessage(t('passwordResetLinkSent'));
    setTimeout(() => {
      setShowPage('LoginPage');
      setSuccessMessage(null);
    }, 3000);
  };

  // 处理重发确认邮件
  const handleResendConfirmation = async () => {
    const confirmUrl = `${window.location.protocol}//${window.location.host}/confirm-email`;
    
    // 使用 Server Action
    const result = await execute(resendConfirmation, {
      email: formEmail,
      confirmUrl,
    });

    if (result.success) {
      setSuccessMessage(t('confirmationEmailResend'));
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleTabChange = (tab: TabType) => {
    if (tab === 'register') {
      router.push('/sign-up');
    }
  };

  // 渲染登录表单
  const renderLoginForm = () => (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      {/* 会话过期提示 */}
      {searchParams.get('expired') === 'true' && (
        <div className="error-banner animate-fade-in">
          {t('sessionExpired')}
        </div>
      )}

      {/* 成功提示 */}
      {(searchParams.get('registered') === 'true' || successMessage) && (
        <div className="success-banner animate-fade-in">
          {successMessage || t('registerSuccess')}
        </div>
      )}

      {/* 错误提示 */}
      {errors.root && (
        <div className="error-banner animate-fade-in">
          {errors.root.message}
        </div>
      )}

      {/* 邮箱 */}
      <Input
        label={t('email')}
        labelClassName="text-base font-medium text-text-secondary"
        type="email"
        placeholder={t('pleaseInput')}
        error={errors.email?.message}
        autoComplete="email"
        inputSize="md"
        {...register('email')}
      />

      {/* 密码 */}
      <div>
        <Input
          label={t('password')}
          labelClassName="text-base font-medium text-text-secondary"
          type="password"
          placeholder={t('pleaseInput')}
          error={errors.password?.message}
          showPasswordToggle
          autoComplete="current-password"
          inputSize="md"
          {...register('password')}
        />
        <div className="mt-2 text-right">
          <button
            type="button"
            onClick={() => setShowPage('ForgetPasswordPage')}
            className="text-sm text-text-link transition-colors hover:underline"
          >
            {t('forgotPassword')}
          </button>
        </div>
      </div>

      {/* 双因素认证输入框 */}
      {twoFaRequired && (
        <div className="animate-fade-in">
          <Input
            label={t('twoFactorAuthentication')}
            labelClassName="text-base font-medium text-text-secondary"
            type="text"
            placeholder={t('pleaseInput')}
            value={twoFaCode}
            onChange={(e) => setTwoFaCode(e.target.value)}
            autoComplete="one-time-code"
            inputSize="md"
          />
          <p className="mt-2 text-sm text-text-secondary">
            {t('pleaseEnterTwoFactorCode')}
          </p>
        </div>
      )}

      {/* 登录按钮 */}
      <div className="mt-8">
        {twoFaRequired ? (
          <Button type="button" className="w-full" loading={isSubmitting || isLoading} onClick={handleTwoFaSubmit}>
            {t('login')}
          </Button>
        ) : (
          <Button type="submit" className="w-full" loading={isSubmitting || isLoading}>
            {t('login')}
          </Button>
        )}
      </div>
    </form>
  );

  // 渲染忘记密码表单
  const renderForgotPasswordForm = () => (
    <form onSubmit={handleSubmitForgot(onForgotPasswordSubmit)} className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-text-primary">
        {t('resetPasswordTitle')}
      </h2>

      {/* 成功提示 */}
      {successMessage && (
        <div className="success-banner animate-fade-in">
          {successMessage}
        </div>
      )}

      {/* 错误提示 */}
      {forgotErrors.root && (
        <div className="error-banner animate-fade-in">
          {forgotErrors.root.message}
        </div>
      )}

      {/* 邮箱 */}
      <Input
        label={t('email')}
        labelClassName="text-base font-medium text-text-secondary"
        type="email"
        placeholder={t('pleaseInput')}
        error={forgotErrors.email?.message}
        autoComplete="email"
        {...registerForgot('email')}
      />

      {/* 发送按钮 */}
      <div className="mt-4">
        <Button type="submit" className="w-full" loading={isForgotSubmitting || isLoading}>
          {t('send')}
        </Button>
      </div>

      {/* 返回登录 */}
      <div className="text-center">
        <button
          type="button"
          onClick={() => setShowPage('LoginPage')}
          className="text-sm text-text-link transition-colors hover:underline"
        >
          {t('backToLogin')}
        </button>
      </div>
    </form>
  );

  // 渲染邮箱验证页面
  const renderEmailVerifyPage = () => (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-text-primary">
        {t('verifyYourEmail')}
      </h2>

      {/* 成功提示 */}
      {successMessage && (
        <div className="success-banner animate-fade-in">
          {successMessage}
        </div>
      )}

      <div className="text-text-secondary">
        <p>
          {t('thankSignUpAndConfirm')}{' '}
          <span className="font-medium text-text-primary">{formEmail}</span>{' '}
          {t('toActivateYourAccount')}
        </p>
        <p className="mt-4">{t('linkExpireContact')}</p>
      </div>

      {/* 重发邮件 */}
      <div className="mt-4">
        <p className="text-sm text-text-secondary">
          {t('resendEmail').replace('Resend Email', '')}
          <button
            type="button"
            onClick={handleResendConfirmation}
            className="text-text-link transition-colors hover:underline"
          >
            {t('resendEmail')}
          </button>
        </p>
      </div>

      {/* 返回登录 */}
      <div className="mt-4 text-center">
        <button
          type="button"
          onClick={() => setShowPage('LoginPage')}
          className="text-sm text-text-link transition-colors hover:underline"
        >
          {t('backToLogin')}
        </button>
      </div>
    </div>
  );

  // 渲染租户选择页面
  const renderSelectTenantPage = () => (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl">
        {t('selectTenant')}
      </h2>

      {/* 租户选择卡片 */}
      <div className="grid grid-cols-3 gap-4">
        {tenantsOptions.map((tenant) => (
          <button
            key={tenant.id}
            type="button"
            onClick={() => handleTenantSelect(tenant.id)}
            className="flex aspect-3/4 cursor-pointer flex-col items-center justify-center gap-3 rounded border border-[rgba(26,29,33,0.08)] bg-white transition-all hover:shadow-md dark:border-[#333] dark:bg-[#171717]"
          >
            {tenant.flag && (
              <Image 
                src={tenant.flag} 
                alt="" 
                width={56} 
                height={56} 
                className="rounded-full"
              />
            )}
            <span>
              {tenant.name}
            </span>
          </button>
        ))}
      </div>

      {/* 双因素认证输入框（在租户选择页面也可能需要） */}
      {twoFaRequired && (
        <div className="animate-fade-in">
          <Input
            label={t('twoFactorAuthentication')}
            labelClassName="text-base font-medium text-text-secondary"
            type="text"
            placeholder={t('pleaseInput')}
            value={twoFaCode}
            onChange={(e) => setTwoFaCode(e.target.value)}
            autoComplete="one-time-code"
            inputSize="md"
          />
          <p className="mt-2 text-sm text-text-secondary">
            {t('pleaseEnterTwoFactorCode')}
          </p>
          <div className="mt-4">
            <Button type="button" className="w-full" loading={isSubmitting || isLoading} onClick={handleTwoFaSubmit}>
              {t('login')}
            </Button>
          </div>
        </div>
      )}

      {/* 返回登录 */}
      <div className="mt-4 text-center">
        <button
          type="button"
          onClick={() => {
            setShowPage('LoginPage');
            setSelectedTenant(null);
            setTenantsOptions([]);
          }}
          className="text-sm text-text-link transition-colors hover:underline"
        >
          {t('backToLogin')}
        </button>
      </div>
    </div>
  );

  // 根据页面状态渲染内容
  const renderContent = () => {
    switch (showPage) {
      case 'ForgetPasswordPage':
        return renderForgotPasswordForm();
      case 'EmailVerifyPage':
        return renderEmailVerifyPage();
      case 'SelectTenantPage':
        return renderSelectTenantPage();
      default:
        return renderLoginForm();
    }
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
        <h1 className="mb-8 text-responsive-2xl font-semibold text-text-primary text-center">
          {t('welcomeLogin')}
        </h1>
        {/* 移动端：标题下方 120px 动画区域，桌面端由左侧插图展示 */}
        <div className="mb-6 flex justify-center md:hidden">
          <AuthIllustration size={140} />
        </div>
        {/* Tab 切换 - 仅在登录页面显示 */}
        {showPage === 'LoginPage' && (
          <div className="relative mb-10 flex gap-6">
            <button
              type="button"
              className="tab-item active"
            >
              {t('login')}
            </button>
            <button
              type="button"
              onClick={() => handleTabChange('register')}
              className="tab-item"
            >
              {t('register')}
            </button>
            {/* 底部分割线 */}
            <div className="absolute bottom-0 left-0 right-0 divider" />
          </div>
        )}

        {/* 动态内容区域 */}
        {renderContent()}

        {/* 注册链接 - 仅在登录页面显示 */}
        {showPage === 'LoginPage' && (
          <>
            <p className="mt-6 text-center text-sm text-text-secondary">
              {t('noAccount')}{' '}
              <Link href="/sign-up" className="link font-medium underline">
                {t('register')}
              </Link>
            </p>
          </>
        )}
      </div>
    </div>
  );
}
