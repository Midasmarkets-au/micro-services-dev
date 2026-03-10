'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useServerAction } from '@/hooks/useServerAction';
import { useUserStore } from '@/stores/userStore';
import { Input, Button } from '@/components/ui';
import { submitContact } from '@/actions';

// 表单验证 schema
const contactSchema = z.object({
  name: z.string().min(1, 'nameRequired'),
  email: z.string().email('emailInvalid'),
  subject: z.string().min(1, 'subjectRequired'),
  message: z.string().min(1, 'messageRequired'),
});

type ContactFormData = z.infer<typeof contactSchema>;

export default function ContactPage() {
  const t = useTranslations('supports');
  const tCommon = useTranslations('common');
  const { execute, isLoading } = useServerAction();
  const user = useUserStore((s) => s.user);
  
  const [submitSuccess, setSubmitSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ContactFormData>({
    resolver: zodResolver(contactSchema),
    defaultValues: {
      name: user?.name || '',
      email: user?.email || '',
      subject: '',
      message: '',
    },
  });

  const onSubmit = async (data: ContactFormData) => {
    // 使用 Server Action
    const result = await execute(submitContact, {
      name: data.name,
      email: data.email,
      subject: data.subject,
      message: data.message,
    });
    
    if (result.success) {
      setSubmitSuccess(true);
      reset();
      // 3秒后重置成功状态
      setTimeout(() => setSubmitSuccess(false), 3000);
    }
  };

  return (
    <div className="flex flex-col items-center">
      <h2 className="text-xl font-semibold text-text-primary mb-8">{t('onlineInquiry')}</h2>
      
      {submitSuccess ? (
        <div className="flex flex-col items-center gap-4 py-10">
          <div className="size-16 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
            <svg className="size-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <p className="text-text-primary font-medium">{t('submitSuccess')}</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className="w-full max-w-2xl space-y-5">
          {/* 姓名和邮箱 */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
            <div>
              <label className="block text-sm text-text-secondary mb-2">
                <span className="text-primary">*</span>{t('form.name')}
              </label>
              <Input
                {...register('name')}
                placeholder={t('form.namePlaceholder')}
                className="w-full dark:bg-surface-secondary dark:border-[#333]"
                error={errors.name?.message ? t(`form.errors.${errors.name.message}`) : undefined}
              />
            </div>
            <div>
              <label className="block text-sm text-text-secondary mb-2">
                <span className="text-primary">*</span>{t('form.email')}
              </label>
              <Input
                {...register('email')}
                type="email"
                placeholder={t('form.emailPlaceholder')}
                className="w-full dark:bg-surface-secondary dark:border-[#333]"
                error={errors.email?.message ? t(`form.errors.${errors.email.message}`) : undefined}
              />
            </div>
          </div>

          {/* 主题 */}
          <div>
            <label className="block text-sm text-text-secondary mb-2">
              <span className="text-primary">*</span>{t('form.subject')}
            </label>
            <Input
              {...register('subject')}
              placeholder={t('form.subjectPlaceholder')}
              className="w-full dark:bg-surface-secondary dark:border-[#333]"
              error={errors.subject?.message ? t(`form.errors.${errors.subject.message}`) : undefined}
            />
          </div>

          {/* 查询内容 */}
          <div>
            <label className="block text-sm text-text-secondary mb-2">
              <span className="text-primary">*</span>{t('form.message')}
            </label>
            <textarea
              {...register('message')}
              rows={5}
              placeholder={t('form.messagePlaceholder')}
              className={`input-field h-auto! py-[14px]! dark:bg-surface-secondary dark:border-[#333] ${errors.message ? 'error-border' : ''}`}
            />
            {errors.message && (
              <p className="mt-1 text-sm text-red-500">{t(`form.errors.${errors.message.message}`)}</p>
            )}
          </div>

          {/* 提交按钮 */}
          <div className="flex justify-center pt-5">
            <Button
              type="submit"
              disabled={isSubmitting || isLoading}
              className="w-full max-w-xs"
            >
              {isSubmitting || isLoading ? tCommon('loading') : tCommon('submit')}
            </Button>
          </div>
        </form>
      )}
    </div>
  );
}
