'use client';

import { useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui/radix/Button';
import { Input } from '@/components/ui/radix/Input';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import { getRegionCodes } from '@/core/data/phonesData';
import type { PaymentInfo } from '@/actions/payment';

interface BankAccountModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: BankAccountFormData) => Promise<void>;
  paymentPlatform: 100 | 240; // 100: Bank Account, 240: USDT Wallet
  initialData?: PaymentInfo | null;
  mode: 'add' | 'edit';
}

export interface BankFormData {
  name: string;
  holder: string;
  bankName: string;
  branchName: string;
  state: string;
  city: string;
  accountNo: string;
  confirmAccountNo: string;
  bankCountry: string;
}

export interface USDTFormData {
  name: string;
  walletAddress: string;
}

export type BankAccountFormData = BankFormData | USDTFormData;

export function BankAccountModal({
  isOpen,
  onClose,
  onSubmit,
  paymentPlatform,
  initialData,
  mode,
}: BankAccountModalProps) {
  const t = useTranslations('profile.bankInfos');
  const tCommon = useTranslations('common');

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Bank account form state
  const [bankFormData, setBankFormData] = useState<BankFormData>({
    name: '',
    holder: '',
    bankName: '',
    branchName: '',
    state: '',
    city: '',
    accountNo: '',
    confirmAccountNo: '',
    bankCountry: 'cn',
  });

  // USDT wallet form state
  const [usdtFormData, setUsdtFormData] = useState<USDTFormData>({
    name: '',
    walletAddress: '',
  });

  // Initialize form data when modal opens
  useEffect(() => {
    if (isOpen && mode === 'edit' && initialData) {
      if (paymentPlatform === 100 && 'holder' in initialData.info) {
        setBankFormData({
          name: initialData.name,
          holder: initialData.info.holder,
          bankName: initialData.info.bankName,
          branchName: initialData.info.branchName,
          state: initialData.info.state,
          city: initialData.info.city,
          accountNo: initialData.info.accountNo,
          confirmAccountNo: initialData.info.accountNo,
          bankCountry: initialData.info.bankCountry,
        });
      } else if (paymentPlatform === 240 && 'walletAddress' in initialData.info) {
        setUsdtFormData({
          name: initialData.name,
          walletAddress: initialData.info.walletAddress,
        });
      }
    }
  }, [isOpen, mode, initialData, paymentPlatform]);

  // Get country options for select
  const countryOptions = Object.entries(getRegionCodes()).map(([code, data]) => ({
    value: code,
    label: data.name,
  }));

  const validateBankForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!bankFormData.name.trim()) {
      newErrors.name = t('errors.nameRequired');
    }
    if (!bankFormData.holder.trim()) {
      newErrors.holder = t('errors.holderRequired');
    }
    if (!bankFormData.bankName.trim()) {
      newErrors.bankName = t('errors.bankNameRequired');
    }
    if (!bankFormData.branchName.trim()) {
      newErrors.branchName = t('errors.branchNameRequired');
    }
    if (!bankFormData.state.trim()) {
      newErrors.state = t('errors.stateRequired');
    }
    if (!bankFormData.city.trim()) {
      newErrors.city = t('errors.cityRequired');
    }
    if (!bankFormData.accountNo.trim()) {
      newErrors.accountNo = t('errors.accountNoRequired');
    }
    if (!bankFormData.confirmAccountNo.trim()) {
      newErrors.confirmAccountNo = t('errors.confirmAccountNoRequired');
    }
    if (bankFormData.accountNo !== bankFormData.confirmAccountNo) {
      newErrors.confirmAccountNo = t('errors.accountNoMismatch');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const validateUSDTForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!usdtFormData.name.trim()) {
      newErrors.name = t('errors.nameRequired');
    }
    if (!usdtFormData.walletAddress.trim()) {
      newErrors.walletAddress = t('errors.walletAddressRequired');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (paymentPlatform === 100) {
      if (!validateBankForm()) return;
    } else {
      if (!validateUSDTForm()) return;
    }

    setLoading(true);
    try {
      const formData = paymentPlatform === 100 ? bankFormData : usdtFormData;
      await onSubmit(formData);
      handleClose();
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      setErrors({});
      setBankFormData({
        name: '',
        holder: '',
        bankName: '',
        branchName: '',
        state: '',
        city: '',
        accountNo: '',
        confirmAccountNo: '',
        bankCountry: 'cn',
      });
      setUsdtFormData({
        name: '',
        walletAddress: '',
      });
      onClose();
    }
  };

  const title = mode === 'edit' ? t('editAccount') : t('addAccount');

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold text-text-primary">
            {title}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-5 mt-5">
          {paymentPlatform === 100 ? (
            // Bank Account Form
            <>
              <Input
                label={t('accountName')}
                value={bankFormData.name}
                onChange={(e) =>
                  setBankFormData({ ...bankFormData, name: e.target.value })
                }
                error={errors.name}
                required
              />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <Input
                  label={t('accountHolder')}
                  value={bankFormData.holder}
                  onChange={(e) =>
                    setBankFormData({ ...bankFormData, holder: e.target.value })
                  }
                  error={errors.holder}
                  required
                />

                <SearchableSelect
                  label={t('bankCountry')}
                  value={countryOptions.find(
                    (opt) => opt.value === bankFormData.bankCountry
                  )}
                  onChange={(option) => {
                    const selected = option as { value: string; label: string } | null;
                    setBankFormData({ ...bankFormData, bankCountry: selected?.value || 'cn' });
                  }}
                  options={countryOptions}
                  placeholder={t('selectCountry')}
                  error={errors.bankCountry}
                />
              </div>

              <Input
                label={t('bsb')}
                value={bankFormData.branchName}
                onChange={(e) =>
                  setBankFormData({ ...bankFormData, branchName: e.target.value })
                }
                error={errors.branchName}
                placeholder={t('bsbPlaceholder')}
              />

              <Input
                label={t('swiftCode')}
                value={bankFormData.state}
                onChange={(e) =>
                  setBankFormData({ ...bankFormData, state: e.target.value })
                }
                error={errors.state}
                placeholder={t('swiftPlaceholder')}
              />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <Input
                  label={t('bankName')}
                  value={bankFormData.bankName}
                  onChange={(e) =>
                    setBankFormData({ ...bankFormData, bankName: e.target.value })
                  }
                  error={errors.bankName}
                  required
                />

                <Input
                  label={t('branchName')}
                  value={bankFormData.city}
                  onChange={(e) =>
                    setBankFormData({ ...bankFormData, city: e.target.value })
                  }
                  error={errors.city}
                  required
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <Input
                  label={t('accountNumber')}
                  value={bankFormData.accountNo}
                  onChange={(e) =>
                    setBankFormData({ ...bankFormData, accountNo: e.target.value })
                  }
                  error={errors.accountNo}
                  required
                />

                <Input
                  label={t('confirmAccountNumber')}
                  value={bankFormData.confirmAccountNo}
                  onChange={(e) =>
                    setBankFormData({
                      ...bankFormData,
                      confirmAccountNo: e.target.value,
                    })
                  }
                  error={errors.confirmAccountNo}
                  required
                />
              </div>
            </>
          ) : (
            // USDT Wallet Form
            <>
              <Input
                label={t('accountName')}
                value={usdtFormData.name}
                onChange={(e) =>
                  setUsdtFormData({ ...usdtFormData, name: e.target.value })
                }
                error={errors.name}
                required
              />

              <Input
                label={t('usdtWalletAddress')}
                value={usdtFormData.walletAddress}
                onChange={(e) =>
                  setUsdtFormData({
                    ...usdtFormData,
                    walletAddress: e.target.value,
                  })
                }
                error={errors.walletAddress}
                placeholder={t('walletPlaceholder')}
                required
              />
            </>
          )}
        </div>

        <DialogFooter className="flex flex-row gap-3 sm:gap-5 mt-6 sm:mt-8 justify-end">
          <Button
            variant="outline"
            onClick={handleClose}
            disabled={loading}
            className="w-[100px] sm:w-[120px]"
          >
            {tCommon('cancel')}
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={loading}
            disabled={loading}
            className="w-[100px] sm:w-[120px]"
          >
            {tCommon('submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
