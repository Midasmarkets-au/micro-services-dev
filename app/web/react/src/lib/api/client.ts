import { cookies } from 'next/headers';
import type { User } from '@/types/auth';
import type { UserInfo } from '@/types/user';
import { syncAuthCookies } from '@/lib/auth/cookies';

// ============================================
// API 配置 - 统一管理（优先读取环境变量）
// ============================================

// 后端 API 基础 URL - 优先读取环境变量
export const API_BASE_URL =
  process.env.API_BASE_URL || 'http://mono:80';

// API 版本定义 - 支持 v1 和 v2 同时存在
export type ApiVersion = 'v1' | 'v2';
export const API_VERSION_V1: ApiVersion = 'v1';
export const API_VERSION_V2: ApiVersion = 'v2';

// 默认 API 版本 - 优先读取环境变量
const envApiVersion = process.env.NEXT_PUBLIC_API_VERSION;
export const DEFAULT_API_VERSION: ApiVersion =
  (envApiVersion === 'v1' || envApiVersion === 'v2') ? envApiVersion : 'v1';

// API 版本前缀 - 保持向后兼容
export const API_VERSION = DEFAULT_API_VERSION;
export const API_PREFIX = `/api/${API_VERSION}`;

// 完整的 API 基础路径
export const API_BASE = `${API_BASE_URL}${API_PREFIX}`;

type BackendRequestAuthMode = 'token' | 'cookie';
const envBackendRequestAuthMode = process.env.BACKEND_REQUEST_AUTH_MODE?.toLowerCase();
export const BACKEND_REQUEST_AUTH_MODE: BackendRequestAuthMode =
  envBackendRequestAuthMode === 'cookie' ? 'cookie' : 'token';

// 获取指定版本的 API 前缀
export const getApiPrefix = (version: ApiVersion = DEFAULT_API_VERSION): string => `/api/${version}`;

// 获取指定版本的完整 API 基础路径
export const getApiBase = (version: ApiVersion = DEFAULT_API_VERSION): string => `${API_BASE_URL}${getApiPrefix(version)}`;

// ============================================
// API 端点配置
// ============================================

// 认证相关
const TOKEN_ENDPOINT = `${API_BASE_URL}/connect/token`; // OAuth2 端点不带版本号
const USER_ME_ENDPOINT = `${API_BASE}/user/me`;
const REGISTER_ENDPOINT = `${API_BASE}/auth/register`;

// API 错误类
export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public errors?: Record<string, string[]>,
    public errorCode?: string
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

// ============================================
// 白名单配置 - 不需要 token 的端点路径
// ============================================
const PUBLIC_ENDPOINTS = [
  '/connect/token',           // 登录
  '/auth/register',           // 注册
  '/auth/password/forgot',    // 忘记密码
  '/auth/password/reset',     // 重置密码
  '/auth/resend-confirmation', // 重新发送确认邮件
  '/auth/c',                  // 获取站点配置
  '/api/app/lead',            // Lead 创建
  '/contact',                 // 联系表单
  '/trade-account/password',  // 交易账户密码修改（通过邮件链接）
];

/**
 * 判断端点是否需要认证
 */
function needsAuth(endpoint: string): boolean {
  return !PUBLIC_ENDPOINTS.some(path => endpoint.includes(path));
}

async function resolveAuthTokenForEndpoint(endpoint: string): Promise<string | undefined> {
  if (!needsAuth(endpoint)) return undefined;

  if (BACKEND_REQUEST_AUTH_MODE === 'cookie') {
    if (await hasCookieAuthContext()) {
      return undefined;
    }
    throw new ApiError('Unauthorized', 401, undefined, 'Unauthorized');
  }

  // token 模式：优先读 auth-token，fallback 读 access_token（cookie 模式写入的 key）
  const token = await getAuthToken();
  if (token) {
    return token;
  }

  throw new ApiError('Unauthorized', 401, undefined, 'Unauthorized');
}

/**
 * 自动从 cookies 获取 token
 * 优先读 auth-token（token 模式），fallback 读 access_token（cookie 模式写入的 key）
 */
async function getAuthToken(): Promise<string | undefined> {
  try {
    const cookieStore = await cookies();
    return cookieStore.get('auth-token')?.value ?? cookieStore.get('access_token')?.value;
  } catch {
    return undefined;
  }
}
// 只透传这些 cookie 到后端，避免把 Next.js 内部 cookie 也发过去
const BACKEND_COOKIE_ALLOWLIST = ['access_token', 'refresh_token', '.AspNetCore.'];

