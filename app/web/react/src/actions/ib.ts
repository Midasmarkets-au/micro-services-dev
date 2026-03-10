'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import { normalizeAmountList } from '@/lib/utils';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  IBClientListResponse,
  IBReferralHistoryResponse,
  IBTradeListResponse,
  IBDepositListResponse,
  IBWithdrawalListResponse,
  IBRebateListResponse,
  IBRebateRuleDetail,
  IBRebateRuleItem,
  IBLinkListResponse,
  IBLinkDetail,
  IBReferralCode,
  IBDefaultLevelSetting,
  IBRebateDistribution,
  IBChildStat,
  IBReferralSupplement,
  IBReportRequestResponse,
  SymbolCategory,
  IBRebateRuleDetailFull,
  IBProductCategory,
  IBDefaultLevelSettingMap,
  IBListParams,
  IBReportValue,
  IBRebateDailySeries,
  IBLatestDeposit,
} from '@/types/ib';

function handleApiError(error: unknown, fallbackMessage: string): ActionResponse<never> {
  if (error instanceof ApiError) {
    return { success: false, error: error.message, errorCode: error.errorCode };
  }
  return { success: false, error: fallbackMessage };
}

/**
 * apiClient.v1.get<T>() 的 request 函数会自动将所有响应包装为 { data: actualValue }。
 * 对于 *ListResponse 类型（自身已含 data 字段），API 响应结构匹配，不需要解包。
 * 对于其他类型（数组、标量、普通对象），需要提取 .data 获得实际值。
 */
function unwrapData<T>(response: unknown): T {
  if (
    response !== null &&
    typeof response === 'object' &&
    !Array.isArray(response) &&
    'data' in (response as Record<string, unknown>)
  ) {
    return (response as Record<string, unknown>).data as T;
  }
  return response as T;
}

function buildQuery(params?: Record<string, unknown>): string {
  if (!params) return '';
  const qs = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    if (Array.isArray(value)) {
      value.forEach((v) => qs.append(key, String(v)));
    } else {
      qs.append(key, String(value));
    }
  });
  const str = qs.toString();
  return str ? `?${str}` : '';
}

// ============================================
// IB Client Account API
// ============================================

