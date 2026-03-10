'use client';

import { useEffect, useMemo } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { useUserStore } from '@/stores/userStore';
import { PageLoading } from '@/components/ui/PageLoading';
import { checkRoutePermission } from '@/lib/rbac/routeConfig';

interface RouteGuardProps {
  children: React.ReactNode;
}

export function RouteGuard({ children }: RouteGuardProps) {
  const pathname = usePathname();
  const router = useRouter();
  const user = useUserStore((s) => s.user);
  const isLoading = useUserStore((s) => s.isLoading);
  const isInitialized = useUserStore((s) => s.isInitialized);

  const redirectTo = useMemo(() => {
    if (isLoading || !isInitialized) return null;
    const userRoles = user?.roles ?? [];
    return checkRoutePermission(pathname, userRoles);
  }, [isLoading, isInitialized, user, pathname]);

  const isAuthorized = isInitialized && !isLoading && !redirectTo;

  useEffect(() => {
    if (redirectTo) {
      console.log(`[RouteGuard] 权限不足，从 ${pathname} 重定向到 ${redirectTo}`);
      router.replace(redirectTo);
    }
  }, [redirectTo, pathname, router]);

  if (!isAuthorized) {
    return <PageLoading />;
  }

  return <>{children}</>;
}
