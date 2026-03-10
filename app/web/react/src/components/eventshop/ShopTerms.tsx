'use client';

import Image from 'next/image';
import { useTranslations } from 'next-intl';
import type { EventDetail } from '@/types/eventshop';

interface ShopTermsProps {
  eventDetail: EventDetail;
}

export function ShopTerms({ eventDetail }: ShopTermsProps) {
  const t = useTranslations('eventshop');

  return (
    <div className="flex-1 bg-surface rounded flex flex-col gap-5 overflow-hidden p-5 min-w-0">
      <div className="flex flex-col gap-5">
        <div className="flex items-center gap-3">
          <Image
            src="/images/eventshop/shield-icon.svg"
            alt=""
            width={26}
            height={26}
            className="shrink-0"
          />
          <h2 className="text-xl font-semibold text-text-primary">
            {t('terms.title')}
          </h2>
        </div>
        <div className="h-px bg-border" />
      </div>

      {eventDetail.term ? (
        <div
          className="text-sm leading-relaxed text-text-secondary [&_a]:text-primary [&_a]:underline [&_h1]:text-xl [&_h1]:font-semibold [&_h1]:mb-3 [&_h2]:text-lg [&_h2]:font-semibold [&_h2]:mb-2 [&_p]:mb-2 [&_ul]:list-disc [&_ul]:pl-5 [&_li]:mb-1"
          dangerouslySetInnerHTML={{ __html: eventDetail.term }}
        />
      ) : (
        <p className="text-text-tertiary text-sm">{t('noData')}</p>
      )}
    </div>
  );
}
