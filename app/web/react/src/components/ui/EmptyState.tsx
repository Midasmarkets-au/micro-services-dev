'use client';

import { useTranslations } from 'next-intl';

export interface EmptyStateProps {
  message?: string;
  className?: string;
}

export function EmptyState({ message, className = '' }: EmptyStateProps) {
  const t = useTranslations('common');
  return (
    <div className={`flex flex-col items-center justify-center gap-3 py-10 ${className}`}>
      <div className="no-data-icon" />
      <span className="text-base text-text-secondary">{message || t('noData')}</span>
    </div>
  );
}
