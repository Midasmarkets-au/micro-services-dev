'use client';

/**
 * /ib 首页用到的接口（客户端可中止版本）。
 *
 * 与 `src/actions/ib.ts` 中对应函数签名保持一致，
 * 只是把第一个参数改为可选的 `signal`，用法见 /ib/page.tsx。
 */

import { browserClient, type BrowserApiResponse } from '../browserClient';
import { buildQuery, normalizeAmountList } from '@/lib/utils';
import type {
  IBReferralHistoryResponse,
  IBTradeListResponse,
  IBLinkListResponse,
  IBReportValue,
  IBRebateDailySeries,
  IBLatestDeposit,
  IBListParams,
} from '@/types/ib';

export interface WithSignal {
  signal?: AbortSignal;
}

// ===== Report Service (StatCard) =====

export function getIBRebateTodayValue(
  opts: WithSignal,
  agentUid: number,
  timezoneOffset?: number
): Promise<BrowserApiResponse<IBReportValue[]>> {
  return browserClient
    .get<IBReportValue[]>(
      `/ib/${agentUid}/report/rebate/today-value${buildQuery({ timezoneOffset })}`,
      { signal: opts.signal }
    )
    .then((res) =>
      res.success && Array.isArray(res.data)
        ? { ...res, data: normalizeAmountList(res.data) as IBReportValue[] }
        : res
    );
}

export function getIBRebateTotalValue(
  opts: WithSignal,
  agentUid: number,
  params?: IBListParams
): Promise<BrowserApiResponse<IBReportValue[]>> {
  return browserClient
    .get<IBReportValue[]>(
      `/ib/${agentUid}/report/rebate/total-value${buildQuery(params)}`,
      { signal: opts.signal }
    )
    .then((res) =>
      res.success && Array.isArray(res.data)
        ? { ...res, data: normalizeAmountList(res.data) as IBReportValue[] }
        : res
    );
}

export function getIBTradeTodayVolume(
  opts: WithSignal,
  agentUid: number,
  timezoneOffset?: number
): Promise<BrowserApiResponse<number>> {
  return browserClient.get<number>(
    `/ib/${agentUid}/report/trade/today-volume${buildQuery({ timezoneOffset })}`,
    { signal: opts.signal }
  );
}

export function getIBTodayAccountCreation(
  opts: WithSignal,
  agentUid: number
): Promise<BrowserApiResponse<number>> {
  return browserClient.get<number>(
    `/ib/${agentUid}/report/account/today-creation`,
    { signal: opts.signal }
  );
}

export function getIBDepositTodayValue(
  opts: WithSignal,
  agentUid: number
): Promise<BrowserApiResponse<IBReportValue[]>> {
  return browserClient
    .get<IBReportValue[]>(`/ib/${agentUid}/report/deposit/today-value`, {
      signal: opts.signal,
    })
    .then((res) =>
      res.success && Array.isArray(res.data)
        ? { ...res, data: normalizeAmountList(res.data) as IBReportValue[] }
        : res
    );
}

// ===== Rebate Chart =====

function normalizeRebateSeries(data: unknown): IBRebateDailySeries[] {
  if (!Array.isArray(data)) return [];
  return normalizeAmountList(
    data as IBRebateDailySeries[],
    'totalValue' as never
  ) as IBRebateDailySeries[];
}

export function getIBRebateDailySeries(
  opts: WithSignal,
  agentUid: number,
  timezoneOffset?: number
): Promise<BrowserApiResponse<IBRebateDailySeries[]>> {
  return browserClient
    .get<IBRebateDailySeries[]>(
      `/ib/${agentUid}/report/rebate/daily${buildQuery({ timezoneOffset })}`,
      { signal: opts.signal }
    )
    .then((res) => (res.success ? { ...res, data: normalizeRebateSeries(res.data) } : res));
}

export function getIBRebateHourlySeries(
  opts: WithSignal,
  agentUid: number,
  timezoneOffset?: number
): Promise<BrowserApiResponse<IBRebateDailySeries[]>> {
  return browserClient
    .get<IBRebateDailySeries[]>(
      `/ib/${agentUid}/report/rebate/hourly${buildQuery({ timezoneOffset })}`,
      { signal: opts.signal }
    )
    .then((res) => (res.success ? { ...res, data: normalizeRebateSeries(res.data) } : res));
}

export function getIBRebateMonthlySeries(
  opts: WithSignal,
  agentUid: number,
  timezoneOffset?: number
): Promise<BrowserApiResponse<IBRebateDailySeries[]>> {
  return browserClient
    .get<IBRebateDailySeries[]>(
      `/ib/${agentUid}/report/rebate/monthly${buildQuery({ timezoneOffset })}`,
      { signal: opts.signal }
    )
    .then((res) => (res.success ? { ...res, data: normalizeRebateSeries(res.data) } : res));
}

// ===== Latest Deposits =====

export function getIBLatestDeposits(
  opts: WithSignal,
  agentUid: number,
  count?: number
): Promise<BrowserApiResponse<IBLatestDeposit[]>> {
  return browserClient
    .get<IBLatestDeposit[]>(
      `/ib/${agentUid}/report/deposit/latest${buildQuery({ count })}`,
      { signal: opts.signal }
    )
    .then((res) =>
      res.success && Array.isArray(res.data)
        ? { ...res, data: normalizeAmountList(res.data) as IBLatestDeposit[] }
        : res
    );
}

// ===== Referral History (new customers) =====

export function getIBReferralHistory(
  opts: WithSignal,
  agentUid: number,
  params?: IBListParams
): Promise<BrowserApiResponse<IBReferralHistoryResponse>> {
  // 这个接口的响应本身含 data/criteria，需要 raw
  return browserClient.getRaw<IBReferralHistoryResponse>(
    `/ib/${agentUid}/referral/user-history${buildQuery(params)}`,
    { signal: opts.signal }
  );
}

// ===== IB Trade Reports (Trading Lots) =====

export function getIBTradeReports(
  opts: WithSignal,
  agentUid: number,
  params?: IBListParams
): Promise<BrowserApiResponse<IBTradeListResponse>> {
  return browserClient.getRaw<IBTradeListResponse>(
    `/ib/${agentUid}/tradetransaction${buildQuery(params)}`,
    { signal: opts.signal }
  );
}

// ===== IB Links =====

export function getIBLinks(
  opts: WithSignal,
  agentUid: number,
  params?: IBListParams
): Promise<BrowserApiResponse<IBLinkListResponse>> {
  return browserClient.getRaw<IBLinkListResponse>(
    `/ib/${agentUid}/referral${buildQuery(params)}`,
    { signal: opts.signal }
  );
}
