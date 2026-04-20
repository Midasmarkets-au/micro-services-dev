'use client';

/**
 * /dashboard 的通知接口（客户端可中止版本）。
 * 对应 src/actions/contact.ts 的 getNotifications。
 */

import { browserClient, type BrowserApiResponse } from '../browserClient';

export interface WithSignal {
  signal?: AbortSignal;
}

export async function getNotifications<T = unknown>(
  opts: WithSignal,
  size: number = 8
): Promise<BrowserApiResponse<T[]>> {
  const res = await browserClient.getRaw<{ data?: T[] } | T[]>(
    `/client/topic/notice?size=${size}`,
    { signal: opts.signal }
  );
  if (!res.success) return res as BrowserApiResponse<T[]>;

  const data = Array.isArray(res.data)
    ? (res.data as T[])
    : Array.isArray((res.data as { data?: T[] })?.data)
      ? ((res.data as { data: T[] }).data)
      : [];
  return { ...res, data };
}
