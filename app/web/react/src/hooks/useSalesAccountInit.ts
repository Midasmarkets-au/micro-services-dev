'use client';

import { useEffect, useRef } from 'react';
import { fetchApiRoute } from '@/lib/api/browser-client';
import { useSalesStore } from '@/stores/salesStore';
import type { SalesAccount } from '@/types/sales';

// Account type fragment for mapping (matches getLiveAccounts return shape)
type LiveAccount = {
  uid: number;
  currencyId?: number;
  fundType?: number;
  role?: number;
  type?: number;
  name?: string;
  siteId?: number;
  hasLevelRule?: boolean;
  group?: string;
  alias?: string;
  tradeAccount?: string;
  code?: string;
  createdOn?: string;
};

export function useSalesAccountInit() {
  const hasFetched = useRef(false);

  useEffect(() => {
    const state = useSalesStore.getState();
    if (hasFetched.current) return;
    if (state.salesAccountList.length > 0) {
      if (!state.isInitialized) state.setInitialized(true);
      hasFetched.current = true;
      return;
    }
    hasFetched.current = true;

    (async () => {
      try {
        // 使用 Route Handler 替代 Server Action，避免触发 RSC 导航
        const result = await fetchApiRoute<LiveAccount[]>('/api/account/live', { roles: [100] });
        if (result.success && Array.isArray(result.data) && result.data.length > 0) {
          const accounts: SalesAccount[] = result.data.map((acc) => ({
            uid: acc.uid,
            currencyId: acc.currencyId,
            fundType: acc.fundType,
            role: acc.role ?? 0,
            type: acc.type,
            name: acc.name,
            siteId: acc.siteId,
            hasLevelRule: acc.hasLevelRule ?? false,
            salesSelfGroupName: acc.group,
            alias: acc.alias,
            tradeAccount: acc.tradeAccount,
          }));
          const latestState = useSalesStore.getState();
          latestState.setSalesAccountList(accounts);
          if (!latestState.salesAccount) {
            let restored: SalesAccount | undefined;
            try {
              const raw = localStorage.getItem('sales-storage');
              if (raw) {
                const saved = JSON.parse(raw) as SalesAccount;
                restored = accounts.find((a) => a.uid === saved.uid);
              }
            } catch {}
            latestState.setSalesAccount(restored ?? accounts[0]);
          }
        }
      } catch (err) {
        console.error('[useSalesAccountInit] 拉取账号失败:', err);
      } finally {
        useSalesStore.getState().setInitialized(true);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}

