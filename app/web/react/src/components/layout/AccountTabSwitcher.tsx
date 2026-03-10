'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { Icon } from '@/components/ui';
import { EditAliasModal, type EditAliasAccount } from './EditAliasModal';

interface AccountTabItem {
  uid: number;
  siteId?: number;
  alias?: string;
}

interface AccountTabSwitcherProps {
  accounts: AccountTabItem[];
  selectedUid?: number;
  defaultAccountUid?: number | string;
  onChangeAccount: (uid: number) => void;
  onUpdate: () => void;
}

export function AccountTabSwitcher({
  accounts,
  selectedUid,
  defaultAccountUid,
  onChangeAccount,
  onUpdate,
}: AccountTabSwitcherProps) {
  const t = useTranslations('common');
  const [editOpen, setEditOpen] = useState(false);

  if (accounts.length <= 1) return null;

  const getSiteLabel = (siteId?: number) => {
    if (siteId === undefined || siteId === null) return '';
    try {
      return t(`siteType.${siteId}`);
    } catch {
      return '';
    }
  };

  const formatLabel = (acc: AccountTabItem) => {
    const site = getSiteLabel(acc.siteId);
    return site ? `${acc.uid}(${site})` : String(acc.uid);
  };

  const editAccounts: EditAliasAccount[] = accounts.map((acc) => ({
    uid: acc.uid,
    siteId: acc.siteId,
    alias: acc.alias,
  }));

  return (
    <>
      <div className="flex items-center gap-2">
        <div className="flex items-center overflow-hidden rounded-lg border border-border">
          {accounts.map((acc) => {
            const isActive = acc.uid === selectedUid;
            return (
              <button
                key={acc.uid}
                type="button"
                onClick={() => !isActive && onChangeAccount(acc.uid)}
                className={`cursor-pointer px-4 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-[#0a1628] text-white dark:bg-primary dark:text-white'
                    : 'bg-surface text-text-secondary hover:bg-(--color-surface-secondary)'
                }`}
              >
                {formatLabel(acc)}
              </button>
            );
          })}
        </div>

        <button
          type="button"
          onClick={() => setEditOpen(true)}
          className="cursor-pointer  flex size-8 items-center justify-center rounded-lg border border-border text-text-secondary transition-colors hover:bg-(--color-surface-secondary) hover:text-text-primary"
          title={t('editAccountSettings')}
        >
          <Icon name="edit" size={16} />
        </button>
      </div>

      <EditAliasModal
        open={editOpen}
        onOpenChange={setEditOpen}
        accounts={editAccounts}
        defaultAccountUid={defaultAccountUid}
        onUpdate={onUpdate}
      />
    </>
  );
}
