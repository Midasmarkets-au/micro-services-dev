import { NextResponse } from 'next/server';
import {
  API_BASE_URL,
  BACKEND_REQUEST_AUTH_MODE,
} from '@/lib/api/client';
import { getAuthCookie, syncAuthCookies } from '@/lib/auth/cookies';
import { cookies } from 'next/headers';

export const dynamic = 'force-dynamic';

/**
 * 透传 Route Handler
 *
 * 作用：
 *   浏览器端的 fetch('/api/backend/v1/xxx') 透传到后端
 *   ${API_BASE_URL}/api/v1/xxx。好处是：
 *   1) 浏览器 fetch 支持 AbortSignal，路由切换时可真正中止网络请求
 *   2) 鉴权在 Route Handler 服务端完成，auth-token / session cookie 不暴露到前端
 *   3) 请求/响应都经过 Next.js 框架，同源，不受 CORS 限制
 *
 * 请求路径规范：
 *   /api/backend/v1/<backendPath>
 *   /api/backend/v2/<backendPath>
 */

type Ctx = {
  params: Promise<{ path: string[] }>;
};

const FORWARDABLE_REQUEST_HEADERS = [
  'accept',
  'accept-language',
  'content-type',
  'x-request-id',
  'x-idempotency-key',
];

function buildTargetUrl(pathSegments: string[], search: string): string {
  const [version, ...rest] = pathSegments;
  const safeVersion = version === 'v1' || version === 'v2' ? version : 'v1';
  const restPath = rest.join('/');
  return `${API_BASE_URL}/api/${safeVersion}/${restPath}${search}`;
}

function pickRequestHeaders(request: Request): Headers {
  const headers = new Headers();
  FORWARDABLE_REQUEST_HEADERS.forEach((name) => {
    const value = request.headers.get(name);
    if (value) headers.set(name, value);
  });
  return headers;
}

async function attachAuth(headers: Headers, extraCookies?: string[]): Promise<void> {
  if (BACKEND_REQUEST_AUTH_MODE === 'cookie') {
    const cookieStore = await cookies();
    const cookieHeader = cookieStore
      .getAll()
      .map((c) => `${c.name}=${c.value}`)
      .join('; ');
    const merged = [cookieHeader, ...(extraCookies ?? [])].filter(Boolean).join('; ');
    if (merged) headers.set('Cookie', merged);
    // cookie 模式下后端会根据 session cookie 鉴权
    return;
  }

  // token 模式
  const token = await getAuthCookie();
  if (token) headers.set('Authorization', `Bearer ${token}`);
}

async function proxy(request: Request, ctx: Ctx): Promise<Response> {
  const { path } = await ctx.params;
  if (!path || path.length === 0) {
    return NextResponse.json({ error: 'Missing path' }, { status: 400 });
  }

  const url = new URL(request.url);
  const targetUrl = buildTargetUrl(path, url.search);

  const headers = pickRequestHeaders(request);
  await attachAuth(headers);

  const method = request.method.toUpperCase();
  const hasBody = method !== 'GET' && method !== 'HEAD';

  let body: BodyInit | undefined;
  if (hasBody) {
    const contentType = request.headers.get('content-type') || '';
    if (contentType.includes('multipart/form-data')) {
      body = await request.formData();
      headers.delete('content-type'); // 让 fetch 自动带 boundary
    } else if (contentType.includes('application/x-www-form-urlencoded')) {
      body = await request.text();
    } else {
      body = await request.text();
    }
  }

  let backendResponse: Response;
  try {
    backendResponse = await fetch(targetUrl, {
      method,
      headers,
      body,
      // 关键：透传浏览器的 abort 到后端
      signal: request.signal,
      cache: 'no-store',
    });
  } catch (error) {
    // 浏览器 abort 时，fetch 会抛 AbortError，我们也返回 499 让客户端 httpClient 识别
    if (error instanceof DOMException && error.name === 'AbortError') {
      return new NextResponse(null, { status: 499 });
    }
    console.error('[api/backend] upstream fetch failed:', error);
    return NextResponse.json(
      { error: 'Upstream request failed', errorCode: 'networkError' },
      { status: 502 }
    );
  }

  // 同步后端 Set-Cookie（cookie 模式下后端可能续期 session）
  try {
    await syncAuthCookies({ response: backendResponse });
  } catch (error) {
    console.warn('[api/backend] syncAuthCookies failed:', error);
  }

  const responseHeaders = new Headers();
  const contentType = backendResponse.headers.get('content-type');
  if (contentType) responseHeaders.set('content-type', contentType);
  responseHeaders.set('cache-control', 'no-store');

  const arrayBuffer = await backendResponse.arrayBuffer();
  return new NextResponse(arrayBuffer, {
    status: backendResponse.status,
    statusText: backendResponse.statusText,
    headers: responseHeaders,
  });
}

export async function GET(request: Request, ctx: Ctx) {
  return proxy(request, ctx);
}
export async function POST(request: Request, ctx: Ctx) {
  return proxy(request, ctx);
}
export async function PUT(request: Request, ctx: Ctx) {
  return proxy(request, ctx);
}
export async function PATCH(request: Request, ctx: Ctx) {
  return proxy(request, ctx);
}
export async function DELETE(request: Request, ctx: Ctx) {
  return proxy(request, ctx);
}
