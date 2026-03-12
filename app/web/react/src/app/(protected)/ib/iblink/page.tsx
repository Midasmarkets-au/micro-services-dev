'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBLinks, getReferralCodeSupplement } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { Button, DataTable, Icon, Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import type { IBLink, IBReferralSupplement } from '@/types/ib';
import { useUserStore } from '@/stores/userStore';
import { getLanguageLabel } from '@/core/types/LanguageTypes';
import { RebateSettingsDialog } from './RebateSettingsDialog';
import { AddLinkDialog } from './AddLinkDialog';
import { EditLinkDialog } from './EditLinkDialog';

const ROLE_TYPE_LABELS: Record<number, string> = {
  300: 'IB',
  400: 'Client',
};

function CopyLinkCell({ item, onCopy }: { item: IBLink; onCopy: (item: IBLink) => void }) {
  const t = useTranslations('ib');
  const [copied, setCopied] = useState(false);

  const handleClick = useCallback(async () => {
    await onCopy(item);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, [item, onCopy]);

  return (
    <button
      type="button"
      className={`flex items-center gap-1.5 text-sm font-medium ${copied ? 'text-[#004EFF]' : 'text-text-primary'} hover:underline`}
      onClick={handleClick}
    >
      {copied ? t('link.copied') : t('link.copyLink')}
      <Icon name="copy" size={14} />
    </button>
  );
}

export default function IBLinkPage() {
  const t = useTranslations('ib');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const agentAccount = useIBStore((s) => s.agentAccount);
  const ibStoreInitialized = useIBStore((s) => s.isInitialized);
  const siteConfig = useUserStore((s) => s.siteConfig);

  const [data, setData] = useState<IBLink[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogData, setDialogData] = useState<IBReferralSupplement | null>(null);
  const [dialogLoading, setDialogLoading] = useState(false);
  const [addLinkOpen, setAddLinkOpen] = useState(false);
  const [editLinkOpen, setEditLinkOpen] = useState(false);
  const [editLinkItem, setEditLinkItem] = useState<IBLink | null>(null);
  const [copyConfirmOpen, setCopyConfirmOpen] = useState(false);
  const [copyConfirmItem, setCopyConfirmItem] = useState<IBLink | null>(null);
  const [copiedLink, setCopiedLink] = useState('');

  const rebateEnabled = useUserStore((s) => s.siteConfig?.rebateEnabled);

  const fetchData = useCallback(async () => {
    if (!agentAccount) return;
    setIsLoading(true);
    try {
      const result = await executeRef.current(getIBLinks, agentAccount.uid, {
        page: 1,
        size: 100,
      });
      if (result.success && result.data) {
        setData(Array.isArray(result.data.data) ? result.data.data : []);
      }
    } finally {
      setIsLoading(false);
    }
  }, [agentAccount]);

  const agentUid = agentAccount?.uid;
  useEffect(() => {
    if (!ibStoreInitialized) return;
    if (agentUid) fetchData();
    else setIsLoading(false);
  }, [agentUid, ibStoreInitialized, fetchData]);

  const handleViewRebateSettings = useCallback(async (item: IBLink) => {
    setDialogOpen(true);
    setDialogData(null);
    setDialogLoading(true);
    try {
      const result = await executeRef.current(getReferralCodeSupplement, item.code);
      if (result.success && result.data) {
        setDialogData(result.data);
      }
    } finally {
      setDialogLoading(false);
    }
  }, []);

  const handleEditLink = useCallback((item: IBLink) => {
    setEditLinkItem(item);
    setEditLinkOpen(true);
  }, []);

  const handleCopyLink = useCallback(async (item: IBLink) => {
    const siteId = siteConfig?.siteId ?? '';
    const lang = item.summary?.language || 'en';
    const baseUrl = typeof window !== 'undefined' ? window.location.origin : '';
    const link = `${baseUrl}/sign-up?code=${item.code}&siteId=${siteId}&lang=${lang}`;
    await navigator.clipboard.writeText(link);
    setCopiedLink(link);
    setCopyConfirmItem(item);
    setCopyConfirmOpen(true);
  }, [siteConfig]);

  const columns = useMemo<DataTableColumn<IBLink>[]>(() => [
    {
      key: 'name',
      title: t('link.linkName'),
      skeletonWidth: 'w-28',
      render: (item) => (
        <div className="flex items-center gap-2">
        <span className="text-sm leading-4">{item.name || '-'}</span>
          <button
            type="button"
            className="cursor-pointer inline-flex h-4 w-4 items-center justify-center text-text-secondary hover:text-primary"
            onClick={() => handleEditLink(item)}
          >
            <Icon name="edit" size={14} className="block w-full h-full" />
          </button>
        </div>
      ),
    },
    {
      key: 'code',
      title: t('link.referCode'),
      skeletonWidth: 'w-24',
      render: (item) => (
        <span className="text-sm">{item.code}</span>
      ),
    },
    {
      key: 'accountType',
      title: t('link.accountType'),
      skeletonWidth: 'w-20',
      render: (item) => {
        const schemas = item.serviceType !== 400
          ? item.summary?.schema
          : item.summary?.allowAccountTypes;
        if (!schemas?.length) return '-';
        return (
          <div className="flex flex-wrap gap-1">
            {schemas.map((s, i) => (
              <span
                key={i}
                className="rounded-md px-2 py-0.5 text-xs"
                style={{
                  background: ['rgba(88,168,255,0.1)', 'rgba(255,164,0,0.15)', 'rgba(123,97,255,0.1)'][i % 3],
                  color: ['#4196f0', '#ffa400', '#7b61ff'][i % 3],
                }}
              >
                {tAccount(`accountTypes.${s.accountType}`)}
              </span>
            ))}
          </div>
        );
      },
    },
    {
      key: 'linkType',
      title: t('link.linkType'),
      skeletonWidth: 'w-16',
      render: (item) => (
        <span className="text-sm">
          {ROLE_TYPE_LABELS[item.serviceType ?? 0] || item.type || '-'}
        </span>
      ),
    },
    {
      key: 'language',
      title: t('link.language'),
      skeletonWidth: 'w-20',
      render: (item) => {
        const lang = item.summary?.language;
        return getLanguageLabel(lang) || lang || '-';
      },
    },
    {
      key: 'rebateSettings',
      title: t('link.rebateSettings'),
      skeletonWidth: 'w-16',
      render: (item) => (
        <span
          className="cursor-pointer text-sm font-medium hover:underline"
          onClick={() => handleViewRebateSettings(item)}
        >
          {t('link.view')}
        </span>
      ),
    },
    {
      key: 'copyLink',
      title: t('link.clickCopy'),
      skeletonWidth: 'w-24',
      render: (item) => (
        <CopyLinkCell item={item} onCopy={handleCopyLink} />
      ),
    },
  ], [t, tAccount, handleCopyLink, handleViewRebateSettings, handleEditLink]);

  return (
    <div className="flex min-h-full w-full min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold text-text-primary">
          {t('link.manageLinks')}
        </h2>
        {rebateEnabled && (
          <Button size="sm" className="flex items-center gap-1.5" onClick={() => setAddLinkOpen(true)}>
            <Icon name="add-plain" size={16} />
            {t('link.addLink')}
          </Button>
        )}
      </div>

      <DataTable<IBLink>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
        className="flex-1"
      />

      <RebateSettingsDialog
        isOpen={dialogOpen}
        onClose={() => setDialogOpen(false)}
        data={dialogData}
        loading={dialogLoading}
        agentUid={agentAccount?.uid ?? 0}
      />

      {agentAccount && (
        <AddLinkDialog
          isOpen={addLinkOpen}
          onClose={() => setAddLinkOpen(false)}
          onSuccess={fetchData}
          agentUid={agentAccount.uid}
        />
      )}

      <EditLinkDialog
        isOpen={editLinkOpen}
        onClose={() => setEditLinkOpen(false)}
        onSuccess={fetchData}
        item={editLinkItem}
        agentUid={agentAccount?.uid ?? 0}
      />

      <Dialog open={copyConfirmOpen} onOpenChange={setCopyConfirmOpen}>
        <DialogContent className="p-6!">
          <DialogHeader>
            <DialogTitle>{t('link.linkCopiedTitle')}</DialogTitle>
            <DialogDescription className="sr-only">{t('link.linkCopiedTitle')}</DialogDescription>
          </DialogHeader>
          {copyConfirmItem && (
            <div className="mt-4 flex flex-col gap-3 text-sm">
              <div className="flex gap-2">
                <span className="text-text-secondary">{t('link.linkName')}:</span>
                <span className="text-text-primary">{copyConfirmItem.name || '-'}</span>
              </div>
              <div className="flex gap-2">
                <span className="text-text-secondary">{t('link.referCode')}:</span>
                <span className="text-text-primary">{copyConfirmItem.code}</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="shrink-0 text-text-secondary">{t('link.accountType')}:</span>
                <div className="flex flex-wrap gap-1">
                  {(copyConfirmItem.serviceType !== 400
                    ? copyConfirmItem.summary?.schema
                    : copyConfirmItem.summary?.allowAccountTypes
                  )?.map((s, i) => (
                    <span
                      key={i}
                      className="rounded-md px-2 py-0.5 text-xs"
                      style={{
                        background: ['rgba(88,168,255,0.1)', 'rgba(255,164,0,0.15)', 'rgba(123,97,255,0.1)'][i % 3],
                        color: ['#4196f0', '#ffa400', '#7b61ff'][i % 3],
                      }}
                    >
                      {tAccount(`accountTypes.${s.accountType}`)}
                    </span>
                  )) || '-'}
                </div>
              </div>
              <div className="flex gap-2">
                <span className="text-text-secondary">{t('link.linkType')}:</span>
                <span className="text-text-primary">
                  {ROLE_TYPE_LABELS[copyConfirmItem.serviceType ?? 0] || '-'}
                </span>
              </div>
              <div className="flex gap-2">
                <span className="text-text-secondary">{t('link.language')}:</span>
                <span className="text-text-primary">
                  {getLanguageLabel(copyConfirmItem.summary?.language) || copyConfirmItem.summary?.language || '-'}
                </span>
              </div>
              <div className="flex gap-2">
                <span className="shrink-0 text-text-secondary">{t('link.referralLink')}:</span>
                <span className="break-all text-text-primary">{copiedLink}</span>
              </div>
            </div>
          )}
          <div className="mt-6 flex justify-end gap-3 md:gap-5">
            <Button variant="secondary" onClick={() => setCopyConfirmOpen(false)} className="w-auto min-w-20 md:w-[120px]">
              OK
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
