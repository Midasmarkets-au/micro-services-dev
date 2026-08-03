// 前端 locale -> Intl 标准 locale
const localeToIntlLocale: Record<string, string> = {
  en: 'en-US',
  zh: 'zh-CN',
  'zh-tw': 'zh-TW',
  es: 'es-ES',
  id: 'id-ID',
  jp: 'ja-JP',
  ko: 'ko-KR',
  km: 'km-KH',
  ms: 'ms-MY',
  th: 'th-TH',
  vi: 'vi-VN',
};

/** 根据当前语言格式化星期、月份和年份 */
export function formatLocalizedDate(
  dateString: string,
  locale: string
): { date: string; time: string } {
  const date = new Date(dateString);
  const intlLocale = localeToIntlLocale[locale] || 'en-US';

  return {
    date: new Intl.DateTimeFormat(intlLocale, {
      weekday: 'long',
    }).format(date),
    time: new Intl.DateTimeFormat(intlLocale, {
      month: 'long',
      year: 'numeric',
    }).format(date),
  };
}
