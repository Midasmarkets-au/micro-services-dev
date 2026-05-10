import { NextResponse } from 'next/server';
import { apiClient, ApiError } from '@/lib/api/client';

/**
 * GET /api/user/config
 *
 * 返回站点公开配置，数据结构与 getConfiguration Server Action 完全相同：
 * { success: true, data: Configuration }
 *
 * Client Components 使用此接口替代 Server Action，避免触发 RSC 导航。
 */
export async function GET() {
  try {
    const response = await apiClient.v1.get<{ data: unknown }>('/configuration/public');
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
      { success: false, error: 'Failed to fetch configuration', errorCode: 'networkError' },
      { status: 500 }
    );
  }
}
