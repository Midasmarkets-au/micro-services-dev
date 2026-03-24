'use client';

import { useState, useEffect, useCallback, use, useMemo, useRef } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import {
  Avatar,
  BalanceShow,
  Skeleton,
  Tabs,
  Button,
  Pagination,
  Tag,
  DataTable,
  Icon,
  DatePicker,
} from '@/components/ui';
import type { TabItem, DataTableColumn, DataTableGroupConfig, DateRange } from '@/components/ui';
import type { TagVariant } from '@/components/ui';
import {
  getSalesClients,
  getSalesClientTrades,
  getSalesClientTransactions,
  getSalesDeposits,
  getSalesWithdrawals,
} from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import {
  AccountRoleTypes,
  DepositState,
  WithdrawalState,
  TransferState,
  CurrencyTypes,
  getCurrencyCode,
} from '@/types/accounts';
import { useCurrencyName } from '@/i18n/useCurrencyName';
import { TradeFilter } from '@/components/TradeFilter';
import type {
  SalesClientAccount,
  SalesTradeRecord,
  SalesTradeListResponse,
  SalesDepositRecord,
  SalesWithdrawalRecord,
  SalesTransactionRecord,
} from '@/types/sales';

type DetailTab = 'transaction' | 'deposit' | 'withdrawal' | 'tradeReport';

// ====================================================================
// 状态 → Tag variant 映射
// ====================================================================

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
  return new Date(dateStr).toLocaleString('sv-SE').replace('T', ' ');
}

function formatGroupKey(dateStr: string) {
  const d = new Date(dateStr);
  const monthYear = d.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  const weekday = d.toLocaleDateString('en-US', { weekday: 'long' });
  return `${monthYear}||${weekday}`;
}

function handleTradeFormatted(price: number | undefined | null, digits: number): string {
  if (price == null || isNaN(price)) return '--';
  return parseFloat(price.toFixed(digits)).toString();
}

function getRoleLabel(
  role: number,
  td: (key: string) => string,
): string {
  if (role === AccountRoleTypes.IB) return td('roleIB');
  if (role === AccountRoleTypes.Sales) return td('roleSales');
  return td('roleClient');
}

