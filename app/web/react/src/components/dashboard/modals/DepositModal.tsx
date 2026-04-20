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
import { Button, Input } from '@/components/ui';
import { BalanceShow } from '@/components/ui/BalanceShow';
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
import {
  getDepositGroups,
  getDepositGroupInfo,
  postAccountDeposit,
  postQrCodePaid,
  getExLinkCurrencies,
  getExLinkExchangeRates,
} from '@/actions/deposit';
import type {
  DepositGroup,
  DepositGroupInfo,
  DepositResponse,
  CurrencyRate,
} from '@/types/deposit';
import { DepositActions } from '@/types/deposit';
import { CurrencyTypes } from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';
import { CreditCardForm, type CreditCardFormHandle } from './CreditCardForm';

const CREDIT_CARD_GROUP = 'Credit Card';
const EXLINK_GLOBAL_KEYWORD = 'exlink global';

interface DepositModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: { uid: number; currencyId: number } | null;
}

type Step = 1 | 2 | 3 | 4 | 5;

const HIDDEN_REQUEST_KEYS = ['returnUrl', 'currencyId'];

function getBase64ImageDataUrl(text?: string): string {
  const value = text?.trim();
  if (!value) return '';

  if (/^data:image\/[a-zA-Z0-9.+-]+;base64,/i.test(value)) return value;

  const compact = value.replace(/\s/g, '');
  const isBase64 = /^[A-Za-z0-9+/]+={0,2}$/.test(compact);
  const looksLikeImage =
    /^(iVBORw0KGgo|\/9j\/|R0lGOD|UklGR)/.test(compact) && compact.length > 100;
  if (!isBase64 || !looksLikeImage) return '';

  let mime = 'image/png';
  if (compact.startsWith('/9j/')) mime = 'image/jpeg';
  else if (compact.startsWith('R0lGOD')) mime = 'image/gif';
  else if (compact.startsWith('UklGR')) mime = 'image/webp';

  return `data:${mime};base64,${compact}`;
}

function getStringField(obj: Record<string, unknown> | null | undefined, keys: string[]): string {
  if (!obj || typeof obj !== 'object') return '';
  for (const key of keys) {
    const v = obj[key];
    if (typeof v === 'string' && v.trim()) return v.trim();
  }
  return '';
}

