'use client';

import { useState } from 'react';
import { UserInfoCard } from './UserInfoCard';
import { ShopSidebar, type SidebarTab } from './ShopSidebar';
import { ShopBanner } from './ShopBanner';
import { NotificationBar } from './NotificationBar';
import { ProductGrid } from './ProductGrid';
import { OrderHistory } from './OrderHistory';
import { PointsHistoryTab } from './PointsHistory';
import { RewardDetails } from './RewardDetails';
import { ShopTerms } from './ShopTerms';
import { PointsRules } from './PointsRules';
import type { EventDetail, EventUserDetail } from '@/types/eventshop';

interface EventShopContentProps {
  eventDetail: EventDetail;
  userDetail: EventUserDetail | null;
  step: number;
}

export function EventShopContent({
  eventDetail,
  userDetail,
}: EventShopContentProps) {
  const [activeTab, setActiveTab] = useState<SidebarTab>('shop');
  const [notificationRefreshKey, setNotificationRefreshKey] = useState(0);

  const renderContent = () => {
    switch (activeTab) {
      case 'shop':
        return (
          <div className="flex flex-1 flex-col gap-5 h-full min-w-0">
            <ShopBanner eventDetail={eventDetail} />
            <ProductGrid userPoints={userDetail?.point ?? 0} onExchangeSuccess={() => setNotificationRefreshKey((k) => k + 1)} />
          </div>
        );
      case 'orderHistory':
        return <OrderHistory />;
      case 'pointsHistory':
        return <PointsHistoryTab />;
      case 'rewardDetails':
        return <RewardDetails />;
      case 'terms':
        return <ShopTerms eventDetail={eventDetail} />;
      case 'pointsRules':
        return <PointsRules eventDetail={eventDetail} />;
      default:
        return null;
    }
  };

  return (
    <div className="flex flex-col gap-3 md:gap-5 w-full">
      <NotificationBar refreshKey={notificationRefreshKey} />

      <div className="flex flex-col md:flex-row gap-3 md:gap-5 w-full flex-1 min-h-0">
        <div className="flex flex-col gap-3 md:gap-5 w-full md:w-[320px] shrink-0">
          {userDetail && <UserInfoCard userDetail={userDetail} />}
          <ShopSidebar activeTab={activeTab} onTabChange={setActiveTab} />
        </div>

        {renderContent()}
      </div>
    </div>
  );
}
