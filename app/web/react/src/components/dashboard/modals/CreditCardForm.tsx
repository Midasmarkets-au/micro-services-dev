'use client';

import { forwardRef, useImperativeHandle, useMemo, useState, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { Input, SimpleSelect, SearchableSelect } from '@/components/ui';
import { getRegionCodes } from '@/core/data/phonesData';

export interface CreditCardFormHandle {
  /** 触发校验，校验通过返回处理后的字段（含 N/A 兜底），未通过返回 null */
  validate: () => Record<string, string> | null;
}

export interface CreditCardFormProps {
  value: Record<string, string>;
  onChange: (key: string, value: string) => void;
  disabled?: boolean;
}

interface FieldConfig {
  required?: boolean;
  maxLength?: number;
  /** 当 billingCountry === 'us' 时才必填 */
  requiredWhenUS?: boolean;
}

const FIELD_RULES: Record<string, FieldConfig> = {
  ccNumber: { required: true },
  billingFirstName: { required: true },
  ccMonth: { required: true },
  ccYear: { required: true },
  ccCvc: { required: true, maxLength: 3 },
  billingAddress: { required: true, maxLength: 200 },
  billingCity: { required: true, maxLength: 100 },
  billingState: { requiredWhenUS: true, maxLength: 3 },
  billingZipcode: { required: true, maxLength: 12 },
  billingCountry: { required: true },
  billingPhone: { required: true, maxLength: 25 },
};

const MONTH_OPTIONS = Array.from({ length: 12 }, (_, i) => {
  const v = String(i + 1).padStart(2, '0');
  return { value: v, label: String(i + 1) };
});

export const CreditCardForm = forwardRef<CreditCardFormHandle, CreditCardFormProps>(
  function CreditCardForm({ value, onChange, disabled }, ref) {
    const t = useTranslations('deposit.creditCard');
    const tDeposit = useTranslations('deposit');
    const [errors, setErrors] = useState<Record<string, string>>({});

    // 国家选项
    const countryOptions = useMemo(() => {
      const regions = getRegionCodes();
      return Object.entries(regions).map(([code, country]) => ({
        value: code,
        label: country.name,
      }));
    }, []);

    // 年份选项 - 当前年起 20 年
    const yearOptions = useMemo(() => {
      const currentYear = new Date().getFullYear();
      return Array.from({ length: 20 }, (_, i) => {
        const y = String(currentYear + i);
        return { value: y, label: y };
      });
    }, []);

    const setError = useCallback((key: string, msg: string) => {
      setErrors((prev) => ({ ...prev, [key]: msg }));
    }, []);

    const clearError = useCallback((key: string) => {
      setErrors((prev) => {
        if (!prev[key]) return prev;
        const next = { ...prev };
        delete next[key];
        return next;
      });
    }, []);

    const handleFieldChange = useCallback(
      (key: string, val: string) => {
        onChange(key, val);
        if (errors[key]) clearError(key);
      },
      [onChange, errors, clearError]
    );

    useImperativeHandle(ref, () => ({
      validate: () => {
        const newErrors: Record<string, string> = {};
        const country = (value.billingCountry || '').toLowerCase();

        for (const [key, rule] of Object.entries(FIELD_RULES)) {
          const v = (value[key] ?? '').toString().trim();

          // 必填校验（含按国家条件必填）
          const isRequired = rule.required || (rule.requiredWhenUS && country === 'us');
          if (isRequired && !v) {
            newErrors[key] = tDeposit('error.requiredField');
            continue;
          }

          // 长度校验
          if (rule.maxLength && v.length > rule.maxLength) {
            newErrors[key] = tDeposit('error.maxLength', { max: rule.maxLength });
          }
        }

        setErrors(newErrors);

        if (Object.keys(newErrors).length > 0) {
          return null;
        }

        // billingState / billingLastName 为空时填 N/A 兜底
        const finalValues: Record<string, string> = { ...value };
        if (!finalValues.billingState?.trim()) finalValues.billingState = 'N/A';
        if (!finalValues.billingLastName?.trim()) finalValues.billingLastName = 'N/A';

        return finalValues;
      },
    }), [value, tDeposit]);

    return (
      <div className="flex flex-col gap-5">
        {/* 信用卡信息分组 */}
        <SectionDivider>{t('title')}</SectionDivider>

        {/* 卡号 + 卡片图标 */}
        <div className="flex flex-col gap-2">
          <label className="flex items-center text-sm font-medium text-text-secondary">
            <span className="mr-0.5 text-primary">*</span>
            {t('ccNumber')}
          </label>
          <div className="flex items-center gap-3">
            <Input
              inputSize="md"
              value={value.ccNumber || ''}
              onChange={(e) => handleFieldChange('ccNumber', e.target.value)}
              disabled={disabled}
              placeholder="1234 5678 9012 3456"
              error={errors.ccNumber}
              errorPosition="bottom"
            />
            <CardBrandIcons />
          </div>
        </div>

        {/* 名 / 姓 */}
        <div className="flex flex-row gap-4">
          <div className="flex-1">
            <Input
              label={t('firstName')}
              required
              inputSize="md"
              value={value.billingFirstName || ''}
              onChange={(e) => handleFieldChange('billingFirstName', e.target.value)}
              disabled={disabled}
              error={errors.billingFirstName}
              errorPosition="bottom"
            />
          </div>
          <div className="flex-1">
            <Input
              label={t('lastName')}
              inputSize="md"
              value={value.billingLastName || ''}
              onChange={(e) => handleFieldChange('billingLastName', e.target.value)}
              disabled={disabled}
              error={errors.billingLastName}
              errorPosition="bottom"
            />
          </div>
        </div>

        {/* CVC / 月 / 年 */}
        <div className="flex flex-row gap-4">
          <div className="flex-1">
            <Input
              label={t('ccCvc')}
              required
              inputSize="md"
              value={value.ccCvc || ''}
              onChange={(e) => handleFieldChange('ccCvc', e.target.value)}
              disabled={disabled}
              placeholder="CVV"
              maxLength={3}
              error={errors.ccCvc}
              errorPosition="bottom"
            />
          </div>
          <div className="flex-1">
            <SelectField
              label={t('ccMonth')}
              required
              value={value.ccMonth || ''}
              onChange={(v) => handleFieldChange('ccMonth', v)}
              options={MONTH_OPTIONS}
              placeholder="--"
              disabled={disabled}
              error={errors.ccMonth}
            />
          </div>
          <div className="flex-1">
            <SelectField
              label={t('ccYear')}
              required
              value={value.ccYear || ''}
              onChange={(v) => handleFieldChange('ccYear', v)}
              options={yearOptions}
              placeholder="--"
              disabled={disabled}
              error={errors.ccYear}
            />
          </div>
        </div>

        {/* 账单信息分组 */}
        <SectionDivider>{t('billingInformation')}</SectionDivider>

        {/* 账单地址 */}
        <Input
          label={t('billingAddress')}
          required
          inputSize="md"
          value={value.billingAddress || ''}
          onChange={(e) => handleFieldChange('billingAddress', e.target.value)}
          disabled={disabled}
          maxLength={200}
          error={errors.billingAddress}
          errorPosition="bottom"
        />

        {/* 城市 / 州 / 邮编 / 国家 */}
        <div className="flex flex-row gap-4">
          <div className="flex-1">
            <Input
              label={t('billingCity')}
              required
              inputSize="md"
              value={value.billingCity || ''}
              onChange={(e) => handleFieldChange('billingCity', e.target.value)}
              disabled={disabled}
              maxLength={100}
              error={errors.billingCity}
              errorPosition="bottom"
            />
          </div>
          <div className="flex-1">
            <Input
              label={t('billingState')}
              required={(value.billingCountry || '').toLowerCase() === 'us'}
              inputSize="md"
              value={value.billingState || ''}
              onChange={(e) => handleFieldChange('billingState', e.target.value)}
              disabled={disabled}
              maxLength={3}
              error={errors.billingState}
              errorPosition="bottom"
            />
          </div>
          <div className="flex-1">
            <Input
              label={t('billingZipcode')}
              required
              inputSize="md"
              value={value.billingZipcode || ''}
              onChange={(e) => handleFieldChange('billingZipcode', e.target.value)}
              disabled={disabled}
              maxLength={12}
              error={errors.billingZipcode}
              errorPosition="bottom"
            />
          </div>
          <div className="flex-1">
            <div className="w-full">
              <div className="mb-2 flex items-center">
                <label className="flex items-center text-sm font-normal text-text-secondary">
                  <span className="mr-0.5 text-primary">*</span>
                  {t('billingCountry')}
                </label>
              </div>
              <SearchableSelect
                options={countryOptions}
                value={countryOptions.find((opt) => opt.value === value.billingCountry) || null}
                onChange={(option: unknown) => {
                  const selected = option as { value: string; label: string } | null;
                  handleFieldChange('billingCountry', selected?.value || '');
                }}
                placeholder="--"
                isDisabled={disabled}
                error={errors.billingCountry}
                errorPosition="bottom"
              />
            </div>
          </div>
        </div>

        {/* 联系电话 */}
        <Input
          label={t('billingPhone')}
          required
          inputSize="md"
          value={value.billingPhone || ''}
          onChange={(e) => handleFieldChange('billingPhone', e.target.value)}
          disabled={disabled}
          maxLength={25}
          error={errors.billingPhone}
          errorPosition="bottom"
        />
      </div>
    );
  }
);

function SectionDivider({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-3 text-text-primary">
      <span className="h-px flex-1 bg-border" />
      <span className="text-sm font-semibold">{children}</span>
      <span className="h-px flex-1 bg-border" />
    </div>
  );
}

const CARD_BRANDS: { src: string; alt: string }[] = [
  { src: '/images/wallet/visa.png', alt: 'Visa' },
  { src: '/images/wallet/mastercard.png', alt: 'MasterCard' },
  { src: '/images/wallet/amex.png', alt: 'American Express' },
  { src: '/images/wallet/discover.png', alt: 'Discover' },
];

function CardBrandIcons() {
  return (
    <div className="flex shrink-0 items-center gap-2">
      {CARD_BRANDS.map((b) => (
        <Image
          key={b.alt}
          src={b.src}
          alt={b.alt}
          width={40}
          height={24}
          className="h-6 w-auto object-contain"
        />
      ))}
    </div>
  );
}

interface SelectFieldProps {
  label: string;
  required?: boolean;
  value: string;
  onChange: (v: string) => void;
  options: { value: string; label: string }[];
  placeholder?: string;
  disabled?: boolean;
  error?: string;
  contentClassName?: string;
}

function SelectField({
  label,
  required,
  value,
  onChange,
  options,
  placeholder,
  disabled,
  error,
  contentClassName,
}: SelectFieldProps) {
  return (
    <div className="w-full">
      <div className="mb-2 flex items-center">
        <label className="flex items-center text-sm font-normal text-text-secondary">
          {required && <span className="mr-0.5 text-primary">*</span>}
          {label}
        </label>
      </div>
      <SimpleSelect
        value={value}
        onChange={onChange}
        options={options}
        placeholder={placeholder}
        disabled={disabled}
        error={!!error}
        triggerSize="md"
        contentClassName={contentClassName}
      />
      {error && <p className="error-text mt-1 text-sm">{error}</p>}
    </div>
  );
}