async function getCookieHeader(): Promise<string | undefined> {
  try {
    const cookieStore = await cookies();
    const cookieHeader = cookieStore.getAll()
      .filter(c => BACKEND_COOKIE_ALLOWLIST.some(prefix => c.name.startsWith(prefix)))
      .map(c => `${c.name}=${c.value}`)
      .join('; ');
    return cookieHeader || undefined;
  } catch {
    // 在非服务端环境中可能会失败，返回 undefined
    return undefined;
  }
}

async function hasCookieAuthContext(): Promise<boolean> {
  try {
    const cookieStore = await cookies();
    const allCookies = cookieStore.getAll();
    console.log('[API Client] cookie store keys:', allCookies.map(c => c.name));
    // 有 access_token（后端 session cookie）或 auth-mode=cookie 标记即视为已认证
    const hasAccessToken = allCookies.some(c => c.name === 'access_token');
    const hasCookieMode = allCookies.some(c => c.name === 'auth-mode' && c.value === 'cookie');
    return hasAccessToken || hasCookieMode;
  } catch {
    return false;
  }
}

/**
 * 从 Response 的 Set-Cookie headers 中提取 name=value 对，
 * 拼成可直接用于下一个请求 Cookie header 的字符串
 */
function extractCookiesFromResponse(response: Response): string {
  const extendedHeaders = response.headers as Headers & { getSetCookie?: () => string[] };
  const setCookieHeaders: string[] = typeof extendedHeaders.getSetCookie === 'function'
    ? extendedHeaders.getSetCookie()
    : (response.headers.get('set-cookie') ? [response.headers.get('set-cookie')!] : []);

  return setCookieHeaders
    .map((cookieStr) => {
      const nameValue = cookieStr.split(';')[0];
      return nameValue?.trim() ?? '';
    })
    .filter(Boolean)
    .join('; ');
}

async function shouldSendAuthorization(token?: string): Promise<boolean> {
  if (!token) return false;
  return BACKEND_REQUEST_AUTH_MODE === 'token';
}

// OAuth Token 响应类型
interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
  scope?: string;
  // 2FA 和多租户相关
  twoFactorRequired?: boolean;
  hasMultipleTenants?: boolean;
  tenantIds?: (string | number)[];
}

// 登录选项类型
export interface LoginOptions {
  tenantId?: string;
  twoFaCode?: string;
}

// 用户信息响应类型 - 直接使用后端返回的完整结构
type UserMeResponse = UserInfo;

// 注册请求类型
export interface RegisterRequest {
  email: string;
  password: string;
  confirmUrl?: string;
  ccc?: string | number;
  countryCode?: string;
  currency?: string;
  firstName?: string;
  lastName?: string;
  FirstName?: string;    // 后端格式
  LastName?: string;     // 后端格式
  phone?: string;
  phoneNumber?: string;  // 保留兼容性
  phoneCode?: string;    // 保留兼容性
  otp?: string | number;
  referCode?: string;
  referralCode?: string; // 保留兼容性
  language?: string;
  sourceComment?: string;
  siteId?: number;
  tenantId?: number;
  country?: string;      // 保留兼容性
  utm?: string;          // UTM 追踪参数
  code?: string;         // 推荐码
}

// 注册响应类型
export interface RegisterResponse {
  success: boolean;
  message: string;
  data?: {
    userId?: string;
    email?: string;
  };
}

