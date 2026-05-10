import { NextResponse } from 'next/server';
import { apiClient, ApiError } from '@/lib/api/client';

/**
 * GET /api/user/info
 *
 * 返回当前登录用户信息，数据结构与 getUserInfo Server Action 完全相同：
 * { success: true, data: UserInfo }
 *
 * Client Components 使用此接口替代 Server Action，避免触发 RSC 导航。
 */
export async function GET() {
  try {
    const response = await apiClient.v1.get<{ data: unknown }>('/user/me');
    return NextResponse.json({ success: true, data: response.data });
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
      { success: false, error: 'Failed to fetch user info', errorCode: 'networkError' },
      { status: 500 }
    );
  }
}
