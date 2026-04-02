'use client';

import { useMemo } from 'react';
import moment from 'moment';
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
  className?: string;
}

export function TimeShow({ dateIsoString, format, type, className }: TimeShowProps) {
  const utcEnabled = useUserStore((s) => s.siteConfig?.utcEnabled ?? false);

  const date = useMemo(() => {
    if (utcEnabled) {
      return moment.parseZone(dateIsoString);
    } else {
      return moment(dateIsoString);
    }
  }, [dateIsoString, utcEnabled]);

  const getExactDateAndTime = () => {
    const d = moment.parseZone(dateIsoString);
    const formattedDate = d.format('YYYY-MM-DD HH:mm:ss');

    if (d.year() === moment().year()) {
      return d.format('MM-DD HH:mm:ss');
    }

    return formattedDate;
  };

  const getExactDate = () => {
    const d = moment.parseZone(dateIsoString);
    const formattedDate = d.format('YYYY-MM-DD');
    return formattedDate;
  };

  const getExactTime = () => {
    const d = moment.parseZone(dateIsoString);
    const formattedDate = d.format('HH:mm:ss');
    return formattedDate;
  };

  const getDateAndTimeFromISOString = () => {
    if (date.year() === 1970 || date.year() === 1969) {
      return '-';
    }

    if (format) {
      return date.format(format);
    }
    if (date.year() === moment().year()) {
      return date.format('MM-DD HH:mm:ss');
    }
    return date.format('YYYY-MM-DD HH:mm:ss');
  };

  if (type === 'inFields') {
    return (
      <div className={className}>
        <h3 style={{ color: '#666666', fontWeight: 400, fontSize: '14px' }}>
          {date.format('HH:mm:ss')}
        </h3>
        <div style={{ fontSize: '12px', color: 'rgba(113, 113, 113, 1)' }}>
          {date.format('YYYY-M-D')}
        </div>
      </div>
    );
  }

  if (type === 'withoutSec') {
    return <div className={className}>{date.format('YYYY-M-D HH:mm')}</div>;
  }

  if (type === 'GMTinFields') {
    return (
      <div className={className}>
        <div style={{ color: '#4d4d4d', fontWeight: 400, fontSize: '14px' }}>
          {getExactTime()}
        </div>
        <div style={{ fontSize: '12px', color: 'rgba(113, 113, 113, 1)' }}>
          {getExactDate()}
        </div>
      </div>
    );
  }

  if (type === 'GMToneLiner') {
    return (
      <div className={className}>
        <span>{getExactDateAndTime()}</span>
      </div>
    );
  }

  if (type === 'oneLiner') {
    return <div className={className}>{date.format('YYYY-M-D HH:mm:ss')}</div>;
  }

  if (type === 'customCSS') {
    return (
      <div className={className}>
        <span>{date.format('YYYY-M-D HH:mm:ss')}</span>
      </div>
    );
  }

  if (type === 'customCSSv2') {
    return (
      <div className={className}>
        <span>{date.format('YYYY-M-D')}</span>
      </div>
    );
  }

  if (type === 'eventShop') {
    return <span className={className}>{date.format('YYYY-M-D')}</span>;
  }

  if (type === 'exactTime') return <span className={className}>{getExactDateAndTime()} GMT</span>;
  if (type === 'reportTime') return <span className={className}>{getExactDateAndTime()}</span>;
  if (type === 'reportDate') return <span className={className}>{getExactDate()}</span>;

  return <span className={className}>{getDateAndTimeFromISOString()}</span>;
}
