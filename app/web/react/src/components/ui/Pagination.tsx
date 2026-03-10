'use client';

import { cn } from '@/lib/utils';

/**
 * 通用分页组件
 *
 * 特性：
 * - 页码超过 7 页时自动显示省略号
 * - 支持上一页/下一页导航
 * - 首尾页始终可见
 * - 自动适配日间/夜间主题
 *
 * @example
 * <Pagination
 *   page={currentPage}
 *   total={totalRecords}
 *   size={pageSize}
 *   onPageChange={(p) => setPage(p)}
 * />
 */

export interface PaginationProps {
  /** 当前页码（从 1 开始） */
  page: number;
  /** 总记录数 */
  total: number;
  /** 每页条数 */
  size: number;
  /** 页码变化回调 */
  onPageChange: (page: number) => void;
  /** 自定义容器 className */
  className?: string;
}

export function Pagination({ page, total, size, onPageChange, className }: PaginationProps) {
  const totalPages = Math.ceil(total / size);
  if (totalPages <= 1) return null;

  const pages = buildPageNumbers(page, totalPages);

  const btnBase =
    'flex size-8 items-center justify-center rounded text-sm transition-colors cursor-pointer';
  const btnNormal =
    'border border-border text-text-secondary hover:border-primary hover:text-primary';
  const btnDisabled = 'disabled:cursor-not-allowed disabled:opacity-40';

  return (
    <div className={cn('flex items-center justify-end gap-1 pt-4', className)}>
      <button
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
        className={cn(btnBase, btnNormal, btnDisabled)}
      >
        &lt;
      </button>

      {pages.map((item, idx) =>
        item === 'ellipsis' ? (
          <span
            key={`ellipsis-${idx}`}
            className="flex size-8 items-center justify-center text-sm text-text-secondary"
          >
            ···
          </span>
        ) : (
          <button
            key={item}
            onClick={() => onPageChange(item)}
            className={cn(
              btnBase,
              item === page ? 'bg-primary text-white' : btnNormal
            )}
          >
            {item}
          </button>
        )
      )}

      <button
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
        className={cn(btnBase, btnNormal, btnDisabled)}
      >
        &gt;
      </button>
    </div>
  );
}

function buildPageNumbers(current: number, total: number): (number | 'ellipsis')[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  const pages: (number | 'ellipsis')[] = [];

  if (current <= 4) {
    for (let i = 1; i <= 5; i++) pages.push(i);
    pages.push('ellipsis');
    pages.push(total);
  } else if (current >= total - 3) {
    pages.push(1);
    pages.push('ellipsis');
    for (let i = total - 4; i <= total; i++) pages.push(i);
  } else {
    pages.push(1);
    pages.push('ellipsis');
    for (let i = current - 1; i <= current + 1; i++) pages.push(i);
    pages.push('ellipsis');
    pages.push(total);
  }

  return pages;
}
