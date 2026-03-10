'use client';

import Link from 'next/link';
import { useTranslations } from 'next-intl';

export function AuthFooter() {
  const t = useTranslations('footer');

  return (
    <footer className="fixed bottom-0 left-0 right-0 flex h-16 items-center justify-center text-sm">
      <Link href="/lead-create" className="flex items-center transition-colors hover:opacity-80">
        <span className="text-text-secondary">{t('contactUs')}:</span>
        <span className="ml-1 text-text-primary">info@midasmkts.com</span>
      </Link>
    </footer>
  );
}

