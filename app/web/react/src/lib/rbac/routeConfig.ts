/**
 * 路由权限配置
 * 采用白名单模式：定义公开路由，其他路由默认需要权限
 */

/**
 * 公开路由配置（Guest 用户也可以访问）
 * 支持精确匹配和前缀匹配
 */
export const publicRoutes: Array<{ path: string; matchType: 'exact' | 'prefix' }> = [
  // 系统页面 - 所有人可访问
  { path: '/403', matchType: 'exact' },  // 无权限页面
  // Profile 模块 - 基础信息页面公开
  { path: '/profile', matchType: 'exact' },
  { path: '/profile/security', matchType: 'exact' },
  // Dashboard 公开
  { path: '/dashboard', matchType: 'exact' },
  { path: '/verification', matchType: 'exact' },
  { path: '/platforms', matchType: 'exact' },
  { path: '/supports', matchType: 'prefix' },
  // prefix 匹配 /supports/notices 及 /supports/notices/*
  // 可以继续添加其他公开路由
];

/**
 * 特殊权限路由配置
 * 用于需要特定角色才能访问的路由（不仅仅是非 Guest）
 * 
 * 优先级规则：
 * 1. exact 匹配优先于 prefix 匹配
 * 2. 配置顺序越靠前优先级越高（建议将 exact 放在 prefix 前面）
 */
export interface SpecialRouteConfig {
  path: string;
  matchType: 'exact' | 'prefix';
  // 允许访问的角色（白名单）
  allowRoles: string[];
  // 重定向目标
  redirectTo: string;
  // 排除的路径（这些路径不受此规则控制，会继续匹配后续规则）
  // 仅对 prefix 匹配有效
  excludePaths?: string[];
}

export const specialRouteConfigs: SpecialRouteConfig[] = [
  // ==================== 精确匹配规则（优先级最高）====================
  
  // Address 页面 - 需要 eventshop 角色
  {
    path: '/profile/address',
    matchType: 'exact',
    allowRoles: ['Eventshop'],
    redirectTo: '/403',
  },

  // ==================== 前缀匹配规则 ====================

  // IB Center - 需要 IB 或 Sales 角色
  {
    path: '/ib',
    matchType: 'prefix',
    allowRoles: ['Ib', 'Sales'],
    redirectTo: '/403',
  },

  // Sales Center - 需要 Sales 角色
  {
    path: '/sales',
    matchType: 'prefix',
    allowRoles: ['Sales'],
    redirectTo: '/403',
  },

  // Rep Center - 需要 Rep 角色
  {
    path: '/rep',
    matchType: 'prefix',
    allowRoles: ['Rep'],
    redirectTo: '/403',
  },
  
  // Supports 模块示例 - 需要 Supports 角色
  // {
  //   path: '/supports',
  //   matchType: 'prefix',
  //   allowRoles: ['Supports'],
  //   redirectTo: '/dashboard',
  //   // 排除某些路径（这些路径不需要 Supports 角色，走默认逻辑）
  //   excludePaths: [
  //     '/supports/public',        // 精确排除
  //     '/supports/help',          // 精确排除
  //   ],
  // },
];

/**
 * 默认重定向目标（权限不足时）
 * 统一跳转到 403 无权限页面
 */
export const defaultRedirectTo = '/403';

/**
 * 检查路由是否匹配（支持排除路径）
 */
function matchPath(
  pathname: string,
  path: string,
  matchType: 'exact' | 'prefix',
  excludePaths?: string[]
): boolean {
  // 精确匹配
  if (matchType === 'exact') {
    return pathname === path;
  }

  // 前缀匹配
  if (!pathname.startsWith(path)) {
    return false;
  }

  // 检查是否在排除路径中
  if (excludePaths && excludePaths.length > 0) {
    // 检查精确排除
    if (excludePaths.includes(pathname)) {
      return false;
    }
    // 检查前缀排除（排除路径以 / 结尾表示前缀匹配）
    for (const excludePath of excludePaths) {
      if (excludePath.endsWith('/') && pathname.startsWith(excludePath)) {
        return false;
      }
    }
  }

  return true;
}

/**
 * 检查路由是否为公开路由
 */
export function isPublicRoute(pathname: string): boolean {
  return publicRoutes.some((route) => matchPath(pathname, route.path, route.matchType));
}

/**
 * 标准化角色名称（首字母大写，其余小写）
 */
function normalizeRole(role: string): string {
  return role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
}

/**
 * 检查用户是否为 Guest
 */
function isGuest(userRoles: string[]): boolean {
  const normalizedRoles = userRoles.map(normalizeRole);
  return normalizedRoles.includes('Guest');
}

/**
 * 根据用户角色检查是否有权限访问路由
 * @param pathname 当前路由路径
 * @param userRoles 用户角色列表
 * @returns 如果需要重定向，返回重定向路径；否则返回 null
 */
export function checkRoutePermission(
  pathname: string,
  userRoles: string[]
): string | null {
  const normalizedRoles = userRoles.map(normalizeRole);
  const userIsGuest = normalizedRoles.includes('Guest');

  // 1. 检查是否为公开路由 - 所有人都可以访问
  if (isPublicRoute(pathname)) {
    return null;
  }

  // 2. 检查是否匹配特殊权限路由（按配置顺序，先匹配先生效）
  for (const config of specialRouteConfigs) {
    if (matchPath(pathname, config.path, config.matchType, config.excludePaths)) {
      // Guest 用户直接拒绝
      if (userIsGuest) {
        return config.redirectTo;
      }
      // 检查是否有特定角色
      const hasAllowedRole = config.allowRoles.some((role) =>
        normalizedRoles.includes(role)
      );
      if (!hasAllowedRole) {
        return config.redirectTo;
      }
      // 有权限访问
      return null;
    }
  }

  // 3. 非公开路由，Guest 用户默认拒绝
  if (userIsGuest) {
    return defaultRedirectTo;
  }

  // 4. 非 Guest 用户可以访问其他路由
  return null;
}

// 导出辅助函数供外部使用
export { isGuest, normalizeRole };
