'use client';

import { useMemo } from 'react';
import { useLocale } from 'next-intl';

interface WebTrader5PageProps {
  params: {
    params?: string[];
  };
}

function mapLocaleToWebTraderLang(locale: string): string {
  switch (locale) {
    case 'en':
      return 'en';
    case 'zh':
      return 'zh';
    case 'zh-tw':
      return 'zt';
    case 'vi':
      return 'vi';
    case 'th':
      return 'th';
    case 'jp':
      return 'ja';
    case 'ms':
      return 'ms';
    default:
      return 'en';
  }
}

export default function WebTrader5Page({ params }: WebTrader5PageProps) {
  const locale = useLocale();
  const webTraderLang = mapLocaleToWebTraderLang(locale);
  const accountNumber = params.params?.[0];

  const webTraderUrl = useMemo(() => {
    const base = 'https://mt5.midasmkts.com/terminal';
    const search = new URLSearchParams();
    search.set('lang', webTraderLang);
    search.set('server', 'MMCo-Main');
    if (accountNumber) {
      search.set('login', accountNumber);
    }
    return `${base}?${search.toString()}`;
  }, [accountNumber, webTraderLang]);

  return (
    <div className="fixed inset-0 left-0 z-50 h-screen w-screen border-[3px] border-gray-500 bg-background">
      <iframe title="webTrader5" src={webTraderUrl} className="h-full w-full" />
    </div>
  );
}
