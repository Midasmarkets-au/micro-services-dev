'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getRepClients, repFuzzySearchAccount, getRepViewEmailCode, getRepEmailByCode } from '@/actions';
import { useRepStore } from '@/stores/repStore';
import { AccountRoleTypes } from '@/types/accounts';
import {
  Avatar,
  BalanceShow,
  Button,
  Skeleton,
  DataTable,
  Pagination,
  Icon,
  SearchFilter,
} from '@/components/ui';
import type { DataTableColumn, SearchFilterResult } from '@/components/ui';
import type { RepClientAccount, RepClientCriteria } from '@/types/rep';
import { UnlockEmailAddressModal } from '@/components/user/UnlockEmailAddressModal';

function getUserName(item: RepClientAccount): string {
  const u = item.user;
  if (u?.nativeName && u.nativeName.trim()) return u.nativeName;
  if (u?.displayName && u.displayName.trim()) return u.displayName;
  if (u?.firstName && u?.lastName && u.firstName.trim() && u.lastName.trim()) {
    return `${u.firstName} ${u.lastName}`;
  }
  return 'No Name';
}

const INITIAL_CRITERIA: RepClientCriteria = {
  page: 1,
  size: 10,
  role: AccountRoleTypes.Sales,
};

export default function RepCustomersPage() {
  const t = useTranslations('rep');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const repAccount = useRepStore((s) => s.repAccount);

  const [customers, setCustomers] = useState<RepClientAccount[]>([]);
  const [criteria, setCriteria] = useState<RepClientCriteria>(INITIAL_CRITERIA);
  const [isLoading, setIsLoading] = useState(true);
  const [ibChain, setIbChain] = useState<RepClientAccount[]>([]);
  const [searchTrigger, setSearchTrigger] = useState(0);
  const [isSearch, setIsSearch] = useState(false);

  // 解锁邮箱弹窗状态
  const [unlockEmailOpen, setUnlockEmailOpen] = useState(false);
  const [unlockEmailUid, setUnlockEmailUid] = useState<number | null>(null);
  const [unlockEmailAddress, setUnlockEmailAddress] = useState<string | undefined>(undefined);
  const [unlockKey, setUnlockKey] = useState(0);

  const isSelectingClient = criteria.role === AccountRoleTypes.Client;
  const isDrilledIn = criteria.role !== AccountRoleTypes.Sales;

  const criteriaRef = useRef(criteria);
  criteriaRef.current = criteria;

  const ibChainRef = useRef(ibChain);
  ibChainRef.current = ibChain;

  // ============================================
  // 数据获取
  // ============================================
  const fetchData = useCallback(
    async (page: number, overrideCriteria?: Partial<RepClientCriteria>) => {
      if (!repAccount) return;
      setIsLoading(true);
      const params = { ...criteriaRef.current, ...overrideCriteria, page };
      try {
        const result = await execute(getRepClients, repAccount.uid, params);
        if (result.success && result.data) {
          const raw = result.data.criteria || params;
          setCriteria({
            page: raw.page ?? 1,
            size: raw.size ?? 10,
            total: raw.total,
            role: raw.role ?? params.role,
            childParentAccountUid: raw.childParentAccountUid,
          });
          setCustomers(Array.isArray(result.data.data) ? result.data.data : []);
        }
      } catch (e) {
        console.error(e);
      }
      setIsLoading(false);
    },
    [repAccount, execute]
  );

  // 初始化加载
  useEffect(() => {
    if (repAccount) {
      fetchData(1, INITIAL_CRITERIA);
    }
  }, [repAccount]); // eslint-disable-line react-hooks/exhaustive-deps

  // ============================================
  // IB/Client 角色切换（仅在钻入子账户后可见）
  // ============================================
  const handleRoleToggle = useCallback((role: number) => {
    const newCriteria: RepClientCriteria = {
      page: 1,
      size: 10,
      role,
      childParentAccountUid: criteriaRef.current.childParentAccountUid,
    };
    setCriteria(newCriteria);
    criteriaRef.current = newCriteria;
    setSearchTrigger((prev) => prev + 1);
    fetchData(1, newCriteria);
  }, [fetchData]);

  // ============================================
  // IB 钻入逻辑
  // ============================================
  const handleIbSearch = useCallback((ibAccount: RepClientAccount) => {
    const newChain = [...ibChainRef.current, ibAccount];
    setIbChain(newChain);
    ibChainRef.current = newChain;

    const newCriteria: RepClientCriteria = {
      page: 1,
      size: 10,
      role: AccountRoleTypes.IB,
      childParentAccountUid: ibAccount.uid,
    };
    setCriteria(newCriteria);
    criteriaRef.current = newCriteria;
    setSearchTrigger((prev) => prev + 1);
    fetchData(1, newCriteria);
  }, [fetchData]);

  const handleGoToLevel = useCallback((idx: number) => {
    if (idx === ibChainRef.current.length - 1) return;
    const newChain = ibChainRef.current.slice(0, idx + 1);
    setIbChain(newChain);
    ibChainRef.current = newChain;

    const newCriteria: RepClientCriteria = {
      page: 1,
      size: 10,
      role: criteriaRef.current.role,
      childParentAccountUid: newChain[idx].uid,
    };
    setCriteria(newCriteria);
    criteriaRef.current = newCriteria;
    fetchData(1, newCriteria);
  }, [fetchData]);

  const handleClearIbSearch = useCallback(() => {
    setIbChain([]);
    ibChainRef.current = [];

    const newCriteria = { ...INITIAL_CRITERIA };
    setCriteria(newCriteria);
    criteriaRef.current = newCriteria;
    setSearchTrigger((prev) => prev + 1);
    fetchData(1, newCriteria);
  }, [fetchData]);

  // ============================================
  // 搜索处理
  // ============================================
  const searchHandler = useCallback(async (keyword: string) => {
    if (!repAccount) return [];
    const params: Record<string, unknown> = {
      ...criteriaRef.current,
      keywords: keyword,
    };
    if (ibChainRef.current.length > 0) {
      params.agentUid = ibChainRef.current[ibChainRef.current.length - 1].uid;
    }
    const result = await repFuzzySearchAccount(repAccount.uid, params);
    if (result.success && result.data) return result.data;
    return [];
  }, [repAccount]);

  const handleSearchResults = useCallback(async (results: SearchFilterResult) => {
    if (!repAccount) return;
    setIsSearch(true);

    if (results.ids.length === 0) {
      setIsSearch(false);
      const baseCriteria: RepClientCriteria = {
        page: 1,
        size: 10,
        role: criteriaRef.current.role ?? AccountRoleTypes.Sales,
      };
      if (ibChainRef.current.length > 0) {
        baseCriteria.childParentAccountUid =
          ibChainRef.current[ibChainRef.current.length - 1].uid;
      }
      setCriteria(baseCriteria);
      criteriaRef.current = baseCriteria;
      fetchData(1, baseCriteria);
      return;
    }

    if (results.criteria) {
      const newCriteria = { ...results.criteria } as RepClientCriteria;
      setCriteria(newCriteria);
      criteriaRef.current = newCriteria;
    }

    setIsLoading(true);
    try {
      const uids = results.data.map((x) => {
        const raw = x as Record<string, unknown>;
        return (raw.accountUid ?? raw.uid ?? raw.id) as number;
      }).filter(Boolean);

      const res = await execute(getRepClients, repAccount.uid, { uids });
      if (res.success && res.data) {
        setCustomers(Array.isArray(res.data.data) ? res.data.data : []);
      }
    } finally {
      setIsLoading(false);
    }
  }, [repAccount, execute, fetchData]);

  // ============================================
  // 邮箱查看
  // ============================================
  const showUnlockEmailAddress = useCallback((uid: number, email?: string) => {
    setUnlockEmailUid(uid);
    setUnlockEmailAddress(email);
    setUnlockKey((k) => k + 1);
    setUnlockEmailOpen(true);
  }, []);

  // ============================================
  // 表格列定义
  // ============================================
  const columns = useMemo<DataTableColumn<RepClientAccount>[]>(() => {
    const cols: DataTableColumn<RepClientAccount>[] = [
      {
        key: 'customer',
        title: t('fields.customer'),
        skeletonRender: () => (
          <div className="flex items-center gap-3">
            <Skeleton className="h-9 w-9 rounded-full" />
            <div className="flex flex-col gap-1">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-3 w-28" />
            </div>
          </div>
        ),
        render: (item) => (
          <div className="flex items-center gap-3">
            <Avatar src={item.user?.avatar} alt={getUserName(item)} size="xs" />
            <span className="text-sm font-medium text-text-primary">{getUserName(item)}</span>
          </div>
        ),
      },
      {
        key: 'accountIdOrUid',
        title: isSelectingClient ? t('fields.accountNo') : t('fields.uid'),
        skeletonWidth: 'w-20',
        render: (item) => (
          <span className="text-sm">
            {item.role === AccountRoleTypes.Client
              ? item.tradeAccount?.accountNumber ?? '-'
              : item.uid}
          </span>
        ),
      },
      {
        key: 'email',
        title: t('fields.email'),
        skeletonWidth: 'w-28',
        render: (item) => (
          <span
            className="cursor-pointer text-sm text-text-secondary hover:text-primary"
            onClick={() => showUnlockEmailAddress(item.uid, item.user?.email)}
          >
            {item.user?.email || '-'}
          </span>
        ),
      },
      {
        key: 'type',
        title: t('fields.type'),
        skeletonWidth: 'w-16',
        render: (item) => (
          <span className="text-sm">
            {tAccount.has(`accountTypes.${item.type}`)
              ? tAccount(`accountTypes.${item.type}`)
              : String(item.type)}
          </span>
        ),
      },
      {
        key: 'group',
        title: t('fields.group'),
        skeletonWidth: 'w-16',
        render: (item) => <span className="text-sm">{item.group || '-'}</span>,
      },
      {
        key: 'code',
        title: t('fields.code'),
        skeletonWidth: 'w-16',
        render: (item) => <span className="text-sm">{item.code || '-'}</span>,
      },
      {
        key: 'createdOn',
        title: t('fields.createdOn'),
        skeletonWidth: 'w-28',
        render: (item) => (
          <span className="text-sm">
            {new Date(item.createdOn).toLocaleString('sv-SE').replace('T', ' ')}
          </span>
        ),
      },
    ];

    if (isSelectingClient) {
      cols.push({
        key: 'balance',
        title: t('fields.balance'),
        skeletonWidth: 'w-20',
        align: 'right',
        render: (item) =>
          item.tradeAccount ? (
            <BalanceShow
              balance={item.tradeAccount.balanceInCents || 0}
              currencyId={item.tradeAccount.currencyId || 840}
              className="text-sm font-semibold text-text-primary"
            />
          ) : (
            <span className="text-sm">-</span>
          ),
      });
    }

    cols.push({
      key: 'actions',
      title: t('fields.actions'),
      skeletonWidth: 'w-20',
      render: (item) => {
        if (criteria.role === AccountRoleTypes.Client) {
          return (
            <Link
              href={`/rep/customers/${item.uid}`}
              className="text-sm text-text-secondary hover:text-primary"
            >
              {t('action.viewDetails')}
            </Link>
          );
        }
        return (
          <button
            type="button"
            onClick={() => handleIbSearch(item)}
            className="text-sm text-text-secondary hover:text-primary"
          >
            {t('action.viewAccounts')}
          </button>
        );
      },
    });

    return cols;
  }, [isSelectingClient, t, tAccount, criteria.role, handleIbSearch, showUnlockEmailAddress]);

  // ============================================
  // 无 Rep 账户提示
  // ============================================
  if (!repAccount) {
    return (
      <div className="flex flex-1 items-center justify-center rounded bg-surface p-10">
        <p className="text-text-secondary">{t('noRepAccount')}</p>
      </div>
    );
  }

  // ============================================
  // 渲染
  // ============================================
  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      {/* 标题栏 + 工具栏 */}
      <div className="flex flex-wrap items-center gap-3 border-b border-border pb-3">
        <h2 className="text-lg font-semibold text-text-primary">{t('fields.accounts')}</h2>

        <div className="ml-auto flex flex-wrap items-center gap-2">
          {/* IB/Client 角色切换（钻入子账户后显示） */}
          {isDrilledIn && (
            <div className="flex overflow-hidden rounded-md border border-border">
              <button
                type="button"
                onClick={() => handleRoleToggle(AccountRoleTypes.IB)}
                className={`px-3 py-1.5 text-sm font-medium transition-colors ${
                  criteria.role === AccountRoleTypes.IB
                    ? 'bg-primary text-white'
                    : 'bg-surface text-text-secondary hover:bg-hover'
                }`}
              >
                IB
              </button>
              <button
                type="button"
                onClick={() => handleRoleToggle(AccountRoleTypes.Client)}
                className={`px-3 py-1.5 text-sm font-medium transition-colors ${
                  criteria.role === AccountRoleTypes.Client
                    ? 'bg-primary text-white'
                    : 'bg-surface text-text-secondary hover:bg-hover'
                }`}
              >
                Client
              </button>
            </div>
          )}

          {/* 搜索过滤 */}
          <SearchFilter
            className="min-w-[220px]"
            customSearchHandler={searchHandler}
            onResultsChange={handleSearchResults}
            placeholder={t('customers.searchPlaceholder')}
            multipleSelection
            searchTrigger={searchTrigger}
          />
        </div>
      </div>

      {/* IB 链面包屑 */}
      {ibChain.length > 0 && (
        <div className="flex flex-wrap items-center gap-2 border-b border-border pb-3">
          {ibChain.map((acc, idx) => (
            <div key={idx} className="flex items-center gap-1">
              <div className="relative">
                <button
                  type="button"
                  onClick={() => handleGoToLevel(idx)}
                  className="text-sm text-text-primary hover:text-primary"
                >
                  {getUserName(acc)}
                </button>
                <span className="absolute -right-4 -top-2.5 rounded-full bg-yellow-400 px-1 text-[10px] font-medium text-gray-900">
                  Lv{idx + 1}
                </span>
              </div>
              <span className="ml-3 text-lg font-bold text-text-secondary">/</span>
            </div>
          ))}

          <Button
            variant="primary"
            size="xs"
            onClick={handleClearIbSearch}
            className="ml-2"
          >
            {t('action.clear')}
          </Button>
        </div>
      )}

      {/* 数据表格 */}
      <DataTable<RepClientAccount>
        columns={columns}
        data={customers}
        rowKey={(item) => item.uid}
        loading={isLoading}
        stretchHeight={false}
      />

      {/* 分页 */}
      <Pagination
        page={criteria.page ?? 1}
        total={criteria.total || 0}
        size={criteria.size ?? 10}
        onPageChange={(p) => fetchData(p)}
      />

      {/* 解锁邮箱弹窗 */}
      {repAccount && (
        <UnlockEmailAddressModal
          key={unlockKey}
          open={unlockEmailOpen}
          onOpenChange={setUnlockEmailOpen}
          uid={unlockEmailUid}
          email={unlockEmailAddress}
          sendCodeAction={getRepViewEmailCode}
          verifyCodeAction={getRepEmailByCode}
          ownerUid={repAccount.uid}
        />
      )}
    </div>
  );
}
