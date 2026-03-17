'use client';

import { useState, useCallback, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import {
  Button,
  Input,
  Checkbox,
  BalanceShow,
  CurrencyCodeMap,
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
  Tabs,
} from '@/components/ui';
import {
  getWalletList,
  getTradeAccounts,
  searchTransferTarget,
  sendTransferVerificationCode,
  transferToTradeAccount,
  transferToWallet,
} from '@/actions';
import type {
  Wallet,
  TradeAccount,
  TransferTargetResult,
} from '@/types/wallet';

interface TransferModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  wallet: Wallet;
  onSuccess?: () => void;
}

type TransferTab = 'self' | 'others';

export function TransferModal({
  open,
  onOpenChange,
  wallet,
  onSuccess,
}: TransferModalProps) {
  const t = useTranslations('wallet');
  const { showSuccess, showError } = useToast();
  const { execute } = useServerAction({ showErrorToast: true });

  const [activeTab, setActiveTab] = useState<TransferTab>('self');
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const [wallets, setWallets] = useState<Wallet[]>([]);
  const [selectedWallet, setSelectedWallet] = useState<Wallet>(wallet);
  const [tradeAccounts, setTradeAccounts] = useState<TradeAccount[]>([]);
  const [selectedAccountUid, setSelectedAccountUid] = useState<number | null>(null);
  const [amount, setAmount] = useState('');

  const [searchEmail, setSearchEmail] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  const [searchResult, setSearchResult] = useState<TransferTargetResult | null>(null);
  const [searchError, setSearchError] = useState('');

  const [verificationCode, setVerificationCode] = useState('');
  const [countdown, setCountdown] = useState(0);
  const countdownRef = useRef<NodeJS.Timeout | null>(null);

  const [confirmed, setConfirmed] = useState(false);

  const reset = useCallback(() => {
    setActiveTab('self');
    setIsLoading(false);
    setIsSubmitting(false);
    setIsSuccess(false);
    setWallets([]);
    setSelectedWallet(wallet);
    setTradeAccounts([]);
    setSelectedAccountUid(null);
    setAmount('');
    setSearchEmail('');
    setIsSearching(false);
    setSearchResult(null);
    setSearchError('');
    setVerificationCode('');
    setCountdown(0);
    setConfirmed(false);
    if (countdownRef.current) clearInterval(countdownRef.current);
  }, [wallet]);

  const loadInitialData = useCallback(async () => {
    setIsLoading(true);
    const [walletsResult, accountsResult] = await Promise.all([
      execute(getWalletList),
      execute(getTradeAccounts),
    ]);
    if (walletsResult.success && walletsResult.data) {
      const primaryWallets = walletsResult.data.filter((w: Wallet) => w.isPrimary);
      setWallets(primaryWallets);
      const current = primaryWallets.find((w: Wallet) => w.hashId === wallet.hashId);
      if (current) setSelectedWallet(current);
    }
    if (accountsResult.success && accountsResult.data) {
      setTradeAccounts(accountsResult.data);
    }
    setIsLoading(false);
  }, [execute, wallet.hashId]);

  useEffect(() => {
    if (!open) return;
    const init = async () => {
      reset();
      await loadInitialData();
    };
    init();
  }, [open, reset, loadInitialData]);

  useEffect(() => {
    return () => {
      if (countdownRef.current) clearInterval(countdownRef.current);
    };
  }, []);

  const handleEmailBlur = async () => {
    const email = searchEmail.trim();
    setSearchError('');

    if (!email) {
      setSearchResult(null);
      return;
    }

    setIsSearching(true);
    setSearchResult(null);

    const result = await execute(searchTransferTarget, email);
    if (result.success && result.data) {
      setSearchResult(result.data);
    } else {
      setSearchError(result.error || t('transfer.noSearchResults'));
    }
    setIsSearching(false);
  };

  const handleEmailInput = (val: string) => {
    setSearchEmail(val);
    setSearchError('');
    if (searchResult) setSearchResult(null);
  };

  const handleSendCode = async () => {
    const authType = activeTab === 'self' ? 'WalletToTradeAccount' : 'WalletToWalletTransfer';
    const result = await execute(sendTransferVerificationCode, authType);
    if (result.success && result.data) {
      const expiresIn = result.data.expiresIn || 60;
      setCountdown(expiresIn);
      if (countdownRef.current) clearInterval(countdownRef.current);
      countdownRef.current = setInterval(() => {
        setCountdown((prev) => {
          if (prev <= 1) {
            if (countdownRef.current) clearInterval(countdownRef.current);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
  };

  const handleSubmit = async () => {
    const numAmount = parseFloat(amount);
    if (!numAmount || numAmount <= 0) {
      showError(t('transfer.enterAmount'));
      return;
    }
    const amountInCents = numAmount * 100;
    if (amountInCents > selectedWallet.balance) {
      showError(t('transfer.insufficientBalance'));
      return;
    }
    if (!confirmed) {
      showError(t('transfer.confirmRequired'));
      return;
    }

    setIsSubmitting(true);

    if (activeTab === 'self') {
      if (!selectedAccountUid) {
        showError(t('transfer.selectAccount'));
        setIsSubmitting(false);
        return;
      }
      const result = await execute(transferToTradeAccount, {
        walletId: selectedWallet.id,
        tradeAccountUid: selectedAccountUid,
        amount: numAmount,
        verificationCode: verificationCode || undefined,
      });
      if (result.success) {
        setIsSuccess(true);
        showSuccess(
          numAmount > 3000 ? t('transfer.successSubmitTip') : t('transfer.toTradeAccountSuccess')
        );
        onSuccess?.();
      }
    } else {
      if (!searchResult) {
        showError(t('transfer.searchTargetFirst'));
        setIsSubmitting(false);
        return;
      }
      const result = await execute(transferToWallet, {
        walletId: selectedWallet.id,
        tradeAccountUid: searchResult.walletId,
        amount: numAmount,
        verificationCode: verificationCode || undefined,
      });
      if (result.success) {
        setIsSuccess(true);
        showSuccess(
          numAmount > 3000 ? t('transfer.successSubmitTip') : t('transfer.toWalletSuccess')
        );
        onSuccess?.();
      }
    }
    setIsSubmitting(false);
  };

  const currentTargetAccount = activeTab === 'self'
    ? tradeAccounts.find((acc) => acc.uid === selectedAccountUid)
    : null;

  const showUscTips = (() => {
    const numAmount = parseFloat(amount);
    if (!numAmount || isNaN(numAmount)) return false;
    if (activeTab === 'self' && currentTargetAccount) {
      return currentTargetAccount.currencyId === 841;
    }
    if (activeTab === 'others' && searchResult) {
      return searchResult.currencyId === 841;
    }
    return false;
  })();

  const uscTipsAmount = showUscTips ? parseFloat(amount) * 10000 : 0;
  const uscTipsCurrencyId = activeTab === 'self' && currentTargetAccount
    ? currentTargetAccount.currencyId
    : (searchResult?.currencyId ?? 841);

  const handleCancel = () => {
    onOpenChange(false);
  };

  if (isSuccess) {
    return (
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent>
          <DialogTitle className="sr-only">{t('transfer.successTitle')}</DialogTitle>
          <div className="flex flex-col items-center gap-4 py-6">
            <div className="w-16 h-16 rounded-full bg-[rgba(0,78,255,0.1)] flex items-center justify-center">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none">
                <path d="M20 6L9 17l-5-5" stroke="#004eff" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
            </div>
            <p className="text-lg font-medium text-text-primary">{t('transfer.successTitle')}</p>
            <p className="text-sm text-text-secondary text-center">{t('transfer.successDesc')}</p>
          </div>
          <Button variant="primary" className="w-full mt-4" onClick={() => onOpenChange(false)}>
            {t('action.close')}
          </Button>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogTitle className="text-xl font-bold text-text-primary mb-8">
          {t('transfer.title')}
        </DialogTitle>
        <DialogDescription className="sr-only">{t('transfer.desc')}</DialogDescription>

          {/* Tab 选择器 */}
          <div className="mb-8">
            <Tabs
              tabs={[
                { key: 'self' as const, label: t('transfer.selfTab') },
                { key: 'others' as const, label: t('transfer.othersTab') },
              ]}
              activeKey={activeTab}
              onChange={(key) => {
                if (key === 'self') { setActiveTab('self'); setSearchResult(null); setSearchEmail(''); }
                else { setActiveTab('others'); setSelectedAccountUid(null); }
              }}
              size="sm"
            />
          </div>

          {/* 钱包 */}
          <div className="mb-6">
            {wallets.length > 1 ? (
              <Select
                value={selectedWallet.hashId}
                onValueChange={(val) => {
                  const w = wallets.find((item) => item.hashId === val);
                  if (w) setSelectedWallet(w);
                }}
              >
                <div className="w-full">
                  <label className="mb-2 flex items-center text-sm font-normal text-text-secondary">
                    <span className="mr-0.5 text-primary">*</span>
                    {t('transfer.walletLabel')}
                  </label>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder={t('transfer.selectWalletPlaceholder')} />
                  </SelectTrigger>
                </div>
                <SelectContent>
                  {wallets.map((w) => (
                    <SelectItem key={w.hashId} value={w.hashId}>
                      {t('transfer.walletLabel')} – <BalanceShow balance={w.balance} currencyId={w.currencyId} />
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <Select value={selectedWallet.hashId}>
                <div className="w-full">
                  <label className="mb-2 flex items-center text-sm font-normal text-text-secondary">
                    <span className="mr-0.5 text-primary">*</span>
                    {t('transfer.walletLabel')}
                  </label>
                  <SelectTrigger className="w-full">
                    <SelectValue>
                      {t('transfer.walletLabel')} – <BalanceShow balance={selectedWallet.balance} currencyId={selectedWallet.currencyId} />
                    </SelectValue>
                  </SelectTrigger>
                </div>
                <SelectContent>
                  <SelectItem value={selectedWallet.hashId}>
                    {t('transfer.walletLabel')} – <BalanceShow balance={selectedWallet.balance} currencyId={selectedWallet.currencyId} />
                  </SelectItem>
                </SelectContent>
              </Select>
            )}
          </div>

          {/* 自己的账户 - 选择交易账户 */}
          {activeTab === 'self' && (
            <div className="mb-6">
              {isLoading ? (
                <div>
                  <label className="text-sm text-text-secondary mb-2 block">
                    <span className="text-[#800020]">*</span>{t('transfer.targetTradeAccount')}
                  </label>
                  <div className="h-10 bg-surface-secondary rounded animate-pulse" />
                </div>
              ) : tradeAccounts.length === 0 ? (
                <Input
                  label={t('transfer.targetTradeAccount')}
                  required
                  value=""
                  readOnly
                  placeholder={t('transfer.noAccounts')}
                />
              ) : (
                <Select
                  value={selectedAccountUid != null ? String(selectedAccountUid) : ''}
                  onValueChange={(val) => setSelectedAccountUid(val ? Number(val) : null)}
                >
                  <div className="w-full">
                    <label className="mb-2 flex items-center text-sm font-normal text-text-secondary">
                      <span className="mr-0.5 text-primary">*</span>
                      {t('transfer.targetTradeAccount')}
                    </label>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder={t('transfer.selectAccountPlaceholder')} />
                    </SelectTrigger>
                  </div>
                  <SelectContent>
                    {tradeAccounts.map((acc) => {
                      const code = CurrencyCodeMap[acc.currencyId] || 'USD';
                      return (
                        <SelectItem key={acc.uid} value={String(acc.uid)}>
                          {acc.accountNumber}({code}) <BalanceShow balance={acc.balanceInCents} currencyId={acc.currencyId} />
                        </SelectItem>
                      );
                    })}
                  </SelectContent>
                </Select>
              )}
            </div>
          )}

          {/* 其他用户 - 邮箱搜索 */}
          {activeTab === 'others' && (
            <div className="mb-6">
              <Input
                label={t('transfer.targetEmail')}
                required
                value={searchEmail}
                onChange={(e) => handleEmailInput(e.target.value)}
                onBlur={handleEmailBlur}
                onKeyDown={(e) => { if (e.key === 'Enter') handleEmailBlur(); }}
                placeholder={t('transfer.emailPlaceholder')}
              />
              {isSearching && (
                <p className="text-xs text-text-secondary mt-1.5">{t('transfer.searching')}...</p>
              )}
              {searchError && (
                <p className="text-xs text-red-500 mt-1.5">{searchError}</p>
              )}
              {searchResult && (
                <div className="mt-2 bg-surface-secondary rounded-lg p-3">
                  <p className="text-sm font-medium text-text-primary">{searchResult.name}</p>
                  <p className="text-xs text-text-secondary">{searchResult.email}</p>
                </div>
              )}
            </div>
          )}

          {/* 金额 */}
          <div className="mb-6">
            <Input
              label={t('transfer.amount')}
              required
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder={t('transfer.amountPlaceholder')}
            />
            {showUscTips && (
              <p className="text-xs text-primary mt-1.5">
                <BalanceShow balance={uscTipsAmount} currencyId={uscTipsCurrencyId} />
              </p>
            )}
            <p className="text-xs text-text-secondary mt-1.5">
              {t('transfer.availableBalance')}: <BalanceShow balance={selectedWallet.balance} currencyId={selectedWallet.currencyId} />
            </p>
          </div>

          {/* 验证码 */}
          <div className="mb-6">
            <div className="flex items-end gap-2">
              <div className="flex-1">
                <Input
                  label={t('transfer.verificationCode')}
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  placeholder={t('transfer.codePlaceholder')}
                />
              </div>
              <Button
                variant="outline"
                className="h-11 shrink-0"
                onClick={handleSendCode}
                disabled={countdown > 0}
              >
                {countdown > 0 ? `${countdown}s` : t('transfer.sendCode')}
              </Button>
            </div>
          </div>

          {/* 确认勾选 */}
          <div className="mb-8">
            <Checkbox
              variant="radio"
              checked={confirmed}
              onCheckedChange={(checked) => setConfirmed(checked === true)}
              label={t('transfer.confirmCheckbox')}
            />
          </div>

          {/* 底部按钮 */}
          <div className="flex justify-end gap-3">
            <Button
              variant="outline"
              onClick={handleCancel}
              className="min-w-[100px]"
            >
              {t('action.cancel')}
            </Button>
            <Button
              variant="primary"
              onClick={handleSubmit}
              loading={isSubmitting}
              disabled={!confirmed}
              className="min-w-[100px]"
            >
              {t('action.submit')}
            </Button>
          </div>
      </DialogContent>
    </Dialog>
  );
}
