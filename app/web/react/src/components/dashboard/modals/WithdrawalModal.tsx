'use client';

import { useEffect, useState, useMemo, useCallback, useRef } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button, Checkbox, BalanceShow, Input } from '@/components/ui';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import { Stepper } from '@/components/ui/Stepper';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/radix/Select';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { useUserStore } from '@/stores/userStore';
import {
  getWalletWithdrawGroups,
  getWalletWithdrawGroupInfo,
  getPaymentInfoList,
  createPaymentInfo,
  submitWalletWithdrawal,
  sendTransferVerificationCode,
} from '@/actions';
import type { PaymentInfo } from '@/actions/payment';
import { getRegionCodes } from '@/core/data/phonesData';
import type {
  Wallet,
  PaymentMethodGroup,
  PaymentMethodGroupInfo,
} from '@/types/wallet';

interface BankFormData {
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

interface USDTFormData {
  name: string;
  walletAddress: string;
}

const INITIAL_BANK_FORM: BankFormData = {
  name: '',
  holder: '',
  bankName: '',
  branchName: '',
  state: '',
  city: '',
  accountNo: '',
  confirmAccountNo: '',
  bankCountry: 'cn',
};

const INITIAL_USDT_FORM: USDTFormData = {
  name: '',
  walletAddress: '',
};

interface WithdrawalModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  wallet: Wallet;
  type?: 'wallet' | 'account';
  accountNumber?: string | number;
  onSuccess?: () => void;
}

type Step = 1 | 2 | 3 | 4 | 5;

const USDT_PLATFORM = 240;

function resolvePaymentLogoSrc(logo: string | undefined, name: string): string {
  const fallback = `/images/wallet/${encodeURIComponent(name)}.png`;
  if (!logo) return fallback;

  const src = logo.trim();
  if (!src) return fallback;

  if (src.startsWith('/') || src.startsWith('data:') || src.startsWith('blob:')) {
    return src;
  }

  if (/^https?:\/\//i.test(src)) {
    try {
      new URL(src);
      return src;
    } catch {
      return fallback;
    }
  }

  if (/^[a-zA-Z][a-zA-Z\d+.-]*:/.test(src)) {
    return fallback;
  }

  return `/${src.replace(/^\/+/, '')}`;
}

