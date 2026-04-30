'use client';

import { useState, useEffect, useRef } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useServerAction } from '@/hooks/useServerAction';
import { getShopOrderList } from '@/actions';
import type { ShopOrder } from '@/types/eventshop';
import { ShopPoints } from './ShopPoints';
import { OrderDetailModal } from './OrderDetailModal';
import { TimeShow } from '@/components/TimeShow';
interface NotificationBarProps {
  refreshKey?: number;
}

export function NotificationBar({ refreshKey = 0 }: NotificationBarProps) {
  const t = useTranslations('eventshop');
  const { theme } = useTheme();
  const { execute } = useServerAction({ showErrorToast: false });
  const [notification, setNotification] = useState<ShopOrder | null>(null);
  const [closed, setClosed] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);
  const isLoadedRef = useRef(false);

  const fetchLatestOrder = async () => {
    const result = await execute(getShopOrderList, {
      page: 1,
      size: 10,
      sortField: 'createdOn',
    });
    if (result.success && result.data?.items?.length) {
      setNotification(result.data.items[0]);
      setClosed(false);
    }
  };

  useEffect(() => {
    if (isLoadedRef.current) return;
    isLoadedRef.current = true;
    fetchLatestOrder();
  }, [execute]);

  useEffect(() => {
    if (refreshKey > 0) fetchLatestOrder();
  }, [refreshKey]);

  if (!notification || closed) return null;

  const isDark = theme === 'dark';

  return (
    <div className={`flex items-center justify-between px-5 py-2 rounded overflow-hidden ${
      isDark ? 'bg-[rgba(0,78,255,0.2)]' : 'bg-[rgba(128,0,32,0.2)]'
    }`}>
      <div className="flex items-center gap-2 flex-1 min-w-0">
        <Image
          src={isDark ? '/images/eventshop/notification-icon-night.svg' : '/images/eventshop/notification-icon-day.svg'}
          alt=""
          width={20}
          height={20}
          className="shrink-0"
        />
        <div className={`text-sm truncate opacity-60 ${
          isDark ? 'text-white' : 'text-[#800020]'
        }`}>
          {t('notification.orderNumber')}:{notification.hashId},
          {t('notification.pointsUsed')}:<ShopPoints value={notification.totalPoint} showIcon={false} className="text-inherit! font-normal!" />,
          {t('notification.itemName')}:{notification.eventShopItemName},
          {t('notification.quantity')}:{notification.quantity},
          {t('notification.date')}:<TimeShow dateIsoString={notification.updatedOn} type="eventShop" />
        </div>
      </div>
      <button
        onClick={() => setDetailOpen(true)}
        className={`flex items-center gap-1 text-sm shrink-0 cursor-pointer ${
          isDark ? 'text-white' : 'text-[#800020]'
        }`}
      >
        {t('notification.view')}
        <Image
          src={isDark ? '/images/eventshop/arrow-right-night.svg' : '/images/eventshop/arrow-right-day.svg'}
          alt=""
          width={20}
          height={20}
        />
      </button>

      <OrderDetailModal
        open={detailOpen}
        onOpenChange={setDetailOpen}
        orderHashId={notification.hashId}
      />
    </div>
  );
}
