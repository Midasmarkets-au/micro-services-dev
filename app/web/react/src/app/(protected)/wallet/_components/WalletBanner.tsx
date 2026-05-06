'use client';

import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { Button, BalanceShow, CurrencyCodeMap } from '@/components/ui';
import { useTheme } from '@/hooks/useTheme';

interface WalletBannerProps {
  balance: number;
  currencyId: number;
  onWithdraw: () => void;
  onTransfer: () => void;
  fractionDigits?: number;
}


export function WalletBanner({
  balance = 0,
  currencyId = 840,
  onWithdraw,
  onTransfer,
  fractionDigits,
}: WalletBannerProps) {
  const t = useTranslations('wallet');
  const { theme } = useTheme();
  const currencyName = CurrencyCodeMap[currencyId] || 'USD';

  // 根据主题选择图片
  const decorationImage =
    theme === 'dark'
      ? '/images/verification/verify-night.svg'
      : '/images/verification/verify-day.svg';

  return (
    <div data-testid="wallet-banner" className="relative overflow-hidden rounded h-[170px]">
      {/* 背景渐变 */}
      <div className="absolute inset-0 verification-banner-bg" />
      <div className="absolute inset-0 verification-banner-grid" />

      {/* 主内容区域 */}
      <div className="relative z-10 flex items-start h-full px-5 py-7">
        {/* 左侧内容 */}
        <div className="flex flex-col gap-5">
          {/* 货币信息 */}
          <div className="flex items-center gap-3">
            {/* 货币图标 */}
            <div className="relative w-[60px] h-[60px] overflow-hidden flex items-center justify-center" style={{ borderRadius: '145.833px', border: '2.917px solid #004EFF', background: '#F5F5F5' }}>
              <Image
                src={`/images/currency/${currencyId}.svg`}
                alt={currencyName}
                width={60}
                height={60}
              />
            </div>

            {/* 货币名称和余额 */}
            <div className="flex flex-col gap-2">
              <span className="text-xl font-semibold text-white">
                {currencyName}
              </span>
              <div className="flex items-center gap-1">
                <span className="text-sm text-text-secondary">{t('balance')}：</span>
                <BalanceShow
                  balance={balance ?? 0}
                  currencyId={currencyId}
                  className="text-xl font-bold text-white"
                  fractionDigits={fractionDigits}
                />
              </div>
            </div>
          </div>

          {/* 操作按钮 */}
          <div className="flex items-center gap-3 md:gap-5 w-full md:w-auto">
            <Button
              variant="secondary"
              size="sm"
              onClick={onWithdraw}
              className="flex-1 md:flex-none md:w-[200px] bg-[#2e2e2e] border-none text-white hover:bg-[#3a3a3a]"
            >
              {t('action.withdraw')}
            </Button>
            <Button
              variant="secondary"
              size="sm"
              onClick={onTransfer}
              className="flex-1 md:flex-none md:w-[200px] bg-[#2e2e2e] border-none text-white hover:bg-[#3a3a3a]"
            >
              {t('action.transfer')}
            </Button>
          </div>
        </div>

        {/* 右侧文字 - 移动端隐藏 */}
        <div className="absolute right-[200px] top-[30px] hidden lg:flex flex-col items-center gap-2 w-[350px]">
          <p
            className="text-2xl font-semibold text-center whitespace-pre-wrap bg-clip-text text-transparent"
            style={{
              backgroundImage: 'linear-gradient(90deg, #333 0%, #fff 50%, #333 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
            }}
          >
            {t('banner.slogan')}
          </p>
          <p className="text-sm font-light text-white text-center whitespace-pre-wrap leading-[1.4]">
            {t('banner.tagline')}
          </p>
        </div>

        {/* 右侧装饰图 - 移动端隐藏 */}
        <div className="absolute right-10 top-0 h-[170px] w-[191px] hidden md:block">
          <Image
            src={decorationImage}
            alt=""
            fill
            className="object-contain object-right"
            priority
          />
        </div>
      </div>
    </div>
  );
}
