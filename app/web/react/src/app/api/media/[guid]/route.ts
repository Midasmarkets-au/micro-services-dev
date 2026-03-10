import { cookies } from 'next/headers';
import { NextResponse } from 'next/server';
import { API_BASE_URL } from '@/lib/api/client';

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ guid: string }> }
) {
  const { guid } = await params;
  const cookieStore = await cookies();
  const token = cookieStore.get('auth-token')?.value;

  if (!token) {
    return new NextResponse('Unauthorized', { status: 401 });
  }

  if (!guid) {
    return new NextResponse('Missing GUID', { status: 400 });
  }

  try {
    const response = await fetch(
      `${API_BASE_URL}/api/v1/client/media/${guid}`,
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    );

    if (!response.ok) {
      return new NextResponse('Failed to fetch media', { status: response.status });
    }

    const contentType = response.headers.get('content-type') || 'image/jpeg';
    const body = response.body;

    return new NextResponse(body, {
      status: 200,
      headers: {
        'Content-Type': contentType,
        'Cache-Control': 'public, max-age=3600, immutable',
      },
    });
  } catch {
    return new NextResponse('Internal Server Error', { status: 500 });
  }
}
