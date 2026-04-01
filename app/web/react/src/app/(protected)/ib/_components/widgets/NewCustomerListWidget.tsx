'use client';

import { useState, useEffect, useRef } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { usePathname } from 'next/navigation';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBReferralHistory } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { Avatar, EmptyState } from '@/components/ui';
import type { IBReferralHistory } from '@/types/ib';

export function NewCustomerListWidget() {
  const t = useTranslations('ib.dashboard');
  const pathname = usePathname();
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const requestIdRef = useRef(0);

  const [customers, setCustomers] = useState<IBReferralHistory[]>([]);
  const [loadedUid, setLoadedUid] = useState<number | null>(null);

  const isLoading = !agentAccount || agentAccount.uid !== loadedUid;

  useEffect(() => {
    if (!agentAccount || pathname !== '/ib') return;
    let cancelled = false;
    const currentRequestId = ++requestIdRef.current;

    const load = async () => {
      try {
        const result = await execute(getIBReferralHistory, agentAccount.uid, {
          page: 1,
          size: 5,
          IsUnverified: true,
        });
        if (cancelled || requestIdRef.current !== currentRequestId || pathname !== '/ib') return;
        if (result.success && result.data?.data) {
          setCustomers(Array.isArray(result.data.data) ? result.data.data : []);
        }
      } catch {
        // ignore
      } finally {
        if (!cancelled && requestIdRef.current === currentRequestId && pathname === '/ib') {
          setLoadedUid(agentAccount.uid);
        }
      }
    };

    load();
    return () => {
      cancelled = true;
    };
  }, [agentAccount, execute, pathname]);

  return (
    <div className="flex h-full flex-col rounded-xl border border-border bg-surface p-5">
      <div className="flex items-center justify-between">
        <h3 className="text-xl font-semibold text-text-primary">{t('newCustomers')}</h3>
        <Link href="/ib/new-customers" className="flex items-center gap-1 text-base text-text-secondary hover:text-primary">
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
