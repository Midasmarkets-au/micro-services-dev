'use client';

import { useState, useEffect } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import Link from 'next/link';
import Image from 'next/image';
import { useRouteScope } from '@/hooks/useRouteScope';
import { useServerAction } from '@/hooks/useServerAction';
import { fetchAction } from '@/lib/api/browser-client';
import { formatLocalizedDate } from '@/lib/formatLocalizedDate';
import {
  resolveNoticeLocalizedContent,
  stripNoticeHtml,
} from '@/lib/noticeLocalization';
import { NotificationsSkeleton } from '@/components/ui';

// 后端返回的通知内容结构
interface NoticeContent {
  id: number;
  language: string;
  title: string;
  subtitle: string;
  content: string;
  author: string;
  updatedOn: string;
}

// 后端返回的通知项结构
interface NoticeItem {
  id: number;
  type: number;
  title: string;
  effectiveFrom: string;
  effectiveTo: string;
  contents: Record<string, NoticeContent>;
  updatedOn: string;
  createdOn: string;
}

export function DashboardNotifications() {
  const t = useTranslations('dashboard');
  const locale = useLocale();
  const { begin } = useRouteScope('/dashboard');
  const { execute } = useServerAction({ showErrorToast: false });
  const [selectedNotification, setSelectedNotification] = useState<number | null>(null);
  const [notifications, setNotifications] = useState<NoticeItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const resolveNotice = (notice: NoticeItem) =>
    resolveNoticeLocalizedContent(notice.contents, locale, notice.title);

  const getNoticeTitle = (notice: NoticeItem): string =>
    resolveNotice(notice)?.title || '';

  const getNoticeContent = (notice: NoticeItem): string =>
    stripNoticeHtml(resolveNotice(notice)?.content || '');

  const isNoticeComplete = (notice: NoticeItem): boolean =>
    resolveNotice(notice) !== null;

  // begin/execute 都是稳定引用，effect 正常只跑一次；StrictMode 双跑由 begin()
  // 的 token 机制去重，不需要额外 ref 守卫。
  useEffect(() => {
    const { isActive } = begin();
    (async () => {
      const result = await execute(async () => fetchAction<NoticeItem[]>('getNotifications', 8));
      if (!isActive()) return;

      const items: NoticeItem[] = Array.isArray(result.data)
        ? (result.data as NoticeItem[]).slice(0, 5)
        : [];

      if (result.success && items.length > 0) {
        setNotifications(items);
        const firstVisible = items.find(isNoticeComplete);
        setSelectedNotification(firstVisible?.id ?? null);
      }

      setIsLoading(false);
    })();
  }, [begin, execute]);

  // title / content 任一为空则不展示（含语言回退后仍不完整）
  const visibleNotifications = notifications.filter(isNoticeComplete);

  // 加载状态 - 复用 NotificationsSkeleton 保持一致
  if (isLoading) {
    return <NotificationsSkeleton />;
  }

  // 无数据状态
  if (visibleNotifications.length === 0) {
    return (
      <aside className="sidebar-responsive">
        <div className="rounded bg-surface p-5">
          <div className="mb-5 flex items-center justify-between">
            <h3 className="text-lg font-semibold text-text-primary">
              {t('announcements')}
            </h3>
          </div>
          <div className="py-8 text-center text-text-secondary">
            {t('noNotifications')}
          </div>
        </div>
      </aside>
    );
  }

  return (
    <aside className="sidebar-responsive">
      <div className="rounded bg-surface p-5">
        {/* 标题 */}
        <div className="mb-5 flex items-center justify-between">
          <h3 className="text-lg font-semibold text-text-primary">
            {t('announcements')}
          </h3>
          <Link
            href="/supports/notices"
            className="flex items-center gap-1 text-sm text-text-secondary hover:underline"
          >
            {t('viewMore')}
            <Image
              src="/images/icons/chevron-line-right.svg"
              alt=""
              width={16}
              height={16}
            />
          </Link>
        </div>

        {/* 通知列表 */}
        <div className="flex flex-col gap-3">
          {visibleNotifications.map((notification) => {
            const { time } = formatLocalizedDate(notification.createdOn, locale);
            const isSelected = selectedNotification === notification.id;
            const title = getNoticeTitle(notification);
            const content = getNoticeContent(notification);
            return (
              <button
                key={notification.id}
                onClick={() => setSelectedNotification(notification.id)}
                className={`flex flex-col gap-2 rounded p-4 text-left transition-colors ${
                  isSelected
                    ? 'bg-primary text-white'
                    : 'border border-border hover:bg-surface-secondary'
                }`}
              >
                {/* <div className="flex items-start justify-between">
                  <span
                    className={`text-sm font-medium ${
                      isSelected ? 'text-white' : 'text-text-primary'
                    }`}
                  >
                    {date}
                  </span>
                </div> */}
                <p
                  className={`text-sm ${
                    isSelected ? 'text-white/90' : 'text-text-secondary'
                  }`}
                >
                  {time}
                </p>
                <h4
                  className={`text-base font-medium ${
                    isSelected ? 'text-white' : 'text-text-primary'
                  }`}
                >
                  {title}
                </h4>
                <p
                  className={`line-clamp-2 text-sm leading-5 ${
                    isSelected ? 'text-white/80' : 'text-text-secondary'
                  }`}
                >
                  {content}
                </p>
              </button>
            );
          })}
        </div>
      </div>
    </aside>
  );
}
