'use client';

import Lottie from "lottie-react";
import loginLight from "@/core/data/animation/homeLight.json";
import loginDark from "@/core/data/animation/homeDark.json";
import { useTheme } from '@/hooks/useTheme';

export function AuthIllustration() {
  const { isDark, mounted } = useTheme();
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
      <div className="h-full w-full max-h-150 max-w-150 object-contain -translate-y-4">
        <Lottie animationData={isDark ? loginDark : loginLight} loop={true} />
      </div>
    </div>
  );
}
