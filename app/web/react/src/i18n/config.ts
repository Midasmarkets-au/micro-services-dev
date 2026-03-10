export const locales = ['en', 'zh', 'zh-tw', 'vi', 'th', 'jp', 'id', 'ms', 'ko', 'km', 'es'] as const;
export const defaultLocale = 'en' as const;

export type Locale = (typeof locales)[number];
