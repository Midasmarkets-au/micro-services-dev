'use client';

import { useState, useEffect, useCallback, use, useMemo } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import {
  Avatar,
  BalanceShow,
  Skeleton,
  Tabs,
  Pagination,
  Tag,
  DataTable,
  Icon,
} from '@/components/ui';
import type { TabItem, DataTableColumn, DataTableGroupConfig } from '@/components/ui';
import type { TagVariant } from '@/components/ui';
import {
  getIBClients,
  getIBDeposits,
  getIBWithdrawals,
  getIBAccountTrades,
  getIBAccountTransactions,
} from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import type {
  IBClientAccount,
  IBDepositRecord,
  IBWithdrawalRecord,
  IBRebateRecord,
} from '@/types/ib';
import {
  DepositState,
  WithdrawalState,
  TransferState,
  CurrencyTypes,
  TransactionAccountType,
  getCurrencyCode,
} from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';
import { TradeReportTable } from '@/components/TradeReportTable';
import { TradeFilter } from '@/components/TradeFilter';
import type { TradeFilterType } from '@/components/TradeFilter';

type DetailTab = 'deposit' | 'withdrawal' | 'transfer' | 'tradeReport' | 'commissionReport';

const PENDING_STATES = new Set([
  DepositState.DepositCreated, DepositState.DepositPaymentCompleted, DepositState.DepositTenantApproved,
  WithdrawalState.WithdrawalCreated, WithdrawalState.WithdrawalTenantApproved,
  TransferState.TransferCreated, TransferState.TransferAwaitingApproval, TransferState.TransferApproved,
]);

const COMPLETED_STATES = new Set([
  DepositState.DepositCompleted, DepositState.DepositCallbackComplete,
  WithdrawalState.WithdrawalCompleted, WithdrawalState.WithdrawalPaymentCompleted,
  TransferState.TransferCompleted,
]);

const REJECTED_STATES = new Set([
  DepositState.DepositTenantRejected, DepositState.DepositCanceled, DepositState.DepositFailed,
  WithdrawalState.WithdrawalTenantRejected, WithdrawalState.WithdrawalCanceled, WithdrawalState.WithdrawalFailed,
  TransferState.TransferRejected, TransferState.TransferCanceled, TransferState.TransferFailed,
]);

function getStateTagVariant(stateId: number): TagVariant {
  if (COMPLETED_STATES.has(stateId)) return 'success';
  if (REJECTED_STATES.has(stateId)) return 'danger';
  if (PENDING_STATES.has(stateId)) return 'warning';
  return 'info';
}

function formatDateTime(dateStr: string) {
  if (!dateStr) return '--';
  const d = new Date(dateStr);
  return d.toLocaleString('sv-SE').replace('T', ' ');
}

function formatGroupKey(dateStr: string) {
  const d = new Date(dateStr);
  const monthYear = d.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  const weekday = d.toLocaleDateString('en-US', { weekday: 'long' });
  return `${monthYear}||${weekday}`;
}

interface TransferItem {
  id?: number;
  accountNumber?: number;
  stateId?: number;
  currencyId?: number;
  amount?: number;
  createdOn: string;
  [key: string]: unknown;
  sourceAccount: {
    accountNumber: number;
    group?: string;
    currencyId?: number;
  };
  targetAccount: {
    accountNumber: number;
    group?: string;
    currencyId?: number;
  };
}

const DEFAULT_DEPOSIT_STATE_IDS = [350, 345];
const DEFAULT_WITHDRAWAL_STATE_IDS = [450];
const DEFAULT_TRANSACTION_STATE_IDS = [250];
const DEFAULT_PAGE_SIZE = 10;
const TAB_FIXED_FILTER_PARAMS: Partial<Record<DetailTab, Record<string, unknown>>> = {
  deposit: {
    isClosed: false,
  },
  withdrawal: {
    isClosed: false,
  },
  transfer: {
    isClosed: false,
    targetAccountType: TransactionAccountType.TradeAccount,
    sourceAccountType: TransactionAccountType.TradeAccount,
  },
};

