'use client';

import { usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { DashboardSidebar } from '@/components/layout';
import { Tabs } from '@/components/ui';
import type { TabItem } from '@/components/ui';
import { useUserStore } from '@/stores/userStore';
import { isGuestOnly } from '@/lib/rbac';

// Tab 导航配置 (基础tabs,所有用户可见)
const baseNavItems = [
  { key: 'basic', path: '/profile', label: 'tabs.basicInfo' },
  { key: 'security', path: '/profile/security', label: 'tabs.security' },
];

// 需要权限的tabs (非Guest用户可见)
const protectedNavItems = [
  { key: 'bank-infos', path: '/profile/bank-infos', label: 'tabs.bankInfos' },
  { key: 'files', path: '/profile/files', label: 'tabs.files' },
];

// Address tab - 需要 eventshop 角色且非 Guest
const addressNavItem = { key: 'address', path: '/profile/address', label: 'tabs.address' };

export default function ProfileLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const t = useTranslations('profile');
  const pathname = usePathname();
  const user = useUserStore((s) => s.user);

  // 判断是否为Guest用户
  const isGuest = isGuestOnly(user?.roles ?? []);
  
  // 判断是否有 eventshop 角色（不区分大小写）
  const hasEventshopRole = (user?.roles ?? []).some(
    (role) => role.toLowerCase() === 'eventshop'
  );

  // 根据用户权限动态组合导航项（Tab 显示/隐藏）
  // 注意：路由级别的权限保护已在 RouteGuard 中统一处理
  const navItems = [
    ...baseNavItems,
    // 非 Guest 用户可见的 tabs
    ...(isGuest ? [] : protectedNavItems),
    // Address tab - 需要 eventshop 角色且非 Guest
    ...(!isGuest && hasEventshopRole ? [addressNavItem] : []),
  ];

  return (
    <>
      {/* 左侧边栏 */}
      <div className="order-2 md:order-1 md:contents">
        <DashboardSidebar />
      </div>

      {/* 右侧内容区域 */}
      <div className="order-1 md:order-2 flex flex-1 flex-col gap-5">
        {/* 导航和内容区域 - 白色背景卡片 */}
        <div className="flex flex-1 flex-col rounded bg-surface p-4 md:p-5">
          {/* Tab 导航 */}
          <Tabs
            tabs={navItems.map((item) => ({
              key: item.key,
              label: t(item.label),
              href: item.path,
            } as TabItem))}
            activeKey={navItems.find((item) => pathname === item.path)?.key || 'basic'}
            onChange={() => {}}
            size="lg"
          />

          {/* 子页面内容 */}
          <div className="flex-1">
            {children}
          </div>
        </div>
      </div>
    </>
  );
}
