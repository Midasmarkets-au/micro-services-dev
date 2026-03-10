'use client';

import { type ReactNode } from 'react';
import type { Role, Permission } from '@/types/auth';
import { 
  hasAnyPermissionWithRoles, 
  hasRoleLevelWithRoles, 
  hasAnyRole,
  normalizeRole 
} from './permissions';

/**
 * 权限守卫 Props
 */
interface PermissionGuardProps {
  /** 用户角色数组 */
  roles: string[];
  /** 需要的权限（单个或多个，满足任一即可） */
  permission: Permission | Permission[];
  /** 无权限时显示的内容 */
  fallback?: ReactNode;
  /** 有权限时显示的内容 */
  children: ReactNode;
}

/**
 * 角色守卫 Props
 */
interface RoleGuardProps {
  /** 用户角色数组 */
  roles: string[];
  /** 需要的角色（满足任一即可） */
  allowedRoles: string | string[];
  /** 无权限时显示的内容 */
  fallback?: ReactNode;
  /** 有权限时显示的内容 */
  children: ReactNode;
}

/**
 * 角色层级守卫 Props
 */
interface RoleLevelGuardProps {
  /** 用户角色数组 */
  roles: string[];
  /** 需要的最低角色层级 */
  minRole: Role;
  /** 无权限时显示的内容 */
  fallback?: ReactNode;
  /** 有权限时显示的内容 */
  children: ReactNode;
}

/**
 * 权限守卫 HOC
 * 包装组件，只有拥有指定权限的用户才能访问
 */
export function withPermission<P extends object>(
  WrappedComponent: React.ComponentType<P>,
  permission: Permission | Permission[]
) {
  return function WithPermissionComponent(
    props: P & { roles: string[]; fallback?: ReactNode }
  ) {
    const { roles, fallback = null, ...rest } = props;

    if (!roles || roles.length === 0) return fallback;

    const permissions = Array.isArray(permission) ? permission : [permission];
    const hasAccess = hasAnyPermissionWithRoles(roles, permissions);

    if (!hasAccess) return fallback;

    return <WrappedComponent {...(rest as P)} />;
  };
}

/**
 * 角色守卫 HOC
 * 包装组件，只有拥有指定角色的用户才能访问
 */
export function withRole<P extends object>(
  WrappedComponent: React.ComponentType<P>,
  allowedRoles: string | string[]
) {
  return function WithRoleComponent(
    props: P & { roles: string[]; fallback?: ReactNode }
  ) {
    const { roles, fallback = null, ...rest } = props;

    if (!roles || roles.length === 0) return fallback;

    const targetRoles = Array.isArray(allowedRoles) ? allowedRoles : [allowedRoles];
    const hasAccess = hasAnyRole(roles, targetRoles);

    if (!hasAccess) return fallback;

    return <WrappedComponent {...(rest as P)} />;
  };
}

/**
 * 角色层级守卫 HOC
 * 包装组件，只有角色层级达到要求的用户才能访问
 */
export function withRoleLevel<P extends object>(
  WrappedComponent: React.ComponentType<P>,
  minRole: Role
) {
  return function WithRoleLevelComponent(
    props: P & { roles: string[]; fallback?: ReactNode }
  ) {
    const { roles, fallback = null, ...rest } = props;

    if (!roles || roles.length === 0) return fallback;

    const hasAccess = hasRoleLevelWithRoles(roles, minRole);

    if (!hasAccess) return fallback;

    return <WrappedComponent {...(rest as P)} />;
  };
}

/**
 * 权限门控组件
 * 根据权限决定是否渲染子组件
 * 
 * @example
 * ```tsx
 * <PermissionGate roles={user.roles} permission="wallet:view">
 *   <WalletButton />
 * </PermissionGate>
 * 
 * <PermissionGate roles={user.roles} permission={['wallet:view', 'account:view']}>
 *   <ManageButton />
 * </PermissionGate>
 * ```
 */