// 基础请求方法 - 所有响应统一封装成 { data: ... } 格式
async function request<T>(
  url: string,
  options: RequestInit = {},
  extraCookies?: string
): Promise<T> {
  const cookieHeader =
    BACKEND_REQUEST_AUTH_MODE === 'cookie' ? await getCookieHeader() : undefined;

  // 合并 cookie store 中的 cookie 和额外传入的 cookie（如上一个请求的 Set-Cookie）
  const mergedCookieHeader = [cookieHeader, extraCookies].filter(Boolean).join('; ') || undefined;

  const config: RequestInit = {
    ...options,
    headers: {
      ...options.headers,
      ...(mergedCookieHeader ? { Cookie: mergedCookieHeader } : {}),
    },
  };

  const response = await fetch(url, config);
  await syncAuthCookies({ response });

  // 打印请求日志
  const contentType = response.headers.get('content-type');
  const hasJsonBody = response.status !== 204 && contentType?.includes('application/json');

  if (hasJsonBody) {
    // 克隆 response 以便打印内容（body 只能读取一次）
    const responseClone = response.clone();
    try {
      const responseBody = await responseClone.json();
      console.log('[API Client] 请求后端API:', {
        url,
        status: response.status,
        body: responseBody
      });
    } catch {
      console.log('[API Client] 请求后端API:', {
        url,
        status: response.status,
        body: '(JSON 解析失败)'
      });
    }
  } else {
    console.log('[API Client] 请求后端API:', {
      url,
      status: response.status,
      body: response.status === 204 ? '(No Content)' : '(非 JSON 响应)'
    });
  }

  if (!response.ok) {
    let errorMessage = 'Request failed';
    let errors: Record<string, string[]> | undefined;
    let errorCode: string | undefined;

    try {
      const errorData = await response.json();
      console.error('[API Client] 请求失败:', {
        url,
        status: response.status,
        statusText: response.statusText,
        errorData
      });

      // 处理后端直接返回字符串的情况
      // 格式4: "Wallet address already exists" - 纯字符串错误信息
      if (typeof errorData === 'string') {
        errorMessage = errorData;
        // 检查字符串是否是错误码格式
        if (/^__[A-Z_]+__$/.test(errorData)) {
          errorCode = errorData;
        }
      } else {
        // 处理对象格式的错误
        errorMessage = errorData.error_description || errorData.message || errorMessage;
        errors = errorData.errors || errorData;
        // 提取错误码（可能的格式）：
        // 1. { error: '__ERROR_CODE__' } - OAuth 错误
        // 2. { errors: ['__ERROR_CODE__'] } - 验证错误数组
        // 3. { message: '__ERROR_CODE__' } - 后端返回的错误码在 message 字段中
        errorCode = errorData.error || (Array.isArray(errorData.errors) ? errorData.errors[0] : undefined);

        // 如果 message 是错误码格式（以 __ 开头和结尾），也作为 errorCode
        if (!errorCode && errorData.message && /^__[A-Z_]+__$/.test(errorData.message)) {
          errorCode = errorData.message;
        }
      }
    } catch {
      console.error('[API Client] 请求失败 (无法解析错误):', {
        url,
        status: response.status,
        statusText: response.statusText
      });
    }

    // 统一兜底：HTTP 401 一律标准化为 Unauthorized，
    // 便于上层 useServerAction 静默处理并跳转登录页（不弹全局错误 Toast）。
    if (response.status === 401) {
      errorCode = 'Unauthorized';
      if (!errorMessage || errorMessage === 'Request failed') {
        errorMessage = 'Unauthorized';
      }
    }

    throw new ApiError(errorMessage, response.status, errors, errorCode);
  }

  // 检查是否有响应体（204 No Content、空内容、非 JSON 都返回空）
  const contentLength = response.headers.get('content-length');

  if (response.status === 204 || contentLength === '0' || !hasJsonBody) {
    return { data: null } as T;
  }

  const json = await response.json();

  // 统一响应格式：如果响应不包含 data 字段，自动包装成 { data: response }
  // 这样所有地方都可以统一使用 response.data 访问数据
  if (json !== null && typeof json === 'object' && !Array.isArray(json) && 'data' in json) {
    // 已经有 data 字段，直接返回
    return json as T;
  }

  // 没有 data 字段（可能是数组或其他格式），包装成标准格式
  return { data: json } as T;
}

// 带认证的请求方法
async function authenticatedRequest<T>(
  url: string,
  token: string,
  options: RequestInit = {}
): Promise<T> {
  console.log('[API Client] 发送认证请求:', {
    url,
    tokenPrefix: token?.substring(0, 50) + '...',
  });
  

  const sendAuthorization = await shouldSendAuthorization(token);

  return request<T>(url, {
    ...options,
    headers: {
      ...options.headers,
      ...(sendAuthorization ? { Authorization: `Bearer ${token}` } : {}),
    },
  });
}

// 扩展的登录响应类型
interface ExtendedLoginResponse {
  user: User | null;
  accessToken: string;
  refreshToken?: string;
  twoFactorRequired?: boolean;
  hasMultipleTenants?: boolean;
  tenantIds?: (string | number)[];
}

