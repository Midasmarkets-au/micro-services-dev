'use client';

import { useMemo } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations, useLocale } from 'next-intl';
import { VerificationFormLayout, SingleStepNav } from './VerificationFormLayout';
import { getVerificationDocuments } from '@/core/data/bcrDocs';
import type { AgreementData } from '@/types/verification';

const agreementSchema = z.object({
  documentConfirmation: z.boolean().refine((val) => val === true, 'required'),
  electronicIdConsent: z.enum(['agree', 'disagree']),
});

type AgreementFormData = z.infer<typeof agreementSchema>;

interface AgreementFormProps {
  initialData?: Partial<AgreementData>;
  onSubmit: (data: Record<string, unknown>) => void;
  onBack: () => void;
  isLoading?: boolean;
  showIbDocuments?: boolean;
}

// 表单区域标题 - 移到组件外部避免重复创建
function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2">
      <div className="h-[18px] w-[3px] rounded-full bg-primary" />
      <h3 className="font-semibold text-base md:text-lg text-text-primary">{children}</h3>
    </div>
  );
}

// 复选框图标
function CheckIcon({ checked }: { checked: boolean }) {
  if (!checked) {
    return (
      <div className="size-4 rounded border-2 border-text-secondary shrink-0" />
    );
  }
  return (
    <div className="size-4 rounded bg-primary shrink-0 flex items-center justify-center">
      <svg width="10" height="8" viewBox="0 0 10 8" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M1 4L3.5 6.5L9 1" stroke="white" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
      </svg>
    </div>
  );
}

// 单选框图标
function RadioIcon({ checked }: { checked: boolean }) {
  if (!checked) {
    return (
      <div className="size-4 rounded-full border-2 border-text-secondary shrink-0" />
    );
  }
  return (
    <div className="size-4 rounded-full border-2 border-primary shrink-0 flex items-center justify-center">
      <div className="size-2 rounded-full bg-primary" />
    </div>
  );
}

export function AgreementForm({ initialData, onSubmit, onBack, isLoading, showIbDocuments = false }: AgreementFormProps) {
  const t = useTranslations('verification');
  const locale = useLocale();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<AgreementFormData>({
    resolver: zodResolver(agreementSchema),
    defaultValues: {
      documentConfirmation: initialData?.consent_1 || true,
      // 当前 UI 仅保留“同意”选项，默认与旧逻辑一致为同意
      electronicIdConsent: 'agree',
    },
  });

  // 获取文档列表
  const documentItems = useMemo(() => {
    return getVerificationDocuments(locale, showIbDocuments);
  }, [locale, showIbDocuments]);

  const onFormSubmit = (data: AgreementFormData) => {
    onSubmit({
      consent_1: data.documentConfirmation,
      consent_2: data.electronicIdConsent === 'agree',
      consent_3: false,
    });
  };

  // 左侧导航
  const stepNav = (
    <SingleStepNav stepNumber="01" label={t('agreement')} />
  );

  return (
    <form onSubmit={handleSubmit(onFormSubmit)}>
      <VerificationFormLayout
        stepNav={stepNav}
        onBack={onBack}
        isLoading={isLoading}
      >
        <div className="flex flex-col gap-8 md:gap-10">
          {/* 协议区域 */}
          <div className="flex flex-col gap-6 md:gap-10">
            <SectionTitle>{t('agreement')}</SectionTitle>

            {/* 文档确认复选框 + 列表 */}
            <div className="flex flex-col gap-4 md:gap-5">
              {/* 复选框行 */}
              <Controller
                name="documentConfirmation"
                control={control}
                render={({ field }) => (
                  <div
                    className="flex items-center gap-2.5 cursor-pointer"
                    onClick={() => field.onChange(!field.value)}
                  >
                    <CheckIcon checked={field.value} />
                    <p className={`text-sm md:text-base leading-relaxed ${
                      field.value ? 'text-text-primary' : 'text-text-secondary'
                    }`}>
                      {t('agreements.documentConfirmation')}
                    </p>
                  </div>
                )}
              />

              {/* 文档列表 */}
              <div className="bg-surface-secondary rounded-lg px-3 md:px-4 py-4 md:py-5">
                <ul className="flex flex-col gap-4 md:gap-5 text-sm md:text-base text-text-primary">
                  {documentItems.map((doc) => (
                    <li key={doc.key} className="flex items-center gap-2">
                      <span className="text-text-primary">•</span>
                      <a
                        href={doc.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-text-primary hover:underline font-medium"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {t(`agreements.documentList.${doc.title}`)}
                      </a>
                    </li>
                  ))}
                </ul>
              </div>

              {/* 错误提示 */}
              {errors.documentConfirmation && (
                <p className="text-sm error-text">
                  {t('errors.agreementRequired')}
                </p>
              )}
            </div>
          </div>

          {/* 电子身份证件区域 */}
          <div className="flex flex-col gap-4 md:gap-5">
            {/* 标题 */}
            <div className="flex items-center gap-1 px-1">
              <span className="text-primary">*</span>
              <span className="font-medium text-sm md:text-base text-text-primary">
                {t('agreements.electronicIdTitle')}
              </span>
            </div>

            {/* 描述 */}
            <div className="px-3 md:px-4">
              <p className="text-xs md:text-sm text-text-secondary">
                {t('agreements.electronicIdDescription')}
              </p>
            </div>

            {/* 选项卡片 */}
            <div className="flex flex-col gap-4 md:gap-5">
              {/* 同意选项 */}
              <Controller
                name="electronicIdConsent"
                control={control}
                render={({ field }) => (
                  <div
                    className={`flex items-center gap-2.5 p-4 md:p-5 rounded-lg cursor-pointer transition-all ${
                      field.value === 'agree'
                        ? 'bg-surface-secondary border border-primary'
                        : 'bg-surface-secondary border border-transparent hover:border-border'
                    }`}
                    onClick={() => field.onChange('agree')}
                  >
                    <RadioIcon checked={field.value === 'agree'} />
                    <p className="text-xs md:text-sm text-text-secondary leading-relaxed flex-1">
                      {t('agreements.agreeElectronicId')}
                    </p>
                  </div>
                )}
              />

              {/* 不同意选项 */}
              {/* <Controller
                name="electronicIdConsent"
                control={control}
                render={({ field }) => (
                  <div
                    className={`flex flex-col gap-1 p-4 md:p-5 rounded-lg cursor-pointer transition-all ${
                      field.value === 'disagree'
                        ? 'bg-surface-secondary border border-primary'
                        : 'bg-surface-secondary border border-transparent hover:border-border'
                    }`}
                    onClick={() => field.onChange('disagree')}
                  >
                    <div className="flex items-center gap-2.5">
                      <RadioIcon checked={field.value === 'disagree'} />
                      <div className="flex flex-col gap-1 flex-1">
                        <p className="text-sm md:text-base text-text-secondary">
                          {t('agreements.disagreeElectronicId')}
                        </p>
                        <p className="text-xs text-primary">
                          {t('agreements.disagreeElectronicIdNote')}
                        </p>
                      </div>
                    </div>
                  </div>
                )}
              /> */}
            </div>
          </div>
        </div>
      </VerificationFormLayout>
    </form>
  );
}
