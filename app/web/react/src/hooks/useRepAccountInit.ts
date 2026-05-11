'use client';

import { useEffect, useRef } from 'react';
import { fetchApiRoute } from '@/lib/api/browser-client';
import { useRepStore } from '@/stores/repStore';
import type { RepAccount } from '@/types/rep';
import type { Account as LiveAccount } from '@/types/accounts';

export function useRepAccountInit() {
  const hasFetched = useRef(false);

  useEffect(() => {
    const state = useRepStore.getState();
    if (hasFetched.current) return;
    if (state.repAccountList.length > 0) {
      if (!state.isInitialized) state.setInitialized(true);
      hasFetched.current = true;
      return;
    }
    hasFetched.current = true;

    (async () => {
      try {
        // 使用 Route Handler 替代 Server Action，避免触发 RSC 导航
        const result = await fetchApiRoute<LiveAccount[]>('/api/account/live', { roles: [110] });
        if (result.success && Array.isArray(result.data) && result.data.length > 0) {
          const accounts: RepAccount[] = result.data.map((acc) => ({
            uid: acc.uid,
            currencyId: acc.currencyId ?? 0,
            fundType: acc.fundType ?? 0,
            role: acc.role ?? 0,
            type: acc.type,
            name: acc.name,
            siteId: acc.siteId,
            hasLevelRule: acc.hasLevelRule ?? false,
            group: acc.group,
            alias: acc.alias,
            tradeAccount: acc.tradeAccount,
          }));
          const latestState = useRepStore.getState();
          latestState.setRepAccountList(accounts);
          if (!latestState.repAccount) {
            let restored: RepAccount | undefined;
            try {
              const raw = localStorage.getItem('rep-storage');
              if (raw) {
                const saved = JSON.parse(raw) as RepAccount;
                restored = accounts.find((a) => a.uid === saved.uid);
              }
            } catch {}
            latestState.setRepAccount(restored ?? accounts[0]);
          }
        }
      } catch (err) {
        console.error('[useRepAccountInit] 拉取账号失败:', err);
      } finally {
        useRepStore.getState().setInitialized(true);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}
