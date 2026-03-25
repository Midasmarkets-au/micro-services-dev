import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// 需要认证的路径
const protectedPaths = ['/dashboard', '/admin', '/settings', '/profile', '/verification'];

// 公开路径（不需要认证）
const publicPaths = ['/sign-in', '/sign-up', '/forgot-password', '/reset-password', '/set-token', '/login', '/register', '/lead-create', '/change-account-password'];

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // 跳过静态资源
  if (
    pathname.startsWith('/_next') ||
    pathname.startsWith('/static') ||
    pathname.startsWith('/api') ||
    pathname.includes('.')
  ) {
    return NextResponse.next();
  }

  // 获取 token（后端的 access_token）
  const token = request.cookies.get('auth-token')?.value;
  const authMode = request.cookies.get('auth-mode')?.value;

  // 兼容两种模式：token 模式或后端 cookie 模式
  // 真正有效性验证在 layout.tsx 中通过后端 API 完成
  const hasToken = !!token || authMode === 'cookie';

  // 检查是否访问受保护的路径
  const isProtectedPath = protectedPaths.some(
    (path) => pathname.startsWith(path)
  );

  // 检查是否访问公开路径
  const isPublicPath = publicPaths.some((path) => pathname.startsWith(path));

  // 未登录用户访问受保护路径，重定向到登录页
  if (isProtectedPath && !hasToken) {
    const loginUrl = new URL('/sign-in', request.url);
    loginUrl.searchParams.set('callbackUrl', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // 已登录用户访问登录/注册页，重定向到仪表盘
  // 但如果带有 expired=true 参数，说明是从 protected 页面因 token 失效而跳转过来的，此时应该清除 cookie 而不是重定向
  // /set-token 路径允许已登录用户访问（用于切换账号）
  if (isPublicPath && hasToken) {
    // /set-token 允许已登录用户访问，用于后端生成的登录链接切换账号
    if (pathname.startsWith('/set-token')) {
      return NextResponse.next();
    }
    
    const expired = request.nextUrl.searchParams.get('expired');
    if (expired === 'true') {
      // Token 已失效，清除 cookie 并允许访问登录页
      const response = NextResponse.next();
      response.cookies.delete('auth-token');
      response.cookies.delete('refresh-token');
      response.cookies.delete('auth-mode');
      return response;
    }
    
    const callbackUrl = request.nextUrl.searchParams.get('callbackUrl');
    return NextResponse.redirect(new URL(callbackUrl || '/dashboard', request.url));
  }

  // 旧路由重定向
  if (pathname === '/login') {
    return NextResponse.redirect(new URL('/sign-in', request.url));
  }
  if (pathname === '/register') {
    return NextResponse.redirect(new URL('/sign-up', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    '/((?!_next/static|_next/image|favicon.ico).*)',
  ],
};
