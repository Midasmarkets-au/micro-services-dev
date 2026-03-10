'use client';

import { useEffect, useState, useRef } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useTranslations, useLocale } from 'next-intl';
import DOMPurify from 'dompurify';
import { useServerAction } from '@/hooks/useServerAction';
import { Skeleton } from '@/components/ui';
import { getNotices } from '@/actions';

// 单个语言版本的公告内容
interface NoticeContent {
  id: number;
  language: string;
  title: string;
  subtitle?: string;
  content: string;
  author?: string;
  updatedOn: string;
}

// API 返回的公告结构
interface ApiNotice {
  id: number;
  type: number;
  title: string;
  effectiveFrom: string;
  effectiveTo: string;
  contents: {
    [language: string]: NoticeContent;
  };
  updatedOn: string;
  createdOn: string;
}

// 处理后的公告详情类型
interface NoticeDetail {
  id: number;
  title: string;
  content: string;
  createdOn: string;
  updatedOn: string;
}

// 语言代码映射：项目语言 -> 后端语言
const localeMapping: Record<string, string[]> = {
  'en': ['en-us', 'en'],
  'zh': ['zh-cn', 'zh'],
  'zh-tw': ['zh-tw', 'zh-hk'],
  'vi': ['vi', 'vi-vn'],
  'th': ['th', 'th-th'],
  'jp': ['ja', 'ja-jp', 'jp'],
  'id': ['id', 'id-id'],
  'ms': ['ms', 'ms-my'],
  'ko': ['ko', 'ko-kr'],
  'km': ['km', 'km-kh'],
  'es': ['es', 'es-es'],
};

// 从多语言公告中提取当前语言内容
function extractLocalizedContent(
  notice: ApiNotice,
  locale: string
): NoticeDetail | null {
  const contents = notice.contents;
  if (!contents) return null;
  
  const possibleLocales = localeMapping[locale] || [locale];
  
  for (const lang of possibleLocales) {
    if (contents[lang]) {
      const content = contents[lang];
      return {
        id: notice.id,
        title: content.title,
        content: content.content,
        createdOn: notice.createdOn,
        updatedOn: notice.updatedOn,
      };
    }
  }
  
  // 回退到 en-us
  if (contents['en-us']) {
    const content = contents['en-us'];
    return {
      id: notice.id,
      title: content.title,
      content: content.content,
      createdOn: notice.createdOn,
      updatedOn: notice.updatedOn,
    };
  }
  
  // 回退到任意可用语言
  const availableLanguages = Object.keys(contents);
  if (availableLanguages.length > 0) {
    const content = contents[availableLanguages[0]];
    return {
      id: notice.id,
      title: content.title,
      content: content.content,
      createdOn: notice.createdOn,
      updatedOn: notice.updatedOn,
    };
  }
  
  return null;
}

// 格式化日期
function formatDate(dateString: string, locale: string) {
  const date = new Date(dateString);
  const dayOfWeek = date.toLocaleDateString(locale === 'zh' ? 'zh-CN' : 'en-US', { weekday: 'long' });
  const day = date.getDate();
  const month = date.toLocaleDateString(locale === 'zh' ? 'zh-CN' : 'en-US', { month: 'short' });
  const year = date.getFullYear();
  return `${dayOfWeek}，${day} ${month} ${year}`;
}

// 骨架屏组件
function DetailSkeleton() {
  return (
    <div className="flex flex-col gap-5">
      {/* 面包屑 */}
      <div className="flex items-center gap-1">
        <Skeleton className="h-5 w-10" />
        <Skeleton className="h-5 w-5" />
        <Skeleton className="h-5 w-16" />
      </div>
      {/* 分割线 */}
      <div className="h-px bg-border" />
      {/* 标题区域 */}
      <div className="flex flex-col gap-2 w-[200px]">
        <Skeleton className="h-6 w-48" />
        <Skeleton className="h-4 w-32" />
      </div>
      {/* 内容区域 */}
      <div className="flex flex-col gap-5">
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-32 w-full rounded-lg" />
        <Skeleton className="h-32 w-full rounded-lg" />
        <Skeleton className="h-4 w-full" />
        <div className="h-px bg-border" />
        <Skeleton className="h-16 w-full" />
      </div>
    </div>
  );
}

