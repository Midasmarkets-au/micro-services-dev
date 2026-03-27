'use client';

import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useServerAction } from '@/hooks/useServerAction';
import { TimeShow } from '@/components/TimeShow';
import { BalanceShow, Button, Skeleton, Tabs, DataTable, Pagination } from '@/components/ui';
import { TradeReportTable } from '@/components/TradeReportTable';
import type { TabItem, DataTableColumn, DataTableGroupConfig } from '@/components/ui';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/radix/Select';
import { TradeFilter } from '@/components/TradeFilter';
import type { TradeFilterType } from '@/components/TradeFilter';
import { DepositModal } from '@/components/dashboard/modals/DepositModal';
import { WithdrawalModal } from '@/components/dashboard/modals/WithdrawalModal';
import { TransferToAccountModal } from '@/components/dashboard/modals/TransferToAccountModal';
import { UploadReceiptModal } from '@/components/dashboard/modals/UploadReceiptModal';
import {
  getAccountByNumber,
  getLiveAccounts,
  getServiceMap,
  getAccountDeposits,
  getAccountWithdrawals,
  getAccountTransactions,
  getAccountTrades,
} from '@/actions';
import type {
  Account,
  ServiceMap,
  AccountDeposit,
  AccountWithdrawal,
  AccountTransaction,
} from '@/types/accounts';
import {
  AccountStatusTypes,
  AccountRoleTypes,
  CurrencyTypes,
  DepositState,
  WithdrawalState,
  TransferState,
  getCurrencyFlag,
  TransactionAccountType,
} from '@/types/accounts';
type DetailTab = 'deposit' | 'withdrawal' | 'transfer' | 'tradeReport';


