'use client';

import { useEffect, useRef } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { getLiveAccounts } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { useUserStore } from '@/stores/userStore';
import type { AgentAccount } from '@/types/ib';

export function useIBAccountInit() {
  const { execute } = useServerAction({ showErrorToast: true });
  const user = useUserStore((s) => s.user);
  const hasFetched = useRef(false);

  useEffect(() => {
    const state = useIBStore.getState();
    const { agentAccountList } = state;
    const currentUids = (user?.ibAccount ?? []).map(Number).sort();
    const cachedUids = agentAccountList.map((a) => a.uid).sort();
    const isStale =
      agentAccountList.length > 0 &&
      (currentUids.length !== cachedUids.length ||
        currentUids.some((uid, i) => uid !== cachedUids[i]));

    if (isStale) {
      useIBStore.getState().clearStore();
      hasFetched.current = false;
    }

    if (hasFetched.current) return;
    if (!isStale && agentAccountList.length > 0) {
      if (!state.isInitialized) state.setInitialized(true);
      hasFetched.current = true;
      return;
    }
    if (!user?.ibAccount?.length) {
      useIBStore.getState().setInitialized(true);
      return;
    }
    hasFetched.current = true;

    (async () => {
      try {
        const uids = user.ibAccount.map((uid) => Number(uid));
        const result = await execute(getLiveAccounts, { uids });
        if (result.success && Array.isArray(result.data) && result.data.length > 0) {
          const accounts: AgentAccount[] = result.data.map((acc) => ({
            uid: acc.uid,
            currencyId: acc.currencyId,
            fundType: acc.fundType,
            role: acc.role ?? 0,
            type: acc.type,
            name: acc.name,
            siteId: acc.siteId,
            hasLevelRule: acc.hasLevelRule ?? false,
            salesGroupName: acc.code,
            createdOn: acc.createdOn,
            agentSelfGroupName: acc.group,
            alias: acc.alias,
            tradeAccount: acc.tradeAccount,
          }));
          const latestState = useIBStore.getState();
          latestState.setAgentAccountList(accounts);
          const cachedUid = latestState.agentAccount?.uid;
          const stillValid = cachedUid && accounts.some((a) => a.uid === cachedUid);
          if (!stillValid) {
            let restored: AgentAccount | undefined;
            try {
              const raw = localStorage.getItem('ib-storage');
              if (raw) {
                const saved = JSON.parse(raw) as AgentAccount;
                restored = accounts.find((a) => a.uid === saved.uid);
              }
            } catch {}
            latestState.setAgentAccount(restored ?? accounts[0]);
          }
        } else {
          useIBStore.getState().clearStore();
        }
      } finally {
        useIBStore.getState().setInitialized(true);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.ibAccount]);
}
