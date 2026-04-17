export const locales = ['en', 'zh', 'zh-tw', 'vi', 'th', 'jp', 'id', 'ms', 'ko', 'km', 'es'] as const;
export const defaultLocale = 'en' as const;

export type Locale = (typeof locales)[number];

// 语言代码映射：LanguageTypes code (user.language) -> i18n locale
export const localeMap: Record<string, Locale> = {
  'en-us': 'en',
  'zh-cn': 'zh',
  'zh-tw': 'zh-tw',
  'vi-vn': 'vi',
  'th-th': 'th',
  'jp-jp': 'jp',
  'id-id': 'id',
  'ms-my': 'ms',
  'ko-kr': 'ko',
  'km-kh': 'km',
  'es-es': 'es',
};

// 反向映射：i18n locale -> LanguageTypes code
export const reverseLocaleMap: Record<string, string> = Object.entries(localeMap).reduce(
  (acc, [key, value]) => ({ ...acc, [value]: key }),
  {}
);