export default function SalesCustomerDetailPage({
  params,
}: {
  params: Promise<{ accountId: string }>;
}) {
  const { accountId } = use(params);
  const accountUid = parseInt(accountId, 10);
  const router = useRouter();
  const t = useTranslations('sales');
  const td = useTranslations('sales.customerDetail');
  const tState = useTranslations('accounts.transactionState');
  const getCurrencyName = useCurrencyName();
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [accountDetail, setAccountDetail] = useState<SalesClientAccount | null>(null);
  const [tab, setTab] = useState<DetailTab>('transaction');
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 15;

  const [deposits, setDeposits] = useState<SalesDepositRecord[]>([]);
  const [withdrawals, setWithdrawals] = useState<SalesWithdrawalRecord[]>([]);
  const [transactions, setTransactions] = useState<SalesTransactionRecord[]>([]);
  const [trades, setTrades] = useState<SalesTradeRecord[]>([]);
  const [tradeCriteria, setTradeCriteria] = useState<SalesTradeListResponse['criteria'] | null>(null);

  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined);
  const tradeFilterParamsRef = useRef<Record<string, unknown>>({ isClosed: false });

  // ---- 加载账户详情（使用 queryAccounts 查询参数 uid=xxx） ----
  useEffect(() => {
    if (!salesAccount || !accountUid) return;
    (async () => {
      try {
        const result = await execute(getSalesClients, salesAccount.uid, { uid: accountUid });
        if (result.success && result.data?.data?.length) {
          setAccountDetail(result.data.data[0]);
        } else {
          router.push('/sales/customers');
        }
      } catch {
        router.push('/sales/customers');
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [salesAccount, accountUid]);

  // ---- Tabs 动态生成 ----
  const tabs: TabItem<DetailTab>[] = useMemo(() => {
    const list: TabItem<DetailTab>[] = [
      { key: 'transaction', label: td('tabs.transaction') },
      { key: 'deposit', label: td('tabs.deposit') },
      { key: 'withdrawal', label: td('tabs.withdrawal') },
    ];
    if (accountDetail?.tradeAccount?.accountNumber && accountDetail.tradeAccount.accountNumber !== 0) {
      list.push({ key: 'tradeReport', label: td('tabs.tradeReport') });
    }
    return list;
  }, [td, accountDetail]);

  // ---- 日期参数构建 ----
  const buildDateParams = useCallback(() => {
    const p: Record<string, unknown> = {};
    if (dateRange?.from) p.from = dateRange.from.toISOString();
    if (dateRange?.to) p.to = dateRange.to.toISOString();
    return p;
  }, [dateRange]);

  // ---- 数据加载 ----
  const loadData = useCallback(async (p: number) => {
    if (!salesAccount || !accountUid) return;
    setIsLoading(true);
    try {
      const dateParams = buildDateParams();

      if (tab === 'transaction') {
        const result = await execute(getSalesClientTransactions, salesAccount.uid, accountUid, {
          page: p, size: pageSize, ...dateParams,
        });
        if (result.success && result.data) {
          const d = result.data as { data?: SalesTransactionRecord[]; criteria?: { total?: number } };
          setTransactions(Array.isArray(d.data) ? d.data : []);
          setTotal(d.criteria?.total || 0);
        }
      } else if (tab === 'deposit') {
        const result = await execute(getSalesDeposits, salesAccount.uid, {
          page: p, size: pageSize, accountUid, ...dateParams,
        });
        if (result.success && result.data) {
          setDeposits(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'withdrawal') {
        const result = await execute(getSalesWithdrawals, salesAccount.uid, {
          page: p, size: pageSize, accountUid, ...dateParams,
        });
        if (result.success && result.data) {
          setWithdrawals(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'tradeReport') {
        const result = await execute(getSalesClientTrades, salesAccount.uid, accountUid, {
          page: p, size: pageSize, ...tradeFilterParamsRef.current,
        });
        if (result.success && result.data) {
          setTrades(Array.isArray(result.data.data) ? result.data.data : []);
          setTradeCriteria(result.data.criteria);
          setTotal(result.data.criteria?.total || 0);
        }
      }
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, accountUid, tab, execute, buildDateParams]);

  useEffect(() => {
    if (salesAccount) loadData(page);
  }, [tab, page, loadData, salesAccount]);

  const handleTabChange = (key: DetailTab) => { setTab(key); setPage(1); setTotal(0); };
  const handleSearch = () => { setPage(1); loadData(1); };
  const handleReset = () => { setDateRange(undefined); setPage(1); };
  const handleTradeFilterSearch = useCallback((params: Record<string, unknown>) => {
    tradeFilterParamsRef.current = params;
    setPage(1);
    loadData(1);
  }, [loadData]);
  const handleTradeFilterReset = useCallback(() => {
    tradeFilterParamsRef.current = { isClosed: false };
    setPage(1);
    loadData(1);
  }, [loadData]);

  // ---- 用户信息 ----
  const user = accountDetail?.user;
  const tradeAccount = accountDetail?.tradeAccount;
  const userName = user?.nativeName || user?.displayName || '--';
  const isIBOrSales = accountDetail?.role === AccountRoleTypes.IB || accountDetail?.role === AccountRoleTypes.Sales;
  const isClient = accountDetail?.role === AccountRoleTypes.Client;
  const currencyId = tradeAccount?.currencyId || CurrencyTypes.USD;

  // ---- Grouped DataTable 配置 ----
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

  const tradeGroupConfig = useMemo<DataTableGroupConfig<SalesTradeRecord>>(() => ({
    groupBy: (item) => formatGroupKey(item.openTime || item.openAt || ''),
    renderGroupHeader: groupHeaderRender,
    headerWidth: 'w-[120px]',
  }), [groupHeaderRender]);

  // ---- Transaction 列定义 ----
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
      render: (item) => <span className="text-sm">{formatDateTime(item.createdOn)}</span>,
    },
  ], [td, tState, getCurrencyName]);

  // ---- Deposit 列定义 ----
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
      render: (item) => <span className="text-sm">{formatDateTime(item.createdOn)}</span>,
    },
  ], [td, tState, getCurrencyName]);

  // ---- Withdrawal 列定义 ----
  const withdrawalColumns = useMemo<DataTableColumn<SalesWithdrawalRecord>[]>(() => [
    {
      key: 'withdrawal',
      title: td('columns.withdrawal'),
      skeletonWidth: 'w-28',
      render: (item) => (
        <div className="flex flex-col">
          <span className="flex items-center gap-1 text-sm">
            <span className="text-xs font-bold text-[#e02b1d]">↑</span>
            No.{item.accountNumber || item.targetTradeAccount?.accountNumber}
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
      key: 'exchangeRate',
      title: td('columns.exchangeRate'),
      align: 'center',
      skeletonWidth: 'w-12',
      render: (item) => <span className="text-sm">{(item as unknown as Record<string, unknown>).exchangeRate as string ?? '--'}</span>,
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

  // ---- Trade 列定义 ----
  const isClosed = tradeFilterParamsRef.current?.isClosed as boolean | undefined;

  const tradeColumns = useMemo<DataTableColumn<SalesTradeRecord>[]>(() => {
    const cols: DataTableColumn<SalesTradeRecord>[] = [
      { key: 'ticket', title: td('columns.ticket'), align: 'center', skeletonWidth: 'w-16', render: (item) => <span className="text-sm">{item.ticket}</span> },
      { key: 'symbol', title: td('columns.symbol'), align: 'center', skeletonWidth: 'w-16', render: (item) => <span className="text-sm">{item.symbol}</span> },
      { key: 'type', title: td('columns.type'), align: 'center', skeletonWidth: 'w-12', render: (item) => <span className="text-sm">{item.cmd === 0 ? 'Buy' : 'Sell'}</span> },
      { key: 'volume', title: td('columns.volume'), align: 'center', skeletonWidth: 'w-12', render: (item) => <span className="text-sm">{item.volume}</span> },
      { key: 'openTime', title: td('columns.openTime'), align: 'center', skeletonWidth: 'w-28', render: (item) => <span className="text-xs">{formatDateTime(item.openTime || item.openAt || '')}</span> },
      { key: 'openPrice', title: td('columns.openPrice'), align: 'center', skeletonWidth: 'w-16', render: (item) => <span className="text-sm">{handleTradeFormatted(item.openPrice, item.digits || 5)}</span> },
      { key: 'sl', title: td('columns.sl'), align: 'center', skeletonWidth: 'w-12', render: (item) => <span className="text-sm">{item.sl ?? '--'}</span> },
      { key: 'tp', title: td('columns.tp'), align: 'center', skeletonWidth: 'w-12', render: (item) => <span className="text-sm">{item.tp ?? '--'}</span> },
    ];

    if (isClosed) {
      cols.push(
        { key: 'closeTime', title: td('columns.closeTime'), align: 'center', skeletonWidth: 'w-28', render: (item) => <span className="text-xs">{formatDateTime(item.closeTime || item.closeAt || '')}</span> },
        { key: 'closePrice', title: td('columns.closePrice'), align: 'center', skeletonWidth: 'w-16', render: (item) => <span className="text-sm">{handleTradeFormatted(item.closePrice, item.digits || 5)}</span> },
      );
    }

    cols.push(
      { key: 'commission', title: td('columns.commission'), align: 'center', skeletonWidth: 'w-16', render: (item) => <span className="text-sm">{item.commission?.toFixed(2) ?? '--'}</span> },
      { key: 'swap', title: td('columns.swap'), align: 'center', skeletonWidth: 'w-12', render: (item) => <span className="text-sm">{(item.swap ?? item.swaps)?.toFixed(2) ?? '--'}</span> },
      {
        key: 'profit', title: td('columns.profit'), align: 'center', skeletonWidth: 'w-16',
        render: (item) => (
          <span className={
            (item.profit || 0) > 0 ? 'text-green-600' : (item.profit || 0) < 0 ? 'text-red-600' : ''
          }>
            {handleTradeFormatted(item.profit, 2)}
          </span>
        ),
      },
    );

    return cols;
  }, [td, isClosed]);

  // ---- 交易汇总行 ----
  const renderTradeSummary = () => {
    if (!tradeCriteria || tab !== 'tradeReport' || trades.length === 0) return null;
    const closedColSpan = isClosed ? 6 : 4;
    return (
      <div className="overflow-x-auto border-t border-border">
        <table className="w-full text-sm">
          <tbody>
            <tr className="border-b border-border">
              <td className="px-3 py-2 font-semibold text-text-primary">{td('columns.subTotal')}</td>
              <td colSpan={2} />
              <td className="px-3 py-2 text-center">{tradeCriteria.pageTotalVolume ?? 0}</td>
              <td colSpan={closedColSpan} />
              <td className="px-3 py-2 text-center">{tradeCriteria.pageTotalCommission ?? 0}</td>
              <td className="px-3 py-2 text-center">{tradeCriteria.pageTotalSwap ?? 0}</td>
              <td className={`px-3 py-2 text-center ${(tradeCriteria.pageTotalProfit || 0) > 0 ? 'text-green-600' : (tradeCriteria.pageTotalProfit || 0) < 0 ? 'text-red-600' : ''}`}>
                {tradeCriteria.pageTotalProfit ?? 0}
              </td>
            </tr>
            <tr className="bg-surface-secondary">
              <td className="px-3 py-2 font-semibold text-text-primary">{td('columns.total')}</td>
              <td colSpan={2} />
              <td className="px-3 py-2 text-center">{tradeCriteria.totalVolume ?? 0}</td>
              <td colSpan={closedColSpan} />
              <td className="px-3 py-2 text-center">{tradeCriteria.totalCommission ?? 0}</td>
              <td className="px-3 py-2 text-center">{tradeCriteria.totalSwap ?? 0}</td>
              <td className={`px-3 py-2 text-center ${(tradeCriteria.totalProfit || 0) > 0 ? 'text-green-600' : (tradeCriteria.totalProfit || 0) < 0 ? 'text-red-600' : ''}`}>
                {tradeCriteria.totalProfit ?? 0}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    );
  };

  // ---- 表格渲染 ----
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
      case 'tradeReport':
        return (
          <>
            <DataTable<SalesTradeRecord>
              columns={tradeColumns}
              data={trades}
              rowKey={(item, idx) => item.id ?? idx}
              loading={isLoading}
              skeletonRows={5}
              groupConfig={tradeGroupConfig}
            />
            {renderTradeSummary()}
          </>
        );
      default:
        return null;
    }
  };

  return (
    <div className="@container flex w-full flex-col gap-3">
      {/* 返回链接 */}
      <Link href="/sales/customers" className="flex items-center gap-3">
        <Icon name="arrow-left" size={24} className="text-text-secondary" />
        <span className="text-xl text-text-secondary">{td('backToList')}</span>
      </Link>

      <div className="h-px w-full bg-border" />

      {/* 用户信息 + 资金卡片 */}
      <div className="flex flex-col gap-3 @[1100px]:flex-row @[1100px]:gap-5">
        {/* 左侧：用户基本信息 */}
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
                    {isIBOrSales ? t('fields.accountUid') : t('fields.accountNo')}
                  </span>
                  <span className="text-sm font-semibold text-text-primary @[1100px]:text-base">
                    {isIBOrSales ? accountDetail.uid : (tradeAccount?.accountNumber || '--')}
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

        {/* 右侧：资金卡片（仅 Client 角色显示） */}
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

      {/* 主内容卡片 */}
      <div className="flex flex-1 flex-col gap-5 rounded bg-surface p-5">
        {/* Tabs + 筛选栏 */}
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
          )}
        </div>

        {/* Trade Report 专用筛选 */}
        {tab === 'tradeReport' && (
          <TradeFilter
            type="trade"
            translationNamespace="sales"
            filterOptions={['isClosed', 'product', 'datePicker', 'allHistory']}
            onSearch={handleTradeFilterSearch}
            onReset={handleTradeFilterReset}
            isLoading={isLoading}
          />
        )}

        {/* 结果数量 */}
        <p className="text-xl font-semibold text-text-secondary">
          {td.rich('showResults', {
            count: String(total),
            num: (chunks) => <span className="text-text-primary">{chunks}</span>,
          })}
        </p>

        {/* 表格内容 */}
        <div className="flex-1 overflow-x-auto">
          {renderTable()}
        </div>

        {/* 分页 */}
        <Pagination page={page} total={total} size={pageSize} onPageChange={setPage} />
      </div>
    </div>
  );
}
