'use client';

import { useTranslations } from 'next-intl';
import Image from 'next/image';
import { QRCodeSVG } from 'qrcode.react';
import { useUserStore } from '@/stores/userStore';
import { useTheme } from '@/hooks/useTheme';
import { PageLoading } from '@/components/ui';
import {
  getAvailablePlatforms,
  getPlatformLinks,
  getWebTraderLink,
  type PlatformType,
  type RegionType,
} from '@/core/data/platformDownloads';

// 设备图标映射
const deviceIcons: Record<string, string> = {
  mac: '/images/platforms/mac-icon.svg',
  windows: '/images/platforms/windows-icon.svg',
  ios: '/images/platforms/ios-icon.svg',
  android: '/images/platforms/android-icon.svg',
};

// 平台 Logo 映射
const platformLogos: Record<PlatformType, string> = {
  mt4: '/images/platforms/mt4-logo.svg',
  mt5: '/images/platforms/mt5-logo.svg',
};

// 下载按钮组件
function DownloadButton({
  icon,
  label,
  url,
}: {
  icon: string;
  label: string;
  url: string;
}) {
  const hasUrl = !!url;

  if (hasUrl) {
    return (
      <a
        href={url}
        target="_blank"
        rel="noopener noreferrer"
        className="relative flex items-center gap-4 border border-border dark:border-[#333] rounded-[10px] px-5 py-2.5 w-full sm:w-[200px] overflow-hidden hover:border-primary dark:hover:border-primary transition-colors dark:bg-[#222]"
      >
        {/* 左侧高亮条 */}
        <div className="absolute left-0 top-1/2 -translate-y-1/2 w-[3px] h-5 bg-primary rounded-r" />
        <Image src={icon} alt={label} width={20} height={20} className="dark:brightness-0 dark:invert" />
        <span className="text-base text-text-primary font-medium">{label}</span>
      </a>
    );
  }

  // 没有链接时显示禁用状态
  return (
    <div className="relative flex items-center gap-4 border border-border dark:border-[#333] rounded-[10px] px-5 py-2.5 w-full sm:w-[200px] overflow-hidden opacity-50 cursor-not-allowed dark:bg-[#222]">
      {/* 左侧高亮条 */}
      <div className="absolute left-0 top-1/2 -translate-y-1/2 w-[3px] h-5 bg-text-secondary rounded-r" />
      <Image src={icon} alt={label} width={20} height={20} className="grayscale dark:brightness-0 dark:invert" />
      <span className="text-text-base text-text-secondary font-medium">{label}</span>
    </div>
  );
}

// 二维码组件
function QRCodeBox({ url, label }: { url: string; label: string }) {
  if (url) {
    return (
      <div className="flex flex-col items-center gap-1">
        <div className="bg-white rounded-[10px] p-1.5 size-20">
          <QRCodeSVG value={url} size={68} level="M" />
        </div>
        <span className="text-xs text-text-secondary">{label}</span>
      </div>
    );
  }

  // 没有链接时显示占位符
  return (
    <div className="flex flex-col items-center gap-1">
      <div className="bg-surface-secondary dark:bg-[#333] rounded-[10px] p-1.5 size-20 flex items-center justify-center">
        <span className="text-text-secondary text-xs">-</span>
      </div>
      <span className="text-xs text-text-secondary">{label}</span>
    </div>
  );
}

