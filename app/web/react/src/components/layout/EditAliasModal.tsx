'use client';

import { useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { updateAccountAlias, updateDefaultParentAccount } from '@/actions';
import { useToast } from '@/hooks/useToast';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  Input,
  Button,
} from '@/components/ui';

export interface EditAliasAccount {
  uid: number;
  siteId?: number;
  alias?: string;
}

interface EditAliasModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accounts: EditAliasAccount[];
  defaultAccountUid?: number | string;
  onUpdate: () => void;
}

export function EditAliasModal({
  open,
  onOpenChange,
  accounts: accountsProp,
  defaultAccountUid,
  onUpdate,
}: EditAliasModalProps) {
  const t = useTranslations('common');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();

  const [localAccounts, setLocalAccounts] = useState<
    (EditAliasAccount & { isDefault: boolean; editAlias: string })[]
  >([]);

  useEffect(() => {
    if (open && accountsProp.length > 0) {
      setLocalAccounts(
        accountsProp.map((acc) => ({
          ...acc,
          isDefault: String(acc.uid) === String(defaultAccountUid),
          editAlias: acc.alias || '',
        }))
      );
    }
  }, [open, accountsProp, defaultAccountUid]);

  const handleUpdateAlias = async (uid: number, alias: string) => {
    const result = await execute(updateAccountAlias, uid, alias);
    if (result.success) {
      showSuccess(t('success'));
      onUpdate();
    }
  };

  const handleSetDefault = async (uid: number) => {
    const result = await execute(updateDefaultParentAccount, uid);
    if (result.success) {
      showSuccess(t('success'));
      setLocalAccounts((prev) =>
        prev.map((acc) => ({ ...acc, isDefault: acc.uid === uid }))
      );
      onUpdate();
    }
  };

  const getSiteLabel = (siteId?: number) => {
    if (siteId === undefined || siteId === null) return '';
    try {
      return t(`siteType.${siteId}`);
    } catch {
      return '';
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[650px]">
        <DialogHeader>
          <DialogTitle>{t('editAccountSettings')}</DialogTitle>
        </DialogHeader>

        <div className="mt-4 flex flex-col gap-4">
          {localAccounts.map((item) => (
            <div key={item.uid} className="flex flex-wrap items-center gap-3 border-b border-border pb-4 last:border-0">
              <div className="w-28 shrink-0 text-sm font-medium text-text-primary">
                {item.uid}
                {item.siteId !== undefined && (
                  <span className="text-text-secondary">({getSiteLabel(item.siteId)})</span>
                )}
              </div>

              <Input
                value={item.editAlias}
                onChange={(e) =>
                  setLocalAccounts((prev) =>
                    prev.map((a) =>
                      a.uid === item.uid ? { ...a, editAlias: e.target.value } : a
                    )
                  )
                }
                placeholder={t('aliasPlaceholder')}
                inputSize="sm"
                className="w-[180px]"
                disabled={isLoading}
              />

              <Button
                size="sm"
                variant="primary"
                onClick={() => handleUpdateAlias(item.uid, item.editAlias)}
                disabled={isLoading}
                className="whitespace-nowrap"
              >
                {t('updateAlias')}
              </Button>

              {item.isDefault ? (
                <Button size="sm" variant="destructive" disabled className="whitespace-nowrap">
                  {t('default')}
                </Button>
              ) : (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => handleSetDefault(item.uid)}
                  disabled={isLoading}
                  className="whitespace-nowrap border-green-500 text-green-600 hover:bg-green-50"
                >
                  {t('setDefault')}
                </Button>
              )}
            </div>
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
}
