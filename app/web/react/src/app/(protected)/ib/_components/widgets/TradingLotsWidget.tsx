'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBTradeReports } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { EmptyState } from '@/components/ui';
import type { IBTradeRecord } from '@/types/ib';

export function TradingLotsWidget() {
  const t = useTranslations('ib.dashboard');
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);

  const [trades, setTrades] = useState<IBTradeRecord[]>([]);
  const [loadedUid, setLoadedUid] = useState<number | null>(null);

  const isLoading = !agentAccount || agentAccount.uid !== loadedUid;

  useEffect(() => {
    if (!agentAccount) return;
    let cancelled = false;

    const load = async () => {
      const result = await execute(getIBTradeReports, agentAccount.uid, {
        page: 1,
        size: 5,
      });
      if (cancelled) return;
      if (result.success && result.data?.data) {
        setTrades(Array.isArray(result.data.data) ? result.data.data : []);
      }
      setLoadedUid(agentAccount.uid);
    };

    load();
    return () => { cancelled = true; };
  }, [agentAccount, execute]);

  return (
    <div className="flex h-full flex-col rounded-xl border border-border bg-surface p-5">
      <div className="flex items-center justify-between">
        <h3 className="text-xl font-semibold text-text-primary">{t('tradingLots')}</h3>
        <Link href="/ib/trade" className="flex items-center gap-1 text-base text-text-secondary hover:text-primary">
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
      ) : trades.length === 0 ? (
        <EmptyState message={t('noRecords')} className="flex-1 py-0" />
      ) : (
        <div className="flex flex-1 flex-col gap-2">
          {trades.slice(0, 5).map((trade, idx) => (
            <div key={trade.id ?? idx} className="flex items-center justify-between rounded-lg bg-surface-secondary px-3 py-2">
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm text-text-primary">{trade.symbol || '-'}</p>
                <p className="text-xs text-text-secondary">#{trade.ticket}</p>
              </div>
              <span className="text-sm font-medium text-primary">
                {(trade.volume ?? 0).toFixed(2)} {t('lots')}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
