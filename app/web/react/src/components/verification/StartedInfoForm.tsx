'use client';

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useEffect } from 'react';
import { SimpleSelect } from '@/components/ui';
import type { SelectOption } from '@/components/ui';
import { VerificationFormLayout, SingleStepNav } from './VerificationFormLayout';
import { type StartedData } from '@/types/verification';
const startedSchema = z.object({
  currency: z.string().min(1, 'required'),
  accountType: z.string().min(1, 'required'),
  serviceId: z.string().min(1, 'required'),
  leverage: z.string().min(1, 'required'),
  questions: z.object({
    q1: z.boolean(),
    q2: z.boolean(),
    q3: z.boolean(),
    q4: z.boolean(),
  }),
});

export type StartedFormData = z.infer<typeof startedSchema>;
export type StartedSubmitData = Omit<StartedFormData, 'currency' | 'accountType' | 'serviceId' | 'leverage'> & {
  currency: number;
  accountType: number;
  serviceId: number;
  leverage: number;
};

interface StartedInfoFormProps {
  initialData?: Partial<StartedData>;
  accountTypeOptions: SelectOption[];
  currencyOptions: SelectOption[];
  leverageOptions: SelectOption[];
  platformOptions: SelectOption[];
  onSubmit: (data: StartedSubmitData) => void;
  isLoading?: boolean;
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2">
      <div className="h-[18px] w-[3px] rounded-full bg-primary" />
      <h3 className="font-semibold text-base text-text-primary">{children}</h3>
    </div>
  );
}

function FieldLabel({ required, children }: { required?: boolean; children: React.ReactNode }) {
  return (
    <div className="flex items-center px-1 text-sm font-medium">
      {required && <span className="text-primary">*</span>}
      <span className="text-text-secondary">{children}</span>
    </div>
  );
}

interface QuestionSwitchProps {
  label: string;
  value: boolean;
  onChange: (value: boolean) => void;
  yesLabel: string;
  noLabel: string;
}

function QuestionSwitch({ label, value, onChange, yesLabel, noLabel }: QuestionSwitchProps) {
  return (
    <div className="flex flex-col gap-2 rounded-lg bg-surface-secondary p-3 md:p-4">
      <p className="text-sm text-text-primary">{label}</p>
      <div className="flex gap-2">
        <button
          type="button"
          onClick={() => onChange(true)}
          className={`rounded px-3 py-1 text-sm ${
            value ? 'bg-primary text-white' : 'bg-surface text-text-secondary'
          }`}
        >
          {yesLabel}
        </button>
        <button
          type="button"
          onClick={() => onChange(false)}
          className={`rounded px-3 py-1 text-sm ${
            !value ? 'bg-primary text-white' : 'bg-surface text-text-secondary'
          }`}
        >
          {noLabel}
        </button>
      </div>
    </div>
  );
}

