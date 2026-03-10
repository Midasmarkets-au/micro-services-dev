'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { updateIBLink, setIBDefaultClient } from '@/actions';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, Input, Button, Switch } from '@/components/ui';
import type { IBLink } from '@/types/ib';

interface EditLinkDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  item: IBLink | null;
  agentUid: number;
}

export function EditLinkDialog({ isOpen, onClose, onSuccess, item, agentUid }: EditLinkDialogProps) {
  const t = useTranslations('ib');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });

  const [newName, setNewName] = useState('');
  const [isDefault, setIsDefault] = useState(false);
  const [defaultLoading, setDefaultLoading] = useState(false);

  const isClient = item?.serviceType === 400;

  useEffect(() => {
    if (isOpen && item) {
      setNewName('');
      setIsDefault(item.isDefault === 1 || item.isDefault === true);
    }
  }, [isOpen, item]);

  const handleSetDefault = useCallback(async (checked: boolean) => {
    if (!checked || !item) return;

    setDefaultLoading(true);
    try {
      const result = await execute(setIBDefaultClient, agentUid, item.code);
      if (result.success) {
        onSuccess();
        onClose();
      }
    } finally {
      setDefaultLoading(false);
    }
  }, [item, agentUid, execute, onSuccess, onClose]);

  const handleUpdate = useCallback(async () => {
    if (!newName.trim() || !item) return;

    const result = await execute(updateIBLink, agentUid, item.code, { name: newName.trim() });
    if (result.success) {
      onSuccess();
      onClose();
    }
  }, [newName, item, agentUid, execute, onSuccess, onClose]);

  if (!item) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="p-6!">
        <DialogHeader className="flex-row items-center justify-between">
          <DialogTitle>{t('link.editLink')}</DialogTitle>
          <DialogDescription className="sr-only">
            {t('link.editLink')}
          </DialogDescription>
          {isClient && (
            <div className="flex items-center gap-2">
              <span className="text-sm text-text-secondary">{t('link.default')}</span>
              <Switch
                checked={isDefault}
                onChange={handleSetDefault}
                disabled={defaultLoading || isDefault}
              />
            </div>
          )}
        </DialogHeader>

        <div className="mt-4 flex flex-col gap-5">
          <div>
            <label className="mb-1.5 block text-sm text-text-secondary">
              <span className="text-primary">*</span>
              {t('link.linkName')}
            </label>
            <Input
              value={item.name || ''}
              disabled
              className="w-full"
            />
          </div>

          <div>
            <label className="mb-1.5 block text-sm text-text-secondary">
              <span className="text-primary">*</span>
              {t('link.newLinkName')}
            </label>
            <Input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder={t('link.pleaseInput')}
              className="w-full"
            />
          </div>
        </div>

        <div className="mt-6 flex justify-end gap-3 md:gap-5">
          <Button variant="secondary" onClick={onClose} className="w-auto min-w-20 md:w-[120px]">
            {t('link.close')}
          </Button>
          <Button
            onClick={handleUpdate}
            loading={isLoading}
            disabled={!newName.trim()}
            className="w-auto min-w-20 md:w-[120px]"
          >
            {t('link.update')}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
