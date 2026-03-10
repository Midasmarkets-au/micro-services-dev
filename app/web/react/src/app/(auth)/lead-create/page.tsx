'use client';

import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useState, useMemo } from 'react';
import { Input, Button } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { SearchableSelect } from '@/components/ui';
import { getRegionCodes } from '@/core/data/phonesData';
import { useServerAction } from '@/hooks/useServerAction';
import { createLead } from '@/actions';

interface CountryOption {
  value: string;
  label: string;
  code: string;
}

export default function LeadCreatePage() {
  const t = useTranslations('auth');
  const tLead = useTranslations('lead');
  const router = useRouter();
  const { execute, isLoading } = useServerAction();
  
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [selectedCountry, setSelectedCountry] = useState<CountryOption | null>(null);

  // 获取国家/地区代码列表
  const regionCodes = useMemo(() => getRegionCodes(), []);
  const countryOptions: CountryOption[] = useMemo(() => 
    Object.entries(regionCodes).map(([code, data]) => ({
      value: `+${data.dialCode}`,
      label: data.name,
      code: code,
    })),
    [regionCodes]
  );

  // 表单验证 schema
  const leadSchema = z.object({
    name: z
      .string()
      .min(1, tLead('nameRequired')),
    email: z
      .string()
      .min(1, t('emailRequired'))
      .email(t('emailInvalid')),
    phoneNumber: z.string().optional(),
    note: z
      .string()
      .min(1, tLead('noteRequired')),
  });

  type LeadFormData = z.infer<typeof leadSchema>;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<LeadFormData>({
    resolver: zodResolver(leadSchema),
  });

  // 处理表单提交
  const onSubmit = async (data: LeadFormData) => {
    // 组合电话号码
    const countryCode = selectedCountry?.value || '';
    const phoneNumber = countryCode && data.phoneNumber 
      ? `${countryCode}${data.phoneNumber}` 
      : data.phoneNumber || '';

    // 使用 Server Action
    const result = await execute(createLead, {
      name: data.name,
      email: data.email,
      phoneNumber,
      note: data.note,
    });

    if (!result.success) {
      return;
    }

    // 显示成功消息
    setSuccessMessage(tLead('submitSuccess'));
    reset();
    setSelectedCountry(null);

    // 3秒后跳转
    setTimeout(() => {
      router.push('/sign-in');
    }, 3000);
  };

  return (
    <div className="auth-card card overflow-hidden">
      {/* 左侧插图 */}
      <div className="auth-illustration-container">
        <AuthIllustration />
      </div>
      {/* 右侧表单 */}
      <div className="auth-card-form flex flex-col">
        {/* 标题 */}
        <h1 className="mb-10 text-3xl font-bold text-text-primary">
          {tLead('title')}
        </h1>

        {/* 成功消息 */}
        {successMessage && (
          <div className="success-banner mb-6">
            {successMessage}
          </div>
        )}

        {/* 错误消息 */}
        {errors.root && (
          <div className="error-banner mb-6">
            {errors.root.message}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
          {/* 第一行：姓名和邮箱 */}
          <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
            <Input
              label={tLead('fullName')}
              placeholder={tLead('fullNamePlaceholder')}
              error={errors.name?.message}
              errorPosition="bottom"
              inputSize="md"
              {...register('name')}
            />
            <Input
              label={t('email')}
              type="email"
              placeholder={t('emailPlaceholder')}
              error={errors.email?.message}
              errorPosition="bottom"
              inputSize="md"
              {...register('email')}
            />
          </div>

          {/* 第二行：国家代码和电话号码 */}
          <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
            <div className="w-full">
              <label className="input-label">{tLead('country')}</label>
              <SearchableSelect
                options={countryOptions}
                value={selectedCountry}
                onChange={(option) => setSelectedCountry(option as CountryOption | null)}
                placeholder={tLead('selectCountry')}
              />
            </div>
            <div className="grid grid-cols-3 gap-2">
              <div className="col-span-1">
                <label className="input-label">&nbsp;</label>
                <input
                  type="text"
                  value={selectedCountry?.value || ''}
                  readOnly
                  className="input-field text-center"
                  placeholder="+00"
                />
              </div>
              <div className="col-span-2">
                <Input
                  label={tLead('phone')}
                  type="tel"
                  placeholder={tLead('phonePlaceholder')}
                  inputSize="md"
                  {...register('phoneNumber')}
                />
              </div>
            </div>
          </div>

          {/* 备注 */}
          <div className="w-full">
            <label className="input-label">{tLead('notes')}</label>
            <textarea
              className={`input-field min-h-[100px] resize-none py-3 ${errors.note ? 'error-border' : ''}`}
              placeholder={tLead('notesPlaceholder')}
              {...register('note')}
            />
            {errors.note && (
              <p className="error-text mt-1 text-sm">{errors.note.message}</p>
            )}
          </div>

          {/* 提交按钮 */}
          <Button
            type="submit"
            loading={isSubmitting || isLoading}
            disabled={isSubmitting || isLoading}
            className="mt-4"
          >
            {tLead('sendToUs')}
          </Button>

          {/* 返回登录链接 */}
          <p className="mt-2 text-center text-sm text-text-secondary">
            <Link href="/sign-in" className="link font-medium underline">
              {t('backToLogin')}
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
