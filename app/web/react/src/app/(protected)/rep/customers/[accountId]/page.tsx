'use client';

import { useState, useEffect, useCallback, use, useMemo } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
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
  getRepClients,
  getRepClientTrades,
  getRepClientTransactions,
  getRepDeposits,
  getRepWithdrawals,
} from '@/actions';
import { useRepStore } from '@/stores/repStore';
import { TimeShow } from '@/components/TimeShow';
import {
  AccountRoleTypes,
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
import type { RepClientAccount } from '@/types/rep';
import type {
  SalesDepositRecord,
  SalesWithdrawalRecord,
  SalesTransactionRecord,
} from '@/types/sales';

type DetailTab = 'transaction' | 'deposit' | 'withdrawal' | 'tradeReport';

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


function formatGroupKey(dateStr: string) {
  const d = new Date(dateStr);
  const monthYear = d.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  const weekday = d.toLocaleDateString('en-US', { weekday: 'long' });
  return `${monthYear}||${weekday}`;
}

const DEFAULT_DEPOSIT_STATE_IDS = [
  DepositState.DepositCompleted,
  DepositState.DepositCallbackComplete,
];

const DEFAULT_WITHDRAWAL_STATE_IDS = [
  WithdrawalState.WithdrawalCompleted,
];

const DEFAULT_TRANSACTION_STATE_IDS = [
  TransferState.TransferCompleted,
];

const TAB_FIXED_FILTER_PARAMS: Partial<Record<DetailTab, Record<string, unknown>>> = {
  deposit: { isClosed: false },
  withdrawal: { isClosed: false },
  transaction: {
    targetAccountType: TransactionAccountType.TradeAccount,
    sourceAccountType: TransactionAccountType.TradeAccount,
    isClosed: false,
  },
};

function getRoleLabel(role: number, td: (key: string) => string): string {
  if (role === AccountRoleTypes.IB) return td('roleIB');
  if (role === AccountRoleTypes.Sales) return td('roleSales');
  return td('roleClient');
}

export default function RepCustomerDetailPage({
  params,
}: {
  params: Promise<{ accountId: string }>;
}) {
  const { accountId } = use(params);
  const accountUid = parseInt(accountId, 10);
  const router = useRouter();
  const t = useTranslations('rep');
  const td = useTranslations('rep.customerDetail');
  const tState = useTranslations('accounts.transactionState');
  const getCurrencyName = useCurrencyName();
  const { execute } = useServerAction({ showErrorToast: true });
  const repAccount = useRepStore((s) => s.repAccount);

  const [accountDetail, setAccountDetail] = useState<RepClientAccount | null>(null);
  const [tab, setTab] = useState<DetailTab>('transaction');
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(15);

  const [deposits, setDeposits] = useState<SalesDepositRecord[]>([]);
  const [withdrawals, setWithdrawals] = useState<SalesWithdrawalRecord[]>([]);
  const [transactions, setTransactions] = useState<SalesTransactionRecord[]>([]);
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({
    stateIds: DEFAULT_TRANSACTION_STATE_IDS,
    size: 15,
    ...(TAB_FIXED_FILTER_PARAMS.transaction ?? {}),
  });

  useEffect(() => {
    if (!repAccount || !accountUid) return;
    (async () => {
      try {
        const result = await execute(getRepClients, repAccount.uid, { uid: accountUid });
        if (result.success && result.data?.data?.length) {
          setAccountDetail(result.data.data[0]);
        } else {
          router.push('/rep/customers');
        }
      } catch {
        router.push('/rep/customers');
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [repAccount, accountUid]);

  const tabs: TabItem<DetailTab>[] = useMemo(() => {
    const list: TabItem<DetailTab>[] = [
      { key: 'deposit', label: td('tabs.deposit') },
      { key: 'withdrawal', label: td('tabs.withdrawal') },
      { key: 'transaction', label: td('tabs.transaction') },
    ];
    if (accountDetail?.tradeAccount?.accountNumber && accountDetail.tradeAccount.accountNumber !== 0) {
      list.push({ key: 'tradeReport', label: td('tabs.tradeReport') });
    }
    return list;
  }, [td, accountDetail]);

  const getDefaultFilterParams = useCallback((tabKey: DetailTab): Record<string, unknown> => {
    const fixedParams = TAB_FIXED_FILTER_PARAMS[tabKey] ?? {};
    if (tabKey === 'deposit') {
      return { stateIds: DEFAULT_DEPOSIT_STATE_IDS, size: 15, ...fixedParams };
    }
    if (tabKey === 'withdrawal') {
      return { stateIds: DEFAULT_WITHDRAWAL_STATE_IDS, size: 15, ...fixedParams };
    }
    return { stateIds: DEFAULT_TRANSACTION_STATE_IDS, size: 15, ...fixedParams };
  }, []);

  const getFilterType = useCallback((tabKey: DetailTab): TradeFilterType => {
    if (tabKey === 'deposit') return 'deposit';
    if (tabKey === 'withdrawal') return 'withdrawal';
    return 'transaction';
  }, []);

  const loadData = useCallback(async (p: number, extraParams?: Record<string, unknown>) => {
    if (!repAccount || !accountUid || tab === 'tradeReport') return;
    setIsLoading(true);
    try {
      const tabFixedParams = TAB_FIXED_FILTER_PARAMS[tab] ?? {};
      const params = { ...filterParams, ...extraParams, ...tabFixedParams };
      if (tab === 'transaction') {
        const result = await execute(getRepClientTransactions, repAccount.uid, accountUid, {
          page: p, size: pageSize, ...params,
        });
        if (result.success && result.data) {
          const d = result.data as { data?: SalesTransactionRecord[]; criteria?: { total?: number } };
          setTransactions(Array.isArray(d.data) ? d.data : []);
          setTotal(d.criteria?.total || 0);
        }
      } else if (tab === 'deposit') {
        const result = await execute(getRepDeposits, repAccount.uid, {
          page: p, size: pageSize, accountUid, ...params,
        });
        if (result.success && result.data) {
          setDeposits(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'withdrawal') {
        const result = await execute(getRepWithdrawals, repAccount.uid, {
          page: p, size: pageSize, accountUid, ...params,
        });
        if (result.success && result.data) {
          setWithdrawals(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      }
    } finally {
      setIsLoading(false);
    }
  }, [repAccount, accountUid, tab, execute, pageSize, filterParams]);

  useEffect(() => {
    if (repAccount) loadData(page);
  }, [tab, page, loadData, repAccount]);

  const handleTabChange = (key: DetailTab) => {
    setTab(key);
    setPage(1);
    setTotal(0);
    const defaults = getDefaultFilterParams(key);
    setFilterParams(defaults);
    if (typeof defaults.size === 'number') setPageSize(defaults.size);
  };

  const handleSearch = (params: Record<string, unknown>) => {
    const tabFixedParams = TAB_FIXED_FILTER_PARAMS[tab] ?? {};
    const mergedParams = { ...params, ...tabFixedParams };
    if (typeof params.size === 'number') setPageSize(params.size);
    setFilterParams(mergedParams);
    setPage(1);
    loadData(1, mergedParams);
  };

  const fetchTradeData = useCallback(async (params: Record<string, unknown>) => {
    if (!repAccount || !accountUid) return null;
    const result = await execute(getRepClientTrades, repAccount.uid, accountUid, params);
    if (result.success && result.data) {
      return { data: result.data.data, criteria: result.data.criteria };
    }
    return null;
  }, [repAccount, accountUid, execute]);

  const user = accountDetail?.user;
  const tradeAccount = accountDetail?.tradeAccount;
  const userName = user?.nativeName || user?.displayName || '--';
  const isIBOrBroker = accountDetail?.role === AccountRoleTypes.IB || accountDetail?.role === AccountRoleTypes.Broker;
  const isClient = accountDetail?.role === AccountRoleTypes.Client;
  const currencyId = tradeAccount?.currencyId || CurrencyTypes.USD;

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

  const transactionColumns = useMemo<DataTableColumn<SalesTransactionRecord>[]>(() => [
    {
      key: 'account',
      title: td('columns.transaction'),
      skeletonWidth: 'w-48',
      render: (item) => (
        <div className="flex items-center gap-2">
          <div className="flex flex-col">
            <div>
              <span className="text-sm">No.{item.sourceAccount?.accountNumber || '--'}</span>
              <span className="ml-1 text-xs text-text-secondary">{getCurrencyCode(item.sourceAccount?.currencyId ?? item.currencyId)}</span>
            </div>
            <span className="text-xs text-text-secondary">{td('columns.group')}: {item.sourceAccount?.group || '--'}</span>
          </div>
          <span className="text-xs font-bold text-[#004eff]">→</span>
          <div className="flex flex-col">
            <div>
              <span className="text-sm">No.{item.targetAccount?.accountNumber || '--'}</span>
              <span className="ml-1 text-xs text-text-secondary">{getCurrencyCode(item.targetAccount?.currencyId ?? item.currencyId)}</span>
            </div>
            <span className="text-xs text-text-secondary">{td('columns.group')}: {item.targetAccount?.group || '--'}</span>
          </div>
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
      render: (item) => <TimeShow type="inFields" dateIsoString={item.createdOn} />,
    },
  ], [td, tState, getCurrencyName]);

  const depositColumns = useMemo<DataTableColumn<SalesDepositRecord>[]>(() => [
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
          <span className="text-xs text-text-secondary">{td('columns.group')}: {item.targetTradeAccount?.group || '--'}</span>
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
      render: (item) => <span className="text-sm"><TimeShow type="inFields" dateIsoString={item.createdOn} />,
    },
  ], [td, tState, getCurrencyName]);

  const withdrawalColumns = useMemo<DataTableColumn<SalesWithdrawalRecord>[]>(() => [
    {
      key: 'withdrawal',
      title: td('columns.withdrawal'),
      skeletonWidth: 'w-28',
      render: (item) => (
        <div className="flex flex-col">
          <span className="flex items-center gap-1 text-sm">
            <span className="text-xs font-bold text-[#e02b1d]">↑</span>
            No.{item.accountNumber || item.source?.accountNumber}
            <span className="text-xs text-text-secondary">{getCurrencyCode(item.source?.currencyId ?? item.currencyId)}</span>
          </span>
          <span className="text-xs text-text-secondary">{td('columns.group')}: {item.source?.agentGroupName || '--'}</span>
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
      render: (item) => <TimeShow type="inFields" dateIsoString={item.createdOn} />,
    },
  ], [td, tState, getCurrencyName]);

  const renderTable = () => {
    switch (tab) {
      case 'transaction':
        return (
          <DataTable<SalesTransactionRecord>
            columns={transactionColumns}
            data={transactions}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<SalesTransactionRecord>}
          />
        );
      case 'deposit':
        return (
          <DataTable<SalesDepositRecord>
            columns={depositColumns}
            data={deposits}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<SalesDepositRecord>}
          />
        );
      case 'withdrawal':
        return (
          <DataTable<SalesWithdrawalRecord>
            columns={withdrawalColumns}
            data={withdrawals}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={dateGroupConfig as DataTableGroupConfig<SalesWithdrawalRecord>}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="@container flex w-full flex-col gap-3">
      <Link href="/rep/customers" className="flex items-center gap-3">
        <Icon name="arrow-left" size={24} className="text-text-secondary" />
        <span className="text-xl text-text-secondary">{td('backToList')}</span>
      </Link>

      <div className="h-px w-full bg-border" />

      <div className="flex flex-col gap-3 @[1100px]:flex-row @[1100px]:gap-5">
        <div className="flex flex-1 flex-col items-center gap-4 rounded bg-surface px-5 py-4 @[1100px]:gap-5 @[1100px]:px-10 @[1100px]:py-5">
          {accountDetail ? (
            <>
              <div className="flex items-center gap-4 @[1100px]:gap-5">
                <Avatar src={user?.avatar} alt={userName} size="md" />
                <div className="flex flex-col justify-between">
                  <span className="text-lg font-semibold text-text-primary @[1100px]:text-xl">{userName}</span>
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{user?.email}</span>
                </div>
              </div>
              <div className="h-px w-full bg-border" />
              <div className="grid w-full grid-cols-4 gap-3 @[1100px]:grid-cols-2 @[1100px]:gap-4">
                <div className="flex flex-col items-center gap-1">
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">
                    {isIBOrBroker ? t('fields.uid') : t('fields.accountNo')}
                  </span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-base">
                    {isIBOrBroker ? accountDetail.uid : (tradeAccount?.accountNumber || '--')}
                  </span>
                </div>
                <div className="flex flex-col items-center gap-1">
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{td('role')}</span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-base">
                    {getRoleLabel(accountDetail.role, td)}
                  </span>
                </div>
                <div className="flex flex-col items-center gap-1">
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{t('fields.group')}</span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-base">
                    {accountDetail.group || '--'}
                  </span>
                </div>
                <div className="flex flex-col items-center gap-1">
                  <span className="text-xs text-text-secondary @[1100px]:text-sm">{t('fields.code')}</span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-base">
                    {accountDetail.code || '--'}
                  </span>
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

        {isClient && (
          <div className="grid flex-3 grid-cols-3 gap-3 @[1100px]:contents">
            <div className="flex flex-1 flex-col items-center justify-center gap-5 rounded bg-surface px-3 py-4 max-sm:gap-3 sm:gap-10 sm:px-5 @[1100px]:px-10 @[1100px]:py-5">
              <span className="text-xl font-semibold text-text-primary max-sm:text-sm">{td('balance')}</span>
              {accountDetail ? (
                <BalanceShow balance={tradeAccount?.balanceInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary max-sm:text-base!" />
              ) : (
                <Skeleton className="h-8 w-40 max-sm:w-full" />
              )}
            </div>
            <div className="flex flex-1 flex-col items-center justify-center gap-5 rounded bg-surface px-3 py-4 max-sm:gap-3 sm:gap-10 sm:px-5 @[1100px]:px-10 @[1100px]:py-5">
              <span className="text-xl font-semibold text-text-primary max-sm:text-sm">{td('equity')}</span>
              {accountDetail ? (
                <BalanceShow balance={tradeAccount?.equityInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary max-sm:text-base!" />
              ) : (
                <Skeleton className="h-8 w-40 max-sm:w-full" />
              )}
            </div>
            <div className="flex flex-1 flex-col items-center justify-center gap-5 rounded bg-surface px-3 py-4 max-sm:gap-3 sm:gap-10 sm:px-5 @[1100px]:px-10 @[1100px]:py-5">
              <span className="text-xl font-semibold text-text-primary max-sm:text-sm">{td('credit')}</span>
              {accountDetail ? (
                <BalanceShow balance={tradeAccount?.creditInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary max-sm:text-base!" />
              ) : (
                <Skeleton className="h-8 w-40 max-sm:w-full" />
              )}
            </div>
          </div>
        )}
      </div>

      <div className="flex flex-1 flex-col gap-5 rounded bg-surface p-5">
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
          {tab !== 'tradeReport' && (
            <div className="mb-3 w-full lg:w-auto">
              <TradeFilter
                type={getFilterType(tab)}
                filterOptions={['stateIds', 'datePicker', 'pageSize']}
                defaultParam={{ pageSize: 15 }}
                fixedParams={{
                  ...(TAB_FIXED_FILTER_PARAMS[tab] ?? {}),
                  ...(tab === 'transaction' ? {} : { accountUid }),
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
            autoFetchKey={`${repAccount?.uid}-${accountUid}`}
          />
        ) : (
          <>
            <p className="text-xl font-semibold text-text-secondary">
              {td.rich('showResults', {
                count: String(total),
                num: (chunks) => <span className="text-text-primary">{chunks}</span>,
              })}
            </p>
            <div className="flex-1 overflow-x-auto">
              {renderTable()}
            </div>
            <Pagination page={page} total={total} size={pageSize} onPageChange={setPage} />
          </>
        )}
      </div>
    </div>
  );
}
