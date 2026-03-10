'use client';

import * as React from 'react';
import { format } from 'date-fns';
import { zhCN, enUS } from 'date-fns/locale';
import { DayPicker, type DateRange } from 'react-day-picker';
import { CalendarIcon } from '@radix-ui/react-icons';
import * as Popover from '@radix-ui/react-popover';
import { cn } from '@/lib/utils';

export type { DateRange };

export function formatDateForApi(date: Date | undefined): string | undefined {
  if (!date) return undefined;
  return format(date, 'yyyy-MM-dd');
}

/**
 * DatePicker 尺寸：
 * - `sm`: 高度 36px (h-9)，圆角 4px，适用于筛选栏、紧凑场景
 * - `md`: 高度 48px (h-12)，圆角 12px，默认尺寸，适用于表单
 */
type DatePickerSize = 'sm' | 'md';

const SIZE_CLASS: Record<DatePickerSize, string> = {
  sm: 'h-9! px-3!',
  md: '',
};

interface DatePickerBaseProps {
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  locale?: 'zh' | 'en';
  /** 触发器尺寸，默认 md */
  size?: DatePickerSize;
  className?: string;
}

interface SingleDatePickerProps extends DatePickerBaseProps {
  mode?: 'single';
  value?: Date;
  onChange?: (date: Date | undefined) => void;
}

interface RangeDatePickerProps extends DatePickerBaseProps {
  mode: 'range';
  value?: DateRange;
  onChange?: (range: DateRange | undefined) => void;
}

export type DatePickerProps = SingleDatePickerProps | RangeDatePickerProps;

export function DatePicker(props: DatePickerProps) {
  const {
    placeholder,
    disabled = false,
    error = false,
    locale = 'zh',
    size = 'md',
    className,
    mode = 'single',
  } = props;

  const [open, setOpen] = React.useState(false);
  const dateLocale = locale === 'zh' ? zhCN : enUS;

  const [tempRange, setTempRange] = React.useState<DateRange | undefined>(
    mode === 'range' ? (props as RangeDatePickerProps).value : undefined
  );

  React.useEffect(() => {
    if (open && mode === 'range') {
      setTempRange((props as RangeDatePickerProps).value);
    }
  }, [open]);

  const defaultMonth = React.useMemo(() => {
    if (mode === 'single') {
      return (props as SingleDatePickerProps).value || new Date();
    }
    const rv = (props as RangeDatePickerProps).value;
    return rv?.from || new Date();
  }, [mode, props]);

  const displayText = React.useMemo(() => {
    if (mode === 'single') {
      const v = (props as SingleDatePickerProps).value;
      return v ? format(v, 'yyyy-MM-dd', { locale: dateLocale }) : '';
    }
    const rv = (props as RangeDatePickerProps).value;
    if (rv?.from && rv?.to) {
      return `${format(rv.from, 'yyyy.MM.dd')} - ${format(rv.to, 'yyyy.MM.dd')}`;
    }
    if (rv?.from) {
      return `${format(rv.from, 'yyyy.MM.dd')} -`;
    }
    return '';
  }, [mode, props, dateLocale]);

  const defaultPlaceholder = locale === 'zh' ? '选择日期' : 'Select Date';

  const handleSingleSelect = (date: Date | undefined) => {
    if (mode === 'single') {
      (props as SingleDatePickerProps).onChange?.(date);
      setOpen(false);
    }
  };

  const handleRangeSelect = (range: DateRange | undefined) => {
    setTempRange(range);
  };

  const handleReset = () => {
    if (mode === 'range') {
      setTempRange(undefined);
      (props as RangeDatePickerProps).onChange?.(undefined);
    }
    setOpen(false);
  };

  const handleConfirm = () => {
    if (mode === 'range') {
      (props as RangeDatePickerProps).onChange?.(tempRange);
    }
    setOpen(false);
  };

  const titleText = locale === 'zh' ? '选择时间' : 'Select Date';
  const resetText = locale === 'zh' ? '重置' : 'Reset';
  const confirmText = locale === 'zh' ? '确定' : 'Confirm';
  const toText = locale === 'zh' ? '至' : 'to';

  return (
    <Popover.Root open={open} onOpenChange={setOpen}>
      <Popover.Trigger asChild>
        <button
          type="button"
          disabled={disabled}
          className={cn(
            'input-field flex items-center gap-2 text-left',
            SIZE_CLASS[size],
            'disabled:cursor-not-allowed disabled:opacity-50',
            error && 'error-border',
            !displayText && 'text-text-placeholder',
            className
          )}
        >
          <CalendarIcon className={cn('shrink-0 text-text-secondary', size === 'sm' ? 'size-4' : 'size-5')} />
          <span className="flex-1 truncate">
            {displayText || placeholder || defaultPlaceholder}
          </span>
        </button>
      </Popover.Trigger>

      <Popover.Portal>
        <Popover.Content
          className={cn(
            'z-50 rounded-xl border p-5',
            'bg-surface border-border',
            'shadow-dropdown',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[side=bottom]:slide-in-from-top-2 data-[side=top]:slide-in-from-bottom-2'
          )}
          sideOffset={4}
          align="start"
        >
          <div className="flex flex-col gap-5">
            <div className="text-xs text-text-secondary">{titleText}</div>

            {mode === 'single' ? (
              <DayPicker
                mode="single"
                selected={(props as SingleDatePickerProps).value}
                onSelect={handleSingleSelect}
                defaultMonth={defaultMonth}
                locale={dateLocale}
                weekStartsOn={0}
                className="mdm-calendar"
              />
            ) : (
              <DayPicker
                mode="range"
                selected={tempRange}
                onSelect={handleRangeSelect}
                defaultMonth={defaultMonth}
                locale={dateLocale}
                weekStartsOn={0}
                className="mdm-calendar"
              />
            )}

            {mode === 'range' && (
              <>
                <div className="h-px bg-border" />
                <div className="flex items-center gap-3 text-sm text-text-primary">
                  <span className="flex-1">
                    {tempRange?.from ? format(tempRange.from, 'yyyy.MM.dd') : '—'}
                  </span>
                  <span className="text-text-secondary">{toText}</span>
                  <span className="flex-1 text-right">
                    {tempRange?.to ? format(tempRange.to, 'yyyy.MM.dd') : '—'}
                  </span>
                </div>
              </>
            )}

            <div className="flex gap-5">
              <button
                type="button"
                onClick={handleReset}
                className="flex-1 rounded-lg bg-surface-secondary py-2.5 text-sm text-text-primary transition-colors hover:opacity-80"
              >
                {resetText}
              </button>
              <button
                type="button"
                onClick={handleConfirm}
                className="flex-1 rounded-lg bg-primary py-2.5 text-sm text-white transition-colors hover:opacity-90"
              >
                {confirmText}
              </button>
            </div>
          </div>
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}
