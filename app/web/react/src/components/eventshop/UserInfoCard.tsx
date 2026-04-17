'use client';

import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import type { EventUserDetail } from '@/types/eventshop';

interface UserInfoCardProps {
  userDetail: EventUserDetail;
}

function useBadge(level: number, isDark: boolean) {
  const daynightSrc = isDark ? '/images/eventshop/mdml-night.svg' : '/images/eventshop/mdml-day.svg';
  if (level >= 3) return { src: '/images/eventshop/mdml-gold.svg', grayscale: false };
  if (level === 2) return { src: daynightSrc, grayscale: false };
  // 1级：grayscale 将彩色 SVG 转为银灰色，更接近设计图效果
  return { src: daynightSrc, grayscale: true };
}

export function UserInfoCard({ userDetail }: UserInfoCardProps) {
  const t = useTranslations('eventshop');
  const { theme } = useTheme();
  const level = userDetail.level || 1;
  const { src, grayscale } = useBadge(level, theme === 'dark');

  return (
    <div className="bg-surface border border-border rounded flex flex-col items-center justify-center py-5 w-full">
      <div className="flex flex-col gap-5 items-center">
        <div className="flex flex-col gap-2 items-center">
          <div className="size-20 rounded-full overflow-hidden flex items-center justify-center">
            <Image
              src={src}
              alt=""
              width={80}
              height={80}
              className="size-20 object-cover"
              style={grayscale ? { filter: 'grayscale(1)' } : undefined}
            />
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
