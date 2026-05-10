import { NextRequest, NextResponse } from 'next/server';
import { apiClient, ApiError } from '@/lib/api/client';
import type { ApiVersion } from '@/lib/api/client';

/**
 * 通用只读 API 代理 Route Handler
 *
 * 用途：Client Components 通过此代理调用后端 GET 接口，完全绕开 Server Action 机制，
 * 从而避免 Server Action 触发 RSC 导航的 Bug。
 *
 * 用法：
 *   GET /api/proxy/user/me            → 代理到 apiClient.v1.get('/user/me')
 *   GET /api/proxy/client/account?roles=100  → 代理到 apiClient.v1.get('/client/account?roles=100')
 *   GET /api/proxy/some/path?_v=v2   → 使用 v2 版本
 *
 * 安全：
 *   - 仅支持 GET（只读），不代理任何写操作
 *   - auth 由 client.ts 内部处理（读取 cookie / Bearer token）
 *   - 未认证请求会被 client.ts 的 resolveAuthTokenForEndpoint 拦截并返回 401
 */
export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
) {
  const { path } = await params;
  const endpoint = '/' + path.join('/');

  const searchParams = new URLSearchParams(request.nextUrl.searchParams);

  // 支持通过 _v 参数指定 API 版本，默认 v1
  const version = (searchParams.get('_v') || 'v1') as ApiVersion;
  searchParams.delete('_v');

  const queryString = searchParams.toString();
  const fullEndpoint = queryString ? `${endpoint}?${queryString}` : endpoint;

  try {
    const data =
      version === 'v2'
        ? await apiClient.v2.get(fullEndpoint)
        : await apiClient.v1.get(fullEndpoint);

    return NextResponse.json({ success: true, data });
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
      { success: false, error: 'Request failed', errorCode: 'networkError' },
      { status: 500 }
    );
  }
}
