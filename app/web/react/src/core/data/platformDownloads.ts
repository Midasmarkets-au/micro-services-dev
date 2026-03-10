/**
 * 平台下载链接配置
 */

// 平台类型映射
export const PLATFORM_TYPE = {
  MT4: 20,
  MT5: 30,
} as const;

// 区域类型
export type RegionType = 'bvi' | 'ba';

// 平台类型
export type PlatformType = 'mt4' | 'mt5';

// 设备类型
export type DeviceType = 'windows' | 'mac' | 'android' | 'ios';

// 下载链接配置
export interface PlatformLinks {
  windows: string;
  mac: string;
  android: string;
  ios: string;
}

// 平台下载链接配置
export const platformDownloadLinks: Record<RegionType, Record<PlatformType, PlatformLinks>> = {
  bvi: {
    mt4: {
      windows: '',
      mac: '',
      android: '',
      ios: '',
    },
    mt5: {
      windows:
        'https://mm-official-web.s3.ap-southeast-1.amazonaws.com/download/mmco5setupWindows.exe',
      mac: 'https://mm-official-web.s3.ap-southeast-1.amazonaws.com/download/MetaTraderMac.pkg',
      android: 'https://download.mql5.com/cdn/mobile/mt5/android?server=MMCo-Main',
      ios: 'https://download.mql5.com/cdn/mobile/mt5/ios?server=MMCo-Main',
    },
  },
  ba: {
    mt4: {
      windows: '',
      mac: '',
      android: '',
      ios: '',
    },
    mt5: {
      windows: '',
      mac: '',
      android: '',
      ios: '',
    },
  },
};

// WebTrader 链接配置
export const webTraderLinks: Record<RegionType, Record<PlatformType, string>> = {
  bvi: {
    mt4: '',
    mt5: 'https://webtrader.mmco.com',
  },
  ba: {
    mt4: '',
    mt5: '',
  },
};

/**
 * 根据 tradingPlatformAvailable 数组获取可用的平台列表
 * @param available - tradingPlatformAvailable 数组，如 [30, 20]
 * @returns 可用的平台类型列表
 */
export function getAvailablePlatforms(available: number[]): PlatformType[] {
  const platforms: PlatformType[] = [];
  
  if (available.includes(PLATFORM_TYPE.MT4)) {
    platforms.push('mt4');
  }
  if (available.includes(PLATFORM_TYPE.MT5)) {
    platforms.push('mt5');
  }
  
  return platforms;
}

/**
 * 获取平台下载链接
 * @param region - 区域类型
 * @param platform - 平台类型
 * @returns 下载链接对象
 */
export function getPlatformLinks(region: RegionType, platform: PlatformType): PlatformLinks {
  return platformDownloadLinks[region]?.[platform] || {
    windows: '',
    mac: '',
    android: '',
    ios: '',
  };
}

/**
 * 获取 WebTrader 链接
 * @param region - 区域类型
 * @param platform - 平台类型
 * @returns WebTrader 链接
 */
export function getWebTraderLink(region: RegionType, platform: PlatformType): string {
  return webTraderLinks[region]?.[platform] || '';
}

/**
 * 检查是否有可用的下载链接
 * @param links - 下载链接对象
 * @returns 是否有可用链接
 */
export function hasAvailableLinks(links: PlatformLinks): boolean {
  return !!(links.windows || links.mac || links.android || links.ios);
}
