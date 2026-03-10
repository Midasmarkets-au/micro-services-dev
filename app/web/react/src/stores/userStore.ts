'use client';

import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { UserInfo, SiteConfiguration, ParsedSiteConfiguration, ContactInfo } from '@/types/user';

interface UserState {
  // 用户信息
  user: UserInfo | null;
  // 站点配置
  siteConfig: ParsedSiteConfiguration | null;
  // 加载状态
  isLoading: boolean;
  // 是否已初始化
  isInitialized: boolean;
  // 最后更新时间
  lastUpdated: number | null;
  
  // Actions
  setUser: (user: UserInfo | null) => void;
  setSiteConfig: (config: SiteConfiguration | null) => void;
  setTwoFactorAuth: (enabled: boolean) => void;
  setLoading: (loading: boolean) => void;
  setInitialized: (initialized: boolean) => void;
  clearStore: () => void;
  
  // 便捷 getter
  isAuthenticated: () => boolean;
  hasRole: (role: string) => boolean;
  hasPermission: (permission: string) => boolean;
  isIB: () => boolean;
  isSales: () => boolean;
}

// 解析 contactInfo JSON 字符串
function parseContactInfo(config: SiteConfiguration): ParsedSiteConfiguration {
  let contactInfo: ContactInfo;
  
  try {
    contactInfo = JSON.parse(config.contactInfo?.value || '{}');
  } catch {
    contactInfo = {
      googleMap: '',
      phone: '',
      department: {
        generalInformation: '',
        marketingDepartment: '',
        complianceDepartment: '',
      },
      offices: {
        address: '',
      },
      socialMedia: {
        facebook: { url: '', icon: '' },
        twitter: { url: '', icon: '' },
        instagram: { url: '', icon: '' },
      },
    };
  }
  
  return {
    ...config,
    contactInfo,
  };
}

export const useUserStore = create<UserState>()(
  persist(
    (set, get) => ({
      user: null,
      siteConfig: null,
      isLoading: false,
      isInitialized: false,
      lastUpdated: null,
      
      setUser: (user) => set({ 
        user, 
        lastUpdated: Date.now(),
      }),
      
      setSiteConfig: (config) => set({ 
        siteConfig: config ? parseContactInfo(config) : null,
        lastUpdated: Date.now(),
      }),

      setTwoFactorAuth: (enabled) => set((state) => ({
        siteConfig: state.siteConfig 
          ? { ...state.siteConfig, twoFactorAuth: enabled }
          : null,
        lastUpdated: Date.now(),
      })),
      
      setLoading: (isLoading) => set({ isLoading }),
      
      setInitialized: (isInitialized) => set({ isInitialized }),
      
      clearStore: () => set({ 
        user: null, 
        siteConfig: null, 
        isInitialized: false,
        lastUpdated: null,
      }),
      
      // 便捷方法
      isAuthenticated: () => !!get().user,
      
      hasRole: (role) => {
        const user = get().user;
        if (!user?.roles) return false;
        // 不区分大小写
        const normalizedTarget = role.toLowerCase();
        return user.roles.some(r => r.toLowerCase() === normalizedTarget);
      },
      
      hasPermission: (permission) => {
        const user = get().user;
        return user?.permissions?.includes(permission) || false;
      },
      
      isIB: () => {
        const user = get().user;
        if (!user?.roles) return false;
        return user.roles.some(r => r.toLowerCase() === 'ib');
      },
      
      isSales: () => {
        const user = get().user;
        if (!user?.roles) return false;
        return user.roles.some(r => r.toLowerCase() === 'sales');
      },
    }),
    {
      name: 'user-storage', // localStorage key
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        // 只持久化这些字段
        user: state.user,
        siteConfig: state.siteConfig,
        lastUpdated: state.lastUpdated,
      }),
    }
  )
);


