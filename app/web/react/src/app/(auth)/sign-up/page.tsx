'use client';

import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { useRouter, useSearchParams, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations, useLocale } from 'next-intl';
import { Input, SearchableSelect, SelectInput, Button } from '@/components/ui';
import { AuthIllustration } from '@/components/layout';
import { getRegionCodes } from '@/core/data/phonesData';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { useTheme } from '@/hooks/useTheme';
import { login, register as registerAction, resendConfirmation, getSiteConfig } from '@/actions';
import {
  TenantTypes,
  Tenancies,
  SiteTypes,
  getTenancy,
  getTenantIdByTenancy,
  getOpenAtByTenantId,
} from '@/core/types/TenantTypes';
import { getBaDocs, getBviDocs, baDocs, bviDocs } from '@/core/data/bcrDocs';

type TabType = 'login' | 'register';
type StepType = 1 | 2 | 3;

// Cookie 工具函数
const setCookie = (name: string, value: string, days: number) => {
  let expires = '';
  if (days) {
    const date = new Date();
    date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
    expires = '; expires=' + date.toUTCString();
  }
  document.cookie = name + '=' + (value || '') + expires + '; path=/';
};

const getCookie = (name: string): string | null => {
  if (typeof document === 'undefined') return null;
  const nameEQ = name + '=';
  const ca = document.cookie.split(';');
  for (let i = 0; i < ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) === ' ') c = c.substring(1, c.length);
    if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
  }
  return null;
};