function extractQrTransactionId(response: DepositResponse): string {
  const tryParse = (text?: string): string => {
    const val = text?.trim();
    if (!val) return '';
    try {
      const parsed = JSON.parse(val);
      return (
        getStringField(parsed, ['transactionId', 'transactionID']) ||
        getStringField(parsed?.data, ['transactionId', 'transactionID'])
      );
    } catch {
      const m = val.match(/transactionid\s*[:=]\s*["']?([a-zA-Z0-9_-]+)/i);
      return m?.[1] || '';
    }
  };

  const direct = getStringField(response as unknown as Record<string, unknown>, ['transactionId', 'transactionID']);
  if (direct) return direct;

  const fromText = tryParse(response.textForQrCode);
  if (fromText) return fromText;

  if (typeof response.form === 'string') {
    const fromForm = tryParse(response.form as string);
    if (fromForm) return fromForm;
  } else if (response.form && typeof response.form === 'object') {
    const fromFormObj = getStringField(response.form as unknown as Record<string, unknown>, ['transactionId', 'transactionID']);
    if (fromFormObj) return fromFormObj;
  }

  return '';
}

export function DepositModal({ open, onOpenChange, account }: DepositModalProps) {
  const t = useTranslations('deposit');
  const getCurrencyName = useCurrencyName();
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();

  const [step, setStep] = useState<Step>(1);

  // Step 1
  const [groups, setGroups] = useState<DepositGroup[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<DepositGroup | null>(null);
  const [isLoadingGroups, setIsLoadingGroups] = useState(false);

  // Step 2
  const [groupInfo, setGroupInfo] = useState<DepositGroupInfo | null>(null);
  const [isLoadingInfo, setIsLoadingInfo] = useState(false);

  // Step 3
  const [paymentCurrency, setPaymentCurrency] = useState<string>('');
  const [amount, setAmount] = useState<string>('');
  const [dynamicFields, setDynamicFields] = useState<Record<string, string>>({});
  const [amountError, setAmountError] = useState<'' | 'required' | 'range' | 'integer'>('');

  // Step 4 & 5
  const [depositResponse, setDepositResponse] = useState<DepositResponse | null>(null);
  const [targetAmount, setTargetAmount] = useState<number>(0);

  // QrCode 支付确认
  const [isPaidSubmitting, setIsPaidSubmitting] = useState(false);
  const [isPaidConfirmed, setIsPaidConfirmed] = useState(false);

  // 信用卡表单 ref
  const creditCardFormRef = useRef<CreditCardFormHandle>(null);

  // 是否是信用卡渠道
  const isCreditCard = selectedGroup?.group === CREDIT_CARD_GROUP;

  // Step 1: 加载支付渠道
  useEffect(() => {
    if (open && account) {
      setIsLoadingGroups(true);
      execute(getDepositGroups, account.uid)
        .then((result) => {
          if (result.success && result.data) {
            setGroups(result.data);
          }
        })
        .finally(() => setIsLoadingGroups(false));
    }
  }, [open, account, execute]);

  const handleClose = useCallback(() => {
    onOpenChange(false);
    setTimeout(() => {
      setStep(1);
      setGroups([]);
      setSelectedGroup(null);
      setGroupInfo(null);
      setPaymentCurrency('');
      setAmount('');
      setDynamicFields({});
      setAmountError('');
      setDepositResponse(null);
      setTargetAmount(0);
      setIsPaidSubmitting(false);
      setIsPaidConfirmed(false);
    }, 200);
  }, [onOpenChange]);

  // Step 2: 加载渠道详情
  const loadGroupInfo = useCallback(async () => {
    if (!account || !selectedGroup) return;
    if (selectedGroup.isActive === false) return;
    setIsLoadingInfo(true);
    try {
      const result = await execute(getDepositGroupInfo, account.uid, selectedGroup.group);
      if (!result.success || !result.data) return;

      let info: DepositGroupInfo = result.data;

      // ExLink Global 渠道：用 ExLink 实时汇率覆盖 currencyRates
      const groupLower = (selectedGroup.group || '').toLowerCase();
      const nameLower = (selectedGroup.paymentMethodName || '').toLowerCase();
      const exLink =
        groupLower.includes(EXLINK_GLOBAL_KEYWORD) || nameLower.includes(EXLINK_GLOBAL_KEYWORD);

      if (exLink) {
        try {
          const [currenciesRes, ratesRes] = await Promise.all([
            execute(getExLinkCurrencies),
            execute(getExLinkExchangeRates),
          ]);
          const rateList = ratesRes.success ? ratesRes.data?.marketPriceList ?? [] : [];
          if (currenciesRes.success && rateList.length > 0) {
            const rateMap = new Map<number, number>(
              rateList.map((r) => [r.sourceCoinId, r.marketInPrice])
            );
            const filtered = (info.currencyRates || [])
              .filter((cr) => rateMap.has(cr.currencyId))
              .map((cr) => ({ ...cr, rate: rateMap.get(cr.currencyId) ?? cr.rate }));
            info = { ...info, currencyRates: filtered };
          }
        } catch (err) {
          console.error('Failed to fetch ExLink currency rates:', err);
        }
      }

      setGroupInfo(info);
      if (info.requestValues) {
        setDynamicFields(
          Object.fromEntries(
            Object.entries(info.requestValues).map(([k, v]) => [
              k,
              v === null || v === undefined ? '' : String(v),
            ])
          )
        );
      }
      if (info.currencyRates?.length === 1) {
        setPaymentCurrency(String(info.currencyRates[0].currencyId));
      } else if (!info.currencyRates?.length && account.currencyId) {
        setPaymentCurrency(String(account.currencyId));
      }
      setStep(2);
    } finally {
      setIsLoadingInfo(false);
    }
  }, [account, selectedGroup, execute]);

  // 当前汇率
  const currentRate = useMemo((): CurrencyRate | null => {
    if (!groupInfo?.currencyRates?.length || !paymentCurrency) return null;
    return groupInfo.currencyRates.find(
      (r) => String(r.currencyId) === paymentCurrency
    ) || null;
  }, [groupInfo, paymentCurrency]);

  // 汇率换算
  useEffect(() => {
    const numAmount = Number(amount);
    if (numAmount > 0 && currentRate && currentRate.rate > 0) {
      setTargetAmount(Math.ceil(numAmount * currentRate.rate));
    } else {
      setTargetAmount(0);
    }
  }, [amount, currentRate]);

  // 金额校验：
  // - 必须为正整数
  // - range 为 USD 固定值（如 5000 -> 500 USD），先转成 USD 口径
  // - 输入值按账户币种换算到 USD（1 USD = 100 USC）后再比较
  const validateAmount = useCallback((val: string): boolean => {
    const num = Number(val);
    if (!num || num <= 0) {
      setAmountError('required');
      return false;
    }
    if (!Number.isInteger(num)) {
      setAmountError('integer');
      return false;
    }

    if (groupInfo?.range && account) {
      const [rawMin, rawMax] = groupInfo.range;
      const minInUsd = rawMin / 100;
      const maxInUsd = rawMax / 100;
      const inputInUsd = account.currencyId === CurrencyTypes.USC ? num / 100 : num;
      if (minInUsd > 0 && inputInUsd < minInUsd) {
        setAmountError('range');
        return false;
      }
      if (maxInUsd > 0 && inputInUsd > maxInUsd) {
        setAmountError('range');
        return false;
      }
    }
    setAmountError('');
    return true;
  }, [groupInfo, account]);

  // 可见的动态字段
  const visibleRequestKeys = useMemo(() => {
    return (groupInfo?.requestKeys || []).filter(
      (key) => !HIDDEN_REQUEST_KEYS.includes(key)
    );
  }, [groupInfo]);

  // Step 3 是否可前进（信用卡的字段校验在 CreditCardForm.validate 内部完成）
  const canProceedStep3 = useMemo(() => {
    if (!amount || Number(amount) <= 0) return false;
    if (amountError) return false;
    if (!isCreditCard) {
      for (const key of visibleRequestKeys) {
        if (!dynamicFields[key]?.trim()) return false;
      }
    }
    return true;
  }, [amount, amountError, isCreditCard, visibleRequestKeys, dynamicFields]);

  const qrCodeImageSrc = useMemo(
    () => (depositResponse?.textForQrCode ? getBase64ImageDataUrl(depositResponse.textForQrCode) : ''),
    [depositResponse?.textForQrCode]
  );

  const qrTransactionId = useMemo(
    () => (depositResponse ? extractQrTransactionId(depositResponse) : ''),
    [depositResponse]
  );

  const notifyPaid = useCallback(async () => {
    if (isPaidConfirmed || !qrTransactionId) return;
    setIsPaidSubmitting(true);
    try {
      const result = await execute(postQrCodePaid, qrTransactionId);
      if (result.success) {
        setIsPaidConfirmed(true);
        showSuccess(t('guide.paidSuccess'));
      }
    } finally {
      setIsPaidSubmitting(false);
    }
  }, [isPaidConfirmed, qrTransactionId, execute, showSuccess, t]);

  // Step 4: 提交入金
  const handleDeposit = useCallback(async () => {
    if (!account || !groupInfo || !selectedGroup) return;

    const numAmount = Number(amount);
    const requestData: Record<string, string | number> = {
      ...dynamicFields,
      amount: numAmount,
      currencyId: Number(paymentCurrency) || account.currencyId,
      returnUrl: typeof window !== 'undefined' ? window.location.href : '',
    };

    const payload = {
      hashId: groupInfo.hashId,
      amount: numAmount * 100,
      request: requestData,
    };

    const result = await execute(postAccountDeposit, account.uid, payload);
    if (result.success && result.data) {
      setDepositResponse(result.data);
      const { action, redirectUrl, endPoint, form } = result.data;

      if (action === DepositActions.Redirect && redirectUrl) {
        window.open(redirectUrl, '_blank');
        setStep(5);
      } else if (action === DepositActions.Post && endPoint && form) {
        const formEl = document.createElement('form');
        formEl.method = 'POST';
        formEl.action = endPoint;
        formEl.target = '_blank';
        Object.entries(form).forEach(([key, value]) => {
          const input = document.createElement('input');
          input.type = 'hidden';
          input.name = key;
          input.value = value;
          formEl.appendChild(input);
        });
        document.body.appendChild(formEl);
        formEl.submit();
        document.body.removeChild(formEl);
        setStep(5);
      } else {
        setStep(5);
      }
    }
  }, [account, groupInfo, selectedGroup, amount, dynamicFields, paymentCurrency, execute]);

  // Stepper 配置
  const stepperSteps = useMemo(() => [
    { id: 'channel', label: t('step.channel'), number: 1 },
    { id: 'notice', label: t('step.notice'), number: 2 },
    { id: 'fill', label: t('step.fill'), number: 3 },
    { id: 'verify', label: t('step.verify'), number: 4 },
    { id: 'guide', label: t('step.guide'), number: 5 },
  ], [t]);

  const stepIdMap: Record<Step, string> = { 1: 'channel', 2: 'notice', 3: 'fill', 4: 'verify', 5: 'guide' };
  const stepperCurrentStep = stepIdMap[step];
  const stepperCompletedSteps = useMemo(() => {
    const completed: string[] = [];
    const ids = ['channel', 'notice', 'fill', 'verify', 'guide'];
    for (let i = 0; i < step - 1; i++) {
      completed.push(ids[i]);
    }
    return completed;
  }, [step]);
  const selectedGroupIsActive = selectedGroup?.isActive !== false;

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose(); else onOpenChange(v); }}>
      <DialogContent
        className="h-[800px]! flex flex-col justify-between"
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        <div className="flex flex-1 flex-col gap-6 overflow-hidden">
          <DialogHeader>
            <DialogTitle>{t('title')}</DialogTitle>
          </DialogHeader>

          <Stepper
            steps={stepperSteps}
            currentStep={stepperCurrentStep}
            completedSteps={stepperCompletedSteps}
          />

          <div className="flex-1 overflow-y-auto">
            {/* Step 1: 选择支付渠道 */}
            {step === 1 && (
              <div className="flex flex-col gap-5">
                {isLoadingGroups ? (
                  <div className="flex items-center justify-center py-20 text-text-secondary">
                    {t('loading')}
                  </div>
                ) : (
                  <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                    {groups.map((group) => {
                      const isSelected = selectedGroup?.group === group.group;
                      const isGroupActive = group.isActive !== false;
                      const groupRange = group.range?.map(r => r * 100);
                      return (
                        <button
                          key={group.group}
                          type="button"
                          disabled={!isGroupActive}
                          onClick={() => setSelectedGroup(group)}
                          className={`relative flex items-start gap-4 overflow-hidden rounded-lg border p-4 text-left transition-colors ${
                            !isGroupActive
                              ? 'cursor-not-allowed border-border bg-surface opacity-50'
                              : isSelected
                                ? 'cursor-pointer border-primary bg-surface'
                                : 'cursor-pointer border-border bg-surface hover:border-primary/50'
                          }`}
                        >
                          {group.logo && (
                            <Image
                              src={group.logo}
                              alt={group.paymentMethodName}
                              width={48}
                              height={48}
                              className="shrink-0 rounded"
                            />
                          )}
                          <div className="flex flex-1 flex-col gap-1">
                            <span className="text-base font-medium text-text-primary">
                              {group.paymentMethodName}
                            </span>
                            <span className="text-xs text-text-secondary">
                              {t('channel.arrival')}：{t('channel.instant')}
                            </span>
                            <span className="text-xs text-text-secondary">
                              {t('channel.fee')}：{t('channel.noFee')}
                            </span>
                            <span className="text-xs text-text-secondary">
                              {t('channel.processing')}：{'< 1'}{t('channel.hour')}
                            </span>
                            {groupRange && account && (
                              <>
                                <span className="text-xs text-text-secondary">
                                  Min: <BalanceShow balance={groupRange[0]} currencyId={CurrencyTypes.USD} />
                                </span>
                                <span className="text-xs text-text-secondary">
                                  Max: <BalanceShow balance={groupRange[1]} currencyId={CurrencyTypes.USD} />
                                </span>
                              </>
                            )}
                          </div>
                          {isSelected && isGroupActive && (
                            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" className="absolute -bottom-px -right-px">
                              <path d="M0 24L24 24L24 0L0 24Z" fill="var(--color-primary)" />
                              <path d="M17 15L14.5 17.5L12.5 15.5" stroke="white" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                            </svg>
                          )}
                        </button>
                      );
                    })}
                  </div>
                )}
              </div>
            )}

            {/* Step 2: 重要提示 */}
            {step === 2 && groupInfo && (
              <div className="flex flex-col gap-5">
                <h3 className="text-lg font-semibold text-text-primary">
                  {t('notice.title', { name: selectedGroup?.paymentMethodName || '' })}
                </h3>
                <div
                  className="prose prose-sm max-w-none text-text-secondary dark:prose-invert"
                  dangerouslySetInnerHTML={{ __html: groupInfo.policy || '' }}
                />
              </div>
            )}

            {/* Step 3: 填写信息 */}
            {step === 3 && groupInfo && account && (
              <div className="flex flex-col gap-5">
                <h3 className="text-base font-semibold text-text-primary">
                  {t('fill.depositTo')}
                </h3>
                {/* 支付币种 */}
                {groupInfo.currencyRates && groupInfo.currencyRates.length > 0 && (
                  <div className="flex flex-col gap-2">
                    <label className="flex items-center text-sm font-medium text-text-secondary">
                      <span className="mr-1 text-primary">*</span>
                      {selectedGroup?.paymentMethodName}{t('fill.currency')}
                    </label>
                    <Select
                      value={paymentCurrency}
                      onValueChange={setPaymentCurrency}
                      disabled={groupInfo.currencyRates.length === 1}
                    >
                      <SelectTrigger className="h-12 w-full bg-input-bg">
                        <SelectValue placeholder={t('fill.selectCurrency')} />
                      </SelectTrigger>
                      <SelectContent>
                        {groupInfo.currencyRates.map((cr) => (
                          <SelectItem key={cr.currencyId} value={String(cr.currencyId)}>
                            {getCurrencyName(cr.currencyId)}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                )}
                {/* 金额输入 + 存款金额 */}
                <div className="flex flex-row gap-4">
                  <div className="flex-2">
                    <Input
                      type="number"
                      label={`${t('fill.amount')}（${getCurrencyName(account.currencyId)}）`}
                      required
                      inputSize="md"
                      value={amount}
                      onChange={(e) => {
                        setAmount(e.target.value);
                        if (amountError) validateAmount(e.target.value);
                      }}
                      onBlur={() => amount && validateAmount(amount)}
                      placeholder="0"
                      disabled={!paymentCurrency}
                      error={
                        amountError === 'required'
                          ? t('error.amountRequired')
                          : amountError === 'integer'
                            ? t('error.amountInteger')
                            : undefined
                      }
                      errorPosition="bottom"
                    />
                    {amountError === 'range' && groupInfo?.range && (
                      <p className="error-text mt-1 text-sm">
                        {t('error.amountRange')}
                        <BalanceShow balance={account.currencyId === CurrencyTypes.USC ? groupInfo.range[0] * 100 : groupInfo.range[0]} currencyId={account.currencyId} />
                        ~
                        <BalanceShow balance={account.currencyId === CurrencyTypes.USC ? groupInfo.range[1] * 100 : groupInfo.range[1]} currencyId={account.currencyId} />
                      </p>
                    )}
                  </div>

                  {currentRate && currentRate.rate !== 1 && (
                    <div className="flex-1">
                      <Input
                        label={t('fill.depositAmount')}
                        inputSize="md"
                        value={targetAmount || ''}
                        readOnly
                      />
                      <span className="mt-1 block text-right text-xs text-text-secondary">
                        {t('fill.exchangeRate')} = 1:{currentRate.rate}
                      </span>
                    </div>
                  )}
                </div>

                {/* 信用卡专用表单 */}
                {isCreditCard && (
                  <CreditCardForm
                    ref={creditCardFormRef}
                    value={dynamicFields}
                    onChange={(key, val) =>
                      setDynamicFields((prev) => ({ ...prev, [key]: val }))
                    }
                    disabled={isLoading}
                  />
                )}

                {/* 通用动态表单字段（非信用卡） */}
                {!isCreditCard && visibleRequestKeys.length > 0 && (
                  <div className="flex flex-row gap-4">
                    {visibleRequestKeys.map((key) => (
                      <div key={key} className="flex-1">
                        <Input
                          label={t(`requestKeys.${key}`, { defaultMessage: key })}
                          required
                          inputSize="md"
                          value={dynamicFields[key] || ''}
                          onChange={(e) =>
                            setDynamicFields((prev) => ({ ...prev, [key]: e.target.value }))
                          }
                        />
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* Step 4: 验证详情 */}
            {step === 4 && account && (
              <div className="flex flex-col gap-5">
                <h3 className="text-base font-semibold text-text-primary">
                  {t('verify.title')}
                </h3>
                <div className="flex flex-col gap-4 rounded-lg border border-border p-5">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-text-secondary">{t('verify.operation')}</span>
                    <span className="text-sm text-text-primary">
                      {t('verify.depositTo', { currency: getCurrencyName(account.currencyId) })}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-text-secondary">{t('verify.channel')}</span>
                    <span className="text-sm text-text-primary">
                      {selectedGroup?.paymentMethodName}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-text-secondary">{t('verify.amount')}</span>
                    <span className="text-sm text-text-primary">
                      <BalanceShow balance={Number(amount) * 100} currencyId={account.currencyId} />
                    </span>
                  </div>
                </div>
              </div>
            )}

            {/* Step 5: 入金指南 / 结果 */}
            {step === 5 && depositResponse && account && (
              <div className="flex flex-col gap-5">
                {depositResponse.isSuccess ? (
                  <>
                    <div className="flex flex-col gap-2">
                      <h3 className="text-base font-semibold text-text-primary">
                        {t('guide.orderCreated')}
                      </h3>
                      <p className="text-sm text-text-secondary">
                        {t('guide.orderDesc')}
                      </p>
                    </div>

                    <div className="flex items-center gap-2 rounded-lg bg-surface-secondary p-4">
                      <svg width="20" height="20" viewBox="0 0 20 20" fill="none" className="shrink-0 text-primary">
                        <circle cx="10" cy="10" r="9" stroke="currentColor" strokeWidth="2" />
                        <text x="10" y="14" textAnchor="middle" fontSize="11" fontWeight="600" fill="currentColor">i</text>
                      </svg>
                      <span className="text-sm text-text-primary">
                        {t('guide.requestAmount')}{' '}
                        <BalanceShow balance={Number(amount) * 100} currencyId={account.currencyId} className="font-semibold" />
                      </span>
                    </div>

                    {/* QrCode */}
                    {depositResponse.action === DepositActions.QrCode && depositResponse.textForQrCode && (
                      <div className="flex flex-col items-center gap-3">
                        {!qrTransactionId && (
                          <p className="text-sm text-text-secondary">{t('guide.qrCodeNotice')}</p>
                        )}

                        {qrCodeImageSrc ? (
                          <Image
                            src={qrCodeImageSrc}
                            alt="QR code"
                            width={132}
                            height={132}
                            unoptimized
                            className="rounded bg-white object-contain p-1.5"
                          />
                        ) : (
                          <>
                            <p className="text-sm text-text-secondary">{t('guide.walletAddress')}</p>
                            <code className="rounded bg-input-bg px-3 py-2 text-xs text-text-primary break-all">
                              {depositResponse.textForQrCode}
                            </code>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => {
                                navigator.clipboard.writeText(depositResponse.textForQrCode || '');
                                showSuccess(t('guide.copied'));
                              }}
                            >
                              {t('guide.copyAddress')}
                            </Button>
                          </>
                        )}

                        {depositResponse.message && (
                          <p className="text-xs">
                            {t('guide.countdown', { minutes: depositResponse.message })}
                          </p>
                        )}

                        {qrTransactionId && (
                          <Button
                            variant="primary"
                            disabled={isPaidSubmitting || isPaidConfirmed}
                            loading={isPaidSubmitting}
                            onClick={notifyPaid}
                          >
                            {isPaidConfirmed ? t('guide.paidConfirmed') : t('guide.completePayment')}
                          </Button>
                        )}
                      </div>
                    )}

                    {/* Instruction */}
                    {depositResponse.instruction && (
                      <div
                        className="prose prose-sm max-w-none text-text-secondary dark:prose-invert"
                        dangerouslySetInnerHTML={{ __html: depositResponse.instruction }}
                      />
                    )}

                    {/* Redirect / Post 跳转链接 */}
                    {(depositResponse.action === DepositActions.Redirect ||
                      depositResponse.action === DepositActions.Post) &&
                      depositResponse.redirectUrl && (
                        <p className="text-sm text-text-secondary">
                          {t('guide.clickRedirect')}：{' '}
                          <a
                            href={depositResponse.redirectUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-primary underline"
                          >
                            {t('guide.redirect')}
                          </a>
                        </p>
                      )}
                  </>
                ) : (
                  <div className="flex flex-col gap-4">
                    <div className="flex items-start gap-4 rounded-lg border border-border p-4">
                      {selectedGroup?.logo && (
                        <Image
                          src={selectedGroup.logo}
                          alt={selectedGroup.paymentMethodName}
                          width={48}
                          height={48}
                          className="shrink-0 rounded"
                        />
                      )}
                      <div className="flex flex-col gap-1">
                        <span className="text-base font-medium text-text-primary">
                          {selectedGroup?.paymentMethodName}
                        </span>
                      </div>
                    </div>
                    <p className="text-sm error-text">
                      {depositResponse.error || t('guide.error')}
                    </p>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>

        {/* 底部按钮 */}
        <DialogFooter className="flex-row! items-center justify-between! pt-5">
          {step === 1 ? (
            <>
              <div />
              <div className="flex gap-2 md:gap-5">
                <Button variant="outline" onClick={handleClose} className="w-auto min-w-20 md:w-[120px]">
                  {t('action.close')}
                </Button>
                <Button
                  variant="primary"
                  onClick={loadGroupInfo}
                  disabled={!selectedGroup || !selectedGroupIsActive || isLoadingInfo}
                  loading={isLoadingInfo}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.next')}
                </Button>
              </div>
            </>
          ) : step === 5 ? (
            <>
              <Button variant="outline" onClick={() => setStep(4)} disabled={isLoading} className="w-auto min-w-16 md:w-[100px]">
                {t('action.prev')}
              </Button>
              <div className="flex gap-2 md:gap-5">
                <Button variant="outline" onClick={handleClose} className="w-auto min-w-20 md:w-[120px]">
                  {t('action.close')}
                </Button>
                <Button variant="primary" onClick={handleClose} className="w-auto min-w-20 md:w-[120px]">
                  {t('action.done')}
                </Button>
              </div>
            </>
          ) : (
            <>
              <Button
                variant="outline"
                onClick={() => setStep((s) => (s - 1) as Step)}
                disabled={isLoading}
                className="w-auto min-w-16 md:w-[100px]"
              >
                {t('action.prev')}
              </Button>
              <div className="flex gap-2 md:gap-5">
                <Button variant="outline" onClick={handleClose} disabled={isLoading} className="w-auto min-w-20 md:w-[120px]">
                  {t('action.close')}
                </Button>
                <Button
                  variant="primary"
                  onClick={() => {
                    if (step === 2) {
                      setStep(3);
                    } else if (step === 3) {
                      if (!validateAmount(amount) || !canProceedStep3) return;
                      // 信用卡渠道额外校验信用卡表单
                      if (isCreditCard) {
                        const validated = creditCardFormRef.current?.validate();
                        if (!validated) return;
                        setDynamicFields((prev) => ({ ...prev, ...validated }));
                      }
                      setStep(4);
                    } else if (step === 4) {
                      handleDeposit();
                    }
                  }}
                  disabled={
                    (step === 3 && !canProceedStep3) ||
                    (step === 4 && isLoading)
                  }
                  loading={step === 4 && isLoading}
                  className="w-auto min-w-20 md:w-[120px]"
                >
                  {t('action.next')}
                </Button>
              </div>
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
