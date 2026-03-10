'use server';

import { cookies } from 'next/headers';
import { z } from 'zod';
import { apiClient, ApiError } from '@/lib/api/client';
import { clearAuthCookies, setLocaleCookie } from '@/lib/auth/cookies';
import { getCurrentUser } from '@/lib/auth/session';
import { locales } from '@/i18n/config';
import type { ActionResponse } from '@/hooks/useServerAction';

// Cookie 配置
const AUTH_COOKIE_NAME = 'auth-token';
const REFRESH_COOKIE_NAME = 'refresh-token';

const cookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'lax' as const,
  path: '/',
};

// ==================== Schema Definitions ====================

const loginSchema = z.object({
  email: z.string().email('请输入有效的邮箱地址'),
  password: z.string().min(1, '请输入密码'),
  rememberMe: z.boolean().optional().default(false),
  tenantId: z.union([z.string(), z.number()]).optional(),
  twoFaCode: z.string().optional(),
});

const registerSchema = z.object({
  email: z.string().email('请输入有效的邮箱地址'),
  password: z.string().min(8, '密码长度至少8位'),
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  FirstName: z.string().optional(),
  LastName: z.string().optional(),
  phoneNumber: z.string().optional(),
  phoneCode: z.string().optional(),
  phone: z.string().optional(),
  country: z.string().optional(),
  countryCode: z.string().optional(),
  referralCode: z.string().optional(),
  referCode: z.string().optional(),
  confirmUrl: z.string().optional(),
  ccc: z.union([z.string(), z.number()]).optional(),
  currency: z.string().optional(),
  otp: z.union([z.string(), z.number()]).optional(),
  language: z.string().optional(),
  sourceComment: z.string().optional(),
  siteId: z.number().optional(),
  tenantId: z.number().optional(),
  utm: z.string().optional(),
  code: z.string().optional(),
});

const forgotPasswordSchema = z.object({
  email: z.string().email('请输入有效的邮箱地址'),
  resetUrl: z.string().url('请提供有效的重置链接'),
});

const passwordResetSchema = z.object({
  email: z.string().email('请输入有效的邮箱地址'),
  password: z.string().min(8, '密码至少8个字符'),
  code: z.string().min(1, '重置码不能为空'),
});

const resendConfirmationSchema = z.object({
  email: z.string().email('请输入有效的邮箱地址'),
  confirmUrl: z.string().url('请提供有效的确认链接'),
});

const setTokenSchema = z.object({
  token: z.string().min(1, 'Token 不能为空'),
});

const setLocaleSchema = z.object({
  locale: z.enum(locales),
});

// ==================== Types ====================

interface LoginData {
  email: string;
  password: string;
  rememberMe?: boolean;
  tenantId?: string | number;
  twoFaCode?: string;
}

interface LoginResponse {
  user?: {
    id: number;
    email: string;
    username?: string;
    nickname?: string;
    avatar?: string;
    role?: string;
  };
  twoFactorRequired?: boolean;
  hasMultipleTenants?: boolean;
  tenantIds?: number[];
}

type RegisterData = z.infer<typeof registerSchema>;

interface ForgotPasswordData {
  email: string;
  resetUrl: string;
}

interface PasswordResetData {
  email: string;
  password: string;
  code: string;
}

interface ResendConfirmationData {
  email: string;
  confirmUrl: string;
}

// ==================== Actions ====================

/**
 * 登录
 */