// API 客户端
export const apiClient = {
  auth: {
    // 登录 - 使用 OAuth2 password grant
    async login(email: string, password: string, options?: LoginOptions): Promise<ExtendedLoginResponse> {
      console.log('[API Client] 准备调用 connect/token:', {
        endpoint: TOKEN_ENDPOINT,
        email,
        hasTenantId: !!options?.tenantId,
        has2FA: !!options?.twoFaCode,
      });

      const formData = new URLSearchParams();
      formData.append('client_id', 'api');
      formData.append('grant_type', 'password');
      formData.append('scopes', 'api');
      formData.append('username', email);
      formData.append('password', password);

      // 添加租户ID（如果有）
      if (options?.tenantId) {
        formData.append('tenantId', options.tenantId);
      }

      // 添加2FA验证码（如果有）
      if (options?.twoFaCode) {
        formData.append('tf_code', options.twoFaCode);
      }

      console.log('[API Client] 请求参数:', formData.toString());

      // 直接用原始 fetch 请求 connect/token，以便提取 Set-Cookie 用于后续请求
      const tokenRawResponse = await fetch(TOKEN_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: formData.toString(),
      });

      // 提取 Set-Cookie，用于链式传递给下一个请求
      const tokenSetCookies = extractCookiesFromResponse(tokenRawResponse);
      console.log('[API Client] connect/token Set-Cookie cookies:', tokenSetCookies || '(none)');

      // 同步写入 Next.js cookie store（供后续页面请求使用）
      await syncAuthCookies({ response: tokenRawResponse });

      // 解析 token 响应体
      let tokenData: TokenResponse = {} as TokenResponse;
      try {
        const tokenBody = await tokenRawResponse.json();
        tokenData = tokenBody?.data ?? tokenBody ?? {};
      } catch {
        // ignore parse error
      }

      if (!tokenRawResponse.ok) {
        const errData = tokenData as unknown as Record<string, string>;
        const errorMessage = errData.error_description || errData.message || 'Login failed';
        throw new ApiError(errorMessage, tokenRawResponse.status);
      }

      // 检查是否需要2FA
      if (tokenData.twoFactorRequired) {
        console.log('[API Client] 需要双因素认证');
        return {
          user: null,
          accessToken: '',
          twoFactorRequired: true,
        };
      }

      // 检查是否有多租户
      if (tokenData.hasMultipleTenants && tokenData.tenantIds) {
        console.log('[API Client] 需要选择租户:', tokenData.tenantIds);
        return {
          user: null,
          accessToken: '',
          hasMultipleTenants: true,
          tenantIds: tokenData.tenantIds,
        };
      }

      let userInfoResponse: { data: UserMeResponse };

      if (BACKEND_REQUEST_AUTH_MODE === 'token') {
        // token 模式：用 response body 里的 access_token 作为 Bearer token
        const accessToken = tokenData.access_token;
        if (!accessToken) {
          throw new ApiError('Login failed: no access_token in response', 401);
        }
        console.log('[API Client] Token 获取成功，准备获取用户信息（token 模式，Bearer token）');
        // 同时把 token 写入 cookie store 供后续请求使用
        await syncAuthCookies({ token: accessToken, refreshToken: tokenData.refresh_token, rememberMe: true });
        userInfoResponse = await authenticatedRequest<{ data: UserMeResponse }>(USER_ME_ENDPOINT, accessToken);
      } else {
        // cookie 模式：将 connect/token 返回的 Set-Cookie 直接传给 user/me
        // 因为 Next.js cookie store 在同一请求周期内无法读取刚写入的 cookie
        console.log('[API Client] Token 获取成功，准备获取用户信息（cookie 模式，直接传递 Set-Cookie）');
        userInfoResponse = await request<{ data: UserMeResponse }>(USER_ME_ENDPOINT, {}, tokenSetCookies || undefined);
      }

      const userInfo = userInfoResponse.data;
      console.log('[API Client] 用户信息获取成功:', { userId: userInfo.uid });

      // 构建用户对象（从 UserInfo 映射到 User）
      const user: User = {
        id: String(userInfo.uid),
        email: userInfo.email,
        username: userInfo.name || userInfo.email.split('@')[0],
        nickname: userInfo.nativeName,
        avatar: userInfo.avatar,
        role: (userInfo.roles?.[0] as User['role']) || 'user',
        createdAt: userInfo.createdOn,
        updatedAt: userInfo.createdOn,
      };

      // [已注释] token 模式：从 response body 返回 accessToken / refreshToken
      // return {
      //   user,
      //   accessToken: accessToken || '',
      //   refreshToken: tokenData.refresh_token,
      // };
      return {
        user,
        accessToken: '',
        refreshToken: undefined,
      };
    },

    // 刷新 Token
    async refreshToken(refreshToken: string): Promise<{ accessToken: string; refreshToken?: string }> {
      const formData = new URLSearchParams();
      formData.append('grant_type', 'refresh_token');
      formData.append('refresh_token', refreshToken);
      formData.append('client_id', 'MDM_App');

      const tokenResponse = await request<{ data: TokenResponse }>(TOKEN_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: formData.toString(),
      });

      return {
        accessToken: tokenResponse.data.access_token,
        refreshToken: tokenResponse.data.refresh_token,
      };
    },

    // 获取当前用户信息 - 返回后端原始数据
    async me(token?: string): Promise<UserInfo> {
      const response = token
        ? await authenticatedRequest<{ data: UserMeResponse }>(USER_ME_ENDPOINT, token)
        : await request<{ data: UserMeResponse }>(USER_ME_ENDPOINT);
      return response.data;
    },

    // 注册新用户
    async register(data: RegisterRequest): Promise<RegisterResponse> {
      // 构建手机号：优先使用 phone，其次使用 phoneCode + phoneNumber
      const phone = data.phone || (data.phoneCode && data.phoneNumber ? `${data.phoneCode}${data.phoneNumber}` : undefined);

      // 支持两种命名格式 (firstName/FirstName)
      const firstName = data.FirstName || data.firstName;
      const lastName = data.LastName || data.lastName;

      console.log('[API Client] 准备调用注册接口:', {
        endpoint: REGISTER_ENDPOINT,
        email: data.email,
        FirstName: firstName,
        LastName: lastName,
        phone,
        tenantId: data.tenantId,
      });

      // 构建请求体，只包含有值的字段
      const requestBody: Record<string, unknown> = {
        email: data.email,
        password: data.password,
        FirstName: firstName,
        LastName: lastName,
      };

      // 添加可选字段
      if (data.confirmUrl) requestBody.confirmUrl = data.confirmUrl;
      if (data.ccc) requestBody.ccc = String(data.ccc);
      if (data.countryCode || data.country) requestBody.countryCode = data.countryCode || data.country;
      if (data.currency) requestBody.currency = data.currency;
      if (phone) requestBody.phone = phone;
      if (data.otp) requestBody.otp = String(data.otp);
      if (data.referCode || data.referralCode) requestBody.referCode = data.referCode || data.referralCode;
      if (data.language) requestBody.language = data.language;
      if (data.sourceComment) requestBody.sourceComment = data.sourceComment;
      if (data.siteId !== undefined) requestBody.siteId = data.siteId;
      if (data.tenantId !== undefined) requestBody.tenantId = data.tenantId;
      if (data.utm) requestBody.utm = data.utm;
      if (data.code) requestBody.code = data.code;

      console.log('[API Client] 发送到后端的 requestBody:', JSON.stringify(requestBody, null, 2));

      const response = await request<RegisterResponse>(REGISTER_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody),
      });

      console.log('[API Client] 注册成功');
      return response;
    },
  },

  // ============================================
  // 通用 API 请求方法
  // ============================================

  /**
   * 构建完整的 API URL
   * @param endpoint 端点路径，支持以下格式：
   *   - '/auth/c' -> 自动添加版本前缀 -> '/api/v1/auth/c'（默认）或 '/api/v2/auth/c'
   *   - '/api/v1/auth/c' -> 已有前缀，直接使用
   *   - '/api/v2/auth/c' -> 已有前缀，直接使用
   *   - '/connect/token' -> 不带版本的路径，直接使用
   *   - '/configuration/public' -> 不带版本的路径，直接使用 /api/configuration/public
   * @param version API 版本，默认使用 DEFAULT_API_VERSION (v1)
   */
  buildUrl(endpoint: string, version: ApiVersion = DEFAULT_API_VERSION): string {
    // 如果已经包含 /api/v 前缀，直接使用（忽略传入的 version）
    if (endpoint.startsWith('/api/v')) {
      return `${API_BASE_URL}${endpoint}`;
    }
    // 如果以 /api/ 开头（已包含 /api 前缀），直接使用
    if (endpoint.startsWith('/api/')) {
      return `${API_BASE_URL}${endpoint}`;
    }
    // 如果以 /connect 开头（OAuth 端点），不添加版本前缀
    if (endpoint.startsWith('/connect')) {
      return `${API_BASE_URL}${endpoint}`;
    }
    // 如果以 /configuration 开头，不添加版本前缀（使用 /api/configuration/...）
    if (endpoint.startsWith('/configuration')) {
      return `${API_BASE_URL}/api${endpoint}`;
    }
    // 其他情况，使用指定版本的前缀
    return `${getApiBase(version)}${endpoint}`;
  },

  /**
   * GET 请求
   * @param endpoint API 端点
   * @param token 可选的认证 token
   * @param version 可选的 API 版本，默认 v1
   */
  async get<T>(endpoint: string, token?: string, version?: ApiVersion): Promise<T> {
    const url = this.buildUrl(endpoint, version);
    if (token) {
      return authenticatedRequest<T>(url, token);
    }
    return request<T>(url);
  },

  /**
   * POST 请求
   * @param endpoint API 端点
   * @param data 请求数据
   * @param token 可选的认证 token
   * @param version 可选的 API 版本，默认 v1
   */
  async post<T>(endpoint: string, data: unknown, token?: string, version?: ApiVersion): Promise<T> {
    const url = this.buildUrl(endpoint, version);
    const options: RequestInit = {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    };

    if (token) {
      return authenticatedRequest<T>(url, token, options);
    }
    return request<T>(url, options);
  },

  /**
   * PUT 请求
   * @param endpoint API 端点
   * @param data 请求数据
   * @param token 可选的认证 token
   * @param version 可选的 API 版本，默认 v1
   */
  async put<T>(endpoint: string, data: unknown, token?: string, version?: ApiVersion): Promise<T> {
    const url = this.buildUrl(endpoint, version);
    const options: RequestInit = {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    };

    if (token) {
      return authenticatedRequest<T>(url, token, options);
    }
    return request<T>(url, options);
  },

  /**
   * DELETE 请求
   * @param endpoint API 端点
   * @param token 可选的认证 token
   * @param version 可选的 API 版本，默认 v1
   */
  async delete<T>(endpoint: string, token?: string, version?: ApiVersion): Promise<T> {
    const url = this.buildUrl(endpoint, version);
    const options: RequestInit = {
      method: 'DELETE',
    };

    if (token) {
      return authenticatedRequest<T>(url, token, options);
    }
    return request<T>(url, options);
  },

  /**
   * POST FormData 请求（用于文件上传）
   * @param endpoint API 端点
   * @param formData FormData 对象
   * @param token 可选的认证 token
   * @param version 可选的 API 版本，默认 v1
   */
  async postFormData<T>(endpoint: string, formData: FormData, token?: string, version?: ApiVersion): Promise<T> {
    const url = this.buildUrl(endpoint, version);
    const options: RequestInit = {
      method: 'POST',
      body: formData,
      // 注意：不要设置 Content-Type，让浏览器自动设置 multipart/form-data 和 boundary
    };

    if (token) {
      return authenticatedRequest<T>(url, token, options);
    }
    return request<T>(url, options);
  },

  /**
   * 快捷方法：使用 v1 版本的 API（自动获取 token）
   */
  v1: {
    async get<T>(endpoint: string): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.get<T>(endpoint, token, 'v1');
    },
    async post<T>(endpoint: string, data: unknown): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.post<T>(endpoint, data, token, 'v1');
    },
    async postFormData<T>(endpoint: string, formData: FormData): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.postFormData<T>(endpoint, formData, token, 'v1');
    },
    async put<T>(endpoint: string, data: unknown): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.put<T>(endpoint, data, token, 'v1');
    },
    async delete<T>(endpoint: string): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.delete<T>(endpoint, token, 'v1');
    },
  },

  /**
   * 快捷方法：使用 v2 版本的 API（自动获取 token）
   */
  v2: {
    async get<T>(endpoint: string): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.get<T>(endpoint, token, 'v2');
    },
    async post<T>(endpoint: string, data: unknown): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.post<T>(endpoint, data, token, 'v2');
    },
    async postFormData<T>(endpoint: string, formData: FormData): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.postFormData<T>(endpoint, formData, token, 'v2');
    },
    async put<T>(endpoint: string, data: unknown): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.put<T>(endpoint, data, token, 'v2');
    },
    async delete<T>(endpoint: string): Promise<T> {
      const token = await resolveAuthTokenForEndpoint(endpoint);
      return apiClient.delete<T>(endpoint, token, 'v2');
    },
  },
};

export default apiClient;
