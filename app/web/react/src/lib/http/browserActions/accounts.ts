'use client';

/**
 * /dashboard 和 /ib 布局用到的账户/服务接口（客户端可中止版本）。
 * 与 `src/actions/accounts.ts` 中的同名函数签名保持一致。
 */

import { browserClient, type BrowserApiResponse } from '../browserClient';
import { buildQuery, normalizeAmountList } from '@/lib/utils';
import {
  PlatformTypes,
  type Account,
  type Application,
  type DemoAccount,
  type Service,
  type ServiceMap,
  type GetAccountsParams,
  type GetApplicationsParams,
} from '@/types/accounts';

export interface WithSignal {
  signal?: AbortSignal;
}

const getPlatformNameFromId = (platform: number): string => {
  const names: Record<number, string> = {
    [PlatformTypes.MetaTrader4]: 'MT4',
    [PlatformTypes.MetaTrader4Demo]: 'MT4 Demo',
    [PlatformTypes.MetaTrader5]: 'MT5',
    [PlatformTypes.MetaTrader5Demo]: 'MT5 Demo',
  };
  return names[platform] || 'Unknown';
};

export async function getLiveAccounts(
  opts: WithSignal,
  params?: GetAccountsParams
): Promise<BrowserApiResponse<Account[]>> {
  const qs = buildQuery(params as Record<string, unknown>);
  const res = await browserClient.getRaw<{ data?: Account[] | { data?: Account[] } } | Account[]>(
    `/client/account${qs}`,
    { signal: opts.signal }
  );
  if (!res.success) return res as BrowserApiResponse<Account[]>;

  const raw = res.data;
  let accounts: Account[] = [];
  if (Array.isArray(raw)) {
    accounts = raw;
  } else if (raw && typeof raw === 'object') {
    const inner = (raw as { data?: unknown }).data;
    if (Array.isArray(inner)) {
      accounts = inner as Account[];
    } else if (inner && typeof inner === 'object' && Array.isArray((inner as { data?: unknown }).data)) {
      accounts = (inner as { data: Account[] }).data;
    }
  }
  return { ...res, data: accounts };
}

export async function getPendingApplications(
  opts: WithSignal,
  params?: GetApplicationsParams
): Promise<BrowserApiResponse<Application[]>> {
  const qs = buildQuery(params);
  const res = await browserClient.getRaw<{ data?: Application[] }>(
    `/client/application${qs}`,
    { signal: opts.signal }
  );
  if (!res.success) return res as BrowserApiResponse<Application[]>;
  const data = Array.isArray(res.data?.data) ? (res.data!.data as Application[]) : [];
  return { ...res, data };
}

export async function getDemoAccounts(
  opts: WithSignal
): Promise<BrowserApiResponse<DemoAccount[]>> {
  const res = await browserClient.getRaw<{ data?: DemoAccount[] }>(
    '/client/trade-demo-account',
    { signal: opts.signal }
  );
  if (!res.success) return res as BrowserApiResponse<DemoAccount[]>;
  const normalized = normalizeAmountList(
    (res.data?.data as DemoAccount[]) || [],
    ['balanceInCents', 'balance']
  ) as DemoAccount[];
  return { ...res, data: normalized };
}

export async function getServiceMap(
  opts: WithSignal
): Promise<BrowserApiResponse<ServiceMap>> {
  const res = await browserClient.getRaw<{ data?: Service[] }>(
    '/client/trade/service',
    { signal: opts.signal }
  );
  if (!res.success) return res as BrowserApiResponse<ServiceMap>;

  const serviceMap: ServiceMap = {};
  (res.data?.data || []).forEach((service) => {
    serviceMap[service.id] = {
      serverName: service.name,
      platform: service.platform,
      platformName: getPlatformNameFromId(service.platform),
    };
  });
  return { ...res, data: serviceMap };
}
