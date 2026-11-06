'use client';

import { useEffect, useState, useMemo, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { CheckCircledIcon, CrossCircledIcon, ResetIcon } from '@radix-ui/react-icons';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button, Input } from '@/components/ui';
import { Stepper } from '@/components/ui/Stepper';
import { SimpleSelect } from '@/components/ui/radix/Select';
import { useServerAction } from '@/hooks/useServerAction';
import { createLiveAccount } from '@/actions';
import { fetchAction } from '@/lib/api/browser-client';
import { useToast } from '@/hooks/useToast';
import { useUserStore } from '@/stores/userStore';
import type { AccountConfig, ServiceMap, CreateLiveAccountParams } from '@/types/accounts';
import { AccountTypes, CurrencyTypes, getPlatformName } from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';

interface CreateLiveAccountModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  serviceMap: ServiceMap;
}

type Step = 1 | 2;

export function CreateLiveAccountModal({
  open,
  onOpenChange,
  onSuccess,
  serviceMap,
}: CreateLiveAccountModalProps) {
  const t = useTranslations('accounts');
  const getCurrencyName = useCurrencyName();
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();
  const hasRole = useUserStore((s) => s.hasRole);
  const user = useUserStore((s) => s.user);

  const isBviTenancy = user?.tenancy === 'bvi';
  const showReferCodeField = hasRole('IB') && !hasRole('MLM');

  const [step, setStep] = useState<Step>(1);
  const [config, setConfig] = useState<AccountConfig | null>(null);
  const [isLoadingConfig, setIsLoadingConfig] = useState(false);
  const [accountTypeAvailable, setAccountTypeAvailable] = useState<number[]>([]);

  const [platform, setPlatform] = useState<string>('');
  const [accountType, setAccountType] = useState<string>('');
  const [currency, setCurrency] = useState<string>('');
  const [leverage, setLeverage] = useState<string>('');

  const [referCode, setReferCode] = useState('');
  const [defaultReferCode, setDefaultReferCode] = useState('');
  const [invalidReferCode, setInvalidReferCode] = useState(false);

  const resetFormState = useCallback(() => {
    setStep(1);
    setConfig(null);
    setAccountTypeAvailable([]);
    setPlatform('');
    setAccountType('');
    setCurrency('');
    setLeverage('');
    setReferCode('');
    setDefaultReferCode('');
    setInvalidReferCode(false);
  }, []);

  const applyPrefillFromConfig = useCallback((data: AccountConfig) => {
    if (data.tradingPlatformAvailable?.length > 0) {
      const first = data.tradingPlatformAvailable[0];
      setPlatform(`${first.serviceId}-${first.platform}`);
    }
    if (data.accountTypeAvailable?.length > 0) {
      setAccountType(String(data.accountTypeAvailable[0]));
    }
    if (data.currencyAvailable?.length > 0) {
      const defaultCurrency = data.currencyAvailable.includes(CurrencyTypes.USD)
        ? CurrencyTypes.USD
        : data.currencyAvailable[0];
      setCurrency(String(defaultCurrency));
    }
    if (data.leverageAvailable?.length > 0) {
      const defaultLeverage = data.leverageAvailable.includes(1000)
        ? 1000
        : data.leverageAvailable[0];
      setLeverage(String(defaultLeverage));
    }
  }, []);

  useEffect(() => {
    if (!open) return;

    let cancelled = false;

    const loadConfig = async () => {
      setIsLoadingConfig(true);
      resetFormState();
      try {
        const result = await execute(async () =>
          fetchAction<AccountConfig>('getLiveAccountConfig')
        );
        if (cancelled || !result.success || !result.data) return;

        const data = result.data;
        setConfig(data);
        setAccountTypeAvailable(data.accountTypeAvailable ?? []);

        const code = data.referCode ?? '';
        setReferCode(code);
        setDefaultReferCode(code);
        applyPrefillFromConfig(data);
      } finally {
        if (!cancelled) setIsLoadingConfig(false);
      }
    };

    void loadConfig();
    return () => {
      cancelled = true;
    };
  }, [open, execute, resetFormState, applyPrefillFromConfig]);

  useEffect(() => {
    if (!open) return;

    let cancelled = false;

    const resolveAccountTypes = async () => {
      const trimmed = referCode.trim();
      if (!trimmed) {
        setInvalidReferCode(true);
        return;
      }

      const result = await fetchAction<number[]>(
        'getAccountTypesByReferCode',
        trimmed
      );
      if (cancelled) return;

      if (result.success && result.data && result.data.length > 0) {
        setInvalidReferCode(false);
        setAccountTypeAvailable(result.data);
        setAccountType(String(result.data[0]));
      } else {
        setInvalidReferCode(true);
      }
    };

    void resolveAccountTypes();
    return () => {
      cancelled = true;
    };
  }, [open, referCode]);

  useEffect(() => {
    if (
      isBviTenancy &&
      currency === String(CurrencyTypes.AUD) &&
      accountType === String(AccountTypes.Standard)
    ) {
      setAccountType('');
    }
  }, [currency, isBviTenancy, accountType]);

  useEffect(() => {
    if (
      isBviTenancy &&
      currency === String(CurrencyTypes.AUD) &&
      accountType === String(AccountTypes.Standard)
    ) {
      setCurrency('');
    }
  }, [accountType, isBviTenancy, currency]);

  const handleClose = () => {
    onOpenChange(false);
    resetFormState();
  };

  const handleSubmit = async () => {
    if (!platform || !accountType || !currency || !leverage) return;

    const [serviceId, platformId] = platform.split('-').map(Number);

    const params: CreateLiveAccountParams = {
      serviceId,
      platform: platformId,
      accountType: Number(accountType),
      currencyId: Number(currency),
      leverage: Number(leverage),
    };

    if (referCode.trim()) {
      params.referCode = referCode.trim();
    }

    const result = await execute(createLiveAccount, params);
    if (result.success) {
      showSuccess(t('toast.accountCreated'));
      onSuccess();
      resetFormState();
    }
  };

  const platformOptions =
    config?.tradingPlatformAvailable?.map((item) => {
      const shortLabel =
        serviceMap[item.serviceId]?.platformName || getPlatformName(item.platform);
      const fullName =
        serviceMap[item.serviceId]?.serverName || getPlatformName(item.platform);
      return {
        value: `${item.serviceId}-${item.platform}`,
        label: fullName,
        shortLabel,
        displayLabel: `${shortLabel} ${fullName}`,
      };
    }) || [];

  const accountTypeOptions = accountTypeAvailable.map((type) => ({
    value: String(type),
    label: t(`accountTypes.${type}`),
    disabled:
      isBviTenancy &&
      type === AccountTypes.Standard &&
      currency === String(CurrencyTypes.AUD),
  }));

  const currencyOptions =
    config?.currencyAvailable?.map((curr) => ({
      value: String(curr),
      label: getCurrencyName(curr),
      disabled:
        isBviTenancy &&
        curr === CurrencyTypes.AUD &&
        accountType === String(AccountTypes.Standard),
    })) || [];

  const leverageOptions =
    config?.leverageAvailable?.map((lev) => ({
      value: String(lev),
      label: `${lev}:1`,
    })) || [];

  const selectedPlatformLabel =
    platformOptions.find((o) => o.value === platform)?.shortLabel || '-';
  const selectedTypeLabel =
    accountTypeOptions.find((o) => o.value === accountType)?.label || '-';
  const selectedCurrencyLabel =
    currencyOptions.find((o) => o.value === currency)?.label || '-';
  const selectedLeverageLabel =
    leverageOptions.find((o) => o.value === leverage)?.label || '-';

  const canProceed = !!platform && !!accountType && !!currency && !!leverage;

  const stepperSteps = useMemo(
    () => [
      { id: 'info', label: t('step.information'), number: 1 },
      { id: 'review', label: t('step.review'), number: 2 },
    ],
    [t]
  );

  const stepperCurrentStep = step === 1 ? 'info' : 'review';
  const stepperCompletedSteps = step === 2 ? ['info'] : [];

  return (
    <Dialog
      open={open}
      onOpenChange={(v) => {
        if (!v) handleClose();
        else onOpenChange(v);
      }}
    >
      <DialogContent
        className="h-[800px]! flex flex-col justify-between"
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        <div className="flex flex-col gap-6">
          <DialogHeader>
            <DialogTitle>{t('modal.createLiveAccount')}</DialogTitle>
          </DialogHeader>

          <Stepper
            steps={stepperSteps}
            currentStep={stepperCurrentStep}
            completedSteps={stepperCompletedSteps}
          />

          {step === 1 && (
            <div className="flex flex-col gap-5">
              <div className="flex flex-col gap-2">
                <label className="flex items-center text-sm font-medium text-text-secondary">
                  <span className="mr-1 text-primary">*</span>
                  {t('fields.platform')}
                </label>
                <div className="flex gap-5">
                  {platformOptions.map((opt) => {
                    const isSelected = platform === opt.value;
                    return (
                      <button
                        key={opt.value}
                        type="button"
                        onClick={() => setPlatform(opt.value)}
                        className={`relative flex flex-1 items-center gap-2.5 overflow-hidden rounded p-3 transition-colors ${
                          isSelected
                            ? 'border border-primary bg-input-bg text-primary'
                            : 'border border-transparent bg-input-bg text-text-secondary hover:text-text-primary'
                        }`}
                      >
                        <svg
                          width="20"
                          height="20"
                          viewBox="0 0 20 20"
                          fill="none"
                          className="shrink-0"
                        >
                          <rect
                            width="20"
                            height="20"
                            rx="4"
                            fill="currentColor"
                            fillOpacity="0.15"
                          />
                          <text
                            x="10"
                            y="14"
                            textAnchor="middle"
                            fontSize="8"
                            fontWeight="600"
                            fill="currentColor"
                          >
                            {opt.shortLabel?.includes('5') ? 'MT5' : 'MT4'}
                          </text>
                        </svg>
                        <span className="flex-1 text-left text-base font-medium">
                          {opt.label}
                        </span>
                        {isSelected && (
                          <svg
                            width="16"
                            height="16"
                            viewBox="0 0 16 16"
                            fill="none"
                            className="absolute -bottom-px -right-px"
                          >
                            <path
                              d="M0 16L16 16L16 0L0 16Z"
                              fill="var(--color-primary)"
                            />
                            <path
                              d="M12 10.5L10 12.5L8.5 11"
                              stroke="white"
                              strokeWidth="1.5"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                            />
                          </svg>
                        )}
                      </button>
                    );
                  })}
                </div>
              </div>

              <div className="flex flex-col gap-2">
                <label className="flex items-center text-sm font-medium text-text-secondary">
                  <span className="mr-1 text-primary">*</span>
                  {t('fields.type')}
                </label>
                <SimpleSelect
                  value={accountType}
                  onChange={setAccountType}
                  options={accountTypeOptions}
                  placeholder={t('placeholder.selectType')}
                  disabled={isLoadingConfig}
                  triggerSize="sm"
                />
              </div>

              <div className="flex flex-col gap-2">
                <label className="flex items-center text-sm font-medium text-text-secondary">
                  <span className="mr-1 text-primary">*</span>
                  {t('fields.currency')}
                </label>
                <SimpleSelect
                  value={currency}
                  onChange={setCurrency}
                  options={currencyOptions}
                  placeholder={t('placeholder.selectCurrency')}
                  disabled={isLoadingConfig}
                  triggerSize="sm"
                />
              </div>

              <div className="flex flex-col gap-2">
                <label className="flex items-center text-sm font-medium text-text-secondary">
                  <span className="mr-1 text-primary">*</span>
                  {t('fields.leverage')}
                </label>
                <SimpleSelect
                  value={leverage}
                  onChange={setLeverage}
                  options={leverageOptions}
                  placeholder={t('placeholder.selectLeverage')}
                  disabled={isLoadingConfig}
                  triggerSize="sm"
                />
              </div>

              {showReferCodeField && (
                <div className="flex flex-col gap-2">
                  <label className="flex items-center text-sm font-medium text-text-secondary">
                    <span className="mr-1 text-primary">*</span>
                    {t('fields.referCode')}
                  </label>
                  <div className="flex items-center gap-3">
                    <Input
                      value={referCode}
                      onChange={(e) => setReferCode(e.target.value)}
                      placeholder={t('placeholder.referCode')}
                      disabled={isLoadingConfig}
                      inputSize="sm"
                      className="flex-1"
                    />
                    <button
                      type="button"
                      className="text-text-secondary hover:text-text-primary"
                      title={t('action.reset')}
                      onClick={() => setReferCode(defaultReferCode)}
                      disabled={isLoadingConfig}
                    >
                      <ResetIcon className="size-5" />
                    </button>
                    {!invalidReferCode ? (
                      <CheckCircledIcon
                        className="size-5 shrink-0 text-[#14a44d]"
                        aria-label={t('referCodeValid')}
                      />
                    ) : (
                      <CrossCircledIcon
                        className="size-5 shrink-0 text-[#900000]"
                        aria-label={t('referCodeInvalid')}
                      />
                    )}
                  </div>
                </div>
              )}
            </div>
          )}

          {step === 2 && (
            <div className="flex flex-col gap-5 px-5">
              <h3 className="text-sm font-semibold text-text-primary">
                {t('review.basicInfo')}
              </h3>
              <div className="flex flex-col gap-5">
                <div className="flex items-center justify-between">
                  <span className="w-[117px] shrink-0 text-sm text-text-table">
                    {t('fields.platform')}：
                  </span>
                  <span className="text-right text-sm text-text-secondary">
                    {selectedPlatformLabel}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="w-[117px] shrink-0 text-sm text-text-table">
                    {t('fields.type')}：
                  </span>
                  <span className="text-right text-sm text-text-secondary">
                    {selectedTypeLabel}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="w-[117px] shrink-0 text-sm text-text-table">
                    {t('fields.currency')}：
                  </span>
                  <span className="text-right text-sm text-text-secondary">
                    {selectedCurrencyLabel}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="w-[117px] shrink-0 text-sm text-text-table">
                    {t('fields.leverage')}：
                  </span>
                  <span className="text-right text-sm text-text-secondary">
                    {selectedLeverageLabel}
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>

        <DialogFooter className="flex-row! items-center justify-between! pt-5">
          {step === 1 ? (
            <>
              <div />
              <div className="flex gap-2 md:gap-3">
                <Button
                  variant="outline"
                  onClick={handleClose}
                  disabled={isLoading}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.close')}
                </Button>
                <Button
                  variant="primary"
                  onClick={() => setStep(2)}
                  disabled={!canProceed || isLoadingConfig}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.next')}
                </Button>
              </div>
            </>
          ) : (
            <>
              <Button
                variant="outline"
                onClick={() => setStep(1)}
                disabled={isLoading}
                className="w-auto min-w-16 md:w-[100px]"
              >
                {t('action.prev')}
              </Button>
              <div className="flex gap-2 md:gap-3">
                <Button
                  variant="outline"
                  onClick={handleClose}
                  disabled={isLoading}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.close')}
                </Button>
                <Button
                  variant="primary"
                  onClick={handleSubmit}
                  loading={isLoading}
                  disabled={isLoading}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.submit')}
                </Button>
              </div>
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
