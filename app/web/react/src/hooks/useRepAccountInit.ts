'use client';

import { useEffect, useRef } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { getLiveAccounts } from '@/actions';
import { useRepStore } from '@/stores/repStore';
import type { RepAccount } from '@/types/rep';

export function useRepAccountInit() {
  const { execute } = useServerAction({ showErrorToast: true });
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
        const result = await execute(getLiveAccounts, { roles: [110] });
        if (result.success && Array.isArray(result.data) && result.data.length > 0) {
          const accounts: RepAccount[] = result.data.map((acc) => ({
            uid: acc.uid,
            currencyId: acc.currencyId,
            fundType: acc.fundType,
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
      } finally {
        useRepStore.getState().setInitialized(true);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}
