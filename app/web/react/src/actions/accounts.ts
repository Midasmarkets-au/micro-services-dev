'use server';

import { apiClient, ApiError } from '@/lib/api/client';
import { normalizeAmountList } from '@/lib/utils';
import type { ActionResponse } from '@/hooks/useServerAction';
import type {
  Account,
  Application,
  DemoAccount,
  Service,
  ServiceMap,
  AccountConfig,
  GetAccountsParams,
  GetApplicationsParams,
  CreateLiveAccountParams,
  CreateDemoAccountParams,
  AccountTransaction,
  AccountDeposit,
  AccountWithdrawal,
  AccountTrade,
  TransactionQueryParams,
  DepositQueryParams,
  WithdrawalQueryParams,
  TradeQueryParams,
  PaginatedResponse,
} from '@/types/accounts';
import { PlatformTypes } from '@/types/accounts';

// 获取平台名称的辅助函数
const getPlatformNameFromId = (platform: number): string => {
  const names: Record<number, string> = {
    [PlatformTypes.MetaTrader4]: 'MT4',
    [PlatformTypes.MetaTrader4Demo]: 'MT4 Demo',
    [PlatformTypes.MetaTrader5]: 'MT5',
    [PlatformTypes.MetaTrader5Demo]: 'MT5 Demo',
  };
  return names[platform] || 'Unknown';
};

/**
 * 获取真实账户列表
 */
