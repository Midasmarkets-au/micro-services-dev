'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import Image from 'next/image';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useServerAction } from '@/hooks/useServerAction';
import { useUserStore } from '@/stores/userStore';
import { isGuestOnly } from '@/lib/rbac';
import { Button, Skeleton } from '@/components/ui';
import {
  getLiveAccounts,
  getPendingApplications,
  getDemoAccounts,
  getServiceMap,
} from '@/actions';
import type {
  Account,
  Application,
  DemoAccount,
  ServiceMap,
} from '@/types/accounts';
import {
  ApplicationStatusType,
  ApplicationType,
  AccountStatusTypes,
  AccountRoleTypes,
} from '@/types/accounts';
import { AccountTabs } from '@/components/dashboard/AccountTabs';
import { TradeAccountCard } from '@/components/dashboard/TradeAccountCard';
import { CreateLiveAccountModal } from '@/components/dashboard/modals/CreateLiveAccountModal';
import { CreateDemoAccountModal } from '@/components/dashboard/modals/CreateDemoAccountModal';
import { ResetPasswordModal } from '@/components/dashboard/modals/ResetPasswordModal';
import { ChangeLeverageModal } from '@/components/dashboard/modals/ChangeLeverageModal';
import { DepositModal } from '@/components/dashboard/modals/DepositModal';

type TabType = 'RealAccounts' | 'DemoAccounts';

