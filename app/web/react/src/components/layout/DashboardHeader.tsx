'use client';

import { useState, useEffect, useRef } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { usePathname } from 'next/navigation';
import { ThemeToggle, LanguageToggle, Logo, Button } from '@/components/ui';
import { useUserStore } from '@/stores/userStore';
import { useLogout } from '@/hooks/useLogout';

// 菜单项配置
interface MenuItem {
  id: string;
  path: string;
  labelKey: string;
  icon?: string;
  /**
   * 允许访问的角色列表（不区分大小写）
   * - 不指定或空数组：所有角色都可以访问
   * - 指定角色列表：只有这些角色可以访问
   * 
   * 常用角色: Guest, Client, IB, Sales
   */
  allowedRoles?: string[];
}

/**
 * 检查用户角色是否有权限访问菜单项
 * @param userRoles 用户角色数组
 * @param allowedRoles 允许的角色列表
 * @returns 是否有权限
 */
function hasMenuAccess(userRoles: string[] | undefined, allowedRoles?: string[]): boolean {
  // 如果没有指定允许角色，所有人都可以访问
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }
  // 如果用户没有角色，不允许访问
  if (!userRoles || userRoles.length === 0) {
    return false;
  }
  // 检查用户是否拥有任一允许的角色（不区分大小写）
  const normalizedAllowed = allowedRoles.map(r => r.toLowerCase());
  return userRoles.some(role => normalizedAllowed.includes(role.toLowerCase()));
}

// 菜单配置
const menuItems: MenuItem[] = [
  { id: 'dashboard', path: '/dashboard', labelKey: 'dashboard' }, // 所有角色可见
  { id: 'verification', path: '/verification', labelKey: 'verification', allowedRoles: ['Guest'] }, // 只有 Guest 可见
   { id: 'account', path: '/account', labelKey: 'account', allowedRoles: ['Client', 'IB', 'Sales'] },
  { id: 'wallet', path: '/wallet', labelKey: 'wallet', allowedRoles: ['Client', 'IB', 'Sales'] },
  { id: 'platforms', path: '/platforms', labelKey: 'platforms' }, // 所有角色可见
  { id: 'supports', path: '/supports', labelKey: 'supports' }, // 所有角色可见
  { id: 'eventshop', path: '/eventshop', labelKey: 'eventshop', allowedRoles: ['Client', 'IB', 'Sales'] },
];

