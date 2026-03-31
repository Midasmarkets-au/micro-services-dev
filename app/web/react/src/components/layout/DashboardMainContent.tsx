'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import Image from 'next/image';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useServerAction } from '@/hooks/useServerAction';
import { useUserStore } from '@/stores/userStore';
import { isGuestOnly } from '@/lib/rbac';
import { Button, Skeleton, Icon } from '@/components/ui';
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

export function DashboardMainContent() {
  const t = useTranslations('dashboard');
  const tAccounts = useTranslations('accounts');
  const router = useRouter();
  const { isDark, mounted } = useTheme();
  const { execute } = useServerAction({ showErrorToast: true });
  
  // 获取用户信息判断是否为 Guest
  const user = useUserStore((state) => state.user);
  const isGuest = isGuestOnly(user?.roles ?? []);

  // 账户状态 - Guest 用户默认显示模拟账户
  const [activeTab, setActiveTab] = useState<TabType>(isGuest ? 'DemoAccounts' : 'RealAccounts');
  const [isInitialLoading, setIsInitialLoading] = useState(!isGuest); // Guest 用户不需要加载状态
  const [liveAccounts, setLiveAccounts] = useState<Account[]>([]);
  const [pendingApplications, setPendingApplications] = useState<Application[]>([]);
  const [demoAccounts, setDemoAccounts] = useState<DemoAccount[]>([]);
  const [serviceMap, setServiceMap] = useState<ServiceMap>({});

  // 弹窗状态
  const [showCreateLiveModal, setShowCreateLiveModal] = useState(false);
  const [showCreateDemoModal, setShowCreateDemoModal] = useState(false);
  const [showResetPasswordModal, setShowResetPasswordModal] = useState(false);
  const [showChangeLeverageModal, setShowChangeLeverageModal] = useState(false);
  const [showDepositModal, setShowDepositModal] = useState(false);
  const [depositAccount, setDepositAccount] = useState<{ uid: number; currencyId: number } | null>(null);
  
  // 选中的账户（用于弹窗）
  const [selectedAccount, setSelectedAccount] = useState<Account | null>(null);

  // 防止 Strict Mode 双重调用
  const isLoadedRef = useRef(false);

  // 根据主题选择图片
  const bannerImage = isDark
    ? '/images/dashboard/banner-night.svg'
    : '/images/dashboard/banner-day.svg';

  // 加载数据
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
      if (accountsResult.success) {
        setLiveAccounts(accountsResult.data || []);
      }
      if (applicationsResult.success) {
        setPendingApplications(applicationsResult.data || []);
      }
      if (demoResult.success) {
        setDemoAccounts(demoResult.data || []);
      }
      if (serviceResult.success) {
        setServiceMap(serviceResult.data || {});
      }
    } finally {
      setIsInitialLoading(false);
    }
  }, [execute]);

  // Guest 用户状态变化时，同步 Tab 和加载状态
  useEffect(() => {
    if (isGuest) {
      setActiveTab('DemoAccounts');
      setIsInitialLoading(false);
    } else {
      // 非 Guest 用户，如果之前未加载过数据则加载
      if (!isLoadedRef.current) {
        setIsInitialLoading(true);
        isLoadedRef.current = true;
        loadData();
      }
    }
  }, [isGuest, loadData]);

  // 刷新数据
  const handleRefresh = () => {
    loadData();
  };

  // 处理创建账户成功
  const handleCreateSuccess = () => {
    setShowCreateLiveModal(false);
    setShowCreateDemoModal(false);
    loadData();
  };

  // 处理重置密码
  const handleResetPassword = (account: Account) => {
    setSelectedAccount(account);
    setShowResetPasswordModal(true);
  };

  // 处理入金
  const handleDeposit = (account: Account) => {
    setDepositAccount({ uid: account.uid, currencyId: account.currencyId });
    setShowDepositModal(true);
  };

  // 处理修改杠杆
  const handleChangeLeverage = (account: Account) => {
    setSelectedAccount(account);
    setShowChangeLeverageModal(true);
  };

  // 处理杠杆修改成功
  const handleLeverageSuccess = () => {
    setShowChangeLeverageModal(false);
    setSelectedAccount(null);
    loadData();
  };

  return (
    <div className="main-content-responsive">
      {/* Banner - 高度 168px = 10.5rem，响应式缩放 */}
      <div className="dashboard-banner relative h-42 w-full overflow-hidden rounded">
        {/* 右侧装饰图片 - 根据主题切换 */}
        {mounted && (
          <div className="absolute right-0 top-1/2 h-[20.94rem] w-123 -translate-y-1/2">
            <Image
              src={bannerImage}
              alt="decoration"
              fill
              className="object-contain object-right"
            />
          </div>
        )}

        {/* 文字内容 */}
        <div className="absolute left-17.5 top-5.5 z-10 flex flex-col">
          <div className="text-responsive-2xl font-semibold text-white">
            <p>MIDAS MARKET</p>
            <p className="flex items-center">
              <span>{t('bannerTitle')}</span>
              <span className="banner-highlight-text ml-1">{t('bannerHighlight')}</span>
            </p>
          </div>

          {/* Deposit Now 按钮 */}
          <button className="mt-5 flex h-7.5 w-26.75 items-center justify-center gap-1 rounded border border-white bg-primary text-xs font-semibold text-white backdrop-blur-sm hover:bg-primary-hover">
            <span>{t('depositNow')}</span>
            <Icon name="add-plain" size={12} />
          </button>
        </div>
      </div>
      <div className="min-h-0 rounded bg-surface p-4">
        {/* 账户切换标签栏 */}
        <div className="flex flex-col mb-4">
          <div className="flex flex-col items-stretch gap-3 md:flex-row md:items-start md:justify-between">
            <AccountTabs 
              activeTab={activeTab} 
              onTabChange={setActiveTab}
              disableRealAccounts={isGuest}
            />

            {/* 右侧创建交易账户按钮 */}
            <div className="w-full md:w-auto">
              <Button
                className="w-full justify-center gap-1 md:h-auto md:w-auto md:px-2.5 md:py-1"
                onClick={() =>
                  activeTab === 'RealAccounts'
                    ? setShowCreateLiveModal(true)
                    : setShowCreateDemoModal(true)
                }
              >
                <Image
                  src="/images/icons/add-plain.svg"
                  alt="add"
                  width={20}
                  height={20}
                />
                <span>
                  {activeTab === 'RealAccounts'
                    ? tAccounts('action.createTradeAccount')
                    : tAccounts('action.createDemoAccount')}
                </span>
              </Button>
            </div>
          </div>

          {/* 分割线 */}
          <div className="mt-0 h-px w-full bg-border" />
        </div>

        {/* 账户列表内容 */}
        {isInitialLoading ? (
          // 骨架屏
          <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
            {[1, 2].map((i) => (
              <div
                key={i}
                className="flex flex-col gap-5 rounded-xl border border-border bg-surface p-10"
              >
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
        ) : activeTab === 'RealAccounts' ? (
          // 真实账户列表
          liveAccounts.length === 0 && pendingApplications.length === 0 ? (
            <div className="flex grow flex-col items-center justify-center rounded bg-surface py-20">
              <div className="flex flex-col items-center gap-4">
                <Image
                  src={mounted && isDark ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
                  alt="No data"
                  width={120}
                  height={120}
                />
                <p className="text-lg text-text-secondary">
                  {tAccounts('noData')}
                </p>
              </div>
            </div>
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
        ) : (
          // 模拟账户列表
          demoAccounts.length === 0 ? (
            <div className="flex grow flex-col items-center justify-center rounded bg-surface py-20">
              <div className="flex flex-col items-center gap-4">
                <Image
                  src={mounted && isDark ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
                  alt="No data"
                  width={120}
                  height={120}
                />
                <p className="text-lg text-text-secondary">
                  {tAccounts('noData')}
                </p>
              </div>
            </div>
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
          )
        )}
      </div>
      {/* 创建真实账户弹窗 */}
      <CreateLiveAccountModal
        open={showCreateLiveModal}
        onOpenChange={setShowCreateLiveModal}
        onSuccess={handleCreateSuccess}
        serviceMap={serviceMap}
      />

      {/* 创建模拟账户弹窗 */}
      <CreateDemoAccountModal
        open={showCreateDemoModal}
        onOpenChange={setShowCreateDemoModal}
        onSuccess={handleCreateSuccess}
        serviceMap={serviceMap}
      />

      {/* 重置密码弹窗 */}
      {selectedAccount && selectedAccount.tradeAccount && (
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
      )}

      {/* 修改杠杆弹窗 */}
      {selectedAccount && selectedAccount.tradeAccount && (
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
      )}

      {/* 入金弹窗 */}
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
