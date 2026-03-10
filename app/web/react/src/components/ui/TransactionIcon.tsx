'use client';

import { TransactionType } from '@/types/wallet';

interface TransactionIconProps {
  type: TransactionType;
  flowType?: string;
  size?: number;
  className?: string;
}

/**
 * 交易类型图标 —— 内联 SVG，自动适配日间/夜间模式
 * 使用 CSS 变量 (--color-primary) 作为主色调
 */
export function TransactionIcon({
  type,
  flowType,
  size = 32,
  className,
}: TransactionIconProps) {
  const iconConfig = getIconConfig(type, flowType);

  return (
    <div
      className={`shrink-0 rounded-full flex items-center justify-center ${iconConfig.bgClass} ${className ?? ''}`}
      style={{ width: size, height: size }}
    >
      <svg
        width={size * 0.5}
        height={size * 0.5}
        viewBox="0 0 16 16"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        {iconConfig.path}
      </svg>
    </div>
  );
}

interface IconConfig {
  bgClass: string;
  path: React.ReactNode;
}

function getIconConfig(type: TransactionType, flowType?: string): IconConfig {
  switch (type) {
    case TransactionType.Withdrawal:
      return {
        bgClass: 'bg-[rgba(224,43,29,0.1)]',
        path: (
          <path
            d="M8 12V4M8 4L4.5 7.5M8 4l3.5 3.5"
            stroke="#e02b1d"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        ),
      };

    case TransactionType.Transfer:
      return {
        bgClass: 'bg-[var(--color-primary-light)]',
        path: (
          <>
            <path
              d="M3 6h10M10 3.5L13 6l-3 2.5"
              stroke="var(--color-primary)"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
            <path
              d="M13 10H3M6 7.5L3 10l3 2.5"
              stroke="var(--color-primary)"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </>
        ),
      };

    case TransactionType.Rebate:
      return {
        bgClass: 'bg-[rgba(0,146,98,0.1)]',
        path: (
          <path
            d="M8,0 C3.582,0 0,3.582 0,8 C0,12.418 3.582,16 8,16 C12.418,16 16,12.418 16,8 C16,3.583 12.417,0 8,0 Z M12.804,7.684 L11.929,8.908 L10.917,7.792 C10.815,7.677 10.824,7.502 10.936,7.397 C11.051,7.295 11.226,7.304 11.33,7.416 L11.613,7.728 C11.476,5.99 10.12,4.595 8.385,4.412 C6.651,4.228 5.032,5.306 4.534,6.978 C4.036,8.649 4.8,10.438 6.352,11.234 C7.904,12.029 9.803,11.605 10.869,10.225 C10.963,10.117 11.123,10.096 11.241,10.177 C11.363,10.27 11.386,10.445 11.293,10.567 C10.06,12.154 7.872,12.64 6.083,11.726 C4.293,10.811 3.406,8.754 3.97,6.824 C4.534,4.895 6.389,3.639 8.389,3.832 C10.39,4.024 11.971,5.612 12.156,7.613 L12.339,7.356 C12.425,7.239 12.589,7.209 12.711,7.289 C12.777,7.328 12.824,7.392 12.842,7.466 C12.86,7.541 12.846,7.62 12.804,7.684 Z M9.388,8.086 L9.388,8.532 L8.465,8.532 L8.465,8.86 L9.388,8.86 L9.388,9.302 L8.465,9.302 L8.465,10.121 L7.814,10.121 L7.814,9.302 L6.902,9.302 L6.902,8.856 L7.814,8.856 L7.814,8.528 L6.902,8.528 L6.902,8.082 L7.609,8.082 L6.545,6.221 L7.289,6.221 L8.141,7.814 L9.008,6.207 L9.753,6.207 L8.688,8.067 L9.388,8.086 Z"
            fill="#009262"
            fillRule="nonzero"
          />
        ),
      };

    case TransactionType.Refund:
      return {
        bgClass: 'bg-[rgba(255,138,0,0.1)]',
        path: (
          <path
            d="M4 6.5h5.5a2.5 2.5 0 010 5H8M4 6.5L6.5 4M4 6.5L6.5 9"
            stroke="#ff8a00"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        ),
      };

    case TransactionType.Adjust:
      return {
        bgClass: 'bg-[rgba(128,128,128,0.1)]',
        path: (
          <>
            <path
              d="M3 5h10M3 8h7M3 11h4"
              stroke="var(--color-text-secondary)"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
            <circle cx="12" cy="10" r="1" fill="var(--color-text-secondary)" />
          </>
        ),
      };

    case TransactionType.DownlineReward:
      if (flowType === 'in') {
        return {
          bgClass: 'bg-[rgba(0,146,98,0.1)]',
          path: (
            <path
              d="M8 4v8M8 12l3.5-3.5M8 12L4.5 8.5"
              stroke="#009262"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ),
        };
      }
      return {
        bgClass: 'bg-[rgba(224,43,29,0.1)]',
        path: (
          <path
            d="M8 12V4M8 4L4.5 7.5M8 4l3.5 3.5"
            stroke="#e02b1d"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        ),
      };

    default:
      return {
        bgClass: 'bg-[var(--color-primary-light)]',
        path: (
          <circle cx="8" cy="8" r="2" stroke="var(--color-primary)" strokeWidth="1.5" />
        ),
      };
  }
}
