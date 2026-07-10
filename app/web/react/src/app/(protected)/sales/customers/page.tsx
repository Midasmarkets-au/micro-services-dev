'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import Link from 'next/link';
import { useSearchParams, useRouter, usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesAccountDetail, getSalesViewEmailCode, getSalesEmailByCode } from '@/actions';
import { fetchAction } from '@/lib/api/browser-client';
import { useSalesStore } from '@/stores/salesStore';
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
import type { SalesClientAccount, SalesClientCriteria, IbLevelItem } from '@/types/sales';
import { CustomerFilter } from '@/components/CustomerFilter';
import type { CustomerFilterRef, CustomerFilterParams } from '@/components/CustomerFilter';
import { useUserStore } from '@/stores';
import { useTheme } from '@/hooks/useTheme';
import { ViewRebateStatModal } from '../_components/modals/ViewRebateStatModal';
import { OpenTradeAccountModal } from '../_components/modals/OpenTradeAccountModal';
import { AccountRebateRelationModal } from '../_components/modals/AccountRebateRelationModal';
import { IbLinksModal } from '../_components/modals/IbLinksModal';
import { RebateRuleEditModal } from '../_components/modals/RebateRuleEditModal';
import { AddSalesLinkDialog } from '../_components/modals/AddSalesLinkDialog';
import { AddIbLinkBySalesDialog } from '../_components/modals/AddIbLinkBySalesDialog';
import { UnlockEmailAddressModal } from '@/components/user/UnlockEmailAddressModal';
import { TimeShow } from '@/components/TimeShow';
type RoleTab = 'all' | 'ib' | 'client' | 'sales';

function getUserName(item: SalesClientAccount): string {
  const u = item.user;
  if (u?.nativeName && u.nativeName.trim()) return u.nativeName;
  if (u?.displayName && u.displayName.trim()) return u.displayName;
  if (u?.firstName && u?.lastName && u.firstName.trim() && u.lastName.trim()) {
    return `${u.firstName} ${u.lastName}`;
  }
  return 'No Name';
}

function getRoleValue(tab: RoleTab): number {
  if (tab === 'ib') return AccountRoleTypes.IB;
  if (tab === 'client') return AccountRoleTypes.Client;
  if (tab === 'all') return 0;
  return AccountRoleTypes.Sales;
}

const LIST_BASE: Omit<SalesClientCriteria, 'role'> = {
  page: 1,
  size: 30,
  sortField: 'createdOn',
  sortFlag: true,
  multiLevel: false,
};

const INITIAL_CRITERIA: SalesClientCriteria = {
  ...LIST_BASE,
  role: AccountRoleTypes.IB,
  relativeLevel: 1,
};

function parseRoleTab(value: string | null): RoleTab {
  if (value === 'all' || value === 'ib' || value === 'client' || value === 'sales') return value;
  return 'ib';
}

function buildListQuery(parentAccountUid?: number, tab: RoleTab = 'ib'): string {
  const params = new URLSearchParams();
  if (parentAccountUid) params.set('parent', String(parentAccountUid));
  if (tab !== 'ib') params.set('tab', tab);
  const qs = params.toString();
  return qs ? `?${qs}` : '';
}

function toIbLevelItem(account: SalesClientAccount, relativeLevel: number): IbLevelItem {
  return {
    uid: account.uid,
    nativeName: getUserName(account),
    relativeLevel,
  };
}

function buildQueryKey(salesUid: number, parentAccountUid?: number, tab: RoleTab = 'ib'): string {
  const params = new URLSearchParams();
  if (parentAccountUid) params.set('parent', String(parentAccountUid));
  if (tab !== 'ib') params.set('tab', tab);
  return `${salesUid}:${params.toString()}`;
}


