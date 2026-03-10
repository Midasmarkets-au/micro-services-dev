'use client';

import { useEffect, useState } from 'react';
import Image from 'next/image';

export function AuthIllustration() {
  const [isDark, setIsDark] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    // 挂载后立即检查主题
    setMounted(true);
    setIsDark(document.documentElement.classList.contains('dark'));

    // 监听主题变化
    const observer = new MutationObserver(() => {
      setIsDark(document.documentElement.classList.contains('dark'));
    });

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['class'],
    });

    return () => observer.disconnect();
  }, []);

  // 根据主题选择插图
  const illustrationSrc = isDark
    ? '/images/auth-illustration-night.svg'
    : '/images/auth-illustration-day.svg';

  // 服务端渲染和客户端首次渲染保持一致，显示占位符
  if (!mounted) {
    return (
      <div className="relative flex h-full w-full max-h-150 max-w-150 items-center justify-center">
        <div className="h-150 w-150" />
      </div>
    );
  }

  return (
    <div className="relative flex h-full w-full max-h-150 max-w-150 items-center justify-center">
      <Image
        src={illustrationSrc}
        alt="Authentication Illustration"
        width={600}
        height={600}
        priority
        className="h-full w-full object-contain"
      />
    </div>
  );
}
