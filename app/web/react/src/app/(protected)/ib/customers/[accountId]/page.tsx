'use client';

import { useState, useEffect, useCallback, use, useMemo } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import {
  Avatar,
  BalanceShow,
  DatePicker,
  Skeleton,
  Tabs,
  Button,
  Pagination,
  Tag,
  DataTable,
  Icon,
} from '@/components/ui';
import type { TabItem, DateRange, DataTableColumn, DataTableGroupConfig } from '@/components/ui';
import type { TagVariant } from '@/components/ui';
import {
  getIBClients,
  getIBDeposits,
  getIBWithdrawals,
  getIBAccountTrades,
  getIBAccountTransactions,
  getIBRebates,
} from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import type {
  IBClientAccount,
  IBDepositRecord,
  IBWithdrawalRecord,
  IBTradeRecord,
  IBRebateRecord,
} from '@/types/ib';
import {
  DepositState,
  WithdrawalState,
  TransferState,
  CurrencyTypes,
} from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';

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
}

export default function IBCustomerDetailPage({
  params,
}: {
  params: Promise<{ accountId: string }>;
}) {
  const { accountId } = use(params);
  const accountUid = parseInt(accountId, 10);
  const t = useTranslations('ib');
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
  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined);
  const pageSize = 15;

  const [deposits, setDeposits] = useState<IBDepositRecord[]>([]);
  const [withdrawals, setWithdrawals] = useState<IBWithdrawalRecord[]>([]);
  const [transfers, setTransfers] = useState<TransferItem[]>([]);
  const [trades, setTrades] = useState<IBTradeRecord[]>([]);
  const [rebates, setRebates] = useState<IBRebateRecord[]>([]);

  const loadCustomer = useCallback(async () => {
    if (!agentAccount || !accountUid) return;
    const result = await execute(getIBClients, agentAccount.uid, {
      page: 1,
      size: 1,
      searchText: String(accountUid),
    });
    if (result.success && result.data?.data?.length) {
      const found = result.data.data.find((c) => c.uid === accountUid);
      if (found) setCustomer(found);
    }
  }, [agentAccount, accountUid, execute]);

  const buildDateParams = useCallback(() => {
    const p: Record<string, unknown> = {};
    if (dateRange?.from) p.from = dateRange.from.toISOString();
    if (dateRange?.to) p.to = dateRange.to.toISOString();
    return p;
  }, [dateRange]);

  const loadData = useCallback(async (p: number) => {
    if (!agentAccount || !accountUid) return;
    setIsLoading(true);
    try {
      const dateParams = buildDateParams();
      const params = { page: p, size: pageSize, searchText: String(accountUid), ...dateParams };

      if (tab === 'deposit') {
        const result = await execute(getIBDeposits, agentAccount.uid, params);
        if (result.success && result.data) {
          setDeposits(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'withdrawal') {
        const result = await execute(getIBWithdrawals, agentAccount.uid, params);
        if (result.success && result.data) {
          setWithdrawals(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'transfer') {
        const result = await execute(getIBAccountTransactions, agentAccount.uid, accountUid, {
          page: p, size: pageSize, ...dateParams,
        });
        if (result.success && result.data) {
          const raw = Array.isArray(result.data.data) ? result.data.data : [];
          setTransfers(raw.map((r: unknown) => {
            const o = r as Record<string, unknown>;
            return { ...o, createdOn: String(o.createdOn || '') } as TransferItem;
          }));
          setTotal((result.data.criteria as Record<string, number>)?.total || 0);
        }
      } else if (tab === 'tradeReport') {
        const result = await execute(getIBAccountTrades, agentAccount.uid, accountUid, {
          page: p, size: pageSize, ...dateParams,
        });
        if (result.success && result.data) {
          setTrades(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
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
  }, [agentAccount, accountUid, tab, execute, buildDateParams]);

  useEffect(() => {
    loadCustomer();
  }, [loadCustomer]);

  useEffect(() => {
    loadData(page);
  }, [tab, page, loadData]);

  const handleSearch = () => { setPage(1); loadData(1); };
  const handleReset = () => { setDateRange(undefined); setPage(1); };
  const handleTabChange = (key: DetailTab) => { setTab(key); setPage(1); setTotal(0); };

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

  const tradeGroupConfig = useMemo<DataTableGroupConfig<IBTradeRecord>>(() => ({
    groupBy: (item) => formatGroupKey(item.openTime || item.openAt || ''),
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
      align: 'center',
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
      align: 'center',
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
      skeletonWidth: 'w-20',
      render: (item) => <span className="text-sm">No.{item.accountNumber || '--'}</span>,
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
      align: 'center',
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

  const tradeColumns = useMemo<DataTableColumn<IBTradeRecord>[]>(() => [
    { key: 'ticket', title: td('columns.ticket'), skeletonWidth: 'w-16', render: (item) => item.ticket },
    { key: 'symbol', title: td('columns.symbol'), skeletonWidth: 'w-16', render: (item) => item.symbol },
    { key: 'volume', title: td('columns.volume'), skeletonWidth: 'w-12', render: (item) => item.volume },
    { key: 'openTime', title: td('columns.openTime'), skeletonWidth: 'w-28', render: (item) => <span className="text-xs">{item.openTime ? formatDateTime(item.openTime) : '-'}</span> },
    { key: 'closeTime', title: td('columns.closeTime'), skeletonWidth: 'w-28', render: (item) => <span className="text-xs">{item.closeTime ? formatDateTime(item.closeTime) : '-'}</span> },
    { key: 'profit', title: td('columns.profit'), skeletonWidth: 'w-16', render: (item) => <span className={(item.profit || 0) >= 0 ? 'text-green-600' : 'text-red-600'}>{item.profit?.toFixed(2)}</span> },
    { key: 'commission', title: td('columns.commission'), skeletonWidth: 'w-16', render: (item) => item.commission?.toFixed(2) },
    { key: 'swap', title: td('columns.swap'), skeletonWidth: 'w-12', render: (item) => item.swap?.toFixed(2) },
  ], [td]);

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
      case 'tradeReport':
        return (
          <DataTable<IBTradeRecord>
            columns={tradeColumns}
            data={trades}
            rowKey={(item, idx) => item.id ?? idx}
            loading={isLoading}
            skeletonRows={5}
            groupConfig={tradeGroupConfig}
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
    <div className="flex w-full flex-col gap-5">
      {/* Back Link */}
      <Link href="/ib/customers" className="flex items-center gap-3">
        <Icon name="arrow-left" size={24} className="text-text-secondary" />
        <span className="text-xl text-text-secondary">{td('backToList')}</span>
      </Link>

      <div className="h-px w-full bg-border" />

      {/* User Info + Stats Cards */}
      <div className="flex gap-5">
        <div className="flex flex-1 flex-col items-center gap-5 rounded bg-surface px-10 py-5">
          {customer ? (
            <>
              <div className="flex items-center gap-5">
                <Avatar src={customer.user?.avatar} alt={userName} size="md" />
                <div className="flex flex-col justify-between">
                  <span className="text-xl font-semibold text-text-primary">{userName}</span>
                  <span className="text-sm text-text-secondary">{customer.user?.email}</span>
                </div>
              </div>
              <div className="h-px w-full bg-border" />
              <div className="flex w-full items-center justify-around">
                <div className="flex flex-col items-center gap-1">
                  <span className="text-sm text-text-secondary">{td('role')}</span>
                  <span className="text-base font-semibold text-text-primary">{roleLabel}</span>
                </div>
                <div className="flex flex-col items-center gap-1">
                  <span className="text-sm text-text-secondary">{td('account')}</span>
                  <span className="text-base font-semibold text-text-primary">{tradeAccount?.accountNumber || accountUid}</span>
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

        <div className="flex flex-1 flex-col items-center justify-center gap-10 rounded bg-surface px-10 py-5">
          <span className="text-xl font-semibold text-text-primary">{td('balance')}</span>
          {customer ? (
            <BalanceShow balance={tradeAccount?.balanceInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary" />
          ) : (
            <Skeleton className="h-8 w-40" />
          )}
        </div>

        <div className="flex flex-1 flex-col items-center justify-center gap-10 rounded bg-surface px-10 py-5">
          <span className="text-xl font-semibold text-text-primary">{td('equity')}</span>
          {customer ? (
            <BalanceShow balance={tradeAccount?.equityInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary" />
          ) : (
            <Skeleton className="h-8 w-40" />
          )}
        </div>

        <div className="flex flex-1 flex-col items-center justify-center gap-10 rounded bg-surface px-10 py-5">
          <span className="text-xl font-semibold text-text-primary">{td('credit')}</span>
          {customer ? (
            <BalanceShow balance={tradeAccount?.creditInCents || 0} currencyId={currencyId} sign="+" className="text-responsive-3xl font-bold text-primary" />
          ) : (
            <Skeleton className="h-8 w-40" />
          )}
        </div>
      </div>

      {/* Main Content Card */}
      <div className="flex flex-1 flex-col gap-5 rounded bg-surface p-5">
        {/* Tabs + Filter Bar */}
        <div className="flex flex-wrap items-end justify-between gap-4 border-b border-border pb-0">
          <Tabs
            tabs={tabs}
            activeKey={tab}
            onChange={handleTabChange}
            size="lg"
            showDivider={false}
          />
          <div className="mb-3 flex shrink-0 items-center gap-3">
            <DatePicker mode="range" size="sm" value={dateRange} onChange={setDateRange} />
            <Button variant="outline" size="sm" className="shrink-0 whitespace-nowrap" onClick={handleReset}>
              <Icon name="reset-line" />
              {t('action.reset')}
            </Button>
            <Button variant="primary" size="sm" className="shrink-0 whitespace-nowrap" onClick={handleSearch}>
              <Icon name="search-line" />
              {t('action.search')}
            </Button>
          </div>
        </div>

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
      </div>
    </div>
  );
}
