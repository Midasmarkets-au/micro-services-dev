'use client';

import { useEffect, useState, useMemo, useCallback, useRef } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';
import { QRCodeSVG } from 'qrcode.react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button, Input } from '@/components/ui';
import { BalanceShow } from '@/components/ui/BalanceShow';
import { Stepper } from '@/components/ui/Stepper';
import { SimpleSelect, type SelectOption } from '@/components/ui/radix/Select';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { fetchAction } from '@/lib/api/browser-client';
import {
  postAccountDeposit,
  postQrCodePaid,
  getExLinkCurrencies,
  getExLinkExchangeRates,
} from '@/actions/deposit';
import { getPaymentInfoList, type PaymentInfo } from '@/actions/payment';
import type {
  DepositGroup,
  DepositGroupInfo,
  DepositResponse,
  CurrencyRate,
  PaymentMethodConfig,
} from '@/types/deposit';
import { DepositActions } from '@/types/deposit';
import { CurrencyTypes } from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';
import { CreditCardForm, type CreditCardFormHandle } from './CreditCardForm';

const CREDIT_CARD_GROUP = 'Credit Card';
const EXLINK_GLOBAL_TYPE = 'ExLinkGlobal';
/** Wire platform — KYC-bound bank accounts for ExLinkGlobal JPY H2H */
const WIRE_PAYMENT_PLATFORM = 100 as const;
const HELP2PAY_TYPE = 'Help2Pay';
const PAY247_TYPE = 'Pay247';
const RDDPAY_TYPE ='RDDPay';
const NPay_TYPE ='NPay';
const AliPay2_TYPE ='AliPay2';
const CHINESE_NATIVE_NAME_REGEX = /^[\u3400-\u9FFF\uF900-\uFAFF\s·]+$/;
/**
 * Groups that fan out into multiple PaymentMethod rows on the backend
 * and render a step-3 dropdown driven by `groupInfo.paymentMethods`.
 * - ExLinkGlobal: one row per primary currency.
 * - Help2Pay:     one row per (channel x currency) — two rows may share a currency.
 * - Pay247:       one row per (currency x pay_method) — bank selection deferred to the hosted page.
 * - AliPay2:      one row per payment method (WeChat / Alipay), QR allocated by payment-tunnel.
 */
const MULTI_METHOD_TYPES = [EXLINK_GLOBAL_TYPE, HELP2PAY_TYPE, PAY247_TYPE,RDDPAY_TYPE, NPay_TYPE, AliPay2_TYPE] as const;

interface DepositModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: { uid: number; currencyId: number } | null;
}

type Step = 1 | 2 | 3 | 4 | 5;

const HIDDEN_REQUEST_KEYS = ['returnUrl', 'currencyId', 'paymentInfoId'];

function formatWirePaymentInfoLabel(info: PaymentInfo): string {
  if ('walletAddress' in info.info) return info.name;
  const bank = info.info;
  let label = info.name;
  if (bank.bankName) label += ` — ${bank.bankName}`;
  if (bank.accountNo) label += ` (${bank.accountNo})`;
  return label;
}

