'use client';

/* eslint-disable @next/next/no-img-element */
import { useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { getMediaUrl } from '@/actions';
import type { EventDetail } from '@/types/eventshop';
import { TimeShow } from '../TimeShow';

interface ShopBannerProps {
  eventDetail: EventDetail;
}

const themeConfig = {
  light: {
    gradient: 'radial-gradient(92.67% 68.59% at 100% 0%, #800020 0%, #000 100%)',
    textGradient: 'linear-gradient(90deg, #800020 0%, #EC7579 100%)',
    giftBox: '/images/eventshop/gift-box.png',
    blendMode: 'normal' as const,
  },
  dark: {
    gradient: 'radial-gradient(92.67% 68.59% at 100% 0%, #004eff 0%, #000 100%)',
    textGradient: 'linear-gradient(90deg, #0025db 0%, #7bd4f1 100%)',
    giftBox: '/images/eventshop/gift-box-dark.png',
    blendMode: 'hard-light' as const,
  },
};

export function ShopBanner({ eventDetail }: ShopBannerProps) {
  const t = useTranslations('eventshop.banner');
  const { theme } = useTheme();
  const config = themeConfig[theme === 'dark' ? 'dark' : 'light'];
  const hasDateRange = eventDetail.startOn || eventDetail.endOn;
  const bannerGuid = (eventDetail as Record<string, unknown> & { images?: { banner?: string } }).images?.banner;
  const directUrl = '';//bannerGuid?.startsWith('http') ? bannerGuid : '';
  const [bannerUrl, setBannerUrl] = useState(directUrl);

  useEffect(() => {
    if (!bannerGuid || bannerGuid.startsWith('http')) return;
    getMediaUrl(bannerGuid).then((res) => {
      if (res.success && res.data) setBannerUrl(res.data);
    });
  }, [bannerGuid]);

  return (
    <div
      className="relative h-[228px] rounded overflow-hidden w-full"
      style={{
        background: bannerUrl ? `url("${bannerUrl}") center/cover no-repeat` : config.gradient,
      }}
    >
      {/* 光效容器 — Figma: left=399, top=-449.3, container=1105.6 */}
      <div
        className="absolute left-[399px] top-[-449.3px] size-[1105.6px] pointer-events-none"
        style={{ mixBlendMode: theme === 'dark' ? 'hard-light' : undefined }}
      >
        {/* 蓝色漩涡底图 — 957px, rotate=-9.8deg, opacity=0.8 */}
        <div className="absolute inset-0 flex items-center justify-center">
          <img
            src="/images/eventshop/light-effect.png"
            alt=""
            className="flex-none size-[957px] rotate-[-9.8deg] opacity-80"
          />
        </div>
        {/* 红色滤镜 — 仅日间模式：红色渐变圆 multiply 在蓝色漩涡上 → 变红 */}
        {theme !== 'dark' && (
          <div className="absolute inset-0 flex items-center justify-center" style={{ mixBlendMode: 'multiply' }}>
            <img
              src="/images/eventshop/light-swirl.svg"
              alt=""
              className="flex-none size-[957px] rotate-[-9.8deg]"
            />
          </div>
        )}
      </div>

      {/* 文字内容 */}
      <div className="absolute left-5 md:left-[70px] top-8 md:top-[49px] z-10">
        <p className="text-responsive-2xl md:text-responsive-3xl font-semibold text-white">
          {t('title')}
        </p>
        <p className="text-responsive-2xl md:text-responsive-3xl font-semibold text-white">
          {/* {t('subtitle')} */}
          {eventDetail.name}
          {/* <span
            className="ml-2 font-semibold bg-clip-text text-transparent"
            style={{
              backgroundImage: config.textGradient,
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
            }}
          >
            {t('newArrival')}
          </span> */}
        </p>
      </div>

      {/* 活动日期 — Figma: left=70, top=161 */}
      {hasDateRange && (
        <p className="absolute left-5 md:left-[70px] top-[161px] z-10 text-sm font-light text-white">
          {t('eventDate')} ：
          {/* <DateDisplay value={eventDetail.startOn} format="date" /> */}
          <TimeShow dateIsoString={eventDetail.startOn} className="text-sm font-medium text-white" type="eventShop" />
          ——<TimeShow
            dateIsoString={eventDetail.endOn}
            className="text-sm  font-medium text-white"
            type="eventShop"
          />
          {/* <DateDisplay value={eventDetail.endOn} format="date" /> */}
        </p>
      )}

      {/* 礼盒 — Figma: left=765, top=-55.3, container=390, image=294, rotate=-25deg */}
      <div className="absolute left-[765px] top-[-55.3px] z-10 size-[390px] flex items-center justify-center">
        <img
          src={config.giftBox}
          alt="Gift"
          className="flex-none size-[294px] rotate-[-25deg] object-contain"
        />
      </div>
    </div>
  );
}
