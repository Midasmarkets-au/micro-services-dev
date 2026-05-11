'use client';

import { useCallback, useEffect, useMemo, useRef } from 'react';
import { fetchApiRoute } from '@/lib/api/browser-client';
import { useIBStore } from '@/stores/ibStore';
import { useUserStore } from '@/stores/userStore';
import type { AgentAccount } from '@/types/ib';
import type { Account as LiveAccount } from '@/types/accounts';

export function useIBAccountInit() {
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

  const beginScope = useCallback(() => {
    const myToken = ++tokenRef.current;
    const isActive = () => myToken === tokenRef.current;
    return { isActive };
  }, []);

  useEffect(() => {
    return () => {
      tokenRef.current += 1;
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

    const { isActive } = beginScope();
    const uids = user.ibAccount.map((uid) => Number(uid));

    (async () => {
      try {
        // 使用 Route Handler 替代 Server Action，避免触发 RSC 导航
        const result = await fetchApiRoute<LiveAccount[]>('/api/account/live', { uids });
        if (!isActive()) return;

        if (result.success && Array.isArray(result.data) && result.data.length > 0) {
          const accounts: AgentAccount[] = result.data.map((acc) => ({
            uid: acc.uid,
            currencyId: acc.currencyId ?? 0,
            fundType: acc.fundType ?? 0,
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
        } else if (!result.success) {
          if (isActive()) useIBStore.getState().clearStore();
        }
      } catch (err) {
        console.error('[useIBAccountInit] 拉取账号失败:', err);
        if (isActive()) useIBStore.getState().clearStore();
      } finally {
        if (isActive()) useIBStore.getState().setInitialized(true);
      }
    })();
    // user.ibAccount 的内容已通过 ibAccountKey 表达；其他引用都是稳定的
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ibAccountKey, beginScope]);
}
