'use client';

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import {
  Input,
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from '@/components/ui';
import { VerificationFormLayout, SingleStepNav } from './VerificationFormLayout';
import type { FinancialData } from '@/types/verification';

const financialSchema = z.object({
  annualIncome: z.string().optional(),
  netWorth: z.string().optional(),
  sourceOfFunds: z.string().optional(),
  employmentStatus: z.string().optional(),
  occupation: z.string().optional(),
});

type FinancialFormData = z.infer<typeof financialSchema>;

interface FinancialInfoFormProps {
  initialData?: Partial<FinancialData>;
  onSubmit: (data: Record<string, unknown>) => void;
  onBack: () => void;
  isLoading?: boolean;
}

// 收入选项
const incomeOptions = [
  { value: '0-25000', label: '$0 - $25,000' },
  { value: '25000-50000', label: '$25,000 - $50,000' },
  { value: '50000-100000', label: '$50,000 - $100,000' },
  { value: '100000-250000', label: '$100,000 - $250,000' },
  { value: '250000+', label: '$250,000+' },
];

// 净资产选项
const netWorthOptions = [
  { value: '0-50000', label: '$0 - $50,000' },
  { value: '50000-100000', label: '$50,000 - $100,000' },
  { value: '100000-500000', label: '$100,000 - $500,000' },
  { value: '500000-1000000', label: '$500,000 - $1,000,000' },
  { value: '1000000+', label: '$1,000,000+' },
];

// 资金来源选项
const fundSourceOptions = [
  { value: 'salary', label: '工资' },
  { value: 'savings', label: '储蓄' },
  { value: 'investment', label: '投资收益' },
  { value: 'inheritance', label: '继承' },
  { value: 'other', label: '其他' },
];

// 就业状态选项
const employmentOptions = [
  { value: 'employed', label: '在职' },
  { value: 'self-employed', label: '自雇' },
  { value: 'retired', label: '退休' },
  { value: 'student', label: '学生' },
  { value: 'unemployed', label: '无业' },
];

// 表单区域标题 - 移到组件外部避免重复创建
function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2">
      <div className="h-[18px] w-[3px] rounded-full bg-primary" />
      <h3 className="font-semibold text-base text-text-primary">{children}</h3>
    </div>
  );
}

// 表单字段标签 - 移到组件外部避免重复创建
function FieldLabel({ required, children }: { required?: boolean; children: React.ReactNode }) {
  return (
    <div className="flex items-center px-1 text-sm font-medium">
      {required && <span className="text-primary">*</span>}
      <span className="text-text-secondary">{children}</span>
    </div>
  );
}

export function FinancialInfoForm({ initialData, onSubmit, onBack, isLoading }: FinancialInfoFormProps) {
  const t = useTranslations('verification');

  const {
    control,
    handleSubmit,
  } = useForm<FinancialFormData>({
    resolver: zodResolver(financialSchema),
    defaultValues: {
      annualIncome: initialData?.annualIncome || '',
      netWorth: initialData?.netWorth || '',
      sourceOfFunds: initialData?.sourceOfFunds || '',
      employmentStatus: initialData?.employmentStatus || '',
      occupation: initialData?.occupation || '',
    },
  });

  // 左侧导航
  const stepNav = (
    <SingleStepNav stepNumber="01" label={t('financialInfo')} />
  );

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <VerificationFormLayout
        stepNav={stepNav}
        onBack={onBack}
        isLoading={isLoading}
      >
        <div className="flex flex-col gap-6 md:gap-10">
          <SectionTitle>{t('financialInfo')}</SectionTitle>

          <p className="text-sm text-text-secondary">
            {t('financialDescription')}
          </p>

          <div className="flex flex-col gap-4 md:gap-5">
            {/* 年收入/净资产 */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
              <div className="flex flex-col gap-2">
                <FieldLabel>{t('fields.annualIncome')}</FieldLabel>
                <Controller
                  name="annualIncome"
                  control={control}
                  render={({ field }) => (
                    <Select value={field.value || ''} onValueChange={field.onChange}>
                      <SelectTrigger>
                        <SelectValue placeholder={t('placeholders.selectIncome')} />
                      </SelectTrigger>
                      <SelectContent>
                        {incomeOptions.map((opt) => (
                          <SelectItem key={opt.value} value={opt.value}>
                            {opt.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
              <div className="flex flex-col gap-2">
                <FieldLabel>{t('fields.netWorth')}</FieldLabel>
                <Controller
                  name="netWorth"
                  control={control}
                  render={({ field }) => (
                    <Select value={field.value || ''} onValueChange={field.onChange}>
                      <SelectTrigger>
                        <SelectValue placeholder={t('placeholders.selectNetWorth')} />
                      </SelectTrigger>
                      <SelectContent>
                        {netWorthOptions.map((opt) => (
                          <SelectItem key={opt.value} value={opt.value}>
                            {opt.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
            </div>

            {/* 资金来源/就业状态 */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
              <div className="flex flex-col gap-2">
                <FieldLabel>{t('fields.sourceOfFunds')}</FieldLabel>
                <Controller
                  name="sourceOfFunds"
                  control={control}
                  render={({ field }) => (
                    <Select value={field.value || ''} onValueChange={field.onChange}>
                      <SelectTrigger>
                        <SelectValue placeholder={t('placeholders.selectFundSource')} />
                      </SelectTrigger>
                      <SelectContent>
                        {fundSourceOptions.map((opt) => (
                          <SelectItem key={opt.value} value={opt.value}>
                            {opt.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
              <div className="flex flex-col gap-2">
                <FieldLabel>{t('fields.employmentStatus')}</FieldLabel>
                <Controller
                  name="employmentStatus"
                  control={control}
                  render={({ field }) => (
                    <Select value={field.value || ''} onValueChange={field.onChange}>
                      <SelectTrigger>
                        <SelectValue placeholder={t('placeholders.selectEmployment')} />
                      </SelectTrigger>
                      <SelectContent>
                        {employmentOptions.map((opt) => (
                          <SelectItem key={opt.value} value={opt.value}>
                            {opt.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
            </div>

            {/* 职业 */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
              <div className="flex flex-col gap-2">
                <FieldLabel>{t('fields.occupation')}</FieldLabel>
                <Controller
                  name="occupation"
                  control={control}
                  render={({ field }) => (
                    <Input
                      placeholder={t('placeholders.occupation')}
                      value={field.value || ''}
                      onChange={field.onChange}
                    />
                  )}
                />
              </div>
              <div className="hidden md:block" /> {/* 占位 - 仅桌面端显示 */}
            </div>
          </div>
        </div>
      </VerificationFormLayout>
    </form>
  );
}
