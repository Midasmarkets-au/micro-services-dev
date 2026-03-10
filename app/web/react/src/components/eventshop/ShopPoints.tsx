'use client';

import Image from 'next/image';
import Decimal from 'decimal.js';
import { useTheme } from '@/hooks/useTheme';

interface ShopPointsProps {
  value: number | string;
  showSign?: boolean;
  decimals?: number;
  showIcon?: boolean;
  className?: string;
}

/**
 * 积分显示组件
 * action 层 normalizeAmountList 已经除了一次 10000，
 * 这里再除一次 10000，与 Vue ShopPoints 逻辑一致（总共除 10^8）。
 */
export function ShopPoints({
  value,
  showSign = false,
  decimals = 2,
  showIcon = true,
  className = '',
}: ShopPointsProps) {
  const { theme } = useTheme();
  const raw = typeof value === 'string' ? parseFloat(value) : value;
  if (isNaN(raw)) return <span className={className}>-</span>;

  const displayValue = new Decimal(raw).div(10000);
  const abs = displayValue.abs().toFixed(decimals);
  const formatted = Number(abs).toLocaleString(undefined, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });

  let text: string;
  if (!showSign) {
    text = formatted;
  } else {
    text = displayValue.gte(0) ? `+${formatted}` : `-${formatted}`;
  }

  const isPositive = displayValue.gte(0);
  const colorClass = showSign
    ? isPositive ? 'text-[#333] dark:text-text-primary' : 'text-text-secondary'
    : '';

  return (
    <span className={`inline-flex items-center gap-1 font-['DIN',sans-serif] text-sm font-bold leading-normal ${colorClass} ${className}`}>
      {showIcon && (
        <Image
          src={theme === 'dark' ? '/images/eventshop/points-icon-night.svg' : '/images/eventshop/points-icon-day.svg'}
          alt=""
          width={20}
          height={20}
          className="shrink-0 aspect-square"
        />
      )}
      <span>{text}</span>
    </span>
  );
}
