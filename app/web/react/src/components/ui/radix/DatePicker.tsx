'use client';

import * as React from 'react';
import { format } from 'date-fns';
import {
  zhCN,
  zhTW,
  enUS,
  vi,
  th,
  ja,
  id,
  ms,
  ko,
  km,
  es,
  type Locale as DateFnsLocale,
} from 'date-fns/locale';
import { type DateRange } from 'react-day-picker';
import { CalendarIcon } from '@radix-ui/react-icons';
import * as Popover from '@radix-ui/react-popover';
import { useTranslations, useLocale } from 'next-intl';
import { cn } from '@/lib/utils';
import { Calendar } from './Calendar';

export type { DateRange };

// date-fns locale map
const DATE_FNS_LOCALE_MAP: Record<string, DateFnsLocale> = {
  zh: zhCN,
  'zh-tw': zhTW,
  en: enUS,
  vi,
  th,
  jp: ja,
  id,
  ms,
  ko,
  km,
  es,
};


export function formatDateForApi(date: Date | undefined): string | undefined {
  if (!date) return undefined;
  return format(date, 'yyyy-MM-dd');
}

/**
 * DatePicker 尺寸：
 * - `sm`: 高度 36px (h-9)，适用于筛选栏、紧凑场景
 * - `md`: 高度 48px (h-12)，默认尺寸，适用于表单
 */
type DatePickerSize = 'sm' | 'md';

interface DatePickerBaseProps {
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  /** @deprecated 已自动从 next-intl useLocale() 读取，无需手动传入 */
  locale?: string;
  /** 触发器尺寸，默认 md */
  size?: DatePickerSize;
  className?: string;
  captionLayout?: 'label' | 'dropdown' | 'dropdown-months' | 'dropdown-years';
  startMonth?: Date;
  endMonth?: Date;
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
    size = 'md',
    className,
    mode = 'single',
    captionLayout,
    startMonth,
    endMonth,
  } = props;

  const [open, setOpen] = React.useState(false);
  const currentLocale = useLocale();
  const dateFnsLocale = DATE_FNS_LOCALE_MAP[currentLocale] ?? enUS;
  // react-day-picker/locale re-exports date-fns locales, so we reuse dateFnsLocale
  const rdpLocale = dateFnsLocale;

  const [tempRange, setTempRange] = React.useState<DateRange | undefined>(
    mode === 'range' ? (props as RangeDatePickerProps).value : undefined
  );

  React.useEffect(() => {
    if (open && mode === 'range') {
      setTempRange((props as RangeDatePickerProps).value);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
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
      return v ? format(v, 'yyyy-MM-dd', { locale: dateFnsLocale }) : '';
    }
    const rv = (props as RangeDatePickerProps).value;
    if (rv?.from && rv?.to) {
      return `${format(rv.from, 'yyyy.MM.dd')} - ${format(rv.to, 'yyyy.MM.dd')}`;
    }
    if (rv?.from) {
      return `${format(rv.from, 'yyyy.MM.dd')} -`;
    }
    return '';
  }, [mode, props, dateFnsLocale]);

  const t = useTranslations('common');
  const defaultPlaceholder = t('selectDate');

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

  const triggerClass = cn(
    'input-field flex w-full items-center gap-2 text-left cursor-pointer',
    size === 'sm' && 'h-9! px-3!',
    'disabled:cursor-not-allowed disabled:opacity-50',
    error && 'error-border',
    !displayText && 'text-text-placeholder',
    className
  );

  return (
    <Popover.Root open={open} onOpenChange={setOpen}>
      <Popover.Trigger asChild>
        <button type="button" disabled={disabled} className={triggerClass}>
          <CalendarIcon className={cn('shrink-0 text-text-secondary', size === 'sm' ? 'size-4' : 'size-5')} />
          <span className="flex-1 truncate">
            {displayText || placeholder || defaultPlaceholder}
          </span>
        </button>
      </Popover.Trigger>

      <Popover.Portal>
        <Popover.Content
          className={cn(
            'z-50 rounded-xl border border-border p-0 overflow-hidden',
            'bg-surface',
            'shadow-dropdown',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[side=bottom]:slide-in-from-top-2 data-[side=top]:slide-in-from-bottom-2'
          )}
          sideOffset={4}
          align="start"
        >
          {mode === 'single' ? (
            <Calendar
              mode="single"
              selected={(props as SingleDatePickerProps).value}
              onSelect={handleSingleSelect}
              defaultMonth={defaultMonth}
              locale={rdpLocale}
              weekStartsOn={0}
              captionLayout={captionLayout}
              startMonth={startMonth}
              endMonth={endMonth}
            />
          ) : (
            <div className="flex flex-col gap-0">
              <Calendar
                mode="range"
                selected={tempRange}
                onSelect={handleRangeSelect}
                defaultMonth={defaultMonth}
                locale={rdpLocale}
                weekStartsOn={0}
                captionLayout={captionLayout}
                startMonth={startMonth}
                endMonth={endMonth}
              />

              <div className="border-t border-border px-4 py-3">
                <div className="mb-3 flex items-center gap-3 text-sm text-text-primary">
                  <span className="flex-1">
                    {tempRange?.from ? format(tempRange.from, 'yyyy.MM.dd') : '—'}
                  </span>
                  <span className="text-text-secondary">{t('to')}</span>
                  <span className="flex-1 text-right">
                    {tempRange?.to ? format(tempRange.to, 'yyyy.MM.dd') : '—'}
                  </span>
                </div>
                <div className="flex gap-3">
                  <button
                    type="button"
                    onClick={handleReset}
                    className="flex-1 rounded-lg bg-surface-secondary py-2 text-sm text-text-primary transition-colors hover:opacity-80"
                  >
                    {t('reset')}
                  </button>
                  <button
                    type="button"
                    onClick={handleConfirm}
                    className="flex-1 rounded-lg bg-primary py-2 text-sm text-white transition-colors hover:opacity-90"
                  >
                    {t('confirm')}
                  </button>
                </div>
              </div>
            </div>
          )}
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}