// 平台卡片组件
function PlatformCard({
  platform,
  region,
}: {
  platform: PlatformType;
  region: RegionType;
}) {
  const t = useTranslations('platforms');
  const links = getPlatformLinks(region, platform);
  const webTraderUrl = getWebTraderLink(region, platform);

  const platformName = platform === 'mt4' ? 'MetaTrader 4' : 'MetaTrader 5';
  const hasMobileLinks = links.ios || links.android;

  return (
    <div className="w-full lg:w-[calc(50%-10px)] border border-border dark:border-[#333] rounded-[12px] p-5 flex flex-col gap-5 dark:bg-surface-secondary">
      {/* 平台 Logo 和名称 */}
      <div className="flex flex-col gap-5 items-center">
        <div className="size-20 border-2 border-border dark:border-[#333] rounded-[12px] flex items-center justify-center overflow-hidden">
          <Image
            src={platformLogos[platform]}
            alt={platformName}
            width={50}
            height={50}
            className="object-contain"
          />
        </div>
        <h3 className="text-xl font-semibold text-text-primary">{platformName}</h3>
      </div>

      {/* 分隔线 */}
      <div className="h-px bg-border" />

      {/* 桌面下载 - 始终显示 */}
      <div className="flex flex-col gap-5">
        <h4 className="text-xl font-semibold text-text-primary">{t('desktop')}</h4>
        <div className="flex flex-wrap gap-5">
          <DownloadButton icon={deviceIcons.mac} label="MAC" url={links.mac} />
          <DownloadButton icon={deviceIcons.windows} label="Windows" url={links.windows} />
        </div>

        {/* 分隔线 */}
        <div className="h-px bg-border" />
      </div>

      {/* 移动端下载 - 始终显示 */}
      <div className="flex flex-col gap-5">
        <h4 className="text-xl font-semibold text-text-primary">{t('mobile')}</h4>
        <div className="flex flex-wrap gap-5">
          <DownloadButton icon={deviceIcons.ios} label="iOS" url={links.ios} />
          <DownloadButton icon={deviceIcons.android} label="Android" url={links.android} />
        </div>

        {/* 二维码区域 - 始终显示 */}
        <div className="border border-border dark:border-[#333] rounded-[10px] px-5 py-2.5 flex items-center gap-5 dark:bg-[#222]">
          <div className="flex items-center gap-2.5">
            <QRCodeBox url={links.ios} label="iOS" />
            <QRCodeBox url={links.android} label="Android" />
          </div>
          <div className="flex flex-col gap-1">
            <span className={`text-base font-medium ${hasMobileLinks ? 'text-text-primary' : 'text-text-secondary'}`}>{t('scanToDownload')}</span>
            <span className={`text-responsive-2xl font-medium ${hasMobileLinks ? 'text-text-primary' : 'text-text-secondary'}`}>ios & Android</span>
          </div>
        </div>

        {/* 分隔线 */}
        <div className="h-px bg-border" />
      </div>

      {/* WebTrader - 始终显示 */}
      <div className="flex flex-col gap-5">
        <h4 className="text-xl font-semibold text-text-primary">WebTrader</h4>
        <div className="relative border border-border dark:border-[#333] rounded-[10px] px-5 py-2.5 flex items-center justify-between overflow-hidden dark:bg-[#222]">
          {/* 左侧高亮条 */}
          <div className={`absolute left-0 top-1/2 -translate-y-1/2 w-[3px] h-5 rounded-r ${webTraderUrl ? 'bg-primary' : 'bg-text-secondary'}`} />
          <Image
            src="/images/platforms/webtrader-logo.png"
            alt="WebTrader"
            width={88}
            height={20}
            className={`object-contain ${!webTraderUrl ? 'grayscale opacity-50' : ''}`}
          />
          {webTraderUrl ? (
            <a
              href={webTraderUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="bg-primary text-white text-sm font-medium px-2.5 py-1 rounded w-20 text-center hover:bg-primary/90 transition-colors"
            >
              {t('visit')}
            </a>
          ) : (
            <span className="bg-text-secondary text-white text-sm font-medium px-2.5 py-1 rounded w-20 text-center cursor-not-allowed">
              {t('visit')}
            </span>
          )}
        </div>

        {/* 分隔线 */}
        <div className="h-px bg-border" />
      </div>

      {/* 底部说明 */}
      <p className="text-xs text-text-secondary leading-relaxed">
        {t('disclaimer')}
      </p>
    </div>
  );
}

export default function PlatformsPage() {
  const t = useTranslations('platforms');
  const isLoading = useUserStore((s) => s.isLoading);
  const isInitialized = useUserStore((s) => s.isInitialized);
  const siteConfig = useUserStore((s) => s.siteConfig);
  const { theme } = useTheme();
  // 获取可用平台
  const availablePlatforms = getAvailablePlatforms(
   siteConfig?.tradingPlatformAvailable || []
   //[30, 20]
  );
  console.log('siteConfig', siteConfig);
  // 确定区域（默认 bvi）
  const region: RegionType = 'bvi';
  
  // 根据主题选择图标
  const platformIcon = theme === 'dark' 
    ? '/images/platforms/platform-icon-night.svg' 
    : '/images/platforms/platform-icon-day.svg';

  if (!isInitialized || isLoading) {
    return <PageLoading fullscreen={false} className="py-20" />;
  }

  return (
    <div className="flex flex-1 flex-col gap-5 rounded bg-surface p-5">
      {/* 页面标题 */}
      <div className="flex items-center gap-3 w-full">
        <Image
          src={platformIcon}
          alt="Platforms"
          width={26}
          height={26}
        />
        <h1 className="text-xl font-semibold text-text-primary">{t('title')}</h1>
      </div>

      {/* 分隔线 */}
      <div className="h-px bg-border w-full" />

      {/* 平台卡片 */}
      {availablePlatforms.length > 0 ? (
        <div className="flex flex-col lg:flex-row gap-5 w-full">
          {availablePlatforms.map((platform) => (
            <PlatformCard key={platform} platform={platform} region={region} />
          ))}
        </div>
      ) : (
        <div className="flex items-center justify-center py-20 w-full">
          <p className="text-text-secondary">{t('noPlatforms')}</p>
        </div>
      )}
    </div>
  );
}