export async function getIBClients(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBClientListResponse>> {
  try {
    const response = await apiClient.v1.get<IBClientListResponse>(
      `/ib/${agentUid}/account${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB clients');
  }
}

export async function getIBReferralHistory(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBReferralHistoryResponse>> {
  try {
    const response = await apiClient.v1.get<IBReferralHistoryResponse>(
      `/ib/${agentUid}/referral/user-history${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch referral history');
  }
}

export async function getIBViewEmailCode(
  agentUid: number,
  accountUid: number
): Promise<ActionResponse<number>> {
  try {
    const response = await apiClient.v1.get<number>(
      `/ib/${agentUid}/account/${accountUid}/view-email-code`
    );
    return { success: true, data: unwrapData<number>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to get email view code');
  }
}

export async function getIBEmailByCode(
  agentUid: number,
  accountUid: number,
  code: number
): Promise<ActionResponse<string>> {
  try {
    const response = await apiClient.v1.get<string>(
      `/ib/${agentUid}/account/${accountUid}/view-email/${code}`
    );
    return { success: true, data: unwrapData<string>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to get email');
  }
}

// ============================================
// IB Trade Reports API
// ============================================

export async function getIBTradeReports(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBTradeListResponse>> {
  try {
    const response = await apiClient.v1.get<IBTradeListResponse>(
      `/ib/${agentUid}/tradetransaction${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB trade reports');
  }
}

export async function getIBAccountTrades(
  agentUid: number,
  tradeAccountUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBTradeListResponse>> {
  try {
    const response = await apiClient.v1.get<IBTradeListResponse>(
      `/ib/${agentUid}/trade-account/${tradeAccountUid}/trade${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account trades');
  }
}

export async function getIBAccountTransactions(
  agentUid: number,
  tradeAccountUid: number,
  params?: IBListParams
): Promise<ActionResponse<{ data: unknown[]; criteria: unknown }>> {
  try {
    const response = await apiClient.v1.get<{ data: unknown[]; criteria: unknown }>(
      `/ib/${agentUid}/trade-account/${tradeAccountUid}/transaction${buildQuery(params)}`
    );
    const normalized = {
      ...response,
      data: normalizeAmountList(response.data || []) as unknown[],
    };
    return { success: true, data: normalized };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account transactions');
  }
}

// ============================================
// IB Deposit / Withdrawal API
// ============================================

export async function getIBDeposits(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBDepositListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: IBDepositListResponse['data'];
      criteria: IBDepositListResponse['criteria'];
    }>(`/ib/${agentUid}/deposit${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as IBDepositListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as IBDepositListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB deposits');
  }
}

export async function getIBWithdrawals(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBWithdrawalListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: IBWithdrawalListResponse['data'];
      criteria: IBWithdrawalListResponse['criteria'];
    }>(`/ib/${agentUid}/withdrawal${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        ...response,
        data: normalizeAmountList(response.data || []) as IBWithdrawalListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as IBWithdrawalListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB withdrawals');
  }
}

// ============================================
// IB Rebate API
// ============================================

export async function getIBRebates(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBRebateListResponse>> {
  try {
    const response = await apiClient.v1.get<{
      data: IBRebateListResponse['data'];
      criteria: IBRebateListResponse['criteria'];
    }>(`/ib/${agentUid}/rebate${buildQuery(params)}`);
    return {
      success: true,
      data: {
        data: response.data ? (normalizeAmountList(response.data) as IBRebateListResponse['data']) : [],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          ['totalAmount', 'pageTotalAmount'] as never
        ) as IBRebateListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB rebates');
  }
}

export async function getIBRebateRuleDetail(
  agentUid: number
): Promise<ActionResponse<IBRebateRuleDetail>> {
  try {
    const response = await apiClient.v1.get<IBRebateRuleDetail>(
      `/ib/${agentUid}/rebate-rule/detail`
    );
    return { success: true, data: unwrapData<IBRebateRuleDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate rule detail');
  }
}

export async function getIBRebateRuleDetailFull(
  agentUid: number
): Promise<ActionResponse<IBRebateRuleDetailFull>> {
  try {
    const response = await apiClient.v1.get<IBRebateRuleDetailFull>(
      `/ib/${agentUid}/rebate-rule/detail`
    );
    return { success: true, data: unwrapData<IBRebateRuleDetailFull>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate rule detail full');
  }
}

export async function getIBProductCategory(): Promise<ActionResponse<IBProductCategory[]>> {
  try {
    const response = await apiClient.v1.get<IBProductCategory[]>(
      '/client/rebate/symbol/category'
    );
    return { success: true, data: unwrapData<IBProductCategory[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch product category');
  }
}

export async function getIBDefaultLevelSettingMap(
  agentUid: number
): Promise<ActionResponse<IBDefaultLevelSettingMap>> {
  try {
    const response = await apiClient.v1.get<IBDefaultLevelSettingMap>(
      `/ib/${agentUid}/rebate-rule/default-level-setting`
    );
    return { success: true, data: unwrapData<IBDefaultLevelSettingMap>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch default level setting map');
  }
}

export async function getIBAccountsWithConfig(): Promise<ActionResponse<{ data: Array<{ uid: number; configurations?: Array<{ key: string; value: string }> }> }>> {
  try {
    const response = await apiClient.v1.get<{ data: Array<{ uid: number; configurations?: Array<{ key: string; value: string }> }> }>(
      '/client/account'
    );
    return { success: true, data: unwrapData<{ data: Array<{ uid: number; configurations?: Array<{ key: string; value: string }> }> }>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB accounts with config');
  }
}

export async function getIBRebateRuleRemain(
  agentUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/ib/${agentUid}/rebate-rule/remain`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate rule remain');
  }
}

export async function getIBRebateRuleByAccount(
  agentUid: number,
  accountUid: number
): Promise<ActionResponse<IBRebateRuleDetail>> {
  try {
    const response = await apiClient.v1.get<IBRebateRuleDetail>(
      `/ib/${agentUid}/rebate-rule/account/${accountUid}`
    );
    return { success: true, data: unwrapData<IBRebateRuleDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account rebate rule');
  }
}

export async function updateIBRebateRule(
  agentUid: number,
  ruleId: number,
  formData: { rules: IBRebateRuleItem[] }
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/ib/${agentUid}/rebate-rule/${ruleId}`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to update rebate rule');
  }
}

export async function getIBDefaultLevelSetting(
  agentUid: number
): Promise<ActionResponse<IBDefaultLevelSetting>> {
  try {
    const response = await apiClient.v1.get<IBDefaultLevelSetting>(
      `/ib/${agentUid}/rebate-rule/default-level-setting`
    );
    return { success: true, data: unwrapData<IBDefaultLevelSetting>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch default level setting');
  }
}

export async function getIBAccountDefaultLevel(
  agentUid: number,
  accountUid: number
): Promise<ActionResponse<IBDefaultLevelSetting>> {
  try {
    const response = await apiClient.v1.get<IBDefaultLevelSetting>(
      `/ib/${agentUid}/account/${accountUid}/default-level-setting`
    );
    return { success: true, data: unwrapData<IBDefaultLevelSetting>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account default level');
  }
}

export async function getIBRebateDistribution(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBRebateDistribution[]>> {
  try {
    const response = await apiClient.v1.get<IBRebateDistribution[]>(
      `/ib/${agentUid}/rebate-rule/distribution${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<IBRebateDistribution[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate distribution');
  }
}

export async function getIBChildStat(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBChildStat>> {
  try {
    const raw = await apiClient.v1.get<IBChildStat>(
      `/ib/${agentUid}/account/child/stat${buildQuery(params)}`
    );
    const normalized = { ...unwrapData<IBChildStat>(raw) };
    const amountFields = ['rebateAmounts', 'depositAmounts', 'netAmounts', 'profitAmounts', 'withdrawalAmounts'] as const;
    for (const field of amountFields) {
      if (normalized[field]) {
        const obj = normalized[field] as Record<string, number[]>;
        for (const key of Object.keys(obj)) {
          obj[key] = normalizeAmountList(obj[key]) as number[];
        }
      }
    }
    return { success: true, data: normalized };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch child stat');
  }
}

export async function getIBRebateStatBySymbol(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<Record<string, { amounts: Record<string, number[]> }>>> {
  try {
    const raw = await apiClient.v1.get<Record<string, { amounts: Record<string, number[]> }>>(
      `/ib/${agentUid}/account/child/stat/rebate/symbol-grouped${buildQuery(params)}`
    );
    const data = unwrapData<Record<string, { amounts: Record<string, number[]> }>>(raw);
    for (const key of Object.keys(data)) {
      if (data[key]?.amounts) {
        for (const currencyId of Object.keys(data[key].amounts)) {
          data[key].amounts[currencyId] = normalizeAmountList(
            data[key].amounts[currencyId]
          ) as number[];
        }
      }
    }
    return { success: true, data };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate stat by symbol');
  }
}

// ============================================
// IB Link API
// ============================================

export async function getIBLinks(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBLinkListResponse>> {
  try {
    const response = await apiClient.v1.get<IBLinkListResponse>(
      `/ib/${agentUid}/referral${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB links');
  }
}

export async function getIBLinkDetail(
  agentUid: number,
  code: string
): Promise<ActionResponse<IBLinkDetail>> {
  try {
    const response = await apiClient.v1.get<IBLinkDetail>(
      `/ib/${agentUid}/referral/${code}`
    );
    return { success: true, data: unwrapData<IBLinkDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch link detail');
  }
}

export async function getReferralCodeSupplement(
  code: string
): Promise<ActionResponse<IBReferralSupplement>> {
  try {
    const response = await apiClient.v1.get<IBReferralSupplement>(
      `/referralcode/${code}`
    );
    return { success: true, data: unwrapData<IBReferralSupplement>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch referral code supplement');
  }
}

export async function createIBLink(
  agentUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/ib/${agentUid}/referral`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create IB link');
  }
}

export async function createIBLinkForIB(
  agentUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/ib/${agentUid}/referral/ib`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create IB link for IB');
  }
}

export async function createIBLinkForClient(
  agentUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/ib/${agentUid}/referral/client`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create IB link for client');
  }
}

export async function updateIBLink(
  agentUid: number,
  codeId: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/ib/${agentUid}/referral/code/${codeId}`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to update IB link');
  }
}

export async function setIBDefaultClient(
  agentUid: number,
  code: string
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/ib/${agentUid}/referral/code/${code}/default-client`,
      {}
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to set default client');
  }
}

// ============================================
// IB Referral Code API
// ============================================

export async function getIBReferralCodes(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBReferralCode[]>> {
  try {
    const response = await apiClient.v1.get<IBReferralCode[]>(
      `/ib/${agentUid}/referral${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<IBReferralCode[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch referral codes');
  }
}

export async function createIBReferralCode(
  agentUid: number,
  data: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/ib/${agentUid}/referral`,
      data
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create referral code');
  }
}

export async function updateIBReferralCode(
  agentUid: number,
  codeId: string,
  data: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/ib/${agentUid}/referral/${codeId}`,
      data
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to update referral code');
  }
}

// ============================================
// Shared API (not IB-prefixed)
// ============================================

export async function getRebateSymbolCategory(): Promise<ActionResponse<SymbolCategory[]>> {
  try {
    const response = await apiClient.v1.get<SymbolCategory[]>(
      '/client/rebate/symbol/category'
    );
    return { success: true, data: unwrapData<SymbolCategory[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate symbol category');
  }
}

export async function getSymbolCategory(): Promise<ActionResponse<SymbolCategory[]>> {
  try {
    const response = await apiClient.v1.get<SymbolCategory[]>(
      '/client/symbol/category'
    );
    return { success: true, data: unwrapData<SymbolCategory[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch symbol category');
  }
}

export async function getIBAgentRules(
  agentUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/ib/${agentUid}/rebate-rule`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch agent rules');
  }
}

export async function getBrokerRebateRules(
  brokerUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/broker/${brokerUid}/rebate-rule/broker`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch broker rules');
  }
}

// ============================================
// IB Report Requests
// ============================================

export async function getIBReportRequests(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBReportRequestResponse>> {
  try {
    const response = await apiClient.v1.get<IBReportRequestResponse>(
      `/ib/${agentUid}/report/request${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch report requests');
  }
}

// ============================================
// IB Report Service API (Dashboard widgets)
// ============================================

export async function getIBRebateTodayValue(
  agentUid: number,
  timezoneOffset?: number
): Promise<ActionResponse<IBReportValue[]>> {
  try {
    const response = await apiClient.v1.get<IBReportValue[]>(
      `/ib/${agentUid}/report/rebate/today-value${buildQuery({ timezoneOffset })}`
    );
    const data = unwrapData<IBReportValue[]>(response);
    return { success: true, data: Array.isArray(data) ? normalizeAmountList(data) as IBReportValue[] : [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch today rebate value');
  }
}

export async function getIBRebateTotalValue(
  agentUid: number,
  params?: IBListParams
): Promise<ActionResponse<IBReportValue[]>> {
  try {
    const response = await apiClient.v1.get<IBReportValue[]>(
      `/ib/${agentUid}/report/rebate/total-value${buildQuery(params)}`
    );
    const data = unwrapData<IBReportValue[]>(response);
    return { success: true, data: Array.isArray(data) ? normalizeAmountList(data) as IBReportValue[] : [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch total rebate value');
  }
}

export async function getIBRebateDailySeries(
  agentUid: number,
  timezoneOffset?: number
): Promise<ActionResponse<IBRebateDailySeries[]>> {
  try {
    const response = await apiClient.v1.get<IBRebateDailySeries[]>(
      `/ib/${agentUid}/report/rebate/daily${buildQuery({ timezoneOffset })}`
    );
    console.log('response===', response);
    const data = unwrapData<IBRebateDailySeries[]>(response);
    console.log('data===', data);
    return {
      success: true,
      data: Array.isArray(data) ? normalizeAmountList(data, 'totalValue' as never) as IBRebateDailySeries[] : [],
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch daily rebate series');
  }
}

export async function getIBRebateHourlySeries(
  agentUid: number,
  timezoneOffset?: number
): Promise<ActionResponse<IBRebateDailySeries[]>> {
  try {
    const response = await apiClient.v1.get<IBRebateDailySeries[]>(
      `/ib/${agentUid}/report/rebate/hourly${buildQuery({ timezoneOffset })}`
    );
    const data = unwrapData<IBRebateDailySeries[]>(response);
    return {
      success: true,
      data: Array.isArray(data) ? normalizeAmountList(data, 'totalValue' as never) as IBRebateDailySeries[] : [],
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch hourly rebate series');
  }
}

export async function getIBRebateMonthlySeries(
  agentUid: number,
  timezoneOffset?: number
): Promise<ActionResponse<IBRebateDailySeries[]>> {
  try {
    const response = await apiClient.v1.get<IBRebateDailySeries[]>(
      `/ib/${agentUid}/report/rebate/monthly${buildQuery({ timezoneOffset })}`
    );
    const data = unwrapData<IBRebateDailySeries[]>(response);
    return {
      success: true,
      data: Array.isArray(data) ? normalizeAmountList(data, 'totalValue' as never) as IBRebateDailySeries[] : [],
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch monthly rebate series');
  }
}

export async function getIBTradeTodayVolume(
  agentUid: number,
  timezoneOffset?: number
): Promise<ActionResponse<number>> {
  try {
    const response = await apiClient.v1.get<number>(
      `/ib/${agentUid}/report/trade/today-volume${buildQuery({ timezoneOffset })}`
    );
    return { success: true, data: unwrapData<number>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch today trade volume');
  }
}

export async function getIBTradeSymbolVolume(
  agentUid: number,
  timezoneOffset?: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/ib/${agentUid}/report/trade/today-symbol-volume${buildQuery({ timezoneOffset })}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch symbol volume');
  }
}

export async function getIBLatestDeposits(
  agentUid: number,
  count?: number
): Promise<ActionResponse<IBLatestDeposit[]>> {
  try {
    const response = await apiClient.v1.get<IBLatestDeposit[]>(
      `/ib/${agentUid}/report/deposit/latest${buildQuery({ count })}`
    );
    const data = unwrapData<IBLatestDeposit[]>(response);
    return { success: true, data: Array.isArray(data) ? normalizeAmountList(data) as IBLatestDeposit[] : [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch latest deposits');
  }
}

export async function getIBTodayAccountCreation(
  agentUid: number
): Promise<ActionResponse<number>> {
  try {
    const response = await apiClient.v1.get<number>(
      `/ib/${agentUid}/report/account/today-creation`
    );
    return { success: true, data: unwrapData<number>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch today account creation');
  }
}

export async function getIBDepositTodayValue(
  agentUid: number
): Promise<ActionResponse<IBReportValue[]>> {
  try {
    const response = await apiClient.v1.get<IBReportValue[]>(
      `/ib/${agentUid}/report/deposit/today-value`
    );
    const data = unwrapData<IBReportValue[]>(response);
    return { success: true, data: Array.isArray(data) ? normalizeAmountList(data) as IBReportValue[] : [] };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch today deposit value');
  }
}

export async function createIBMonthlyReport(
  params?: IBListParams
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v2.get<unknown>(
      `/client/ib/report/monthly${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to create monthly report');
  }
}
