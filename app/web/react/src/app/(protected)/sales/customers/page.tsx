'use client';

import { useState, useEffect, useCallback, useMemo } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesClients, getSalesAccountDetail } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { AccountRoleTypes } from '@/types/accounts';
import {
  Avatar,
  BalanceShow,
  Button,
  Input,
  Skeleton,
  Tag,
  Tabs,
  DataTable,
  DropdownMenu,
  Pagination,
  SimpleSelect,
  Icon,
} from '@/components/ui';
import type { TabItem, DataTableColumn, DropdownMenuItem } from '@/components/ui';
import type { SalesClientAccount, SalesClientCriteria } from '@/types/sales';
import { useUserStore } from '@/stores';
import { ViewRebateStatModal } from '../_components/modals/ViewRebateStatModal';
import { OpenTradeAccountModal } from '../_components/modals/OpenTradeAccountModal';
import { AccountRebateRelationModal } from '../_components/modals/AccountRebateRelationModal';
import { IbLinksModal } from '../_components/modals/IbLinksModal';
import { RebateRuleEditModal } from '../_components/modals/RebateRuleEditModal';
import { NewIbReferralLinkModal } from '../_components/modals/NewIbReferralLinkModal';

type RoleTab = 'ib' | 'client' | 'sales';

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
  return AccountRoleTypes.Sales;
}

const INITIAL_CRITERIA: SalesClientCriteria = {
  page: 1,
  size: 15,
  role: AccountRoleTypes.IB,
  sortField: 'createdOn',
  sortFlag: true,
  searchText: undefined,
  relativeLevel: 1,
  multiLevel: false,
};