export default function IBCustomerDetailPage({
  params,
}: {
  params: Promise<{ accountId: string }>;
}) {
  const { accountId } = use(params);
  const accountUid = parseInt(accountId, 10);
  const td = useTranslations('ib.customerDetail');
  const tState = useTranslations('accounts.transactionState');
  const getCurrencyName = useCurrencyName();
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);

  const [customer, setCustomer] = useState<IBClientAccount | null>(null);
  const [tab, setTab] = useState<DetailTab>('deposit');
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const [deposits, setDeposits] = useState<IBDepositRecord[]>([]);
  const [withdrawals, setWithdrawals] = useState<IBWithdrawalRecord[]>([]);
  const [transfers, setTransfers] = useState<TransferItem[]>([]);
  const [rebates] = useState<IBRebateRecord[]>([]);
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({
    stateIds: DEFAULT_DEPOSIT_STATE_IDS,
    size: DEFAULT_PAGE_SIZE,
    accountUid: String(accountUid),
    ...(TAB_FIXED_FILTER_PARAMS.deposit ?? {}),
  });

  const loadCustomer = useCallback(async () => {
    if (!agentAccount || !accountUid) return;
    const result = await execute(getIBClients, agentAccount.uid, {
      page: 1,
      size: 1,
      uid: String(accountUid),
    });
    if (result.success && result.data?.data?.length) {
      const found = result.data.data.find((c) => c.uid === accountUid);
      if (found) setCustomer(found);
    }
  }, [agentAccount, accountUid, execute]);

  const getDefaultFilterParams = useCallback((tabKey: DetailTab): Record<string, unknown> => {
    const fixedParams = TAB_FIXED_FILTER_PARAMS[tabKey] ?? {};
    if (tabKey === 'deposit') {
      return {
        stateIds: DEFAULT_DEPOSIT_STATE_IDS,
        size: DEFAULT_PAGE_SIZE,
        accountUid: String(accountUid),
        ...fixedParams,
      };
    }
    if (tabKey === 'withdrawal') {
      return {
        stateIds: DEFAULT_WITHDRAWAL_STATE_IDS,
        size: DEFAULT_PAGE_SIZE,
        accountUid: String(accountUid),
        ...fixedParams,
      };
    }
    return {
      stateIds: DEFAULT_TRANSACTION_STATE_IDS,
      size: DEFAULT_PAGE_SIZE,
      ...fixedParams,
    };
  }, [accountUid]);

  const getFilterType = useCallback((tabKey: DetailTab): TradeFilterType => {
    if (tabKey === 'deposit') return 'deposit';
    if (tabKey === 'withdrawal') return 'withdrawal';
    return 'transaction';
  }, []);

  const loadData = useCallback(async (p: number, extraParams?: Record<string, unknown>) => {
    if (!agentAccount || !accountUid || tab === 'tradeReport') return;
    setIsLoading(true);
    try {
      const tabFixedParams = TAB_FIXED_FILTER_PARAMS[tab] ?? {};
      const params = { ...filterParams, ...extraParams, ...tabFixedParams };

      if (tab === 'deposit') {
        const result = await execute(getIBDeposits, agentAccount.uid, {
          page: p,
          size: pageSize,
          ...params,
        });
        if (result.success && result.data) {
          setDeposits(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'withdrawal') {
        const result = await execute(getIBWithdrawals, agentAccount.uid, {
          page: p,
          size: pageSize,
          ...params,
        });
        if (result.success && result.data) {
          setWithdrawals(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'transfer') {
        const result = await execute(getIBAccountTransactions, agentAccount.uid, accountUid, {
          page: p,
          size: pageSize,
          ...params,
        });
        if (result.success && result.data) {
          const raw = Array.isArray(result.data.data) ? result.data.data : [];
          setTransfers(raw.map((r: unknown) => {
            const o = r as Record<string, unknown>;
            return { ...o, createdOn: String(o.createdOn || '') } as TransferItem;
          }));
          setTotal((result.data.criteria as Record<string, number>)?.total || 0);
        }
      } else if (tab === 'commissionReport') {
        // TODO: 原项目中此 tab 未实现数据渲染，暂用 getIBRebates 占位，后续需确认正确的 API 和字段
        // const result = await execute(getIBRebates, agentAccount.uid, {
        //   page: p, size: pageSize, searchText: String(accountUid), ...dateParams,
        // });
        // if (result.success && result.data) {
        //   setRebates(Array.isArray(result.data.data) ? result.data.data : []);
        //   setTotal(result.data.criteria?.total || 0);
        // }
      }
    } finally {
      setIsLoading(false);
    }
  }, [agentAccount, accountUid, tab, execute, pageSize, filterParams]);

  useEffect(() => {
    loadCustomer();
  }, [loadCustomer]);

  useEffect(() => {
    loadData(page);
  }, [tab, page, loadData]);

  const handleSearch = (params: Record<string, unknown>) => {
    const tabFixedParams = TAB_FIXED_FILTER_PARAMS[tab] ?? {};
    const mergedParams = { ...params, ...tabFixedParams };
    if (typeof params.size === 'number') setPageSize(params.size);
    setFilterParams(mergedParams);
    setPage(1);
    loadData(1, mergedParams);
  };
  const handleTabChange = (key: DetailTab) => {
    setTab(key);
    setPage(1);
    setTotal(0);
    const defaults = getDefaultFilterParams(key);
    setFilterParams(defaults);
    if (typeof defaults.size === 'number') setPageSize(defaults.size);
  };
  const fetchTradeData = useCallback(async (params: Record<string, unknown>) => {
    if (!agentAccount || !accountUid) return null;
    const result = await execute(getIBAccountTrades, agentAccount.uid, accountUid, params);
    if (result.success && result.data) {
      return { data: result.data.data, criteria: result.data.criteria };
    }
    return null;
  }, [agentAccount, accountUid, execute]);

  const userName = customer?.user?.nativeName || customer?.user?.displayName || 'Customer';
  const roleLabel = customer?.role === 1 ? td('roleIB') : td('roleClient');

  const tabs: TabItem<DetailTab>[] = useMemo(() => [
    { key: 'deposit', label: td('tabs.deposit') },
    { key: 'withdrawal', label: td('tabs.withdrawal') },
    { key: 'transfer', label: td('tabs.transfer') },
    { key: 'tradeReport', label: td('tabs.tradeReport') },
    { key: 'commissionReport', label: td('tabs.commissionReport') },
  ], [td]);

  const tradeAccount = customer?.tradeAccount;
  const currencyId = tradeAccount?.currencyId || CurrencyTypes.USD;

  // --- Grouped DataTable configs for deposit/withdrawal/transfer ---

  const groupHeaderRender = useCallback((groupKey: string) => {
    const [monthYear, weekday] = groupKey.split('||');
    return (
      <div className="flex flex-col">
        <span className="text-lg font-bold text-text-primary">{monthYear}</span>
        <span className="text-sm text-text-secondary">{weekday}</span>
      </div>
    );
  }, []);

  const dateGroupConfig = useMemo<DataTableGroupConfig<{ createdOn: string }>>(() => ({
    groupBy: (item) => formatGroupKey(item.createdOn),
    renderGroupHeader: groupHeaderRender,
    headerWidth: 'w-[120px]',
  }), [groupHeaderRender]);

  const depositColumns = useMemo<DataTableColumn<IBDepositRecord>[]>(() => [
    {
      key: 'deposit',
      title: td('columns.deposit'),
      skeletonWidth: 'w-28',
      render: (item) => (
        <div className="flex flex-col">
          <span className="flex items-center gap-1 text-sm">
            <span className="text-xs font-bold text-[#004eff]">↓</span>
            No.{item.targetTradeAccount?.accountNumber}
            <span className="text-xs text-text-secondary">{getCurrencyCode(item.targetTradeAccount?.currencyId ?? item.currencyId)}</span>
          </span>
          <span className="text-xs">{td('columns.group')}: {item.targetTradeAccount?.group || '--'}</span>
          
        </div>
      ),
    },
    {
      key: 'status',
      title: td('columns.status'),
      align: 'center',
      skeletonWidth: 'w-16',
      render: (item) => <Tag variant={getStateTagVariant(item.stateId)} soft>{tState(String(item.stateId))}</Tag>,
    },
    {
      key: 'currency',
      title: td('columns.currency'),
      align: 'center',
      skeletonWidth: 'w-12',
      render: (item) => <span className="text-sm">{getCurrencyName(item.currencyId)}</span>,
    },
    {
      key: 'amount',
      title: td('columns.amount'),
      align: 'right',
      skeletonWidth: 'w-20',
      render: (item) => <BalanceShow balance={item.amount} currencyId={item.currencyId} className="text-sm" />,
    },
    {
      key: 'time',
      title: td('columns.time'),
      align: 'right',
      skeletonWidth: 'w-28',
      render: (item) => <span className="text-sm">{formatDateTime(item.createdOn)}</span>,
    },
  ], [td, tState, getCurrencyName]);

  const withdrawalColumns = useMemo<DataTableColumn<IBWithdrawalRecord>[]>(() => [
    {
      key: 'withdrawal',
      title: td('columns.withdrawal'),
      skeletonWidth: 'w-28',
      render: (item) => (
        <div className="flex flex-col">
          <span className="flex items-center gap-1 text-sm">
            <span className="text-xs font-bold text-[#e02b1d]">↑</span>
            No.{item.source?.displayNumber}
            <span className="text-xs text-text-secondary">{getCurrencyCode(item.targetTradeAccount?.currencyId ?? item.currencyId)}</span>
          </span>
          <span className="text-xs">{td('columns.group')}: {item.source?.agentGroupName || '--'}</span>
        </div>
      ),
    },
    {
      key: 'status',
      title: td('columns.status'),
      align: 'center',
      skeletonWidth: 'w-16',
      render: (item) => <Tag variant={getStateTagVariant(item.stateId)} soft>{tState(String(item.stateId))}</Tag>,
    },
    {
      key: 'currency',
      title: td('columns.currency'),
      align: 'center',
      skeletonWidth: 'w-12',
      render: (item) => <span className="text-sm">{getCurrencyName(item.currencyId)}</span>,
    },
    {
      key: 'amount',
      title: td('columns.amount'),
      align: 'right',
      skeletonWidth: 'w-20',
      render: (item) => <BalanceShow balance={item.amount} currencyId={item.currencyId} className="text-sm" />,
    },
    {
      key: 'time',
      title: td('columns.time'),
      align: 'right',
      skeletonWidth: 'w-28',
      render: (item) => <span className="text-sm">{formatDateTime(item.createdOn)}</span>,
    },
  ], [td, tState, getCurrencyName]);

  const transferColumns = useMemo<DataTableColumn<TransferItem>[]>(() => [
    {
      key: 'account',
      title: td('columns.account'),
      skeletonWidth: 'w-48',
      render: (item) => (
        <div className="flex items-center gap-2">
          <div className="flex flex-col">
            <div>
              <span className="text-sm">No.{item.sourceAccount.accountNumber || '--'}</span>
              <span className="text-xs text-text-secondary">&nbsp;{getCurrencyCode(item.sourceAccount.currencyId ?? item.currencyId ?? 840)}</span>
            </div>
            <span className="text-xs text-text-secondary">{td('columns.group')}:{item.sourceAccount.group || '--'}</span>
          </div>
          <span className="text-xs font-bold text-[#004eff]">→</span>
          <div className="flex flex-col">
            <div>
              <span className="text-sm">No.{item.targetAccount.accountNumber || '--'}</span>
              <span className="text-xs text-text-secondary">&nbsp;{getCurrencyCode(item.targetAccount.currencyId ?? item.currencyId ?? 840)}</span>
            </div>    
            <span className="text-xs">{td('columns.group')}:{item.targetAccount.group || '--'}</span>
             
          </div>
        </div>
      ),
    },
    {
      key: 'status',
      title: td('columns.status'),
      align: 'center',
      skeletonWidth: 'w-16',
      render: (item) => <Tag variant={getStateTagVariant(item.stateId || 0)} soft>{tState(String(item.stateId || 0))}</Tag>,
    },
    {
      key: 'currency',
      title: td('columns.currency'),
      align: 'center',
      skeletonWidth: 'w-12',
      render: (item) => <span className="text-sm">{getCurrencyName(item.currencyId || 840)}</span>,
    },
    {
      key: 'amount',
      title: td('columns.amount'),
      align: 'right',
      skeletonWidth: 'w-20',
      render: (item) => <BalanceShow balance={item.amount || 0} currencyId={item.currencyId || 840} className="text-sm" />,
    },
    {
      key: 'time',
      title: td('columns.time'),
      align: 'right',
      skeletonWidth: 'w-28',
      render: (item) => <span className="text-sm">{formatDateTime(item.createdOn)}</span>,
    },
  ], [td, tState, getCurrencyName]);

  const rebateColumns = useMemo<DataTableColumn<IBRebateRecord>[]>(() => [
    { key: 'ticket', title: td('columns.ticket'), skeletonWidth: 'w-16', render: (item) => item.trade?.ticket || '-' },
    { key: 'symbol', title: td('columns.symbol'), skeletonWidth: 'w-16', render: (item) => item.trade?.symbol || '-' },
    { key: 'volume', title: td('columns.volume'), skeletonWidth: 'w-12', render: (item) => item.trade?.volume != null ? item.trade.volume / 100 : '-' },
    { key: 'rebateRate', title: td('columns.rebateRate'), skeletonWidth: 'w-16', render: (item) => item.rebateRate },
    { key: 'amount', title: td('columns.amount'), skeletonWidth: 'w-20', render: (item) => <BalanceShow balance={item.amount} currencyId={item.currencyId} className="text-sm" /> },
    { key: 'sourceAccount', title: td('columns.sourceAccount'), skeletonWidth: 'w-20', render: (item) => item.sourceAccountNumber },
    { key: 'time', title: td('columns.time'), skeletonWidth: 'w-28', render: (item) => <span className="text-xs">{formatDateTime(item.createdOn)}</span> },
  ], [td]);

  const renderTable = () => {
    switch (tab) {
      case 'deposit':
        return (
          <DataTable<IBDepositRecord>
            columns={depositColumns}
            data={deposits}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<IBDepositRecord>}
          />
        );
      case 'withdrawal':
        return (
          <DataTable<IBWithdrawalRecord>
            columns={withdrawalColumns}
            data={withdrawals}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<IBWithdrawalRecord>}
          />
        );
      case 'transfer':
        return (
          <DataTable<TransferItem>
            columns={transferColumns}
            data={transfers}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<TransferItem>}
          />
        );
      case 'commissionReport':
        return (
          <DataTable<IBRebateRecord>
            columns={rebateColumns}
            data={rebates}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<IBRebateRecord>}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="@container flex w-full flex-col gap-3">
      {/* Back Link */}
      <Link href="/ib/customers" className="flex items-center gap-3">
        <Icon name="arrow-left" size={24} className="text-text-secondary" />
        <span className="text-xl text-text-secondary">{td('backToList')}</span>
      </Link>

      <div className="h-px w-full bg-border" />

      {/* User Info + Stats Cards */}
      <div className="flex flex-col gap-3 @[1100px]:flex-row @[1100px]:gap-5">
        <div className="flex flex-1 flex-col items-center gap-4 rounded bg-surface px-5 py-4 @[1100px]:gap-5 @[1100px]:px-10 @[1100px]:py-5">
          {customer ? (
            <>
              <div className="flex items-center gap-4 @[1100px]:gap-5">
                <Avatar src={customer.user?.avatar} alt={userName} size="md" />
                <div className="flex flex-col justify-between">
                  <span className="text-lg font-semibold text-text-primary @[1100px]:text-xl">{userName}</span>
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{customer.user?.email}</span>
                </div>
              </div>
              <div className="h-px w-full bg-border" />
              <div className="flex w-full items-center justify-around">
                <div className="flex flex-col items-center gap-1">
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{td('role')}</span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-base">{roleLabel}</span>
                </div>
                <div className="flex flex-col items-center gap-1">
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{td('account')}</span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-sm">{tradeAccount?.accountNumber || accountUid}</span>
                </div>
              </div>
            </>
          ) : (
            <div className="flex flex-col items-center gap-3 py-4">
              <Skeleton className="h-[60px] w-[60px] rounded-full" />
              <Skeleton className="h-5 w-32" />
              <Skeleton className="h-4 w-48" />
            </div>
          )}
        </div>

        <div className="grid flex-3 grid-cols-3 gap-3 @[1100px]:contents">
          <div className="flex flex-1 flex-col items-center justify-center gap-5 rounded bg-surface px-3 py-4 max-sm:gap-3 sm:gap-10 sm:px-5 @[1100px]:px-10 @[1100px]:py-5">
            <span className="text-xl font-semibold text-text-primary max-sm:text-sm">{td('balance')}</span>
            {customer ? (
              <BalanceShow balance={tradeAccount?.balanceInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary max-sm:text-base!" />
            ) : (
              <Skeleton className="h-8 w-40 max-sm:w-full" />
            )}
          </div>

          <div className="flex flex-1 flex-col items-center justify-center gap-5 rounded bg-surface px-3 py-4 max-sm:gap-3 sm:gap-10 sm:px-5 @[1100px]:px-10 @[1100px]:py-5">
            <span className="text-xl font-semibold text-text-primary max-sm:text-sm">{td('equity')}</span>
            {customer ? (
              <BalanceShow balance={tradeAccount?.equityInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary max-sm:text-base!" />
            ) : (
              <Skeleton className="h-8 w-40 max-sm:w-full" />
            )}
          </div>

          <div className="flex flex-1 flex-col items-center justify-center gap-5 rounded bg-surface px-3 py-4 max-sm:gap-3 sm:gap-10 sm:px-5 @[1100px]:px-10 @[1100px]:py-5">
            <span className="text-xl font-semibold text-text-primary max-sm:text-sm">{td('credit')}</span>
            {customer ? (
              <BalanceShow balance={tradeAccount?.creditInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary max-sm:text-base!" />
            ) : (
              <Skeleton className="h-8 w-40 max-sm:w-full" />
            )}
          </div>
        </div>
      </div>

      {/* Main Content Card */}
      <div className="flex flex-1 flex-col gap-5 rounded bg-surface p-5">
        {/* Tabs + Filter Bar */}
        <div className="flex flex-col gap-3 border-b border-border pb-0 lg:flex-row lg:flex-wrap lg:items-end lg:justify-between lg:gap-4">
          <div className="overflow-x-auto">
            <Tabs
              tabs={tabs}
              activeKey={tab}
              onChange={handleTabChange}
              size="lg"
              showDivider={false}
            />
          </div>
          {(tab === 'deposit' || tab === 'withdrawal' || tab === 'transfer') && (
            <div className="mb-3 w-full lg:w-auto">
              <TradeFilter
                type={getFilterType(tab)}
                filterOptions={['stateIds', 'datePicker', 'pageSize']}
                defaultParam={{ pageSize: DEFAULT_PAGE_SIZE }}
                fixedParams={{
                  ...(TAB_FIXED_FILTER_PARAMS[tab] ?? {}),
                  ...(tab === 'transfer' ? {} : { accountUid: String(accountUid) }),
                }}
                onSearch={handleSearch}
                isLoading={isLoading}
              />
            </div>
          )}
        </div>

        {tab === 'tradeReport' ? (
          <TradeReportTable
            fetchData={fetchTradeData}
            filterOptions={['isClosed', 'product', 'datePicker', 'allHistory']}
            pageSize={pageSize}
            autoFetchKey={`${agentAccount?.uid}-${accountUid}`}
          />
        ) : (
          <>
            {/* Results count */}
            <p className="text-xl font-semibold text-text-secondary">
              {td.rich('showResults', {
                count: String(total),
                num: (chunks) => <span className="text-text-primary">{chunks}</span>,
              })}
            </p>

            {/* Table Content */}
            <div className="flex-1 overflow-x-auto">
              {renderTable()}
            </div>

            {/* Pagination */}
            <Pagination page={page} total={total} size={pageSize} onPageChange={setPage} />
          </>
        )}
      </div>
    </div>
  );
}
