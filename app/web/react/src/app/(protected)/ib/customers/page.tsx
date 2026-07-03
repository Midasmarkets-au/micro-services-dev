'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import moment from 'moment';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBViewEmailCode, getIBEmailByCode } from '@/actions';
import { fetchAction } from '@/lib/api/browser-client';
import { useIBStore } from '@/stores/ibStore';
import { AccountRoleTypes } from '@/types/accounts';
import {
  Avatar,
  BalanceShow,
  Button,
  Skeleton,
  Tag,
  Tabs,
  DataTable,
  DropdownMenu,
  Pagination,
  Icon,
} from '@/components/ui';
import type { TabItem, DataTableColumn, DropdownMenuItem } from '@/components/ui';
import { CustomerFilter } from '@/components/CustomerFilter';
import type { CustomerFilterRef, CustomerFilterParams } from '@/components/CustomerFilter';
import type { IBClientAccount, IBClientCriteria } from '@/types/ib';
import { useUserStore } from '@/stores';
import { useTheme } from '@/hooks/useTheme';
import { ViewRebateStatModal } from '../_components/modals/ViewRebateStatModal';
import { RebateRuleEditModal } from '../_components/modals/RebateRuleEditModal';
import { UnlockEmailAddressModal } from '@/components/user/UnlockEmailAddressModal';
import { TimeShow } from '@/components/TimeShow';
type RoleTab = 'all' | 'ib' | 'client';

function getUserName(item: IBClientAccount): string {
  const u = item.user;
  if (u?.nativeName && u.nativeName.trim()) return u.nativeName;
  if (u?.displayName && u.displayName.trim()) return u.displayName;
  if (u?.firstName && u?.lastName && u.firstName.trim() && u.lastName.trim()) {
    return `${u.firstName} ${u.lastName}`;
  }
  return 'No Name';
}

function getRoleValue(tab: RoleTab): number | undefined {
  if (tab === 'ib') return AccountRoleTypes.IB;
  if (tab === 'client') return AccountRoleTypes.Client;
  return undefined;
}

const INITIAL_CRITERIA: IBClientCriteria = {
  page: 1,
  size: 15,
  role: undefined,
  sortField: 'createdOn',
  sortFlag: true,
  searchText: undefined,
  relativeLevel: 1,
};

