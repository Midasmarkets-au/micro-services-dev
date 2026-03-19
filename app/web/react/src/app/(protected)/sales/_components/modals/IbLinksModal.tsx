'use client';

import { useState, useCallback, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useLocale } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Switch, Pagination, Icon, Button, DataTable, Input } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import {
  getSalesLinks,
  updateSalesLink,
  setSalesDefaultCode,
} from '@/actions';
import type { SalesClientAccount, SalesLink } from '@/types/sales';
import { AccountRoleTypes } from '@/types/accounts';
import { SalesRebateSettingsDialog } from './SalesRebateSettingsDialog';

/* eslint-disable @typescript-eslint/no-explicit-any */

const REFERRAL_SERVICE_TYPE = {
  Agent: 300,
  Client: 400,
} as const;

const LOCALE_MAP: Record<string, string> = {
  en: 'en-us',
  zh: 'zh-cn',
  'zh-tw': 'zh-tw',
  vi: 'vi-vn',
  th: 'th-th',
  jp: 'jp-jp',
  id: 'id-id',
  ms: 'ms-my',
  ko: 'ko-kr',
  km: 'km-kh',
  es: 'es-es',
};

interface IbLinksModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}

export function IbLinksModal({
  open,
  onOpenChange,
  account,
}: IbLinksModalProps) {
  const t = useTranslations('sales');
  const locale = useLocale();
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const { showToast } = useToast();

  const [isLoading, setIsLoading] = useState(false);
  const [data, setData] = useState<SalesLink[]>([]);
  const [criteria, setCriteria] = useState({
    page: 1,
    pageSize: 10,
    childAccountUid: 0,
    total: 0,
  });

  // 链接详情弹窗状态
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [detailCode, setDetailCode] = useState<string | null>(null);

  // 编辑链接名称状态
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<SalesLink | null>(null);
  const [editNewName, setEditNewName] = useState('');
  const [isUpdating, setIsUpdating] = useState(false);
  const [editIsDefault, setEditIsDefault] = useState(0);

  const currentLanguage = LOCALE_MAP[locale] || 'en-us';
  const initRef = useRef(false);

  const fetchData = useCallback(
    async (page: number, childAccountUid: number) => {
      if (!salesAccount) return;
      setIsLoading(true);

      try {
        const res = await execute(getSalesLinks, salesAccount.uid, {
          page,
          pageSize: 10,
          childAccountUid,
          status: 0,
        } as any);

        if (res.success && res.data) {
          setData(Array.isArray(res.data.data) ? res.data.data : []);
          const resCriteria = (res.data.criteria || {}) as any;
          setCriteria((prev) => ({
            ...prev,
            page: resCriteria.page ?? page,
            pageSize: resCriteria.pageSize ?? resCriteria.size ?? 10,
            total: resCriteria.total ?? 0,
          }));
        }
      } catch {
        // error handled by useServerAction
      }

      setIsLoading(false);
    },
    [salesAccount, execute]
  );

  const initData = useCallback(async () => {
    if (!salesAccount || !account) return;
    const childUid = account.uid;
    setCriteria((prev) => ({ ...prev, childAccountUid: childUid }));
    await fetchData(1, childUid);
  }, [salesAccount, account, fetchData]);

  useEffect(() => {
    if (!open || !account) {
      initRef.current = false;
      return;
    }
    if (initRef.current) return;
    initRef.current = true;

    let cancelled = false;
    (async () => {
      if (!cancelled) await initData();
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, account]);

  // 切换激活状态
  const changeStatus = async (item: SalesLink) => {
    if (!salesAccount) return;
    const newStatus = item.status === 0 ? 1 : 0;
    try {
      await execute(updateSalesLink, salesAccount.uid, item.id, {
        status: newStatus,
        name: item.name,
      });
      setData((prev) =>
        prev.map((d) => (d.id === item.id ? { ...d, status: newStatus } : d))
      );
    } catch {
      // error handled
    }
  };

  // 打开编辑链接名称弹窗
  const editCode = (item: SalesLink) => {
    setEditItem(item);
    setEditNewName('');
    setEditIsDefault(
      typeof item.isDefault === 'number' ? item.isDefault : item.isDefault ? 1 : 0
    );
    setEditDialogOpen(true);
  };

  // 更新链接名称
  const handleUpdateName = async () => {
    if (!salesAccount || !editItem || !editNewName.trim()) return;
    setIsUpdating(true);
    try {
      const result = await execute(
        updateSalesLink,
        salesAccount.uid,
        editItem.id,
        {
          status: editItem.status,
          name: editNewName.trim(),
        }
      );
      if (result.success) {
        //showToast({ message: t('ibLinks.copied'), type: 'success' });
        setEditDialogOpen(false);
        await fetchData(criteria.page, criteria.childAccountUid);
      }
    } catch {
      // error handled
    }
    setIsUpdating(false);
  };

  // 设为默认链接
  const handleSetDefault = async () => {
    if (!salesAccount || !editItem) return;
    if (editIsDefault === 0) {
      setEditIsDefault(1);
      return;
    }
    setIsUpdating(true);
    try {
      const result = await execute(
        setSalesDefaultCode,
        salesAccount.uid,
        editItem.code
      );
      if (result.success) {
        setEditDialogOpen(false);
        await fetchData(1, criteria.childAccountUid);
      }
    } catch {
      // error handled
    }
    setIsUpdating(false);
  };

  // 查看详情
  const showDetail = (code: string) => {
    setDetailCode(code);
    setDetailDialogOpen(true);
  };

  // 复制推荐链接
  const copyReferralLink = (item: SalesLink) => {
    const baseUrl = process.env.NEXT_PUBLIC_APP_URL || window.location.origin;
    const lang =
      item.displaySummary?.language === undefined
        ? currentLanguage
        : item.displaySummary.language;
    const link = `${baseUrl}/sign-up?code=${item.code}&siteId=${salesAccount?.siteId ?? ''}&lang=${lang}`;
    navigator.clipboard.writeText(link).then(() => {
      showToast({ message: t('ibLinks.copiedToClipboard'), type: 'success' });
    });
  };

  // 获取链接语言显示
  const getLinkLanguage = (item: SalesLink): string => {
    const lang =
      item.displaySummary?.language === undefined
        ? currentLanguage
        : item.displaySummary.language;
    return t(`type.language.${lang}` as any) || lang;
  };

  const userName =
    account?.user?.displayName || account?.user?.nativeName || '';

  // 换页
  const handlePageChange = (page: number) => {
    fetchData(page, criteria.childAccountUid);
  };

  const columns: DataTableColumn<SalesLink>[] = [
    {
      key: 'name',
      title: t('ibLinks.linkName'),
      align: 'center',
      skeletonWidth: 'w-24',
      render: (item) => (
        <div className="flex items-center justify-center">
          {item.name}
          <Icon name="edit" className="ms-1" size={14} onClick={() => editCode(item)} />
        </div>
      ),
    },
    {
      key: 'code',
      title: t('ibLinks.referCode'),
      align: 'center',
      skeletonWidth: 'w-18',
      render: (item) => <span className="font-mono">{item.code}</span>,
    },
    {
      key: 'accountType',
      title: t('ibLinks.accountType'),
      align: 'center',
      skeletonWidth: 'w-10',
      width: 'w-10',
      render: (item) => (
        <div className="flex items-center justify-center">
          {item.serviceType === REFERRAL_SERVICE_TYPE.Client &&
            item.displaySummary?.allowAccountTypes?.map((acc, i) => (
              <span
                key={`type_${i}`}
                className={`ms-1 inline-block rounded-lg px-2 py-0.5 text-[10px] font-bold ${
                  [
                    'bg-[rgba(88,168,255,0.1)] text-[#4196f0]',
                    'bg-[rgba(255,164,0,0.15)] text-[#ffa400]',
                    'bg-[rgba(123,97,255,0.1)] text-[#7b61ff]',
                  ][i % 3]
                }`}
              >
                {t(`type.account.${acc.accountType}` as any)}
              </span>
            ))}
          {item.serviceType === REFERRAL_SERVICE_TYPE.Agent &&
            item.displaySummary?.schema?.map((acc, i) => (
              <span
                key={`type_${i}`}
                className={`ms-1 inline-block rounded-lg px-2 py-0.5 text-[10px] font-bold ${
                  [
                    'bg-[rgba(88,168,255,0.1)] text-[#4196f0]',
                    'bg-[rgba(255,164,0,0.15)] text-[#ffa400]',
                    'bg-[rgba(123,97,255,0.1)] text-[#7b61ff]',
                  ][i % 3]
                }`}
              >
                {t(`type.account.${acc.accountType}` as any)}
              </span>
            ))}
        </div>
      ),
    },
    {
      key: 'serviceType',
      title: t('ibLinks.linkType'),
      align: 'center',
      skeletonWidth: 'w-20',
      render: (item) => (
        <div className="flex items-center justify-center gap-1">
          {t(`type.accountRole.${item.serviceType}` as any)}
          {item.serviceType === AccountRoleTypes.Client &&
            item.isDefault === 1 && (
              <span className="rounded bg-green-100 px-1.5 py-0.5 text-xs text-green-700">
                {t('ibLinks.default')}
              </span>
            )}
        </div>
      ),
    },
    {
      key: 'language',
      title: t('ibLinks.language'),
      align: 'center',
      skeletonWidth: 'w-16',
      render: (item) => getLinkLanguage(item),
    },
    {
      key: 'view',
      title: t('ibLinks.view'),
      align: 'center',
      skeletonWidth: 'w-8',
      render: (item) => (
        <button
          type="button"
          className="text-text-secondary cursor-pointer hover:text-primary"
          onClick={() => showDetail(item.code)}
        >
          <Icon name="eye_open" size={16} />
        </button>
      ),
    },
    {
      key: 'active',
      title: t('ibLinks.active'),
      align: 'center',
      skeletonWidth: 'w-8',
      skeletonHeight: 'h-5',
      render: (item) => (
        <div className="flex justify-center">
          <Switch
            checked={item.status === 0}
            onChange={() => changeStatus(item)}
          />
        </div>
      ),
    },
    {
      key: 'autoCreate',
      title: t('ibLinks.autoCreateAccount'),
      align: 'center',
      skeletonWidth: 'w-8',
      render: (item) =>
        item.displaySummary?.isAutoCreatePaymentMethod === 1
          ? t('ibLinks.yes')
          : t('ibLinks.no'),
    },
    {
      key: 'actions',
      title: t('ibLinks.actions'),
      align: 'center',
      skeletonWidth: 'w-16',
      render: (item) => (
        <Button
          variant="outline"
          size="xs"
          className="rounded px-3 py-1 text-xs font-normal"
          onClick={() => copyReferralLink(item)}
        >
          {t('ibLinks.copyLink')}
        </Button>
      ),
    },
  ];

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {userName} {t('ibLinks.title')}
            </DialogTitle>
          </DialogHeader>

          <div className="max-h-[80vh] overflow-y-auto">
            <DataTable<SalesLink>
              columns={columns}
              data={data}
              rowKey={(item) => item.id}
              loading={isLoading}
              emptyContent={
                <div className="py-12 text-center text-text-secondary">
                  {t('ibLinks.noLinks')}
                </div>
              }
              stretchHeight={false}
            />
            <div className="mt-4">
              <Pagination
                page={criteria.page}
                total={criteria.total}
                size={criteria.pageSize}
                onPageChange={handlePageChange}
              />
            </div>
          </div>
          <div className="flex justify-end gap-3 pt-4">
            <Button
              variant="outline"
              size="sm"
              className="w-auto min-w-20 md:w-[120px]"
              onClick={() => onOpenChange(false)}
            >
              {t('action.close')}
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* 链接详情弹窗 */}
      <SalesRebateSettingsDialog
        open={detailDialogOpen}
        onOpenChange={setDetailDialogOpen}
        code={detailCode}
      />

      {/* 编辑链接名称弹窗 */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <div className="flex items-center gap-4">
              <DialogTitle>{t('ibLinks.editLink')}</DialogTitle>
              {editItem?.serviceType === AccountRoleTypes.Client && (
                <div className="flex items-center gap-2">
                  <Switch
                    checked={editIsDefault === 1}
                    onChange={() => handleSetDefault()}
                    disabled={editIsDefault === 1}
                  />
                  <span className="text-sm text-text-secondary">
                    {editIsDefault === 1
                      ? t('ibLinks.default')
                      : t('ibLinks.notDefault')}
                  </span>
                </div>
              )}
            </div>
          </DialogHeader>

          <div className="py-4">
            <div className="grid grid-cols-2 gap-6">
              <Input
                label={t('ibLinks.linkName')}
                value={editItem?.name || ''}
                disabled
                readOnly
                inputSize="sm"
              />
              <Input
                label={t('ibLinks.newLinkName')}
                value={editNewName}
                onChange={(e) => setEditNewName(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleUpdateName()}
                inputSize="sm"
              />
            </div>

            <div className="mt-8 flex justify-center gap-3">
              <Button variant="outline" size="sm" className="w-auto min-w-20 md:w-[120px]" onClick={() => setEditDialogOpen(false)}>
                {t('action.cancel')}
              </Button>
              <Button
                size="sm"
                className="w-auto min-w-20 md:w-[120px]"
                onClick={handleUpdateName}
                disabled={isUpdating || !editNewName.trim()}
              >
                {isUpdating ? '...' : t('ibLinks.update')}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
