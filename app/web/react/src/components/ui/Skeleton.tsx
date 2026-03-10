'use client';

import { type HTMLAttributes } from 'react';
import { MDMLoading } from './PageLoading';

interface SkeletonProps extends HTMLAttributes<HTMLDivElement> {
  className?: string;
}

/**
 * 骨架屏基础组件 - 带流光效果
 */
export function Skeleton({ className = '', ...props }: SkeletonProps) {
  return (
    <div
      className={`skeleton-shimmer rounded ${className}`}
      {...props}
    />
  );
}

/**
 * 侧边栏骨架屏 - 与 DashboardSidebar 高度一致
 */
export function SidebarSkeleton() {
  return (
    <aside className="sidebar-responsive">
      {/* 用户信息卡片骨架 - 与实际组件 py-5 + 头像60px + gap-3*2 + 用户名24px + 邮箱20px 一致 */}
      <div className="flex flex-col items-center gap-3 overflow-hidden rounded bg-surface px-0 py-5">
        {/* 头像骨架 */}
        <Skeleton className="size-15 rounded-full" />
        {/* 用户名骨架 */}
        <Skeleton className="h-6 w-24" />
        {/* 邮箱骨架 */}
        <Skeleton className="h-5 w-40" />
      </div>

      {/* 菜单骨架 - 使用 flex-1 填充剩余高度 */}
      <div className="flex flex-1 flex-col gap-2.5 rounded border border-[rgba(26,29,33,0.08)] bg-surface p-5">
        <div className="flex flex-col gap-5">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <Skeleton key={i} className="h-12 w-full rounded" />
          ))}
        </div>
        {/* 验证卡片骨架 - 底部对齐 */}
        <Skeleton className="mt-auto hidden h-[9.375rem] w-full md:block" />
      </div>
    </aside>
  );
}

/**
 * 主内容区骨架屏 - 与 DashboardMainContent 高度一致
 */
export function MainContentSkeleton() {
  return (
    <div className="main-content-responsive">
      {/* Banner 骨架 - 高度 10.5rem = 168px */}
      <Skeleton className="h-[10.5rem] w-full rounded" />

      {/* 标签栏骨架 - 与实际组件结构一致 */}
      <div className="flex flex-col">
        <div className="flex items-start justify-between">
          <div className="flex items-start gap-10">
            <div className="flex w-20 flex-col items-center gap-[18px]">
              <Skeleton className="h-7 w-16" />
              <Skeleton className="h-[2px] w-20" />
            </div>
            <div className="flex w-20 flex-col items-center gap-[18px]">
              <Skeleton className="h-7 w-16" />
            </div>
          </div>
          <Skeleton className="h-8 w-40" />
        </div>
        <div className="h-px w-full bg-border" />
      </div>

      {/* 内容区骨架 - 使用 flex-1 填充剩余高度 */}
      <div className="flex flex-1 flex-col items-center justify-center rounded bg-surface py-20">
        <Skeleton className="size-[120px] rounded-full" />
        <Skeleton className="mt-4 h-6 w-32" />
      </div>
    </div>
  );
}

/**
 * 通知区骨架屏 - 与 DashboardNotifications 高度一致
 */
export function NotificationsSkeleton() {
  return (
    <aside className="sidebar-responsive">
      {/* 使用 flex-1 和 h-full 确保填充整个高度 */}
      <div className="flex flex-1 flex-col rounded bg-surface p-5">
        {/* 标题骨架 */}
        <div className="mb-5 flex items-center justify-between">
          <Skeleton className="h-6 w-24" />
          <Skeleton className="h-5 w-16" />
        </div>

        {/* 通知列表骨架 - 与实际通知卡片结构一致 */}
        <div className="flex flex-col gap-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex flex-col gap-2 rounded border border-border p-4">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-5 w-3/4" />
              <Skeleton className="h-10 w-full" />
            </div>
          ))}
        </div>
      </div>
    </aside>
  );
}

/**
 * Dashboard 完整骨架屏
 * 桌面端：三栏骨架屏布局，与实际 Dashboard 完全一致
 * 移动端：MDM Logo 动画
 */
export function DashboardSkeleton() {
  return (
    <>
      {/* 移动端显示 MDM 加载动画 */}
      <div className="flex w-full flex-col items-center justify-center gap-8 py-20 md:hidden">
        <MDMLoading size="lg" />
        <div className="loading-bar-container">
          <div className="loading-bar" />
        </div>
        <p className="loading-text text-sm text-text-secondary">Loading...</p>
      </div>
      
      {/* 桌面端显示骨架屏 - 使用 contents 让子元素参与父级 flex 布局 */}
      <div className="hidden w-full md:contents">
        <SidebarSkeleton />
        <MainContentSkeleton />
        <NotificationsSkeleton />
      </div>
    </>
  );
}