export async function login(data: LoginData): Promise<ActionResponse<LoginResponse>> {
  try {
    const validationResult = loginSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: '验证失败',
        message: validationResult.error.flatten().fieldErrors.email?.[0] ||
          validationResult.error.flatten().fieldErrors.password?.[0] ||
          '请检查输入',
      };
    }

    const { email, password, rememberMe, tenantId, twoFaCode } = validationResult.data;

    let backendResponse;
    try {
      backendResponse = await apiClient.auth.login(email, password, {
        tenantId: tenantId?.toString(),
        twoFaCode,
      });
    } catch (error) {
      if (error instanceof ApiError) {
        return {
          success: false,
          error: error.message || '登录失败，请检查邮箱和密码',
          errorCode: error.errorCode,
        };
      }
      throw error;
    }

    // 处理双因素认证（不弹 Toast，由页面 UI 处理）
    if (backendResponse.twoFactorRequired) {
      return {
        success: true,
        twoFactorRequired: true,
        message: 'twoFactorRequired',
        skipToast: true,
      };
    }

    // 处理多租户（不弹 Toast，由页面 UI 处理）
    if (backendResponse.hasMultipleTenants && backendResponse.tenantIds && !tenantId) {
      return {
        success: true,
        hasMultipleTenants: true,
        tenantIds: backendResponse.tenantIds.map((id: string | number) => Number(id)),
        message: 'selectTenantRequired',
        skipToast: true,
      };
    }

    const { user, accessToken, refreshToken } = backendResponse;

    if (!user || !accessToken) {
      return {
        success: false,
        error: '登录失败，用户信息不存在',
      };
    }

    // 设置 Cookie
    const cookieStore = await cookies();
    const maxAge = rememberMe ? 7 * 24 * 60 * 60 : 24 * 60 * 60;

    cookieStore.set(AUTH_COOKIE_NAME, accessToken, {
      ...cookieOptions,
      maxAge,
    });

    if (refreshToken) {
      cookieStore.set(REFRESH_COOKIE_NAME, refreshToken, {
        ...cookieOptions,
        maxAge: 30 * 24 * 60 * 60,
      });
    }

    return {
      success: true,
      data: {
        user: {
          id: Number(user.id),
          email: user.email,
          username: user.username,
          nickname: user.nickname,
          avatar: user.avatar,
          role: user.role,
        },
      },
      message: '登录成功',
    };
  } catch (error) {
    console.error('Login error:', error);
    return {
      success: false,
      error: '服务器错误，请稍后重试',
    };
  }
}

/**
 * 登出
 */
export async function logout(): Promise<ActionResponse> {
  try {
    await clearAuthCookies();
    return {
      success: true,
      message: '退出登录成功',
    };
  } catch (error) {
    console.error('Logout error:', error);
    return {
      success: false,
      error: '退出登录失败',
    };
  }
}

/**
 * 注册
 */
export async function register(data: RegisterData): Promise<ActionResponse> {
  try {
    const validationResult = registerSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: '验证失败',
      };
    }

    const validData = validationResult.data;

    // 标准化参数名称
    const normalizedData = {
      ...validData,
      FirstName: validData.FirstName || validData.firstName,
      LastName: validData.LastName || validData.lastName,
      otp: validData.otp?.toString() || '771578',
      ccc: validData.ccc?.toString(),
    };

    let backendResponse;
    try {
      backendResponse = await apiClient.auth.register(normalizedData);
    } catch (error) {
      if (error instanceof ApiError) {
        return {
          success: false,
          error: error.message || '注册失败，请稍后重试',
          errorCode: error.errorCode,
        };
      }
      throw error;
    }

    return {
      success: true,
      data: backendResponse.data,
      message: backendResponse.message || '注册成功',
    };
  } catch (error) {
    console.error('Register error:', error);
    return {
      success: false,
      error: '服务器错误，请稍后重试',
    };
  }
}

/**
 * 获取当前用户
 */
export async function getMe(): Promise<ActionResponse<{ user: unknown }>> {
  try {
    const user = await getCurrentUser();

    if (!user) {
      return {
        success: false,
        error: '未登录',
      };
    }

    return {
      success: true,
      data: { user },
    };
  } catch (error) {
    console.error('Get current user error:', error);
    return {
      success: false,
      error: '获取用户信息失败',
    };
  }
}

/**
 * 忘记密码
 */
