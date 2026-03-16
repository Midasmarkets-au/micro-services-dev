'use client';

import { useTranslations } from 'next-intl';
import Image from 'next/image';

interface VerificationCompleteProps {
  status: 'pending' | 'approved' | 'rejected';
  rejectionReason?: string;
}

export function VerificationComplete({ status, rejectionReason }: VerificationCompleteProps) {
  const t = useTranslations('verification');

  // 根据状态选择不同的文本
  const statusConfig = {
    pending: {
      title: 'successTitle',
      description: 'successDescription',
      iconDay: '/images/icons/success-day.svg',
      iconNight: '/images/icons/success-night.svg',
    },
    approved: {
      title: 'approvedTitle',
      description: 'approvedDescription',
      iconDay: '/images/icons/success-day.svg',
      iconNight: '/images/icons/success-night.svg',
    },
    rejected: {
      title: 'rejectedTitle',
      description: 'rejectedDescription',
      iconDay: '/images/icons/error-day.svg',
      iconNight: '/images/icons/error-night.svg',
    },
  };

  const config = statusConfig[status];

  return (
    <div className="flex flex-col items-center px-5 py-10">
      {/* 内容容器 */}
      <div className="flex flex-col items-center justify-center overflow-hidden rounded-[20px] p-10 w-[300px]">
        <div className="flex flex-col items-center gap-5 w-full">
          {/* 图标 - 日间/夜间模式 */}
          <div className="relative size-[120px] overflow-hidden rounded-[12px]">
            <Image
              src={config.iconDay}
              alt="Status"
              fill
              className="object-contain dark:hidden"
            />
            <Image
              src={config.iconNight}
              alt="Status"
              fill
              className="hidden object-contain dark:block"
            />
          </div>

          {/* 标题 */}
          <h2 className="text-xl font-semibold text-text-primary text-center">
            {t(`complete.${config.title}`)}
          </h2>

          {/* 描述 */}
          <p className="text-sm text-text-secondary text-center whitespace-nowrap">
            {t(`complete.${config.description}`)}
          </p>

          {/* 拒绝原因 */}
          {status === 'rejected' && rejectionReason && (
            <div className="mt-4 w-full rounded border border-error/30 bg-error/10 p-4">
              <p className="text-sm font-medium text-error">
                {t('complete.rejectionReason')}:
              </p>
              <p className="mt-1 text-sm text-text-secondary">
                {rejectionReason}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
