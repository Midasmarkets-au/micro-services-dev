'use client';

import { useState, useCallback, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { MagnifyingGlassIcon } from '@radix-ui/react-icons';
import { useUserStore } from '@/stores/userStore';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, Skeleton, Input, SimpleSelect } from '@/components/ui';
import type { SelectOption } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import {
  salesOpenTradeAccount,
  getSalesIBAccountConfig,
  getReferralCodeSupplement,
} from '@/actions';
import type { SalesClientAccount } from '@/types/sales';
import { AccountRoleTypes, getPlatformName } from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';

interface OpenTradeAccountModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
  onSuccess?: () => void;
}

export function OpenTradeAccountModal({ open, onOpenChange, account, onSuccess }: OpenTradeAccountModalProps) {
  const t = useTranslations('sales');
  const tAccounts = useTranslations('accounts');
  const getCurrencyName = useCurrencyName();
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const siteConfig = useUserStore((s) => s.siteConfig);
  const { showToast } = useToast();

  const [isConfigLoading, setIsConfigLoading] = useState(true);
  const [isCheckingCode, setIsCheckingCode] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [referCode, setReferCode] = useState('');
  const [referCodeValid, setReferCodeValid] = useState(false);
  const [referCodeRole, setReferCodeRole] = useState('');
  const [showForm, setShowForm] = useState(false);

  const [accountTypeOptions, setAccountTypeOptions] = useState<SelectOption[]>([]);
  const [leverageOptions, setLeverageOptions] = useState<SelectOption[]>(() =>
    (siteConfig?.leverageAvailable ?? []).map((lev) => ({ value: String(lev), label: `${lev}:1` }))
  );
  const [currencyOptions, setCurrencyOptions] = useState<SelectOption[]>(() =>
    (siteConfig?.currencyAvailable ?? []).map((id) => ({
      value: String(id),
      label: getCurrencyName(id),
    }))
  );
  const [platformOptions, setPlatformOptions] = useState<SelectOption[]>(() =>
    (siteConfig?.tradingPlatformAvailable ?? []).map((platformId) => ({
      value: String(platformId),
      label: getPlatformName(platformId),
    }))
  );

  const [selectedAccountType, setSelectedAccountType] = useState('');
  const [selectedCurrency, setSelectedCurrency] = useState('');
  const [selectedLeverage, setSelectedLeverage] = useState('');
  const [selectedPlatform, setSelectedPlatform] = useState('');

  const resetForm = useCallback(() => {
    setReferCode('');
    setReferCodeValid(false);
    setReferCodeRole('');
    setShowForm(false);
    setAccountTypeOptions([]);
    setSelectedAccountType('');
    setSelectedCurrency('');
    setSelectedLeverage('');
    setSelectedPlatform('');
  }, []);

  const loadConfig = useCallback(async () => {
    if (!salesAccount || !account) return;
    setIsConfigLoading(true);
    try {
      const result = await execute(getSalesIBAccountConfig, salesAccount.uid, account.uid);
      if (result.success && result.data) {
        const config = result.data as Record<string, unknown>;
        if (Array.isArray(config.leverages)) {
          const opts = (config.leverages as number[]).map((lev) => ({
            value: String(lev),
            label: `${lev}:1`,
          }));
          setLeverageOptions(opts);
          if (opts.length > 0) setSelectedLeverage(opts[0].value);
        }
        if (Array.isArray(config.currencies)) {
          const opts = (config.currencies as (number | { id: number })[]).map((c) => {
            const id = typeof c === 'number' ? c : c.id;
            return { value: String(id), label: getCurrencyName(id) };
          });
          setCurrencyOptions(opts);
          if (opts.length > 0) setSelectedCurrency(opts[0].value);
        }
        if (Array.isArray(config.platforms)) {
          const opts = (config.platforms as { id: number; name: string }[]).map((p) => ({
            value: String(p.id ?? p),
            label: p.name ?? String(p.id ?? p),
          }));
          setPlatformOptions(opts);
          if (opts.length > 0) setSelectedPlatform(opts[0].value);
        }
      }
    } finally {
      setIsConfigLoading(false);
    }
  }, [salesAccount, account, execute, getCurrencyName]);

  useEffect(() => {
    if (open && account) {
      resetForm();
      loadConfig();
    }
  }, [open, account, resetForm, loadConfig]);

  const getRoleLabel = (serviceType: number): string => {
    return tAccounts(`accountRole.${serviceType}`);
  };

  const handleCheckReferCode = async () => {
    if (!referCode.trim()) return;
    setIsCheckingCode(true);
    try {
      const result = await execute(getReferralCodeSupplement, referCode.trim());
      if (result.success && result.data) {
        const data = result.data as { serviceType: number; summary?: { schema?: { accountType: number }[]; allowAccountTypes?: { accountType: number }[] } };
        setReferCodeValid(true);
        setShowForm(true);
        setReferCodeRole(getRoleLabel(data.serviceType));

        let types: { accountType: number }[] = [];
        switch (data.serviceType) {
          case AccountRoleTypes.IB:
            types = data.summary?.schema ?? [];
            break;
          case AccountRoleTypes.Client:
            types = data.summary?.allowAccountTypes ?? [];
            break;
          default:
            types = data.summary?.schema ?? data.summary?.allowAccountTypes ?? [];
            break;
        }

        const opts = types.map((item) => ({
          value: String(item.accountType),
          label: tAccounts(`accountTypes.${item.accountType}`),
        }));
        setAccountTypeOptions(opts);
        if (opts.length > 0) setSelectedAccountType(opts[0].value);

        //showToast({ message: t('openAccount.referCodeValid'), type: 'success' });
      } else {
        setReferCodeValid(false);
        setShowForm(false);
      }
    } catch {
      setReferCodeValid(false);
      setShowForm(false);
    } finally {
      setIsCheckingCode(false);
    }
  };

  const handleSubmit = async () => {
    if (!salesAccount || !account) return;
    if (!selectedAccountType || !selectedCurrency || !selectedLeverage) return;

    setIsSubmitting(true);
    try {
      const formData: Record<string, unknown> = {
        referCode: referCode.trim(),
        accountType: Number(selectedAccountType),
        currencyId: Number(selectedCurrency),
        leverage: Number(selectedLeverage),
        platform: Number(selectedPlatform),
      };
      if (selectedPlatform) formData.serviceId = Number(selectedPlatform);

      const result = await execute(salesOpenTradeAccount, salesAccount.uid, account.uid, formData);
      if (result.success) {
        showToast({ message: t('openAccount.createSuccess'), type: 'success' });
        onOpenChange(false);
        onSuccess?.();
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const userName = account?.user?.displayName || account?.user?.nativeName || '';
  const formDisabled = !referCodeValid || isCheckingCode;
  const canSubmit = referCodeValid && selectedAccountType && selectedCurrency && selectedLeverage;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('openAccount.title')} - {userName}</DialogTitle>
        </DialogHeader>

        {isConfigLoading ? (
          <div className="space-y-4 py-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : (
          <div className="space-y-4 py-2">
            {/* 推荐码输入 */}
            <div>
              <label className="mb-1.5 block text-sm text-text-secondary">
                {t('openAccount.referCode')}
              </label>
              <div className="flex gap-2">
                <div className="flex-1">
                  <Input
                    value={referCode}
                    onChange={(e) => {
                      setReferCode(e.target.value);
                      if (referCodeValid) {
                        setReferCodeValid(false);
                        setShowForm(false);
                        setAccountTypeOptions([]);
                        setSelectedAccountType('');
                      }
                    }}
                    placeholder={t('openAccount.referCodePlaceholder')}
                    disabled={isCheckingCode}
                    inputSize="sm"
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') handleCheckReferCode();
                    }}
                  />
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleCheckReferCode}
                  disabled={!referCode.trim() || isCheckingCode}
                  loading={isCheckingCode}
                  className="shrink-0"
                >
                  <MagnifyingGlassIcon className="size-4" />
                </Button>
              </div>
            </div>

            {/* 推荐码角色信息 */}
            {showForm && referCodeRole && (
              <div>
                <label className="mb-1.5 block text-sm text-text-secondary">
                  {t('openAccount.role')}
                </label>
                <Input value={referCodeRole} disabled inputSize="sm" />
              </div>
            )}

            {/* 表单字段 - 仅在推荐码验证通过后显示 */}
            {showForm && (
              <>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="mb-1.5 block text-sm text-text-secondary">
                      {t('openAccount.accountType')}
                    </label>
                    <SimpleSelect
                      value={selectedAccountType}
                      onChange={setSelectedAccountType}
                      options={accountTypeOptions}
                      placeholder={t('openAccount.selectAccountType')}
                      disabled={formDisabled}
                      triggerSize="sm"
                    />
                  </div>
                  <div>
                    <label className="mb-1.5 block text-sm text-text-secondary">
                      {t('openAccount.currency')}
                    </label>
                    <SimpleSelect
                      value={selectedCurrency}
                      onChange={setSelectedCurrency}
                      options={currencyOptions}
                      placeholder={t('openAccount.selectCurrency')}
                      disabled={formDisabled}
                      triggerSize="sm"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="mb-1.5 block text-sm text-text-secondary">
                      {t('openAccount.leverage')}
                    </label>
                    <SimpleSelect
                      value={selectedLeverage}
                      onChange={setSelectedLeverage}
                      options={leverageOptions}
                      placeholder={t('openAccount.selectLeverage')}
                      disabled={formDisabled}
                      triggerSize="sm"
                    />
                  </div>
                  {platformOptions.length > 0 && (
                    <div>
                      <label className="mb-1.5 block text-sm text-text-secondary">
                        {t('openAccount.platform')}
                      </label>
                      <SimpleSelect
                        value={selectedPlatform}
                        onChange={setSelectedPlatform}
                        options={platformOptions}
                        placeholder={t('openAccount.selectPlatform')}
                        disabled={formDisabled}
                        triggerSize="sm"
                      />
                    </div>
                  )}
                </div>

                <div className="flex justify-end gap-3 pt-4">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onOpenChange(false)}
                    disabled={isSubmitting}
                  >
                    {t('action.cancel')}
                  </Button>
                  <Button
                    variant="primary"
                    size="sm"
                    loading={isSubmitting}
                    onClick={handleSubmit}
                    disabled={!canSubmit}
                  >
                    {t('action.submit')}
                  </Button>
                </div>
              </>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
