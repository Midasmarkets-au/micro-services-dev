import { NextRequest, NextResponse } from 'next/server';
import { apiClient, ApiError } from '@/lib/api/client';

/**
 * GET /api/account/live
 *
 * 返回当前用户的真实账户列表，数据结构与 getLiveAccounts Server Action 完全相同：
 * { success: true, data: Account[] }
 *
 * 支持所有 getLiveAccounts 的 query params（roles, uids 等）：
 *   GET /api/account/live?roles=100
 *   GET /api/account/live?roles=110
 *   GET /api/account/live?uids=123&uids=456
 *
 * Client Components 使用此接口替代 Server Action，避免触发 RSC 导航。
 */
export async function GET(request: NextRequest) {
  const searchParams = request.nextUrl.searchParams;
  const queryString = searchParams.toString();
  const endpoint = queryString ? `/client/account?${queryString}` : '/client/account';

  try {
    const response = await apiClient.v1.get<{
      data: unknown[] | { data: unknown[] };
    }>(endpoint);

    const rawData = response.data;
    const accounts: unknown[] = Array.isArray(rawData)
      ? rawData
      : Array.isArray((rawData as { data?: unknown[] })?.data)
        ? (rawData as { data: unknown[] }).data
        : [];

    return NextResponse.json({ success: true, data: accounts });
  } catch (error) {
    if (error instanceof ApiError) {
      return NextResponse.json(
        {
          success: false,
          error: error.message,
          errorCode: error.errorCode,
          statusCode: error.statusCode,
        },
        { status: error.statusCode || 500 }
      );
    }
    return NextResponse.json(
      { success: false, error: 'Failed to fetch accounts', errorCode: 'networkError' },
      { status: 500 }
    );
  }
}
