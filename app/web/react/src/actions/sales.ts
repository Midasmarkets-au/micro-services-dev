'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import { normalizeAmountList,buildQuery } from '@/lib/utils';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  SalesClientListResponse,
  SalesReferralHistoryResponse,
  SalesTradeListResponse,
  SalesDepositListResponse,
  SalesWithdrawalListResponse,
  SalesTransactionListResponse,
  SalesLeadListResponse,
  SalesLeadDetail,
  SalesLinkListResponse,
  SalesLinkDetail,
  SalesRebateRuleDetail,
  SalesRebateRuleItem,
  SalesDefaultLevelSetting,
  SalesDefaultLevelSettingMap,
  SalesChildStat,
  SalesAccountStat,
  SalesStatistics,
  SalesListParams,
  SymbolCategory,
} from '@/types/sales';

function handleApiError(error: unknown, fallbackMessage: string): ActionResponse<never> {
  if (error instanceof ApiError) {
    return { success: false, error: error.message, errorCode: error.errorCode };
  }
  return { success: false, error: fallbackMessage };
}

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

 

// ============================================
// Sales Account API
// ============================================

export async function getSalesClients(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesClientListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesClientListResponse>(
      `/sales/${salesUid}/account${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch sales clients');
  }
}

export async function getSalesAccountDetail(
  salesUid: number,
  accountUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/account/${accountUid}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account detail');
  }
}

export async function getSalesAccountStat(
  salesUid: number,
  childUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesAccountStat>> {
  try {
    const response = await apiClient.v1.get<SalesAccountStat>(
      `/sales/${salesUid}/account/${childUid}/stat${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<SalesAccountStat>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account stat');
  }
}

export async function getSalesReferralPath(
  salesUid: number,
  accountUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/account/referralPath/${accountUid}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch referral path');
  }
}

export async function getSalesLevelAccounts(
  salesUid: number,
  childUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/account/${childUid}/level-account`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch level accounts');
  }
}

export async function getSalesViewEmailCode(
  salesUid: number,
  accountUid: number
): Promise<ActionResponse<number>> {
  try {
    const response = await apiClient.v1.get<number>(
      `/sales/${salesUid}/account/${accountUid}/view-email-code`
    );
    return { success: true, data: unwrapData<number>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to get email view code');
  }
}

export async function getSalesEmailByCode(
  salesUid: number,
  accountUid: number,
  code: number
): Promise<ActionResponse<string>> {
  try {
    const response = await apiClient.v1.get<string>(
      `/sales/${salesUid}/account/${accountUid}/view-email/${code}`
    );
    return { success: true, data: unwrapData<string>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to get email');
  }
}

// ============================================
// Sales Trade Reports API
// ============================================

export async function getSalesTradeReports(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesTradeListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesTradeListResponse>(
      `/sales/${salesUid}/tradetransaction${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch trade reports');
  }
}

