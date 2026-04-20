'use client';

/**
 * 浏览器端 HTTP 客户端
 *
 * 通过 /api/backend/[version]/* 透传到真正的后端，所以：
 *   - 支持 AbortSignal（路由切换时可真正中止请求）
 *   - 保留 Cookie 鉴权（由 Route Handler 服务端注入）
 *
 * 返回格式统一为 { success, data, error, errorCode, statusCode, aborted }
 */

export type ApiVersion = 'v1' | 'v2';

export interface BrowserApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  errorCode?: string;
  statusCode?: number;
  /** 请求被 abort（路由切换时常见），调用方一般直接忽略 */
  aborted?: boolean;
}

export interface BrowserRequestOptions {
  signal?: AbortSignal;
  version?: ApiVersion;
  /** HTTP 超时（毫秒）；未传则不设超时 */
  timeoutMs?: number;
  headers?: Record<string, string>;
  /**
   * 如果为 true，直接返回后端原始响应体（不剥离 { data }）。
   * 适合需要 criteria / pagination 字段的接口。
   */
  raw?: boolean;
}

const DEFAULT_VERSION: ApiVersion = 'v1';

function buildUrl(path: string, version: ApiVersion): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return `/api/backend/${version}${normalized}`;
}

function isAbortError(error: unknown): boolean {
  if (!error) return false;
  if (error instanceof DOMException && error.name === 'AbortError') return true;
  if (typeof error === 'object' && error !== null && 'name' in error) {
    return (error as { name?: string }).name === 'AbortError';
  }
  return false;
}

/**
 * 和 server actions 里 unwrapData 语义一致：
 *   - 如果响应是 { data: X } 对象，返回 X
 *   - 否则原样返回
 */
function unwrapData<T>(raw: unknown): T {
  if (
    raw !== null &&
    typeof raw === 'object' &&
    !Array.isArray(raw) &&
    'data' in (raw as Record<string, unknown>)
  ) {
    return (raw as Record<string, unknown>).data as T;
  }
  return raw as T;
}

function extractError(
  raw: unknown,
  status: number
): { error: string; errorCode?: string } {
  let error = 'Request failed';
  let errorCode: string | undefined;

  if (typeof raw === 'string') {
    error = raw || error;
    if (/^__[A-Z_]+__$/.test(raw)) errorCode = raw;
  } else if (raw && typeof raw === 'object') {
    const r = raw as Record<string, unknown>;
    error = String(r.error_description || r.message || r.error || error);
    errorCode = typeof r.errorCode === 'string' ? r.errorCode : undefined;
    if (!errorCode && typeof r.error === 'string') errorCode = r.error;
    if (
      !errorCode &&
      typeof r.message === 'string' &&
      /^__[A-Z_]+__$/.test(r.message)
    ) {
      errorCode = r.message;
    }
  }

  if (status === 401 && !errorCode) errorCode = 'Unauthorized';
  if (status === 403 && !errorCode) errorCode = 'Forbidden';

  return { error, errorCode };
}

async function coreRequest<T>(
  method: string,
  path: string,
  body: unknown,
  options?: BrowserRequestOptions
): Promise<BrowserApiResponse<T>> {
  const version = options?.version ?? DEFAULT_VERSION;
  const url = buildUrl(path, version);

  let timeoutId: ReturnType<typeof setTimeout> | undefined;
  let signal = options?.signal;
  if (options?.timeoutMs && options.timeoutMs > 0) {
    const timeoutCtrl = new AbortController();
    timeoutId = setTimeout(() => timeoutCtrl.abort(), options.timeoutMs);
    // 组合外部 signal 与超时 signal
    if (options.signal) {
      if (options.signal.aborted) {
        timeoutCtrl.abort();
      } else {
        options.signal.addEventListener('abort', () => timeoutCtrl.abort(), {
          once: true,
        });
      }
    }
    signal = timeoutCtrl.signal;
  }

  const headers: Record<string, string> = {
    Accept: 'application/json',
    ...(options?.headers ?? {}),
  };

  let payload: BodyInit | undefined;
  if (body !== undefined && body !== null) {
    if (body instanceof FormData) {
      payload = body;
    } else {
      headers['Content-Type'] = headers['Content-Type'] || 'application/json';
      payload = JSON.stringify(body);
    }
  }

  try {
    const response = await fetch(url, {
      method,
      headers,
      body: payload,
      signal,
      credentials: 'same-origin',
      cache: 'no-store',
    });
    if (timeoutId) clearTimeout(timeoutId);

    if (response.status === 499) {
      return { success: false, aborted: true, statusCode: 499, errorCode: 'aborted' };
    }

    const contentType = response.headers.get('content-type') || '';
    const isJson = contentType.includes('application/json');
    const raw = isJson ? await response.json().catch(() => null) : await response.text();

    if (!response.ok) {
      const errInfo = extractError(raw, response.status);
      return {
        success: false,
        statusCode: response.status,
        error: errInfo.error,
        errorCode: errInfo.errorCode,
      };
    }

    const data = options?.raw ? (raw as T) : unwrapData<T>(raw);
    return { success: true, data, statusCode: response.status };
  } catch (error) {
    if (timeoutId) clearTimeout(timeoutId);
    if (isAbortError(error)) {
      return { success: false, aborted: true, errorCode: 'aborted' };
    }
    console.error('[browserClient] request failed:', url, error);
    return {
      success: false,
      error: error instanceof Error ? error.message : 'Network error',
      errorCode: 'networkError',
    };
  }
}

export const browserClient = {
  get<T>(path: string, options?: BrowserRequestOptions) {
    return coreRequest<T>('GET', path, undefined, options);
  },
  getRaw<T>(path: string, options?: BrowserRequestOptions) {
    return coreRequest<T>('GET', path, undefined, { ...options, raw: true });
  },
  post<T>(path: string, body?: unknown, options?: BrowserRequestOptions) {
    return coreRequest<T>('POST', path, body, options);
  },
  put<T>(path: string, body?: unknown, options?: BrowserRequestOptions) {
    return coreRequest<T>('PUT', path, body, options);
  },
  delete<T>(path: string, options?: BrowserRequestOptions) {
    return coreRequest<T>('DELETE', path, undefined, options);
  },
};