export default function IBCustomersPage() {
  const t = useTranslations('ib');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  // execute 引用在 useServerAction 内部 state 变化时会刷新，
  // 用 ref 固定下来，避免 fetchData/useEffect 因依赖变化触发死循环
  const executeRef = useRef(execute);
  useEffect(() => { executeRef.current = execute; });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const siteConfig = useUserStore((s) => s.siteConfig);
  const { isDark } = useTheme();
  const settingIcon = isDark ? 'setting-night' : 'setting-day';

  const [customers, setCustomers] = useState<IBClientAccount[]>([]);
  const [criteria, setCriteria] = useState<IBClientCriteria>(INITIAL_CRITERIA);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<RoleTab>('all');
  const [ibChain, setIbChain] = useState<IBClientAccount[]>([]);
  const filterRef = useRef<CustomerFilterRef>(null);
  const [rebateStatOpen, setRebateStatOpen] = useState(false);
  const [editSchemaOpen, setEditSchemaOpen] = useState(false);
  const [selectedAccount, setSelectedAccount] = useState<IBClientAccount | null>(null);
  const [unlockEmailOpen, setUnlockEmailOpen] = useState(false);
  const [unlockEmailUid, setUnlockEmailUid] = useState<number | null>(null);
  const [unlockEmailAddress, setUnlockEmailAddress] = useState<string | undefined>(undefined);
  const [unlockKey, setUnlockKey] = useState(0);

  const tabs: TabItem<RoleTab>[] = useMemo(() => [
    { key: 'all', label: t('fields.all') },
    { key: 'ib', label: t('customers.ibType') },
    { key: 'client', label: t('customers.clientType') },
  ], [t]);

  const fetchData = useCallback(
    async (params: IBClientCriteria) => {
      if (!agentAccount) return;
      setIsLoading(true);
      try {
        const result = await executeRef.current(async () => fetchAction<{ data: IBClientAccount[]; criteria?: IBClientCriteria }>('getIBClients', agentAccount.uid, params));
        if (result.success && result.data) {
          const raw = result.data.criteria || params;
          setCriteria({
            page: raw.page ?? 1,
            size: raw.size ?? 15,
            total: raw.total,
            role: raw.role,
            sortField: raw.sortField,
            sortFlag: raw.sortFlag,
            searchText: raw.searchText,
            relativeLevel: raw.relativeLevel,
            childParentAccountUid: raw.childParentAccountUid,
            // 后端可能不回传以下筛选字段，回落到本次请求参数，保证翻页/下钻不丢条件
            isActive: raw.isActive ?? params.isActive,
            from: raw.from ?? params.from,
            to: raw.to ?? params.to,
          });
          setCustomers(Array.isArray(result.data.data) ? result.data.data : []);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [agentAccount]
  );

  // 仅在账户初始化/切换账户时触发；切换账户重置下钻链
  useEffect(() => {
    if (agentAccount) {
      setIbChain([]);
      fetchData({
        ...INITIAL_CRITERIA,
        role: getRoleValue(activeTab),
        sortFlag: true,
      });
    } else {
      setIsLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [agentAccount, fetchData]);

  const handleTabChange = (tab: RoleTab) => {
    setActiveTab(tab);
    // 对齐 Vue：切换角色保留下钻链（ibChain）与搜索条件，在当前下钻层级下按新角色查询
    const f = filterRef.current?.getValues();
    const isClient = tab === 'client';
    const lastIb = ibChain[ibChain.length - 1];
    const hasDateOrActive = isClient && (!!f?.dateRange?.from || !!f?.dateRange?.to || f?.isActive !== undefined);
    const isSearching = !!f?.searchText || hasDateOrActive;
    fetchData({
      ...INITIAL_CRITERIA,
      role: getRoleValue(tab),
      sortFlag: (f?.sortOrder ?? 'newest') !== 'oldest',
      searchText: f?.searchText || undefined,
      childParentAccountUid: lastIb?.uid,
      // 搜索态跨全层级（对齐 handleFilterSearch），否则按下钻层级/直属
      relativeLevel: isSearching ? undefined : (lastIb ? ibChain.length + 1 : 1),
      from: isClient && f?.dateRange?.from ? moment(f.dateRange.from).startOf('day').toISOString() : undefined,
      to: isClient && f?.dateRange?.to ? moment(f.dateRange.to).endOf('day').toISOString() : undefined,
      isActive: isClient ? f?.isActive : undefined,
    });
  };

  const handleFilterSearch = useCallback(
    (params: CustomerFilterParams) => {
      const isClient = activeTab === 'client';
      fetchData({
        ...criteria,
        page: 1,
        searchText: params.searchText,
        role: getRoleValue(activeTab),
        sortFlag: params.sortOrder !== 'oldest',
        // 对齐 Vue：搜索时跨所有层级（relativeLevel 置空），下钻态仍受 childParentAccountUid 约束
        relativeLevel: undefined,
        from: isClient ? params.from : undefined,
        to: isClient ? params.to : undefined,
        isActive: isClient ? params.isActive : undefined,
      });
    },
    [criteria, activeTab, fetchData],
  );

  const handleFilterReset = useCallback(() => {
    setIbChain([]);
    setActiveTab('all');
    fetchData(INITIAL_CRITERIA);
  }, [fetchData]);

  const handleIbDrillDown = useCallback((ibAccount: IBClientAccount) => {
    setIbChain((prev) => [...prev, ibAccount]);
    fetchData({
      ...criteria,
      page: 1,
      childParentAccountUid: ibAccount.uid,
      relativeLevel: ibChain.length + 2,
      searchText: undefined,
    });
  }, [criteria, ibChain.length, fetchData]);

  const handleClearChain = () => {
    setIbChain([]);
    const currentSortOrder = filterRef.current?.getValues().sortOrder ?? 'newest';
    fetchData({ ...INITIAL_CRITERIA, role: getRoleValue(activeTab), sortFlag: currentSortOrder !== 'oldest' });
  };

  const handleGoToLevel = (idx: number, childParentAccountUid: number) => {
    if (idx === ibChain.length - 1) return;
    const newChain = ibChain.slice(0, idx + 1);
    setIbChain(newChain);
    fetchData({
      ...criteria,
      page: 1,
      childParentAccountUid,
      relativeLevel: idx + 2,
      searchText: undefined,
    });
  };

  const showRebateStat = useCallback((item: IBClientAccount) => {
    setSelectedAccount(item);
    setRebateStatOpen(true);
  }, []);

  const showEditSchema = useCallback((item: IBClientAccount) => {
    setSelectedAccount(item);
    setEditSchemaOpen(true);
  }, []);

  const showUnlockEmailAddress = useCallback((uid: number, email?: string) => {
    setUnlockEmailUid(uid);
    setUnlockEmailAddress(email);
    setUnlockKey((k) => k + 1);
    setUnlockEmailOpen(true);
  }, []);

  const showRoleColumn = activeTab === 'all';

  const columns = useMemo<DataTableColumn<IBClientAccount>[]>(() => {
    const cols: DataTableColumn<IBClientAccount>[] = [
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
            <div className="flex flex-col">
              <span className="text-sm font-medium text-text-primary">{getUserName(item)}</span>
              <span className="text-xs cursor-pointer text-text-secondary" onClick={() => showUnlockEmailAddress(item.uid,item.user?.email)}>{item.user?.email}</span>
            </div>
          </div>
        ),
      },
    ];

    if (showRoleColumn) {
      cols.push({
        key: 'role',
        title: t('fields.role'),
        skeletonWidth: 'w-12',
        skeletonHeight: 'h-5',
        render: (item) => (
          <Tag variant={item.role === AccountRoleTypes.Client ? 'success' : 'danger'} soft>
            {item.role === AccountRoleTypes.Client ? t('customers.clientType') : t('customers.ibType')}
          </Tag>
        ),
      });
    }

    cols.push(
      {
        key: 'accountUid',
        title: <>{t('fields.accountUid')}/{t('fields.accountNo')}</>,
        skeletonWidth: 'w-20',
        render: (item) => (
          <span className="text-sm">
            {item.role === AccountRoleTypes.IB
              ? item.uid
              : item.tradeAccount?.accountNumber ?? t('customers.noTradeAccount')}
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
        key: 'type',
        title: t('fields.type'),
        skeletonWidth: 'w-12',
        render: (item) => (
          <span className="text-sm">
            {tAccount.has(`accountTypes.${item.type}`) ? tAccount(`accountTypes.${item.type}`) : (item.type === 0 ? t('fields.default') : String(item.type))}
          </span>
        ),
      },
      {
        key: 'balance',
        title: t('fields.balance'),
        skeletonWidth: 'w-20',
        align: 'right',
        render: (item) =>
          item.role !== AccountRoleTypes.IB && item.tradeAccount ? (
            <BalanceShow
              balance={item.tradeAccount.balanceInCents || 0}
              currencyId={item.tradeAccount.currencyId || 840}
              className="text-sm font-semibold text-text-primary"
            />
          ) : (
            <span className="text-sm">-</span>
          ),
      },
      {
        key: 'createdOn',
        title: t('fields.createdOn'),
        skeletonWidth: 'w-28',
        render: (item) => (
          <TimeShow
          dateIsoString={item.createdOn}
          type="oneLiner"
        />
        ),
      },
      {
        key: 'actions',
        title: t('fields.actions'),
        skeletonWidth: 'w-10',
        render: (item) => {
          if (item.role === AccountRoleTypes.Client) {
            return (
              <Link href={`/ib/customers/${item.uid}`} className="text-sm text-text-primary underline font-medium">
                {t('action.view')}
              </Link>
            );
          }
          if (item.role === AccountRoleTypes.IB) {
            const dropdownItems: DropdownMenuItem[] = [
              {
                key: 'viewAccounts',
                label: t('action.viewAccounts'),
                onClick: () => handleIbDrillDown(item),
              },
              {
                key: 'viewRebateStat',
                label: t('action.viewRebateStatistics'),
                onClick: () => showRebateStat(item),
              },
              {
                key: 'editSchema',
                label: t('action.editSchema'),
                onClick: () => showEditSchema(item),
                hidden: !siteConfig?.rebateEnabled || ibChain.length > 0,
              },
            ];
            return (
              <DropdownMenu
                trigger={
                  <Button variant="outline" size="xs" className="gap-1">
                    {t('fields.action')}
                    <Icon name="chevron-down-sm" size={12} />
                  </Button>
                }
                items={dropdownItems}
              />
            );
          }
          return null;
        },
      },
    );

    return cols;
  }, [showRoleColumn, t, tAccount, handleIbDrillDown, showRebateStat, showEditSchema, showUnlockEmailAddress, siteConfig, ibChain.length]);

  const sortOptions = [
    { value: 'newest', label: t('action.sortNewest') },
    { value: 'oldest', label: t('action.sortOldest') },
  ];

  return (
    <div className="flex w-full min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      {/* Tabs + Filter Bar */}
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-3">
        <Tabs
          tabs={tabs}
          activeKey={activeTab}
          onChange={handleTabChange}
          size="xl"
          showDivider={false}
        />
        <div className="ml-auto">
          <CustomerFilter
            ref={filterRef}
            showSortOrder
            sortOptions={sortOptions}
            showActiveFilter={activeTab === 'client'}
            showDatePicker={activeTab === 'client'}
            defaultValues={{ sortOrder: 'newest' }}
            onSearch={handleFilterSearch}
            onReset={handleFilterReset}
            isLoading={isLoading}
            searchPlaceholder={t('customers.searchPlaceholder')}
          />
        </div>
      </div>

      {/* IB Chain breadcrumb */}
      {ibChain.length > 0 && (
        <div className="flex items-center gap-2 border-b border-border pb-3">
          {ibChain.map((acc, idx) => (
            <div key={idx} className="flex items-center gap-1">
              <div className="relative inline-block pr-3">
                <button
                  type="button"
                  onClick={() => handleGoToLevel(idx, acc.uid)}
                  className="text-sm text-primary hover:underline cursor-pointer"
                >
                  {getUserName(acc)}
                </button>
                <span className="pointer-events-none absolute -top-2 -right-1 rounded-full bg-yellow-400 px-1 py-0 text-[10px] font-medium leading-tight text-gray-900">
                  Lv{idx + 2}
                </span>
              </div>
              {idx < ibChain.length - 1 && (
                <span className="text-text-secondary">/</span>
              )}
            </div>
          ))}
          <button
            type="button"
            onClick={handleClearChain}
            className="ml-2 text-xs text-primary hover:underline"
          >
            {t('action.clear')}
          </button>
        </div>
      )}

      {/* Mobile card list — only on small screens */}
      <div className="flex flex-col gap-3 md:hidden">
        {isLoading ? (
          Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="flex items-center gap-3 rounded-lg border border-border p-4">
              <Skeleton className="size-10 shrink-0 rounded-full" />
              <div className="flex flex-1 flex-col gap-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-3 w-32" />
              </div>
              <div className="flex shrink-0 flex-col items-end gap-2">
                <Skeleton className="h-4 w-16" />
                <Skeleton className="h-3 w-12" />
              </div>
            </div>
          ))
        ) : customers.length === 0 ? (
          <div className="py-12 text-center text-sm text-text-secondary">
            {t('dashboard.noData')}
          </div>
        ) : (
          customers.map((item) => {
            const name = getUserName(item);
            const isIB = item.role === AccountRoleTypes.IB;
            const accountId = isIB
              ? String(item.uid)
              : (item.tradeAccount?.accountNumber ?? t('customers.noTradeAccount'));
            const subInfo = item.code || item.group || '-';

            const ibDropdownItems: DropdownMenuItem[] = [
              {
                key: 'viewAccounts',
                label: t('action.viewAccounts'),
                onClick: () => handleIbDrillDown(item),
              },
              {
                key: 'viewRebateStat',
                label: t('action.viewRebateStatistics'),
                onClick: () => showRebateStat(item),
              },
              {
                key: 'editSchema',
                label: t('action.editSchema'),
                onClick: () => showEditSchema(item),
                hidden: !siteConfig?.rebateEnabled || ibChain.length > 0,
              },
            ];

            return (
              <div
                key={item.uid}
                className="flex items-center gap-3 rounded-lg border border-border bg-surface p-4"
              >
                {/* Avatar */}
                <Avatar
                  src={item.user?.avatar}
                  alt={name}
                  size="sm"
                  className="shrink-0"
                />

                {/* Name + email */}
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-1.5">
                    <p className="truncate text-sm font-medium text-text-primary">{name}</p>
                    {showRoleColumn && (
                      <Tag variant={isIB ? 'danger' : 'success'} soft className="shrink-0 text-[10px] px-1 py-0">
                        {isIB ? t('customers.ibType') : t('customers.clientType')}
                      </Tag>
                    )}
                  </div>
                  <p
                    className="cursor-pointer truncate text-xs text-text-secondary"
                    onClick={() => showUnlockEmailAddress(item.uid, item.user?.email)}
                  >
                    {item.user?.email}
                  </p>
                </div>

                {/* Account info + action */}
                <div className="flex shrink-0 items-center gap-2">
                  <div className="flex flex-col items-end">
                    <span className="text-sm font-medium text-text-primary">{accountId}</span>
                    <span className="text-xs text-text-secondary">{subInfo}</span>
                  </div>

                  {/* Action button */}
                  {isIB ? (
                    <DropdownMenu
                      trigger={
                        <button
                          type="button"
                          className="flex items-center justify-center text-text-secondary hover:text-text-primary transition-colors"
                          aria-label={t('fields.action')}
                        >
                          <Icon name={settingIcon} size={18} />
                        </button>
                      }
                      items={ibDropdownItems}
                    />
                  ) : (
                    <Link
                      href={`/ib/customers/${item.uid}`}
                      className="flex items-center justify-center text-text-secondary hover:text-text-primary transition-colors"
                      aria-label={t('action.view')}
                    >
                      <Icon name="eye_open" size={18} />
                    </Link>
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Desktop table — hidden on mobile */}
      <div className="hidden md:block">
        <DataTable<IBClientAccount>
          columns={columns}
          data={customers}
          rowKey={(item) => item.uid}
          loading={isLoading}
        />
      </div>

      {/* Pagination */}
      <Pagination
        page={criteria.page ?? 1}
        total={criteria.total || 0}
        size={criteria.size ?? 15}
        onPageChange={(p) => fetchData({ ...criteria, page: p })}
      />

      {/* Modals */}
      <ViewRebateStatModal
        open={rebateStatOpen}
        onOpenChange={setRebateStatOpen}
        account={selectedAccount}
      />
      <RebateRuleEditModal
        open={editSchemaOpen}
        onOpenChange={setEditSchemaOpen}
        account={selectedAccount}
        onSuccess={() => fetchData(criteria)}
      />
      {agentAccount && (
        <UnlockEmailAddressModal
          key={unlockKey}
          open={unlockEmailOpen}
          onOpenChange={setUnlockEmailOpen}
          uid={unlockEmailUid}
          email={unlockEmailAddress}
          sendCodeAction={getIBViewEmailCode}
          verifyCodeAction={getIBEmailByCode}
          ownerUid={agentAccount.uid}
        />
      )}
    </div>
  );
}
