'use client';

import { useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useUserStore } from '@/stores/userStore';
import { getUserInfo, getConfiguration } from '@/actions';
import type { UserInfo, SiteConfiguration } from '@/types/user';

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
  const {
    setUser,
    setSiteConfig,
    setLoading,
    setInitialized,
    clearStore,
  } = useUserStore();
  
  const fetchingRef = useRef(false);
  const mountedRef = useRef(false);
  
  useEffect(() => {
    if (mountedRef.current) return;
    mountedRef.current = true;
    
    const hasCachedData = !!useUserStore.getState().user;

    if (hasCachedData) {
      setInitialized(true);
      setLoading(false);
    }

    const fetchData = async () => {
      if (fetchingRef.current) return;
      fetchingRef.current = true;
      
      if (!hasCachedData) {
        setLoading(true);
      }
      
      try {
        console.log('[UserDataProvider] 开始获取用户数据...');
        
        const [userResult, configResult] = await Promise.allSettled([
          getUserInfo(),
          getConfiguration(),
        ]);
        
        if (userResult.status === 'fulfilled' && userResult.value?.success && userResult.value?.data) {
          console.log('[UserDataProvider] 用户数据获取成功:', (userResult.value.data as UserInfo).email);
          setUser(userResult.value.data as UserInfo);
        } else {
          console.error('[UserDataProvider] 获取用户数据失败:', 
            userResult.status === 'rejected' ? userResult.reason : 'No data');
          clearStore();
          router.push('/sign-in?expired=true');
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
        if (!hasCachedData) {
          clearStore();
          router.push('/sign-in?expired=true');
        }
      } finally {
        setLoading(false);
        fetchingRef.current = false;
      }
    };
    
    fetchData();
  }, [setUser, setSiteConfig, setLoading, setInitialized, clearStore, router]);
  
  return <>{children}</>;
}