export default function SalesCustomersPage() {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const siteConfig = useUserStore((s) => s.siteConfig);
  const { isDark } = useTheme();
  const settingIcon = isDark ? 'setting-night' : 'setting-day';
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();
  const lastFetchedQueryRef = useRef<string | null>(null);
  const isUserNavigationRef = useRef(false);
  const fetchRequestIdRef = useRef(0);
  const ibChainRef = useRef<IbLevelItem[]>([]);

  const [customers, setCustomers] = useState<SalesClientAccount[]>([]);
  const [criteria, setCriteria] = useState<SalesClientCriteria>(INITIAL_CRITERIA);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<RoleTab>('ib');
  const [ibChain, setIbChain] = useState<IbLevelItem[]>([]);
  ibChainRef.current = ibChain;
  const filterRef = useRef<CustomerFilterRef>(null);

  // 弹窗状态
  const [rebateStatOpen, setRebateStatOpen] = useState(false);
  const [openAccountOpen, setOpenAccountOpen] = useState(false);
  const [rebateRelationOpen, setRebateRelationOpen] = useState(false);
  const [ibLinksOpen, setIbLinksOpen] = useState(false);
  const [editSchemaOpen, setEditSchemaOpen] = useState(false);
  const [newReferralOpen, setNewReferralOpen] = useState(false);
  const [unlockEmailOpen, setUnlockEmailOpen] = useState(false);
  const [unlockEmailUid, setUnlockEmailUid] = useState<number | null>(null);
  const [unlockEmailAddress, setUnlockEmailAddress] = useState<string | undefined>(undefined);
  const [unlockKey, setUnlockKey] = useState(0);
  const [selectedAccount, setSelectedAccount] = useState<SalesClientAccount | null>(null);
  const [editSchemaContext, setEditSchemaContext] = useState<{
    parentRole: number;
    parentUid: number;
    editUid: number;
  } | null>(null);

  const tabs: TabItem<RoleTab>[] = useMemo(() => [
    { key: 'all', label: t('customers.all') },
    { key: 'ib', label: t('customers.ibType') },
    { key: 'client', label: t('customers.clientType') },
    { key: 'sales', label: t('customers.salesType') },
  ], [t]);

  const fetchData = useCallback(
    async (params: SalesClientCriteria, options?: { skipChainUpdate?: boolean }) => {
      if (!salesAccount) return;
      const requestId = ++fetchRequestIdRef.current;
      setIsLoading(true);
      try {
        const result = await execute(async () => fetchAction<{ data: SalesClientAccount[]; criteria?: SalesClientCriteria }>('getSalesClients', salesAccount.uid, params));
        if (requestId !== fetchRequestIdRef.current) return;
        if (result.success && result.data) {
          const raw = result.data.criteria || params;
          setCriteria({
            page: raw.page ?? 1,
            size: raw.size ?? 30,
            total: raw.total,
            role: raw.role || undefined,
            sortField: raw.sortField,
            sortFlag: raw.sortFlag,
            searchText: raw.searchText,
            relativeLevel: raw.relativeLevel,
            childParentAccountUid: raw.childParentAccountUid,
            parentAccountUid: raw.parentAccountUid,
            multiLevel: raw.multiLevel ?? params.multiLevel ?? false,
          });
          setCustomers(Array.isArray(result.data.data) ? result.data.data : []);
          if (!options?.skipChainUpdate) {
            const chain = raw.levelAccountsInBetween ?? [];
            ibChainRef.current = chain;
            setIbChain(chain);
          }
        }
      } finally {
        if (requestId === fetchRequestIdRef.current) {
          setIsLoading(false);
        }
      }
    },
    [salesAccount, execute]
  );

  const buildFetchParams = useCallback((tab: RoleTab, keepParentUid?: number) => {
    const filterValues = filterRef.current?.getValues();
    const isClient = tab === 'client';
    return {
      ...LIST_BASE,
      role: getRoleValue(tab) || undefined,
      sortFlag: true,
      searchText: filterValues?.searchText || undefined,
      multiLevel: filterValues?.multiLevel ?? false,
      from: isClient ? filterValues?.dateRange?.from?.toISOString() : undefined,
      to: isClient ? filterValues?.dateRange?.to?.toISOString() : undefined,
      isActive: isClient ? filterValues?.isActive : undefined,
      parentAccountUid: keepParentUid,
    };
  }, []);

  const commitNavigation = useCallback((parentAccountUid?: number, tab?: RoleTab) => {
    if (!salesAccount) return;
    const tabValue = tab ?? activeTab;
    router.replace(`${pathname}${buildListQuery(parentAccountUid, tabValue)}`, { scroll: false });
  }, [router, pathname, activeTab, salesAccount]);

  const navigateWithParent = useCallback((
    parentAccountUid: number | undefined,
    tab: RoleTab,
    params: SalesClientCriteria,
    nextChain: IbLevelItem[],
    skipChainUpdate: boolean,
  ) => {
    if (!salesAccount) return;
    ibChainRef.current = nextChain;
    setIbChain(nextChain);
    isUserNavigationRef.current = true;
    lastFetchedQueryRef.current = buildQueryKey(salesAccount.uid, parentAccountUid, tab);
    commitNavigation(parentAccountUid, tab);
    fetchData(params, { skipChainUpdate });
  }, [salesAccount, commitNavigation, fetchData]);

  const buildDetailHref = useCallback((uid: number) => {
    const parentUid = ibChainRef.current.at(-1)?.uid;
    return `/sales/customers/${uid}${buildListQuery(parentUid, activeTab)}`;
  }, [activeTab]);

  // 从 URL 恢复下钻层级（浏览器前进/后退、从详情页返回）
  useEffect(() => {
    if (!salesAccount) return;

    if (isUserNavigationRef.current) {
      isUserNavigationRef.current = false;
      return;
    }

    const parentParam = searchParams.get('parent');
    const parentUid = parentParam ? Number(parentParam) : undefined;
    const tab = parseRoleTab(searchParams.get('tab'));
    const queryKey = buildQueryKey(salesAccount.uid, parentUid, tab);

    if (lastFetchedQueryRef.current === queryKey) return;
    lastFetchedQueryRef.current = queryKey;

    setActiveTab(tab);
    fetchData(buildFetchParams(tab, parentUid));
  }, [salesAccount, searchParams, fetchData, buildFetchParams]);

  const handleTabChange = (tab: RoleTab) => {
    setActiveTab(tab);
    const parentUid = ibChainRef.current.at(-1)?.uid;
    isUserNavigationRef.current = true;
    if (salesAccount) {
      lastFetchedQueryRef.current = buildQueryKey(salesAccount.uid, parentUid, tab);
    }
    commitNavigation(parentUid, tab);
    fetchData(buildFetchParams(tab, parentUid), { skipChainUpdate: true });
  };

  const handleFilterSearch = useCallback(
    (params: CustomerFilterParams) => {
      const isClient = activeTab === 'client';
      fetchData({
        ...criteria,
        page: 1,
        searchText: params.searchText,
        role: getRoleValue(activeTab) || undefined,
        sortFlag: true,
        multiLevel: params.multiLevel ?? false,
        from: isClient ? params.from : undefined,
        to: isClient ? params.to : undefined,
        isActive: isClient ? params.isActive : undefined,
      });
    },
    [criteria, activeTab, fetchData],
  );

  const handleFilterReset = useCallback(() => {
    setActiveTab('ib');
    fetchData({
      ...INITIAL_CRITERIA,
      parentAccountUid: criteria.parentAccountUid,
    });
  }, [fetchData, criteria.parentAccountUid]);

  const handleIbDrillDown = useCallback((ibAccount: SalesClientAccount) => {
    const nextChain = [
      ...ibChainRef.current,
      toIbLevelItem(ibAccount, ibChainRef.current.length + 1),
    ];
    navigateWithParent(
      ibAccount.uid,
      activeTab,
      {
        ...buildFetchParams(activeTab, ibAccount.uid),
        page: 1,
        searchText: undefined,
      },
      nextChain,
      true,
    );
  }, [activeTab, buildFetchParams, navigateWithParent]);

  const handleClearChain = useCallback(() => {
    navigateWithParent(
      undefined,
      activeTab,
      buildFetchParams(activeTab),
      [],
      false,
    );
  }, [activeTab, buildFetchParams, navigateWithParent]);

  const handleGoToLevel = useCallback((acc: IbLevelItem, idx: number) => {
    if (idx === ibChainRef.current.length - 1) return;
    const nextChain = ibChainRef.current.slice(0, idx + 1);
    navigateWithParent(
      acc.uid,
      activeTab,
      {
        ...buildFetchParams(activeTab, acc.uid),
        page: 1,
        searchText: undefined,
      },
      nextChain,
      true,
    );
  }, [activeTab, buildFetchParams, navigateWithParent]);

  const showRebateStat = useCallback((item: SalesClientAccount) => {
    setSelectedAccount(item);
    setRebateStatOpen(true);
  }, []);

  const showOpenAccount = useCallback((item: SalesClientAccount) => {
    setSelectedAccount(item);
    setOpenAccountOpen(true);
  }, []);

  const showRebateRelation = useCallback((item: SalesClientAccount) => {
    setSelectedAccount(item);
    setRebateRelationOpen(true);
  }, []);

  const showIbLinks = useCallback((item: SalesClientAccount) => {
    setSelectedAccount(item);
    setIbLinksOpen(true);
  }, []);

  const showEditSchema = useCallback(async (item: SalesClientAccount) => {
    if (!salesAccount) return;
    const result = await execute(getSalesAccountDetail, salesAccount.uid, item.uid);
    if (result.success && result.data) {
      const detail = result.data as { agentAccountUid?: number; salesAccountUid?: number; uid: number };
      const parentRole = detail.agentAccountUid === 0 ? AccountRoleTypes.Sales : AccountRoleTypes.IB;
      const parentUid = detail.agentAccountUid === 0 ? (detail.salesAccountUid ?? salesAccount.uid) : (detail.agentAccountUid ?? 0);
      setEditSchemaContext({ parentRole, parentUid, editUid: detail.uid });
      setSelectedAccount(item);
      setEditSchemaOpen(true);
    }
  }, [salesAccount, execute]);

  const showNewReferral = useCallback((item: SalesClientAccount) => {
    setSelectedAccount(item);
    setNewReferralOpen(true);
  }, []);

  const showUnlockEmailAddress = useCallback((uid: number, email?: string) => {
    setUnlockEmailUid(uid);
    setUnlockEmailAddress(email);
    setUnlockKey((k) => k + 1);
    setUnlockEmailOpen(true);
  }, []);

  const showRoleColumn = activeTab === 'all';

  const columns = useMemo<DataTableColumn<SalesClientAccount>[]>(() => {
    const cols: DataTableColumn<SalesClientAccount>[] = [
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
        render: (item) => {
          let variant: 'success' | 'danger' | 'warning' = 'success';
          let label = t('customers.clientType');
          if (item.role === AccountRoleTypes.IB) {
            variant = 'success';
            label = t('customers.ibType');
          } else if (item.role === AccountRoleTypes.Sales) {
            variant = 'danger';
            label = t('customers.salesType');
          } else {
            variant = 'warning';
            label = t('customers.clientType');
          }
          return <Tag variant={variant} soft>{label}</Tag>;
        },
      });
    }

    cols.push(
      {
        key: 'accountUid',
        title: <>{t('fields.accountUid')}/{t('fields.accountNo')}</>,
        skeletonWidth: 'w-20',
        render: (item) => (
          <span className="text-sm">
            {item.role === AccountRoleTypes.Client
              ? item.tradeAccount?.accountNumber ?? t('customers.noTradeAccount')
              : item.uid}
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
          <TimeShow dateIsoString={item.createdOn} type="inFields" />
        ),
      },
      {
        key: 'actions',
        title: t('fields.actions'),
        skeletonWidth: 'w-10',
        render: (item) => {
          const dropdownItems: DropdownMenuItem[] = [];
          if (item.role === AccountRoleTypes.IB || item.role === AccountRoleTypes.Sales) {
            dropdownItems.push({
              key: 'viewAccounts',
              label: t('action.viewAccounts'),
              onClick: () => handleIbDrillDown(item),
            });
          }

          if (item.role === AccountRoleTypes.Client) {
            dropdownItems.push({
              key: 'viewDetails',
              label: <Link href={buildDetailHref(item.uid)} className="block w-full">{t('action.viewDetails')}</Link>,
              onClick: () => {},
            });
          }

          dropdownItems.push({
            key: 'viewRebateStat',
            label: t('action.viewRebateStatistics'),
            onClick: () => showRebateStat(item),
          });

          dropdownItems.push({
            key: 'createTradeAccount',
            label: t('action.createTradeAccount'),
            onClick: () => showOpenAccount(item),
          });

          if (item.role === AccountRoleTypes.IB) {
            dropdownItems.push({
              key: 'viewRebateRelation',
              label: t('action.viewRebateRelation'),
              onClick: () => showRebateRelation(item),
            });

            dropdownItems.push({
              key: 'referralCodeList',
              label: t('action.referralCodeList'),
              onClick: () => showIbLinks(item),
            });

            dropdownItems.push({
              key: 'editSchema',
              label: t('action.editSchema'),
              onClick: () => showEditSchema(item),
              hidden: !siteConfig?.rebateEnabled ,
            });

            dropdownItems.push({
              key: 'newIBReferralCode',
              label: t('action.newIBReferralCode'),
              onClick: () => showNewReferral(item),
              hidden: !siteConfig?.rebateEnabled,
            });
          }
          if (item.role === AccountRoleTypes.Sales) {
            dropdownItems.push({
              key: 'referralCodeList',
              label: t('action.referralCodeList'),
              onClick: () => showIbLinks(item),
            });
            dropdownItems.push({
              key: 'newIBReferralCode',
              label: t('action.newIBReferralCode'),
              onClick: () => showNewReferral(item),
              hidden: !siteConfig?.rebateEnabled || ibChain.length > 0,
            });
          }

          return (
            <DropdownMenu
              trigger={
                <Button variant="outline" size="xs" className="gap-1">
                  {t('action.action')}
                  <Icon name="chevron-down-sm" size={12} />
                </Button>
              }
              items={dropdownItems}
            />
          );
        },
      },
    );

    return cols;
  }, [showRoleColumn, t, tAccount, buildDetailHref, handleIbDrillDown, showRebateStat, showOpenAccount, showRebateRelation, showIbLinks, showEditSchema, showNewReferral, showUnlockEmailAddress, siteConfig, ibChain.length]);
  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      {/* Tabs + 筛选区同一行 */}
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
            showMultiLevel
            showActiveFilter={activeTab === 'client'}
            showDatePicker={activeTab === 'client'}
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
            <div key={acc.uid} className="flex items-center gap-1">
              <div className="relative inline-block pr-3">
                <button
                  type="button"
                  onClick={() => handleGoToLevel(acc, idx)}
                  className="text-sm text-primary hover:underline cursor-pointer"
                >
                  {acc.nativeName}
                </button>
                <span className="pointer-events-none absolute -top-2 -right-1 rounded-full bg-yellow-400 px-1 py-0 text-[10px] font-medium leading-tight text-gray-900">
                  Lv{acc.relativeLevel}
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
            const isClient = item.role === AccountRoleTypes.Client;
            const isNonClient = !isClient;
            const accountId = isClient
              ? (item.tradeAccount?.accountNumber ?? t('customers.noTradeAccount'))
              : String(item.uid);
            const subInfo = item.code || item.group || '-';

            // 角色 Tag 样式
            let roleVariant: 'success' | 'danger' | 'warning' = 'warning';
            let roleLabel = t('customers.clientType');
            if (item.role === AccountRoleTypes.IB) { roleVariant = 'success'; roleLabel = t('customers.ibType'); }
            else if (item.role === AccountRoleTypes.Sales) { roleVariant = 'danger'; roleLabel = t('customers.salesType'); }

            // 非 Client 的 dropdown
            const mobileDropdownItems: DropdownMenuItem[] = [];
            if (isNonClient) {
              mobileDropdownItems.push({ key: 'viewAccounts', label: t('action.viewAccounts'), onClick: () => handleIbDrillDown(item) });
            }
            mobileDropdownItems.push(
              { key: 'viewRebateStat', label: t('action.viewRebateStatistics'), onClick: () => showRebateStat(item) },
              { key: 'createTradeAccount', label: t('action.createTradeAccount'), onClick: () => showOpenAccount(item) },
            );
            if (item.role === AccountRoleTypes.IB) {
              mobileDropdownItems.push(
                { key: 'viewRebateRelation', label: t('action.viewRebateRelation'), onClick: () => showRebateRelation(item) },
                { key: 'referralCodeList', label: t('action.referralCodeList'), onClick: () => showIbLinks(item) },
                { key: 'editSchema', label: t('action.editSchema'), onClick: () => showEditSchema(item), hidden: !siteConfig?.rebateEnabled },
                { key: 'newIBReferralCode', label: t('action.newIBReferralCode'), onClick: () => showNewReferral(item), hidden: !siteConfig?.rebateEnabled },
              );
            }
            if (item.role === AccountRoleTypes.Sales) {
              mobileDropdownItems.push(
                { key: 'referralCodeList', label: t('action.referralCodeList'), onClick: () => showIbLinks(item) },
                { key: 'newIBReferralCode', label: t('action.newIBReferralCode'), onClick: () => showNewReferral(item), hidden: !siteConfig?.rebateEnabled || ibChain.length > 0 },
              );
            }

            return (
              <div
                key={item.uid}
                className="flex items-center gap-3 rounded-lg border border-border bg-surface p-4 transition-colors hover:bg-(--color-surface-secondary)"
              >
                <Avatar src={item.user?.avatar} alt={name} size="sm" className="shrink-0" />

                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-1.5">
                    <p className="truncate text-sm font-medium text-text-primary">{name}</p>
                    {showRoleColumn && (
                      <Tag variant={roleVariant} soft className="shrink-0 text-[10px] px-1 py-0">
                        {roleLabel}
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

                <div className="flex shrink-0 items-center gap-2">
                  <div className="flex flex-col items-end">
                    <span className="text-sm font-medium text-text-primary">{accountId}</span>
                    <span className="text-xs text-text-secondary">{subInfo}</span>
                  </div>

                  {isClient ? (
                    <Link
                      href={buildDetailHref(item.uid)}
                      className="flex items-center justify-center text-text-secondary hover:text-text-primary transition-colors"
                      aria-label={t('action.viewDetails')}
                    >
                      <Icon name="eye_open" size={18} />
                    </Link>
                  ) : (
                    <DropdownMenu
                      trigger={
                        <button
                          type="button"
                          className="flex items-center justify-center text-text-secondary hover:text-text-primary transition-colors"
                          aria-label={t('action.action')}
                        >
                          <Icon name={settingIcon} size={18} />
                        </button>
                      }
                      items={mobileDropdownItems}
                    />
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Desktop table — hidden on mobile */}
      <div className="hidden md:block">
        <DataTable<SalesClientAccount>
          columns={columns}
          data={customers}
          rowKey={(item) => item.uid}
          loading={isLoading}
          stretchHeight={false}
        />
      </div>

      {/* Pagination */}
      <Pagination
        page={criteria.page ?? 1}
        total={criteria.total || 0}
        size={criteria.size ?? 30}
        onPageChange={(p) => fetchData({ ...criteria, page: p })}
      />

      {/* Modals */}
      <ViewRebateStatModal
        open={rebateStatOpen}
        onOpenChange={setRebateStatOpen}
        account={selectedAccount}
      />
      <OpenTradeAccountModal
        open={openAccountOpen}
        onOpenChange={setOpenAccountOpen}
        account={selectedAccount}
        onSuccess={() => fetchData(criteria)}
      />
      <AccountRebateRelationModal
        open={rebateRelationOpen}
        onOpenChange={setRebateRelationOpen}
        account={selectedAccount}
      />
      <IbLinksModal
        open={ibLinksOpen}
        onOpenChange={setIbLinksOpen}
        account={selectedAccount}
      />
      <RebateRuleEditModal
        open={editSchemaOpen}
        onOpenChange={setEditSchemaOpen}
        account={selectedAccount}
        context={editSchemaContext}
        onSuccess={() => fetchData(criteria)}
      />
      {selectedAccount?.role === AccountRoleTypes.IB ? (
        <AddIbLinkBySalesDialog
          isOpen={newReferralOpen}
          onClose={() => setNewReferralOpen(false)}
          onSuccess={() => fetchData(criteria)}
          ibUid={selectedAccount.uid}
          userName={getUserName(selectedAccount)}
        />
      ) : (
        <AddSalesLinkDialog
          isOpen={newReferralOpen}
          onClose={() => setNewReferralOpen(false)}
          onSuccess={() => fetchData(criteria)}
          salesUid={selectedAccount?.uid}
          userName={selectedAccount ? getUserName(selectedAccount) : undefined}
        />
      )}
      {salesAccount && (
        <UnlockEmailAddressModal
          key={unlockKey}
          open={unlockEmailOpen}
          onOpenChange={setUnlockEmailOpen}
          uid={unlockEmailUid}
          email={unlockEmailAddress}
          sendCodeAction={getSalesViewEmailCode}
          verifyCodeAction={getSalesEmailByCode}
          ownerUid={salesAccount.uid}
        />
      )}
    </div>
  );
}
