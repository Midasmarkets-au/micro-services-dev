'use client';

import { useState, useCallback, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, Skeleton, Stepper } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import {
  getSalesIBRebateRuleDetail,
  getSalesAccountDefaultLevel,
  getSalesIBAccountConfig,
  createSalesIBAgentLink,
  createSalesIBClientLink,
} from '@/actions';
import type { SalesClientAccount } from '@/types/sales';

/* eslint-disable @typescript-eslint/no-explicit-any */

interface AccountTypeConfig {
  accountType: number;
  optionName: string | null;
  selected: boolean;
  items: { cid: number; name?: string; r: number; total: number }[];
}

interface NewIbReferralLinkModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}

const LANGUAGES = [
  { code: 'en', name: 'English' },
  { code: 'zh', name: '中文' },
  { code: 'zh-tw', name: '繁體中文' },
  { code: 'ja', name: '日本語' },
  { code: 'ko', name: '한국어' },
  { code: 'vi', name: 'Tiếng Việt' },
  { code: 'th', name: 'ไทย' },
  { code: 'ms', name: 'Bahasa Melayu' },
  { code: 'id', name: 'Bahasa Indonesia' },
  { code: 'es', name: 'Español' },
  { code: 'km', name: 'ភាសាខ្មែរ' },
];

