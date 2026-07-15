'use client';

import { useCallback } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { useIBStore } from '@/stores/ibStore';
import { fetchAction } from '@/lib/api/browser-client';
import type { IBChildStat, IBClientAccount } from '@/types/ib';
import { ViewRebateStatModal as SharedViewRebateStatModal } from '@/components/rebate/ViewRebateStatModal';

interface ViewRebateStatModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: IBClientAccount | null;
}

export function ViewRebateStatModal({ open, onOpenChange, account }: ViewRebateStatModalProps) {
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);

  const fetchChildStat = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!agentAccount) return { success: false as const, data: null };
    const params: Record<string, unknown> = { uid };
    if (from) params.from = from;
    if (to) params.to = to;
    return execute(() =>
      fetchAction<IBChildStat>('getIBChildStat', agentAccount.uid, params),
    );
  }, [agentAccount, execute]);

  const fetchRebateStat = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!agentAccount) return { success: false as const, data: null };
    const params: Record<string, unknown> = { uid };
    if (from) params.from = from;
    if (to) params.to = to;
    return execute(() =>
      fetchAction<Array<{
        symbol?: string;
        currencyId?: number;
        volume?: number;
        profit?: number;
        amounts?: Record<string, number>;
      }>>('getIBRebateStatBySymbol', agentAccount.uid, params),
    );
  }, [agentAccount, execute]);

  return (
    <SharedViewRebateStatModal
      open={open}
      onOpenChange={onOpenChange}
      account={account}
      translationNamespace="ib"
      fetchChildStat={fetchChildStat}
      fetchRebateStat={fetchRebateStat}
      rebateStatFormat="ib"
    />
  );
}
