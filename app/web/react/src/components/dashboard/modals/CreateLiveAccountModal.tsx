'use client';

import { useEffect, useState, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui';
import { Stepper } from '@/components/ui/Stepper';
import { SimpleSelect } from '@/components/ui/radix/Select';
import { Input } from '@/components/ui/radix/Input';
import { useServerAction } from '@/hooks/useServerAction';
import { getLiveAccountConfig, createLiveAccount } from '@/actions';
import { useToast } from '@/hooks/useToast';
import type { AccountConfig, ServiceMap, CreateLiveAccountParams } from '@/types/accounts';
import { CurrencyTypes, getPlatformName } from '@/types/accounts';

interface CreateLiveAccountModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  serviceMap: ServiceMap;
}

type Step = 1 | 2;

const CurrencyNames: Record<number, string> = {
  [CurrencyTypes.AUD]: 'AUD',
  [CurrencyTypes.CNY]: 'CNY',
  [CurrencyTypes.USD]: 'USD',
  [CurrencyTypes.USC]: 'USC',
};

export function CreateLiveAccountModal({
  open,
  onOpenChange,
  onSuccess,
  serviceMap,
}: CreateLiveAccountModalProps) {
  const t = useTranslations('accounts');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();

  const [step, setStep] = useState<Step>(1);
  const [config, setConfig] = useState<AccountConfig | null>(null);
  const [isLoadingConfig, setIsLoadingConfig] = useState(false);

  // 表单数据
  const [platform, setPlatform] = useState<string>('');
  const [accountType, setAccountType] = useState<string>('');
  const [currency, setCurrency] = useState<string>('');
  const [leverage, setLeverage] = useState<string>('');
  const [referCode, setReferCode] = useState<string>('');

  // 加载配置
  useEffect(() => {
    if (open && !config) {
      const loadConfig = async () => {
        setIsLoadingConfig(true);
        try {
          const result = await execute(getLiveAccountConfig);
          if (result.success && result.data) {
            setConfig(result.data);
            if (result.data.tradingPlatformAvailable?.length > 0) {
              const first = result.data.tradingPlatformAvailable[0];
              setPlatform(`${first.serviceId}-${first.platform}`);
            }
            if (result.data.accountTypeAvailable?.length > 0) {
              setAccountType(String(result.data.accountTypeAvailable[0]));
            }
            if (result.data.currencyAvailable?.length > 0) {
              setCurrency(String(result.data.currencyAvailable[0]));
            }
            if (result.data.leverageAvailable?.length > 0) {
              setLeverage(String(result.data.leverageAvailable[0]));
            }
            if (result.data.referCode) {
              setReferCode(result.data.referCode);
            }
          }
        } finally {
          setIsLoadingConfig(false);
        }
      };
      loadConfig();
    }
  }, [open, config, execute]);

  // 关闭时重置
  const handleClose = () => {
    onOpenChange(false);
    setStep(1);
  };

  // 提交表单
  const handleSubmit = async () => {
    if (!platform || !accountType || !currency || !leverage) return;

    const [serviceId, platformId] = platform.split('-').map(Number);

    const params: CreateLiveAccountParams = {
      serviceId,
      platform: platformId,
      accountType: Number(accountType),
      currencyId: Number(currency),
      leverage: Number(leverage),
      referCode: referCode || undefined,
    };

    const result = await execute(createLiveAccount, params);
    if (result.success) {
      showSuccess(t('toast.accountCreated'));
      onSuccess();
      setPlatform('');
      setAccountType('');
      setCurrency('');
      setLeverage('');
      setReferCode('');
      setConfig(null);
      setStep(1);
    }
  };

  // 平台选项（设计稿：MT4 Meta Trader 4 / MT5 Meta Trader 5）
  const platformOptions = config?.tradingPlatformAvailable?.map((item) => {
    const shortLabel = serviceMap[item.serviceId]?.platformName || getPlatformName(item.platform);
    const fullName = serviceMap[item.serviceId]?.serverName || getPlatformName(item.platform);
    return {
      value: `${item.serviceId}-${item.platform}`,
      label: fullName,
      shortLabel,
      displayLabel: `${shortLabel} ${fullName}`,
    };
  }) || [];

  // 账户类型选项
  const accountTypeOptions = config?.accountTypeAvailable?.map((type) => ({
    value: String(type),
    label: t(`accountTypes.${type}`),
  })) || [];

  // 货币选项
  const currencyOptions = config?.currencyAvailable?.map((curr) => ({
    value: String(curr),
    label: CurrencyNames[curr] || 'USD',
  })) || [];

  // 杠杆选项
  const leverageOptions = config?.leverageAvailable?.map((lev) => ({
    value: String(lev),
    label: `${lev}:1`,
  })) || [];

  // 获取当前选中的显示名称
  const selectedPlatformLabel = platformOptions.find(o => o.value === platform)?.shortLabel || '-';
  const selectedTypeLabel = accountTypeOptions.find(o => o.value === accountType)?.label || '-';
  const selectedCurrencyLabel = currencyOptions.find(o => o.value === currency)?.label || '-';
  const selectedLeverageLabel = leverageOptions.find(o => o.value === leverage)?.label || '-';

  const canProceed = !!platform && !!accountType && !!currency && !!leverage;

  // Stepper 步骤定义
  const stepperSteps = useMemo(() => [
    { id: 'info', label: t('step.information'), number: 1 },
    { id: 'review', label: t('step.review'), number: 2 },
  ], [t]);

  const stepperCurrentStep = step === 1 ? 'info' : 'review';
  const stepperCompletedSteps = step === 2 ? ['info'] : [];

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose(); else onOpenChange(v); }}>
      <DialogContent
        className="h-[800px]! flex flex-col justify-between"
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        <div className="flex flex-col gap-6">
          <DialogHeader>
            <DialogTitle>{t('modal.createLiveAccount')}</DialogTitle>
          </DialogHeader>

          {/* 步骤指示器 */}
          <Stepper
            steps={stepperSteps}
            currentStep={stepperCurrentStep}
            completedSteps={stepperCompletedSteps}
          />

          {/* Step 1: 表单 */}
          {step === 1 && (
            <div className="flex flex-col gap-5">
              {/* 平台 - 卡片选择（设计稿：选中=primary边框+右下角勾+primary文字） */}
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
                        <svg width="20" height="20" viewBox="0 0 20 20" fill="none" className="shrink-0">
                          <rect width="20" height="20" rx="4" fill="currentColor" fillOpacity="0.15" />
                          <text x="10" y="14" textAnchor="middle" fontSize="8" fontWeight="600" fill="currentColor">
                            {opt.shortLabel?.includes('5') ? 'MT5' : 'MT4'}
                          </text>
                        </svg>
                        <span className="flex-1 text-left text-base font-medium">{opt.label}</span>
                        {isSelected && (
                          <svg width="16" height="16" viewBox="0 0 16 16" fill="none" className="absolute -bottom-px -right-px">
                            <path d="M0 16L16 16L16 0L0 16Z" fill="var(--color-primary)" />
                            <path d="M12 10.5L10 12.5L8.5 11" stroke="white" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                          </svg>
                        )}
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* 账户类型 */}
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

              {/* 货币 */}
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

              {/* 杠杆 */}
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

              {/* 推荐代理（设计稿：请填写代理） */}
              <Input
                label={t('fields.referCode')}
                required
                value={referCode}
                onChange={(e) => setReferCode(e.target.value)}
                placeholder={t('placeholder.referCode')}
              />
            </div>
          )}

          {/* Step 2: 审查 */}
          {step === 2 && (
            <div className="flex flex-col gap-5">
              <h3 className="text-base font-semibold text-text-primary">
                {t('review.basicInfo')}
              </h3>
              <div className="flex flex-col gap-4">
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm text-text-secondary">{t('fields.platform')}：</span>
                  <span className="text-sm text-text-primary">{selectedPlatformLabel}</span>
                </div>
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm text-text-secondary">{t('fields.type')}：</span>
                  <span className="text-sm text-text-primary">{selectedTypeLabel}</span>
                </div>
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm text-text-secondary">{t('fields.currency')}：</span>
                  <span className="text-sm text-text-primary">{selectedCurrencyLabel}</span>
                </div>
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm text-text-secondary">{t('fields.leverage')}：</span>
                  <span className="text-sm text-text-primary">{selectedLeverageLabel}</span>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* 底部按钮 */}
        <DialogFooter className="flex-row! items-center justify-between! pt-5">
          {step === 1 ? (
            <>
              <div />
              <div className="flex gap-2 md:gap-3">
                <Button
                  variant="secondary"
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
                  variant="secondary"
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
