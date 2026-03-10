'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import Image from 'next/image';
import { Button } from '@/components/ui';
import { useTheme } from '@/hooks/useTheme';

export default function NoPermissionPage() {
  const t = useTranslations('errors.noPermission');
  const router = useRouter();
  const { theme } = useTheme();

  const handleGoBack = () => {
    router.back();
  };

  const handleGoHome = () => {
    router.push('/dashboard');
  };

  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-8 p-8 bg-surface">
      {/* Icon */}
      <div className="relative w-[120px] h-[120px] md:w-[160px] md:h-[160px]">
        <Image
          src={theme === 'dark' ? '/images/icons/error-night.svg' : '/images/icons/error-day.svg'}
          alt="No Permission"
          fill
          className="object-contain"
        />
      </div>

      {/* Title */}
      <h1 className="text-responsive-3xl font-bold text-text-primary text-center">
        {t('title')}
      </h1>

      {/* Description */}
      <p className="text-base text-text-secondary text-center max-w-md">
        {t('description')}
      </p>

      {/* Action buttons */}
      <div className="flex flex-col sm:flex-row gap-4">
        <Button  variant="secondary" onClick={handleGoBack}>
          {t('goBack')}
        </Button>
        <Button variant="primary" onClick={handleGoHome}>
          {t('goHome')}
        </Button>
      </div>
    </div>
  );
}
