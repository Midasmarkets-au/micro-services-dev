'use client';

import Image from 'next/image';

interface LogoProps {
  size?: 'sm' | 'md' | 'lg';
  showText?: boolean;
  className?: string;
}

const sizeMap = {
  sm: 32,
  md: 40,
  lg: 48,
};

const textSizeClasses = {
  sm: 'text-lg',  /* 18px */
  md: 'text-2xl', /* 24px */
  lg: 'text-3xl', /* 30px */
};

export function Logo({ size = 'md', showText = true, className = '' }: LogoProps) {
  const iconSize = sizeMap[size];

  return (
    <div className={`flex items-center gap-3 ${className}`}>
      {/* Logo 图标 - 使用 Figma 设计的图片 */}
      <div className="logo-size shrink-0" style={{ width: iconSize, height: iconSize }}>
        <Image
          src="/images/logo.png"
          alt="MDM Logo"
          width={iconSize}
          height={iconSize}
          className="h-full w-full object-contain"
          priority
        />
      </div>
      {/* Logo 文字 - 根据主题变色，移动端隐藏 */}
      {showText && (
        <span
          className={`hidden font-brand font-medium text-text-primary sm:inline ${textSizeClasses[size]}`}
          style={{ fontVariationSettings: "'wdth' 100" }}
        >
          MDM
        </span>
      )}
    </div>
  );
}
