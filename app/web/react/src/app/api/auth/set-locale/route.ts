import { NextRequest, NextResponse } from 'next/server';
import { setLocale } from '@/actions/auth';

/**
 * POST /api/auth/set-locale
 *
 * 通过 Route Handler 调用 setLocale，避免在 useEffect 中直接调用 Server Action
 * 时 Next.js 附带的 RSC 重渲染指令导致页面无限刷新的问题。
 */
export async function POST(request: NextRequest) {
  try {
    const body = await request.json() as { locale?: unknown };
    const locale = typeof body.locale === 'string' ? body.locale : '';

    if (!locale) {
      return NextResponse.json({ success: false, error: 'Locale 不能为空' }, { status: 400 });
    }

    const result = await setLocale({ locale });
    return NextResponse.json(result);
  } catch {
    return NextResponse.json({ success: false, error: '服务器错误' }, { status: 500 });
  }
}
