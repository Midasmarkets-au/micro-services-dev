'use client';

import { useMemo } from 'react';
import { useLocale } from 'next-intl';
import { CurrencyTypes } from '@/types/accounts';

export const CurrencyCodeMap: Record<number, string> = Object.fromEntries(
  Object.entries(CurrencyTypes)
    .filter(([key, value]) => !Number.isNaN(Number(key)) && typeof value === 'string')
    .map(([key, value]) => [Number(key), value])
) as Record<number, string>;

const LOCALE_MAP: Record<string, string> = {
  en: 'en-US',
  zh: 'zh-CN',
  'zh-tw': 'zh-TW',
  vi: 'vi-VN',
  th: 'th-TH',
  ja: 'ja-JP',
  id: 'id-ID',
  ms: 'ms-MY',
  ko: 'ko-KR',
  km: 'km-KH',
  es: 'es-ES',
};

export interface BalanceShowProps {
  balance: number;
  currencyId?: number;
  locale?: string;
  sign?: '+' | '-' | '';
  className?: string;
  /** 固定小数位数，传入后覆盖默认的自动判断逻辑（有小数显示4位，无小数显示2位） */
  fractionDigits?: number;
}

/**
 * 金额显示组件，完全匹配旧项目逻辑：
 * - 数据源：旧项目 BalanceShow.vue + filters.toCurrency
 * - balance 已经过 normalizeAmountList(÷10000) 处理，单位为"分"
 * - toCurrency: value / 100 → Intl.NumberFormat 格式化
 * - 有小数部分时显示4位，否则2位
 * - currencyId == -1 时回退为 USD
 */
export function BalanceShow({
  balance,
  currencyId = 840,
  locale: localeProp,
  sign = '',
  className,
  fractionDigits,
}: BalanceShowProps) {
  const nextIntlLocale = useLocale();
  const locale = localeProp || LOCALE_MAP[nextIntlLocale] || nextIntlLocale || 'en-US';
  const effectiveCurrencyId = currencyId === -1 ? 840 : currencyId;

  const displayBalance = sign ? Math.abs(balance) : balance;

  const formatted = useMemo(
    () => formatBalance(displayBalance, effectiveCurrencyId, locale, fractionDigits),
    [displayBalance, effectiveCurrencyId, locale, fractionDigits]
  );

  return (
    <span className={className}>
      {sign}{formatted}
    </span>
  );
}

/**
 * 纯函数版本，完全对应旧项目 filters.toCurrency
 */
export function formatBalance(
  balance: number,
  currencyId: number = 840,
  locale: string = 'en-US',
  fractionDigits?: number
): string {
  const id = currencyId === -1 ? 840 : currencyId;
  const value = balance;
  const hasDecimal = value % 1 !== 0;
  const digits = fractionDigits !== undefined ? fractionDigits : (hasDecimal ? 4 : 2);
  const code = CurrencyCodeMap[id] || 'USD';

  try {
    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency: code,
      minimumFractionDigits: digits,
      maximumFractionDigits: digits,
    }).format(value/100);
  } catch {
    return `${code} ${value.toFixed(digits)}`;
  }
}
