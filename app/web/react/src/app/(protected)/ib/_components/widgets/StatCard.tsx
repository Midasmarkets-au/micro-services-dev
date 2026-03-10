'use client';

import Link from 'next/link';
import Image from 'next/image';

interface StatCardProps {
  title: string;
  value: React.ReactNode;
  loading?: boolean;
  className?: string;
  icon?: React.ReactNode;
  iconSrc?: string;
  centered?: boolean;
  viewMoreHref?: string;
  viewMoreLabel?: string;
  extra?: React.ReactNode;
  children?: React.ReactNode;
}

export function StatCard({
  title,
  value,
  loading,
  className = '',
  icon,
  iconSrc,
  centered,
  viewMoreHref,
  viewMoreLabel,
  extra,
  children,
}: StatCardProps) {
  const renderIcon = () => {
    if (iconSrc) {
      return <Image src={iconSrc} alt="" width={28} height={28} className="shrink-0 dark:brightness-0 dark:invert" />;
    }
    if (icon) {
      return (
        <span className="flex size-5 shrink-0 items-center justify-center text-primary">
          {icon}
        </span>
      );
    }
    return null;
  };

  if (centered) {
    return (
      <div className={`flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface ${className}`}>
        <div className="flex flex-1 flex-col items-center p-5">
          {/* Header row with optional right action */}
          {(viewMoreHref || extra) && (
            <div className="mb-2 flex w-full items-center justify-between">
              <div />
              <div className="flex items-center gap-2">
                {viewMoreHref && (
                  <Link href={viewMoreHref} className="text-xs text-text-secondary hover:text-primary">
                    {viewMoreLabel || 'View More'} &gt;
                  </Link>
                )}
                {extra}
              </div>
            </div>
          )}

          {/* Icon + Title */}
          <div className="flex items-center gap-2">
            {renderIcon()}
            <span className="text-xl font-semibold text-text-primary">{title}</span>
          </div>

          {/* Value */}
          <div className="mt-3">
            {loading ? (
              <div className="mx-auto h-10 w-32 animate-pulse rounded bg-border" />
            ) : (
              <div className="text-responsive-3xl font-bold text-primary">{value}</div>
            )}
          </div>
        </div>

        {/* Chart area: centered, design size */}
        {children && (
          <div className="flex w-full items-end justify-center pb-2">
            {children}
          </div>
        )}
      </div>
    );
  }

  return (
    <div className={`flex h-full flex-col rounded-xl border border-border bg-surface p-5 ${className}`}>
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          {renderIcon()}
          <span className="text-sm text-text-secondary">{title}</span>
        </div>
        <div className="flex items-center gap-2">
          {viewMoreHref && (
            <Link href={viewMoreHref} className="text-xs text-text-secondary hover:text-primary">
              {viewMoreLabel || 'View More'} &gt;
            </Link>
          )}
          {extra}
        </div>
      </div>
      <div className="mt-2">
        {loading ? (
          <div className="h-8 w-28 animate-pulse rounded bg-border" />
        ) : (
          <div className="text-2xl font-bold text-primary">{value}</div>
        )}
      </div>
      {children && <div className="mt-auto pt-3">{children}</div>}
    </div>
  );
}
