'use client';

import { useTranslations } from 'next-intl';
import type { EventUserDetail } from '@/types/eventshop';

interface UserInfoCardProps {
  userDetail: EventUserDetail;
}

export function UserInfoCard({ userDetail }: UserInfoCardProps) {
  const t = useTranslations('eventshop');

  return (
    <div className="bg-surface border border-border rounded flex flex-col items-center justify-center py-5 w-full">
      <div className="flex flex-col gap-5 items-center">
        <div className="flex flex-col gap-2 items-center">
          <div className="size-20 rounded-full bg-surface-secondary overflow-hidden mix-blend-luminosity flex items-center justify-center">
            {/* <Image
              src="/images/mdm-badge.png"
              alt=""
              width={80}
              height={80}
              className="size-20 object-cover"
            /> */}
          </div>
          <p className="text-3xl font-bold text-text-primary tracking-tight">
            Lv.{userDetail.level || 1}
          </p>
        </div>

        <div className="flex flex-col gap-1 items-center px-2 w-[200px]">
          <div className="flex items-center justify-between w-full leading-relaxed">
            <span className="text-sm text-text-secondary">
              {t('userInfo.points')}
            </span>
            <span className="text-xl font-semibold text-text-primary text-center">
              {typeof userDetail.point === 'number'
                ? userDetail.point.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                : userDetail.point}
            </span>
          </div>
          <div className="flex items-center justify-between w-full text-text-secondary leading-relaxed">
            <span className="text-sm">
              {t('userInfo.unavailable')}
            </span>
            <span className="text-base">
              {typeof userDetail.notavailable === 'number'
                ? userDetail.notavailable.toFixed(4)
                : userDetail.notavailable}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