export async function getLiveAccounts(
  params?: GetAccountsParams
): Promise<ActionResponse<Account[]>> {
  try {
    const queryParams = new URLSearchParams();
    if (params?.hasTradeAccount !== undefined) {
      queryParams.append('hasTradeAccount', String(params.hasTradeAccount));
    }
    if (params?.status !== undefined) {
      queryParams.append('status', String(params.status));
    }
    if (params?.roles?.length) {
      params.roles.forEach((role) => queryParams.append('roles', String(role)));
    }
    if (params?.uids?.length) {
      params.uids.forEach((uid) => queryParams.append('uids', String(uid)));
    }

    const queryString = queryParams.toString();
    const url = `/client/account${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.v1.get<{ data: Account[] | Record<string, unknown> }>(url);
    const rawData = response.data;
    const accounts: Account[] = Array.isArray(rawData)
      ? rawData
      : Array.isArray((rawData as Record<string, unknown>)?.data)
        ? (rawData as Record<string, unknown>).data as Account[]
        : [];
    return { success: true, data: accounts };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch accounts' };
  }
}

/**
 * 获取待审核申请列表
 */
export async function getPendingApplications(
  params?: GetApplicationsParams
): Promise<ActionResponse<Application[]>> {
  try {
    // 构建查询参数
    const queryParams = new URLSearchParams();
    if (params?.statuses?.length) {
      params.statuses.forEach((status) =>
        queryParams.append('statuses', String(status))
      );
    }
    if (params?.type !== undefined) {
      queryParams.append('type', String(params.type));
    }

    const queryString = queryParams.toString();
    const url = `/client/application${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.v1.get<{ data: Application[] }>(url);
    return { success: true, data: response.data || [] };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch applications' };
  }
}

/**
 * 获取模拟账户列表
 */
export async function getDemoAccounts(): Promise<ActionResponse<DemoAccount[]>> {
  try {
    const response = await apiClient.v1.get<{ data: DemoAccount[] }>(
      '/client/trade-demo-account'
    );
    const normalized = normalizeAmountList(
      response.data || [],
      ['balanceInCents', 'balance']
    ) as DemoAccount[];
    return { success: true, data: normalized };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch demo accounts' };
  }
}

/**
 * 获取服务/平台映射
 */
export async function getServiceMap(): Promise<ActionResponse<ServiceMap>> {
  try {
    const response = await apiClient.v1.get<{ data: Service[] }>(
      '/client/trade/service'
    );

    // 转换为 Map 结构
    const serviceMap: ServiceMap = {};
    (response.data || []).forEach((service) => {
      serviceMap[service.id] = {
        serverName: service.name,
        platform: service.platform,
        platformName: getPlatformNameFromId(service.platform),
      };
    });
    return { success: true, data: serviceMap };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch service map' };
  }
}

/**
 * 获取创建真实账户配置
 */
export async function getLiveAccountConfig(): Promise<ActionResponse<AccountConfig>> {
  try {
    const response = await apiClient.v2.get<{ data: AccountConfig }>(
      '/client/account/application-config'
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch account config' };
  }
}

/**
 * 获取创建模拟账户配置
 */
export async function getDemoAccountConfig(): Promise<ActionResponse<AccountConfig>> {
  try {
    const response = await apiClient.v2.get<{ data: AccountConfig }>(
      '/client/account/demo/application-config'
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch demo account config' };
  }
}

/**
 * 创建真实账户申请
 */
export async function createLiveAccount(
  params: CreateLiveAccountParams
): Promise<ActionResponse<{ id: number }>> {
  try {
    const response = await apiClient.v1.post<{ data: { id: number } }>(
      '/client/application/trade-account',
      params
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to create live account' };
  }
}

/**
 * 创建模拟账户
 */
export async function createDemoAccount(
  params: CreateDemoAccountParams
): Promise<ActionResponse<DemoAccount>> {
  try {
    const response = await apiClient.v1.post<{ data: DemoAccount }>(
      '/client/trade-demo-account',
      params
    );
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to create demo account' };
  }
}

/**
 * 获取账户杠杆配置
 * GET /api/v2/client/account/{accountNumber}/config
 */
export async function getAccountLeverageConfig(
  accountNumber: number
): Promise<ActionResponse<{ leverages: number[]; currentLeverage?: number }>> {
  try {
    const response = await apiClient.v2.get<{ data: { leverageAvailable: number[]; leverage?: number } }>(
      `/client/account/${accountNumber}/config`
    );
    return {
      success: true,
      data: {
        leverages: response.data?.leverageAvailable || [],
        currentLeverage: response.data?.leverage,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch leverage config' };
  }
}

/**
 * 修改账户杠杆
 * POST /api/v1/client/application/change-leverage
 */
export async function changeLeverage(
  accountUid: number,
  accountNumber: number,
  leverage: number
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.post<void>(
      '/client/application/change-leverage',
      { accountUid, accountNumber, leverage }
    );
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to change leverage' };
  }
}

/**
 * 请求重置交易账户密码
 * POST /api/v1/client/application/change-password
 */
export async function requestPasswordReset(
  accountUid: number,
  accountNumber: number,
  callbackUrl: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.post<void>(
      '/client/application/change-password',
      { accountUid, accountNumber, callbackUrl }
    );
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to request password reset' };
  }
}

/**
 * 根据 accountNumber 获取账户信息
 */
export async function getAccountByNumber(
  accountNumber: number
): Promise<ActionResponse<Account | null>> {
  try {
    const response = await apiClient.v1.get<{ data: Account[] }>(
      `/client/account?hasTradeAccount=true&status=0&accountNumber=${accountNumber}`
    );
    const accounts = response.data || [];
    const account = accounts.find(
      (a) => a.tradeAccount?.accountNumber === accountNumber
    );
    return { success: true, data: account || null };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch account' };
  }
}

/**
 * 获取交易账户转账记录
 * GET /api/v1/client/trade-account/{uid}/transaction
 */
export async function getAccountTransactions(
  tradeAccountUid: number,
  params?: TransactionQueryParams
): Promise<ActionResponse<PaginatedResponse<AccountTransaction>>> {
  try {
    const queryParams = new URLSearchParams();
    if (params?.page !== undefined) queryParams.append('page', String(params.page));
    if (params?.size !== undefined) queryParams.append('size', String(params.size));
    if (params?.period) queryParams.append('period', params.period);
    if (params?.type !== undefined) queryParams.append('type', String(params.type));

    const queryString = queryParams.toString();
    const url = `/client/trade-account/${tradeAccountUid}/transaction${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.v1.get<{
      data: AccountTransaction[];
      total: number;
      page: number;
      size: number;
    }>(url);

    const normalized = normalizeAmountList(
      response.data || [],
      ['amountInCents', 'amount']
    ) as AccountTransaction[];

    return {
      success: true,
      data: {
        data: normalized,
        total: response.total || 0,
        page: response.page || 0,
        size: response.size || 20,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch transactions' };
  }
}

/**
 * 获取交易账户入金记录
 * GET /api/v2/client/account/{uid}/deposit
 */
export async function getAccountDeposits(
  accountUid: number,
  params?: DepositQueryParams
): Promise<ActionResponse<PaginatedResponse<AccountDeposit>>> {
  try {
    const queryParams = new URLSearchParams();
    if (params?.page !== undefined) queryParams.append('page', String(params.page));
    if (params?.size !== undefined) queryParams.append('size', String(params.size));
    if (params?.period) queryParams.append('period', params.period);
    if (params?.state !== undefined) queryParams.append('depositState', String(params.state));

    const queryString = queryParams.toString();
    const url = `/client/account/${accountUid}/deposit${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.v2.get<{
      data: AccountDeposit[];
      total: number;
      page: number;
      size: number;
    }>(url);

    const normalized = normalizeAmountList(
      response.data || [],
      ['amountInCents', 'amount']
    ) as AccountDeposit[];

    return {
      success: true,
      data: {
        data: normalized,
        total: response.total || 0,
        page: response.page || 0,
        size: response.size || 20,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch deposits' };
  }
}

/**
 * 获取交易账户出金记录
 * GET /api/v2/client/account/{uid}/withdrawal
 */
export async function getAccountWithdrawals(
  accountUid: number,
  params?: WithdrawalQueryParams
): Promise<ActionResponse<PaginatedResponse<AccountWithdrawal>>> {
  try {
    const queryParams = new URLSearchParams();
    if (params?.page !== undefined) queryParams.append('page', String(params.page));
    if (params?.size !== undefined) queryParams.append('size', String(params.size));
    if (params?.period) queryParams.append('period', params.period);
    if (params?.state !== undefined) queryParams.append('withdrawalState', String(params.state));

    const queryString = queryParams.toString();
    const url = `/client/account/${accountUid}/withdrawal${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.v2.get<{
      data: AccountWithdrawal[];
      total: number;
      page: number;
      size: number;
    }>(url);

    const normalized = normalizeAmountList(
      response.data || [],
      ['amountInCents', 'amount']
    ) as AccountWithdrawal[];

    return {
      success: true,
      data: {
        data: normalized,
        total: response.total || 0,
        page: response.page || 0,
        size: response.size || 20,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch withdrawals' };
  }
}

/**
 * 获取交易报告
 * GET /api/v1/client/trade-account/{tradeAccountUid}/trade
 */
export async function getAccountTrades(
  tradeAccountUid: number,
  params?: TradeQueryParams
): Promise<ActionResponse<PaginatedResponse<AccountTrade>>> {
  try {
    const queryParams = new URLSearchParams();
    if (params?.page !== undefined) queryParams.append('page', String(params.page));
    if (params?.size !== undefined) queryParams.append('size', String(params.size));
    if (params?.period) queryParams.append('period', params.period);
    if (params?.symbol) queryParams.append('symbol', params.symbol);
    if (params?.isClosed !== undefined) queryParams.append('isClosed', String(params.isClosed));

    const queryString = queryParams.toString();
    const url = `/client/trade-account/${tradeAccountUid}/trade${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.v1.get<{
      data: AccountTrade[];
      total: number;
      page: number;
      size: number;
    }>(url);

    return {
      success: true,
      data: {
        data: response.data || [],
        total: response.total || 0,
        page: response.page || 0,
        size: response.size || 20,
      },
    };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch trades' };
  }
}

/**
 * 取消出金
 * POST /api/v1/client/withdrawal/{hashId}/cancel
 */
export async function cancelAccountWithdrawal(
  hashId: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.post<void>(
      `/client/withdrawal/${hashId}/cancel`
    );
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to cancel withdrawal' };
  }
}

/**
 * 获取入金引导信息
 * GET /api/v2/client/account/{uid}/deposit/{hashId}/guide
 */
export async function getDepositGuide(
  accountUid: number,
  depositHashId: string
): Promise<ActionResponse<{
  paymentMethodName: string;
  platform: number;
  instruction: string;
  info: Record<string, string>;
}>> {
  try {
    const response = await apiClient.v2.get<{
      data: {
        paymentMethodName: string;
        platform: number;
        instruction: string;
        info: Record<string, string>;
      };
    }>(`/client/account/${accountUid}/deposit/${depositHashId}/guide`);
    return { success: true, data: response.data };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch deposit guide' };
  }
}

/**
 * 获取入金收据文件
 * GET /api/v2/client/account/{uid}/deposit/{hashId}/receipt
 */
export async function getDepositReceiptFiles(
  accountUid: number,
  depositHashId: string
): Promise<ActionResponse<string[]>> {
  try {
    const response = await apiClient.v2.get<{ data: string[] }>(
      `/client/account/${accountUid}/deposit/${depositHashId}/receipt`
    );
    return { success: true, data: response.data || [] };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to fetch receipt files' };
  }
}

/**
 * 更新账户别名
 * PUT /api/v1/user/account/alias
 */
export async function updateAccountAlias(
  uid: number,
  alias: string
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v1.put<void>('/user/account/alias', { uid, alias });
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to update alias' };
  }
}

/**
 * 设置默认父级账户
 * PUT /api/v2/client/account/{uid}/default-parent
 */
export async function updateDefaultParentAccount(
  uid: number
): Promise<ActionResponse<void>> {
  try {
    await apiClient.v2.put<void>(`/client/account/${uid}/default-parent`, {});
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to update default account' };
  }
}

/**
 * 上传入金收据
 * POST /api/v2/client/account/{uid}/deposit/{hashId}/receipt
 */
export async function uploadDepositReceipt(
  accountUid: number,
  depositHashId: string,
  file: { name: string; type: string; data: string }
): Promise<ActionResponse<void>> {
  try {
    const formData = new FormData();
    const binaryData = Buffer.from(file.data, 'base64');
    const blob = new Blob([binaryData], { type: file.type });
    formData.append('file', blob, file.name);

    await apiClient.v2.postFormData<void>(
      `/client/account/${accountUid}/deposit/${depositHashId}/receipt`,
      formData
    );
    return { success: true };
  } catch (error) {
    if (error instanceof ApiError) {
      return { success: false, error: error.message, errorCode: error.errorCode };
    }
    return { success: false, error: 'Failed to upload receipt' };
  }
}
