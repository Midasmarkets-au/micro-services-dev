// JWT 工具（保留，可能其他地方用到）
export { signToken, verifyToken, getTokenExpiresMs } from './jwt';

// Cookie 管理（简化版）
export {
  setAuthCookie,
  setRefreshCookie,
  getAuthCookie,
  getRefreshCookie,
  clearAuthCookies,
} from './cookies';

// 会话管理
export {
  getCurrentUser,
  isAuthenticated,
  checkPermission,
  checkRole,
  clearUserCache,
} from './session';

