'use server';

import { z } from 'zod';
import { apiClient, ApiError } from '@/lib/api/client';
import type { ActionResponse } from '@/hooks/useServerAction';

// ==================== Schema Definitions ====================

const changePasswordSchema = z.object({
  referenceId: z.string().min(1, 'referenceId is required'),
  partyId: z.string().min(1, 'partyId is required'),
  token: z.string().min(1, 'token is required'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
  tenantId: z.string().min(1, 'tenantId is required'),
});

// ==================== Types ====================

interface ChangePasswordData {
  referenceId: string;
  partyId: string;
  token: string;
  password: string;
  tenantId: string;
}

// ==================== Actions ====================

/**
 * 修改交易账户密码
 */
export async function changeTradeAccountPassword(
  data: ChangePasswordData
): Promise<ActionResponse> {
  try {
    const validationResult = changePasswordSchema.safeParse(data);
    if (!validationResult.success) {
      return {
        success: false,
        error: 'Validation failed',
        message: validationResult.error.flatten().fieldErrors.password?.[0] || '验证失败',
      };
    }

    const { referenceId, partyId, token, password, tenantId } = validationResult.data;

    console.log('[changeTradeAccountPassword] 开始修改交易账户密码:', { tenantId, referenceId, partyId });

    await apiClient.put(`/trade-account/${tenantId}/change-password`, {
      referenceId,
      partyId,
      token,
      password,
    });

    console.log('[changeTradeAccountPassword] 密码修改成功');

    return {
      success: true,
      message: 'Password changed successfully',
    };
  } catch (error) {
    console.error('[changeTradeAccountPassword] 修改密码失败:', error);

    if (error instanceof ApiError) {
      return {
        success: false,
        error: error.message || 'Failed to change password',
        errorCode: error.errorCode,
      };
    }

    return {
      success: false,
      error: 'Internal server error',
    };
  }
}
