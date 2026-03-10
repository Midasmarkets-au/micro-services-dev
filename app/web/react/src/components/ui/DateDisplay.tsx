'use client';

import { useMemo } from 'react';

export type DateTimezone = 'local' | 'utc';
export type DateFormat = 'date' | 'datetime';

export interface DateDisplayProps {
  value?: string | Date | null;
  format?: DateFormat;
  timezone?: DateTimezone;
  fallback?: string;
  className?: string;
}

function pad(n: number): string {
  return String(n).padStart(2, '0');
}

export function formatDateValue(
  value: string | Date,
  format: DateFormat = 'datetime',
  timezone: DateTimezone = 'local'
): string {
  const date = typeof value === 'string' ? new Date(value) : value;
  if (isNaN(date.getTime())) return '-';

  const isUTC = timezone === 'utc';
  const y = isUTC ? date.getUTCFullYear() : date.getFullYear();
  const m = isUTC ? date.getUTCMonth() + 1 : date.getMonth() + 1;
  const d = isUTC ? date.getUTCDate() : date.getDate();

  if (format === 'date') {
    return `${y}-${pad(m)}-${pad(d)}`;
  }

  const h = isUTC ? date.getUTCHours() : date.getHours();
  const min = isUTC ? date.getUTCMinutes() : date.getMinutes();
  const s = isUTC ? date.getUTCSeconds() : date.getSeconds();

  return `${y}-${pad(m)}-${pad(d)} ${pad(h)}:${pad(min)}:${pad(s)}`;
}

export function DateDisplay({
  value,
  format = 'datetime',
  timezone = 'local',
  fallback = '-',
  className,
}: DateDisplayProps) {
  const formatted = useMemo(() => {
    if (!value) return fallback;
    return formatDateValue(value, format, timezone);
  }, [value, format, timezone, fallback]);

  return <span className={className}>{formatted}</span>;
}
