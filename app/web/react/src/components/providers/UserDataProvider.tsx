'use client';

import { useEffect, useRef, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { useLocale } from 'next-intl';
import { useUserStore } from '@/stores/userStore';
import { getUserInfo, getConfiguration, setLocale } from '@/actions';
import { localeMap } from '@/i18n/config';
import type { UserInfo, SiteConfiguration } from '@/types/user';

// 缓存有效期：5 分钟。超过后切回 tab 或重新 mount 时会静默后台更新
const CACHE_TTL_MS = 5 * 60 * 1000;

const isCacheStale = (lastUpdated: number | null) =>
  !lastUpdated || Date.now() - lastUpdated > CACHE_TTL_MS;

interface UserDataProviderProps {
  children: React.ReactNode;
}

/**
 * 用户数据提供者
 * 在受保护的布局中使用，自动获取和缓存用户数据
 * 如果 token 无效会重定向到登录页
 */
export function UserDataProvider({ children }: UserDataProviderProps) {
  const router = useRouter();
  const currentLocale = useLocale();
  const {
    setUser,
    setSiteConfig,
    setLoading,
    setInitialized,
    clearStore,
  } = useUserStore();
  
  const fetchingRef = useRef(false);
  const mountedRef = useRef(false);

  const fetchData = useCallback(async (silent = false) => {
    if (fetchingRef.current) return;
    fetchingRef.current = true;

    const hasCachedData = !!useUserStore.getState().user;

    if (!silent && !hasCachedData) {
      setLoading(true);
    }

    try {
      console.log('[UserDataProvider] 开始获取用户数据...');

      const [userResult, configResult] = await Promise.allSettled([
        getUserInfo(),
        getConfiguration(),
      ]);

      if (userResult.status === 'fulfilled' && userResult.value?.success && userResult.value?.data) {
        const userData = userResult.value.data as UserInfo;
        console.log('[UserDataProvider] 用户数据获取成功:', userData.email);
        setUser(userData);

        // 同步 user.language 到 locale Cookie（最高优先级）
        const userLocale = localeMap[userData.language];
        if (userLocale && userLocale !== currentLocale) {
          console.log(`[UserDataProvider] 语言同步: ${currentLocale} -> ${userLocale}`);
          await setLocale({ locale: userLocale });
          window.location.reload();
          return;
        }
      } else {
        console.error('[UserDataProvider] 获取用户数据失败:',
          userResult.status === 'rejected' ? userResult.reason : 'No data');
        // 静默刷新失败时不清除缓存，保留旧数据继续展示
        if (!hasCachedData) {
          clearStore();
          router.push('/sign-in?expired=true');
        }
        return;
      }

      if (configResult.status === 'fulfilled' && configResult.value?.success && configResult.value?.data) {
        console.log('[UserDataProvider] 站点配置获取成功');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        setSiteConfig(configResult.value.data as any as SiteConfiguration);
      } else {
        console.warn('[UserDataProvider] 获取站点配置失败:',
          configResult.status === 'rejected' ? configResult.reason : 'No data');
      }

      setInitialized(true);
    } catch (error) {
      console.error('[UserDataProvider] 请求失败:', error);
      if (!useUserStore.getState().user) {
        clearStore();
        router.push('/sign-in?expired=true');
      }
    } finally {
      setLoading(false);
      fetchingRef.current = false;
    }
  }, [currentLocale, setUser, setSiteConfig, setLoading, setInitialized, clearStore, router]);
  
  useEffect(() => {
    if (mountedRef.current) return;
    mountedRef.current = true;

    const state = useUserStore.getState();
    const hasCachedData = !!state.user;

    if (hasCachedData) {
      setInitialized(true);
      setLoading(false);
    }

    // 首次 mount 始终拉取最新数据（有缓存时静默后台更新）
    fetchData(hasCachedData);

    // 切回 tab 时，若缓存已过期则静默后台更新（捕获其他设备/方式修改的数据）
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') {
        const stale = isCacheStale(useUserStore.getState().lastUpdated);
        if (stale) {
          console.log('[UserDataProvider] 缓存过期，切回 tab 触发静默更新');
          fetchData(true);
        }
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, [fetchData, setInitialized, setLoading]);
  
  return <>{children}</>;
}
