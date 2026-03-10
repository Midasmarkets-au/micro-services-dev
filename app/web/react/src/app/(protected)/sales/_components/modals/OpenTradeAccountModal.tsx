'use client';

import { useState, useCallback, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, Skeleton } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import { salesOpenTradeAccount, getSalesIBAccountConfig, getSalesAvailableAccountTypes } from '@/actions';
import type { SalesClientAccount } from '@/types/sales';

interface AccountTypeOption {
  id: number;
  name: string;
}

interface OpenTradeAccountModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
  onSuccess?: () => void;
}

export function OpenTradeAccountModal({ open, onOpenChange, account, onSuccess }: OpenTradeAccountModalProps) {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const { showToast } = useToast();

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [accountTypes, setAccountTypes] = useState<AccountTypeOption[]>([]);
  const [leverages, setLeverages] = useState<number[]>([]);
  const [currencies, setCurrencies] = useState<{ id: number; name: string }[]>([]);
  const [platforms, setPlatforms] = useState<{ id: number; name: string }[]>([]);

  const [selectedAccountType, setSelectedAccountType] = useState<number | ''>('');
  const [selectedCurrency, setSelectedCurrency] = useState<number | ''>('');
  const [selectedLeverage, setSelectedLeverage] = useState<number | ''>('');
  const [selectedPlatform, setSelectedPlatform] = useState<number | ''>('');

  const initData = useCallback(async () => {
    if (!salesAccount || !account) return;
    setIsLoading(true);
    try {
      const [configResult, typesResult] = await Promise.all([
        execute(getSalesIBAccountConfig, salesAccount.uid, account.uid),
        execute(getSalesAvailableAccountTypes, salesAccount.uid),
      ]);

      if (configResult.success && configResult.data) {
        const config = configResult.data as Record<string, unknown>;
        if (Array.isArray(config.leverages)) setLeverages(config.leverages as number[]);
        if (Array.isArray(config.currencies)) {
          setCurrencies((config.currencies as { id: number; name: string }[]).map((c) => ({
            id: c.id ?? (c as unknown as number),
            name: c.name ?? String(c.id ?? c),
          })));
        }
        if (Array.isArray(config.platforms)) {
          setPlatforms((config.platforms as { id: number; name: string }[]).map((p) => ({
            id: p.id ?? (p as unknown as number),
            name: p.name ?? String(p.id ?? p),
          })));
        }
      }

      if (typesResult.success && Array.isArray(typesResult.data)) {
        setAccountTypes(
          (typesResult.data as { id?: number; accountType?: number; name?: string }[]).map((at) => ({
            id: at.id ?? at.accountType ?? 0,
            name: at.name ?? String(at.id ?? at.accountType ?? 0),
          }))
        );
      }
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute]);

  useEffect(() => {
    if (open && account) {
      setSelectedAccountType('');
      setSelectedCurrency('');
      setSelectedLeverage('');
      setSelectedPlatform('');
      initData();
    }
  }, [open, account, initData]);

  const handleSubmit = async () => {
    if (!salesAccount || !account) return;
    if (!selectedAccountType || !selectedCurrency || !selectedLeverage) return;

    setIsSubmitting(true);
    try {
      const formData: Record<string, unknown> = {
        accountType: selectedAccountType,
        currencyId: selectedCurrency,
        leverage: selectedLeverage,
      };
      if (selectedPlatform) formData.platform = selectedPlatform;

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

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('openAccount.title')} - {userName}</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4 py-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : (
          <div className="space-y-4 py-2">
            <div>
              <label className="mb-1.5 block text-sm font-medium text-text-primary">{t('openAccount.accountType')}</label>
              <select
                value={selectedAccountType}
                onChange={(e) => setSelectedAccountType(e.target.value ? Number(e.target.value) : '')}
                className="input-field w-full rounded px-3 py-2.5 text-sm"
              >
                <option value="">{t('openAccount.selectAccountType')}</option>
                {accountTypes.map((at) => (
                  <option key={at.id} value={at.id}>{at.name}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-text-primary">{t('openAccount.currency')}</label>
              <select
                value={selectedCurrency}
                onChange={(e) => setSelectedCurrency(e.target.value ? Number(e.target.value) : '')}
                className="input-field w-full rounded px-3 py-2.5 text-sm"
              >
                <option value="">{t('openAccount.selectCurrency')}</option>
                {currencies.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-text-primary">{t('openAccount.leverage')}</label>
              <select
                value={selectedLeverage}
                onChange={(e) => setSelectedLeverage(e.target.value ? Number(e.target.value) : '')}
                className="input-field w-full rounded px-3 py-2.5 text-sm"
              >
                <option value="">{t('openAccount.selectLeverage')}</option>
                {leverages.map((lev) => (
                  <option key={lev} value={lev}>1:{lev}</option>
                ))}
              </select>
            </div>

            {platforms.length > 0 && (
              <div>
                <label className="mb-1.5 block text-sm font-medium text-text-primary">{t('openAccount.platform')}</label>
                <select
                  value={selectedPlatform}
                  onChange={(e) => setSelectedPlatform(e.target.value ? Number(e.target.value) : '')}
                  className="input-field w-full rounded px-3 py-2.5 text-sm"
                >
                  <option value="">{t('openAccount.selectPlatform')}</option>
                  {platforms.map((p) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
                </select>
              </div>
            )}

            <div className="flex justify-center pt-4">
              <Button
                variant="primary"
                size="md"
                loading={isSubmitting}
                onClick={handleSubmit}
                disabled={!selectedAccountType || !selectedCurrency || !selectedLeverage}
                className="w-60"
              >
                {t('action.submit')}
              </Button>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
