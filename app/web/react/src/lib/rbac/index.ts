// 权限相关
export {
  rolePermissions,
  roleHierarchy,
  // 标准化
  normalizeRole,
  normalizeRoles,
  // 单角色权限检查
  hasPermission,
  hasAllPermissions,
  hasAnyPermission,
  getRolePermissions,
  hasRoleLevel,
  // 多角色权限检查（推荐使用）
  hasPermissionWithRoles,
  hasAllPermissionsWithRoles,
  hasAnyPermissionWithRoles,
  getRolesPermissions,
  hasRoleLevelWithRoles,
  // 角色检查
  hasRole,
  hasAnyRole,
  isGuestOnly,
  isIB,
  isSales,
  isAdmin,
} from './permissions';

// 守卫组件
export { 
  // HOC
  withPermission, 
  withRole,
  withRoleLevel,
  // 门控组件
  PermissionGate, 
  RoleGate,
  RoleLevelGate,
  // 便捷组件
  GuestOnly,
  AuthenticatedOnly,
  IBOnly,
  SalesOnly,
  AdminOnly,
} from './guards';

// 路由权限配置
export {
  publicRoutes,
  specialRouteConfigs,
  defaultRedirectTo,
  isPublicRoute,
  checkRoutePermission,
  type SpecialRouteConfig,
} from './routeConfig';