export default function NoticeDetailPage() {
  const params = useParams();
  const id = params.id as string;
  const t = useTranslations('supports');
  const locale = useLocale();
  const { execute } = useServerAction({ showErrorToast: false });
  const [notice, setNotice] = useState<NoticeDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);
  
  const isInitialized = useRef(false);

  useEffect(() => {
    if (isInitialized.current) return;
    isInitialized.current = true;
    
    const fetchNotice = async () => {
      setIsLoading(true);
      try {
        const result = await execute(getNotices);
        if (result.success && result.data) {
          const rawData = result.data as unknown as ApiNotice[];
          // 找到对应 ID 的公告
          const targetNotice = rawData.find((n) => n.id === parseInt(id, 10));
          if (targetNotice) {
            const localizedNotice = extractLocalizedContent(targetNotice, locale);
            if (localizedNotice) {
              setNotice(localizedNotice);
            } else {
              setNotFound(true);
            }
          } else {
            setNotFound(true);
          }
        } else {
          setNotFound(true);
        }
      } catch {
        setNotFound(true);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchNotice();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  if (isLoading) {
    return <DetailSkeleton />;
  }

  if (notFound || !notice) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4">
        <p className="text-text-secondary">{t('noticeNotFound')}</p>
        <Link 
          href="/supports/notices" 
          className="text-primary hover:underline"
        >
          {t('backToList')}
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-5">
      {/* 面包屑导航 */}
      <div className="flex items-center gap-1">
        <Link 
          href="/supports/notices" 
          className="text-sm text-text-secondary hover:text-primary transition-colors"
        >
          {t('tabs.announcements')}
        </Link>
        <svg 
          className="size-5 text-text-secondary" 
          fill="none" 
          viewBox="0 0 24 24" 
          stroke="currentColor" 
          strokeWidth={2}
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
        </svg>
        <span className="text-sm text-[#333] dark:text-white">
          {t('noticeDetail')}
        </span>
      </div>

      {/* 分割线 */}
      <div className="h-px bg-[#EEE] dark:bg-[#333]" />

      {/* 标题区域 */}
      <div className="flex flex-col gap-2">
        <h1 className="text-xl font-medium text-[#333] dark:text-white">
          {notice.title}
        </h1>
        <p className="text-xs text-[#666]">
          {formatDate(notice.createdOn, locale)}
        </p>
      </div>

      {/* 内容区域 */}
      <div className="flex flex-col gap-5">
        {/* HTML 内容 */}
        <div 
          className="
            prose prose-sm max-w-none dark:prose-invert
            [&_p]:text-sm [&_p]:leading-relaxed [&_p]:mb-4
            [&_p]:text-[#666] dark:[&_p]:text-text-secondary
            [&_a]:text-primary [&_a]:underline
            [&_strong]:font-medium [&_strong]:text-[#333] dark:[&_strong]:text-white
            [&_ul]:list-disc [&_ul]:pl-5
            [&_ol]:list-decimal [&_ol]:pl-5
            [&_li]:mb-2
            [&_blockquote]:border-l-4 [&_blockquote]:border-primary [&_blockquote]:pl-4 [&_blockquote]:italic
            [&_pre]:rounded-lg [&_pre]:p-4 [&_pre]:bg-[#f8f8f8] dark:[&_pre]:bg-[#111]
            [&_code]:text-sm [&_code]:bg-[#f8f8f8] dark:[&_code]:bg-[#111] [&_code]:px-1 [&_code]:py-0.5 [&_code]:rounded
            [&_table]:w-full [&_table]:border-collapse
            [&_th]:border [&_th]:p-2 [&_th]:border-[#EEE] [&_th]:bg-[#f8f8f8] dark:[&_th]:border-[#333] dark:[&_th]:bg-[#111]
            [&_td]:border [&_td]:p-2 [&_td]:border-[#EEE] dark:[&_td]:border-[#333]
            [&_img]:rounded-lg [&_img]:max-w-full
          "
          dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(notice.content) }}
        />

        {/* 底部分割线 */}
        <div className="h-px bg-[#EEE] dark:bg-[#333]" />

        {/* 免责声明 */}
        <div className="text-xs leading-relaxed text-[#666]">
          <p className="mb-2">
            <span className="text-primary">*</span>
            {t('disclaimer.spread')}
          </p>
          <p className="mb-2">
            <span className="text-primary">*</span>
            {t('disclaimer.scheduleChange')}
          </p>
          <p>
            <span className="text-primary">*</span>
            {t('disclaimer.translation')}
          </p>
        </div>
      </div>
    </div>
  );
}
