/**
 * 用户角色类型（匹配后端返回的角色）
 * - Guest: 游客，未完成认证
 * - Client: 普通客户，已完成认证
 * - IB: 介绍经纪人 (Introducing Broker)
 * - Sales: 销售人员
 * - Admin: 管理员
 */
export type Role = 'Guest' | 'Client' | 'IB' | 'Sales' | 'Admin';

// 所有可能的角色（用于类型守卫）
export const ALL_ROLES: Role[] = ['Guest', 'Client', 'IB', 'Sales', 'Admin'];

// 权限操作类型
export type Permission = 
  // 账户相关权限
  | 'account:view'
  | 'account:manage'
  // 钱包权限
  | 'wallet:view'
  | 'wallet:deposit'
  | 'wallet:withdraw'
  | 'wallet:transfer'
  // 交易权限
  | 'trading:view'
  | 'trading:execute'
  // 认证权限
  | 'verification:view'
  | 'verification:submit'
  // IB 权限
  | 'ib:view'
  | 'ib:manage'
  // 销售权限
  | 'sales:view'
  | 'sales:manage'
  // 管理权限
  | 'admin:view'
  | 'admin:manage';

// 用户信息（兼容旧接口）
export interface User {
  id: string;
  email: string;
  username: string;
  nickname?: string;
  avatar?: string;
  role: Role;           // 主要角色（兼容）
  roles?: Role[];       // 角色数组（新增）
  permissions?: Permission[];
  createdAt: string;
  updatedAt: string;
}

// JWT Payload
export interface JWTPayload {
  userId: string;
  email: string;
  role: Role;
  permissions: Permission[];
  iat: number;
  exp: number;
}

// 登录请求
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

// 登录响应
export interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken?: string;
}

// 认证状态
export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

