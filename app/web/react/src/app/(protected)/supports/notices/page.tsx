'use client';

import { useEffect, useState, useRef } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import Link from 'next/link';
import DOMPurify from 'dompurify';
import { useServerAction } from '@/hooks/useServerAction';
import { Skeleton } from '@/components/ui';
import { getNotices } from '@/actions';
import { useNoticesStore } from '@/stores';

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

// 处理后的公告项类型
interface NoticeItem {
  id: number;
  title: string;
  content: string;
  createdOn: string;
  updatedOn?: string;
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
): NoticeItem | null {
  const contents = notice.contents;
  if (!contents) return null;
  
  // 获取当前语言对应的后端语言代码列表
  const possibleLocales = localeMapping[locale] || [locale];
  
  // 尝试按优先级查找
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
function formatDate(dateString: string) {
  const date = new Date(dateString);
  const dayOfWeek = date.toLocaleDateString('en-US', { weekday: 'long' });
  const monthYear = date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  return { dayOfWeek, monthYear };
}

// 公告项组件
// 设计稿: 水平布局（左侧内容+右侧按钮），左边有竖线，卡片之间有分割线
function NoticeCard({ notice }: { notice: NoticeItem }) {
  const t = useTranslations('supports');
  const { dayOfWeek, monthYear } = formatDate(notice.createdOn);
  
  return (
    <div className="relative flex gap-5 items-center py-5 pl-5 pr-0 overflow-hidden border-b border-[#EEE] dark:border-[#333]">
      {/* 左侧竖线 - 设计稿: 宽2px, 高16px, top:20px, 日间#800020, 夜间#004eff */}
      <div className="absolute left-0 top-5 w-0.5 h-4 bg-primary" />
      
      {/* 内容区域 - flex-1, 垂直排列, gap:20px */}
      <div className="flex-1 flex flex-col gap-5 min-w-0">
        {/* 日期区域 - 设计稿: w:60px, gap:10px */}
        <div className="flex flex-col gap-2.5 w-[60px]">
          <span 
            className="text-sm font-bold leading-normal text-[#333] dark:text-white"
            style={{ fontFamily: 'DIN, sans-serif' }}
          >
            {dayOfWeek}
          </span>
          <span 
            className="text-xs text-text-secondary leading-normal"
            style={{ fontFamily: 'DIN, sans-serif' }}
          >
            {monthYear}
          </span>
        </div>
        
        {/* 标题 - 设计稿: 14px, 日间#333, 夜间白 */}
        <h3 className="text-sm font-medium leading-normal text-[#333] dark:text-white">
          {notice.title}
        </h3>
        
        {/* 描述 - 设计稿: 12px, #999, 最多2行，支持 HTML 内容（已净化防 XSS） */}
        <div 
          className="text-xs text-text-secondary leading-normal line-clamp-2 [&_p]:m-0 **:inline"
          dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(notice.content) }}
        />
      </div>
      
      {/* 查看详情按钮 - 设计稿: 宽100px, px:12px py:6px, 圆角4px, backdrop-blur */}
      <Link
        href={`/supports/notices/${notice.id}`}
        className="shrink-0 w-[100px] px-3 py-1.5 rounded text-sm text-center transition-colors backdrop-blur-sm bg-[#f8f8f8] text-[#333] hover:bg-[#efefef] dark:bg-[#2e2e2e] dark:text-white dark:hover:bg-[#3e3e3e]"
      >
        {t('viewDetails')}
      </Link>
    </div>
  );
}

// 骨架屏组件 - 与 NoticeCard 布局一致
function NoticesSkeleton() {
  return (
    <div className="flex flex-col">
      {[1, 2, 3, 4, 5].map((i) => (
        <div key={i} className="relative flex gap-5 items-center py-5 pl-5 pr-0 overflow-hidden border-b border-[#EEE] dark:border-[#333]">
          {/* 左侧竖线占位 */}
          <div className="absolute left-0 top-5 w-0.5 h-4 bg-gray-200 dark:bg-gray-700" />
          
          {/* 内容区域 - flex-1, 垂直排列, gap:20px */}
          <div className="flex-1 flex flex-col gap-5 min-w-0">
            {/* 日期区域 - w:60px, gap:10px */}
            <div className="flex flex-col gap-2.5 w-[60px]">
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-3 w-12" />
            </div>
            {/* 标题 */}
            <Skeleton className="h-4 w-48" />
            {/* 描述 */}
            <Skeleton className="h-3 w-full max-w-[600px]" />
          </div>
          
          {/* 查看详情按钮 */}
          <Skeleton className="shrink-0 h-8 w-[100px] rounded" />
        </div>
      ))}
    </div>
  );
}

export default function NoticesPage() {
  const t = useTranslations('supports');
  const locale = useLocale();
  const { execute } = useServerAction({ showErrorToast: false });
  const [notices, setNotices] = useState<NoticeItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const setLatestNotice = useNoticesStore((state) => state.setLatestNotice);
  
  // 防止重复请求
  const isInitialized = useRef(false);

  useEffect(() => {
    if (isInitialized.current) return;
    isInitialized.current = true;
    
    const fetchNotices = async () => {
      setIsLoading(true);
      try {
        // 使用 Server Action
        const result = await execute(getNotices);
        if (result.success && result.data) {
          // 处理多语言数据：从每个公告中提取当前语言的内容
          const rawData = result.data as unknown as ApiNotice[];
          const localizedNotices = rawData
            .map((notice) => extractLocalizedContent(notice, locale))
            .filter((item): item is NoticeItem => item !== null);
          setNotices(localizedNotices);
          // 保存第一条公告到 store，供 Banner 使用
          if (localizedNotices.length > 0) {
            setLatestNotice(localizedNotices[0]);
          }
        }
      } catch {
        // 错误已处理
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchNotices();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (isLoading) {
    return <NoticesSkeleton />;
  }

  if (notices.length === 0) {
    return (
      <div className="flex items-center justify-center py-20">
        <p className="text-text-secondary">{t('noAnnouncements')}</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col">
      {notices.map((notice, index) => (
        <NoticeCard key={`${notice.id}-${index}`} notice={notice} />
      ))}
    </div>
  );
}