export function PermissionGate({
  roles,
  permission,
  fallback = null,
  children,
}: PermissionGuardProps) {
  if (!roles || roles.length === 0) return fallback;

  const permissions = Array.isArray(permission) ? permission : [permission];
  const hasAccess = hasAnyPermissionWithRoles(roles, permissions);

  if (!hasAccess) return fallback;

  return <>{children}</>;
}

/**
 * 角色门控组件
 * 根据角色决定是否渲染子组件
 * 
 * @example
 * ```tsx
 * // 只有 Guest 可见
 * <RoleGate roles={user.roles} allowedRoles="Guest">
 *   <VerificationBanner />
 * </RoleGate>
 * 
 * // Client, IB, Sales 可见
 * <RoleGate roles={user.roles} allowedRoles={['Client', 'IB', 'Sales']}>
 *   <WalletButton />
 * </RoleGate>
 * ```
 */
export function RoleGate({
  roles,
  allowedRoles,
  fallback = null,
  children,
}: RoleGuardProps) {
  if (!roles || roles.length === 0) return fallback;

  const targetRoles = Array.isArray(allowedRoles) ? allowedRoles : [allowedRoles];
  const hasAccess = hasAnyRole(roles, targetRoles);

  if (!hasAccess) return fallback;

  return <>{children}</>;
}

/**
 * 角色层级门控组件
 * 根据角色层级决定是否渲染子组件
 * 
 * @example
 * ```tsx
 * // 只有 Client 及以上层级可见
 * <RoleLevelGate roles={user.roles} minRole="Client">
 *   <TradingPanel />
 * </RoleLevelGate>
 * ```
 */
export function RoleLevelGate({
  roles,
  minRole,
  fallback = null,
  children,
}: RoleLevelGuardProps) {
  if (!roles || roles.length === 0) return fallback;

  const hasAccess = hasRoleLevelWithRoles(roles, minRole);

  if (!hasAccess) return fallback;

  return <>{children}</>;
}

/**
 * 便捷组件：仅 Guest 可见
 */
export function GuestOnly({ 
  roles, 
  fallback = null, 
  children 
}: { roles: string[]; fallback?: ReactNode; children: ReactNode }) {
  return (
    <RoleGate roles={roles} allowedRoles="Guest" fallback={fallback}>
      {children}
    </RoleGate>
  );
}

/**
 * 便捷组件：仅认证用户可见（排除 Guest）
 */
export function AuthenticatedOnly({ 
  roles, 
  fallback = null, 
  children 
}: { roles: string[]; fallback?: ReactNode; children: ReactNode }) {
  return (
    <RoleGate roles={roles} allowedRoles={['Client', 'IB', 'Sales', 'Admin']} fallback={fallback}>
      {children}
    </RoleGate>
  );
}

/**
 * 便捷组件：仅 IB 可见
 */
export function IBOnly({ 
  roles, 
  fallback = null, 
  children 
}: { roles: string[]; fallback?: ReactNode; children: ReactNode }) {
  return (
    <RoleGate roles={roles} allowedRoles="IB" fallback={fallback}>
      {children}
    </RoleGate>
  );
}

/**
 * 便捷组件：仅 Sales 可见
 */
export function SalesOnly({ 
  roles, 
  fallback = null, 
  children 
}: { roles: string[]; fallback?: ReactNode; children: ReactNode }) {
  return (
    <RoleGate roles={roles} allowedRoles="Sales" fallback={fallback}>
      {children}
    </RoleGate>
  );
}

/**
 * 便捷组件：仅 Admin 可见
 */
export function AdminOnly({ 
  roles, 
  fallback = null, 
  children 
}: { roles: string[]; fallback?: ReactNode; children: ReactNode }) {
  return (
    <RoleGate roles={roles} allowedRoles="Admin" fallback={fallback}>
      {children}
    </RoleGate>
  );
}