export default function AccountPage() {
  const tAccounts = useTranslations('accounts');
  const router = useRouter();
  const { isDark, mounted } = useTheme();
  const { execute } = useServerAction({ showErrorToast: true });

  const user = useUserStore((state) => state.user);
  const isGuest = isGuestOnly(user?.roles ?? []);

  const [activeTab, setActiveTab] = useState<TabType>(isGuest ? 'DemoAccounts' : 'RealAccounts');
  const [isInitialLoading, setIsInitialLoading] = useState(!isGuest);
  const [liveAccounts, setLiveAccounts] = useState<Account[]>([]);
  const [pendingApplications, setPendingApplications] = useState<Application[]>([]);
  const [demoAccounts, setDemoAccounts] = useState<DemoAccount[]>([]);
  const [serviceMap, setServiceMap] = useState<ServiceMap>({});

  const [showCreateLiveModal, setShowCreateLiveModal] = useState(false);
  const [showCreateDemoModal, setShowCreateDemoModal] = useState(false);
  const [showResetPasswordModal, setShowResetPasswordModal] = useState(false);
  const [showChangeLeverageModal, setShowChangeLeverageModal] = useState(false);
  const [showDepositModal, setShowDepositModal] = useState(false);
  const [depositAccount, setDepositAccount] = useState<{ uid: number; currencyId: number } | null>(null);
  const [selectedAccount, setSelectedAccount] = useState<Account | null>(null);

  const isLoadedRef = useRef(false);

  const loadData = useCallback(async () => {
    try {
      const [accountsResult, applicationsResult, demoResult, serviceResult] =
        await Promise.all([
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
          execute(getPendingApplications, {
            statuses: [
              ApplicationStatusType.AwaitingApproval,
              ApplicationStatusType.Approved,
            ],
            type: ApplicationType.TradeAccount,
          }),
          execute(getDemoAccounts),
          execute(getServiceMap),
        ]);

      if (accountsResult.success) setLiveAccounts(accountsResult.data || []);
      if (applicationsResult.success) setPendingApplications(applicationsResult.data || []);
      if (demoResult.success) setDemoAccounts(demoResult.data || []);
      if (serviceResult.success) setServiceMap(serviceResult.data || {});
    } finally {
      setIsInitialLoading(false);
    }
  }, [execute]);

  useEffect(() => {
    if (isGuest) {
      setActiveTab('DemoAccounts');
      setIsInitialLoading(false);
    } else if (!isLoadedRef.current) {
      setIsInitialLoading(true);
      isLoadedRef.current = true;
      loadData();
    }
  }, [isGuest, loadData]);

  const handleRefresh = () => loadData();

  const handleCreateSuccess = () => {
    setShowCreateLiveModal(false);
    setShowCreateDemoModal(false);
    loadData();
  };

  const handleResetPassword = (account: Account) => {
    setSelectedAccount(account);
    setShowResetPasswordModal(true);
  };

  const handleDeposit = (account: Account) => {
    setDepositAccount({ uid: account.uid, currencyId: account.currencyId });
    setShowDepositModal(true);
  };

  const handleChangeLeverage = (account: Account) => {
    setSelectedAccount(account);
    setShowChangeLeverageModal(true);
  };

  const handleLeverageSuccess = () => {
    setShowChangeLeverageModal(false);
    setSelectedAccount(null);
    loadData();
  };

  const renderSkeleton = () => (
    <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
      {[1, 2].map((i) => (
        <div key={i} className="flex flex-col gap-5 rounded-xl border border-border bg-surface p-10">
          <div className="flex items-center justify-between">
            <div className="flex gap-2">
              <Skeleton className="h-6 w-16" />
              <Skeleton className="h-6 w-12" />
            </div>
            <Skeleton className="size-8" />
          </div>
          <div className="flex items-center gap-5">
            <Skeleton className="size-10 rounded-full" />
            <div className="flex flex-col gap-2">
              <Skeleton className="h-6 w-32" />
              <Skeleton className="h-4 w-24" />
            </div>
          </div>
          <Skeleton className="h-px w-full" />
          <div className="flex items-center justify-between">
            <Skeleton className="h-12 w-20" />
            <Skeleton className="h-12 w-20" />
            <Skeleton className="h-12 w-20" />
          </div>
          <div className="flex gap-5">
            <Skeleton className="h-9 flex-1" />
            <Skeleton className="h-9 flex-1" />
          </div>
        </div>
      ))}
    </div>
  );

  const renderNoData = () => (
    <div className="flex grow flex-col items-center justify-center rounded bg-surface py-20">
      <div className="flex flex-col items-center gap-4">
        <Image
          src={mounted && isDark ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
          alt="No data"
          width={120}
          height={120}
        />
        <p className="text-lg text-text-secondary">{tAccounts('noData')}</p>
      </div>
    </div>
  );

  return (
    <div className="flex w-full flex-col gap-6 rounded-xl bg-surface p-6">
      {/* Tab 栏 + 创建按钮 */}
      <div className="flex flex-col">
        <div className="flex flex-col items-stretch gap-3 md:flex-row md:items-start md:justify-between">
          <AccountTabs
            activeTab={activeTab}
            onTabChange={setActiveTab}
            disableRealAccounts={isGuest}
          />
          <div className="w-full md:w-auto">
            <Button
              
              className="w-full justify-center gap-1 md:h-auto md:w-auto md:px-2.5 md:py-1"
              onClick={() =>
                activeTab === 'RealAccounts'
                  ? setShowCreateLiveModal(true)
                  : setShowCreateDemoModal(true)
              }
            >
              <Image src="/images/icons/add-plain.svg" alt="add" width={20} height={20} />
              <span>
                {activeTab === 'RealAccounts'
                  ? tAccounts('action.createTradeAccount')
                  : tAccounts('action.createDemoAccount')}
              </span>
            </Button>
          </div>
        </div>
        <div className="mt-0 h-px w-full bg-border" />
      </div>

      {/* 账户列表 */}
      {isInitialLoading ? (
        renderSkeleton()
      ) : activeTab === 'RealAccounts' ? (
        liveAccounts.length === 0 && pendingApplications.length === 0 ? (
          renderNoData()
        ) : (
          <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
            {pendingApplications.map((app, index) => (
              <TradeAccountCard
                key={`app-${app.id ?? index}`}
                item={app}
                type="application"
                serviceMap={serviceMap}
                onRefresh={handleRefresh}
              />
            ))}
            {liveAccounts.map((account, index) => (
              <TradeAccountCard
                key={`account-${account.id ?? account.uid ?? index}`}
                item={account}
                type="account"
                serviceMap={serviceMap}
                onDeposit={() => handleDeposit(account)}
                onResetPassword={() => handleResetPassword(account)}
                onChangeLeverage={() => handleChangeLeverage(account)}
                onViewDetails={() => account.tradeAccount && router.push(`/account/${account.tradeAccount.accountNumber}`)}
                onRefresh={handleRefresh}
              />
            ))}
          </div>
        )
      ) : demoAccounts.length === 0 ? (
        renderNoData()
      ) : (
        <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
          {demoAccounts.map((demo, index) => (
            <TradeAccountCard
              key={`demo-${demo.id ?? demo.accountNumber ?? index}`}
              item={demo}
              type="demo"
              serviceMap={serviceMap}
              onRefresh={handleRefresh}
            />
          ))}
        </div>
      )}

      {/* 弹窗 */}
      <CreateLiveAccountModal
        open={showCreateLiveModal}
        onOpenChange={setShowCreateLiveModal}
        onSuccess={handleCreateSuccess}
        serviceMap={serviceMap}
      />
      <CreateDemoAccountModal
        open={showCreateDemoModal}
        onOpenChange={setShowCreateDemoModal}
        onSuccess={handleCreateSuccess}
        serviceMap={serviceMap}
      />
      {selectedAccount?.tradeAccount && (
        <>
          <ResetPasswordModal
            open={showResetPasswordModal}
            onOpenChange={(open) => {
              setShowResetPasswordModal(open);
              if (!open) setSelectedAccount(null);
            }}
            accountUid={selectedAccount.uid}
            accountNumber={selectedAccount.tradeAccount.accountNumber}
            platformName={serviceMap[selectedAccount.tradeAccount.serviceId]?.platformName || 'MT4/MT5'}
          />
          <ChangeLeverageModal
            open={showChangeLeverageModal}
            onOpenChange={(open) => {
              setShowChangeLeverageModal(open);
              if (!open) setSelectedAccount(null);
            }}
            onSuccess={handleLeverageSuccess}
            accountUid={selectedAccount.uid}
            accountNumber={selectedAccount.tradeAccount.accountNumber}
            currentLeverage={selectedAccount.tradeAccount.leverage}
          />
        </>
      )}
      <DepositModal
        open={showDepositModal}
        onOpenChange={(open) => {
          setShowDepositModal(open);
          if (!open) setDepositAccount(null);
        }}
        account={depositAccount}
      />
    </div>
  );
}
