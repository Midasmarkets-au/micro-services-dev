'use client';

import { useSyncExternalStore } from 'react';

export type Theme = 'light' | 'dark';

// ===== Theme Store =====
function getTheme(): Theme {
  if (typeof window === 'undefined') return 'light';
  return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
}

function subscribeTheme(callback: () => void) {
  const observer = new MutationObserver(callback);
  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class'],
  });
  return () => observer.disconnect();
}

// ===== Mounted Store =====
let isMounted = false;
const mountedListeners = new Set<() => void>();

function getMounted() {
  return isMounted;
}

function subscribeMounted(callback: () => void) {
  mountedListeners.add(callback);
  
  // 首次订阅时设置为 mounted
  if (!isMounted) {
    isMounted = true;
    // 异步通知，避免同步 setState 警告
    queueMicrotask(() => {
      mountedListeners.forEach((listener) => listener());
    });
  }
  
  return () => {
    mountedListeners.delete(callback);
  };
}

function getServerMounted() {
  return false;
}

// ===== Hook =====
export function useTheme() {
  const theme = useSyncExternalStore(subscribeTheme, getTheme, () => 'light');
  const mounted = useSyncExternalStore(subscribeMounted, getMounted, getServerMounted);

  return {
    theme,
    mounted,
    isDark: theme === 'dark',
  };
}