export function DashboardHeader() {
  const t = useTranslations('dashboard');
  const pathname = usePathname();
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [isSticky, setIsSticky] = useState(false);
  const [headerHeight, setHeaderHeight] = useState(0);
  const drawerRef = useRef<HTMLDivElement>(null);
  const sentinelRef = useRef<HTMLDivElement>(null);
  const headerRef = useRef<HTMLElement>(null);
  const { logout, isLoading: isLoggingOut } = useLogout();
  
  // 从 store 获取用户信息和初始化状态
  const user = useUserStore((state) => state.user);
  const isInitialized = useUserStore((state) => state.isInitialized);
  
  // 菜单是否正在加载（用户数据未初始化）
  const isMenuLoading = !isInitialized;
  
  // 根据角色过滤菜单
  const filteredMenuItems = menuItems.filter((item) => 
    hasMenuAccess(user?.roles, item.allowedRoles)
  );
  // 获取当前激活的菜单项
  const getActiveMenuId = (): string | null => {
    for (const item of filteredMenuItems) {
      if (pathname === item.path || pathname.startsWith(item.path + '/')) {
        return item.id;
      }
    }
    if (pathname === '/' || pathname === '/dashboard') return 'dashboard';
    return null;
  };

  const activeMenuId = getActiveMenuId();

  // 使用 IntersectionObserver 检测 header 是否已吸顶（比 scroll 事件更高效、更丝滑）
  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        setIsSticky(!entry.isIntersecting);
      },
      { threshold: 0 }
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, []);

  // 吸顶时保留占位高度，避免布局突变
  useEffect(() => {
    const header = headerRef.current;
    if (!header) return;

    const updateHeaderHeight = () => {
      setHeaderHeight(header.getBoundingClientRect().height);
    };

    updateHeaderHeight();

    const resizeObserver = new ResizeObserver(() => {
      updateHeaderHeight();
    });
    resizeObserver.observe(header);

    return () => resizeObserver.disconnect();
  }, []);

  // 点击外部关闭 Drawer
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (drawerRef.current && !drawerRef.current.contains(event.target as Node)) {
        setIsDrawerOpen(false);
      }
    };

    if (isDrawerOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      // 防止滚动
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.body.style.overflow = '';
    };
  }, [isDrawerOpen]);

  // 关闭 Drawer
  const closeDrawer = () => setIsDrawerOpen(false);

  // 用户显示名称
  const displayName = user?.name || user?.nativeName || user?.email?.split('@')[0] || '-';
  const displayEmail = user?.email || '-';
  const displayAvatar = user?.avatar || '/images/default-avatar.svg';

  return (
    <>
      {/* 哨兵元素：用于 IntersectionObserver 检测 header 是否需要吸顶 */}
      <div ref={sentinelRef} className="-mt-px h-px w-full" aria-hidden="true" />
      <div className="w-full" style={isSticky ? { height: headerHeight } : undefined}>
      <header
        ref={headerRef}
        className={`w-full border-b border-border transition-shadow duration-300 ease-out ${
          isSticky
            ? 'fixed inset-x-0 top-0 z-30 bg-surface shadow-md'
            : 'relative z-10 bg-surface shadow-none'
        }`}
      >
        <div className="container-responsive flex h-16 items-center justify-between sm:h-20">
          {/* Logo 和导航 */}
          <div className="flex items-center gap-4 sm:gap-6 lg:gap-10">
            {/* 移动端汉堡菜单按钮 */}
            <button
              type="button"
              onClick={() => setIsDrawerOpen(true)}
              className="flex size-10 items-center justify-center rounded-lg hover:bg-surface-secondary md:hidden"
              aria-label={t('openMenu')}
            >
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                className="text-text-primary"
              >
                <path
                  d="M3 12H21M3 6H21M3 18H21"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </button>

            {/* Logo */}
            <Link href="/" className="flex shrink-0 items-center gap-2 sm:gap-3">
              <Logo size="md" />
            </Link>

            {/* 桌面端导航菜单 - 胶囊形状 */}
            <nav
              className="hidden items-start overflow-x-auto rounded-[500px] bg-surface-secondary/80 px-5 pt-2 backdrop-blur-[28px] md:flex"
              style={{ gap: 'clamp(30px, calc(-4.286px + 3.348vw), 60px)' }}
            >
              {isMenuLoading ? (
                // 菜单骨架屏
                <>
                  {[1, 2, 3, 4, 5].map((i) => (
                    <div key={i} className="flex shrink-0 flex-col items-center gap-1.5 pb-2">
                      <div className="h-4 w-16 animate-pulse rounded bg-border" />
                      <div className="h-[2px] w-5 rounded-full bg-transparent" />
                    </div>
                  ))}
                </>
              ) : (
                filteredMenuItems.map((item) => {
                  const isActive = activeMenuId === item.id;
                  return (
                    <Link
                      key={item.id}
                      href={item.path}
                      className="group flex shrink-0 flex-col items-center gap-1.5 pb-2"
                    >
                      <span
                        className={`text-base whitespace-nowrap transition-colors ${
                          isActive
                            ? 'text-text-primary'
                            : 'text-text-secondary hover:text-text-primary/70'
                        }`}
                      >
                        {t(`menu.${item.labelKey}`)}
                      </span>
                      {/* 下划线指示器 - 日间黑色/夜间白色 */}
                      <div 
                        className={`h-[2px] w-5 rounded-full transition-all duration-200 ${
                          isActive 
                            ? 'bg-(--color-text-primary)' 
                            : 'bg-transparent group-hover:bg-[#999]/50'
                        }`}
                      />
                    </Link>
                  );
                })
              )}
            </nav>
          </div>

          {/* 右侧：主题切换 + 语言切换 + IB/Sales 按钮 */}
          <div className="flex shrink-0 items-center gap-3 sm:gap-4">
            <ThemeToggle />
            <LanguageToggle />
            {/* IB Center 按钮 */}
            {!isMenuLoading && hasMenuAccess(user?.roles, ['IB']) && (
              <Button asChild variant="primary" size="xs" className="hidden lg:inline-flex">
                <Link href="/ib" className='text-sm'>{t('menu.ib')}</Link>
              </Button>
            )}
            {/* Sales Center 按钮 */}
            {!isMenuLoading && hasMenuAccess(user?.roles, ['Sales']) && (
              <Button asChild variant="primary" size="xs" className="hidden lg:inline-flex">
                <Link href="/sales"  className='text-sm'>{t('menu.sales')}</Link>
              </Button>
            )}
          </div>
        </div>
      </header>
      </div>

      {/* 移动端 Drawer 遮罩 */}
      {isDrawerOpen && (
        <div className="fixed inset-0 z-40 bg-black/50 md:hidden" onClick={closeDrawer} />
      )}

      {/* 移动端 Drawer */}
      <div
        ref={drawerRef}
        className={`fixed left-0 top-0 z-50 flex h-full w-72 transform flex-col bg-surface shadow-lg transition-transform duration-300 ease-in-out md:hidden ${
          isDrawerOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {/* Drawer 头部 - 用户信息 */}
        <div className="flex items-center gap-3 border-b border-border px-5 py-6">
          {/* 头像 */}
          <div className="relative size-12 shrink-0 overflow-hidden rounded-full bg-surface-secondary">
            {displayAvatar ? (
              <Image
                src={displayAvatar}
                alt={displayName}
                fill
                className="object-cover"
              />
            ) : (
              <div className="flex size-full items-center justify-center text-xl font-semibold text-text-secondary">
                {displayName.charAt(0).toUpperCase()}
              </div>
            )}
          </div>
          {/* 用户信息 */}
          <div className="flex min-w-0 flex-col">
            <p className="truncate text-base font-semibold text-text-primary">
              {displayName}
            </p>
            <p className="truncate text-sm text-text-secondary">
              {displayEmail}
            </p>
          </div>
        </div>

        {/* Drawer 菜单列表 - 可滚动 */}
        <nav className="flex flex-1 flex-col gap-1 overflow-y-auto p-4">
          {isMenuLoading ? (
            // 移动端菜单骨架屏
            <>
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="flex flex-col gap-1 rounded-lg px-4 py-3.5">
                  <div className="h-5 w-24 animate-pulse rounded bg-border" />
                </div>
              ))}
            </>
          ) : (
            <>
              {filteredMenuItems.map((item) => (
                <Link
                  key={item.id}
                  href={item.path}
                  onClick={closeDrawer}
                  className={`flex flex-col gap-1 rounded-lg px-4 py-3.5 text-base transition-colors ${
                    activeMenuId === item.id
                      ? 'font-medium text-primary'
                      : 'text-text-primary hover:bg-surface-secondary'
                  }`}
                >
                  {t(`menu.${item.labelKey}`)}
                  {activeMenuId === item.id && (
                    <div className="h-0.5 w-5 rounded-full bg-primary" />
                  )}
                </Link>
              ))}

              {/* 移动端 IB / Sales 入口 */}
              {(hasMenuAccess(user?.roles, ['IB', 'Sales']) || hasMenuAccess(user?.roles, ['Sales'])) && (
                <div className="my-2 border-t border-border pt-2">
                  {hasMenuAccess(user?.roles, ['IB', 'Sales']) && (
                    <Link
                      href="/ib"
                      onClick={closeDrawer}
                      className={`flex flex-col gap-1 rounded-lg px-4 py-3.5 text-base transition-colors ${
                        pathname.startsWith('/ib')
                          ? 'font-medium text-primary'
                          : 'text-text-primary hover:bg-surface-secondary'
                      }`}
                    >
                      {t('menu.ib')}
                      {pathname.startsWith('/ib') && (
                        <div className="h-0.5 w-5 rounded-full bg-primary" />
                      )}
                    </Link>
                  )}
                  {hasMenuAccess(user?.roles, ['Sales']) && (
                    <Link
                      href="/sales"
                      onClick={closeDrawer}
                      className={`flex flex-col gap-1 rounded-lg px-4 py-3.5 text-base transition-colors ${
                        pathname.startsWith('/sales')
                          ? 'font-medium text-primary'
                          : 'text-text-primary hover:bg-surface-secondary'
                      }`}
                    >
                      {t('menu.sales')}
                      {pathname.startsWith('/sales') && (
                        <div className="h-0.5 w-5 rounded-full bg-primary" />
                      )}
                    </Link>
                  )}
                </div>
              )}
            </>
          )}
        </nav>

        {/* Drawer 底部 - 设置、登出 */}
        <div className="border-t border-border p-4">
          <div className="flex flex-col gap-1">
            {/* 账户设置 */}
            <Link
              href="/profile"
              onClick={closeDrawer}
              className="flex items-center gap-3 rounded-lg px-4 py-3 text-base text-text-primary hover:bg-surface-secondary"
            >
              <svg
                width="20"
                height="20"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                className="text-text-secondary"
              >
                <path
                  d="M12 15C13.6569 15 15 13.6569 15 12C15 10.3431 13.6569 9 12 9C10.3431 9 9 10.3431 9 12C9 13.6569 10.3431 15 12 15Z"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
                <path
                  d="M19.4 15C19.2669 15.3016 19.2272 15.6362 19.286 15.9606C19.3448 16.285 19.4995 16.5843 19.73 16.82L19.79 16.88C19.976 17.0657 20.1235 17.2863 20.2241 17.5291C20.3248 17.7719 20.3766 18.0322 20.3766 18.295C20.3766 18.5578 20.3248 18.8181 20.2241 19.0609C20.1235 19.3037 19.976 19.5243 19.79 19.71C19.6043 19.896 19.3837 20.0435 19.1409 20.1441C18.8981 20.2448 18.6378 20.2966 18.375 20.2966C18.1122 20.2966 17.8519 20.2448 17.6091 20.1441C17.3663 20.0435 17.1457 19.896 16.96 19.71L16.9 19.65C16.6643 19.4195 16.365 19.2648 16.0406 19.206C15.7162 19.1472 15.3816 19.1869 15.08 19.32C14.7842 19.4468 14.532 19.6572 14.3543 19.9255C14.1766 20.1938 14.0813 20.5082 14.08 20.83V21C14.08 21.5304 13.8693 22.0391 13.4942 22.4142C13.1191 22.7893 12.6104 23 12.08 23C11.5496 23 11.0409 22.7893 10.6658 22.4142C10.2907 22.0391 10.08 21.5304 10.08 21V20.91C10.0723 20.579 9.96512 20.258 9.77251 19.9887C9.5799 19.7194 9.31074 19.5143 9 19.4C8.69838 19.2669 8.36381 19.2272 8.03941 19.286C7.71502 19.3448 7.41568 19.4995 7.18 19.73L7.12 19.79C6.93425 19.976 6.71368 20.1235 6.47088 20.2241C6.22808 20.3248 5.96783 20.3766 5.705 20.3766C5.44217 20.3766 5.18192 20.3248 4.93912 20.2241C4.69632 20.1235 4.47575 19.976 4.29 19.79C4.10405 19.6043 3.95653 19.3837 3.85588 19.1409C3.75523 18.8981 3.70343 18.6378 3.70343 18.375C3.70343 18.1122 3.75523 17.8519 3.85588 17.6091C3.95653 17.3663 4.10405 17.1457 4.29 16.96L4.35 16.9C4.58054 16.6643 4.73519 16.365 4.794 16.0406C4.85282 15.7162 4.81312 15.3816 4.68 15.08C4.55324 14.7842 4.34276 14.532 4.07447 14.3543C3.80618 14.1766 3.49179 14.0813 3.17 14.08H3C2.46957 14.08 1.96086 13.8693 1.58579 13.4942C1.21071 13.1191 1 12.6104 1 12.08C1 11.5496 1.21071 11.0409 1.58579 10.6658C1.96086 10.2907 2.46957 10.08 3 10.08H3.09C3.42099 10.0723 3.742 9.96512 4.0113 9.77251C4.28059 9.5799 4.48572 9.31074 4.6 9C4.73312 8.69838 4.77282 8.36381 4.714 8.03941C4.65519 7.71502 4.50054 7.41568 4.27 7.18L4.21 7.12C4.02405 6.93425 3.87653 6.71368 3.77588 6.47088C3.67523 6.22808 3.62343 5.96783 3.62343 5.705C3.62343 5.44217 3.67523 5.18192 3.77588 4.93912C3.87653 4.69632 4.02405 4.47575 4.21 4.29C4.39575 4.10405 4.61632 3.95653 4.85912 3.85588C5.10192 3.75523 5.36217 3.70343 5.625 3.70343C5.88783 3.70343 6.14808 3.75523 6.39088 3.85588C6.63368 3.95653 6.85425 4.10405 7.04 4.29L7.1 4.35C7.33568 4.58054 7.63502 4.73519 7.95941 4.794C8.28381 4.85282 8.61838 4.81312 8.92 4.68H9C9.29577 4.55324 9.54802 4.34276 9.72569 4.07447C9.90337 3.80618 9.99872 3.49179 10 3.17V3C10 2.46957 10.2107 1.96086 10.5858 1.58579C10.9609 1.21071 11.4696 1 12 1C12.5304 1 13.0391 1.21071 13.4142 1.58579C13.7893 1.96086 14 2.46957 14 3V3.09C14.0013 3.41179 14.0966 3.72618 14.2743 3.99447C14.452 4.26276 14.7042 4.47324 15 4.6C15.3016 4.73312 15.6362 4.77282 15.9606 4.714C16.285 4.65519 16.5843 4.50054 16.82 4.27L16.88 4.21C17.0657 4.02405 17.2863 3.87653 17.5291 3.77588C17.7719 3.67523 18.0322 3.62343 18.295 3.62343C18.5578 3.62343 18.8181 3.67523 19.0609 3.77588C19.3037 3.87653 19.5243 4.02405 19.71 4.21C19.896 4.39575 20.0435 4.61632 20.1441 4.85912C20.2448 5.10192 20.2966 5.36217 20.2966 5.625C20.2966 5.88783 20.2448 6.14808 20.1441 6.39088C20.0435 6.63368 19.896 6.85425 19.71 7.04L19.65 7.1C19.4195 7.33568 19.2648 7.63502 19.206 7.95941C19.1472 8.28381 19.1869 8.61838 19.32 8.92V9C19.4468 9.29577 19.6572 9.54802 19.9255 9.72569C20.1938 9.90337 20.5082 9.99872 20.83 10H21C21.5304 10 22.0391 10.2107 22.4142 10.5858C22.7893 10.9609 23 11.4696 23 12C23 12.5304 22.7893 13.0391 22.4142 13.4142C22.0391 13.7893 21.5304 14 21 14H20.91C20.5882 14.0013 20.2738 14.0966 20.0055 14.2743C19.7372 14.452 19.5268 14.7042 19.4 15Z"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              {t('accountSettings')}
            </Link>
            
            {/* 登出 */}
            <button
              type="button"
              onClick={() => {
                closeDrawer();
                logout();
              }}
              disabled={isLoggingOut}
              className="flex w-full items-center gap-3 rounded-lg px-4 py-3 text-base text-text-primary hover:bg-surface-secondary disabled:opacity-50"
            >
              <svg
                width="20"
                height="20"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                className="text-text-secondary"
              >
                <path
                  d="M9 21H5C4.46957 21 3.96086 20.7893 3.58579 20.4142C3.21071 20.0391 3 19.5304 3 19V5C3 4.46957 3.21071 3.96086 3.58579 3.58579C3.96086 3.21071 4.46957 3 5 3H9M16 17L21 12M21 12L16 7M21 12H9"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              {isLoggingOut ? t('loggingOut') : t('logout')}
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
