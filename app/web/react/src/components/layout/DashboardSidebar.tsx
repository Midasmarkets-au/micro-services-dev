'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useLogout } from '@/hooks/useLogout';
import { useUserStore } from '@/stores/userStore';
import { GuestOnly } from '@/lib/rbac';
import { Avatar } from '@/components/ui';
import {
  platformDownloadLinks
} from '@/core/data/platformDownloads';

const MOBILE_BREAKPOINT = 768;

interface DashboardSidebarProps {
  // 可选的用户信息覆盖（向后兼容）
  user?: {
    nickname?: string;
    email?: string;
    avatar?: string;
  };
}

export function DashboardSidebar({ user: propUser }: DashboardSidebarProps) {
  const t = useTranslations('dashboard');
  const pathname = usePathname();
  const { isDark, mounted } = useTheme();
  const { logout, isLoading: isLoggingOut } = useLogout();

  const [isMobile, setIsMobile] = useState(false);
  useEffect(() => {
    const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`);
    const update = () => setIsMobile(mql.matches);
    mql.addEventListener('change', update);
    queueMicrotask(update);
    return () => mql.removeEventListener('change', update);
  }, []);
  // 从 store 获取用户信息
  const storeUser = useUserStore((state) => state.user);
  // 优先使用 store 中的数据，其次使用 props
  const user = {
    nickname:  storeUser?.nativeName || storeUser?.name || propUser?.nickname,
    email: storeUser?.email || propUser?.email,
    avatar: storeUser?.avatar || propUser?.avatar,
  };

  // 根据主题选择图片
  const verifyImage = isDark
    ? '/images/verification/verify-night.svg'
    : '/images/verification/verify-day.svg';
  const settingIcon = isDark
    ? '/images/icons/setting-night.svg'
    : '/images/icons/setting-day.svg';

  const allMenuItems = [
    { id: 'dashboard', label: t('accountManagement'), icon: '/images/icons/wallet.svg' },
    { id: 'profile', label: t('personalInfo'), icon: '/images/icons/user.svg' },
    // { id: 'user-guide', label: t('userGuide'), icon: '/images/icons/book.svg' },
    { id: 'app-store', label: t('appStore'), icon: '/images/icons/store.svg', url: platformDownloadLinks.bvi.mt5.android },
    { id: 'android-apk', label: t('androidApk'), icon: '/images/icons/android.svg', url: platformDownloadLinks.bvi.mt5.ios },
  ];
  // 移动端不显示「账户管理」「个人资料」
  const menuItems = isMobile
    ? allMenuItems.filter((item) => item.id !== 'dashboard' && item.id !== 'profile')
    : allMenuItems;
  const isProfilePage = pathname === '/profile' || pathname.startsWith('/profile/');

  return (
    <aside className="sidebar-responsive">
      {/* 用户信息卡片；profile 及子页面移动端隐藏 */}
      <div
        className={`relative flex flex-col items-center border  border-[rgba(26,29,33,0.08)] gap-3 overflow-hidden rounded bg-surface px-0 py-5 ${isProfilePage ? 'max-md:hidden' : ''}`}
      >
        {/* 右上角设置图标 - 点击跳转 /profile */}
        <Link
          href="/profile"
          className="absolute right-4 top-4 flex size-9 items-center justify-center rounded-full hover:bg-(--color-surface-secondary)"
          aria-label={t('personalInfo')}
        >
          <Image
            src={settingIcon}
            alt=""
            width={20}
            height={20}
            className="opacity-80"
          />
        </Link>
        {/* 头像 */}
        <Avatar 
          src={user.avatar} 
          alt={user.nickname || 'User'} 
          size="lg"
          skeleton={!storeUser}
        />

        {/* 用户信息 */}
        <div className="flex flex-col items-center gap-1">
          <p className="text-xl font-semibold text-center leading-6.5 text-text-primary px-2">
            {user.nickname || '-'}
          </p>
          <div className="flex items-center justify-center gap-1">
            <Image
              src="/images/icons/mail.svg"
              alt="email"
              width={20}
              height={20}
            />
            <p className="text-base leading-6.5 text-text-secondary">
              {user.email || '-'}
            </p>
          </div>
        </div>
      </div>

      {/* 菜单导航 + 验证卡片；/dashboard 或 profile 及子页面在移动端隐藏整块 */}
      <div
        className={`flex grow flex-col gap-2.5 rounded border border-[rgba(26,29,33,0.08)] bg-surface p-5 ${ isProfilePage ? 'max-md:hidden' : ''}`}
      >
        {/* 菜单列表 */}
        <div className="flex flex-col gap-5">
          {menuItems.map((item, index) => {
            // 根据当前路由判断是否激活
            const isActive = pathname === `/${item.id}` || pathname.startsWith(`/${item.id}/`);
            return (
              <div key={item.id}>
                <Link
                  href={item.url || `/${item.id}`}
                  target={item.url ? "_blank" : undefined}
                  className={`flex items-center gap-3 overflow-hidden rounded px-4 py-3 ${
                    isActive
                      ? 'bg-primary'
                      : 'bg-transparent hover:bg-surface-secondary'
                  }`}
                >
                  <Image
                    src={item.icon}
                    alt={item.label}
                    width={20}
                    height={20}
                    className={isActive ? 'brightness-0 invert' : 'opacity-60 dark:invert dark:opacity-60'}
                  />
                  <span
                    className={`text-base font-medium ${
                      isActive ? 'text-white' : 'text-text-secondary'
                    }`}
                  >
                    {item.label}
                  </span>
                </Link>
                {/* 分割线 - 桌面端在 index 1/2/4 后，移动端仅在 index 1 后（仅 2 个菜单项） */}
                {((isMobile && index === 1) || (!isMobile && (index === 1 || index === 2 || index === 4))) && (
                  <div className="mt-5 h-px w-full bg-border" />
                )}
              </div>
            );
          })}

          {/* 退出按钮 */}
          {!isMobile ? (
             <button
            type="button"
            onClick={logout}
            disabled={isLoggingOut}
            className="flex w-full cursor-pointer items-center gap-3 overflow-hidden rounded px-4 py-3 hover:bg-surface-secondary disabled:cursor-not-allowed disabled:opacity-50"
          >
            <Image
              src="/images/icons/logout.svg"
              alt={t('logout')}
              width={20}
              height={20}
              className="opacity-60 dark:invert dark:opacity-60"
            />
            <span className="text-base font-medium text-text-secondary">
              {isLoggingOut ? t('loggingOut') : t('logout')}
            </span>
          </button>
          ) : null}
        </div>

        {/* 验证账户卡片 - 在 border 容器内部，支持日/夜间模式 */}
        {/* h-[9.375rem] = 150px, size-[8.125rem] = 130px - 使用 rem 实现响应式缩放 */}
        <GuestOnly roles={storeUser?.roles ?? []}>
        <div className="verify-card relative mt-auto hidden h-37.5 w-full overflow-hidden md:block">
          {/* 右侧装饰图 - 根据主题动态切换 */}
          {mounted && (
            <div className="absolute right-2 top-2 size-32.5 opacity-80">
              <Image
                src={verifyImage}
                alt="verify"
                fill
                className="object-contain"
              />
            </div>
          )}
          
          {/* 文字内容 - 日间深色/夜间白色 */}
          <div className="absolute left-5 top-5 z-10 flex flex-col">
            <p className="verify-card-title text-base font-semibold">
              {t('verifyYourAccount')}
            </p>
            <p className="verify-card-desc mt-2 w-[120px] text-responsive-2xs">
              {t('verifyDescription')}
            </p>
          </div>
          
          {/* 立即开始按钮 */}
          <Link
            href="/verification"
            className="absolute bottom-4 left-4 flex items-center gap-1.5 rounded bg-primary px-4 py-1.5 text-xs font-semibold text-white backdrop-blur-sm hover:bg-primary-hover"
          >
            {t('verifyNow')}
            <svg width="7" height="7" viewBox="0 0 7 7" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1 6L6 1M6 1H1M6 1V6" stroke="white" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </Link>
        </div>
        </GuestOnly>
      </div>
    </aside>
  );
}
