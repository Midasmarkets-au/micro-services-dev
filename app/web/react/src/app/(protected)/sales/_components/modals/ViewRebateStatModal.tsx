'use client';

import { useCallback } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { fetchAction } from '@/lib/api/browser-client';
import type { SalesClientAccount, SalesChildStat } from '@/types/sales';
import { AccountRoleTypes } from '@/types/accounts';
import {
  ViewRebateStatModal as SharedViewRebateStatModal,
  type RebateStatFormat,
} from '@/components/rebate/ViewRebateStatModal';

interface ViewRebateStatModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}

export function ViewRebateStatModal({ open, onOpenChange, account }: ViewRebateStatModalProps) {
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const isSales = account?.role === AccountRoleTypes.Sales;
  // 客户与代理都走 symbol-grouped 返佣接口，对应 MM-Front ViewRebateStat 里非 Sales 的分支
  const rebateStatFormat: RebateStatFormat = isSales ? 'sales' : 'ib';

  const fetchChildStat = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!salesAccount) return { success: false as const, data: null };
    const params: Record<string, unknown> = { uid };
    if (from) params.from = from;
    if (to) params.to = to;
    return execute(() =>
      fetchAction<SalesChildStat>('getSalesChildStat', salesAccount.uid, params),
    );
  }, [salesAccount, execute]);

  const fetchRebateStat = useCallback(async (uid: number, from?: string, to?: string) => {
    if (!salesAccount) return { success: false as const, data: null };
    const params: Record<string, unknown> = { uid };
    if (from) params.from = from;
    if (to) params.to = to;
    params.format = 'rows';

    type RebateStatRow = {
      symbol?: string;
      currencyId?: number;
      volume?: number;
      profit?: number;
      amounts?: Record<string, number>;
    };

    if (isSales) {
      return execute(() =>
        fetchAction<RebateStatRow[]>('getSalesRebateStatBySymbol', salesAccount.uid, params),
      );
    }
    return execute(() =>
      fetchAction<RebateStatRow[]>('getSalesIbRebateStatBySymbol', salesAccount.uid, params),
    );
  }, [salesAccount, execute, isSales]);

  return (
    <SharedViewRebateStatModal
      open={open}
      onOpenChange={onOpenChange}
      account={account}
      translationNamespace="sales"
      fetchChildStat={fetchChildStat}
      fetchRebateStat={fetchRebateStat}
      rebateStatFormat={rebateStatFormat}
      truncateTotalsToTwoDecimals
    />
  );
}
