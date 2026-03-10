import { cn } from '@/lib/utils';

export interface IconProps {
  /** SVG 文件名（不含 .svg 后缀），对应 /public/images/icons/ 目录下的文件 */
  name: string;
  /** 图标尺寸 (px)，默认 16 */
  size?: number;
  className?: string;
}

/**
 * 通用图标组件，通过 CSS mask-image 加载 /public/images/icons/ 下的 SVG 文件。
 * 图标颜色自动继承父元素的 `color`（即 currentColor），无需为不同场景维护多色版本。
 *
 * @example
 * ```tsx
 * // 在 Button 中 - 自动跟随按钮文字颜色
 * <Button variant="primary"><Icon name="search-line" /> 搜索</Button>
 *
 * // 自定义颜色和大小
 * <Icon name="arrow-left" size={24} className="text-text-secondary" />
 * ```
 */
export function Icon({ name, size = 16, className }: IconProps) {
  return (
    <span
      role="img"
      aria-hidden
      className={cn('inline-block shrink-0', className)}
      style={{
        width: size,
        height: size,
        backgroundColor: 'currentColor',
        maskImage: `url(/images/icons/${name}.svg)`,
        WebkitMaskImage: `url(/images/icons/${name}.svg)`,
        maskSize: 'contain',
        WebkitMaskSize: 'contain',
        maskRepeat: 'no-repeat',
        WebkitMaskRepeat: 'no-repeat',
        maskPosition: 'center',
        WebkitMaskPosition: 'center',
      }}
    />
  );
}
