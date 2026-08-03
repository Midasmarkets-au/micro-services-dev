/** 项目 locale -> 后端 language key（按优先级） */
export const NOTICE_LOCALE_MAPPING: Record<string, string[]> = {
  en: ['en-us', 'en'],
  zh: ['zh-cn', 'zh'],
  'zh-tw': ['zh-tw', 'zh-hk'],
  vi: ['vi', 'vi-vn'],
  th: ['th', 'th-th'],
  jp: ['ja', 'ja-jp', 'jp', 'jp-jp'],
  id: ['id', 'id-id'],
  ms: ['ms', 'ms-my'],
  ko: ['ko', 'ko-kr'],
  km: ['km', 'km-kh'],
  es: ['es', 'es-es'],
};

export interface NoticeLocalizedFields {
  title?: string;
  content?: string;
}

export function stripNoticeHtml(html: string): string {
  return html.replace(/<[^>]*>/g, '');
}

export function isNoticeTextComplete(title: string, content: string): boolean {
  return Boolean(title.trim() && stripNoticeHtml(content).trim());
}

/**
 * 解析公告多语言内容：
 * 当前语言 → en-us → 任意可用语言；
 * title / content（去 HTML）任一为空则尝试下一候选，全部不完整则返回 null。
 */
export function resolveNoticeLocalizedContent(
  contents: Record<string, NoticeLocalizedFields> | null | undefined,
  locale: string,
  fallbackTitle = ''
): { title: string; content: string } | null {
  if (!contents) return null;

  const preferred = NOTICE_LOCALE_MAPPING[locale] || [locale];
  const orderedKeys: string[] = [];
  const seen = new Set<string>();

  const pushKey = (lang: string) => {
    if (contents[lang] && !seen.has(lang)) {
      orderedKeys.push(lang);
      seen.add(lang);
    }
  };

  for (const lang of preferred) pushKey(lang);
  pushKey('en-us');
  for (const lang of Object.keys(contents)) pushKey(lang);

  for (const lang of orderedKeys) {
    const entry = contents[lang];
    const title = (entry?.title || fallbackTitle || '').trim();
    const content = entry?.content || '';
    if (isNoticeTextComplete(title, content)) {
      return {
        title: (entry?.title || fallbackTitle || '').trim(),
        content,
      };
    }
  }

  return null;
}
