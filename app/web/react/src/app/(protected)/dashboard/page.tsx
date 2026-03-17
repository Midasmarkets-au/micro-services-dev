'use client';

import { useUserStore } from '@/stores/userStore';
import { TwoFactorAuthDialog } from '@/components/layout/TwoFactorAuthDialog';
import {
  DashboardSidebar,
  DashboardMainContent,
  DashboardNotifications,
} from '@/components/layout';
import { DashboardSkeleton } from '@/components/ui';

export default function DashboardPage() {
  // 从 store 获取加载状态
  const { isLoading, isInitialized, user } = useUserStore();

  // 显示骨架屏：正在加载 或 未初始化且没有缓存数据
  const showSkeleton = isLoading || (!isInitialized && !user);

  if (showSkeleton) {
    return <DashboardSkeleton />;
  }

  return (
    <>
      <TwoFactorAuthDialog />

      {/* 移动端：内容优先，sidebar 在底部 */}
      {/* 桌面端：sidebar - content - notifications */}

      {/* 左侧边栏 - 桌面端显示在左侧，移动端显示在内容下方 */}
      <div className="order-2 md:order-1 md:contents">
        <DashboardSidebar />
      </div>

      {/* 中间内容 - 始终显示 */}
      <div className="order-1 md:order-2 md:contents">
        <DashboardMainContent />
      </div>

      {/* 右侧通知 - 桌面端显示在右侧，移动端显示在最下方 */}
      <div className="order-3 md:contents">
        <DashboardNotifications />
      </div>
    </>
  );
}