function formatAmount(amount: number): string {
  if (amount == null) return '';
  return new Intl.NumberFormat('en-US').format(amount);
}

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

  const fromInfo = getStringField(response.info as unknown as Record<string, unknown>, ['transactionId', 'transactionID']);
  if (fromInfo) return fromInfo;

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
  const tErrors = useTranslations('errorCodes');
  const getCurrencyName = useCurrencyName();
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess, showWarning } = useToast();

  const [step, setStep] = useState<Step>(1);

  // Step 1
  const [groups, setGroups] = useState<DepositGroup[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<DepositGroup | null>(null);
  const [isLoadingGroups, setIsLoadingGroups] = useState(false);

  // Step 2
  const [groupInfo, setGroupInfo] = useState<DepositGroupInfo | null>(null);
  const [isLoadingInfo, setIsLoadingInfo] = useState(false);

  // Step 3
  // 对多 method 渠道（ExLinkGlobal / Help2Pay）该字段存的是 paymentMethod 的 hashId；
  // 对其余渠道沿用旧语义，存的是 currencyId 的字符串。
  const [selectedMethodKey, setSelectedMethodKey] = useState<string>('');
  const [amount, setAmount] = useState<string>('');
  const [dynamicFields, setDynamicFields] = useState<Record<string, string>>({});
  const [amountError, setAmountError] = useState<'' | 'required' | 'range' | 'integer'>('');
  const [wirePaymentInfos, setWirePaymentInfos] = useState<PaymentInfo[]>([]);
  const [jpyPaymentInfoError, setJpyPaymentInfoError] = useState(false);

  // Step 4 & 5
  const [depositResponse, setDepositResponse] = useState<DepositResponse | null>(null);
  const [targetAmount, setTargetAmount] = useState<number>(0);

  // Step 5 显示控制
  const [showInstruction, setShowInstruction] = useState(false);

  // QrCode 支付确认
  const [isPaidSubmitting, setIsPaidSubmitting] = useState(false);
  const [isPaidConfirmed, setIsPaidConfirmed] = useState(false);

  // QrCode 倒计时
  const [countDown, setCountDown] = useState(0);
  const [isExpired, setIsExpired] = useState(false);
  const countdownTimerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // 信用卡表单 ref
  const creditCardFormRef = useRef<CreditCardFormHandle>(null);

  // 是否是信用卡渠道
  const isCreditCard = selectedGroup?.group === CREDIT_CARD_GROUP;

  // 是否是 ExLinkGlobal 渠道（按 type 字段判定）
  // 仅用于 ExLink 专有的实时汇率覆盖逻辑
  const isExLinkGlobal = selectedGroup?.type === EXLINK_GLOBAL_TYPE;

  // 是否是“多 PaymentMethod 行 + 单一入口”渠道（ExLinkGlobal / Help2Pay）。
  // 这些渠道在 Step 1 只显示一个卡片，Step 3 通过 paymentMethods 下拉框选择具体 method。
  const isMultiMethodGroup =
    selectedGroup?.type !== undefined &&
    (MULTI_METHOD_TYPES as readonly string[]).includes(selectedGroup.type);

  const wireBankSelectOptions = useMemo<SelectOption[]>(
    () =>
      wirePaymentInfos.map((info) => ({
        value: String(info.id),
        label: formatWirePaymentInfoLabel(info),
      })),
    [wirePaymentInfos]
  );

  const depositErrorMessage = useMemo(() => {
    const err = depositResponse?.error;
    if (!err) return t('guide.error');
    const lookup =
      err === 'No available payment codes' ? '__NO_AVAILABLE_PAYMENT_CODES__' : err;
    if (lookup.startsWith('__')) {
      const translated = tErrors(lookup);
      if (translated && !translated.startsWith('errorCodes.')) return translated;
    }
    return err;
  }, [depositResponse?.error, t, tErrors]);

  // Step 1: 加载支付渠道
  useEffect(() => {
    if (open && account) {
      setIsLoadingGroups(true);
      execute(async () =>
        fetchAction<DepositGroup[]>('getDepositGroups', account.uid)
      )
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
      setSelectedMethodKey('');
      setAmount('');
      setDynamicFields({});
      setAmountError('');
      setDepositResponse(null);
      setTargetAmount(0);
      setShowInstruction(false);
      setIsPaidSubmitting(false);
      setIsPaidConfirmed(false);
      setCountDown(0);
      setIsExpired(false);
      setWirePaymentInfos([]);
      setJpyPaymentInfoError(false);
    }, 200);
  }, [onOpenChange]);

  // Step 2: 加载渠道详情
  const loadGroupInfo = useCallback(async () => {
    if (!account || !selectedGroup) return;
    if (selectedGroup.isActive === false) return;
    setIsLoadingInfo(true);
    try {
      const result = await execute(async () =>
        fetchAction<DepositGroupInfo>(
          'getDepositGroupInfo',
          account.uid,
          selectedGroup.group,
          selectedGroup.type,
        )
      );
      if (!result.success || !result.data) return;

      let info: DepositGroupInfo = result.data;

      // ExLink Global 渠道：用 ExLink 实时汇率覆盖 currencyRates（保留汇率展示）
      const exLink = selectedGroup.type === EXLINK_GLOBAL_TYPE;
      // 多 method 渠道（ExLinkGlobal / Help2Pay）：Step 3 下拉项来自 paymentMethods，
      // 选中值用 hashId 而非 currencyId（Help2Pay 两条 IDR 行需要分别可选）。
      const multiMethod =
        selectedGroup.type !== undefined &&
        (MULTI_METHOD_TYPES as readonly string[]).includes(selectedGroup.type);

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
      // 自动选中初值：
      // - 多 method 渠道（ExLinkGlobal / Help2Pay）：仅当只有 1 个 paymentMethod 时自动选中其 hashId。
      // - 其他渠道：沿用 currencyRates 的旧逻辑，存储 currencyId 字符串。
      if (multiMethod && info.paymentMethods?.length) {
        if (info.paymentMethods.length === 1) {
          setSelectedMethodKey(info.paymentMethods[0].hashId);
        }
      } else if (info.currencyRates?.length === 1) {
        setSelectedMethodKey(String(info.currencyRates[0].currencyId));
      } else if (!info.currencyRates?.length && account.currencyId) {
        setSelectedMethodKey(String(account.currencyId));
      }
      setStep(2);
    } finally {
      setIsLoadingInfo(false);
    }
  }, [account, selectedGroup, execute]);

  // 多 method 渠道（ExLinkGlobal / Help2Pay）：按所选下拉项的 hashId 命中 paymentMethod 配置（提供 hashId / range / currencyId）
  const currentPaymentMethod = useMemo((): PaymentMethodConfig | null => {
    if (!isMultiMethodGroup || !groupInfo?.paymentMethods?.length || !selectedMethodKey) return null;
    return (
      groupInfo.paymentMethods.find((pm) => pm.hashId === selectedMethodKey) || null
    );
  }, [isMultiMethodGroup, groupInfo, selectedMethodKey]);

  const selectedCurrencyId = useMemo(() => {
    if (currentPaymentMethod) return currentPaymentMethod.currencyId;
    const n = Number(selectedMethodKey);
    return Number.isFinite(n) && n > 0 ? n : undefined;
  }, [currentPaymentMethod, selectedMethodKey]);

  const isExLinkJpy = useMemo(
    () => isExLinkGlobal && selectedCurrencyId === CurrencyTypes.JPY,
    [isExLinkGlobal, selectedCurrencyId]
  );

  // ExLinkGlobal JPY: load verified Wire payment infos for KYC-bound bank selector
  useEffect(() => {
    if (!open || !isExLinkJpy) {
      setWirePaymentInfos([]);
      return;
    }

    let cancelled = false;
    void (async () => {
      const result = await execute(getPaymentInfoList);
      if (cancelled) return;
      if (result.success && result.data) {
        setWirePaymentInfos(
          result.data.filter((x) => x.paymentPlatform === WIRE_PAYMENT_PLATFORM)
        );
      } else {
        setWirePaymentInfos([]);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [open, isExLinkJpy, execute]);

  // Clear JPY-only paymentInfoId when currency is not JPY
  useEffect(() => {
    if (isExLinkJpy) return;
    setJpyPaymentInfoError(false);
    setDynamicFields((prev) => {
      if (!prev.paymentInfoId) return prev;
      const { paymentInfoId: _removed, ...rest } = prev;
      return rest;
    });
  }, [isExLinkJpy]);

  // 当前汇率：
  // - 多 method 渠道：用 currentPaymentMethod.currencyId 反查 currencyRates。
  // - 其他渠道：selectedMethodKey 本身就是 currencyId 字符串。
  const currentRate = useMemo((): CurrencyRate | null => {
    if (!groupInfo?.currencyRates?.length) return null;
    const lookup = currentPaymentMethod
      ? String(currentPaymentMethod.currencyId)
      : selectedMethodKey;
    if (!lookup) return null;
    return groupInfo.currencyRates.find((r) => String(r.currencyId) === lookup) || null;
  }, [groupInfo, currentPaymentMethod, selectedMethodKey]);

  // Step 3 下拉选项：
  // - 多 method 渠道：来源于 paymentMethods，value=hashId（Help2Pay 两条 IDR 行需要分别可选），label 优先用 paymentMethodName。
  // - 其他渠道：沿用 currencyRates，value=currencyId 字符串，label 用币种名。
  const currencyOptions = useMemo<SelectOption[]>(() => {
    if (isMultiMethodGroup && groupInfo?.paymentMethods?.length) {
      return groupInfo.paymentMethods.map((pm) => ({
        value: pm.hashId,
        label: pm.paymentMethodName || getCurrencyName(pm.currencyId),
      }));
    }
    return (groupInfo?.currencyRates || []).map((cr) => ({
      value: String(cr.currencyId),
      label: getCurrencyName(cr.currencyId),
    }));
  }, [isMultiMethodGroup, groupInfo, getCurrencyName]);

  // 用于校验/展示的金额区间：多 method 渠道跟随当前 paymentMethod 切换，其他渠道沿用 groupInfo.range
  const activeRange = useMemo<[number, number] | undefined>(() => {
    if (isMultiMethodGroup) return currentPaymentMethod?.range;
    return groupInfo?.range;
  }, [isMultiMethodGroup, currentPaymentMethod, groupInfo]);

  // 当前展示的渠道名：
  // - 多 method 渠道（ExLinkGlobal / Help2Pay）且已选中 paymentMethod -> 用 paymentMethod.paymentMethodName
  // - 多 method 渠道但尚未选中 -> 退回 selectedGroup.group（如 "ExLink Global"、"Help2Pay"）
  // - 其他渠道 -> selectedGroup.paymentMethodName
  const displayMethodName = useMemo<string>(() => {
    if (isMultiMethodGroup) {
      return currentPaymentMethod?.paymentMethodName || selectedGroup?.group || '';
    }
    return selectedGroup?.paymentMethodName || '';
  }, [isMultiMethodGroup, currentPaymentMethod, selectedGroup]);

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
  // - ExLinkGlobal 渠道按当前币种命中的 paymentMethod.range 校验
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

    if (activeRange && account) {
      const [rawMin, rawMax] = activeRange;
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
  }, [activeRange, account]);

  // 多 method 渠道切换 paymentMethod -> range 改变，需要立即重新校验已有金额
  useEffect(() => {
    if (!isMultiMethodGroup) return;
    if (!amount) return;
    validateAmount(amount);
  }, [isMultiMethodGroup, currentPaymentMethod, amount, validateAmount]);

  // 可见的动态字段
  const visibleRequestKeys = useMemo(() => {
    return (groupInfo?.requestKeys || []).filter(
      (key) => !HIDDEN_REQUEST_KEYS.includes(key)
    );
  }, [groupInfo]);

  const requiresChineseNativeName =
    selectedGroup?.type === NPay_TYPE &&
    visibleRequestKeys.includes('nativeName');

  const nativeNameChineseError = useMemo(() => {
    if (!requiresChineseNativeName) return false;
    const nativeName = dynamicFields.nativeName?.trim();
    if (!nativeName) return false;
    return !CHINESE_NATIVE_NAME_REGEX.test(nativeName);
  }, [requiresChineseNativeName, dynamicFields.nativeName]);

  const validateJpyBeforeProceed = useCallback((): boolean => {
    if (!isExLinkJpy) return true;
    if (wirePaymentInfos.length === 0) {
      showWarning(t('fill.jpy.noVerifiedWireBankInfo'));
      return false;
    }
    if (!dynamicFields.paymentInfoId?.trim()) {
      setJpyPaymentInfoError(true);
      showWarning(t('fill.jpy.bankRequired'));
      return false;
    }
    setJpyPaymentInfoError(false);
    return true;
  }, [isExLinkJpy, wirePaymentInfos.length, dynamicFields.paymentInfoId, showWarning, t]);

  // Step 3 是否可前进（信用卡的字段校验在 CreditCardForm.validate 内部完成）
  const canProceedStep3 = useMemo(() => {
    if (!amount || Number(amount) <= 0) return false;
    if (amountError) return false;
    if (isExLinkJpy) {
      if (wirePaymentInfos.length === 0) return false;
      if (!dynamicFields.paymentInfoId?.trim()) return false;
    }
    if (!isCreditCard) {
      for (const key of visibleRequestKeys) {
        if (!dynamicFields[key]?.trim()) return false;
      }
    }
    if (nativeNameChineseError) return false;
    return true;
  }, [
    amount,
    amountError,
    isCreditCard,
    isExLinkJpy,
    wirePaymentInfos.length,
    visibleRequestKeys,
    dynamicFields,
    nativeNameChineseError,
  ]);

  const qrCodeImageSrc = useMemo(
    () => (depositResponse?.textForQrCode ? getBase64ImageDataUrl(depositResponse.textForQrCode) : ''),
    [depositResponse?.textForQrCode]
  );

  const qrTransactionId = useMemo(
    () => (depositResponse ? extractQrTransactionId(depositResponse) : ''),
    [depositResponse]
  );

  const bankTransferInfo = useMemo(() => {
    const info = depositResponse?.info;
    if (!info) return null;
    const bankName = info.bankName?.trim() || '';
    const bankBranch = info.bankBranch?.trim() || '';
    const accountName = info.accountName?.trim() || '';
    const accountNo = info.accountNo?.trim() || '';
    if (!bankName && !bankBranch && !accountName && !accountNo) return null;
    return { bankName, bankBranch, accountName, accountNo };
  }, [depositResponse?.info]);

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

  // QrCode 倒计时：当 depositResponse.message 变化时启动
  useEffect(() => {
    if (countdownTimerRef.current !== null) {
      clearInterval(countdownTimerRef.current);
      countdownTimerRef.current = null;
    }
    setIsExpired(false);
    setCountDown(0);

    const raw: unknown = depositResponse?.message;
    if (
      !raw ||
      (depositResponse?.action !== DepositActions.QrCode &&
        depositResponse?.action !== DepositActions.BankTransfer)
    ) return;

    // 解析 message：数字（分钟数）、数字字符串，或 UTC 绝对时间戳字符串
    let expiresAt: Date | null = null;
    if (typeof raw === 'number' && Number.isFinite(raw)) {
      expiresAt = new Date(Date.now() + raw * 60_000);
    } else if (typeof raw === 'string' && /^\d+(\.\d+)?$/.test(raw.trim())) {
      expiresAt = new Date(Date.now() + Number(raw.trim()) * 60_000);
    } else if (typeof raw === 'string') {
      // 服务端返回 UTC+0 时间字符串，强制按 UTC 解析（等价于 moment.utc(raw).local()）
      const utcStr = /Z$|[+-]\d{2}:\d{2}$/.test(raw.trim()) ? raw.trim() : raw.trim() + 'Z';
      const parsed = new Date(utcStr);
      if (!Number.isNaN(parsed.getTime())) expiresAt = parsed;
    }
    if (!expiresAt) return;

    const computeRemaining = () => Math.max(0, Math.floor((expiresAt!.getTime() - Date.now()) / 1000));

    const initial = computeRemaining();
    if (initial <= 0) { setIsExpired(true); return; }
    setCountDown(initial);

    countdownTimerRef.current = setInterval(() => {
      const remaining = computeRemaining();
      setCountDown(remaining);
      if (remaining <= 0 && countdownTimerRef.current !== null) {
        clearInterval(countdownTimerRef.current);
        countdownTimerRef.current = null;
        setIsExpired(true);
      }
    }, 1000);

    return () => {
      if (countdownTimerRef.current !== null) {
        clearInterval(countdownTimerRef.current);
        countdownTimerRef.current = null;
      }
    };
  }, [depositResponse?.message, depositResponse?.action]);

  const countDownText = useMemo(() => {
    const total = Math.max(0, countDown);
    const h = Math.floor(total / 3600);
    const m = Math.floor((total % 3600) / 60);
    const s = total % 60;
    const pad = (n: number) => String(n).padStart(2, '0');
    return h > 0 ? `${pad(h)}:${pad(m)}:${pad(s)}` : `${pad(m)}:${pad(s)}`;
  }, [countDown]);

  // Step 4: 提交入金
  const handleDeposit = useCallback(async () => {
    if (!account || !groupInfo || !selectedGroup) return;

    const numAmount = Number(amount);
    // 多 method 渠道：currencyId 来自当前 paymentMethod；其他渠道：selectedMethodKey 即 currencyId 字符串
    const resolvedCurrencyId = currentPaymentMethod
      ? currentPaymentMethod.currencyId
      : Number(selectedMethodKey) || account.currencyId;
    const requestData: Record<string, string | number> = {
      ...dynamicFields,
      amount: numAmount,
      currencyId: resolvedCurrencyId,
      returnUrl: typeof window !== 'undefined' ? window.location.href : '',
    };

    // 多 method 渠道（ExLinkGlobal / Help2Pay）的 hashId 由当前所选 paymentMethod 决定，
    // 其他渠道沿用 groupInfo 顶层 hashId
    const hashId = isMultiMethodGroup && currentPaymentMethod
      ? currentPaymentMethod.hashId
      : groupInfo.hashId;

    const payload = {
      hashId,
      amount: numAmount * 100,
      request: requestData,
    };

    const result = await execute(postAccountDeposit, account.uid, payload);
    if (result.success && result.data) {
      setDepositResponse(result.data);
      const { action, redirectUrl, endPoint, form } = result.data;

      // Post / Redirect：不显示 instruction，显示 MethodCard + 跳转链接
      const isRedirectAction = action === DepositActions.Post || action === DepositActions.Redirect;
      setShowInstruction(!isRedirectAction && action !== DepositActions.BankTransfer);

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
  }, [account, groupInfo, selectedGroup, amount, dynamicFields, selectedMethodKey, isMultiMethodGroup, currentPaymentMethod, execute]);

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
        onInteractOutside={(e) => e.preventDefault()}
        onPointerDownOutside={(e) => e.preventDefault()}
        onEscapeKeyDown={(e) => e.preventDefault()}
      >
        <div className="flex flex-1 flex-col gap-6 overflow-hidden">
          <DialogHeader>
            <DialogTitle>{t('title')}</DialogTitle>
            <DialogDescription className="sr-only">{t('title')}</DialogDescription>
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
                    {groups.length === 0 && (
                      <p className="col-span-full py-6 text-center text-sm text-text-secondary">
                        {t('channel.noChannel')}
                      </p>
                    )}
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
                              {group.type ? group.group : group.paymentMethodName}
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
             
                  {t('notice.title', { name: displayMethodName })}
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
                {/* 多 method 渠道（Help2Pay / ExLinkGlobal）选的是 paymentMethod；其他渠道选的是 currency */}
                {currencyOptions.length > 0 && (
                  <div className="flex flex-col gap-2">
                    <label className="flex items-center text-sm font-medium text-text-secondary">
                      <span className="mr-1 text-primary">*</span>
                      {selectedGroup?.group || ''}{isMultiMethodGroup ? t('fill.paymentMethod') : t('fill.currency')}
                    </label>
                    <SimpleSelect
                      value={selectedMethodKey}
                      onChange={setSelectedMethodKey}
                      options={currencyOptions}
                      placeholder={isMultiMethodGroup ? t('fill.selectPaymentMethod') : t('fill.selectCurrency')}
                      disabled={currencyOptions.length === 1}
                      triggerSize="md"
                      className="w-full bg-input-bg"
                    />
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
                      disabled={!selectedMethodKey}
                      error={
                        amountError === 'required'
                          ? t('error.amountRequired')
                          : amountError === 'integer'
                            ? t('error.amountInteger')
                            : undefined
                      }
                      errorPosition="bottom"
                    />
                    {amountError === 'range' && activeRange && (
                      <p className="error-text mt-1 text-sm">
                        {t('error.amountRange')}
                        <BalanceShow balance={account.currencyId === CurrencyTypes.USC ? activeRange[0] * 100 : activeRange[0]} currencyId={account.currencyId} />
                        ~
                        <BalanceShow balance={account.currencyId === CurrencyTypes.USC ? activeRange[1] * 100 : activeRange[1]} currencyId={account.currencyId} />
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

                {/* ExLinkGlobal JPY H2H: KYC-bound Wire bank account */}
                {!isCreditCard && isExLinkJpy && (
                  <div className="flex flex-col gap-3">
                    <div
                      className="flex items-start gap-2 rounded-lg px-3.5 py-2.5 text-[13px] leading-normal"
                      style={{ backgroundColor: '#ffecec', color: '#9f005b' }}
                    >
                      <svg
                        width="20"
                        height="20"
                        viewBox="0 0 20 20"
                        fill="none"
                        className="mt-0.5 shrink-0"
                        aria-hidden
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
                      <span>{t('fill.jpy.kycWarning')}</span>
                    </div>

                    <div className="flex flex-col gap-2">
                      <label className="flex items-center text-sm font-medium text-text-secondary">
                        <span className="mr-1 text-primary">*</span>
                        {t('fill.jpy.kycBoundBankAccount')}
                      </label>

                      {wireBankSelectOptions.length > 0 ? (
                        <SimpleSelect
                          value={dynamicFields.paymentInfoId || ''}
                          onChange={(val) => {
                            setDynamicFields((prev) => ({ ...prev, paymentInfoId: val }));
                            if (val) setJpyPaymentInfoError(false);
                          }}
                          options={wireBankSelectOptions}
                          placeholder={t('fill.jpy.selectKycBoundBankAccount')}
                          triggerSize="md"
                          className="w-full bg-input-bg"
                        />
                      ) : (
                        <div className="flex items-start gap-2 rounded-lg bg-surface-secondary px-3 py-3 text-sm text-text-secondary">
                          <svg
                            width="20"
                            height="20"
                            viewBox="0 0 20 20"
                            fill="none"
                            className="mt-0.5 shrink-0 text-primary"
                            aria-hidden
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
                          <span>{t('fill.jpy.noVerifiedWireBankInfo')}</span>
                        </div>
                      )}

                      {jpyPaymentInfoError && (
                        <p className="error-text mt-1 text-sm">{t('fill.jpy.bankRequired')}</p>
                      )}
                    </div>
                  </div>
                )}

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
                  <div className="grid grid-cols-2 gap-4">
                    {visibleRequestKeys.map((key) => {
                      const label = t(`requestKeys.${key}`, { defaultMessage: key });
                      // Help2Pay 的 `bank` 字段：当前 (channel x currency) 行带白名单时，
                      // 用 "CODE - Name" 下拉选择，避免用户手填触发后端
                      // __HELP2PAY_BANK_NOT_SUPPORTED__ 等校验失败。
                      if (key === 'bank' && currentPaymentMethod?.banks?.length) {
                        // Legacy DB rows (pre-name migration) come back with name === code;
                        // showing "MBB - MBB" is noisy, so collapse to the code alone in that case.
                        const bankOptions: SelectOption[] = currentPaymentMethod.banks.map((b) => ({
                          value: b.code,
                          label: b.name && b.name !== b.code ? `${b.code} - ${b.name}` : b.code,
                        }));
                        return (
                          <div key={key} className="flex flex-col gap-1">
                            <label className="flex items-center text-sm font-medium text-text-secondary">
                              <span className="mr-1 text-primary">*</span>
                              {label}
                            </label>
                            <SimpleSelect
                              value={dynamicFields[key] || ''}
                              onChange={(val) =>
                                setDynamicFields((prev) => ({ ...prev, [key]: val }))
                              }
                              options={bankOptions}
                              placeholder={t('fill.selectBank', { defaultMessage: label })}
                              triggerSize="md"
                              className="w-full bg-input-bg"
                            />
                          </div>
                        );
                      }
                      return (
                        <Input
                          key={key}
                          label={label}
                          required
                          inputSize="md"
                          value={dynamicFields[key] || ''}
                          onChange={(e) =>
                            setDynamicFields((prev) => ({ ...prev, [key]: e.target.value }))
                          }
                          error={
                            requiresChineseNativeName && key === 'nativeName' && nativeNameChineseError
                              ? t('error.nativeNameChinese')
                              : undefined
                          }
                          errorPosition="bottom"
                        />
                      );
                    })}
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
                      {displayMethodName}
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
                            <div className="rounded bg-white p-1.5">
                              <QRCodeSVG
                                value={depositResponse.textForQrCode}
                                size={132}
                                level="M"
                              />
                            </div>
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
                          <p className="text-xs text-text-secondary">
                            {t('guide.paymentExpireTime')}：{' '}
                            <span className={`font-bold ${isExpired ? 'text-error' : 'text-danger'}`}>
                              {isExpired ? t('guide.expired') : countDownText}
                            </span>
                          </p>
                        )}

                        {qrTransactionId && (
                          <Button
                            variant="primary"
                            disabled={isPaidSubmitting || isPaidConfirmed || isExpired}
                            loading={isPaidSubmitting}
                            onClick={notifyPaid}
                          >
                            {isPaidConfirmed ? t('guide.paidConfirmed') : t('guide.completePayment')}
                          </Button>
                        )}
                      </div>
                    )}

                    {depositResponse.action === DepositActions.BankTransfer && bankTransferInfo && (
                      <div className="flex flex-col gap-3">
                        <p className="text-sm text-text-secondary">{t('guide.bankTransferNotice')}</p>
                        <div className="grid grid-cols-2 gap-2">
                          {(
                            [
                              ['bankName', bankTransferInfo.bankName],
                              ['bankBranch', bankTransferInfo.bankBranch],
                              ['accountName', bankTransferInfo.accountName],
                              ['accountNo', bankTransferInfo.accountNo],
                            ] as const
                          )
                            .filter(([, value]) => value)
                            .map(([key, value]) => (
                              <div
                                key={key}
                                className="flex min-w-0 items-center justify-between gap-2 rounded-lg bg-surface-secondary px-3 py-3"
                              >
                                <div className="min-w-0">
                                  <p className="text-xs text-text-secondary">{t(`guide.${key}`)}</p>
                                  <p className="text-sm font-medium text-text-primary break-all">{value}</p>
                                </div>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  className="shrink-0"
                                  onClick={() => {
                                    navigator.clipboard.writeText(value);
                                    showSuccess(t('guide.copied'));
                                  }}
                                >
                                  {t('guide.copy')}
                                </Button>
                              </div>
                            ))}
                        </div>

                        {depositResponse.message && (
                          <p className="text-xs text-text-secondary">
                            {t('guide.paymentExpireTime')}：{' '}
                            <span className={`font-bold ${isExpired ? 'text-error' : 'text-danger'}`}>
                              {isExpired ? t('guide.expired') : countDownText}
                            </span>
                          </p>
                        )}

                        {qrTransactionId && (
                          <Button
                            variant="primary"
                            disabled={isPaidSubmitting || isPaidConfirmed || isExpired}
                            loading={isPaidSubmitting}
                            onClick={notifyPaid}
                          >
                            {isPaidConfirmed ? t('guide.paidConfirmed') : t('guide.completePayment')}
                          </Button>
                        )}
                      </div>
                    )}

                    {/* v-if="showInstruction": instruction + 存款金额；v-else: MethodCard + 跳转链接 */}
                    {showInstruction ? (
                      <div className="flex flex-col gap-2">
                        <div
                          className="prose prose-sm max-w-none text-text-secondary dark:prose-invert"
                          dangerouslySetInnerHTML={{ __html: depositResponse.instruction || '' }}
                        />
                        {targetAmount > 0 && (
                          <>
                            <p className="text-sm text-text-secondary">{t('fill.depositAmount')}</p>
                            <p className="text-base font-semibold text-text-primary">
                              {formatAmount(targetAmount)}
                            </p>
                          </>
                        )}
                      </div>
                    ) : (
                      <>
                        {/* MethodCard */}
                        {selectedGroup && (
                          <div className="w-[200px] min-h-[160px] rounded-lg border border-border bg-surface-secondary p-5 flex flex-col gap-0">
                            <div className="flex items-center justify-between text-xs text-text-secondary">
                              <span>{t('channel.noFee')}</span>
                              <span>{'< 1'} {t('channel.hour')}</span>
                            </div>
                            <hr className="my-1 border-border" />
                            <div className="text-right text-xs text-text-secondary">24/5</div>
                            <div className="flex flex-1 items-center justify-center py-2">
                              {selectedGroup.logo && (
                                <Image
                                  src={selectedGroup.logo}
                                  alt={displayMethodName}
                                  width={110}
                                  height={45}
                                  className="max-h-[45px] w-auto object-contain"
                                  unoptimized
                                />
                              )}
                            </div>
                            <div className="text-center text-xs text-text-secondary mt-2">
                              {displayMethodName}
                            </div>
                          </div>
                        )}

                        {/* 跳转链接（Post / Redirect 分支必然有 redirectUrl） */}
                        {depositResponse.redirectUrl && (
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
                    )}
                  </>
                ) : (
                  <div className="flex flex-col gap-4">
                    <div className="flex items-start gap-4 rounded-lg border border-border p-4">
                      {selectedGroup?.logo && (
                        <Image
                          src={selectedGroup.logo}
                          alt={displayMethodName}
                          width={48}
                          height={48}
                          className="shrink-0 rounded"
                        />
                      )}
                      <div className="flex flex-col gap-1">
                        <span className="text-base font-medium text-text-primary">
                          {displayMethodName}
                        </span>
                      </div>
                    </div>
                    <p className="text-sm error-text">
                      {depositErrorMessage}
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
                      if (!validateJpyBeforeProceed()) return;
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
