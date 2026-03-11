'use client';

import Lottie from "lottie-react";
import loginLight from "@/core/data/animation/homeLight.json";
import loginDark from "@/core/data/animation/homeDark.json";
import { useTheme } from '@/hooks/useTheme';

export interface AuthIllustrationProps {
  /** 动画区域尺寸（px），不传则填满容器 / 桌面最大 600px */
  size?: number;
}

export function AuthIllustration({ size }: AuthIllustrationProps) {
  const { isDark, mounted } = useTheme();
  const sizeStyle = size != null ? { width: size, height: size, minWidth: size, minHeight: size, maxWidth: size, maxHeight: size } : undefined;
  const wrapperClass = size != null
    ? 'relative flex items-center justify-center shrink-0'
    : 'relative flex h-full w-full min-h-0 min-w-0 max-h-full max-w-full md:max-h-150 md:max-w-150 items-center justify-center';
  // 服务端渲染和客户端首次渲染保持一致，显示占位符
  if (!mounted) {
    return (
      <div className={wrapperClass} style={sizeStyle}>
        <div className={size != null ? 'size-full' : 'h-full w-full max-h-150 max-w-150 min-h-0 min-w-0'} />
      </div>
    );
  }
  return (
    <div className={wrapperClass} style={sizeStyle}>
      <div className={size != null ? 'size-full object-contain -translate-y-1' : 'h-full w-full max-h-full max-w-full min-h-0 min-w-0 object-contain -translate-y-4'}>
        <Lottie animationData={isDark ? loginDark : loginLight} loop={true} />
      </div>
    </div>
  );
}