export async function forgotPassword(data: ForgotPasswordData): Promise<ActionResponse> {
  try {
    const validationResult = forgotPasswordSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: '验证失败',
      };
    }

    const { email, resetUrl } = validationResult.data;

    try {
      await apiClient.post('/auth/password/forgot', { email, resetUrl });
    } catch (error) {
      if (error instanceof ApiError) {
        return {
          success: false,
          error: error.message || '发送重置邮件失败',
          errorCode: error.errorCode,
        };
      }
      throw error;
    }

    return {
      success: true,
      message: '重置密码链接已发送到您的邮箱',
    };
  } catch (error) {
    console.error('Forgot password error:', error);
    return {
      success: false,
      error: '服务器错误，请稍后重试',
    };
  }
}

/**
 * 重置密码
 */
export async function resetPassword(data: PasswordResetData): Promise<ActionResponse> {
  try {
    const validationResult = passwordResetSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: '验证失败',
      };
    }

    const { email, password, code } = validationResult.data;

    try {
      await apiClient.post('/auth/password/reset', { email, password, code });
    } catch (error) {
      if (error instanceof ApiError) {
        return {
          success: false,
          error: error.message || '重置密码失败',
          errorCode: error.errorCode,
        };
      }
      throw error;
    }

    return {
      success: true,
      message: '密码重置成功',
    };
  } catch (error) {
    console.error('Password reset error:', error);
    return {
      success: false,
      error: '服务器错误，请稍后重试',
    };
  }
}

/**
 * 重发确认邮件
 */
export async function resendConfirmation(data: ResendConfirmationData): Promise<ActionResponse> {
  try {
    const validationResult = resendConfirmationSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: '验证失败',
      };
    }

    const { email, confirmUrl } = validationResult.data;

    try {
      await apiClient.post('/auth/resend-confirmation', { email, confirmUrl });
    } catch (error) {
      if (error instanceof ApiError) {
        return {
          success: false,
          error: error.message || '发送确认邮件失败',
          errorCode: error.errorCode,
        };
      }
      throw error;
    }

    return {
      success: true,
      message: '确认邮件已重新发送',
    };
  } catch (error) {
    console.error('Resend confirmation error:', error);
    return {
      success: false,
      error: '服务器错误，请稍后重试',
    };
  }
}

/**
 * 设置 Token（用于后端生成的登录链接）
 */
export async function setToken(data: { token: string }): Promise<ActionResponse> {
  try {
    const validationResult = setTokenSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: 'Token 不能为空',
      };
    }

    const { token } = validationResult.data;
    const cookieStore = await cookies();

    cookieStore.set(AUTH_COOKIE_NAME, token, {
      ...cookieOptions,
      maxAge: 7 * 24 * 60 * 60,
    });

    return {
      success: true,
      message: 'Token 设置成功',
    };
  } catch (error) {
    console.error('Set token error:', error);
    return {
      success: false,
      error: '服务器错误，请稍后重试',
    };
  }
}

/**
 * 设置语言
 */
export async function setLocale(data: { locale: string }): Promise<ActionResponse> {
  try {
    const validationResult = setLocaleSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: '无效的语言设置',
      };
    }

    const { locale } = validationResult.data;
    await setLocaleCookie(locale);

    return {
      success: true,
      message: '语言设置已更新',
    };
  } catch (error) {
    console.error('Set locale error:', error);
    return {
      success: false,
      error: '设置语言失败',
    };
  }
}

/**
 * 获取站点配置
 */
export async function getSiteConfig(openAt: string = 'bvi'): Promise<ActionResponse<number[]>> {
  try {
    const response = await apiClient.get<{ data: number[] }>(`/auth/c?openAt=${openAt}`);
    return {
      success: true,
      data: response.data || [0],
    };
  } catch (error) {
    console.error('Get site config error:', error);
    // 返回默认值，不阻塞页面
    return {
      success: true,
      data: [0],
    };
  }
}
