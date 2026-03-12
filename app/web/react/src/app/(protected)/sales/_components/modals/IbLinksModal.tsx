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
import { Skeleton, Switch, Pagination, Icon, Button } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import {
  getSalesLinks,
  updateSalesLink,
  setSalesDefaultCode,
  getSalesIBRebateRuleDetail,
  getSalesAccountDefaultLevel,
  getSalesIBAccountConfig,
} from '@/actions';
import type { SalesClientAccount, SalesLink } from '@/types/sales';
import { AccountRoleTypes } from '@/types/accounts';

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

function camelCaseKeys(obj: any): any {
  if (Array.isArray(obj)) return obj.map(camelCaseKeys);
  if (typeof obj === 'object' && obj !== null) {
    return Object.keys(obj).reduce((acc: any, key: string) => {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      acc[camelKey] = camelCaseKeys(obj[key]);
      return acc;
    }, {});
  }
  return obj;
}

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

  // 用于 IBLinkDetail 子模态
  const [rebateRuleDetail, setRebateRuleDetail] = useState<any>(null);
  const [currentAccountRebateRule, setCurrentAccountRebateRule] = useState<
    Record<number, any>
  >({});

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
    setIsLoading(true);

    try {
      // 获取 rebateRuleDetail
      const ruleDetailResult = await execute(
        getSalesIBRebateRuleDetail,
        salesAccount.uid,
        account.uid
      );

      if (!ruleDetailResult.success || !ruleDetailResult.data) {
        setIsLoading(false);
        return;
      }

      const ruleDetail = ruleDetailResult.data as any;
      setRebateRuleDetail(ruleDetail);

      // 获取 defaultLevelSetting
      const defaultLevelResult = await execute(
        getSalesAccountDefaultLevel,
        salesAccount.uid,
        ruleDetail.agentAccountUid
      );
      if (defaultLevelResult.success && defaultLevelResult.data) {
        camelCaseKeys(defaultLevelResult.data);
      }

      // 获取 IB 账户配置
      const ibConfigResult = await execute(
        getSalesIBAccountConfig,
        salesAccount.uid,
        account.uid
      );
      let configLevelSetting: any = {};
      if (ibConfigResult.success && Array.isArray(ibConfigResult.data)) {
        const ibConfig = ibConfigResult.data as any[];
        if (ibConfig.length !== 0) {
          const getDefaultLevelSetting = ibConfig.find(
            (x: any) => x.key === 'DefaultRebateLevelSetting'
          );
          if (getDefaultLevelSetting) {
            configLevelSetting = JSON.parse(getDefaultLevelSetting.value);
          }
        }
      }

      // 构建 currentAccountRebateRule
      const levelSetting =
        ruleDetail.calculatedLevelSetting?.allowedAccounts || [];
      const newCurrentAccountRebateRule: Record<number, any> = {};

      levelSetting.forEach((accountItem: any) => {
        const currentAccount: any = {
          selected: false,
          optionName: accountItem.optionName,
          accountType: accountItem.accountType,
          percentage: accountItem.percentage,
          allowPips: accountItem.allowPips,
          allowCommissions: accountItem.allowCommissions,
          pips: accountItem.pips,
          commission: accountItem.commission,
          items: {} as Record<number, number>,
        };

        (accountItem.items || []).forEach((item: any) => {
          if (Object.keys(configLevelSetting).length === 0) {
            currentAccount.items[item.cid] = item.r;
          } else {
            currentAccount.items[item.cid] =
              configLevelSetting[accountItem.accountType]?.[0]?.Category?.[
                item.cid
              ];
          }
        });

        newCurrentAccountRebateRule[accountItem.accountType] = currentAccount;
      });

      setCurrentAccountRebateRule(newCurrentAccountRebateRule);

      // 获取链接列表
      const childUid = account.uid;
      setCriteria((prev) => ({ ...prev, childAccountUid: childUid }));
      await fetchData(1, childUid);
    } catch {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute, fetchData]);

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
        showToast({ message: t('ibLinks.copied'), type: 'success' });
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
        showToast({ message: t('ibLinks.copied'), type: 'success' });
        setEditDialogOpen(false);
        await fetchData(1, criteria.childAccountUid);
      }
    } catch {
      // error handled
    }
    setIsUpdating(false);
  };

  // 查看详情
  const showDetail = (_code: string) => {
    // IBLinkDetail 子模态的展示逻辑
    // 可在后续实现完整的 IBLinkDetailModal
    void _code;
    void rebateRuleDetail;
    void currentAccountRebateRule;
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

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-[1000px]">
          <DialogHeader>
            <DialogTitle>
              {userName} {t('ibLinks.title')}
            </DialogTitle>
          </DialogHeader>

          <div className="max-h-[80vh] overflow-y-auto">
            {isLoading && data.length === 0 ? (
              <div className="space-y-3 py-4">
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
              </div>
            ) : !isLoading && data.length === 0 ? (
              <div className="py-12 text-center text-text-secondary">
                {t('ibLinks.noLinks')}
              </div>
            ) : (
              <>
                <div className="w-full overflow-x-auto">
                  <table className="min-w-max border-collapse align-middle text-sm whitespace-nowrap">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.linkName')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.referCode')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.accountType')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.linkType')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.language')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.view')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.active')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.autoCreateAccount')}
                      </th>
                      <th className="px-3 py-3 text-center font-medium">
                        {t('ibLinks.actions')}
                      </th>
                    </tr>
                  </thead>

                  {isLoading ? (
                    <tbody>
                      <tr>
                        <td colSpan={9} className="py-8 text-center">
                          <Skeleton className="mx-auto h-6 w-48" />
                        </td>
                      </tr>
                    </tbody>
                  ) : (
                    <tbody>
                      {data.map((item, index) => (
                        <tr
                          key={index}
                          className="border-b border-border last:border-b-0"
                        >
                          {/* 链接名称 */}
                          <td className="px-3 py-3 text-center">
                            {item.name}
                            <button
                              type="button"
                              className="ms-2 text-text-secondary hover:text-primary"
                              onClick={() => editCode(item)}
                            >
                              <Icon name="edit-line" size={14} />
                            </button>
                          </td>

                          {/* 推荐码 */}
                          <td className="px-3 py-3 text-center font-mono">
                            {item.code}
                          </td>

                          {/* 账户类型徽章 */}
                          <td className="px-3 py-3 text-center">
                            <div className="flex items-center justify-center">
                              {item.serviceType ===
                                REFERRAL_SERVICE_TYPE.Client &&
                                item.displaySummary?.allowAccountTypes?.map(
                                  (acc, i) => (
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
                                      {t(
                                        `type.shortAccount.${acc.accountType}` as any
                                      )}
                                    </span>
                                  )
                                )}
                              {item.serviceType ===
                                REFERRAL_SERVICE_TYPE.Agent &&
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
                                    {t(
                                      `type.shortAccount.${acc.accountType}` as any
                                    )}
                                  </span>
                                ))}
                            </div>
                          </td>

                          {/* 链接类型 */}
                          <td className="px-3 py-3 text-center">
                            <div className="flex items-center justify-center gap-1">
                              {t(
                                `type.accountRole.${item.serviceType}` as any
                              )}
                              {item.serviceType ===
                                AccountRoleTypes.Client &&
                                item.isDefault === 1 && (
                                  <span className="rounded bg-green-100 px-1.5 py-0.5 text-xs text-green-700">
                                    {t('ibLinks.default')}
                                  </span>
                                )}
                            </div>
                          </td>

                          {/* 语言 */}
                          <td className="px-3 py-3 text-center">
                            {getLinkLanguage(item)}
                          </td>

                          {/* 查看 */}
                          <td className="px-3 py-3 text-center">
                            <button
                              type="button"
                              className="text-text-secondary hover:text-primary"
                              onClick={() => showDetail(item.code)}
                            >
                              <Icon name="eye-line" size={16} />
                            </button>
                          </td>

                          {/* 激活状态开关 */}
                          <td className="px-3 py-3 text-center">
                            <div className="flex justify-center">
                              <Switch
                                checked={item.status === 0}
                                onChange={() => changeStatus(item)}
                              />
                            </div>
                          </td>

                          {/* 自动开户 */}
                          <td className="px-3 py-3 text-center">
                            {item.displaySummary
                              ?.isAutoCreatePaymentMethod === 1
                              ? t('ibLinks.yes')
                              : t('ibLinks.no')}
                          </td>

                          {/* 操作：复制推荐链接 */}
                          <td className="px-3 py-3 text-center">
                            <button
                              type="button"
                              className="rounded bg-primary/10 px-3 py-1 text-xs font-normal text-primary hover:bg-primary/20"
                              onClick={() => copyReferralLink(item)}
                            >
                              {t('ibLinks.copyReferralLink')}
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  )}
                  </table>
                </div>

                {/* 分页 */}
                <div className="mt-4">
                  <Pagination
                    page={criteria.page}
                    total={criteria.total}
                    size={criteria.pageSize}
                    onPageChange={handlePageChange}
                  />
                </div>
              </>
            )}
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

      {/* 编辑链接名称弹窗 */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="max-w-[750px]">
          <DialogHeader>
            <div className="flex items-center gap-4">
              <DialogTitle>{t('ibLinks.editLink')}</DialogTitle>
              {editItem?.serviceType === AccountRoleTypes.Client && (
                <Switch
                  checked={editIsDefault === 1}
                  onChange={() => handleSetDefault()}
                />
              )}
            </div>
          </DialogHeader>

          <div className="py-4">
            <div className="grid grid-cols-2 gap-6">
              <div>
                <div className="mb-2 flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-primary" />
                  <span className="text-sm font-medium">
                    {t('ibLinks.linkName')}
                  </span>
                </div>
                <input
                  className="w-full rounded border border-gray-300 bg-gray-50 px-3 py-2 text-sm"
                  value={editItem?.name || ''}
                  disabled
                  readOnly
                />
              </div>
              <div>
                <div className="mb-2 flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-primary" />
                  <span className="text-sm font-medium">
                    {t('ibLinks.newLinkName')}
                  </span>
                </div>
                <input
                  className="w-full rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none"
                  value={editNewName}
                  onChange={(e) => setEditNewName(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleUpdateName()}
                  placeholder=""
                />
              </div>
            </div>

            <div className="mt-8 flex justify-center">
              <button
                type="button"
                className="rounded-lg bg-primary px-8 py-2.5 text-sm font-medium text-white transition-colors hover:bg-primary/90 disabled:opacity-50"
                onClick={handleUpdateName}
                disabled={isUpdating || !editNewName.trim()}
              >
                {isUpdating ? '...' : t('ibLinks.update')}
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
