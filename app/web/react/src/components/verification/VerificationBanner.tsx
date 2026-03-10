'use client';

import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';

export function VerificationBanner() {
  const t = useTranslations('verification');
  const { isDark, mounted } = useTheme();

  // 根据主题选择图片
  const verifyImage = isDark
    ? '/images/verification/verify-night.svg'
    : '/images/verification/verify-day.svg';

  return (
    <div className="verification-banner verification-banner-bg relative h-32 md:h-60 w-full overflow-hidden rounded">
      {/* 内容 */}
      <div className="relative z-10 flex h-full items-center justify-between px-4 md:px-18">
        <div className="text-white">
          <h1 className="text-xl md:text-responsive-3xl font-semibold">
            {t('banner.title')}
          </h1>
          <p className="text-sm md:text-responsive-3xl font-semibold">
            {t('banner.subtitle')}
          </p>
        </div>

        {/* 右侧图标 - 根据主题动态切换，移动端隐藏 */}
        <div className="relative hidden sm:flex size-32 md:size-65 items-center justify-center">
          {mounted && (
            <div className="relative size-[100px] md:size-[200px]">
              <Image
                src={verifyImage}
                alt="verify"
                fill
                className="object-contain opacity-80"
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
