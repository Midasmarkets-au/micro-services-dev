// 租户类型枚举
export enum TenantTypes {
  au = 1,
  bvi = 10000,
  sea = 10004,
  jp = 10005,
}

// 租户名称枚举
export enum Tenancies {
  au = 'au',
  bvi = 'bvi',
  sea = 'sea',
  jp = 'jp',
}

// 站点类型枚举
export enum SiteTypes {
  Default = 0,
  BritishVirginIslands = 1,
  Australia = 2,
  China = 3,
  Taiwan = 4,
  Vietnam = 5,
  Japan = 6,
  Mongolia = 7,
  Malaysia = 8,
}

// JP 站点域名列表
export const jpSites = ['thebcrjp.com'];

// 获取当前租户类型
export const getTenancy = (): Tenancies => {
  if (typeof window === 'undefined') return Tenancies.bvi;
  
  const hostname = window.location.hostname;
  
  // JP 站点判断
  if (jpSites.includes(hostname)) {
    return Tenancies.jp;
  }
  
  // 根据子域名判断
  const type = hostname.split('.')[0] as keyof typeof Tenancies;
  if (Tenancies[type]) {
    return Tenancies[type];
  }
  
  // 默认 BVI
  return Tenancies.bvi;
};

// 根据租户获取默认 tenantId
export const getTenantIdByTenancy = (tenancy: Tenancies): number => {
  const tenantMap: Record<Tenancies, number> = {
    [Tenancies.au]: TenantTypes.au,
    [Tenancies.bvi]: TenantTypes.bvi,
    [Tenancies.sea]: TenantTypes.sea,
    [Tenancies.jp]: TenantTypes.jp,
  };
  return tenantMap[tenancy] || TenantTypes.bvi;
};

// 根据 tenantId 获取 openAt 参数
export const getOpenAtByTenantId = (tenantId: number): string => {
  const openAtMap: Record<number, string> = {
    [TenantTypes.au]: 'au',
    [TenantTypes.bvi]: 'bvi',
    [TenantTypes.sea]: 'sea',
    [TenantTypes.jp]: 'jp',
  };
  return openAtMap[tenantId] || 'bvi';
};

// 联系邮箱配置
export const getContactEmail = (tenancy: Tenancies, siteType?: SiteTypes): string => {
  if (tenancy === Tenancies.jp) {
    return 'info@isec.jp';
  }
  if (tenancy === Tenancies.au || siteType === SiteTypes.Australia) {
    return 'info@midasmkts.com';
  }
  return 'info@midasmkts.com';
};

