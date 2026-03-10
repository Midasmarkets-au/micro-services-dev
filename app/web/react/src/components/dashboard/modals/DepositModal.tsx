'use client';

import { useEffect, useState, useMemo, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui';
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
import { getDepositGroups, getDepositGroupInfo, postAccountDeposit } from '@/actions/deposit';
import type {
  DepositGroup,
  DepositGroupInfo,
  DepositResponse,
  CurrencyRate,
} from '@/types/deposit';
import { DepositActions } from '@/types/deposit';
import { CurrencyTypes, getCurrencySymbol } from '@/types/accounts';

interface DepositModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: { uid: number; currencyId: number } | null;
}

type Step = 1 | 2 | 3 | 4 | 5;

const HIDDEN_REQUEST_KEYS = ['returnUrl', 'currencyId'];

const CurrencyNames: Record<number, string> = {
  [CurrencyTypes.AUD]: 'AUD',
  [CurrencyTypes.CNY]: 'CNY/RMB',
  [CurrencyTypes.USD]: 'USD',
  [CurrencyTypes.USC]: 'USC',
};

function getCurrencyName(currencyId: number): string {
  return CurrencyNames[currencyId] || 'USD';
}

export function DepositModal({ open, onOpenChange, account }: DepositModalProps) {
  const t = useTranslations('deposit');
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
  const [amountError, setAmountError] = useState<string>('');

  // Step 4 & 5
  const [depositResponse, setDepositResponse] = useState<DepositResponse | null>(null);
  const [targetAmount, setTargetAmount] = useState<number>(0);

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
    }, 200);
  }, [onOpenChange]);

  // Step 2: 加载渠道详情
  const loadGroupInfo = useCallback(async () => {
    if (!account || !selectedGroup) return;
    setIsLoadingInfo(true);
    try {
      const result = await execute(getDepositGroupInfo, account.uid, selectedGroup.group);
      if (result.success && result.data) {
        setGroupInfo(result.data);
        if (selectedGroup.group === 'Credit Card' && result.data.requestValues) {
          setDynamicFields(
            Object.fromEntries(
              Object.entries(result.data.requestValues).map(([k, v]) => [k, String(v)])
            )
          );
        }
        if (result.data.currencyRates?.length === 1) {
          setPaymentCurrency(String(result.data.currencyRates[0].currencyId));
        } else if (!result.data.currencyRates?.length && account.currencyId) {
          setPaymentCurrency(String(account.currencyId));
        }
        setStep(2);
      }
    } finally {
      setIsLoadingInfo(false);
    }
  }, [account, selectedGroup, execute]);

  // 金额范围
  const amountRange = useMemo(() => {
    if (!groupInfo?.range || !account) return { min: 0, max: 0 };
    const [rawMin, rawMax] = groupInfo.range;
    if (account.currencyId === CurrencyTypes.USC) {
      return { min: rawMin, max: rawMax };
    }
    return { min: rawMin / 100, max: rawMax / 100 };
  }, [groupInfo, account]);

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

  // 金额校验
  const validateAmount = useCallback((val: string): boolean => {
    const num = Number(val);
    if (!num || num <= 0) {
      setAmountError(t('error.amountRequired'));
      return false;
    }
    if (amountRange.min > 0 && num < amountRange.min) {
      setAmountError(t('error.amountRange', { min: amountRange.min, max: amountRange.max }));
      return false;
    }
    if (amountRange.max > 0 && num > amountRange.max) {
      setAmountError(t('error.amountRange', { min: amountRange.min, max: amountRange.max }));
      return false;
    }
    setAmountError('');
    return true;
  }, [amountRange, t]);

  // 可见的动态字段
  const visibleRequestKeys = useMemo(() => {
    return (groupInfo?.requestKeys || []).filter(
      (key) => !HIDDEN_REQUEST_KEYS.includes(key)
    );
  }, [groupInfo]);

  // Step 3 是否可前进
  const canProceedStep3 = useMemo(() => {
    if (!amount || Number(amount) <= 0) return false;
    if (amountError) return false;
    for (const key of visibleRequestKeys) {
      if (!dynamicFields[key]?.trim()) return false;
    }
    return true;
  }, [amount, amountError, visibleRequestKeys, dynamicFields]);

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
                      return (
                        <button
                          key={group.group}
                          type="button"
                          onClick={() => setSelectedGroup(group)}
                          className={`relative flex cursor-pointer items-start gap-4 overflow-hidden rounded-lg border p-4 text-left transition-colors ${
                            isSelected
                              ? 'border-primary bg-surface'
                              : 'border-border bg-surface hover:border-primary/50'
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
                          </div>
                          {isSelected && (
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
                {groupInfo.currencyRates && groupInfo.currencyRates.length > 1 && (
                  <div className="flex flex-col gap-2">
                    <label className="flex items-center text-sm font-medium text-text-secondary">
                      <span className="mr-1 text-primary">*</span>
                      {selectedGroup?.paymentMethodName}{t('fill.currency')}
                    </label>
                    <Select value={paymentCurrency} onValueChange={setPaymentCurrency}>
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

                {/* 金额输入 */}
                <div className="flex flex-col gap-2">
                  <label className="flex items-center text-sm font-medium text-text-secondary">
                    <span className="mr-1 text-primary">*</span>
                    {t('fill.amount')}（{getCurrencyName(account.currencyId)}）
                  </label>
                  <div className="relative">
                    <input
                      type="number"
                      value={amount}
                      onChange={(e) => {
                        setAmount(e.target.value);
                        if (amountError) validateAmount(e.target.value);
                      }}
                      onBlur={() => amount && validateAmount(amount)}
                      min={amountRange.min}
                      max={amountRange.max}
                      className="h-12 w-full rounded bg-input-bg px-3 text-sm font-medium text-text-primary outline-none"
                      placeholder="0"
                    />
                    {amountRange.max > 0 && (
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-text-secondary">
                        {t('fill.amountHint')}：{getCurrencySymbol(account.currencyId)}{amountRange.min}~{getCurrencySymbol(account.currencyId)}{amountRange.max}
                      </span>
                    )}
                  </div>
                  {amountError && (
                    <span className="text-xs text-error">{amountError}</span>
                  )}
                </div>

                {/* 存款金额（汇率换算） */}
                {currentRate && currentRate.rate !== 1 && (
                  <div className="flex flex-col gap-2">
                    <label className="flex items-center text-sm font-medium text-text-secondary">
                      <span className="mr-1 text-primary">*</span>
                      {t('fill.depositAmount')}
                    </label>
                    <div className="relative">
                      <input
                        type="text"
                        value={targetAmount || ''}
                        readOnly
                        className="h-12 w-full rounded bg-input-bg px-3 text-sm font-medium text-text-primary outline-none"
                      />
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-text-secondary">
                        {t('fill.exchangeRate')}：1:{currentRate.rate}
                      </span>
                    </div>
                  </div>
                )}

                {/* 动态表单字段 */}
                {visibleRequestKeys.map((key) => (
                  <div key={key} className="flex flex-col gap-2">
                    <label className="flex items-center text-sm font-medium text-text-secondary">
                      <span className="mr-1 text-primary">*</span>
                      {t(`requestKeys.${key}`, { defaultMessage: key })}
                    </label>
                    <input
                      type="text"
                      value={dynamicFields[key] || ''}
                      onChange={(e) =>
                        setDynamicFields((prev) => ({ ...prev, [key]: e.target.value }))
                      }
                      className="h-12 w-full rounded bg-input-bg px-3 text-sm font-medium text-text-primary outline-none"
                    />
                  </div>
                ))}
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
                      {getCurrencySymbol(account.currencyId)} {Number(amount).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </span>
                  </div>
                  {targetAmount > 0 && currentRate && currentRate.rate !== 1 && (
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-text-secondary">{t('verify.depositAmount')}</span>
                      <span className="text-sm text-text-primary">
                        {getCurrencyName(Number(paymentCurrency))} {targetAmount.toLocaleString()}
                      </span>
                    </div>
                  )}
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

                    {/* 支付渠道卡片 */}
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
                      <div className="flex flex-1 flex-col gap-1">
                        <span className="text-base font-medium text-text-primary">
                          {selectedGroup?.paymentMethodName}
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
                      </div>
                    </div>

                    {/* QrCode */}
                    {depositResponse.action === DepositActions.QrCode && depositResponse.textForQrCode && (
                      <div className="flex flex-col items-center gap-3">
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
                        {depositResponse.message && (
                          <p className="text-xs text-error">
                            {t('guide.countdown', { minutes: depositResponse.message })}
                          </p>
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
                    <p className="text-sm text-error">
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
                <Button variant="secondary" onClick={handleClose} className="w-auto min-w-20 md:w-[120px]">
                  {t('action.close')}
                </Button>
                <Button
                  variant="primary"
                  onClick={loadGroupInfo}
                  disabled={!selectedGroup || isLoadingInfo}
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
                <Button variant="secondary" onClick={handleClose} className="w-auto min-w-20 md:w-[120px]">
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
                <Button variant="secondary" onClick={handleClose} disabled={isLoading} className="w-auto min-w-20 md:w-[120px]">
                  {t('action.close')}
                </Button>
                <Button
                  variant="primary"
                  onClick={() => {
                    if (step === 2) {
                      setStep(3);
                    } else if (step === 3) {
                      if (validateAmount(amount) && canProceedStep3) {
                        setStep(4);
                      }
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