export default function SignUpPage() {
  const t = useTranslations('auth');
  const tCommon = useTranslations('common');
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const locale = useLocale();
  const { execute, isLoading } = useServerAction();
  const { showSuccess } = useToast();

  const { isDark, mounted } = useTheme();
  
  const [activeTab] = useState<TabType>('register');
  const [step, setStep] = useState<StepType>(1);
  const [loading, setLoading] = useState(false);
  const [registeredEmail, setRegisteredEmail] = useState('');
  const [tenantNo, setTenantNo] = useState<number>(TenantTypes.bvi);
  const [siteConfig, setSiteConfig] = useState<SiteTypes[]>([SiteTypes.Default]);
  const [checked, setChecked] = useState(true); // 澳洲站点第一个条款勾选状态
  const [checkedTwo, setCheckedTwo] = useState(false); // 第二个条款勾选状态
  
  // 防止 useEffect 无限循环的标志
  const isInitialized = useRef(false);
  
  // 初始化数据
  const [formConfig, setFormConfig] = useState({
    tenantId: '',
    siteId: '',
    utm: '',
    code: '',
    language: '',
    confirmUrl: '',
  });

  // 获取站点配置 (对应 Vue 的 getC 函数)
  const fetchSiteConfig = useCallback(async (tenantId?: number) => {
    try {
      let openAt = searchParams.get('open_at') || 'bvi';
      
      // 根据 tenantId 获取 openAt
      if (tenantId) {
        openAt = getOpenAtByTenantId(tenantId);
      }
      
      // 使用 Server Action
      const result = await execute(getSiteConfig, openAt);
      
      if (result.success && result.data) {
        let siteData = result.data;
        const tenancy = getTenancy();
        
        // AU 站点特殊处理
        if (tenancy === Tenancies.au) {
          siteData = [SiteTypes.Australia];
        }
        
        setSiteConfig(siteData);
        
        // 保存到 localStorage
        if (typeof window !== 'undefined') {
          window.localStorage.setItem('c', JSON.stringify(siteData));
        }
        
        // AU 站点特殊处理
        if (siteData[0] === SiteTypes.Australia) {
          setChecked(false);
        }
        
        return siteData;
      }
    } catch (error) {
      console.error('Failed to fetch site config:', error);
    }
    return null;
  }, [execute, searchParams]);

  // configTenantId - 配置租户ID
  const configTenantId = useCallback(() => {
    const tenancy = getTenancy();
    let tenantId = parseInt(searchParams.get('tenantId') || '', 10);
    
    if (tenantId) {
      setCookie('tenantId', tenantId.toString(), 30);
    } else {
      const cookieTenantId = getCookie('tenantId');
      tenantId = cookieTenantId ? parseInt(cookieTenantId, 10) : TenantTypes.bvi;
    }
    
    // JP 站点特殊处理
    if (tenancy === Tenancies.jp) {
      tenantId = TenantTypes.jp;
      setCheckedTwo(true);
    }
    
    setTenantNo(tenantId);
    
    // AU 站点特殊处理
    if (tenantId === TenantTypes.au) {
      setChecked(false);
    }
    
    return tenantId;
  }, [searchParams]);

  // configUtm - 配置 UTM 参数
  const configUtm = useCallback(() => {
    const utm = searchParams.get('utm') || '';
    if (utm) {
      setCookie('utm', utm, 7);
    }
    return utm || getCookie('utm') || '';
  }, [searchParams]);

  // configCode - 配置推荐码
  const configCode = useCallback(() => {
    let code = searchParams.get('code') || '';
    if (code) {
      setCookie('code', code, 30);
    }
    code = getCookie('code') || '';
    return code;
  }, [searchParams]);

  // initFormData - 初始化表单数据
  const initFormData = useCallback(() => {
    // 1. 初始化语言
    const passInLang = searchParams.get('lang') || '';
    let language = passInLang || locale || 'en';
    
    // 语言映射
    if (language === 'zh-hk') {
      language = 'zh-tw';
    } else if (language === 'id-id') {
      language = 'en-us';
    }
    
    // 保存语言到 localStorage
    if (typeof window !== 'undefined') {
      localStorage.setItem('language', language);
    }
    
    // 2. 生成 confirmUrl
    let confirmUrl = `${window.location.protocol}//${window.location.host}/confirm-email`;
    if (window.location.href.includes('portal')) {
      confirmUrl = `${window.location.protocol}//${window.location.host}/portal/confirm-email`;
    }
    
    return { language, confirmUrl };
  }, [locale, searchParams]);

  // onMounted 初始化逻辑
  useEffect(() => {
    if (isInitialized.current) return;
    isInitialized.current = true;
    
    const tenancy = getTenancy();
    const hostname = typeof window !== 'undefined' ? window.location.hostname : '';
    const type = hostname.split('.')[0];
    const codeParam = searchParams.get('code');
    const tenantIdParam = searchParams.get('tenantId');
    
    // 路由替换逻辑
    if (!codeParam) {
      const defaultTenantId = getTenantIdByTenancy(tenancy);
      
      if (!tenantIdParam) {
        const params = new URLSearchParams(searchParams.toString());
        params.set('tenantId', defaultTenantId.toString());
        router.replace(`${pathname}?${params.toString()}`);
      } else {
        if (tenancy === type) {
          const params = new URLSearchParams(searchParams.toString());
          params.set('tenantId', defaultTenantId.toString());
          router.replace(`${pathname}?${params.toString()}`);
        }
      }
    }
    
    // 初始化表单数据
    const { language, confirmUrl } = initFormData();
    
    setTimeout(() => {
      const tenantId = configTenantId();
      const code = configCode();
      const utm = configUtm();
      
      fetchSiteConfig(tenantId);
      
      setFormConfig({
        tenantId: tenantId.toString(),
        siteId: '1',
        utm,
        code,
        language,
        confirmUrl,
      });
    }, 0);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // 第一步验证 schema
  const stepOneSchema = useMemo(() => z.object({
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
      .min(1, t('confirmPasswordRequiredShort')),
  }).refine((data) => data.password === data.confirmPassword, {
    message: t('passwordNotMatch'),
    path: ['confirmPassword'],
  }), [t]);

  // 第二步验证 schema
  const stepTwoSchema = useMemo(() => z.object({
    firstName: z.string().min(1, t('firstNameRequired')),
    lastName: z.string().min(1, t('lastNameRequired')),
    countryCode: z.string().min(1, t('countryRequired')),
    referralCode: z.string().optional(),
    phone: z.string(),
    phoneNumber: z.string().min(1, t('phoneNumberRequired')),
    agreedToTerms: z.boolean().refine(val => val === true, {
      message: t('agreeTermsRequired'),
    }),
  }), [t]);

  type StepOneFormData = z.infer<typeof stepOneSchema>;
  type StepTwoFormData = z.infer<typeof stepTwoSchema>;

  // 第一步表单
  const {
    register: registerStepOne,
    handleSubmit: handleSubmitStepOne,
    formState: { errors: errorsStepOne },
    getValues: getValuesStepOne,
  } = useForm<StepOneFormData>({
    resolver: zodResolver(stepOneSchema),
  });

  // 第二步表单
  const {
    register: registerStepTwo,
    handleSubmit: handleSubmitStepTwo,
    formState: { errors: errorsStepTwo },
    watch,
    setValue,
    control: controlStepTwo,
  } = useForm<StepTwoFormData>({
    resolver: zodResolver(stepTwoSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      phone: '+86',
      countryCode: 'cn',
      agreedToTerms: false,
      phoneNumber: '',
      referralCode: '',
    },
  });

  const agreedToTerms = watch('agreedToTerms');
  const selectedCountryCode = watch('countryCode');
  const phoneCodeValue = watch('phone');
  const phoneNumberValue = watch('phoneNumber');

  const regionsData = useMemo(() => getRegionCodes(), []);
  
  const countryOptions = useMemo(
    () => Object.values(regionsData).map((country) => ({
      value: country.code,
      label: country.name,
    })),
    [regionsData]
  );

  const phoneCodeOptions = useMemo(
    () => Object.values(regionsData).map((country) => ({
      value: `+${country.dialCode}`,
      label: `${country.name} +${country.dialCode}`,
      code: country.code,
    })),
    [regionsData]
  );

  useEffect(() => {
    if (selectedCountryCode && regionsData[selectedCountryCode]) {
      const dialCode = `+${regionsData[selectedCountryCode].dialCode}`;
      setValue('phone', dialCode);
    }
  }, [selectedCountryCode, setValue, regionsData]);

  useEffect(() => {
    if (formConfig.code) {
      setValue('referralCode', formConfig.code);
    }
  }, [formConfig.code, setValue]);

  const onStepOneSubmit = () => {
    setStep(2);
  };

  // 执行自动登录
  const performAutoLogin = useCallback(async (email: string, password: string) => {
    const loginResult = await execute(login, {
      email,
      password,
      rememberMe: true,
      tenantId: formConfig.tenantId || undefined,
    });

    if (loginResult.success) {
      router.push('/verification');
      router.refresh();
      return true;
    }

    if (loginResult.twoFactorRequired) {
      showSuccess(t('pleaseLoginWith2FA'));
      router.push('/sign-in');
      return false;
    }

    if (loginResult.hasMultipleTenants) {
      showSuccess(t('pleaseSelectTenant'));
      router.push('/sign-in');
      return false;
    }

    router.push('/sign-in?registered=true');
    return false;
  }, [execute, formConfig.tenantId, router, showSuccess, t]);

  // 第二步提交 - 注册
  const onStepTwoSubmit = async (data: StepTwoFormData) => {
    setLoading(true);

    const stepOneData = getValuesStepOne();
    const ccc = regionsData[data.countryCode]?.dialCode || '';

    const registerData: Record<string, unknown> = {
      email: stepOneData.email,
      password: stepOneData.password,
      FirstName: data.firstName,
      LastName: data.lastName,
      countryCode: data.countryCode,
      phone: data.phoneNumber,
      ccc: ccc || undefined,
      otp: 771578,
      language: formConfig.language,
      confirmUrl: formConfig.confirmUrl,
    };

    if (data.referralCode) registerData.referCode = data.referralCode;
    if (formConfig.code) registerData.code = formConfig.code;
    if (formConfig.tenantId) registerData.tenantId = parseInt(formConfig.tenantId);
    if (formConfig.siteId) registerData.siteId = parseInt(formConfig.siteId);
    if (formConfig.utm) registerData.utm = formConfig.utm;

    // 使用 Server Action
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const result = await execute(registerAction, registerData as any);

    if (!result.success) {
      setLoading(false);
      
      if (result.errorCode === '__EMAIL_EXISTS__') {
        setStep(1);
      }
      return;
    }

    setRegisteredEmail(stepOneData.email);
    showSuccess(t('registerSuccess'));
    
    const loginSuccess = await performAutoLogin(stepOneData.email, stepOneData.password);
    
    if (!loginSuccess) {
      setStep(3);
    }
    
    setLoading(false);
  };

  // 重发确认邮件
  const handleResendConfirmation = async () => {
    const result = await execute(resendConfirmation, {
      email: registeredEmail,
      confirmUrl: formConfig.confirmUrl,
    });

    if (result.success) {
      showSuccess(t('confirmationEmailResend'));
    }
  };

  const handleTabChange = (tab: TabType) => {
    if (tab === 'login') {
      router.push('/sign-in');
    }
  };

  const goToLogin = () => {
    router.push('/sign-in');
  };

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
          {step === 3 ? t('verifyYourEmail') : t('createAccount')}
        </h1>

        {/* Tab 切换 - 仅第1、2步显示 */}
        {step !== 3 && (
          <div className="relative mb-10 flex gap-6">
            <button
              type="button"
              onClick={() => handleTabChange('login')}
              className="tab-item"
            >
              {t('login')}
            </button>
            <button
              type="button"
              className={`tab-item ${activeTab === 'register' ? 'active' : ''}`}
            >
              {t('register')}
            </button>
            <div className="absolute bottom-0 left-0 right-0 divider" />
          </div>
        )}

        {/* 第一步：账户信息 */}
        {step === 1 && (
          <form onSubmit={handleSubmitStepOne(onStepOneSubmit)} className="flex flex-col gap-6">
            <Input
              label={t('email')}
              labelClassName="text-base font-medium text-text-secondary"
              type="email"
              placeholder={t('emailPlaceholder')}
              error={errorsStepOne.email?.message}
              autoComplete="email"
              inputSize="md"
              {...registerStepOne('email')}
            />

            <Input
              label={t('password')}
              labelClassName="text-base font-medium text-text-secondary"
              type="password"
              placeholder={t('passwordHint')}
              error={errorsStepOne.password?.message}
              showPasswordToggle
              autoComplete="new-password"
              inputSize="md"
              {...registerStepOne('password')}
            />

            <Input
              label={t('confirmPassword')}
              labelClassName="text-base font-medium text-text-secondary"
              type="password"
              placeholder={t('confirmPasswordPlaceholderShort')}
              error={errorsStepOne.confirmPassword?.message}
              showPasswordToggle
              autoComplete="new-password"
              inputSize="md"
              {...registerStepOne('confirmPassword')}
            />

            <div className="mt-4">
              <Button type="submit" className="w-full">
                {t('nextStep')}
              </Button>
            </div>
          </form>
        )}

        {/* 第二步：个人信息 */}
        {step === 2 && (
          <form onSubmit={handleSubmitStepTwo(onStepTwoSubmit)} className="flex flex-col gap-6">
            <button
              type="button"
              onClick={() => setStep(1)}
              className="self-start text-sm text-text-secondary hover:text-text-primary transition-colors"
            >
              ← {tCommon('back')}
            </button>

            <p className="text-sm text-text-secondary -mt-2">
              {t('enterYourInfo')}
            </p>

            <div className="flex gap-4.5 flex-col sm:flex-row">
              <div className="flex-1">
                <Input
                  label={t('firstName')}
                  labelClassName="text-base font-medium text-text-secondary"
                  type="text"
                  placeholder={t('firstNamePlaceholder')}
                  error={errorsStepTwo.firstName?.message}
                  errorPosition="bottom"
                  autoComplete="given-name"
                  inputSize="md"
                  {...registerStepTwo('firstName')}
                />
              </div>
              <div className="flex-1">
                <Input
                  label={t('lastName')}
                  labelClassName="text-base font-medium text-text-secondary"
                  type="text"
                  placeholder={t('lastNamePlaceholder')}
                  error={errorsStepTwo.lastName?.message}
                  errorPosition="bottom"
                  autoComplete="family-name"
                  inputSize="md"
                  {...registerStepTwo('lastName')}
                />
              </div>
            </div>

            <div className="flex gap-4.5 flex-col sm:flex-row">
              <div className="flex-1">
                <Controller
                  name="countryCode"
                  control={controlStepTwo}
                  render={({ field }) => (
                    <SearchableSelect
                      {...field}
                      label={t('country')}
                      labelClassName="text-base font-medium text-text-secondary"
                      options={countryOptions}
                      error={errorsStepTwo.countryCode?.message}
                      errorPosition="bottom"
                      placeholder={t('countryPlaceholder')}
                      isSearchable
                      onChange={(option) => field.onChange((option as { value: string } | null)?.value || '')}
                      value={countryOptions.find(opt => opt.value === field.value) || null}
                    />
                  )}
                />
              </div>
              <div className="flex-1">
                <Input
                  label={t('referralCode')}
                  labelClassName="text-base font-medium text-text-secondary"
                  placeholder={t('referralCodePlaceholder')}
                  error={errorsStepTwo.referralCode?.message}
                  errorPosition="bottom"
                  disabled={!!formConfig.code}
                  inputSize="md"
                  {...registerStepTwo('referralCode')}
                />
              </div>
            </div>

            <SelectInput
              label={t('phoneNumber')}
              labelClassName="text-base font-medium text-text-secondary"
              selectValue={phoneCodeValue}
              inputValue={phoneNumberValue}
              selectOptions={phoneCodeOptions}
              onSelectChange={(value) => setValue('phone', value)}
              onInputChange={(value) => setValue('phoneNumber', value)}
              error={errorsStepTwo.phoneNumber?.message}
              errorPosition="bottom"
              placeholder={t('phoneNumberPlaceholder')}
              inputType="tel"
            />

            {/* 协议条款 */}
            <div className="flex flex-col gap-3">
              {/* 澳洲站点：第一个复选框 */}
              {(siteConfig[0] === SiteTypes.Australia || tenantNo === TenantTypes.au) && (
                <div className="flex items-start gap-2">
                  <div className="relative shrink-0 size-5">
                    <input
                      type="checkbox"
                      id="terms-au-1"
                      checked={checked}
                      onChange={(e) => setChecked(e.target.checked)}
                      style={{
                        borderColor: checked 
                          ? 'var(--color-primary)'
                          : (mounted && isDark ? '#4b5563' : '#d1d5db'),
                        backgroundColor: checked
                          ? 'var(--color-primary)'
                          : 'transparent'
                      }}
                      className="peer size-5 cursor-pointer appearance-none rounded-full border-2 transition-all"
                    />
                    <svg
                      className="pointer-events-none absolute left-1/2 top-1/2 size-3 -translate-x-1/2 -translate-y-1/2 stroke-white stroke-2 opacity-0 transition-opacity peer-checked:opacity-100"
                      viewBox="0 0 12 12"
                      fill="none"
                    >
                      <path
                        d="M2 6L5 9L10 3"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                  <label htmlFor="terms-au-1" className="cursor-pointer text-sm leading-5 text-text-secondary">
                    {t('pleaseEnsure')}
                    <a
                      href={getBaDocs(baDocs.productDisclosureDocument.title, locale)}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-primary underline"
                    >
                      {t('productDisclosureDocument')}
                    </a>
                    ,{' '}
                    <a
                      href={getBaDocs(baDocs.financialServicesGuide.title, locale)}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-primary underline"
                    >
                      {t('financialServicesGuide')}
                    </a>
                    ,{' '}
                    <a
                      href={getBaDocs(baDocs.targetMarketDetermination.title, locale)}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-primary underline"
                    >
                      {t('targetMarketDetermination')}
                    </a>
                    {t('andHaveSought')}
                  </label>
                </div>
              )}

              {/* 日本站点：不显示条款 */}
              {tenantNo === TenantTypes.jp ? null : (
                (siteConfig[0] === SiteTypes.Australia || tenantNo === TenantTypes.au) ? (
                  <div className="flex items-start gap-2">
                    <div className="relative shrink-0 size-5">
                      <input
                        type="checkbox"
                        id="terms-au-2"
                        checked={checkedTwo}
                        onChange={(e) => {
                          setCheckedTwo(e.target.checked);
                          setValue('agreedToTerms', e.target.checked);
                        }}
                        style={{
                          borderColor: checkedTwo 
                            ? 'var(--color-primary)'
                            : (mounted && isDark ? '#4b5563' : '#d1d5db'),
                          backgroundColor: checkedTwo
                            ? 'var(--color-primary)'
                            : 'transparent'
                        }}
                        className="peer size-5 cursor-pointer appearance-none rounded-full border-2 transition-all"
                      />
                      <svg
                        className="pointer-events-none absolute left-1/2 top-1/2 size-3 -translate-x-1/2 -translate-y-1/2 stroke-white stroke-2 opacity-0 transition-opacity peer-checked:opacity-100"
                        viewBox="0 0 12 12"
                        fill="none"
                      >
                        <path
                          d="M2 6L5 9L10 3"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                    </div>
                    <label htmlFor="terms-au-2" className="cursor-pointer text-sm leading-5 text-text-secondary">
                      {t('byCreateAccountYouAgree')}
                      <a
                        href={getBaDocs(baDocs.termAndConditions.title, locale)}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-primary underline"
                      >
                        {t('termAndConditions')}
                      </a>
                      {t('and')}
                      <a
                        href={getBaDocs(baDocs.privacyPolicy.title, locale)}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-primary underline"
                      >
                        {t('privacyPolicy')}
                      </a>
                      。
                    </label>
                  </div>
                ) : (
                  <div className="flex items-start gap-2">
                    <div className="relative shrink-0 size-5">
                      <input
                        type="checkbox"
                        id="terms"
                        {...registerStepTwo('agreedToTerms')}
                        style={{
                          borderColor: agreedToTerms 
                            ? 'var(--color-primary)'
                            : (mounted && isDark ? '#4b5563' : '#d1d5db'),
                          backgroundColor: agreedToTerms
                            ? 'var(--color-primary)'
                            : 'transparent'
                        }}
                        className="peer size-5 cursor-pointer appearance-none rounded-full border-2 transition-all"
                      />
                      <svg
                        className="pointer-events-none absolute left-1/2 top-1/2 size-3 -translate-x-1/2 -translate-y-1/2 stroke-white stroke-2 opacity-0 transition-opacity peer-checked:opacity-100"
                        viewBox="0 0 12 12"
                        fill="none"
                      >
                        <path
                          d="M2 6L5 9L10 3"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                    </div>
                    <label htmlFor="terms" className="cursor-pointer text-sm leading-5 text-text-secondary">
                      {t('byCreateAccountYouAgree')}
                      <a
                        href={getBviDocs(bviDocs.TermsAndConditions.title, locale) || '#'}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-primary underline"
                      >
                        {t('TermsAndConditions')}
                      </a>
                      {t('and')}
                      <a
                        href={getBviDocs(bviDocs.PrivacyPolicy.title, locale) || '#'}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-primary underline"
                      >
                        {t('PrivacyPolicy')}
                      </a>
                      。
                    </label>
                  </div>
                )
              )}
            </div>
            {errorsStepTwo.agreedToTerms && (
              <p className="error-text text-sm">{errorsStepTwo.agreedToTerms.message}</p>
            )}

            <div className="mt-4">
              <Button 
                className="w-full"
                type="submit" 
                loading={loading || isLoading} 
                disabled={
                  tenantNo === TenantTypes.jp 
                    ? false
                    : (siteConfig[0] === SiteTypes.Australia || tenantNo === TenantTypes.au)
                      ? (!checked || !checkedTwo)
                      : !agreedToTerms
                }
              >
                {t('submit')}
              </Button>
            </div>
          </form>
        )}

        {/* 第三步：邮箱验证提示 */}
        {step === 3 && (
          <div className="flex flex-col gap-6">
            <div className="text-text-secondary space-y-4">
              <p>
                {t('thankSignUpAndConfirm')}{' '}
                <span className="font-medium text-text-primary">{registeredEmail}</span>{' '}
                {t('toActivateYourAccount')}
              </p>
              <p>{t('linkExpireContact')}</p>
              <p>
                {t('notReceiveEmail')}{' '}
                <button
                  type="button"
                  onClick={handleResendConfirmation}
                  className="text-text-link hover:underline"
                >
                  {t('resendEmail')}
                </button>
              </p>
            </div>

            <p className="text-sm text-text-secondary">
              <Link href="/lead-create" className="text-text-link hover:underline">
                {t('contactUs')}
              </Link>
            </p>

            <div className="mt-4">
              <Button type="button" onClick={goToLogin}>
                {t('backToLogin')}
              </Button>
            </div>
          </div>
        )}

        {/* 登录链接 - 仅第1、2步显示 */}
        {step !== 3 && (
          <p className="mt-6 text-center text-sm text-text-secondary">
            {t('hasAccount')}{' '}
            <Link href="/sign-in" className="link font-medium underline">
              {t('login')}
            </Link>
          </p>
        )}
      </div>
    </div>
  );
}
