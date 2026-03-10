import { getRequestConfig } from 'next-intl/server';
import { cookies, headers } from 'next/headers';
import { defaultLocale, locales, type Locale } from './config';

export default getRequestConfig(async () => {
  // 优先从 cookie 获取语言设置
  const cookieStore = await cookies();
  const localeCookie = cookieStore.get('locale')?.value as Locale | undefined;
  
  // 其次从 Accept-Language header 获取
  const headersList = await headers();
  const acceptLanguage = headersList.get('accept-language');
  const browserLocale = acceptLanguage?.split(',')[0].split('-')[0] as Locale | undefined;
  
  // 确定最终使用的语言
  let locale: Locale = defaultLocale;
  
  if (localeCookie && locales.includes(localeCookie)) {
    locale = localeCookie;
  } else if (browserLocale && locales.includes(browserLocale)) {
    locale = browserLocale;
  }

  return {
    locale,
    messages: (await import(`@/messages/${locale}.json`)).default
  };
});