export default function SalesCustomersPage() {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const siteConfig = useUserStore((s) => s.siteConfig);

  const [customers, setCustomers] = useState<SalesClientAccount[]>([]);
  const [criteria, setCriteria] = useState<SalesClientCriteria>(INITIAL_CRITERIA);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<RoleTab>('ib');
  const [searchText, setSearchText] = useState('');
  const [ibChain, setIbChain] = useState<SalesClientAccount[]>([]);

  // 弹窗状态
  const [rebateStatOpen, setRebateStatOpen] = useState(false);
  const [openAccountOpen, setOpenAccountOpen] = useState(false);
  const [rebateRelationOpen, setRebateRelationOpen] = useState(false);
  const [ibLinksOpen, setIbLinksOpen] = useState(false);
  const [editSchemaOpen, setEditSchemaOpen] = useState(false);
  const [newReferralOpen, setNewReferralOpen] = useState(false);
  const [selectedAccount, setSelectedAccount] = useState<SalesClientAccount | null>(null);
  const [editSchemaContext, setEditSchemaContext] = useState<{
    parentRole: number;
    parentUid: number;
    editUid: number;
  } | null>(null);

  const tabs: TabItem<RoleTab>[] = useMemo(() => [
    { key: 'ib', label: t('customers.ibType') },
    { key: 'client', label: t('customers.clientType') },
    { key: 'sales', label: t('customers.salesType') },
  ], [t]);

  const fetchData = useCallback(
    async (params: SalesClientCriteria) => {
      if (!salesAccount) return;
      setIsLoading(true);
      try {
        const result = await execute(getSalesClients, salesAccount.uid, params);
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
            multiLevel: raw.multiLevel ?? criteria.multiLevel ?? false,
          });
          setCustomers(Array.isArray(result.data.data) ? result.data.data : []);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [salesAccount, execute]
  );

  useEffect(() => {
    if (salesAccount) {
      fetchData({
        ...INITIAL_CRITERIA,
        role: getRoleValue(activeTab),
        sortFlag: true,
        multiLevel: criteria.multiLevel ?? false,
      });
    } else {
      setIsLoading(false);
    }
  }, [salesAccount, activeTab, fetchData]);

  const handleTabChange = (tab: RoleTab) => {
    setActiveTab(tab);
    setSearchText('');
    setIbChain([]);
  };

  const handleSearch = () => {
    fetchData({
      ...criteria,
      page: 1,
      searchText: searchText || undefined,
      role: getRoleValue(activeTab),
      sortFlag: true,
      relativeLevel: ibChain.length > 0 ? ibChain.length + 1 : 1,
      multiLevel: criteria.multiLevel ?? false,
    });
  };

  const handleReset = () => {
    setSearchText('');
    setIbChain([]);
    setActiveTab('ib');
    fetchData(INITIAL_CRITERIA);
  };

  const handleIbDrillDown = useCallback((ibAccount: SalesClientAccount) => {
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
    fetchData({ ...INITIAL_CRITERIA, role: getRoleValue(activeTab), sortFlag: true, multiLevel: criteria.multiLevel ?? false });
  };

  const handleGoToLevel = (idx: number) => {
    if (idx === ibChain.length - 1) return;
    const newChain = ibChain.slice(0, idx + 1);
    setIbChain(newChain);
    fetchData({
      ...criteria,
      page: 1,
      childParentAccountUid: newChain[idx].uid,
      relativeLevel: idx + 2,
      searchText: undefined,
    });
  };

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

  const showRoleColumn = activeTab === 'ib' || activeTab === 'sales';

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
              <span className="text-xs text-text-secondary">{item.user?.email}</span>
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
          <span className="text-sm">
            {new Date(item.createdOn).toLocaleString('sv-SE').replace('T', ' ')}
          </span>
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
              label: <Link href={`/sales/customers/${item.uid}`} className="block w-full">{t('action.viewDetails')}</Link>,
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
              hidden: !siteConfig?.rebateEnabled || ibChain.length > 0,
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
  }, [showRoleColumn, t, tAccount, handleIbDrillDown, showRebateStat, showOpenAccount, showRebateRelation, showIbLinks, showEditSchema, showNewReferral, siteConfig, ibChain.length]);
  const multiLevelOptions = [
    { value: 'false', label: t('fields.directLevel') },
    { value: 'true', label: t('fields.allLevels') },
  ];
  const multiLevel = criteria.multiLevel ?? false;

  return (
    <div className="flex w-full min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      {/* Tabs + 筛选区 同一行，移动端自动换行 */}
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-3">
        <Tabs
          tabs={tabs}
          activeKey={activeTab}
          onChange={handleTabChange}
          size="xl"
          showDivider={false}
        />
        <div className="ml-auto flex flex-wrap items-center gap-2">
          <SimpleSelect
            value={String(multiLevel)}
            onChange={(val) => {
              const next = val === 'true';
              const nextCriteria = { ...criteria, multiLevel: next, page: 1 };
              setCriteria(nextCriteria);
              fetchData(nextCriteria);
            }}
            options={multiLevelOptions}
            triggerSize="sm"
            className="w-auto! min-w-28 shrink-0 bg-input-bg"
          />
          <div className="relative">
            <Icon name="search-line" className="pointer-events-none absolute left-3 top-1/2 z-10 -translate-y-1/2 text-text-secondary" />
            <Input
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              placeholder={t('customers.searchPlaceholder')}
              inputSize="sm"
              className="w-[200px] pl-9!"
            />
          </div>
          <Button variant="secondary" size="sm" onClick={handleReset} disabled={isLoading} className="bg-[#000f32] text-white hover:bg-[#000f32]/90">
            <Icon name="reset-line" />
            {t('action.reset')}
          </Button>
          <Button variant="primary" size="sm" onClick={handleSearch} disabled={isLoading}>
            <Icon name="search-line" />
            {t('action.search')}
          </Button>
        </div>
      </div>

      {/* IB Chain breadcrumb */}
      {ibChain.length > 0 && (
        <div className="flex items-center gap-2 border-b border-border pb-3">
          {ibChain.map((acc, idx) => (
            <div key={idx} className="flex items-center gap-1">
              <button
                type="button"
                onClick={() => handleGoToLevel(idx)}
                className="text-sm text-primary hover:underline"
              >
                {getUserName(acc)}
              </button>
              <span className="rounded-full bg-yellow-400 px-1.5 text-xs font-medium text-gray-900">
                Lv{idx + 2}
              </span>
              <span className="text-text-secondary">/</span>
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

      {/* Table */}
      <DataTable<SalesClientAccount>
        columns={columns}
        data={customers}
        rowKey={(item) => item.uid}
        loading={isLoading}
      />

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
      <NewIbReferralLinkModal
        open={newReferralOpen}
        onOpenChange={setNewReferralOpen}
        account={selectedAccount}
      />
    </div>
  );
}
