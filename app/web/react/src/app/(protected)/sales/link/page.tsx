'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesLinks } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { useUserStore } from '@/stores/userStore';
import { Button, DataTable, Icon, Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import type { SalesLink } from '@/types/sales';
import { getLanguageLabel } from '@/core/types/LanguageTypes';
import { SalesRebateSettingsDialog } from '../_components/modals/SalesRebateSettingsDialog';
import { EditSalesLinkDialog } from '../_components/modals/EditSalesLinkDialog';
import { AddSalesLinkDialog } from '../_components/modals/AddSalesLinkDialog';

const SERVICE_TYPE_LABELS: Record<number, string> = {
  200: 'IB',
  300: 'IB',
  400: 'Client',
};

function CopyLinkCell({
  item,
  onCopy,
}: {
  item: SalesLink;
  onCopy: (item: SalesLink) => void;
}) {
  const t = useTranslations('sales');
  const [copied, setCopied] = useState(false);

  const handleClick = useCallback(async () => {
    await onCopy(item);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, [item, onCopy]);

  return (
    <button
      type="button"
      className={`cursor-pointer flex items-center gap-1.5 text-sm font-medium ${
        copied ? 'text-[#004EFF]' : 'text-text-primary'
      } hover:underline`}
      onClick={handleClick}
    >
      {copied ? t('link.copied') : t('link.copyLink')}
      <Icon name="copy" size={14} />
    </button>
  );
}

export default function SalesLinkPage() {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const salesAccount = useSalesStore((s) => s.salesAccount);
  const salesStoreInitialized = useSalesStore((s) => s.isInitialized);
  const siteConfig = useUserStore((s) => s.siteConfig);
  const rebateEnabled = siteConfig?.rebateEnabled ?? false;

  const [data, setData] = useState<SalesLink[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [rebateDialogOpen, setRebateDialogOpen] = useState(false);
  const [rebateDialogCode, setRebateDialogCode] = useState<string | null>(null);

  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editDialogItem, setEditDialogItem] = useState<SalesLink | null>(null);

  const [addDialogOpen, setAddDialogOpen] = useState(false);

  const [copyConfirmOpen, setCopyConfirmOpen] = useState(false);
  const [copyConfirmItem, setCopyConfirmItem] = useState<SalesLink | null>(null);
  const [copiedLink, setCopiedLink] = useState('');

  const fetchData = useCallback(async () => {
    if (!salesAccount) return;
    setIsLoading(true);
    try {
      const result = await executeRef.current(getSalesLinks, salesAccount.uid, {
        page: 1,
        size: 100,
        status: 0,
      });
      if (result.success && result.data) {
        let links = Array.isArray(result.data.data) ? result.data.data : [];
        if (rebateEnabled) {
          links = links.filter((item) => item.code?.startsWith('RS'));
        }
        setData(links);
      }
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, rebateEnabled]);

  const salesUid = salesAccount?.uid;
  useEffect(() => {
    if (!salesStoreInitialized) return;
    if (salesUid) fetchData();
    else setIsLoading(false);
  }, [salesUid, salesStoreInitialized, fetchData]);

  const handleViewRebateSettings = useCallback((item: SalesLink) => {
    setRebateDialogCode(item.code);
    setRebateDialogOpen(true);
  }, []);

  const handleEditLink = useCallback((item: SalesLink) => {
    setEditDialogItem(item);
    setEditDialogOpen(true);
  }, []);

  const handleCopyLink = useCallback(
    async (item: SalesLink) => {
      const siteId = siteConfig?.siteId ?? '';
      const lang =
        item.displaySummary?.language ??
        item.summary?.language ??
        'en';
      const baseUrl =
        typeof window !== 'undefined' ? window.location.origin : '';
      const link = `${baseUrl}/sign-up?code=${item.code}&siteId=${siteId}&lang=${lang}`;
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(link);
      } else {
        const ta = document.createElement('textarea');
        ta.value = link;
        ta.style.cssText = 'position:fixed;opacity:0';
        document.body.appendChild(ta);
        ta.select();
        document.execCommand('copy');
        document.body.removeChild(ta);
      }
      setCopiedLink(link);
      setCopyConfirmItem(item);
      setCopyConfirmOpen(true);
    },
    [siteConfig]
  );

  const handleAddSuccess = useCallback(() => {
    fetchData();
  }, [fetchData]);

  const columns = useMemo<DataTableColumn<SalesLink>[]>(() => {
    const viewRebateLabel = (() => {
      try {
        return t('link.viewRebate');
      } catch {
        return t('link.view');
      }
    })();
    const cols: DataTableColumn<SalesLink>[] = [
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
        render: (item) => <span className="text-sm">{item.code}</span>,
      },
      // {
      //   key: 'accountType',
      //   title: t('link.accountType'),
      //   skeletonWidth: 'w-20',
      //   render: (item) => {
      //     const schemas =
      //       item.serviceType !== 400
      //         ? item.summary?.schema
      //         : item.summary?.allowAccountTypes;
      //     if (!schemas?.length) return '-';
      //     return (
      //       <div className="flex flex-wrap gap-1">
      //         {schemas.map((s, i) => (
      //           <span
      //             key={i}
      //             className="rounded-md px-2 py-0.5 text-xs"
      //             style={{
      //               background: [
      //                 'rgba(88,168,255,0.1)',
      //                 'rgba(255,164,0,0.15)',
      //                 'rgba(123,97,255,0.1)',
      //               ][i % 3],
      //               color: ['#4196f0', '#ffa400', '#7b61ff'][i % 3],
      //             }}
      //           >
      //             {tAccount(`accountTypes.${s.accountType}`)}
      //           </span>
      //         ))}
      //       </div>
      //     );
      //   },
      // },
      {
        key: 'linkType',
        title: t('link.linkType'),
        skeletonWidth: 'w-16',
        render: (item) => (
          <span className="text-sm">
            {tAccount(`accountRole.${item.serviceType}`)}
          </span>
        ),
      },
      {
        key: 'language',
        title: t('link.language'),
        skeletonWidth: 'w-20',
        render: (item) => {
          const lang =
            item.displaySummary?.language ?? item.summary?.language;
          return (
            <span className="text-sm">
              {getLanguageLabel(lang) || lang || '-'}
            </span>
          );
        },
      },
    ];

    if (rebateEnabled) {
      cols.push({
        key: 'rebateSettings',
        title: t('link.rebateSettings'),
        skeletonWidth: 'w-16',
        render: (item) => (
          <span
            className="cursor-pointer text-sm font-medium hover:underline"
            onClick={() => handleViewRebateSettings(item)}
          >
            {viewRebateLabel}
          </span>
        ),
      });
    }
    cols.push({
      key: 'isAutoCreatePaymentMethod',
      title: t('link.autoCreateAccount'),
      skeletonWidth: 'w-10',
      render: (item) => <span className="text-sm">{item.isAutoCreatePaymentMethod === 1 ? t('link.yes') : t('link.no')}</span>,
    });
    cols.push({
      key: 'copyLink',
      title: t('link.clickCopy'),
      skeletonWidth: 'w-24',
      render: (item) => <CopyLinkCell item={item} onCopy={handleCopyLink} />,
    });

    return cols;
  }, [
    t,
    tAccount,
    rebateEnabled,
    handleCopyLink,
    handleViewRebateSettings,
    handleEditLink,
  ]);

  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-text-primary">
          {t('link.manageLinks')}
        </h2>
        {rebateEnabled && (
          <Button
            size="sm"
            className="flex items-center gap-1.5"
            onClick={() => setAddDialogOpen(true)}
          >
            <Icon name="add-plain" size={16} />
            {t('link.addLink')}
          </Button>
        )}
      </div>
      <DataTable<SalesLink>
        columns={columns}
        data={data}
        rowKey={(item, idx) => item.id ?? idx}
        loading={isLoading}
      />

      {/* 返佣设置弹窗 */}
      <SalesRebateSettingsDialog
        open={rebateDialogOpen}
        onOpenChange={setRebateDialogOpen}
        code={rebateDialogCode}
      />

      {/* 编辑链接弹窗 */}
      <EditSalesLinkDialog
        isOpen={editDialogOpen}
        onClose={() => setEditDialogOpen(false)}
        onSuccess={fetchData}
        item={editDialogItem}
      />

      {/* 添加链接弹窗 */}
      {salesAccount && rebateEnabled && (
        <AddSalesLinkDialog
          isOpen={addDialogOpen}
          onClose={() => setAddDialogOpen(false)}
          onSuccess={handleAddSuccess}
        />
      )}

      {/* 复制链接确认弹窗 */}
      <Dialog open={copyConfirmOpen} onOpenChange={setCopyConfirmOpen}>
        <DialogContent className="p-6!">
          <DialogHeader>
            <DialogTitle>{t('link.linkCopiedTitle')}</DialogTitle>
            <DialogDescription className="sr-only">
              {t('link.linkCopiedTitle')}
            </DialogDescription>
          </DialogHeader>
          {copyConfirmItem && (
            <div className="mt-4 flex flex-col gap-3 text-sm">
              <div className="flex gap-2">
                <span className="text-text-secondary">
                  {t('link.linkName')}:
                </span>
                <span className="text-text-primary">
                  {copyConfirmItem.name || '-'}
                </span>
              </div>
              <div className="flex gap-2">
                <span className="text-text-secondary">
                  {t('link.referCode')}:
                </span>
                <span className="text-text-primary">
                  {copyConfirmItem.code}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <span className="shrink-0 text-text-secondary">
                  {t('link.accountType')}:
                </span>
                <div className="flex flex-wrap gap-1">
                  {(copyConfirmItem.serviceType !== 400
                    ? copyConfirmItem.summary?.schema
                    : copyConfirmItem.summary?.allowAccountTypes
                  )?.map((s, i) => (
                    <span
                      key={i}
                      className="rounded-md px-2 py-0.5 text-xs"
                      style={{
                        background: [
                          'rgba(88,168,255,0.1)',
                          'rgba(255,164,0,0.15)',
                          'rgba(123,97,255,0.1)',
                        ][i % 3],
                        color: ['#4196f0', '#ffa400', '#7b61ff'][i % 3],
                      }}
                    >
                      {tAccount(`accountTypes.${s.accountType}`)}
                    </span>
                  )) || '-'}
                </div>
              </div>
              <div className="flex gap-2">
                <span className="text-text-secondary">
                  {t('link.linkType')}:
                </span>
                <span className="text-text-primary">
                  {SERVICE_TYPE_LABELS[copyConfirmItem.serviceType ?? 0] || '-'}
                </span>
              </div>
              <div className="flex gap-2">
                <span className="text-text-secondary">
                  {t('link.language')}:
                </span>
                <span className="text-text-primary">
                  {getLanguageLabel(
                    copyConfirmItem.displaySummary?.language ??
                      copyConfirmItem.summary?.language
                  ) ||
                    copyConfirmItem.summary?.language ||
                    '-'}
                </span>
              </div>
              <div className="flex gap-2">
                <span className="shrink-0 text-text-secondary">
                  {t('link.referralLink')}:
                </span>
                <span className="break-all text-text-primary">
                  {copiedLink}
                </span>
              </div>
            </div>
          )}
          <DialogFooter>
            <div className="mt-6 flex justify-end gap-3 md:gap-5">
              <Button
                variant="outline"
                onClick={() => setCopyConfirmOpen(false)}
                className="w-auto min-w-20 md:w-[120px]"
              >
                OK
              </Button>
            </div>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
