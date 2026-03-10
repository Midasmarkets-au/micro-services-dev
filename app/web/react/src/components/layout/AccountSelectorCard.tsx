'use client';

import React from 'react';
import { Avatar, BalanceShow, SimpleSelect } from '@/components/ui';

function DecorativeChart() {
  return (
    <svg viewBox="0 0 200 60" className="h-14 w-full text-primary opacity-20" preserveAspectRatio="none">
      <path
        d="M0 50 C20 45 40 30 60 35 C80 40 100 15 120 20 C140 25 160 10 180 18 L200 15 L200 60 L0 60 Z"
        fill="currentColor"
      />
      <path
        d="M0 50 C20 45 40 30 60 35 C80 40 100 15 120 20 C140 25 160 10 180 18 L200 15"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        opacity="0.6"
      />
    </svg>
  );
}

export interface AccountOption {
  uid: number;
  label: string;
}

export interface AccountSelectorCardProps {
  avatar?: string;
  userName: string;
  balanceLabel: string;
  balanceInCents: number;
  currencyId: number;
  accountGroupLabel: string;
  accounts: AccountOption[];
  selectedUid?: number;
  onChangeAccount: (uid: number) => void;
  loading: boolean;
  noAccount: boolean;
  noAccountMessage: string;
  showDecorativeChart?: boolean;
  className?: string;
}

function SkeletonState({ showDecorativeChart }: { showDecorativeChart?: boolean }) {
  return (
    <div className="relative flex h-full flex-col items-center gap-5 overflow-hidden rounded-xl border border-border bg-surface px-5 py-6">
      <div className="size-20 animate-pulse rounded-full bg-border" />
      <div className="h-5 w-24 animate-pulse rounded bg-border" />
      <div className="h-px w-full bg-border" />
      <div className="flex w-full items-center justify-between gap-4">
        <div className="flex flex-1 flex-col items-center gap-1">
          <div className="h-4 w-10 animate-pulse rounded bg-border" />
          <div className="h-5 w-20 animate-pulse rounded bg-border" />
        </div>
        <div className="flex flex-1 flex-col items-center gap-1">
          <div className="h-4 w-10 animate-pulse rounded bg-border" />
          <div className="h-5 w-24 animate-pulse rounded bg-border" />
        </div>
      </div>
      {showDecorativeChart && (
        <div className="absolute inset-x-0 bottom-0">
          <DecorativeChart />
        </div>
      )}
    </div>
  );
}

export function AccountSelectorCard({
  avatar,
  userName,
  balanceLabel,
  balanceInCents,
  currencyId,
  accountGroupLabel,
  accounts,
  selectedUid,
  onChangeAccount,
  loading,
  noAccount,
  noAccountMessage,
  showDecorativeChart,
  className,
}: AccountSelectorCardProps) {
  if (noAccount) {
    return (
      <div className="flex h-full items-center justify-center rounded-xl border border-border bg-surface p-8">
        <p className="text-sm text-text-secondary">{noAccountMessage}</p>
      </div>
    );
  }

  if (loading) {
    return <SkeletonState showDecorativeChart={showDecorativeChart} />;
  }

  return (
    <div className={`relative flex h-full flex-col items-center overflow-hidden rounded-xl border border-border bg-surface px-5 py-6 ${className ?? ''}`}>
      <Avatar src={avatar} alt={userName} size="xl" />
      <h3 className="mt-3 text-lg font-semibold text-text-primary">{userName}</h3>

      <div className="mt-5 flex w-full items-start justify-between gap-4">
        <div className="flex flex-col gap-1">
          <span className="text-xs text-text-secondary">{balanceLabel}</span>
          <span className="text-base font-semibold text-text-primary">
            <BalanceShow balance={balanceInCents} currencyId={currencyId} />
          </span>
        </div>

        <div className="flex flex-col items-end gap-1">
          <span className="text-xs text-text-secondary">{accountGroupLabel}</span>
          {accounts.length > 1 ? (
            <SimpleSelect
              value={selectedUid != null ? String(selectedUid) : ''}
              onChange={(val) => onChangeAccount(Number(val))}
              options={accounts.map((acc) => ({ value: String(acc.uid), label: acc.label }))}
              triggerSize="sm"
              className="min-w-28 bg-input-bg"
            />
            
          ) : (
            <span className="text-sm font-medium text-text-primary">
              {accounts[0]?.label ?? '--'}
            </span>
          )}
        </div>
      </div>

      {showDecorativeChart && (
        <div className="absolute inset-x-0 bottom-0">
          <DecorativeChart />
        </div>
      )}
    </div>
  );
}
