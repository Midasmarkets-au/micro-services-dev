'use client';

import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { useUserStore } from '@/stores/userStore';
import { changePassword, enable2FA, disable2FA, getUserInfo, getConfiguration } from '@/actions';
import { Input, Button } from '@/components/ui';
import type { UserInfo, SiteConfiguration } from '@/types/user';

// 表单验证 schema
const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'currentPasswordRequired'),
  newPassword: z
    .string()
    .min(1, 'newPasswordRequired')
    .min(8, 'newPasswordMinLength')
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).+$/,
      'newPasswordPattern'
    ),
  confirmPassword: z.string().min(1, 'confirmPasswordRequired'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'passwordMismatch',
  path: ['confirmPassword'],
});

type PasswordFormData = z.infer<typeof passwordSchema>;

export default function SecurityPage() {
  const t = useTranslations('profile.security');
  const tMessages = useTranslations('profile.security.messages');
  const tCommon = useTranslations('common');
  const { execute, isLoading } = useServerAction();
  const { showSuccess, showError } = useToast();
  
  const siteConfig = useUserStore((s) => s.siteConfig);
  const { setUser, setSiteConfig } = useUserStore();
  const [twoFactorEnabled, setTwoFactorEnabled] = useState(false);

  const refreshUserData = async () => {
    const [userResult, configResult] = await Promise.all([
      getUserInfo(),
      getConfiguration(),
    ]);
    if (userResult.success && userResult.data) {
      setUser(userResult.data as UserInfo);
    }
    if (configResult.success && configResult.data) {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      setSiteConfig(configResult.data as any as SiteConfiguration);
    }
  };

  // 当 siteConfig 数据加载完成后，同步 twoFactorAuth 状态
  useEffect(() => {
    console.log('siteConfig', siteConfig);
    if (siteConfig?.twoFactorAuth !== undefined) {
      setTwoFactorEnabled(siteConfig.twoFactorAuth);
    }
  }, [siteConfig?.twoFactorAuth]);
  const [showTwoFactorInput, setShowTwoFactorInput] = useState(false);
  const [twoFactorCode, setTwoFactorCode] = useState('');
  const [pendingTwoFactorAction, setPendingTwoFactorAction] = useState<'enable' | 'disable' | null>(null);
  const [twoFactorLoading, setTwoFactorLoading] = useState(false);
  const [showCodeSentMessage, setShowCodeSentMessage] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PasswordFormData>({
    resolver: zodResolver(passwordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  });

  // 提交表单
  const onSubmit = async (data: PasswordFormData) => {
    const result = await execute(() => 
      changePassword(data.currentPassword, data.newPassword)
    );

    if (result.success) {
      showSuccess(tMessages('passwordChanged'));
      reset();
    } else {
      showError(result.error || tCommon('error'));
    }
  };

  // 重置表单
  const handleReset = () => {
    reset();
  };

  // 点击双重认证开关 - 先发请求让后台发送验证码
  const handleTwoFactorToggle = async () => {
    const action = twoFactorEnabled ? 'disable' : 'enable';
    setPendingTwoFactorAction(action);
    setTwoFactorLoading(true);

    try {
      // 调用 API，code 为空，让后台发送验证码邮件
      const apiAction = action === 'enable' ? enable2FA : disable2FA;
      await execute(() => apiAction(''));

      // 显示验证码输入框和提示
      setShowTwoFactorInput(true);
      setTwoFactorCode('');
      setShowCodeSentMessage(true);
    } catch (error) {
      console.error('[2FA] Error:', error);
      showError(tCommon('error'));
    } finally {
      setTwoFactorLoading(false);
    }
  };

  // 验证双重认证
  const handleVerifyTwoFactor = async () => {
    if (!twoFactorCode.trim()) {
      showError(t('errors.codeRequired'));
      return;
    }

    setTwoFactorLoading(true);

    try {
      const action = pendingTwoFactorAction === 'enable' ? enable2FA : disable2FA;
      const result = await execute(() => action(twoFactorCode));

      if (result.success) {
        // 重新从后端 API 获取最新数据（与登录初始化逻辑一致）
        await refreshUserData();
        showSuccess(
          pendingTwoFactorAction === 'enable' 
            ? tMessages('twoFactorEnabled') 
            : tMessages('twoFactorDisabled')
        );
        // 重置状态
        setShowTwoFactorInput(false);
        setTwoFactorCode('');
        setPendingTwoFactorAction(null);
        setShowCodeSentMessage(false);
      } else {
        showError(result.error || tCommon('error'));
      }
    } finally {
      setTwoFactorLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center gap-10 md:gap-[80px] bg-surface rounded-xl px-4 py-10 md:px-[40px] md:py-[80px]">
      {/* 表单区域 */}
      <div className="flex flex-col items-center gap-10 w-full">
        {/* 标题 */}
        <h1 className="text-responsive-2xl md:text-responsive-3xl font-semibold text-text-primary">
          {t('title')}
        </h1>

        {/* 表单 */}
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5 w-full md:w-[335px]">
          {/* 旧密码 */}
          <Input
            type="password"
            label={t('currentPassword')}
            placeholder={t('currentPasswordPlaceholder')}
            required
            error={errors.currentPassword?.message ? t(`errors.${errors.currentPassword.message}`) : undefined}
            className="w-full"
            {...register('currentPassword')}
          />

          {/* 新密码 */}
          <Input
            type="password"
            label={t('newPassword')}
            placeholder={t('newPasswordPlaceholder')}
            required
            error={errors.newPassword?.message ? t(`errors.${errors.newPassword.message}`) : undefined}
            className="w-full"
            {...register('newPassword')}
          />

          {/* 确认密码 */}
          <Input
            type="password"
            label={t('confirmPassword')}
            placeholder={t('confirmPasswordPlaceholder')}
            required
            error={errors.confirmPassword?.message ? t(`errors.${errors.confirmPassword.message}`) : undefined}
            className="w-full"
            {...register('confirmPassword')}
          />
        </form>
      </div>

      {/* 双重认证 */}
      <div className="flex flex-col items-center gap-3">
        <div className="flex items-center gap-4 flex-wrap justify-center">
          {/* 开关 */}
          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={handleTwoFactorToggle}
              disabled={twoFactorLoading}
              className={`relative w-8 h-5 rounded-full transition-colors disabled:opacity-50 ${
                twoFactorEnabled 
                  ? 'bg-primary' 
                  : 'bg-[#ccc] dark:bg-[#444]'
              }`}
            >
              <span
                className={`absolute top-0.5 w-4 h-4 rounded-full bg-white transition-transform ${
                  twoFactorEnabled ? 'left-[14px]' : 'left-0.5'
                }`}
              />
            </button>
            <span className="text-sm font-medium text-[#333] dark:text-white">
              {t('twoFactor')}
            </span>
          </div>

          {/* 验证码输入区域 - 同一行 */}
          {showTwoFactorInput && (
            <div className="flex items-center gap-2">
              <input
                type="text"
                value={twoFactorCode}
                onChange={(e) => setTwoFactorCode(e.target.value)}
                placeholder={t('twoFactorCodePlaceholder')}
                className="w-[140px] h-10 px-3 rounded border border-border bg-surface text-sm text-text-primary placeholder:text-text-placeholder focus:outline-none focus:border-primary"
              />
              <Button
                type="button"
                onClick={handleVerifyTwoFactor}
                loading={twoFactorLoading}
                disabled={!twoFactorCode.trim()}
                className="w-[70px] h-10"
              >
                {t('verify')}
              </Button>
            </div>
          )}
        </div>

        {/* 验证码已发送提示 */}
        {showCodeSentMessage && (
          <p className="text-sm text-text-secondary">
            {tMessages('codeSent')}
          </p>
        )}
      </div>

      {/* 按钮区域 */}
      <div className="flex items-center gap-5">
        {/* 重置按钮 */}
        <Button
          type="button"
          variant="secondary"
          onClick={handleReset}
          className="w-[120px] h-[44px] bg-[#f8f8f8] dark:bg-[#2e2e2e] dark:border dark:border-[#333] text-[#333] dark:text-white"
        >
          {t('reset')}
        </Button>

        {/* 保存按钮 */}
        <Button
          type="submit"
          onClick={handleSubmit(onSubmit)}
          loading={isLoading}
          className="w-[120px] h-[44px]"
        >
          {t('save')}
        </Button>
      </div>
    </div>
  );
}
