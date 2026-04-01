import { NextRequest, NextResponse } from 'next/server';
import { apiClient, ApiError } from '@/lib/api';

// POST /api/verification/upload/chunk - 上传文件切片
export async function POST(request: NextRequest) {
  try {
    const formData = await request.formData();

    // 统一走 apiClient 的鉴权逻辑：
    // - token 模式：读取 auth-token
    // - cookie 模式：读取 access_token / auth-mode
    const data = await apiClient.v2.postFormData('/client/media/upload/chunk', formData);

    return NextResponse.json({ success: true, data });
  } catch (error) {
    console.error('Chunk upload error:', error);
    
    if (error instanceof ApiError) {
      return NextResponse.json(
        { success: false, message: error.message },
        { status: error.statusCode }
      );
    }
    
    return NextResponse.json(
      { success: false, message: 'Internal server error' },
      { status: 500 }
    );
  }
}
