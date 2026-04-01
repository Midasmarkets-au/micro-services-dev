'use client';

import { useMemo } from 'react';
import { useLocale } from 'next-intl';
import { ServiceTypes } from '@/types/accounts';

interface WebTraderPageProps {
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

export default function WebTraderPage({ params }: WebTraderPageProps) {
  const locale = useLocale();
  const webTraderLang = mapLocaleToWebTraderLang(locale);
  const accountNumber = params.params?.[0];
  const serviceIdRaw = params.params?.[1];
  const serviceId = serviceIdRaw ? Number(serviceIdRaw) : undefined;

  const webTraderUrl = useMemo(() => {
    const base = 'https://metatraderweb.app/trade';
    const search = new URLSearchParams();
    search.set('servers', 'MMCo-Main');
    search.set('trade_server', 'MMCo-Main');
    search.set('lang', webTraderLang);

    if (
      accountNumber &&
      (serviceId === ServiceTypes.MetaTrader4Co || serviceId === ServiceTypes.MetaTrader4)
    ) {
      search.set('login', accountNumber);
    }

    return `${base}?${search.toString()}`;
  }, [accountNumber, serviceId, webTraderLang]);

  return (
    <div className="fixed inset-0 left-0 z-50 h-screen w-screen border-[3px] border-gray-500 bg-background">
      <iframe title="webTrader" src={webTraderUrl} className="h-full w-full" />
    </div>
  );
}
