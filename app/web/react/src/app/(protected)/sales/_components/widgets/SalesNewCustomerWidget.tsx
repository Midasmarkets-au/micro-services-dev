'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesReferralHistory } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { Avatar, EmptyState } from '@/components/ui';
import type { SalesReferralHistory } from '@/types/sales';

export function SalesNewCustomerWidget() {
  const t = useTranslations('sales.dashboard');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [customers, setCustomers] = useState<SalesReferralHistory[]>([]);
  const [loadedUid, setLoadedUid] = useState<number | null>(null);

  const isLoading = !salesAccount || salesAccount.uid !== loadedUid;

  useEffect(() => {
    if (!salesAccount) return;
    let cancelled = false;

    const load = async () => {
      const result = await execute(getSalesReferralHistory, salesAccount.uid, {
        page: 1,
        size: 5,
      });
      if (cancelled) return;
      if (result.success && result.data?.data) {
        setCustomers(Array.isArray(result.data.data) ? result.data.data : []);
      }
      setLoadedUid(salesAccount.uid);
    };

    load();
    return () => { cancelled = true; };
  }, [salesAccount, execute]);

  return (
    <div className="flex h-full flex-col rounded-xl border border-border bg-surface p-5">
      <div className="flex items-center justify-between">
        <h3 className="text-xl font-semibold text-text-primary">{t('newCustomers')}</h3>
        <Link href="/sales/new-customers" className="flex items-center gap-1 text-base text-text-secondary hover:text-primary">
          {t('viewMore')} &gt;
        </Link>
      </div>

      <div className="my-5 h-px w-full bg-border" />

      {isLoading ? (
        <div className="flex flex-1 flex-col gap-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-10 animate-pulse rounded bg-border" />
          ))}
        </div>
      ) : customers.length === 0 ? (
        <EmptyState message={t('noRecords')} className="flex-1 py-0" />
      ) : (
        <div className="flex flex-1 flex-col gap-2">
          {customers.slice(0, 5).map((c, idx) => (
            <div key={c.id ?? idx} className="flex items-center gap-2">
              <Avatar src={c.avatar || c.user?.avatar} alt={c.userName || c.user?.displayName} size="xs" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm text-text-primary">
                  {c.userName || c.user?.displayName || '-'}
                </p>
                <p className="truncate text-xs text-text-secondary">{c.email || c.user?.email}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