export function WithdrawalModal({
  open,
  onOpenChange,
  wallet,
  type = 'wallet',
  accountNumber,
  onSuccess,
}: WithdrawalModalProps) {
  const t = useTranslations('wallet');
  const tCommon = useTranslations('common');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess, showError } = useToast();
  const isPasswordChangedWithin24h = useUserStore(
    (s) => s.siteConfig?.passwordChangedWithinLast24h === true
  );

  const [step, setStep] = useState<Step>(1);

  // Step 1
  const [groups, setGroups] = useState<PaymentMethodGroup[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<PaymentMethodGroup | null>(null);
  const [isLoadingGroups, setIsLoadingGroups] = useState(false);

  // Step 2
  const [groupInfo, setGroupInfo] = useState<PaymentMethodGroupInfo | null>(null);
  const [isLoadingInfo, setIsLoadingInfo] = useState(false);

  // Step 3
  const [amount, setAmount] = useState('');
  const [amountError, setAmountError] = useState('');
  const [paymentInfos, setPaymentInfos] = useState<PaymentInfo[]>([]);
  const [selectedPaymentIndex, setSelectedPaymentIndex] = useState('');
  const [confirmed, setConfirmed] = useState(false);
  const [isLoadingPaymentInfos, setIsLoadingPaymentInfos] = useState(false);

  // Inline add account form
  const [showAddAccountForm, setShowAddAccountForm] = useState(false);
  const [bankFormData, setBankFormData] = useState<BankFormData>(INITIAL_BANK_FORM);
  const [usdtFormData, setUsdtFormData] = useState<USDTFormData>(INITIAL_USDT_FORM);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [isSubmittingAccount, setIsSubmittingAccount] = useState(false);

  // Step 4
  const [verificationCode, setVerificationCode] = useState('');
  const [isSendingCode, setIsSendingCode] = useState(false);
  const [countdown, setCountdown] = useState(0);
  const countdownTimerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const withdrawalTargetId = type === 'account' ? accountNumber : wallet?.hashId;

  // Step 1: load groups
  useEffect(() => {
    if (open && withdrawalTargetId) {
      setIsLoadingGroups(true);
      execute(getWalletWithdrawGroups, withdrawalTargetId, type)
        .then((result) => {
          if (result.success && result.data) {
            console.log('result.data', result.data);
            setGroups(result.data);
          }
        })
        .finally(() => setIsLoadingGroups(false));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, withdrawalTargetId, type]);

  const handleClose = useCallback(() => {
    onOpenChange(false);
    setTimeout(() => {
      setStep(1);
      setGroups([]);
      setSelectedGroup(null);
      setGroupInfo(null);
      setAmount('');
      setAmountError('');
      setPaymentInfos([]);
      setSelectedPaymentIndex('');
      setConfirmed(false);
      setShowAddAccountForm(false);
      setBankFormData(INITIAL_BANK_FORM);
      setUsdtFormData(INITIAL_USDT_FORM);
      setFormErrors({});
      setVerificationCode('');
      setCountdown(0);
      if (countdownTimerRef.current) {
        clearInterval(countdownTimerRef.current);
        countdownTimerRef.current = null;
      }
    }, 200);
  }, [onOpenChange]);

  // Step 1 -> 2: load group info
  const loadGroupInfo = useCallback(async () => {
    if (isPasswordChangedWithin24h || !selectedGroup || !withdrawalTargetId) return;
    setIsLoadingInfo(true);
    try {
      const result = await execute(
        getWalletWithdrawGroupInfo,
        selectedGroup.hashId,
        withdrawalTargetId,
        type
      );
      if (result.success && result.data) {
        setGroupInfo(result.data);
        setStep(2);
      }
    } finally {
      setIsLoadingInfo(false);
    }
  }, [selectedGroup, execute, withdrawalTargetId, type, isPasswordChangedWithin24h]);

  // Fetch and filter payment infos by service type
  const fetchPaymentInfos = useCallback(async () => {
    const result = await execute(getPaymentInfoList);
    if (result.success && result.data) {
      const isUSDT = selectedGroup?.name?.includes('USDT') ?? false;
      const filtered = isUSDT
        ? result.data.filter((p) => p.paymentPlatform === USDT_PLATFORM)
        : result.data.filter((p) => p.paymentPlatform !== USDT_PLATFORM);

      setPaymentInfos(filtered);
      return filtered;
    }
    return [];
  }, [execute, selectedGroup]);

  // Step 2 -> 3: load payment methods
  const loadPaymentInfos = useCallback(async () => {
    setIsLoadingPaymentInfos(true);
    try {
      await fetchPaymentInfos();
      setStep(3);
    } finally {
      setIsLoadingPaymentInfos(false);
    }
  }, [fetchPaymentInfos]);

  const tBank = useTranslations('profile.bankInfos');

  // Country options for bank country select
  const countryOptions = useMemo(
    () =>
      Object.entries(getRegionCodes()).map(([code, data]) => ({
        value: code,
        label: data.name,
      })),
    []
  );

  // Validate and submit inline add account form
  const handleAddAccountSubmit = useCallback(async () => {
    const isUSDT = selectedGroup?.name?.includes('USDT') ?? false;
    const newErrors: Record<string, string> = {};

    if (isUSDT) {
      if (!usdtFormData.name.trim()) newErrors.name = tBank('errors.nameRequired');
      if (!usdtFormData.walletAddress.trim())
        newErrors.walletAddress = tBank('errors.walletAddressRequired');
    } else {
      if (!bankFormData.name.trim()) newErrors.name = tBank('errors.nameRequired');
      if (!bankFormData.holder.trim()) newErrors.holder = tBank('errors.holderRequired');
      if (!bankFormData.bankName.trim()) newErrors.bankName = tBank('errors.bankNameRequired');
      if (!bankFormData.branchName.trim()) newErrors.branchName = tBank('errors.branchNameRequired');
      if (!bankFormData.state.trim()) newErrors.state = tBank('errors.stateRequired');
      if (!bankFormData.city.trim()) newErrors.city = tBank('errors.cityRequired');
      if (!bankFormData.accountNo.trim()) newErrors.accountNo = tBank('errors.accountNoRequired');
      if (!bankFormData.confirmAccountNo.trim())
        newErrors.confirmAccountNo = tBank('errors.confirmAccountNoRequired');
      if (bankFormData.accountNo !== bankFormData.confirmAccountNo)
        newErrors.confirmAccountNo = tBank('errors.accountNoMismatch');
    }

    setFormErrors(newErrors);
    if (Object.keys(newErrors).length > 0) return;

    const platform = isUSDT ? USDT_PLATFORM : 100;
    const formData = isUSDT ? usdtFormData : bankFormData;
    const payload = {
      paymentPlatform: platform as 100 | 240,
      name: formData.name,
      info: isUSDT
        ? { name: usdtFormData.name, walletAddress: usdtFormData.walletAddress }
        : {
            name: bankFormData.name,
            holder: bankFormData.holder,
            bankName: bankFormData.bankName,
            branchName: bankFormData.branchName,
            state: bankFormData.state,
            city: bankFormData.city,
            accountNo: bankFormData.accountNo,
            confirmAccountNo: bankFormData.confirmAccountNo,
            bankCountry: bankFormData.bankCountry,
          },
    };

    setIsSubmittingAccount(true);
    try {
      const result = await execute(createPaymentInfo, payload);
      if (result.success) {
        showSuccess(t('withdraw.addAccountSuccess'));
        setShowAddAccountForm(false);
        setBankFormData(INITIAL_BANK_FORM);
        setUsdtFormData(INITIAL_USDT_FORM);
        setFormErrors({});
        const refreshed = await fetchPaymentInfos();
        if (refreshed.length > 0) {
          setSelectedPaymentIndex(String(refreshed.length - 1));
        }
      }
    } finally {
      setIsSubmittingAccount(false);
    }
  }, [selectedGroup, bankFormData, usdtFormData, execute, fetchPaymentInfos, showSuccess, t, tBank]);

  // Amount validation
  const validateAmount = useCallback(
    (val: string): boolean => {
      const num = Number(val);
      if (!num || num <= 0) {
        setAmountError(t('withdraw.enterAmount'));
        return false;
      }
      if (num > wallet.balance / 100) {
        setAmountError(t('withdraw.insufficientBalance'));
        return false;
      }
      if (groupInfo?.range) {
        const [min, max] = groupInfo.range;
        if (num < min || num > max) {
          setAmountError(t('withdraw.amountOutOfRange'));
          return false;
        }
      }
      setAmountError('');
      return true;
    },
    [t, wallet.balance, groupInfo]
  );

  // Step 3 validation
  const validateStep3 = useCallback((): boolean => {
    if (!validateAmount(amount)) return false;
    if (selectedPaymentIndex === '') {
      showError(t('withdraw.selectPaymentInfo'));
      return false;
    }
    if (!confirmed) {
      showError(t('withdraw.confirmRequired'));
      return false;
    }
    return true;
  }, [amount, selectedPaymentIndex, confirmed, validateAmount, showError, t]);

  // Send verification code
  const handleSendCode = useCallback(async () => {
    if (countdown > 0) return;
    setIsSendingCode(true);
    try {
      const result = await execute(sendTransferVerificationCode, 'Withdrawal');
      if (result.success && result.data) {
        const expires = result.data.expiresIn;
        setCountdown(expires);
        if (countdownTimerRef.current) clearInterval(countdownTimerRef.current);
        countdownTimerRef.current = setInterval(() => {
          setCountdown((prev) => {
            if (prev <= 1) {
              if (countdownTimerRef.current) {
                clearInterval(countdownTimerRef.current);
                countdownTimerRef.current = null;
              }
              return 0;
            }
            return prev - 1;
          });
        }, 1000);
        showSuccess(t('withdraw.codeSent'));
      }
    } finally {
      setIsSendingCode(false);
    }
  }, [countdown, execute, showSuccess, t]);

  // Submit withdrawal
  const handleSubmit = useCallback(async () => {
    if (!verificationCode.trim()) {
      showError(t('withdraw.codeRequired'));
      return;
    }

    const selectedInfo = paymentInfos[Number(selectedPaymentIndex)];
    if (!selectedInfo || !selectedGroup) return;

    const isUSDT = selectedInfo.paymentPlatform === USDT_PLATFORM;
    const info = selectedInfo.info;

    const requestData = isUSDT && 'walletAddress' in info
      ? {
          returnUrl: typeof window !== 'undefined' ? window.location.href : '',
          walletAddress: info.walletAddress,
        }
      : 'holder' in info
      ? {
          returnUrl: typeof window !== 'undefined' ? window.location.href : '',
          name: info.holder,
          accountName: info.holder,
          accountNo: info.accountNo,
          accountNumber: info.accountNo,
          bankName: info.bankName,
          branchName: info.branchName,
          state: info.state,
          city: info.city,
          bankCountry: info.bankCountry,
        }
      : {
          returnUrl: typeof window !== 'undefined' ? window.location.href : '',
        };

    const result = await execute(submitWalletWithdrawal, wallet.hashId, {
      hashId: selectedGroup.hashId,
      amount: parseFloat(amount),
      verificationCode,
      request: requestData,
    });

    if (result.success) {
      setStep(5);
      onSuccess?.();
    }
  }, [
    verificationCode,
    paymentInfos,
    selectedPaymentIndex,
    selectedGroup,
    execute,
    wallet.hashId,
    amount,
    onSuccess,
    showError,
    t,
  ]);

  // Cleanup timer
  useEffect(() => {
    return () => {
      if (countdownTimerRef.current) {
        clearInterval(countdownTimerRef.current);
      }
    };
  }, []);

  // Stepper
  const stepperSteps = useMemo(
    () => [
      { id: 'channel', label: t('withdraw.step1Title'), number: 1 },
      { id: 'notice', label: t('withdraw.step2Title'), number: 2 },
      { id: 'fill', label: t('withdraw.step3Title'), number: 3 },
      { id: 'verify', label: t('withdraw.step4Title'), number: 4 },
      { id: 'guide', label: t('withdraw.step5Title'), number: 5 },
    ],
    [t]
  );

  const stepIdMap: Record<Step, string> = {
    1: 'channel',
    2: 'notice',
    3: 'fill',
    4: 'verify',
    5: 'guide',
  };

  const stepperCurrentStep = stepIdMap[step];

  const stepperCompletedSteps = useMemo(() => {
    const ids = ['channel', 'notice', 'fill', 'verify', 'guide'];
    return ids.slice(0, step - 1);
  }, [step]);

  const selectedPaymentInfo = useMemo(() => {
    if (selectedPaymentIndex === '') return null;
    return paymentInfos[Number(selectedPaymentIndex)] || null;
  }, [selectedPaymentIndex, paymentInfos]);

  const isUSDTService = selectedGroup?.name?.includes('USDT') ?? false;

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
        <div className="flex flex-1 flex-col gap-6 overflow-hidden">
          <DialogHeader>
            <DialogTitle>{t('action.withdraw')}</DialogTitle>
          </DialogHeader>

          {isPasswordChangedWithin24h && (
            <div className="text-primary rounded-lg bg-error/10 p-3 text-sm text-error">
              {t('withdraw.passwordChangeRestrictionNotice')}
            </div>
          )}

          <Stepper
            steps={stepperSteps}
            currentStep={stepperCurrentStep}
            completedSteps={stepperCompletedSteps}
          />

          <div className="flex-1 overflow-y-auto">
            {/* Step 1: Select Channel */}
            {step === 1 && (
              <div className="flex flex-col gap-5">
                {isLoadingGroups ? (
                  <div className="space-y-3 w-full">
                    {[1, 2, 3].map((i) => (
                      <div
                        key={i}
                        className="h-16 bg-surface-secondary rounded-lg animate-pulse"
                      />
                    ))}
                  </div>
                ) : groups.length === 0 ? (
                  <p className="text-sm text-text-secondary py-8 text-center">
                    {t('withdraw.noMethods')}
                  </p>
                ) : (
                  <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                    {groups.map((group) => {
                      const isDisabled = group.isActive === false || isPasswordChangedWithin24h;
                      const isSelected = !isDisabled && selectedGroup?.hashId === group.hashId;
                      const logoSrc = resolvePaymentLogoSrc(group.logo, group.name);
                      return (
                        <button
                          key={group.hashId}
                          type="button"
                          disabled={isDisabled}
                          onClick={() => setSelectedGroup(group)}
                          className={`relative flex items-start gap-4 overflow-hidden rounded-lg border p-4 text-left transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${
                            isDisabled
                              ? 'border-border bg-surface-secondary'
                              : 'cursor-pointer'
                          } ${
                            isSelected
                              ? 'border-primary bg-surface'
                              : 'border-border bg-surface hover:border-primary/50'
                          }`}
                        >
                          {logoSrc && (
                            <Image
                              src={logoSrc}
                              alt={group.name}
                              width={48}
                              height={48}
                              className="shrink-0 rounded"
                            />
                          )}
                          <div className="flex flex-1 flex-col gap-1">
                            <span className="text-base font-medium text-text-primary">
                              {group.name}
                            </span>
                            <span className="text-xs text-text-secondary">
                              {t('withdraw.fee')}：0%
                            </span>
                            <span className="text-xs text-text-secondary">
                              {t('withdraw.processing')}：{'< 1'}{t('withdraw.hour')}
                            </span>
                          </div>
                          {isSelected && !isDisabled && (
                            <svg
                              width="24"
                              height="24"
                              viewBox="0 0 24 24"
                              fill="none"
                              className="absolute -bottom-px -right-px"
                            >
                              <path
                                d="M0 24L24 24L24 0L0 24Z"
                                fill="var(--color-primary)"
                              />
                              <path
                                d="M17 15L14.5 17.5L12.5 15.5"
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
                )}
              </div>
            )}

            {/* Step 2: Policy / Notes */}
            {step === 2 && groupInfo && (
              <div className="flex flex-col gap-5">
                <h3 className="text-lg font-semibold text-text-primary">
                  {t('withdraw.policyTitle')}
                </h3>
                {groupInfo.policy ? (
                  <div
                    className="prose prose-sm max-w-none text-text-secondary dark:prose-invert"
                    dangerouslySetInnerHTML={{ __html: groupInfo.policy }}
                  />
                ) : groupInfo.notes && groupInfo.notes.length > 0 ? (
                  <div className="space-y-2">
                    {groupInfo.notes.map((note, i) => (
                      <p key={i} className="text-sm text-text-secondary">
                        {note}
                      </p>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-text-secondary">
                    {t('withdraw.noPolicy')}
                  </p>
                )}
              </div>
            )}

            {/* Step 3: Fill Info */}
            {step === 3 && (
              <div className="flex flex-col gap-5">
                {/* Available Balance */}
                <div className="flex items-center gap-2 rounded-lg bg-surface-secondary p-4">
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 20 20"
                    fill="none"
                    className="shrink-0 text-primary"
                  >
                    <circle cx="10" cy="10" r="9" stroke="currentColor" strokeWidth="2" />
                    <text
                      x="10"
                      y="14"
                      textAnchor="middle"
                      fontSize="11"
                      fontWeight="600"
                      fill="currentColor"
                    >
                      i
                    </text>
                  </svg>
                  <span className="text-sm text-text-primary">
                    {t('withdraw.availableBalance')}{' '}
                    <BalanceShow
                      balance={wallet.balance}
                      currencyId={wallet.currencyId}
                      className="font-semibold"
                    />
                  </span>
                </div>

                <h3 className="text-base font-semibold text-text-primary">
                  {t('withdraw.withdrawFromCurrentWallet')}
                </h3>

                {isLoadingPaymentInfos ? (
                  <div className="space-y-3">
                    {[1, 2].map((i) => (
                      <div
                        key={i}
                        className="h-12 bg-surface-secondary rounded-lg animate-pulse"
                      />
                    ))}
                  </div>
                ) : (
                  <>
                    {/* Amount */}
                    <div className="flex flex-col gap-2">
                      <label className="flex items-center text-sm font-medium text-text-secondary">
                        <span className="mr-1 text-primary">*</span>
                        {t('withdraw.amount')}
                      </label>
                      <input
                        type="number"
                        value={amount}
                        onChange={(e) => {
                          setAmount(e.target.value);
                          if (amountError) validateAmount(e.target.value);
                        }}
                        onBlur={() => amount && validateAmount(amount)}
                        className="h-12 w-full rounded bg-input-bg px-3 text-sm font-medium text-text-primary outline-none"
                        placeholder={
                          groupInfo?.range
                            ? `${groupInfo.range[0]} - ${groupInfo.range[1]}`
                            : '0.00'
                        }
                      />
                      {amountError && (
                        <span className="text-xs text-error">{amountError}</span>
                      )}
                      {groupInfo?.range && (
                        <span className="text-xs text-text-secondary">
                          {t('withdraw.range')}: ${groupInfo.range[0]} - $
                          {groupInfo.range[1]}
                        </span>
                      )}
                    </div>

                    {/* Target Account: Select + Add Button */}
                    <div className="flex flex-col gap-3">
                      <label className="flex items-center text-sm font-medium text-text-secondary">
                        <span className="mr-1 text-primary">*</span>
                        {t('withdraw.targetAccount')}
                      </label>
                      <div className="flex items-center gap-3">
                        <div className="flex-1">
                          {paymentInfos.length === 0 ? (
                            <div className="flex h-12 items-center rounded bg-input-bg px-3 text-sm text-text-secondary">
                              {isUSDTService
                                ? t('withdraw.noUsdtWallet')
                                : t('withdraw.noBankAccount')}
                            </div>
                          ) : (
                            <Select
                              value={selectedPaymentIndex}
                              onValueChange={setSelectedPaymentIndex}
                            >
                              <SelectTrigger className="h-12 w-full bg-input-bg">
                                <SelectValue
                                  placeholder={
                                    isUSDTService
                                      ? t('withdraw.walletAddress')
                                      : t('withdraw.selectBankAccount')
                                  }
                                />
                              </SelectTrigger>
                              <SelectContent>
                                {paymentInfos.map((info, index) => (
                                  <SelectItem key={info.id} value={String(index)}>
                                    {'walletAddress' in info.info
                                      ? info.info.walletAddress || '-'
                                      : `${info.info.bankName || info.name}${
                                          info.info.branchName
                                            ? ` (${info.info.branchName})`
                                            : ''
                                        } - ${info.info.accountNo || '-'}`}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          )}
                        </div>
                        <Button
                          variant="primary"
                          size="sm"
                          className="shrink-0"
                          onClick={() => {
                            setShowAddAccountForm(!showAddAccountForm);
                            if (showAddAccountForm) {
                              setBankFormData(INITIAL_BANK_FORM);
                              setUsdtFormData(INITIAL_USDT_FORM);
                              setFormErrors({});
                            }
                          }}
                        >
                          {showAddAccountForm
                            ? tCommon('cancel')
                            : t('withdraw.addNewAccount')}
                        </Button>
                      </div>

                      {/* Inline Add Account Form */}
                      {showAddAccountForm && (
                        <div className="flex flex-col gap-4 rounded-lg border border-border p-5">
                          {isUSDTService ? (
                            <>
                              <Input
                                label={tBank('accountName')}
                                value={usdtFormData.name}
                                onChange={(e) =>
                                  setUsdtFormData({ ...usdtFormData, name: e.target.value })
                                }
                                error={formErrors.name}
                                required
                              />
                              <Input
                                label={tBank('usdtWalletAddress')}
                                value={usdtFormData.walletAddress}
                                onChange={(e) =>
                                  setUsdtFormData({
                                    ...usdtFormData,
                                    walletAddress: e.target.value,
                                  })
                                }
                                error={formErrors.walletAddress}
                                placeholder={tBank('walletPlaceholder')}
                                required
                              />
                            </>
                          ) : (
                            <>
                              <Input
                                label={tBank('accountName')}
                                value={bankFormData.name}
                                onChange={(e) =>
                                  setBankFormData({ ...bankFormData, name: e.target.value })
                                }
                                error={formErrors.name}
                                required
                              />
                              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                                <Input
                                  label={tBank('accountHolder')}
                                  value={bankFormData.holder}
                                  onChange={(e) =>
                                    setBankFormData({ ...bankFormData, holder: e.target.value })
                                  }
                                  error={formErrors.holder}
                                  required
                                />
                                <SearchableSelect
                                  label={tBank('bankCountry')}
                                  value={countryOptions.find(
                                    (opt) => opt.value === bankFormData.bankCountry
                                  )}
                                  onChange={(option) => {
                                    const selected = option as { value: string; label: string } | null;
                                    setBankFormData({
                                      ...bankFormData,
                                      bankCountry: selected?.value || 'cn',
                                    });
                                  }}
                                  options={countryOptions}
                                  placeholder={tBank('selectCountry')}
                                  error={formErrors.bankCountry}
                                />
                              </div>
                              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                                <Input
                                  label={tBank('bsb')}
                                  value={bankFormData.branchName}
                                  onChange={(e) =>
                                    setBankFormData({ ...bankFormData, branchName: e.target.value })
                                  }
                                  error={formErrors.branchName}
                                  placeholder={tBank('bsbPlaceholder')}
                                />
                                <Input
                                  label={tBank('swiftCode')}
                                  value={bankFormData.state}
                                  onChange={(e) =>
                                    setBankFormData({ ...bankFormData, state: e.target.value })
                                  }
                                  error={formErrors.state}
                                  placeholder={tBank('swiftPlaceholder')}
                                />
                              </div>
                              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                                <Input
                                  label={tBank('bankName')}
                                  value={bankFormData.bankName}
                                  onChange={(e) =>
                                    setBankFormData({ ...bankFormData, bankName: e.target.value })
                                  }
                                  error={formErrors.bankName}
                                  required
                                />
                                <Input
                                  label={tBank('branchName')}
                                  value={bankFormData.city}
                                  onChange={(e) =>
                                    setBankFormData({ ...bankFormData, city: e.target.value })
                                  }
                                  error={formErrors.city}
                                  required
                                />
                              </div>
                              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                                <Input
                                  label={tBank('accountNumber')}
                                  value={bankFormData.accountNo}
                                  onChange={(e) =>
                                    setBankFormData({ ...bankFormData, accountNo: e.target.value })
                                  }
                                  error={formErrors.accountNo}
                                  required
                                />
                                <Input
                                  label={tBank('confirmAccountNumber')}
                                  value={bankFormData.confirmAccountNo}
                                  onChange={(e) =>
                                    setBankFormData({
                                      ...bankFormData,
                                      confirmAccountNo: e.target.value,
                                    })
                                  }
                                  error={formErrors.confirmAccountNo}
                                  required
                                />
                              </div>
                            </>
                          )}
                          <div className="flex justify-end">
                            <Button
                              variant="primary"
                              size="sm"
                              loading={isSubmittingAccount}
                              onClick={handleAddAccountSubmit}
                            >
                              {tCommon('submit')}
                            </Button>
                          </div>
                        </div>
                      )}
                    </div>

                    {/* Confirm Checkbox */}
                    <div className="flex flex-col gap-2">
                      <label className="flex items-center text-sm font-medium text-text-secondary">
                        <span className="mr-1 text-primary">*</span>
                        {t('withdraw.notesLabel')}
                      </label>
                      <Checkbox
                        checked={confirmed}
                        onCheckedChange={(checked) => setConfirmed(checked === true)}
                        label={t('withdraw.confirmCheckbox')}
                      />
                    </div>
                  </>
                )}
              </div>
            )}

            {/* Step 4: Review & Verify */}
            {step === 4 && (
              <div className="flex flex-col gap-5">
                {/* Available Balance */}
                <div className="flex items-center gap-2 rounded-lg bg-surface-secondary p-4">
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 20 20"
                    fill="none"
                    className="shrink-0 text-primary"
                  >
                    <circle cx="10" cy="10" r="9" stroke="currentColor" strokeWidth="2" />
                    <text
                      x="10"
                      y="14"
                      textAnchor="middle"
                      fontSize="11"
                      fontWeight="600"
                      fill="currentColor"
                    >
                      i
                    </text>
                  </svg>
                  <span className="text-sm text-text-primary">
                    {t('withdraw.availableBalance')}{' '}
                    <BalanceShow
                      balance={wallet.balance}
                      currencyId={wallet.currencyId}
                      className="font-semibold"
                    />
                  </span>
                </div>

                {/* Review Details */}
                <h3 className="text-base font-semibold text-text-primary">
                  {t('withdraw.reviewTitle')}
                </h3>
                <div className="flex flex-col gap-4 rounded-lg border border-border p-5">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-text-secondary">
                      {t('withdraw.operation')}
                    </span>
                    <span className="text-sm text-text-primary">
                      {t('withdraw.withdrawFromWallet')}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-text-secondary">
                      {t('withdraw.amount')}
                    </span>
                    <span className="text-sm font-semibold text-primary">{amount}</span>
                  </div>
                  {selectedPaymentInfo && (
                    <>
                      {'walletAddress' in selectedPaymentInfo.info ? (
                        <div className="flex items-center justify-between">
                          <span className="text-sm text-text-secondary">
                            {t('withdraw.walletAddress')}
                          </span>
                          <span className="text-sm text-text-primary max-w-[200px] truncate">
                            {selectedPaymentInfo.info.walletAddress}
                          </span>
                        </div>
                      ) : (
                        <>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-text-secondary">
                              {t('withdraw.accountName')}
                            </span>
                            <span className="text-sm text-text-primary">
                              {selectedPaymentInfo.info.holder} -{' '}
                              {selectedPaymentInfo.info.bankName}
                              {selectedPaymentInfo.info.branchName &&
                                ` (${selectedPaymentInfo.info.branchName})`}
                            </span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-text-secondary">
                              {t('withdraw.accountNumber')}
                            </span>
                            <span className="text-sm text-text-primary">
                              {selectedPaymentInfo.info.accountNo}
                            </span>
                          </div>
                        </>
                      )}
                    </>
                  )}
                </div>

                {/* Verification Code */}
                <div className="flex flex-col gap-2">
                  <label className="flex items-center text-sm font-medium text-text-secondary">
                    <span className="mr-1 text-primary">*</span>
                    {t('withdraw.verificationCode')}
                  </label>
                  <div className="flex items-center gap-3">
                    <input
                      type="text"
                      value={verificationCode}
                      onChange={(e) => setVerificationCode(e.target.value)}
                      className="h-12 flex-1 rounded bg-input-bg px-3 text-sm font-medium text-text-primary outline-none"
                      placeholder={t('withdraw.enterCode')}
                    />
                    <Button
                      variant="primary"
                      size="sm"
                      disabled={isSendingCode || countdown > 0}
                      onClick={handleSendCode}
                      className="min-w-[120px]"
                    >
                      {countdown > 0
                        ? `${Math.floor(countdown / 60)}:${String(countdown % 60).padStart(2, '0')}`
                        : t('withdraw.sendCode')}
                    </Button>
                  </div>
                </div>

                {/* Warning note */}
                <div className="flex gap-3 text-sm text-error">
                  <span className="shrink-0">*</span>
                  <div>
                    <strong>{t('withdraw.pleaseNote')}: </strong>
                    {t('withdraw.withdrawalNote')}
                  </div>
                </div>
              </div>
            )}

            {/* Step 5: Success */}
            {step === 5 && (
              <div className="flex flex-col gap-5">
                <div className="flex items-center gap-4">
                  <div className="shrink-0">
                    <div className="flex h-16 w-16 items-center justify-center rounded-full bg-[rgba(0,78,255,0.1)]">
                      <svg width="32" height="32" viewBox="0 0 24 24" fill="none">
                        <path
                          d="M20 6L9 17l-5-5"
                          stroke="#004eff"
                          strokeWidth="2"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                    </div>
                  </div>
                  <div>
                    <h4 className="mb-1 text-base font-semibold text-text-primary">
                      {t('withdraw.orderCreated')}
                    </h4>
                    <p className="text-sm text-text-secondary">
                      {t('withdraw.successDesc')}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2 rounded-lg bg-surface-secondary p-4">
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 20 20"
                    fill="none"
                    className="shrink-0 text-primary"
                  >
                    <circle cx="10" cy="10" r="9" stroke="currentColor" strokeWidth="2" />
                    <text
                      x="10"
                      y="14"
                      textAnchor="middle"
                      fontSize="11"
                      fontWeight="600"
                      fill="currentColor"
                    >
                      i
                    </text>
                  </svg>
                  <span className="text-sm text-text-primary">
                    {t('withdraw.requestAmount')}{' '}
                    <BalanceShow
                      balance={parseFloat(amount || '0') * 100}
                      currencyId={wallet.currencyId}
                      className="font-semibold"
                    />
                  </span>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <DialogFooter className="flex-row! items-center justify-between! pt-5">
          {step === 1 ? (
            <>
              <div />
              <div className="flex gap-2 md:gap-5">
                <Button
                  variant="secondary"
                  onClick={handleClose}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.cancel')}
                </Button>
                <Button
                  variant="primary"
                  onClick={loadGroupInfo}
                  disabled={!selectedGroup || isLoadingInfo || isPasswordChangedWithin24h}
                  loading={isLoadingInfo}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.next')}
                </Button>
              </div>
            </>
          ) : step === 5 ? (
            <>
              <div />
              <Button
                variant="secondary"
                onClick={handleClose}
                className="w-auto min-w-20 md:w-[120px]"
              >
                {t('action.close')}
              </Button>
            </>
          ) : (
            <>
              <Button
                variant="outline"
                onClick={() => setStep((s) => (s - 1) as Step)}
                disabled={isLoading}
                className="w-auto min-w-16 md:w-[100px]"
              >
                {t('action.back')}
              </Button>
              <div className="flex gap-2 md:gap-5">
                <Button
                  variant="secondary"
                  onClick={handleClose}
                  disabled={isLoading}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.cancel')}
                </Button>
                <Button
                  variant="primary"
                  onClick={() => {
                    if (step === 2) {
                      loadPaymentInfos();
                    } else if (step === 3) {
                      if (validateStep3()) {
                        setStep(4);
                      }
                    } else if (step === 4) {
                      handleSubmit();
                    }
                  }}
                  disabled={
                    (step === 3 && (!amount || selectedPaymentIndex === '' || !confirmed)) ||
                    (step === 4 && isLoading)
                  }
                  loading={
                    (step === 2 && isLoadingPaymentInfos) || (step === 4 && isLoading)
                  }
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {step === 4 ? t('action.submit') : t('action.next')}
                </Button>
              </div>
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
