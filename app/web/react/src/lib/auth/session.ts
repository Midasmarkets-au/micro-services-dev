import { cache } from 'react';
import { getAuthCookie, clearAuthCookies } from './cookies';
import { apiClient, ApiError } from '@/lib/api';
import type { User, Role, Permission } from '@/types/auth';

// 用户信息缓存（避免重复请求后端）
let userCache: { user: User | null; token: string | null } = { user: null, token: null };

// 标记 token 是否无效（用于避免重复清除）
let tokenInvalid = false;

/**
 * 获取当前用户（通过后端 API 验证 token）
 * 使用 React cache 避免同一请求中重复调用
 */
export const getCurrentUser = cache(async (): Promise<User | null> => {
  const token = await getAuthCookie();
  
  if (!token) {
    console.log('[Session] Token 为空，用户未登录');
    return null;
  }

  // 如果缓存的 token 匹配，直接返回缓存的用户
  if (userCache.token === token && userCache.user) {
    console.log('[Session] 使用缓存的用户');
    return userCache.user;
  }

  try {
    console.log('[Session] 调用后端 /user/me 验证 token');
    // 调用后端 API 验证 token 并获取用户信息
    const userInfo = await apiClient.auth.me(token);
    console.log('userInfo', userInfo)
    // 从 UserInfo 映射到 User
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

    // 更新缓存，重置无效标记
    userCache = { user, token };
    tokenInvalid = false;
    
    return user;
  } catch (error) {
    console.error('[Session] 验证用户失败:', error);
    
    // Token 无效或过期，清除 cookies
    if (error instanceof ApiError && error.statusCode === 401) {
      console.log('[Session] Token 无效 (401)，清除 cookies');
      if (!tokenInvalid) {
        tokenInvalid = true;
        try {
          await clearAuthCookies();
          console.log('[Session] Cookies 已清除');
        } catch (clearError) {
          console.error('[Session] 清除 cookies 失败:', clearError);
        }
      }
    }
    
    userCache = { user: null, token: null };
    return null;
  }
});

/**
 * 检查是否已认证（只检查 token 是否存在）
 * 注意：这个方法不验证 token 有效性，只检查是否存在
 */
export async function isAuthenticated(): Promise<boolean> {
  const token = await getAuthCookie();
  return !!token;
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

/**
 * 清除用户缓存（登出时调用）
 */
export function clearUserCache(): void {
  userCache = { user: null, token: null };
}

