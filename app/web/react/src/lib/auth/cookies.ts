import { cookies } from 'next/headers';

// ============================================
// Cookie 配置 - 简化版，只用一个 Token
// ============================================

const AUTH_COOKIE_NAME = 'auth-token';       // 后端 access_token（唯一的认证 token）
const REFRESH_COOKIE_NAME = 'refresh-token'; // 刷新 token

// Cookie 配置
const cookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'lax' as const,
  path: '/',
};

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
 * 设置刷新 Token Cookie
 */
export async function setRefreshCookie(token: string): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.set(REFRESH_COOKIE_NAME, token, {
    ...cookieOptions,
    maxAge: 30 * 24 * 60 * 60, // 30天
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
 * 清除所有认证 Cookie
 */
export async function clearAuthCookies(): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.delete(AUTH_COOKIE_NAME);
  cookieStore.delete(REFRESH_COOKIE_NAME);
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