export async function getSalesClientTrades(
  salesUid: number,
  tradeAccountUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesTradeListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesTradeListResponse>(
      `/sales/${salesUid}/trade-account/${tradeAccountUid}/trade${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch client trades');
  }
}

export async function getSalesClientTransactions(
  salesUid: number,
  tradeAccountUid: number,
  params?: SalesListParams
): Promise<ActionResponse<{ data: unknown[]; criteria: unknown }>> {
  try {
    const response = await apiClient.v1.get<{ data: unknown[]; criteria: unknown }>(
      `/sales/${salesUid}/trade-account/${tradeAccountUid}/transaction${buildQuery(params)}`
    );
    const normalized = {
      ...response,
      data: normalizeAmountList(response.data || []) as unknown[],
    };
    return { success: true, data: normalized };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch client transactions');
  }
}

// ============================================
// Sales Transaction Reports API
// ============================================

export async function getSalesTransactionReports(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesTransactionListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: SalesTransactionListResponse['data'];
      criteria: SalesTransactionListResponse['criteria'];
    }>(`/sales/${salesUid}/transaction${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as SalesTransactionListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as SalesTransactionListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch transaction reports');
  }
}

// ============================================
// Sales Deposit / Withdrawal API
// ============================================

export async function getSalesDeposits(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesDepositListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: SalesDepositListResponse['data'];
      criteria: SalesDepositListResponse['criteria'];
    }>(`/sales/${salesUid}/deposit${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as SalesDepositListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as SalesDepositListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch sales deposits');
  }
}

export async function getSalesWithdrawals(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesWithdrawalListResponse>> {
  try {
    const raw = params ? { ...params } : {};
    delete raw.totalAmount;
    const response = await apiClient.v1.get<{
      data: SalesWithdrawalListResponse['data'];
      criteria: SalesWithdrawalListResponse['criteria'];
    }>(`/sales/${salesUid}/withdrawal${buildQuery(raw)}`);
    return {
      success: true,
      data: {
        data: normalizeAmountList(response.data || []) as SalesWithdrawalListResponse['data'],
        criteria: normalizeAmountList(
          response.criteria || { page: 1, size: 15 },
          'totalAmount' as never
        ) as SalesWithdrawalListResponse['criteria'],
      },
    };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch sales withdrawals');
  }
}

// ============================================
// Sales Link / Referral API
// ============================================

export async function getSalesLinks(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesLinkListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesLinkListResponse>(
      `/sales/${salesUid}/referral${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch sales links');
  }
}
export async function getReferralLinkDetail(
  code: string
): Promise<ActionResponse<SalesLinkDetail>> {
  try {
    const response = await apiClient.v1.get<SalesLinkDetail>(
      `/referralcode/${code}`
    );
    return { success: true, data: unwrapData<SalesLinkDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch link detail');
  }
}

export async function getSalesLinkDetail(
  salesUid: number,
  code: string
): Promise<ActionResponse<SalesLinkDetail>> {
  try {
    const response = await apiClient.v1.get<SalesLinkDetail>(
      `/sales/${salesUid}/referral/code/${code}`
    );
    return { success: true, data: unwrapData<SalesLinkDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch link detail');
  }
}

export async function createSalesLinkForIB(
  salesUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  console.log('createSalesLinkForIB-formData', formData);
  try {
    const response = await apiClient.v1.post<unknown>(
      `/sales/${salesUid}/referral/top-agent`,
      formData
    );
    console.log('createSalesLinkForIB-response', response);
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create sales link for IB');
  }
}

export async function createSalesLinkForClient(
  salesUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/sales/${salesUid}/referral/client`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create sales link for client');
  }
}

export async function updateSalesLink(
  salesUid: number,
  codeId: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/sales/${salesUid}/referral/code/${codeId}`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to update sales link');
  }
}

export async function setSalesDefaultCode(
  salesUid: number,
  code: string
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/sales/${salesUid}/referral/code/${code}/default-client`,
      {}
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to set default code');
  }
}

export async function getSalesReferralHistory(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesReferralHistoryResponse>> {
  try {
    const response = await apiClient.v1.get<SalesReferralHistoryResponse>(
      `/sales/${salesUid}/referral/user-history${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch referral history');
  }
}

// ============================================
// Sales IB Management (from Sales perspective)
// ============================================

export async function getSalesIBAccountConfig(
  salesUid: number,
  agentUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/account/configuration/${agentUid}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB account configuration');
  }
}

export async function getSalesAccountDefaultLevel(
  salesUid: number,
  agentUid: number
): Promise<ActionResponse<SalesDefaultLevelSettingMap>> {
  try {
    const response = await apiClient.v1.get<SalesDefaultLevelSettingMap>(
      `/sales/${salesUid}/account/${agentUid}/default-level-setting`
    );
    return { success: true, data: unwrapData<SalesDefaultLevelSettingMap>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch account default level');
  }
}

export async function getSalesDefaultLevelSetting(
  salesUid: number
): Promise<ActionResponse<SalesDefaultLevelSetting>> {
  try {
    const response = await apiClient.v1.get<SalesDefaultLevelSetting>(
      `/sales/${salesUid}/rebate-rule/default-level-setting`
    );
    return { success: true, data: unwrapData<SalesDefaultLevelSetting>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch default level setting');
  }
}

export async function getSalesAvailableAccountTypes(
  salesUid: number
): Promise<ActionResponse<unknown[]>> {
  try {
    const response = await apiClient.v1.get<unknown[]>(
      `/sales/${salesUid}/account/configuration/account-type-available`
    );
    return { success: true, data: unwrapData<unknown[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch available account types');
  }
}

export async function getSalesIBRebateRuleDetail(
  salesUid: number,
  agentUid: number
): Promise<ActionResponse<SalesRebateRuleDetail>> {
  try {
    const response = await apiClient.v1.get<SalesRebateRuleDetail>(
      `/sales/${salesUid}/rebate-rule/agent/${agentUid}/detail`
    );
    return { success: true, data: unwrapData<SalesRebateRuleDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB rebate rule detail');
  }
}

export async function getSalesIBRebateRuleRemain(
  salesUid: number,
  agentUid: number
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/rebate-rule/agent/${agentUid}/remain`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB rebate rule remain');
  }
}

export async function getSalesIBRebateRule(
  salesUid: number,
  agentUid: number
): Promise<ActionResponse<SalesRebateRuleDetail>> {
  try {
    const response = await apiClient.v1.get<SalesRebateRuleDetail>(
      `/sales/${salesUid}/rebate-rule/agent/${agentUid}`
    );
    return { success: true, data: unwrapData<SalesRebateRuleDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB rebate rule');
  }
}

export async function updateSalesIBRebateRule(
  salesUid: number,
  agentUid: number,
  ruleId: number,
  formData: { rules: SalesRebateRuleItem[] }
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/sales/${salesUid}/rebate-rule/agent/${agentUid}/${ruleId}`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to update IB rebate rule');
  }
}

export async function updateSalesTopIBRebateRule(
  salesUid: number,
  agentUid: number,
  ruleId: number,
  formData: { rules: SalesRebateRuleItem[] }
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.put<unknown>(
      `/sales/${salesUid}/rebate-rule/top-agent/${agentUid}/${ruleId}`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to update top IB rebate rule');
  }
}

export async function getSalesAgentRules(
  salesUid: number,
  params?: { agentUids?: number[] }
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/rebate-rule/agent${buildQuery(params as Record<string, unknown>)}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch agent rules');
  }
}

export async function createSalesIBAgentLink(
  salesUid: number,
  agentUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/sales/${salesUid}/referral/agent/${agentUid}/agent`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create IB agent link');
  }
}

export async function createSalesIBClientLink(
  salesUid: number,
  agentUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/sales/${salesUid}/referral/agent/${agentUid}/client`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to create IB client link');
  }
}

// ============================================
// Sales Lead API
// ============================================

export async function getSalesLeads(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesLeadListResponse>> {
  try {
    const response = await apiClient.v1.get<SalesLeadListResponse>(
      `/sales/${salesUid}/lead${buildQuery(params)}`
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch sales leads');
  }
}

export async function getSalesLeadDetail(
  salesUid: number,
  leadId: number
): Promise<ActionResponse<SalesLeadDetail>> {
  try {
    const response = await apiClient.v1.get<SalesLeadDetail>(
      `/sales/${salesUid}/lead/${leadId}`
    );
    return { success: true, data: unwrapData<SalesLeadDetail>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch lead detail');
  }
}

export async function addSalesLeadComment(
  salesUid: number,
  leadId: number,
  data: { content: string }
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/sales/${salesUid}/lead/${leadId}/comment`,
      data
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to add lead comment');
  }
}

// ============================================
// Sales Statistics API
// ============================================

export async function getSalesStatistics(
  params?: SalesListParams
): Promise<ActionResponse<SalesStatistics>> {
  try {
    const response = await apiClient.v1.get<SalesStatistics>(
      `/sales/statistics${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<SalesStatistics>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch sales statistics');
  }
}

// ============================================
// Sales Child Stat API
// ============================================

export async function getSalesChildStat(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<SalesChildStat>> {
  try {
    const raw = await apiClient.v1.get<SalesChildStat>(
      `/sales/${salesUid}/account/child/stat${buildQuery(params)}`
    );
    const normalized = { ...unwrapData<SalesChildStat>(raw) };
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

export async function getSalesIbStat(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/account/child/stat${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch IB stat');
  }
}

export async function getSalesRebateStatBySymbol(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<Record<string, { amounts: Record<string, number[]> }>>> {
  try {
    const raw = await apiClient.v1.get<Record<string, { amounts: Record<string, number[]> }>>(
      `/sales/${salesUid}/account/child/stat/trade/symbol-grouped${buildQuery(params)}`
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
// Sales Account Operations
// ============================================

export async function salesOpenTradeAccount(
  salesUid: number,
  accountUid: number,
  formData: Record<string, unknown>
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.post<unknown>(
      `/sales/${salesUid}/application/for-user/${accountUid}/trade-account`,
      formData
    );
    return { success: true, data: response };
  } catch (error) {
    return handleApiError(error, 'Failed to open trade account');
  }
}

export async function salesFuzzySearchAccount(
  salesUid: number,
  params?: SalesListParams
): Promise<ActionResponse<unknown>> {
  try {
    const response = await apiClient.v1.get<unknown>(
      `/sales/${salesUid}/search/account${buildQuery(params)}`
    );
    return { success: true, data: unwrapData<unknown>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to search accounts');
  }
}

// ============================================
// Shared API (not sales-prefixed)
// ============================================

export async function getSalesSymbolCategory(): Promise<ActionResponse<SymbolCategory[]>> {
  try {
    const response = await apiClient.v1.get<SymbolCategory[]>(
      '/client/rebate/symbol/category'
    );
    return { success: true, data: unwrapData<SymbolCategory[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch symbol category');
  }
}

export async function getSalesRebateDirectSymbols(): Promise<ActionResponse<unknown[]>> {
  try {
    const response = await apiClient.v1.get<unknown[]>(
      '/client/rebate-direct-schema/symbol/all'
    );
    return { success: true, data: unwrapData<unknown[]>(response) };
  } catch (error) {
    return handleApiError(error, 'Failed to fetch rebate direct symbols');
  }
}
