import type { Role, Permission } from '@/types/auth';

/**
 * 角色对应的权限映射
 * 
 * 角色说明：
 * - Guest: 游客，未完成认证，只能查看认证页面
 * - Client: 普通客户，可以进行交易、出入金等操作
 * - IB: 介绍经纪人，可以查看下级客户、佣金等
 * - Sales: 销售人员，可以管理客户
 * - Admin: 管理员，拥有所有权限
 */
export const rolePermissions: Record<Role, Permission[]> = {
  Guest: [
    // 游客只能查看和提交认证
    'verification:view',
    'verification:submit',
  ],
  Client: [
    // 普通客户权限
    'account:view',
    'account:manage',
    'wallet:view',
    'wallet:deposit',
    'wallet:withdraw',
    'wallet:transfer',
    'trading:view',
    'trading:execute',
    'verification:view',
  ],
  IB: [
    // IB 拥有 Client 的所有权限 + IB 专属权限
    'account:view',
    'account:manage',
    'wallet:view',
    'wallet:deposit',
    'wallet:withdraw',
    'wallet:transfer',
    'trading:view',
    'trading:execute',
    'verification:view',
    'ib:view',
    'ib:manage',
  ],
  Sales: [
    // 销售人员权限
    'account:view',
    'account:manage',
    'wallet:view',
    'wallet:deposit',
    'wallet:withdraw',
    'wallet:transfer',
    'trading:view',
    'trading:execute',
    'verification:view',
    'sales:view',
    'sales:manage',
  ],
  Admin: [
    // 管理员拥有所有权限
    'account:view',
    'account:manage',
    'wallet:view',
    'wallet:deposit',
    'wallet:withdraw',
    'wallet:transfer',
    'trading:view',
    'trading:execute',
    'verification:view',
    'verification:submit',
    'ib:view',
    'ib:manage',
    'sales:view',
    'sales:manage',
    'admin:view',
    'admin:manage',
  ],
};

/**
 * 角色层级（数字越大权限越高）
 * 用于判断角色是否有足够的权限级别
 */
export const roleHierarchy: Record<Role, number> = {
  Guest: 0,
  Client: 1,
  IB: 2,
  Sales: 2,
  Admin: 3,
};

/**
 * 标准化角色名称（不区分大小写）
 */
export function normalizeRole(role: string): Role | null {
  const normalized = role.toLowerCase();
  const roleMap: Record<string, Role> = {
    guest: 'Guest',
    client: 'Client',
    ib: 'IB',
    sales: 'Sales',
    admin: 'Admin',
  };
  return roleMap[normalized] || null;
}

/**
 * 标准化角色数组
 */
export function normalizeRoles(roles: string[]): Role[] {
  return roles
    .map(normalizeRole)
    .filter((role): role is Role => role !== null);
}

/**
 * 检查单个角色是否拥有指定权限
 */
export function hasPermission(role: Role, permission: Permission): boolean {
  return rolePermissions[role]?.includes(permission) ?? false;
}

/**
 * 检查角色数组中是否有任一角色拥有指定权限
 */
export function hasPermissionWithRoles(roles: string[], permission: Permission): boolean {
  const normalizedRoles = normalizeRoles(roles);
  return normalizedRoles.some(role => hasPermission(role, permission));
}

/**
 * 检查角色是否拥有所有指定权限
 */
export function hasAllPermissions(role: Role, permissions: Permission[]): boolean {
  return permissions.every((permission) => hasPermission(role, permission));
}

/**
 * 检查角色数组是否拥有所有指定权限
 */
export function hasAllPermissionsWithRoles(roles: string[], permissions: Permission[]): boolean {
  return permissions.every(permission => hasPermissionWithRoles(roles, permission));
}

/**
 * 检查角色是否拥有任一指定权限
 */
export function hasAnyPermission(role: Role, permissions: Permission[]): boolean {
  return permissions.some((permission) => hasPermission(role, permission));
}

/**
 * 检查角色数组是否拥有任一指定权限
 */
export function hasAnyPermissionWithRoles(roles: string[], permissions: Permission[]): boolean {
  return permissions.some(permission => hasPermissionWithRoles(roles, permission));
}

/**
 * 获取角色的所有权限
 */
export function getRolePermissions(role: Role): Permission[] {
  return rolePermissions[role] ?? [];
}

/**
 * 获取角色数组的所有权限（合并去重）
 */
export function getRolesPermissions(roles: string[]): Permission[] {
  const normalizedRoles = normalizeRoles(roles);
  const allPermissions = normalizedRoles.flatMap(role => getRolePermissions(role));
  return [...new Set(allPermissions)];
}

/**
 * 检查角色层级是否足够
 */
export function hasRoleLevel(userRole: Role, requiredRole: Role): boolean {
  return roleHierarchy[userRole] >= roleHierarchy[requiredRole];
}

/**
 * 检查角色数组中是否有任一角色达到要求的层级
 */
export function hasRoleLevelWithRoles(roles: string[], requiredRole: Role): boolean {
  const normalizedRoles = normalizeRoles(roles);
  return normalizedRoles.some(role => hasRoleLevel(role, requiredRole));
}

/**
 * 检查用户角色数组是否包含指定角色（不区分大小写）
 */
export function hasRole(userRoles: string[], targetRole: string): boolean {
  const normalizedTarget = targetRole.toLowerCase();
  return userRoles.some(role => role.toLowerCase() === normalizedTarget);
}

/**
 * 检查用户角色数组是否包含任一指定角色（不区分大小写）
 */
export function hasAnyRole(userRoles: string[], targetRoles: string[]): boolean {
  return targetRoles.some(target => hasRole(userRoles, target));
}

/**
 * 检查用户是否只有 Guest 角色
 */
export function isGuestOnly(roles: string[]): boolean {
  return roles.length === 1 && hasRole(roles, 'Guest');
}

/**
 * 检查用户是否是 IB
 */
export function isIB(roles: string[]): boolean {
  return hasRole(roles, 'IB');
}

/**
 * 检查用户是否是 Sales
 */
export function isSales(roles: string[]): boolean {
  return hasRole(roles, 'Sales');
}

/**
 * 检查用户是否是 Admin
 */
export function isAdmin(roles: string[]): boolean {
  return hasRole(roles, 'Admin');
}
