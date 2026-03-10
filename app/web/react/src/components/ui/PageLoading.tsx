interface PageLoadingProps {
  /** 是否全屏显示 */
  fullscreen?: boolean;
  /** 自定义类名 */
  className?: string;
}

/**
 * MDM 品牌加载动画组件
 * 包含 MDM 字母动画和进度条
 * 注意：这是一个服务端组件，CSS 动画不依赖 JavaScript
 */
export function PageLoading({ fullscreen = true, className = '' }: PageLoadingProps) {
  return (
    <div
      className={`
        flex w-full flex-col items-center justify-center gap-8
        ${fullscreen ? 'fixed inset-0 z-9999 bg-background' : ''}
        ${className}
      `}
    >
      {/* MDM Logo 动画 */}
      <div className="flex items-center gap-1">
        {/* M */}
        <span className="mdm-letter mdm-letter-1 font-brand text-5xl font-bold text-primary">
          M
        </span>
        {/* D */}
        <span className="mdm-letter mdm-letter-2 font-brand text-5xl font-bold text-primary">
          D
        </span>
        {/* M */}
        <span className="mdm-letter mdm-letter-3 font-brand text-5xl font-bold text-primary">
          M
        </span>
      </div>

      {/* 进度条动画 */}
      <div className="loading-bar-container">
        <div className="loading-bar" />
      </div>

      {/* 加载文字 */}
      <p className="loading-text text-sm text-text-secondary">
        Loading...
      </p>
    </div>
  );
}

/**
 * 简化版加载动画（用于小区域）
 */
export function MiniLoading({ className = '' }: { className?: string }) {
  return (
    <div className={`flex items-center justify-center gap-2 ${className}`}>
      <div className="loading-dots">
        <span className="loading-dot" />
        <span className="loading-dot" />
        <span className="loading-dot" />
      </div>
    </div>
  );
}

/**
 * MDM Logo 加载动画（无背景）
 */
export function MDMLoading({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) {
  const sizeClasses = {
    sm: 'text-2xl gap-0.5',
    md: 'text-4xl gap-1',
    lg: 'text-6xl gap-1.5',
  };

  return (
    <div className={`flex items-center ${sizeClasses[size]}`}>
      <span className="mdm-letter mdm-letter-1 font-brand font-bold text-primary">M</span>
      <span className="mdm-letter mdm-letter-2 font-brand font-bold text-primary">D</span>
      <span className="mdm-letter mdm-letter-3 font-brand font-bold text-primary">M</span>
    </div>
  );
}
