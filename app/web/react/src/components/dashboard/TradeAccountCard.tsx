'use client';

import { useState } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { Button, BalanceShow } from '@/components/ui';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/radix/DropdownMenu';
import type {
  Account,
  Application,
  DemoAccount,
  ServiceMap,
  CardType,
} from '@/types/accounts';
import {
  showWebTrader,
  getWebTraderLink,
  getCurrencyFlag,
  AccountTypes,
} from '@/types/accounts';
import { useTheme } from '@/hooks/useTheme';
interface TradeAccountCardProps {
  item: Account | Application | DemoAccount;
  type: CardType;
  serviceMap: ServiceMap;
  onResetPassword?: () => void;
  onChangeLeverage?: () => void;
  onDeposit?: () => void;
  onViewDetails?: () => void;
  onRefresh?: () => void;
}

// 复制图标
const CopyIcon = () => (
  <svg width="16" height="16" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M13.3333 7.5V5.5C13.3333 4.09987 13.3333 3.3998 13.0608 2.86502C12.8212 2.39462 12.4387 2.01217 11.9683 1.77248C11.4335 1.5 10.7335 1.5 9.33333 1.5H5.5C4.09987 1.5 3.3998 1.5 2.86502 1.77248C2.39462 2.01217 2.01217 2.39462 1.77248 2.86502C1.5 3.3998 1.5 4.09987 1.5 5.5V9.33333C1.5 10.7335 1.5 11.4335 1.77248 11.9683C2.01217 12.4387 2.39462 12.8212 2.86502 13.0608C3.3998 13.3333 4.09987 13.3333 5.5 13.3333H7.5M10.6667 18.5H14.5C15.9001 18.5 16.6002 18.5 17.135 18.2275C17.6054 17.9878 17.9878 17.6054 18.2275 17.135C18.5 16.6002 18.5 15.9001 18.5 14.5V10.6667C18.5 9.26654 18.5 8.56647 18.2275 8.03169C17.9878 7.56129 17.6054 7.17883 17.135 6.93915C16.6002 6.66667 15.9001 6.66667 14.5 6.66667H10.6667C9.26654 6.66667 8.56647 6.66667 8.03169 6.93915C7.56129 7.17883 7.17883 7.56129 6.93915 8.03169C6.66667 8.56647 6.66667 9.26654 6.66667 10.6667V14.5C6.66667 15.9001 6.66667 16.6002 6.93915 17.135C7.17883 17.6054 7.56129 17.9878 8.03169 18.2275C8.56647 18.5 9.26654 18.5 10.6667 18.5Z"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);
// 设置图标
const SettingsIcon = () => {
  const { isDark } = useTheme();
  const icon = isDark ? '/images/icons/setting-night.svg' : '/images/icons/setting-day.svg';
  return (
  <Button
    size="icon"
    className="rounded-full bg-surface-secondary border-border w-10 h-10 drop-shadow-[0_3.657px_6.857px_rgba(51,51,51,0.20)] text-text-primary"
  >
    <Image src={icon} alt="settings" width={17} height={17} />
  </Button>
)};


export function TradeAccountCard({
  item,
  type,
  serviceMap,
  onResetPassword,
  onChangeLeverage,
  onDeposit,
  onViewDetails,
}: TradeAccountCardProps) {
  const t = useTranslations('accounts');
  const [copied, setCopied] = useState(false);

  // 根据类型提取数据
  const getData = () => {
    if (type === 'account') {
      const account = item as Account;
      const tradeAccount = account.tradeAccount;
      if (!tradeAccount) {
        return {
          serviceId: 0,
          accountNumber: 0,
          currencyId: account.currencyId,
          leverage: 0,
          equityInCents: 0,
          creditInCents: 0,
          accountType: account.type,
          hasTradeAccount: account.hasTradeAccount,
        };
      }
      return {
        serviceId: tradeAccount.serviceId,
        accountNumber: tradeAccount.accountNumber,
        currencyId: tradeAccount.currencyId,
        leverage: tradeAccount.leverage,
        equityInCents: tradeAccount.equityInCents,
        creditInCents: tradeAccount.creditInCents,
        accountType: account.type,
        hasTradeAccount: account.hasTradeAccount,
      };
    }

    if (type === 'application') {
      const app = item as Application;
      return {
        serviceId: app.supplement.serviceId,
        accountNumber: 0,
        currencyId: app.supplement.currencyId,
        leverage: app.supplement.leverage,
        equityInCents: 0,
        creditInCents: 0,
        accountType: app.supplement.accountType,
        hasTradeAccount: false,
      };
    }

    // demo
    const demo = item as DemoAccount;
    return {
      serviceId: demo.serviceId,
      accountNumber: demo.accountNumber,
      currencyId: demo.currencyId,
      leverage: demo.leverage,
      equityInCents: demo.balanceInCents,
      creditInCents: 0,
      accountType: AccountTypes.Standard,
      hasTradeAccount: true,
    };
  };

  const data = getData();
  const service = serviceMap[data.serviceId];
  const serverName = service?.serverName || 'Unknown';
  const platformName = service?.platformName || 'MT5';

  // 复制账号
  const handleCopy = async () => {
    if (data.accountNumber) {
      await navigator.clipboard.writeText(String(data.accountNumber));
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  // 按钮配置
  const getButtonConfig = () => {
    if (type === 'account') {
      if (data.hasTradeAccount) {
        return {
          text: t('action.deposit'),
          handler: onDeposit,
          disabled: false,
        };
      }
      return {
        text: t('status.pendingTtd'),
        handler: undefined,
        disabled: true,
      };
    }

    if (type === 'application') {
      return {
        text: t('status.pending'),
        handler: undefined,
        disabled: true,
      };
    }

    return {
      text: t('title.demo'),
      handler: undefined,
      disabled: true,
    };
  };

  const buttonConfig = getButtonConfig();
  const canShowWebTrader = type !== 'demo' && data.hasTradeAccount && showWebTrader(data.serviceId);
  const webTraderLink = canShowWebTrader
    ? getWebTraderLink(data.serviceId, data.accountNumber)
    : '';

  return (
    <div className="flex flex-col gap-5 rounded-xl border border-border bg-surface p-6 overflow-hidden">
      {/* 顶部区域 */}
      <div className="flex flex-col gap-5">
        {/* 标签和设置 */}
        <div className="flex h-10 items-center justify-between">
          <div className="flex items-center gap-2.5">
            {/* 服务器名称标签 */}
            <span className="rounded border border-[#004eff] px-3 py-1 text-xs font-medium text-primary">
              {serverName}
            </span>
            {/* 平台标签 */}
            <span className="rounded border border-primary px-3 py-1 text-xs font-medium text-primary">
              {platformName}
            </span>
          </div>

          {/* 设置下拉菜单 */}
          {type === 'account' && data.hasTradeAccount && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <div className="cursor-pointer text-text-secondary hover:text-text-primary transition-colors">
                  <SettingsIcon />
                </div>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="min-w-[160px]"> 
                <DropdownMenuItem onClick={onResetPassword}>
                  {t('action.changePassword')}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={onChangeLeverage}>
                  {t('action.changeLeverage')}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={onViewDetails}>
                  {t('action.details')}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>

        {/* 余额信息 */}
        <div className="flex items-center gap-5">
          {/* 货币图标 */}
          <div className="relative size-10 rounded-full overflow-hidden bg-surface-secondary">
            <Image
              src={getCurrencyFlag(data.currencyId)}
              alt="currency"
              fill
              className="object-cover"
            />
          </div>

          {/* 净值和账号 */}
          <div className="flex flex-col gap-2">
            {type === 'application' ? (
              <span className="font-bold text-xl text-text-primary">
                {t('status.pending')}
              </span>
            ) : (
              <>
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-text-secondary">
                    {t('fields.equity')}：
                  </span>
                  <BalanceShow
                    balance={data.equityInCents}
                    currencyId={data.currencyId}
                    className="font-bold text-xl text-text-primary"
                  />
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-text-secondary">
                    {t('fields.accountNo')}：
                  </span>
                  {data.accountNumber > 0 ? (
                    <>
                      <span className="text-sm font-medium text-text-secondary">
                        {data.accountNumber}
                      </span>
                      <button
                        onClick={handleCopy}
                        className="text-text-secondary hover:text-primary transition-colors"
                        title={copied ? t('action.copied') : t('action.copy')}
                      >
                        <CopyIcon />
                      </button>
                    </>
                  ) : (
                    <span className="text-sm font-medium text-text-secondary">--</span>
                  )}
                </div>
              </>
            )}
          </div>
        </div>
      </div>

      {/* 分割线 */}
      <div className="h-px w-full bg-border" />

      {/* 中间信息区域 */}
      <div className="flex flex-col gap-5">
        {/* 类型、杠杆、信用 */}
        <div className="flex items-center justify-between">
          <div className="flex flex-1 flex-col items-center gap-1">
            <span className="text-sm font-medium text-text-secondary">
              {t('fields.type')}
            </span>
            <span className="text-base font-semibold text-text-primary">
              {type === 'application' ? t('status.pending') : t(`accountTypes.${data.accountType}`)}
            </span>
          </div>
          <div className="flex flex-1 flex-col items-center gap-1">
            <span className="text-sm font-medium text-text-secondary">
              {t('fields.leverage')}
            </span>
            <span className="font-bold text-base text-text-primary">
              {data.leverage}：1
            </span>
          </div>
          <div className="flex flex-1 flex-col items-center gap-1">
            <span className="text-sm font-medium text-text-secondary">
              {t('fields.credit')}
            </span>
            <BalanceShow
              balance={data.creditInCents}
              currencyId={data.currencyId}
              className="font-bold text-base text-text-primary"
            />
          </div>
        </div>

        {/* 底部按钮 */}
        <div className="flex items-center gap-3 lg:gap-5">
          {canShowWebTrader && (
            <Button
              variant="secondary"
              size="sm"
              className="flex-1 whitespace-nowrap"
              onClick={() => window.open(webTraderLink, '_blank')}
            >
              Web Trader
            </Button>
          )}
          <Button
            variant="secondary"
            size="sm"
            className="flex-1 whitespace-nowrap"
            disabled={buttonConfig.disabled}
            onClick={buttonConfig.handler}
          >
            {buttonConfig.text}
          </Button>
        </div>
      </div>
    </div>
  );
}
