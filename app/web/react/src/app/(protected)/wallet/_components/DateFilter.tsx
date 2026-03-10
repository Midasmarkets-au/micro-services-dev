'use client';

import { useCallback, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';
import { Button, DatePicker, type DateRange } from '@/components/ui';
import { format, parse } from 'date-fns';

interface DateFilterProps {
  startDate: string | null;
  endDate: string | null;
  onDateChange: (startDate: string | null, endDate: string | null) => void;
  onReset: () => void;
  onSearch: () => void;
}

function toDate(s: string | null): Date | undefined {
  if (!s) return undefined;
  const d = parse(s, 'yyyy-MM-dd', new Date());
  return isNaN(d.getTime()) ? undefined : d;
}

export function DateFilter({
  startDate,
  endDate,
  onDateChange,
  onReset,
  onSearch,
}: DateFilterProps) {
  const t = useTranslations('wallet');

  const rangeValue = useMemo<DateRange | undefined>(() => {
    const from = toDate(startDate);
    if (!from) return undefined;
    return { from, to: toDate(endDate) };
  }, [startDate, endDate]);

  const handleRangeChange = useCallback(
    (range: DateRange | undefined) => {
      const s = range?.from ? format(range.from, 'yyyy-MM-dd') : null;
      const e = range?.to ? format(range.to, 'yyyy-MM-dd') : null;
      onDateChange(s, e);
    },
    [onDateChange]
  );

  const handleReset = useCallback(() => {
    onDateChange(null, null);
    onReset();
  }, [onDateChange, onReset]);

  return (
    <div className="flex items-center gap-4 md:gap-10">
      <DatePicker
        mode="range"
        value={rangeValue}
        onChange={handleRangeChange}
        placeholder={t('filter.selectTime')}
        className="h-auto! w-auto! min-w-0 rounded! border-border bg-input-bg px-2.5! py-1! text-sm! text-text-secondary"
      />

      <Button
        variant="secondary"
        size="xs"
        onClick={handleReset}
        className="bg-[#000f32] text-white hover:bg-[#000f32]/90 dark:bg-[#2e2e2e] dark:hover:bg-[#2e2e2e]/80"
      >
        <Image
          src="/images/icons/refresh.svg"
          alt=""
          width={20}
          height={20}
        />
        <span>{t('filter.reset')}</span>
      </Button>

      <Button
        variant="primary"
        size="xs"
        onClick={onSearch}
      >
        <Image
          src="/images/icons/search.svg"
          alt=""
          width={20}
          height={20}
        />
        <span>{t('filter.search')}</span>
      </Button>
    </div>
  );
}
