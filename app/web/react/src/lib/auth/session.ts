import logger from '@/lib/logger';
import { cache } from 'react';
import { getAuthCookie, getAuthMode } from './cookies';
import { apiClient, ApiError } from '@/lib/api';
import type { User, Role, Permission } from '@/types/auth';

/**
 * 获取当前用户（通过后端 API 验证 token）
 * 使用 React cache 避免同一请求中重复调用（per-request 级别，不跨请求共享）
 */
export const getCurrentUser = cache(async (): Promise<User | null> => {
  const token = await getAuthCookie();
  const authMode = await getAuthMode();
  const shouldTryCookieMode = authMode === 'cookie';

  if (!token && !shouldTryCookieMode) {
    logger.info('[Session] Token 为空，用户未登录');
    return null;
  }

  try {
    logger.info('[Session] 调用后端 /user/me 验证 token');
    // cookie 模式下 token 为 undefined，apiClient.auth.me 会透传 cookie header
    const userInfo = await apiClient.auth.me(token);
    const user: User = {
      id: String(userInfo.uid),
      email: userInfo.email,
      username: userInfo.name || userInfo.email.split('@')[0],
      nickname: userInfo.nativeName,
      avatar: userInfo.avatar,
      role: (userInfo.roles?.[0] as Role) || 'user',
      permissions: (userInfo.permissions as Permission[]) || [],
      createdAt: userInfo.createdOn || new Date().toISOString(),
      updatedAt: userInfo.createdOn || new Date().toISOString(),
    };
    return user;
  } catch (error) {
    logger.error('[Session] 验证用户失败:', error);
    if (error instanceof ApiError && error.statusCode === 401) {
      logger.info('[Session] Token 无效 (401)，用户需重新登录');
    }
    return null;
  }
});

/**
 * 检查是否已认证（只检查 token 是否存在）
 * 注意：这个方法不验证 token 有效性，只检查是否存在
 */
export async function isAuthenticated(): Promise<boolean> {
  const token = await getAuthCookie();
  const authMode = await getAuthMode();
  return !!token || authMode === 'cookie';
}

/**
 * 检查当前用户是否有指定权限
 */
export async function checkPermission(permission: Permission): Promise<boolean> {
  const user = await getCurrentUser();
  if (!user) return false;
  return user.permissions?.includes(permission) || false;
}

/**
 * 检查当前用户角色
 */
export async function checkRole(role: Role): Promise<boolean> {
  const user = await getCurrentUser();
  if (!user) return false;
  return user.role === role;
}
