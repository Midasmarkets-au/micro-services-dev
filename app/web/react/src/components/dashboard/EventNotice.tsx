'use client';

/* eslint-disable @next/next/no-img-element */
import {
  useState,
  useRef,
  useCallback,
  forwardRef,
  useImperativeHandle,
} from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useUserStore } from '@/stores/userStore';
import { getEventList, getEventDetail, markEventChecked } from '@/actions';
import {
  Button,
  Checkbox,
  MiniLoading,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui';
import type { EventDetail } from '@/types/eventshop';

export interface EventNoticeRef {
  showData: () => Promise<void>;
}

const EXCLUDED_KEYS = ['EventShop'];

/**
 * EventNotice - 活动公告弹窗
 * 对应 Vue: EventNotice.vue
 * 通过 ref.showData() 由父组件触发，拉取活动列表后弹窗展示
 * 支持翻页浏览、"不再显示"勾选
 */
export const EventNotice = forwardRef<EventNoticeRef>(
  function EventNotice(_, ref) {
    const t = useTranslations('dashboard.eventNotice');
    const { execute } = useServerAction({ showErrorToast: true });
    const user = useUserStore((s) => s.user);

    const [open, setOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [eventList, setEventList] = useState<EventDetail[]>([]);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [currentData, setCurrentData] = useState<EventDetail | null>(null);
    const [desktopImageUrl, setDesktopImageUrl] = useState('');
    const [mobileImageUrl, setMobileImageUrl] = useState('');
    const [isChecked, setIsChecked] = useState(false);
    const checkedListRef = useRef<string[]>([]);

    const dataLength = eventList.length;
    const isLastEvent = currentIndex === dataLength - 1;

    const fetchContent = useCallback(
      async (index: number, list: EventDetail[]) => {
        const item = list[index];
        if (!item) return;

        setIsLoading(true);
        setCurrentIndex(index);

        const itemKey = (item as Record<string, unknown>).key as string;
        setIsChecked(checkedListRef.current.includes(itemKey));

        try {
          const result = await execute(getEventDetail, itemKey);
          if (result.success && result.data) {
            setCurrentData(result.data);

            // 获取桌面端 / 移动端图片 URL
            const images = (result.data as Record<string, unknown>).images as
              | { desktop?: string; mobile?: string }
              | undefined;

            setDesktopImageUrl(
              images?.desktop ? `/api/media/${images.desktop}` : ''
            );
            setMobileImageUrl(
              images?.mobile ? `/api/media/${images.mobile}` : ''
            );
          }
        } catch (e) {
          console.error(e);
        } finally {
          setIsLoading(false);
        }
      },
      [execute]
    );

    // 父组件通过 ref 调用此方法触发弹窗
    const showData = useCallback(async () => {
      const criteria: Record<string, unknown> = {
        page: 1,
        size: 20,
        status: 1,
        sortField: 'createdOn',
        sortFlag: false,
      };

      // 仅拉取 id 大于 lastCheckedEventId 的活动
      if (user?.lastCheckedEventId && user.lastCheckedEventId > 0) {
        criteria.idLargerThan = user.lastCheckedEventId;
      }

      const result = await execute(getEventList, criteria);
      if (!result.success || !result.data) return;

      // 过滤掉 EventShop 类型的活动
      const filtered = (result.data.data || []).filter((item) => {
        const key = (item as Record<string, unknown>).key as string;
        return !EXCLUDED_KEYS.includes(key);
      });

      if (filtered.length > 0) {
        setEventList(filtered);
        setOpen(true);
        await fetchContent(0, filtered);
      }
    }, [execute, fetchContent, user?.lastCheckedEventId]);

    useImperativeHandle(ref, () => ({ showData }), [showData]);

    // "不再显示" - 对应 Vue 的 noShow 方法
    const handleNoShow = async () => {
      if (!currentData || isChecked) return;

      const key = (currentData as Record<string, unknown>).key as string;
      if (!key) return;

      setIsSubmitting(true);
      try {
        const result = await execute(markEventChecked, key);
        if (result.success) {
          checkedListRef.current.push(key);
          setIsChecked(true);
        }
      } catch (e) {
        console.error(e);
      } finally {
        setIsSubmitting(false);
      }
    };

    return (
      <Dialog
        open={open}
        onOpenChange={(val) => {
          // 只有在最后一条活动时才允许关闭
          if (!val && !isLastEvent) return;
          setOpen(val);
        }}
      >
        <DialogContent
          className="max-w-[761px] flex flex-col"
          onInteractOutside={(e) => {
            if (!isLastEvent) e.preventDefault();
          }}
          onEscapeKeyDown={(e) => {
            if (!isLastEvent) e.preventDefault();
          }}
        >
          {/* 头部 */}
          <DialogHeader className="border-b border-border pb-4">
            <div className="flex items-center justify-between">
              <DialogTitle className="text-xl">
                {currentData?.title || ''}
              </DialogTitle>
              {isLastEvent && (
                <button
                  className="flex size-6 shrink-0 items-center justify-center rounded text-text-secondary hover:bg-surface-hover"
                  onClick={() => setOpen(false)}
                >
                  ✕
                </button>
              )}
            </div>
          </DialogHeader>

          {/* 内容区域 */}
          {isLoading ? (
            <div className="flex min-h-[40vh] items-center justify-center">
              <MiniLoading />
            </div>
          ) : (
            <div className="max-h-[60vh] min-h-[40vh] overflow-auto">
              {/* 桌面端图片 */}
              {desktopImageUrl && (
                <div className="hidden md:block">
                  <img
                    src={desktopImageUrl}
                    alt="Event"
                    className="w-full rounded"
                  />
                </div>
              )}
              {/* 移动端图片（优先移动端图，回退到桌面图） */}
              {(mobileImageUrl || desktopImageUrl) && (
                <div className="block md:hidden">
                  <img
                    src={mobileImageUrl || desktopImageUrl}
                    alt="Event"
                    className="w-full rounded"
                  />
                </div>
              )}

              {/* 描述 HTML（key !== 'post' 时显示，对应 Vue 逻辑） */}
              {currentData?.description &&
                (currentData as Record<string, unknown>).key !== 'post' && (
                  <div
                    className="notice-content mt-6 text-sm text-text-primary"
                    dangerouslySetInnerHTML={{
                      __html: currentData.description,
                    }}
                  />
                )}
            </div>
          )}

          {/* 底部：不再显示 + 翻页控制 */}
          <div className="flex flex-col gap-3 border-t border-border pt-4">
            <div className="flex justify-end">
              <Checkbox
                checked={isChecked}
                disabled={isSubmitting || isChecked}
                onCheckedChange={() => handleNoShow()}
                label={t('dontShowAgain')}
              />
            </div>

            <div className="flex items-center justify-center gap-4">
              <Button
                variant="outline"
                size="sm"
                disabled={isLoading || currentIndex === 0}
                onClick={() => fetchContent(currentIndex - 1, eventList)}
              >
                {t('previous')}
              </Button>

              <span className="text-sm text-text-secondary">
                {currentIndex + 1}/{dataLength}
              </span>

              {isLastEvent ? (
                <Button
                  size="sm"
                  disabled={isLoading}
                  onClick={() => setOpen(false)}
                >
                  {t('close')}
                </Button>
              ) : (
                <Button
                  size="sm"
                  disabled={isLoading}
                  onClick={() => fetchContent(currentIndex + 1, eventList)}
                >
                  {t('next')}
                </Button>
              )}
            </div>
          </div>
        </DialogContent>
      </Dialog>
    );
  }
);
