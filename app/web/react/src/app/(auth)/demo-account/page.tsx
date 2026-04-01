'use client';

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { AuthIllustration } from '@/components/layout';
import { AuthSuccessState, Button, Input, SearchableSelect, SelectInput, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { createDemoAccountFromNonAuth, getSiteConfig } from '@/actions';
import { getRegionCodes } from '@/core/data/phonesData';
import { SiteTypes, TenantTypes, getOpenAtByTenantId, getTenancy, getTenantIdByTenancy } from '@/core/types/TenantTypes';
import { getPlatformName } from '@/types/accounts';

export default function DemoAccountPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const tAuth = useTranslations('auth');
  const tCommon = useTranslations('common');
  const tAccounts = useTranslations('accounts');
  const tProfileFields = useTranslations('profile.fields');
  const tVerificationErrors = useTranslations('verification.errors');
  const { execute, isLoading } = useServerAction();

  const [submittedEmail, setSubmittedEmail] = useState('');
  const [tenantId, setTenantId] = useState<number>(TenantTypes.bvi);
  const [leverageOptions, setLeverageOptions] = useState<number[]>([20, 25, 30, 50, 100, 200, 400]);

  const requiredMsg = tVerificationErrors('required');

  const schema = z.object({
    platform: z.string().min(1, requiredMsg),
    accountType: z.string().min(1, requiredMsg),
    amount: z.string().min(1, requiredMsg),
    currencyId: z.string().min(1, requiredMsg),
    leverage: z.string().min(1, requiredMsg),
    countryCode: z.string().min(1, requiredMsg),
    phoneCode: z.string().min(1, requiredMsg),
    phoneNumber: z.string().min(1, tAuth('phoneNumberRequired')),
    name: z.string().min(1, requiredMsg),
    email: z.string().email(tAuth('emailInvalid')),
  });

  type FormData = z.infer<typeof schema>;

  const regionsData = useMemo(() => getRegionCodes(), []);
  const countryOptions = useMemo(
    () =>
      Object.values(regionsData).map((country) => ({
        value: country.code,
        label: country.name,
      })),
    [regionsData]
  );
  const phoneCodeOptions = useMemo(
    () =>
      Object.values(regionsData).map((country) => ({
        value: `+${country.dialCode}`,
        label: `${country.name} +${country.dialCode}`,
      })),
    [regionsData]
  );

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      platform: '',
      accountType: '',
      amount: '10000',
      currencyId: '',
      leverage: '',
      countryCode: 'cn',
      phoneCode: '+86',
      phoneNumber: '',
      name: '',
      email: '',
    },
  });

  const selectedCountryCode = watch('countryCode');
  const selectedPhoneCode = watch('phoneCode');
  const phoneNumber = watch('phoneNumber');

  useEffect(() => {
    if (selectedCountryCode && regionsData[selectedCountryCode]) {
      setValue('phoneCode', `+${regionsData[selectedCountryCode].dialCode}`);
    }
  }, [regionsData, selectedCountryCode, setValue]);

  useEffect(() => {
    const initPageData = async () => {
      let resolvedTenantId = Number(searchParams.get('tenantId')) || getTenantIdByTenancy(getTenancy());
      const openAt = searchParams.get('open_at') || getOpenAtByTenantId(resolvedTenantId);
      const siteConfig = await execute(getSiteConfig, openAt);
      if (siteConfig.success && siteConfig.data?.[0] === SiteTypes.Vietnam) {
        resolvedTenantId = TenantTypes.sea;
      }
      setTenantId(resolvedTenantId);

      let leverages: number[] = [20, 25, 30, 50, 100, 200, 400];
      if (resolvedTenantId === TenantTypes.au) {
        leverages = [30];
      } else if (resolvedTenantId === TenantTypes.sea) {
        leverages = [20, 25, 30, 50, 100, 200, 400, 500, 1000];
      }
      setLeverageOptions(leverages);
      setValue('leverage', String(leverages[0]));
      setValue('platform', '21');
      setValue('accountType', '4');
      setValue('currencyId', '840');
    };

    initPageData();
  }, [execute, searchParams, setValue]);

  const onSubmit = async (data: FormData) => {
    const referralCode = searchParams.get('referralCode') || undefined;
    const phoneNumberWithDialCode = `${data.phoneCode.replace('+', '')}${data.phoneNumber}`;

    const result = await execute(createDemoAccountFromNonAuth, {
      platform: Number(data.platform),
      accountType: Number(data.accountType),
      amount: Number(data.amount),
      currencyId: Number(data.currencyId),
      leverage: Number(data.leverage),
      countryCode: data.countryCode,
      phoneNumber: phoneNumberWithDialCode,
      name: data.name,
      email: data.email,
      referralCode,
      tenantId,
    });

    if (result.success) {
      setSubmittedEmail(data.email);
    }
  };

  const platformOptions = [
    { value: '21', label: getPlatformName(21) },
    { value: '31', label: getPlatformName(31) },
  ];
  const accountTypeOptions = [
    { value: '4', label: tAccounts('accountTypes.4') },
    { value: '6', label: tAccounts('accountTypes.6') },
  ];
  const currencyOptions = [
    { value: '840', label: 'USD' },
    { value: '36', label: 'AUD' },
  ];
  const amountOptions = ['10000', '50000', '100000'];

  if (submittedEmail) {
    return (
      <div className="card auth-card">
        <div className="auth-illustration-container">
          <AuthIllustration />
        </div>
        <div className="auth-card-form flex flex-col">
          <AuthSuccessState
            title={tAccounts('toast.accountCreated')}
            description={submittedEmail}
            buttonText={tAuth('register')}
            onButtonClick={() => router.push('/sign-up')}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="card auth-card">
      <div className="auth-illustration-container">
        <AuthIllustration />
      </div>

      <div className="auth-card-form flex flex-col">
        <h1 className="mb-8 text-center text-responsive-2xl font-semibold text-text-primary">
          {tAccounts('action.createDemoAccount')}
        </h1>

        <div className="mb-6 flex justify-center md:hidden">
          <AuthIllustration size={140} />
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
          <Controller
            name="platform"
            control={control}
            render={({ field }) => (
              <div>
                <label className="mb-2 block text-base font-medium text-text-secondary">
                  {tAccounts('fields.platform')}
                </label>
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger className="h-12 w-full bg-input-bg">
                    <SelectValue placeholder={tAccounts('placeholder.selectPlatform')} />
                  </SelectTrigger>
                  <SelectContent>
                    {platformOptions.map((item) => (
                      <SelectItem key={item.value} value={item.value}>
                        {item.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.platform?.message ? <p className="error-text mt-1 text-sm">{errors.platform.message}</p> : null}
              </div>
            )}
          />

          <div className="grid gap-4.5 sm:grid-cols-2">
            <Controller
              name="accountType"
              control={control}
              render={({ field }) => (
                <div>
                  <label className="mb-2 block text-base font-medium text-text-secondary">
                    {tAccounts('fields.type')}
                  </label>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="h-12 w-full bg-input-bg">
                      <SelectValue placeholder={tAccounts('placeholder.selectType')} />
                    </SelectTrigger>
                    <SelectContent>
                      {accountTypeOptions.map((item) => (
                        <SelectItem key={item.value} value={item.value}>
                          {item.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.accountType?.message ? <p className="error-text mt-1 text-sm">{errors.accountType.message}</p> : null}
                </div>
              )}
            />

            <Controller
              name="amount"
              control={control}
              render={({ field }) => (
                <div>
                  <label className="mb-2 block text-base font-medium text-text-secondary">
                    {tAccounts('fields.initialAmount')}
                  </label>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="h-12 w-full bg-input-bg">
                      <SelectValue placeholder={tAccounts('placeholder.selectAmount')} />
                    </SelectTrigger>
                    <SelectContent>
                      {amountOptions.map((item) => (
                        <SelectItem key={item} value={item}>
                          ${Number(item).toLocaleString()}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.amount?.message ? <p className="error-text mt-1 text-sm">{errors.amount.message}</p> : null}
                </div>
              )}
            />
          </div>

          <div className="grid gap-4.5 sm:grid-cols-2">
            <Controller
              name="currencyId"
              control={control}
              render={({ field }) => (
                <div>
                  <label className="mb-2 block text-base font-medium text-text-secondary">
                    {tAccounts('fields.currency')}
                  </label>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="h-12 w-full bg-input-bg">
                      <SelectValue placeholder={tAccounts('placeholder.selectCurrency')} />
                    </SelectTrigger>
                    <SelectContent>
                      {currencyOptions.map((item) => (
                        <SelectItem key={item.value} value={item.value}>
                          {item.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.currencyId?.message ? <p className="error-text mt-1 text-sm">{errors.currencyId.message}</p> : null}
                </div>
              )}
            />

            <Controller
              name="leverage"
              control={control}
              render={({ field }) => (
                <div>
                  <label className="mb-2 block text-base font-medium text-text-secondary">
                    {tAccounts('fields.leverage')}
                  </label>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="h-12 w-full bg-input-bg">
                      <SelectValue placeholder={tAccounts('placeholder.selectLeverage')} />
                    </SelectTrigger>
                    <SelectContent>
                      {leverageOptions.map((item) => (
                        <SelectItem key={item} value={String(item)}>
                          {item}:1
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.leverage?.message ? <p className="error-text mt-1 text-sm">{errors.leverage.message}</p> : null}
                </div>
              )}
            />
          </div>

          <div className="grid gap-4.5 sm:grid-cols-2">
            <Controller
              name="countryCode"
              control={control}
              render={({ field }) => (
                <SearchableSelect
                  {...field}
                  label={tAuth('country')}
                  labelClassName="text-base font-medium text-text-secondary"
                  options={countryOptions}
                  error={errors.countryCode?.message}
                  errorPosition="bottom"
                  placeholder={tAuth('countryPlaceholder')}
                  isSearchable
                  onChange={(option) => field.onChange((option as { value: string } | null)?.value || '')}
                  value={countryOptions.find((opt) => opt.value === field.value) || null}
                />
              )}
            />

            <Input
              label={tProfileFields('name')}
              labelClassName="text-base font-medium text-text-secondary"
              type="text"
              placeholder={tProfileFields('name')}
              error={errors.name?.message}
              inputSize="md"
              {...register('name')}
            />
          </div>

          <SelectInput
            label={tAuth('phoneNumber')}
            labelClassName="text-base font-medium text-text-secondary"
            selectValue={selectedPhoneCode}
            inputValue={phoneNumber}
            selectOptions={phoneCodeOptions}
            onSelectChange={(value) => setValue('phoneCode', value)}
            onInputChange={(value) => setValue('phoneNumber', value)}
            error={errors.phoneNumber?.message}
            errorPosition="bottom"
            placeholder={tAuth('phoneNumberPlaceholder')}
            inputType="tel"
          />

          <Input
            label={tAuth('email')}
            labelClassName="text-base font-medium text-text-secondary"
            type="email"
            placeholder={tAuth('emailPlaceholder')}
            error={errors.email?.message}
            autoComplete="email"
            inputSize="md"
            {...register('email')}
          />

          <div className="mt-2">
            <Button type="submit" className="w-full" loading={isLoading}>
              {tCommon('submit')}
            </Button>
          </div>
        </form>

        <p className="mt-6 text-center text-sm text-text-secondary">
          {tAuth('hasAccount')}{' '}
          <Link href="/sign-in" className="link font-medium underline">
            {tAuth('login')}
          </Link>
        </p>
      </div>
    </div>
  );
}
