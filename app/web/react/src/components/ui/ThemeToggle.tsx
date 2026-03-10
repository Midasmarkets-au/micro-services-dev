'use client';

import { useEffect, useState } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';

type Theme = 'light' | 'dark';

export function ThemeToggle() {
  const t = useTranslations('common');
  const [theme, setTheme] = useState<Theme>('light');
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    // 挂载后获取实际主题
    const savedTheme = localStorage.getItem('theme') as Theme | null;
    const initialTheme = savedTheme || 
      (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    
    setTheme(initialTheme);
    setMounted(true);
  }, []);

  useEffect(() => {
    if (!mounted) return;
    // Apply theme changes to DOM
    document.documentElement.classList.toggle('dark', theme === 'dark');
  }, [theme, mounted]);

  const toggleTheme = () => {
    const newTheme = theme === 'light' ? 'dark' : 'light';
    setTheme(newTheme);
    localStorage.setItem('theme', newTheme);
    document.documentElement.classList.toggle('dark', newTheme === 'dark');
  };

  if (!mounted) {
    return (
      <div className="flex h-5 items-center gap-1">
        <div className="size-5" />
        <span className="text-base text-text-secondary">--</span>
      </div>
    );
  }

  return (
    <button
      onClick={toggleTheme}
      className="flex cursor-pointer items-center gap-1 text-text-secondary transition-colors hover:text-text-primary"
    >
      {/* 太阳/月亮图标 */}
      <svg
        className="size-5"
        viewBox="0 0 20 20"
        fill="currentColor"
      >
        {theme === 'light' ? (
          // 太阳图标 (日间模式)
          <path
            fillRule="evenodd"
            d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z"
            clipRule="evenodd"
          />
        ) : (
          // 月亮图标 (夜间模式)
          <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" />
        )}
      </svg>
      <span className="">
        {theme === 'light' ? t('dayMode') : t('nightMode')}
      </span>
      <Image 
        src="/images/icons/chevron-down.svg" 
        alt="" 
        width={20}
        height={20}
        className="size-5"
      />
    </button>
  );
}

