'use client';

import { useCallback, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useUserStore } from '@/stores/userStore';
import { useIBStore } from '@/stores/ibStore';
import { useSalesStore } from '@/stores/salesStore';
import { logout as logoutAction } from '@/actions';

/**
 * 退出登录 Hook
 * 清除所有本地存储数据，然后调用服务端退出 Server Action
 */
export function useLogout() {
  const router = useRouter();
  const clearStore = useUserStore((state) => state.clearStore);
  const [isLoading, setIsLoading] = useState(false);

  const logout = useCallback(async () => {
    setIsLoading(true);
    
    try {
      clearStore();
      useIBStore.getState().clearStore();
      useSalesStore.getState().clearStore();
      
      if (typeof window !== 'undefined') {
        localStorage.removeItem('user-storage');
        sessionStorage.clear();
      }
      
      const result = await logoutAction();
      
      if (result.success) {
        router.push('/sign-in');
      } else {
        router.push('/sign-in');
      }
    } catch {
      router.push('/sign-in');
    } finally {
      setIsLoading(false);
    }
  }, [clearStore, router]);

  return { logout, isLoading };
}

