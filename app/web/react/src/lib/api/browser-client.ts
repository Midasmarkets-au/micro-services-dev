/**
 * 浏览器端 API 客户端
 *
 * 在 Client Components 中使用此模块代替 Server Actions 进行只读数据拉取，
 * 从根本上避免 Server Actions 触发 RSC 导航的 Bug。
 *
 * - fetchApiRoute：调用有专属 Route Handler 的接口（数据结构与对应 Server Action 完全一致）
 * - proxyGet：通过通用代理调用任意后端 GET 接口
 *
 * 所有函数均返回 ActionResponse<T>，可无缝接入现有的 useServerAction / useApiQuery 工作流。
 */

import type { ActionResponse } from '@/hooks/useServerAction';

type QueryParams = Record<
  string,
  string | number | boolean | string[] | number[] | undefined | null
>;

/**
 * 序列化参数对象为 URLSearchParams，支持数组（重复 key）
 */
function serializeParams(params: QueryParams): string {
  const qs = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    if (Array.isArray(value)) {
      value.forEach((v) => qs.append(key, String(v)));
    } else {
      qs.append(key, String(value));
    }
  });
  return qs.toString();
}

/**
 * 调用 Next.js Route Handler，不触发 RSC 导航。
 *
 * 适用于有专属 Route Handler 的接口（如 /api/user/info、/api/account/live）。
 * 这些 Route Handler 的响应结构与对应 Server Action 完全一致，
 * 调用方无需调整数据访问代码。
 *
 * @param routePath  Route Handler 路径（如 '/api/user/info'）
 * @param params     查询参数（可选）
 */
export async function fetchApiRoute<T>(
  routePath: string,
  params?: QueryParams
): Promise<ActionResponse<T>> {
  try {
    let url = routePath;
    if (params) {
      const str = serializeParams(params);
      if (str) url += (routePath.includes('?') ? '&' : '?') + str;
    }

    const response = await fetch(url);
    const result = (await response.json()) as ActionResponse<T>;

    if (!response.ok || result.success === false) {
      return {
        success: false,
        error: result.error || 'Request failed',
        errorCode: result.errorCode,
        statusCode: result.statusCode ?? response.status,
        skipToast: result.skipToast,
      };
    }

    return result;
  } catch (error) {
    return {
      success: false,
      error: error instanceof Error ? error.message : 'Network error',
      errorCode: 'networkError',
    };
  }
}

/**
 * 通过通用代理调用任意后端 GET 接口，不触发 RSC 导航。
 *
 * 适用于没有专属 Route Handler 的接口。代理 Route Handler 使用 client.ts
 * 转发请求，自动处理认证（Bearer token / cookie 模式）。
 *
 * 注意：返回的 data 是后端原始响应体（通常形如 { data: T, ... }）。
 * 对于 `/api/v1/xxx` 形式的 endpoint，代理会自动处理版本前缀。
 *
 * @param endpoint  后端接口相对路径（如 '/client/account', '/sales/deposit'）
 * @param params    查询参数（可选）
 * @param version   API 版本，默认 'v1'
 */
export async function proxyGet<T>(
  endpoint: string,
  params?: QueryParams,
  version: 'v1' | 'v2' = 'v1'
): Promise<ActionResponse<T>> {
  // 清理 endpoint，确保不包含版本前缀（proxy 路径自行处理）
  const cleanEndpoint = endpoint
    .replace(/^\/api\/v[12]\//, '/')
    .replace(/^\/api\//, '/');

  const allParams: QueryParams = version === 'v2' ? { ...params, _v: 'v2' } : { ...params };

  return fetchApiRoute<T>(`/api/proxy${cleanEndpoint}`, allParams);
}

/**
 * 通过 Action Bridge 在服务端调用只读 Server Action，不触发 RSC 导航。
 *
 * 工作原理：
 *   客户端发起 POST /api/action，服务端直接调用对应的 action 函数，
 *   以普通 JSON 返回结果，不附带 RSC 重渲染指令。
 *
 * 仅限 get* 前缀的只读函数（enforced by allowlist in /api/action/route.ts）。
 *
 * @param actionName  Server Action 函数名（如 'getLiveAccounts'）
 * @param args        传给 action 的参数列表
 */
export async function fetchAction<T>(
  actionName: string,
  ...args: unknown[]
): Promise<ActionResponse<T>> {
  try {
    const response = await fetch('/api/action', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: actionName, args }),
    });

    const result = (await response.json()) as ActionResponse<T>;

    if (!response.ok || result.success === false) {
      return {
        success: false,
        error: result.error || 'Request failed',
        errorCode: result.errorCode,
        statusCode: result.statusCode ?? response.status,
        skipToast: result.skipToast,
      };
    }

    return result;
  } catch (error) {
    return {
      success: false,
      error: error instanceof Error ? error.message : 'Network error',
      errorCode: 'networkError',
    };
  }
}
