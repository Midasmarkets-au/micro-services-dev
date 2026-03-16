import { cookies } from 'next/headers';

// ============================================
// Cookie 配置 - 简化版，只用一个 Token
// ============================================

const AUTH_COOKIE_NAME = 'auth-token';       // 后端 access_token（唯一的认证 token）
const REFRESH_COOKIE_NAME = 'refresh-token'; // 刷新 token
const AUTH_MODE_COOKIE_NAME = 'auth-mode';   // 认证模式：token / cookie

// Cookie 配置
const cookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'lax' as const,
  path: '/',
};

type CookieSameSite = 'strict' | 'lax' | 'none';

interface SyncAuthCookiesOptions {
  response?: Response;
  setCookieHeaders?: string[];
  token?: string;
  refreshToken?: string;
  rememberMe?: boolean;
}

export type AuthMode = 'token' | 'cookie';

function normalizeSameSite(value: unknown): CookieSameSite {
  const sameSite = String(value ?? '').toLowerCase();
  if (sameSite === 'strict' || sameSite === 'none') return sameSite;
  return 'lax';
}

function extractSetCookieHeaders(response: Response): string[] {
  const extendedHeaders = response.headers as Headers & { getSetCookie?: () => string[] };
  if (typeof extendedHeaders.getSetCookie === 'function') {
    return extendedHeaders.getSetCookie();
  }

  const merged = response.headers.get('set-cookie');
  return merged ? [merged] : [];
}

/**
 * 设置认证 Cookie（存储后端的 access_token）
 */
export async function setAuthCookie(token: string, rememberMe = false): Promise<void> {
  const cookieStore = await cookies();
  const maxAge = rememberMe ? 7 * 24 * 60 * 60 : 24 * 60 * 60; // 7天或1天

  cookieStore.set(AUTH_COOKIE_NAME, token, {
    ...cookieOptions,
    maxAge,
  });
}

/**
 * 统一同步认证 Cookie：
 * 1) 传 token 时，直接写入 auth-token；
 * 2) 传 setCookieHeaders 时，解析并回写所有 Set-Cookie。
 */
export async function syncAuthCookies({
  response,
  setCookieHeaders = [],
  token,
  refreshToken,
  rememberMe = true,
}: SyncAuthCookiesOptions): Promise<void> {
  const cookieStore = await cookies();
  
  const cookieHeaders =
    setCookieHeaders.length > 0 ? setCookieHeaders : response ? extractSetCookieHeaders(response) : [];
  const hasSetCookie = cookieHeaders.length > 0;
  // 规则：
  // 1) 若后端返回 Set-Cookie，优先认为是 cookie 模式（不再依赖 token）
  // 2) 若没有 Set-Cookie 且有 token，走 token 模式
  if (hasSetCookie) {
    cookieStore.set(AUTH_MODE_COOKIE_NAME, 'cookie', cookieOptions);
  } else if (token) {
    const maxAge = rememberMe ? 7 * 24 * 60 * 60 : 24 * 60 * 60;
    cookieStore.set(AUTH_COOKIE_NAME, token, {
      ...cookieOptions,
      maxAge,
    });
    cookieStore.set(AUTH_MODE_COOKIE_NAME, 'token', cookieOptions);
  }

  if (refreshToken) {
    cookieStore.set(REFRESH_COOKIE_NAME, refreshToken, {
      ...cookieOptions,
      maxAge: 30 * 24 * 60 * 60,
    });
  }

  if (cookieHeaders.length === 0) return;

  cookieHeaders.forEach((cookieStr) => {
    const [nameValue = '', ...attrs] = cookieStr.split('; ');
    const [name = '', ...valueParts] = nameValue.split('=');
    const value = valueParts.join('=');
    if (!name || !value) return;

    const attrsObj = Object.fromEntries(
      attrs.map((a) => {
        const [k, ...vParts] = a.split('=');
        return [k.toLowerCase(), vParts.length ? vParts.join('=') : true];
      })
    );

    const maxAge =
      typeof attrsObj['max-age'] === 'string' ? Number(attrsObj['max-age']) : undefined;
    const expires =
      typeof attrsObj.expires === 'string' ? new Date(attrsObj.expires) : undefined;

    cookieStore.set(name, value, {
      httpOnly: 'httponly' in attrsObj,
      secure: 'secure' in attrsObj,
      path: (attrsObj.path as string) || '/',
      domain: attrsObj.domain as string | undefined,
      maxAge: Number.isFinite(maxAge) ? maxAge : undefined,
      expires: expires && !Number.isNaN(expires.getTime()) ? expires : undefined,
      sameSite: normalizeSameSite(attrsObj.samesite),
    });
  });
}

/**
 * 获取认证 Cookie（后端 access_token）
 * 用于：请求后端 API、验证用户登录状态
 */
export async function getAuthCookie(): Promise<string | undefined> {
  const cookieStore = await cookies();
  const token = cookieStore.get(AUTH_COOKIE_NAME)?.value;
  return token;
}

/**
 * 获取刷新 Token Cookie
 */
export async function getRefreshCookie(): Promise<string | undefined> {
  const cookieStore = await cookies();
  return cookieStore.get(REFRESH_COOKIE_NAME)?.value;
}

/**
 * 获取当前认证模式
 */
export async function getAuthMode(): Promise<AuthMode | undefined> {
  const cookieStore = await cookies();
  const mode = cookieStore.get(AUTH_MODE_COOKIE_NAME)?.value;
  return mode === 'cookie' || mode === 'token' ? mode : undefined;
}

/**
 * 清除所有认证 Cookie
 */
export async function clearAuthCookies(): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.delete(AUTH_COOKIE_NAME);
  cookieStore.delete(REFRESH_COOKIE_NAME);
  cookieStore.delete(AUTH_MODE_COOKIE_NAME);
}

// 设置语言 Cookie
export async function setLocaleCookie(locale: string): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.set('locale', locale, {
    ...cookieOptions,
    httpOnly: false, // 允许客户端读取
    maxAge: 365 * 24 * 60 * 60, // 1年
  });
}