const CopyIcon = () => (
  <svg width="14" height="14" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M13.3333 7.5V5.5C13.3333 4.09987 13.3333 3.3998 13.0608 2.86502C12.8212 2.39462 12.4387 2.01217 11.9683 1.77248C11.4335 1.5 10.7335 1.5 9.33333 1.5H5.5C4.09987 1.5 3.3998 1.5 2.86502 1.77248C2.39462 2.01217 2.01217 2.39462 1.77248 2.86502C1.5 3.3998 1.5 4.09987 1.5 5.5V9.33333C1.5 10.7335 1.5 11.4335 1.77248 11.9683C2.01217 12.4387 2.39462 12.8212 2.86502 13.0608C3.3998 13.3333 4.09987 13.3333 5.5 13.3333H7.5M10.6667 18.5H14.5C15.9001 18.5 16.6002 18.5 17.135 18.2275C17.6054 17.9878 17.9878 17.6054 18.2275 17.135C18.5 16.6002 18.5 15.9001 18.5 14.5V10.6667C18.5 9.26654 18.5 8.56647 18.2275 8.03169C17.9878 7.56129 17.6054 7.17883 17.135 6.93915C16.6002 6.66667 15.9001 6.66667 14.5 6.66667H10.6667C9.26654 6.66667 8.56647 6.66667 8.03169 6.93915C7.56129 7.17883 7.17883 7.56129 6.93915 8.03169C6.66667 8.56647 6.66667 9.26654 6.66667 10.6667V14.5C6.66667 15.9001 6.66667 16.6002 6.93915 17.135C7.17883 17.6054 7.56129 17.9878 8.03169 18.2275C8.56647 18.5 9.26654 18.5 10.6667 18.5Z"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

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

function getStateBadgeCls(stateId: number): string {
  if (COMPLETED_STATES.has(stateId)) return 'bg-[rgba(0,78,255,0.2)] text-[#004eff]';
  if (REJECTED_STATES.has(stateId)) return 'bg-[rgba(128,0,32,0.2)] text-[#800020]';
  if (PENDING_STATES.has(stateId)) return 'bg-[rgba(255,165,0,0.2)] text-[#ffa500]';
  return 'bg-gray-500/20 text-gray-500';
}

const DEFAULT_DEPOSIT_STATE_IDS = [350, 345];
const DEFAULT_WITHDRAWAL_STATE_IDS = [450];
const DEFAULT_TRANSACTION_STATE_IDS = [250];
const TAB_FIXED_FILTER_PARAMS: Partial<Record<DetailTab, Record<string, unknown>>> = {
  deposit: {
    sourceAccountType: TransactionAccountType.TradeAccount,
    targetAccountType: TransactionAccountType.TradeAccount,
    isClosed:false,
  },
  withdrawal: {
    sourceAccountType: TransactionAccountType.TradeAccount,
    targetAccountType: TransactionAccountType.TradeAccount,
    isClosed:false,
  },
  transfer: {
    sourceAccountType: TransactionAccountType.TradeAccount,
    targetAccountType: TransactionAccountType.TradeAccount,
    isClosed:false,
  },
};

function getPaginationTotal(payload: unknown, fallbackLength = 0): number {
  const p = payload as {
    criteria?: { total?: number };
    data?: unknown[] | { criteria?: { total?: number } };
  } | null;
  const nested = p?.data && !Array.isArray(p.data) ? p.data : undefined;
  return (
    p?.criteria?.total ??
    nested?.criteria?.total ??
    fallbackLength
  );
}

export default function AccountDetailPage() {
  const params = useParams();
  const accountNumber = Number(params.accountNumber);
  const t = useTranslations('accounts');
  const { isDark, mounted } = useTheme();
  const { execute } = useServerAction({ showErrorToast: true });

  const [isLoading, setIsLoading] = useState(true);
  const [currentAccount, setCurrentAccount] = useState<Account | null>(null);
  const [allAccounts, setAllAccounts] = useState<Account[]>([]);
  const [serviceMap, setServiceMap] = useState<ServiceMap>({});
  const [activeTab, setActiveTab] = useState<DetailTab>('deposit');
  const [copied, setCopied] = useState(false);

  // Tab data
  const [deposits, setDeposits] = useState<AccountDeposit[]>([]);
  const [withdrawals, setWithdrawals] = useState<AccountWithdrawal[]>([]);
  const [transactions, setTransactions] = useState<AccountTransaction[]>([]);
  const [tabLoading, setTabLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);

  // Modals
  const [showDepositModal, setShowDepositModal] = useState(false);
  const [showWithdrawalModal, setShowWithdrawalModal] = useState(false);
  const [showTransferModal, setShowTransferModal] = useState(false);
  const [receiptModal, setReceiptModal] = useState<{ open: boolean; hashId: string; methodName: string }>({
    open: false, hashId: '', methodName: '',
  });

  // Filters
  const [filterParams, setFilterParams] = useState<Record<string, unknown>>({
    stateIds: DEFAULT_DEPOSIT_STATE_IDS,
    size: 20,
    page:1
  });
  const isLoadedRef = useRef(false);
  const decorationImage = isDark
    ? '/images/verification/verify-night.svg'
    : '/images/verification/verify-day.svg';

  const loadAccountData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [accountResult, accountsResult, serviceResult] = await Promise.all([
        execute(getAccountByNumber, accountNumber),
        execute(getLiveAccounts, {
          hasTradeAccount: true,
          status: AccountStatusTypes.Activate,
          roles: [
            AccountRoleTypes.Client,
            AccountRoleTypes.SuperAdmin,
            AccountRoleTypes.TenantAdmin,
            AccountRoleTypes.Wholesale,
            AccountRoleTypes.Guest,
          ],
        }),
        execute(getServiceMap),
      ]);

      if (accountResult.success && accountResult.data) {
        setCurrentAccount(accountResult.data);
      }
      if (accountsResult.success) {
        setAllAccounts(accountsResult.data || []);
      }
      if (serviceResult.success) {
        setServiceMap(serviceResult.data || {});
      }
    } finally {
      setIsLoading(false);
    }
  }, [execute, accountNumber]);

  useEffect(() => {
    if (!isLoadedRef.current) {
      isLoadedRef.current = true;
      loadAccountData();
    }
  }, [loadAccountData]);

  const getDefaultFilterParams = useCallback((tab: DetailTab): Record<string, unknown> => {
    const fixedParams = TAB_FIXED_FILTER_PARAMS[tab] ?? {};
    if (tab === 'deposit') return { stateIds: DEFAULT_DEPOSIT_STATE_IDS, size: 20, ...fixedParams };
    if (tab === 'withdrawal') return { stateIds: DEFAULT_WITHDRAWAL_STATE_IDS, size: 20, ...fixedParams };
    if (tab === 'transfer') return { stateIds: DEFAULT_TRANSACTION_STATE_IDS, size: 20, ...fixedParams };
    return fixedParams;
  }, []);

  const getFilterType = useCallback((tab: DetailTab): TradeFilterType => {
    if (tab === 'deposit') return 'deposit';
    if (tab === 'withdrawal') return 'withdrawal';
    return 'transaction';
  }, []);

  const loadTabData = useCallback(async (extraParams?: Record<string, unknown>) => {
    if (!currentAccount || activeTab === 'tradeReport') return;
    setTabLoading(true);
    try {
      const uid = currentAccount.uid;
      const tabFixedParams = TAB_FIXED_FILTER_PARAMS[activeTab] ?? {};
      const params = { ...filterParams, ...extraParams, ...tabFixedParams };
      switch (activeTab) {
        case 'deposit': {
          const result = await execute(getAccountDeposits, uid, params);
          if (result.success) {
            const rows = result.data?.data || [];
            setDeposits(rows);
            setTotalCount(getPaginationTotal(result.data, rows.length));
          }
          break;
        }
        case 'withdrawal': {
          const result = await execute(getAccountWithdrawals, uid, params);
          if (result.success) {
            const rows = result.data?.data || [];
            setWithdrawals(rows);
            setTotalCount(getPaginationTotal(result.data, rows.length));
          }
          break;
        }
        case 'transfer': {
          const result = await execute(getAccountTransactions, uid, params);
          if (result.success) {
            const rows = result.data?.data || [];
            setTransactions(rows);
            setTotalCount(getPaginationTotal(result.data, rows.length));
          }
          break;
        }
      }
    } finally {
      setTabLoading(false);
    }
  }, [currentAccount, activeTab, execute, filterParams]);

  useEffect(() => {
    if (currentAccount) {
      loadTabData();
    }
  }, [currentAccount, activeTab, loadTabData]);

  const handleCopy = useCallback(() => {
    if (!currentAccount?.tradeAccount?.accountNumber) return;
    const text = String(currentAccount.tradeAccount.accountNumber);
    try {
      if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(text);
      } else {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);
      }
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // silently fail
    }
  }, [currentAccount]);

  const handleAccountSwitch = (value: string) => {
    const newAccountNumber = Number(value);
    if (newAccountNumber !== accountNumber) {
      window.location.href = `/account/${newAccountNumber}`;
    }
  };

  const handleTabChange = (tab: DetailTab) => {
    setActiveTab(tab);
    const defaults = getDefaultFilterParams(tab);
    setFilterParams(defaults);
    setTotalCount(0);
  };

  const handleSearch = (params: Record<string, unknown>) => {
    const tabFixedParams = TAB_FIXED_FILTER_PARAMS[activeTab] ?? {};
    const mergedParams = { ...params, page: 1, ...tabFixedParams };
    setFilterParams(mergedParams);
    loadTabData(mergedParams);
  };

  const handlePageChange = useCallback((nextPage: number) => {
    const tabFixedParams = TAB_FIXED_FILTER_PARAMS[activeTab] ?? {};
    const mergedParams = { ...filterParams, page: nextPage, ...tabFixedParams };
    setFilterParams(mergedParams);
    loadTabData(mergedParams);
  }, [activeTab, filterParams, loadTabData]);

  const fetchTradeData = useCallback(async (params: Record<string, unknown>) => {
    if (!currentAccount) return null;
    const result = await execute(getAccountTrades, currentAccount.uid, params);
    if (result.success && result.data) {
      return { data: result.data.data, criteria: result.data.criteria };
    }
    return null;
  }, [currentAccount, execute]);

  const tradeAccount = currentAccount?.tradeAccount;
  const service = tradeAccount ? serviceMap[tradeAccount.serviceId] : undefined;
  const serverName = service?.serverName || '--';
  const platformName = service?.platformName || '--';

  const tabs: TabItem<DetailTab>[] = [
    { key: 'deposit', label: t('detail.tabs.deposit') },
    { key: 'withdrawal', label: t('detail.tabs.withdrawal') },
    { key: 'transfer', label: t('detail.tabs.transfer') },
    { key: 'tradeReport', label: t('detail.tabs.tradeReport') },
  ];

  const groupHeaderRender = useCallback((groupKey: string) => {
    const [weekday, dayMonthYear] = groupKey.split('||');
    return (
      <div className="flex flex-col gap-2.5">
        <span className="text-xl font-bold text-text-primary font-['DIN',sans-serif]">{weekday}</span>
        <span className="text-sm text-text-secondary font-['DIN',sans-serif]">{dayMonthYear}</span>
      </div>
    );
  }, []);

  const dateGroupConfig = useMemo<DataTableGroupConfig<{ createdOn: string }>>(() => ({
    groupBy: (item) => {
      const d = new Date(item.createdOn);
      const weekday = d.toLocaleDateString('en-US', { weekday: 'long' });
      const dayMonthYear = d.toLocaleDateString('en-US', { day: 'numeric', month: 'short', year: 'numeric' });
      return `${weekday}||${dayMonthYear}`;
    },
    renderGroupHeader: groupHeaderRender,
    headerWidth: 'w-[140px]',
  }), [groupHeaderRender]);

  const transferDateGroupConfig = useMemo<DataTableGroupConfig<{ statedOn: string }>>(() => ({
    groupBy: (item) => {
      const d = new Date(item.statedOn);
      const weekday = d.toLocaleDateString('en-US', { weekday: 'long' });
      const dayMonthYear = d.toLocaleDateString('en-US', { day: 'numeric', month: 'short', year: 'numeric' });
      return `${weekday}||${dayMonthYear}`;
    },
    renderGroupHeader: groupHeaderRender,
    headerWidth: 'w-[140px]',
  }), [groupHeaderRender]);

  const depositColumns = useMemo<DataTableColumn<AccountDeposit>[]>(() => [
    {
      key: 'deposit',
      title: t('detail.tabs.deposit'),
      skeletonWidth: 'w-40',
      render: (item) => (
        <div className="flex items-center gap-2">
          <span className="text-[#004eff] text-xs font-bold">↓</span>
          <span className="text-sm text-text-primary">{item.paymentMethodName || '--'}</span>
          {item.stateId === DepositState.DepositCreated && (
            <button
              type="button"
              className="ml-2 inline-flex items-center gap-1 rounded bg-[#000f32] px-2.5 py-1 text-xs text-white transition-colors hover:bg-[#001a4d] cursor-pointer"
              onClick={() => setReceiptModal({ open: true, hashId: item.hashId, methodName: item.paymentMethodName })}
            >
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" />
                <polyline points="14,2 14,8 20,8" />
              </svg>
              {t('action.uploadReceipt')}
            </button>
          )}
        </div>
      ),
    },
    {
      key: 'status',
      title: t('detail.table.status'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => (
        <span className={`inline-flex items-center px-3 py-1 rounded text-xs font-normal ${getStateBadgeCls(item.stateId)}`}>
          {t(`transactionState.${item.stateId}`)}
        </span>
      ),
    },
    {
      key: 'payment',
      title: t('detail.table.payment'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => (
        <span className="text-sm text-text-secondary">
          {t(`paymentStatus.${item.paymentStatus}`)}
        </span>
      ),
    },
    {
      key: 'amount',
      title: t('detail.table.amount'),
      align: 'right',
      skeletonWidth: 'w-28',
      render: (item) => (
        <BalanceShow
          balance={item.amount}
          currencyId={item.currencyId || tradeAccount?.currencyId || CurrencyTypes.USD}
          className="text-base font-bold text-text-primary font-['DIN',sans-serif]"
        />
      ),
    },
    {
      key: 'time',
      title: t('detail.table.createdOn'),
      align: 'right',
      skeletonWidth: 'w-20',
      render: (item) => <TimeShow dateIsoString={item.createdOn} format="HH:mm A" />,
    },
  ], [t, tradeAccount?.currencyId]);

  const withdrawalColumns = useMemo<DataTableColumn<AccountWithdrawal>[]>(() => [
    {
      key: 'withdrawal',
      title: t('detail.tabs.withdrawal'),
      skeletonWidth: 'w-40',
      render: (item) => (
        <div className="flex items-center gap-2">
          <span className="text-[#e02b1d] text-xs font-bold">↑</span>
          <span className="text-sm text-text-primary">{item.paymentMethodName || '--'}</span>
        </div>
      ),
    },
    {
      key: 'status',
      title: t('detail.table.status'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => (
        <span className={`inline-flex items-center px-3 py-1 rounded text-xs font-normal ${getStateBadgeCls(item.stateId)}`}>
          {t(`transactionState.${item.stateId}`)}
        </span>
      ),
    },
    {
      key: 'payment',
      title: t('detail.table.payment'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => (
        <span className="text-sm text-text-secondary">
          {t(`paymentStatus.${item.paymentStatus}`)}
        </span>
      ),
    },
    {
      key: 'amount',
      title: t('detail.table.amount'),
      align: 'right',
      skeletonWidth: 'w-28',
      render: (item) => (
        <BalanceShow
          balance={item.amount}
          currencyId={item.currencyId || tradeAccount?.currencyId || CurrencyTypes.USD}
          className="text-base font-bold text-text-primary font-['DIN',sans-serif]"
        />
      ),
    },
    {
      key: 'createdOn',
      title: t('detail.table.createdOn'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => <TimeShow dateIsoString={item.createdOn} format="HH:mm A" />,
    },
    {
      key: 'updatedOn',
      title: t('detail.table.updatedOn'),
      align: 'right',
      skeletonWidth: 'w-20',
      render: (item) => <TimeShow dateIsoString={item.updatedOn} format="HH:mm A" />,
    },
  ], [t, tradeAccount?.currencyId]);

  const transferColumns = useMemo<DataTableColumn<AccountTransaction>[]>(() => [
    {
      key: 'transfer',
      title: t('detail.tabs.transfer'),
      skeletonWidth: 'w-40',
      render: (item) => (
        <div className="flex items-center gap-1 text-sm">
          <span className={tradeAccount?.accountNumber === item.sourceAccountNumber ? ' border-primary text-primary' : 'text-text-primary'}>
            NO.{item.sourceAccountNumber || 'Wallet'} 
          </span>
          <span className="text-primary mx-1">→</span>
          <span className={tradeAccount?.accountNumber === item.targetAccountNumber ? ' border-primary text-primary' : 'text-text-primary'}>
          NO.{item.targetAccountNumber || 'Wallet'}
          </span>
        </div>
      ),
    },
    {
      key: 'status',
      title: t('detail.table.status'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => (
        <span className={`inline-flex items-center px-3 py-1 rounded text-xs font-normal ${getStateBadgeCls(item.stateId)}`}>
          {t(`transactionState.${item.stateId}`)}
        </span>
      ),
    },
    {
      key: 'amount',
      title: t('detail.table.amount'),
      align: 'right',
      skeletonWidth: 'w-28',
      render: (item) => {
        const isIncoming = item.targetTradeAccountNumber === tradeAccount?.accountNumber;
        return (
          <div className="flex items-center justify-end gap-0.5">
            <span className={isIncoming ? 'text-green-500' : 'text-red-500'}>
              {isIncoming ? '+' : '-'}
            </span>
            <BalanceShow
              balance={item.amount}
              currencyId={tradeAccount?.currencyId || CurrencyTypes.USD}
              className="inline text-base font-bold text-text-primary font-['DIN',sans-serif]"
            />
          </div>
        );
      },
    },
    {
      key: 'time',
      title: t('detail.table.createdOn'),
      align: 'right',
      skeletonWidth: 'w-20',
      render: (item) => <TimeShow dateIsoString={item.statedOn} format="HH:mm A" />,
    },
  ], [t, tradeAccount?.accountNumber, tradeAccount?.currencyId]);

  if (isLoading) {
    return (
      <div className="flex w-full flex-col gap-6">
        <Skeleton className="h-6 w-40" />
        <Skeleton className="h-40 w-full rounded" />
        <div className="flex gap-6">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-8 w-20" />
          ))}
        </div>
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!currentAccount || !tradeAccount) {
    return (
      <div className="flex w-full flex-col items-center justify-center gap-6 py-20">
        <Image
          src={mounted && isDark ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
          alt="No data"
          width={120}
          height={120}
        />
        <p className="text-lg text-text-secondary">{t('noData')}</p>
        <Link href="/account" className="text-primary hover:underline">
          {t('detail.backToAccountList')}
        </Link>
      </div>
    );
  }

  return (
    <div className="flex w-full flex-col gap-6">
      {/* 返回链接 */}
      <Link
        href="/account"
        className="flex items-center gap-2 text-sm text-text-primary hover:text-primary"
      >
        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M10 12L6 8L10 4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
        {t('detail.backToAccountList')}
      </Link>

      {/* 账户信息 Banner - 白色容器包裹 */}
      <div className="rounded-xl bg-surface p-5">
        <div className="relative overflow-hidden rounded">
          {/* 背景渐变 */}
          <div className="absolute inset-0 verification-banner-bg" />
          <div className="absolute inset-0 verification-banner-grid" />

          {/* 右侧装饰图 */}
          {mounted && (
            <div className="absolute right-10 top-0 h-full w-[191px] hidden md:block">
              <Image
                src={decorationImage}
                alt=""
                fill
                className="object-contain object-right"
                priority
              />
            </div>
          )}

          {/* 内容 */}
          <div className="relative z-10 flex flex-col gap-4 p-6">
            {/* 上方：国旗 + 当前账户 + 账号 */}
            <div className="flex items-center gap-4">
              <div className="relative size-14 overflow-hidden rounded-full border-2 border-white/20">
                <Image
                  src={getCurrencyFlag(tradeAccount.currencyId)}
                  alt="currency"
                  fill
                  className="object-cover"
                />
              </div>
              <div className="flex flex-col gap-1">
                <span className="text-sm font-medium text-white/80">{t('detail.currentAccount')}</span>
                <div className="flex items-center gap-2">
                  {allAccounts.length > 1 ? (
                    <Select
                      value={String(tradeAccount.accountNumber)}
                      onValueChange={handleAccountSwitch}
                    >
                      <SelectTrigger className="h-auto! w-auto! border-none! bg-transparent! p-0! text-lg font-bold text-white! shadow-none! gap-1.5">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {allAccounts
                          .filter((a) => a.tradeAccount)
                          .map((a) => (
                            <SelectItem
                              key={a.tradeAccount!.accountNumber}
                              value={String(a.tradeAccount!.accountNumber)}
                            >
                              {a.tradeAccount!.accountNumber}
                            </SelectItem>
                          ))}
                      </SelectContent>
                    </Select>
                  ) : (
                    <span className="text-lg font-bold text-white">{tradeAccount.accountNumber}</span>
                  )}
                  <button
                    type="button"
                    onClick={handleCopy}
                    className="text-white/60 hover:text-white transition-colors cursor-pointer"
                    title={copied ? t('action.copied') : t('action.copy')}
                  >
                    {copied ? (
                      <svg width="14" height="14" viewBox="0 0 14 14" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M2 7L5.5 10.5L12 3.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                      </svg>
                    ) : (
                      <CopyIcon />
                    )}
                  </button>
                </div>
              </div>
            </div>

            {/* 下方：账户信息 2行 - web端只占半宽 */}
            <div className="grid grid-cols-3 gap-y-3 md:w-1/2 md:gap-x-8">
              {/* Row 1 */}
              <div className="flex flex-col gap-0.5">
                <span className="text-xs text-white/50">{t('fields.type')}</span>
                <span className="text-sm font-semibold text-white">{t(`accountTypes.${currentAccount.type}`)}</span>
              </div>
              <div className="flex flex-col gap-0.5">
                <span className="text-xs text-white/50">{t('fields.leverage')}</span>
                <span className="text-sm font-semibold text-white">{tradeAccount.leverage} : 1</span>
              </div>
              <div className="flex flex-col gap-0.5">
                <span className="text-xs text-white/50">{t('detail.balance')}</span>
                <BalanceShow
                  balance={tradeAccount.balanceInCents}
                  currencyId={tradeAccount.currencyId}
                  className="text-sm font-semibold text-white"
                />
              </div>
              {/* Row 2 */}
              <div className="flex flex-col gap-0.5">
                <span className="text-xs text-white/50">{t('detail.server')}</span>
                <span className="text-sm font-semibold text-white">{serverName}</span>
              </div>
              <div className="flex flex-col gap-0.5">
                <span className="text-xs text-white/50">{t('detail.platform')}</span>
                <span className="text-sm font-semibold text-white">{platformName}</span>
              </div>
              <div className="flex flex-col gap-0.5">
                <span className="text-xs text-white/50">{t('fields.credit')}</span>
                <BalanceShow
                  balance={tradeAccount.creditInCents}
                  currencyId={tradeAccount.currencyId}
                  className="text-sm font-semibold text-white"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tab + 筛选 + 内容 - 白色容器包裹 */}
      <div className="rounded-xl bg-surface p-6">

      {/* Tab 栏 */}
      <Tabs
        tabs={tabs}
        activeKey={activeTab}
        onChange={handleTabChange}
        size="base"
      />

      {activeTab === 'tradeReport' ? (
       <div className="flex flex-col gap-5 mt-4">
        <TradeReportTable
            fetchData={fetchTradeData}
            filterOptions={['isClosed', 'product', 'datePicker', 'allHistory']}
            autoFetchKey={currentAccount?.uid}
          />
        </div>
      ) : (
        <>
          {/* 筛选栏 */}
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-4 mt-4">
            <TradeFilter
              key={`trade-filter-${activeTab}`}
              type={getFilterType(activeTab)}
              filterOptions={['stateIds', 'datePicker', 'pageSize']}
              defaultPageSize={20}
              fixedParams={TAB_FIXED_FILTER_PARAMS[activeTab] ?? {}}
              onSearch={handleSearch}
              isLoading={tabLoading}
            />

            {/* 右侧快捷按钮 */}
            {activeTab === 'deposit' && (
              <Button variant="primary" size="sm" className="gap-1.5 shrink-0" onClick={() => setShowDepositModal(true)}>
                <Image src="/images/icons/add-plain.svg" alt="add" width={20} height={20} />
                {t('detail.tabs.deposit')}
              </Button>
            )}
            {activeTab === 'withdrawal' && tradeAccount && (
              <Button variant="primary" size="sm" className="gap-1.5 shrink-0" onClick={() => setShowWithdrawalModal(true)}>
                 <Image src="/images/icons/add-plain.svg" alt="add" width={20} height={20} />
                {t('detail.tabs.withdrawal')}
              </Button>
            )}
            {activeTab === 'transfer' && currentAccount && (
              <Button variant="primary" size="sm" className="gap-1.5 shrink-0" onClick={() => setShowTransferModal(true)}>
                <Image src="/images/icons/add-plain.svg" alt="add" width={20} height={20} />
                {t('detail.tabs.transfer')}
              </Button>
            )}
          </div>

          {/* 结果计数 */}
          <p className="text-sm text-text-secondary">
            {t('detail.table.showingResults', { count: totalCount })}
          </p>

          {/* Tab 内容 */}
          <div className="overflow-x-auto">
            {activeTab === 'deposit' && (
              <DataTable<AccountDeposit>
                columns={depositColumns}
                data={deposits}
                rowKey={(item, idx) => item.id ?? idx}
                loading={tabLoading}
                skeletonRows={3}
                groupConfig={dateGroupConfig as DataTableGroupConfig<AccountDeposit>}
              />
            )}
            {activeTab === 'withdrawal' && (
              <DataTable<AccountWithdrawal>
                columns={withdrawalColumns}
                data={withdrawals}
                rowKey={(item, idx) => item.id ?? idx}
                loading={tabLoading}
                skeletonRows={3}
                groupConfig={dateGroupConfig as DataTableGroupConfig<AccountWithdrawal>}
              />
            )}
            {activeTab === 'transfer' && (
              <DataTable<AccountTransaction>
                columns={transferColumns}
                data={transactions}
                rowKey={(item, idx) => item.id ?? idx}
                loading={tabLoading}
                skeletonRows={3}
                groupConfig={transferDateGroupConfig as DataTableGroupConfig<AccountTransaction>}
              />
            )}
          </div>
          <Pagination
            page={Number(filterParams.page) || 1}
            total={totalCount}
            size={Number(filterParams.size) || 20}
            onPageChange={handlePageChange}
          />
        </>
      )}

      </div>
      {/* end: Tab + 筛选 + 内容 白色容器 */}

      {/* Modals */}
      <DepositModal
        open={showDepositModal}
        onOpenChange={(open) => {
          setShowDepositModal(open);
          if (!open) loadTabData();
        }}
        account={currentAccount ? { uid: currentAccount.uid, currencyId: currentAccount.currencyId } : null}
      />

      {tradeAccount && (
        <WithdrawalModal
          open={showWithdrawalModal}
          onOpenChange={setShowWithdrawalModal}
          wallet={tradeAccount}
          type='account'
          accountNumber={tradeAccount.accountNumber}
          onSuccess={loadTabData}
        />
      )}

      {currentAccount && (
        <TransferToAccountModal
          open={showTransferModal}
          onOpenChange={setShowTransferModal}
          sourceAccount={currentAccount}
          allAccounts={allAccounts}
          onSuccess={loadTabData}
        />
      )}

      <UploadReceiptModal
        open={receiptModal.open}
        onOpenChange={(open) => setReceiptModal((prev) => ({ ...prev, open }))}
        accountUid={currentAccount.uid}
        depositHashId={receiptModal.hashId}
        paymentMethodName={receiptModal.methodName}
        onSuccess={loadTabData}
      />
    </div>
  );
}