export function NewIbReferralLinkModal({ open, onOpenChange, account }: NewIbReferralLinkModalProps) {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const { showToast } = useToast();

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [step, setStep] = useState(0);

  // Step 1: Link name
  const [linkName, setLinkName] = useState('');
  // Step 2: Language
  const [selectedLanguage, setSelectedLanguage] = useState('en');
  // Step 3: Link type (agent / client)
  const [linkType, setLinkType] = useState<'agent' | 'client'>('agent');
  // Step 4: Account types + rebate
  const [accountTypeConfigs, setAccountTypeConfigs] = useState<AccountTypeConfig[]>([]);
  // Step 5: Auto-create account
  const [autoCreate, setAutoCreate] = useState(false);

  const initData = useCallback(async () => {
    if (!salesAccount || !account) return;
    setIsLoading(true);
    try {
      const [ruleResult, defaultResult, configResult] = await Promise.all([
        execute(getSalesIBRebateRuleDetail, salesAccount.uid, account.uid),
        execute(getSalesAccountDefaultLevel, salesAccount.uid, account.uid),
        execute(getSalesIBAccountConfig, salesAccount.uid, account.uid),
      ]);

      const defaultLevel = defaultResult.success ? defaultResult.data : null;
      const config = configResult.success ? configResult.data : null;
      const ruleDetail = ruleResult.success ? ruleResult.data : null;

      const allowedAccounts = (config as any)?.allowedAccounts || (ruleDetail as any)?.calculatedLevelSetting?.allowedAccounts || [];
      const configs: AccountTypeConfig[] = [];

      for (const acct of allowedAccounts) {
        const at = acct.accountType;
        const items = acct.items?.map((item: any) => ({
          cid: item.cid,
          name: item.name,
          r: 0,
          total: item.r ?? 0,
        })) || [];

        configs.push({
          accountType: at,
          optionName: acct.optionName,
          selected: configs.length === 0,
          items,
        });
      }

      setAccountTypeConfigs(configs);
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute]);

  useEffect(() => {
    if (open && account) {
      setStep(0);
      setLinkName('');
      setSelectedLanguage('en');
      setLinkType('agent');
      setAutoCreate(false);
      initData();
    }
  }, [open, account, initData]);

  const canProceed = () => {
    switch (step) {
      case 0: return linkName.trim().length > 0;
      case 1: return !!selectedLanguage;
      case 2: return true;
      case 3: return accountTypeConfigs.some((c) => c.selected);
      case 4: return true;
      default: return false;
    }
  };

  const handleNext = () => {
    if (step < 4) setStep(step + 1);
  };

  const handlePrev = () => {
    if (step > 0) setStep(step - 1);
  };

  const handleSubmit = async () => {
    if (!salesAccount || !account) return;
    setIsSubmitting(true);

    try {
      const selectedConfigs = accountTypeConfigs.filter((c) => c.selected);

      for (const config of selectedConfigs) {
        const formData: Record<string, unknown> = {
          name: linkName.trim(),
          language: selectedLanguage,
          accountType: config.accountType,
          optionName: config.optionName,
          autoCreateAccount: autoCreate,
          items: config.items.map((item) => ({ cid: item.cid, r: item.r })),
        };

        const createFn = linkType === 'agent' ? createSalesIBAgentLink : createSalesIBClientLink;
        const result = await execute(createFn, salesAccount.uid, account.uid, formData);

        if (!result.success) {
          return;
        }
      }

      showToast({ message: t('newReferralLink.success'), type: 'success' });
      onOpenChange(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const toggleAccountType = (idx: number) => {
    setAccountTypeConfigs((prev) => {
      const next = [...prev];
      next[idx] = { ...next[idx], selected: !next[idx].selected };
      return next;
    });
  };

  const updateRebateValue = (configIdx: number, itemIdx: number, value: number) => {
    setAccountTypeConfigs((prev) => {
      const next = [...prev];
      const items = [...next[configIdx].items];
      items[itemIdx] = { ...items[itemIdx], r: value };
      next[configIdx] = { ...next[configIdx], items };
      return next;
    });
  };

  const stepItems = [
    { id: 's1', label: t('newReferralLink.step1LinkName'), number: 1 },
    { id: 's2', label: t('newReferralLink.step2Language'), number: 2 },
    { id: 's3', label: t('newReferralLink.step3LinkType'), number: 3 },
    { id: 's4', label: t('newReferralLink.step4AccountType'), number: 4 },
    { id: 's5', label: t('newReferralLink.step5AutoCreate'), number: 5 },
  ];

  const currentStepId = stepItems[step]?.id ?? 's1';
  const completedStepIds = stepItems.filter((_, i) => i < step).map((s) => s.id);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[600px]">
        <DialogHeader>
          <DialogTitle>{t('newReferralLink.title')}</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4 py-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : (
          <div className="space-y-6 py-2">
            <Stepper
              steps={stepItems}
              currentStep={currentStepId}
              completedSteps={completedStepIds}
            />

            {/* Step 1: Link Name */}
            {step === 0 && (
              <div>
                <label className="mb-1.5 block text-sm font-medium text-text-primary">
                  {t('newReferralLink.step1LinkName')}
                </label>
                <input
                  type="text"
                  value={linkName}
                  onChange={(e) => setLinkName(e.target.value)}
                  placeholder={t('newReferralLink.nameRequired')}
                  className="input-field w-full rounded px-3 py-2.5 text-sm"
                />
              </div>
            )}

            {/* Step 2: Language */}
            {step === 1 && (
              <div className="grid grid-cols-3 gap-2">
                {LANGUAGES.map((lang) => (
                  <button
                    key={lang.code}
                    type="button"
                    onClick={() => setSelectedLanguage(lang.code)}
                    className={`rounded px-3 py-2 text-sm transition-colors ${
                      selectedLanguage === lang.code
                        ? 'bg-primary text-white'
                        : 'bg-surface-secondary text-text-primary hover:bg-surface-secondary/80'
                    }`}
                  >
                    {lang.name}
                  </button>
                ))}
              </div>
            )}

            {/* Step 3: Link Type */}
            {step === 2 && (
              <div className="flex gap-4">
                <button
                  type="button"
                  onClick={() => setLinkType('agent')}
                  className={`flex-1 rounded-lg border-2 px-4 py-6 text-center text-sm font-medium transition-colors ${
                    linkType === 'agent'
                      ? 'border-primary bg-primary/5 text-primary'
                      : 'border-border text-text-secondary hover:border-primary/30'
                  }`}
                >
                  {t('newReferralLink.agent')}
                </button>
                <button
                  type="button"
                  onClick={() => setLinkType('client')}
                  className={`flex-1 rounded-lg border-2 px-4 py-6 text-center text-sm font-medium transition-colors ${
                    linkType === 'client'
                      ? 'border-primary bg-primary/5 text-primary'
                      : 'border-border text-text-secondary hover:border-primary/30'
                  }`}
                >
                  {t('newReferralLink.client')}
                </button>
              </div>
            )}

            {/* Step 4: Account Type + Rebate */}
            {step === 3 && (
              <div className="space-y-4">
                <div className="flex flex-wrap gap-3">
                  {accountTypeConfigs.map((config, idx) => (
                    <label key={config.accountType} className="flex cursor-pointer items-center gap-2">
                      <input
                        type="checkbox"
                        checked={config.selected}
                        onChange={() => toggleAccountType(idx)}
                        className="h-4 w-4 rounded border-border"
                      />
                      <span className="text-sm">{config.accountType}</span>
                    </label>
                  ))}
                </div>

                {accountTypeConfigs.map((config, configIdx) => {
                  if (!config.selected) return null;
                  return (
                    <div key={config.accountType} className="rounded border border-border p-3">
                      <div className="mb-2 text-sm font-medium">{config.accountType}</div>
                      <table className="w-full border-collapse text-center text-sm">
                        <thead>
                          <tr className="bg-surface-secondary text-text-secondary">
                            <th className="px-2 py-2 font-medium">{t('newReferralLink.category')}</th>
                            <th className="px-2 py-2 font-medium">{t('newReferralLink.totalRebate')}</th>
                            <th className="px-2 py-2 font-medium">{t('newReferralLink.personalRebate')}</th>
                            <th className="px-2 py-2 font-medium">{t('newReferralLink.remainRebate')}</th>
                          </tr>
                        </thead>
                        <tbody>
                          {config.items.map((item, itemIdx) => (
                            <tr key={item.cid} className="border-t border-border">
                              <td className="px-2 py-2">{item.name || item.cid}</td>
                              <td className="px-2 py-2">{item.total}</td>
                              <td className="px-2 py-2">
                                <input
                                  type="number"
                                  step="0.1"
                                  min={0}
                                  max={item.total}
                                  value={item.r}
                                  onChange={(e) => {
                                    let val = parseFloat(e.target.value);
                                    if (isNaN(val)) val = 0;
                                    if (val > item.total) val = item.total;
                                    if (val < 0) val = 0;
                                    updateRebateValue(configIdx, itemIdx, Number(val.toFixed(1)));
                                  }}
                                  className="input-field w-20 rounded px-2 py-1 text-center"
                                />
                              </td>
                              <td className="px-2 py-2">
                                {Number((item.total - item.r).toFixed(1))}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  );
                })}
              </div>
            )}

            {/* Step 5: Auto Create Account */}
            {step === 4 && (
              <div className="flex gap-4">
                <button
                  type="button"
                  onClick={() => setAutoCreate(true)}
                  className={`flex-1 rounded-lg border-2 px-4 py-6 text-center text-sm font-medium transition-colors ${
                    autoCreate
                      ? 'border-primary bg-primary/5 text-primary'
                      : 'border-border text-text-secondary hover:border-primary/30'
                  }`}
                >
                  {t('newReferralLink.yes')}
                </button>
                <button
                  type="button"
                  onClick={() => setAutoCreate(false)}
                  className={`flex-1 rounded-lg border-2 px-4 py-6 text-center text-sm font-medium transition-colors ${
                    !autoCreate
                      ? 'border-primary bg-primary/5 text-primary'
                      : 'border-border text-text-secondary hover:border-primary/30'
                  }`}
                >
                  {t('newReferralLink.no')}
                </button>
              </div>
            )}

            {/* Navigation buttons */}
            <div className="flex items-center justify-between pt-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={handlePrev}
                disabled={step === 0}
              >
                ← {t('action.reset')}
              </Button>

              {step < 4 ? (
                <Button
                  variant="primary"
                  size="sm"
                  onClick={handleNext}
                  disabled={!canProceed()}
                >
                  {t('action.submit')} →
                </Button>
              ) : (
                <Button
                  variant="primary"
                  size="sm"
                  loading={isSubmitting}
                  onClick={handleSubmit}
                  disabled={!canProceed()}
                >
                  {t('newReferralLink.generate')}
                </Button>
              )}
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
