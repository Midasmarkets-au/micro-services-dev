'use client';

import { useMemo } from 'react';
import { useUserStore } from '@/stores';

type TimeShowType =
  | 'inFields'
  | 'withoutSec'
  | 'GMTinFields'
  | 'GMToneLiner'
  | 'oneLiner'
  | 'customCSS'
  | 'customCSSv2'
  | 'eventShop'
  | 'exactTime'
  | 'reportTime'
  | 'reportDate';

export interface TimeShowProps {
  dateIsoString?: string | null;
  format?: string;
  type?: TimeShowType | string;
}

interface DateParts {
  year: number;
  month: number;
  day: number;
  hour: number;
  minute: number;
  second: number;
}

function pad2(n: number): string {
  return String(n).padStart(2, '0');
}

function parseIsoParts(value?: string | null): DateParts | null {
  if (!value) return null;
  const match = value.match(/^(\d{4})-(\d{1,2})-(\d{1,2})[T\s](\d{1,2}):(\d{1,2})(?::(\d{1,2}))?/);
  if (!match) return null;
  return {
    year: Number(match[1]),
    month: Number(match[2]),
    day: Number(match[3]),
    hour: Number(match[4]),
    minute: Number(match[5]),
    second: Number(match[6] ?? '0'),
  };
}

function toDate(value?: string | null): Date | null {
  if (!value) return null;
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? null : d;
}

function partsFromDate(date: Date, useUTC: boolean): DateParts {
  if (useUTC) {
    return {
      year: date.getUTCFullYear(),
      month: date.getUTCMonth() + 1,
      day: date.getUTCDate(),
      hour: date.getUTCHours(),
      minute: date.getUTCMinutes(),
      second: date.getUTCSeconds(),
    };
  }
  return {
    year: date.getFullYear(),
    month: date.getMonth() + 1,
    day: date.getDate(),
    hour: date.getHours(),
    minute: date.getMinutes(),
    second: date.getSeconds(),
  };
}

function formatByTemplate(parts: DateParts, template: string): string {
  const hour12 = parts.hour % 12 === 0 ? 12 : parts.hour % 12;
  const ampm = parts.hour >= 12 ? 'PM' : 'AM';

  const tokenMap: Record<string, string> = {
    YYYY: String(parts.year),
    MM: pad2(parts.month),
    M: String(parts.month),
    DD: pad2(parts.day),
    D: String(parts.day),
    HH: pad2(parts.hour),
    hh: pad2(hour12),
    mm: pad2(parts.minute),
    ss: pad2(parts.second),
    A: ampm,
  };

  return template.replace(/YYYY|MM|M|DD|D|HH|hh|mm|ss|A/g, (token) => tokenMap[token] ?? token);
}

export function TimeShow({ dateIsoString, format, type }: TimeShowProps) {
  const utcEnabled = useUserStore((s) => s.siteConfig?.utcEnabled ?? false);

  const displayParts = useMemo(() => {
    const parsed = parseIsoParts(dateIsoString);
    if (utcEnabled && parsed) return parsed;

    const d = toDate(dateIsoString);
    if (!d) return null;
    return partsFromDate(d, utcEnabled);
  }, [dateIsoString, utcEnabled]);

  const exactParts = useMemo(() => {
    const parsed = parseIsoParts(dateIsoString);
    if (parsed) return parsed;

    const d = toDate(dateIsoString);
    if (!d) return null;
    return partsFromDate(d, true);
  }, [dateIsoString]);

  const currentYear = new Date().getFullYear();

  const getExactDateAndTime = () => {
    if (!exactParts) return '-';
    if (exactParts.year === currentYear) return formatByTemplate(exactParts, 'MM-DD HH:mm:ss');
    return formatByTemplate(exactParts, 'YYYY-MM-DD HH:mm:ss');
  };

  const getExactDate = () => {
    if (!exactParts) return '-';
    return formatByTemplate(exactParts, 'YYYY-MM-DD');
  };

  const getExactTime = () => {
    if (!exactParts) return '-';
    return formatByTemplate(exactParts, 'HH:mm:ss');
  };

  const getDateAndTimeFromISOString = () => {
    if (!displayParts) return '-';
    if (displayParts.year === 1970 || displayParts.year === 1969) return '-';
    if (format) return formatByTemplate(displayParts, format);
    if (displayParts.year === currentYear) return formatByTemplate(displayParts, 'MM-DD HH:mm:ss');
    return formatByTemplate(displayParts, 'YYYY-MM-DD HH:mm:ss');
  };

  if (type === 'inFields') {
    return (
      <div>
        <h3 className="text-sm font-normal text-[#666666]">
          {displayParts ? formatByTemplate(displayParts, 'HH:mm:ss') : '-'}
        </h3>
        <div className="text-xs text-[#717171]">
          {displayParts ? formatByTemplate(displayParts, 'YYYY-M-D') : '-'}
        </div>
      </div>
    );
  }

  if (type === 'withoutSec') {
    return <div>{displayParts ? formatByTemplate(displayParts, 'YYYY-M-D HH:mm') : '-'}</div>;
  }

  if (type === 'GMTinFields') {
    return (
      <div>
        <div className="text-sm font-normal text-[#4d4d4d]">{getExactTime()}</div>
        <div className="text-xs text-[#717171]">{getExactDate()}</div>
      </div>
    );
  }

  if (type === 'GMToneLiner') return <span>{getExactDateAndTime()}</span>;
  if (type === 'oneLiner') return <div>{displayParts ? formatByTemplate(displayParts, 'YYYY-M-D HH:mm:ss') : '-'}</div>;
  if (type === 'customCSS') return <span>{displayParts ? formatByTemplate(displayParts, 'YYYY-M-D HH:mm:ss') : '-'}</span>;
  if (type === 'customCSSv2') return <span>{displayParts ? formatByTemplate(displayParts, 'YYYY-M-D') : '-'}</span>;
  if (type === 'eventShop') return <span>{displayParts ? formatByTemplate(displayParts, 'YYYY-M-D') : '-'}</span>;
  if (type === 'exactTime') return <span>{getExactDateAndTime()} GMT</span>;
  if (type === 'reportTime') return <span>{getExactDateAndTime()}</span>;
  if (type === 'reportDate') return <span>{getExactDate()}</span>;
  return <span>{getDateAndTimeFromISOString()}</span>;
}
