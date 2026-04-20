'use client';

import { useCallback, useEffect, useMemo, useRef } from 'react';
import { useBrowserAction } from '@/lib/http';
import { getLiveAccounts } from '@/lib/http/browserActions/accounts';
import { useIBStore } from '@/stores/ibStore';
import { useUserStore } from '@/stores/userStore';
import type { AgentAccount } from '@/types/ib';

export function useIBAccountInit() {
  const { execute } = useBrowserAction({ showErrorToast: true });
  const user = useUserStore((s) => s.user);

  // 把 ibAccount 数组变成稳定的字符串 key：
  //   - 同一账号集合 → 同一 key → effect 不会重复触发，不需要 hasFetched 守卫
  //   - 账号集合变化 → key 变化 → 正常触发重新拉取
  const ibAccountKey = useMemo(
    () => (user?.ibAccount ?? []).map(Number).sort((a, b) => a - b).join(','),
    [user?.ibAccount]
  );

  // token + controller：和 useRouteScope 同构，用来并发去重 + 安全取消
  const tokenRef = useRef(0);
  const controllerRef = useRef<AbortController | null>(null);

  const beginScope = useCallback(() => {
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const myToken = ++tokenRef.current;
    const isActive = () =>
      myToken === tokenRef.current && !controller.signal.aborted;
    return { signal: controller.signal, isActive };
  }, []);

  useEffect(() => {
    return () => {
      tokenRef.current += 1;
      controllerRef.current?.abort();
      controllerRef.current = null;
    };
  }, []);

  useEffect(() => {
    const state = useIBStore.getState();
    const { agentAccountList } = state;
    const currentUids = (user?.ibAccount ?? []).map(Number).sort((a, b) => a - b);
    const cachedUids = agentAccountList.map((a) => a.uid).sort((a, b) => a - b);
    const isStale =
      agentAccountList.length > 0 &&
      (currentUids.length !== cachedUids.length ||
        currentUids.some((uid, i) => uid !== cachedUids[i]));

    if (isStale) {
      useIBStore.getState().clearStore();
    }

    // 缓存命中：账号列表没变且已有数据，只补一次 initialized 标记
    if (!isStale && agentAccountList.length > 0) {
      if (!state.isInitialized) state.setInitialized(true);
      return;
    }

    // 用户根本没有 IB 账号：直接标记 initialized，结束
    if (!user?.ibAccount?.length) {
      useIBStore.getState().setInitialized(true);
      return;
    }

    const { signal, isActive } = beginScope();

    (async () => {
      const uids = user.ibAccount!.map((uid) => Number(uid));
      const result = await execute(getLiveAccounts, { signal }, { uids });
      if (!isActive()) return;

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
      } else if (!result.aborted) {
        useIBStore.getState().clearStore();
      }

      useIBStore.getState().setInitialized(true);
    })();
    // user.ibAccount 的内容已通过 ibAccountKey 表达；其他引用都是稳定的
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ibAccountKey, execute, beginScope]);
}
