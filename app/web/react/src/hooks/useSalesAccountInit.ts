'use client';

import { useEffect, useRef } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { getLiveAccounts } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import type { SalesAccount } from '@/types/sales';

export function useSalesAccountInit() {
  const { execute } = useServerAction({ showErrorToast: true });
  const hasFetched = useRef(false);

  useEffect(() => {
    const { salesAccountList } = useSalesStore.getState();
    if (hasFetched.current || salesAccountList.length > 0) return;
    hasFetched.current = true;

    (async () => {
      const result = await execute(getLiveAccounts, { roles: [100] });
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
        const state = useSalesStore.getState();
        state.setSalesAccountList(accounts);
        if (!state.salesAccount) {
          let restored: SalesAccount | undefined;
          try {
            const raw = localStorage.getItem('sales-storage');
            if (raw) {
              const saved = JSON.parse(raw) as SalesAccount;
              restored = accounts.find((a) => a.uid === saved.uid);
            }
          } catch {}
          state.setSalesAccount(restored ?? accounts[0]);
        }
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}
