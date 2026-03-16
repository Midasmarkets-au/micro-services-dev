import { redirect } from 'next/navigation';
import { DashboardHeader } from '@/components/layout';
import { getAuthCookie, getAuthMode } from '@/lib/auth/cookies';
import { UserDataProvider, RouteGuard } from '@/components/providers';

export default async function ProtectedLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // 检查是否有 token 或 cookie 模式（middleware 已经做过一次检查，这里双保险）
  // 真正的 token 验证在 UserDataProvider 中进行
  const token = await getAuthCookie();
  const authMode = await getAuthMode();
  
  // cookie 模式下没有 auth-token，但 auth-mode=cookie 表示已登录
  if (!token && authMode !== 'cookie') {
    redirect('/sign-in');
  }
  
  // 注意：不再调用 getCurrentUser() 验证 token
  // UserDataProvider 会获取用户信息，如果 token 无效会处理重定向

  return (
    <div className="relative flex min-h-screen flex-col overflow-x-hidden bg-background">
      {/* 顶部导航 - 所有受保护页面共享 */}
      <DashboardHeader />

      {/* 主内容区域 - 使用 container-responsive */}
      <main className="container-responsive flex grow flex-col gap-5 py-5 md:flex-row">
        {/* UserDataProvider 获取并缓存用户完整信息和站点配置 */}
        <UserDataProvider>
          {/* RouteGuard 统一处理路由级别的 RBAC 权限检查 */}
          <RouteGuard>
            {children}
          </RouteGuard>
        </UserDataProvider>
      </main>

      {/* 固定定位的联系客服按钮 */}
      {/* <button className="fixed bottom-8 right-8 z-50 flex size-17 flex-col items-center justify-center gap-1 rounded-lg bg-primary shadow-lg hover:bg-primary-hover">
        <Image
          src="/images/icons/support.svg"
          alt="Contact Support"
          width={24}
          height={24}
          className="brightness-0 invert"
        />
        <span className="text-xs font-semibold leading-5 text-white">
          客服
        </span>
      </button> */}
    </div>
  );
}