export function StartedInfoForm({
  initialData,
  accountTypeOptions,
  currencyOptions,
  leverageOptions,
  platformOptions,
  onSubmit,
  isLoading,
}: StartedInfoFormProps) {
  const t = useTranslations('verification');
  const {
    control,
    handleSubmit,
    setValue,
    getValues,
    formState: { errors },
  } = useForm<StartedFormData>({
    resolver: zodResolver(startedSchema),
    defaultValues: {
      currency: initialData?.currency ? String(initialData.currency) : '',
      accountType: initialData?.accountType ? String(initialData.accountType) : '',
      serviceId: initialData?.serviceId ? String(initialData.serviceId) : '',
      leverage: initialData?.leverage ? String(initialData.leverage) : '',
      questions: {
        q1: initialData?.questions?.q1 ?? true,
        q2: initialData?.questions?.q2 ?? true,
        q3: initialData?.questions?.q3 ?? true,
        q4: initialData?.questions?.q4 ?? true,
      },
    },
  });

  useEffect(() => {
    const ensureDefaultValue = (
      field: 'currency' | 'accountType' | 'serviceId' | 'leverage',
      options: SelectOption[]
    ) => {
      const value = getValues(field);
      if (!value && options.length > 0) {
        setValue(field, options[0].value);
      }
    };

    ensureDefaultValue('currency', currencyOptions);
    ensureDefaultValue('accountType', accountTypeOptions);
    ensureDefaultValue('serviceId', platformOptions);
    ensureDefaultValue('leverage', leverageOptions);
  }, [
    accountTypeOptions,
    currencyOptions,
    leverageOptions,
    platformOptions,
    getValues,
    setValue,
  ]);

  const stepNav = <SingleStepNav stepNumber="01" label={t('mainSteps.gettingStarted')} />;

  const handleFormSubmit = (data: StartedFormData) => {
    onSubmit({
      ...data,
      currency: Number(data.currency),
      accountType: Number(data.accountType),
      serviceId: Number(data.serviceId),
      leverage: Number(data.leverage),
    });
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)}>
      <VerificationFormLayout stepNav={stepNav} onBack={() => {}} isLoading={isLoading} showBackButton={false}>
        <div className="flex flex-col gap-8 md:gap-10">
          <SectionTitle>{t('mainSteps.gettingStarted')}</SectionTitle>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
            <div className="flex flex-col gap-2">
              <FieldLabel required>{t('fields.currency')}</FieldLabel>
              <Controller
                name="currency"
                control={control}
                render={({ field }) => (
                  <SimpleSelect
                    value={field.value}
                    onChange={field.onChange}
                    options={currencyOptions}
                    placeholder={t('fields.currency')}
                    error={!!errors.currency}
                  />
                )}
              />
            </div>

            <div className="flex flex-col gap-2">
              <FieldLabel required>{t('fields.accountType')}</FieldLabel>
              <Controller
                name="accountType"
                control={control}
                render={({ field }) => (
                  <SimpleSelect
                    value={field.value}
                    onChange={field.onChange}
                    options={accountTypeOptions}
                    placeholder={t('fields.accountType')}
                    error={!!errors.accountType}
                  />
                )}
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
            <div className="flex flex-col gap-2">
              <FieldLabel required>{t('fields.platform')}</FieldLabel>
              <Controller
                name="serviceId"
                control={control}
                render={({ field }) => (
                  <SimpleSelect
                    value={field.value}
                    onChange={field.onChange}
                    options={platformOptions}
                    placeholder={t('fields.platform')}
                    error={!!errors.serviceId}
                  />
                )}
              />
            </div>

            <div className="flex flex-col gap-2">
              <FieldLabel required>{t('fields.leverage')}</FieldLabel>
              <Controller
                name="leverage"
                control={control}
                render={({ field }) => (
                  <SimpleSelect
                    value={field.value}
                    onChange={field.onChange}
                    options={leverageOptions}
                    placeholder={t('fields.leverage')}
                    error={!!errors.leverage}
                  />
                )}
              />
            </div>
          </div>

          {/* <div className="flex flex-col gap-4">
            <SectionTitle>{t('started.quizTitle')}</SectionTitle>
            <p className="text-sm text-text-secondary">{t('started.quizDescription')}</p>
            <Controller
              name="questions.q1"
              control={control}
              render={({ field }) => (
                <QuestionSwitch
                  label={t('started.questions.q1')}
                  value={field.value}
                  onChange={field.onChange}
                  yesLabel={t('started.answers.yes')}
                  noLabel={t('started.answers.no')}
                />
              )}
            />
            <Controller
              name="questions.q2"
              control={control}
              render={({ field }) => (
                <QuestionSwitch
                  label={t('started.questions.q2')}
                  value={field.value}
                  onChange={field.onChange}
                  yesLabel={t('started.answers.yes')}
                  noLabel={t('started.answers.no')}
                />
              )}
            />
            <Controller
              name="questions.q3"
              control={control}
              render={({ field }) => (
                <QuestionSwitch
                  label={t('started.questions.q3')}
                  value={field.value}
                  onChange={field.onChange}
                  yesLabel={t('started.answers.yes')}
                  noLabel={t('started.answers.no')}
                />
              )}
            />
            <Controller
              name="questions.q4"
              control={control}
              render={({ field }) => (
                <QuestionSwitch
                  label={t('started.questions.q4')}
                  value={field.value}
                  onChange={field.onChange}
                  yesLabel={t('started.answers.yes')}
                  noLabel={t('started.answers.no')}
                />
              )}
            />
          </div> */}
        </div>
      </VerificationFormLayout>
    </form>
  );
}
