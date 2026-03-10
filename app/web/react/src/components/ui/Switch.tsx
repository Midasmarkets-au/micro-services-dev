'use client';

import { cn } from '@/lib/utils';

export interface SwitchProps {
  /** 当前是否选中 */
  checked: boolean;
  /** 值变化回调 */
  onChange: (checked: boolean) => void;
  /** 是否禁用 */
  disabled?: boolean;
  className?: string;
}

/**
 * 开关组件，用于二选一状态切换。
 *
 * @example
 * ```tsx
 * <Switch checked={isClosed} onChange={setIsClosed} />
 * ```
 */
export function Switch({ checked, onChange, disabled = false, className }: SwitchProps) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      disabled={disabled}
      onClick={() => onChange(!checked)}
      className={cn(
        'relative inline-flex h-5 w-8 shrink-0 cursor-pointer items-center rounded-full transition-colors duration-200',
        checked ? 'bg-primary' : 'bg-[#ccc] dark:bg-[#555]',
        disabled && 'cursor-not-allowed opacity-50',
        className,
      )}
    >
      <span
        className={cn(
          'pointer-events-none inline-block size-4 rounded-full bg-white shadow-sm transition-transform duration-200',
          checked ? 'translate-x-[14px]' : 'translate-x-[2px]',
        )}
      />
    </button>
  );
}
