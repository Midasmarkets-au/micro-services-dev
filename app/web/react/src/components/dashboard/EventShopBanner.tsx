'use client';

import { useState, useEffect, useRef } from 'react';
import Image from 'next/image';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useServerAction } from '@/hooks/useServerAction';
import { getEventDetail } from '@/actions';
import { TimeShow } from '@/components/TimeShow';
import {
  MiniLoading,
  Icon,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  Button,
} from '@/components/ui';
import type { EventDetail } from '@/types/eventshop';

/**
 * EventShopBanner - 替换 dashboard 顶部 banner
 * 保留原始背景图片，文字/数据采用 EventShopBanner 逻辑
 * 对应 Vue: EventShopBanner.vue + EventDetailsCard.vue
 */
export function EventShopBanner() {
  const t = useTranslations('dashboard');
  const tCommon = useTranslations('common');
  const router = useRouter();
  const { isDark, mounted } = useTheme();
  const { execute } = useServerAction({ showErrorToast: true });

  const [eventData, setEventData] = useState<EventDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const loadedRef = useRef(false);

  const bannerImage = isDark
    ? '/images/dashboard/banner-night.svg'
    : '/images/dashboard/banner-day.svg';

  useEffect(() => {
    if (loadedRef.current) return;
    loadedRef.current = true;

    (async () => {
      try {
        const result = await execute(getEventDetail, 'EventShop');
        if (result.success && result.data) {
          setEventData(result.data);
        }
      } finally {
        setIsLoading(false);
      }
    })();
  }, [execute]);

  // Vue 中使用 data.name 作为活动名称
  const eventName = eventData
    ? ((eventData as Record<string, unknown>).name as string) || eventData.title
    : '';

  return (
    <>
      <div className="dashboard-banner relative h-42 w-full overflow-hidden rounded">
        {/* 右侧装饰图片 - 根据主题切换 */}
        {mounted && (
          <div className="absolute right-0 top-1/2 h-[20.94rem] w-123 -translate-y-1/2">
            <Image
              src={bannerImage}
              alt="decoration"
              fill
              className="object-contain object-right"
            />
          </div>
        )}

        {/* 文字内容 - 采用 EventShopBanner 逻辑 */}
        <div className="absolute left-5 md:left-17.5 top-4 md:top-5.5 z-10 flex flex-col">
          {isLoading ? (
            <MiniLoading className="text-white" />
          ) : eventData ? (
            <>
              {/* 活动名称 */}
              <h3 className="text-xl md:text-2xl font-bold text-white leading-tight">
                {eventName}
              </h3>

              {/* 开始日期 / 结束日期 */}
              <div className="mt-4 flex flex-row gap-5 text-xs md:text-sm">
                <span className="flex items-center gap-1.5">
                  <span className="text-white/80">{t('startDate')}</span>
                  <TimeShow dateIsoString={eventData.startOn} className="text-white font-medium" type="eventShop" />
                </span>
                <span className="flex items-center gap-1.5">
                  <span className="text-white/80">{t('endDate')}</span>
          
                  <TimeShow dateIsoString={eventData.endOn} className="text-white font-medium" type="eventShop" />
                </span>
              </div>
              {/* 操作按钮：查看详情 + 积分商城 */}
              <div className="mt-4 md:mt-5 flex flex-row items-center gap-3">
                <button
                  className="rounded cursor-pointer border border-white/50 bg-white/10 px-4 md:px-5 py-1.5 text-xs font-semibold text-white backdrop-blur-sm transition-colors hover:bg-white/20"
                  onClick={() => setShowDetailModal(true)}
                >
                  {t('viewDetails')}
                </button>
                <button
                  className="rounded cursor-pointer bg-white px-4 md:px-5 py-1.5 text-xs font-semibold text-black transition-colors hover:bg-white/90"
                  onClick={() => router.push('/eventshop')}
                >
                  {t('eventShop')}
                </button>
              </div>
            </>
          ) : (
            /* 接口失败时回退到原始 banner 内容 */
            <>
              <div className="text-responsive-2xl font-semibold text-white">
                <p>MIDAS MARKET</p>
                <p className="flex items-center">
                  <span>{t('bannerTitle')}</span>
                  <span className="banner-highlight-text ml-1">
                    {t('bannerHighlight')}
                  </span>
                </p>
              </div>
              <button className="mt-5 cursor-pointer flex h-7.5 w-26.75 items-center justify-center gap-1 rounded border border-white bg-primary text-xs font-semibold text-white backdrop-blur-sm hover:bg-primary-hover">
                <span>{t('depositNow')}</span>
                <Icon name="add-plain" size={12} />
              </button>
            </>
          )}
        </div>
      </div>

      {/* EventDetailsCard 模态框 - 对应 Vue 的 EventDetailsCard.vue */}
      {eventData && (
        <Dialog open={showDetailModal} onOpenChange={setShowDetailModal}>
          <DialogContent className="max-w-[761px]">
            {/* 头部：标题 + 更新时间 */}
            <DialogHeader className="border-b border-border pb-4">
              <DialogTitle className="text-xl font-semibold">
                {eventName}
              </DialogTitle>
              {!!(eventData as Record<string, unknown>).updatedOn && (
                <div className="flex items-center gap-1 pt-2 text-sm text-text-secondary">
                  <span>{t('eventNotice.postedOn')}:</span>
                  {/* <DateDisplay
                    value={
                      (eventData as Record<string, unknown>).updatedOn as string
                    }
                    format="datetime"
                  /> */}
                  <TimeShow dateIsoString={(eventData as Record<string, unknown>).updatedOn as string} className="text-sm font-semibold text-text-primary" type="eventShop" />
                </div>
              )}
            </DialogHeader>

            {/* 日期区域：开始日期 + 活动周期 */}
            <div className="mt-4 flex gap-8">
              <div className="flex flex-col gap-1">
                <span className="text-sm font-semibold text-text-secondary">
                  {t('startDate')}
                </span>
                <TimeShow dateIsoString={eventData.startOn} className="text-sm font-semibold text-text-primary" type="eventShop" />
              </div>
              <div className="flex flex-col gap-1">
                <span className="text-sm font-semibold text-text-secondary">
                  {t('activityCycle')}
                </span>
                <span className="text-sm font-semibold text-text-primary">
                  {t('longTerm')}
                </span>
              </div>
            </div>

            {/* 活动描述 HTML */}
            {eventData.description && (
              <div
                className="notice-content mt-4 max-h-[50vh] overflow-auto text-sm text-text-primary"
                dangerouslySetInnerHTML={{ __html: eventData.description }}
              />
            )}
            <DialogFooter>
              <Button variant="outline" size="sm" className="w-[120px]" onClick={() => setShowDetailModal(false)}>
                {tCommon('close')}
              </Button>
            </DialogFooter>
            
          </DialogContent>
        </Dialog>
      )}
    </>
  );
}
