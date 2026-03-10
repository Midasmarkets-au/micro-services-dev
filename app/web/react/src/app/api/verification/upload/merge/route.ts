import { NextRequest, NextResponse } from 'next/server';
import { getAuthCookie } from '@/lib/auth/cookies';
import { apiClient, ApiError } from '@/lib/api';

interface MergeResponse {
  guid?: string;
  id?: string;
}

// POST /api/verification/upload/merge - 合并文件切片
export async function POST(request: NextRequest) {
  try {
    const token = await getAuthCookie();
    
    if (!token) {
      return NextResponse.json(
        { success: false, message: 'Unauthorized' },
        { status: 401 }
      );
    }

    const formData = await request.formData();

    // 使用 apiClient v2 版本合并文件切片
    const data = await apiClient.v2.postFormData<MergeResponse>('/client/media/upload/merge', formData, token);

    return NextResponse.json({ 
      success: true, 
      data,
      guid: data.guid || data.id 
    });
  } catch (error) {
    console.error('Merge upload error:', error);
    
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
