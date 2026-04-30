'use client';

import { useEffect, useMemo, useState, useRef, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  Button,
  Input,
} from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import {
  getReferralLinkDetail,
  getSalesSymbolCategory,
  getSalesAccountDefaultLevel,
  getSalesIBRebateRuleDetail,
} from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import type {
  SalesLinkDetail,
  SalesLinkSchema,
  SalesDefaultLevelSettingMap,
  SalesDefaultLevelSettingOption,
} from '@/types/sales';

interface SalesRebateSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  code: string | null;
  agentUid?: number;
}

const SERVICE_TYPE_BROKER = 200;
const SERVICE_TYPE_IB_BROKER = 300;
const SERVICE_TYPE_CLIENT = 400;

type ProductCategoryItem = {
  key?: number;
  id?: number;
  value?: string;
  name?: string;
};

export function SalesRebateSettingsDialog({
  open,
  onOpenChange,
  code,
  agentUid,
}: SalesRebateSettingsDialogProps) {
  const t = useTranslations('sales');
  const tType = useTranslations('type');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [isLoading, setIsLoading] = useState(false);
  const [detail, setDetail] = useState<SalesLinkDetail | null>(null);
  const [productCategory, setProductCategory] = useState<ProductCategoryItem[]>([]);

  // serviceType=300 额外数据
  const [currentAccountRebateRule, setCurrentAccountRebateRule] = useState<Record<number, Record<number, number>>>({});
  const [defaultLevelSetting, setDefaultLevelSetting] = useState<SalesDefaultLevelSettingMap>({});
  const [isRoot, setIsRoot] = useState(false);
  const [extraLoading, setExtraLoading] = useState(false);
  const extraFetched = useRef(false);

  const fetchExtraData = useCallback(async () => {
    if (!salesAccount || !agentUid || extraFetched.current) return;
    setExtraLoading(true);
    try {
      const [ruleRes, defaultRes] = await Promise.all([
        executeRef.current(getSalesIBRebateRuleDetail, salesAccount.uid, agentUid),
        executeRef.current(getSalesAccountDefaultLevel, salesAccount.uid, agentUid),
      ]);

      if (ruleRes.success && ruleRes.data) {
        setIsRoot(ruleRes.data.isRoot ?? false);
        const allowed = ruleRes.data.calculatedLevelSetting?.allowedAccounts ?? [];
        const ruleMap: Record<number, Record<number, number>> = {};
        allowed.forEach((acc) => {
          const items: Record<number, number> = {};
          (acc.items ?? []).forEach((item) => { items[item.cid] = item.r; });
          ruleMap[acc.accountType] = items;
        });
        setCurrentAccountRebateRule(ruleMap);
      }

      if (defaultRes.success && defaultRes.data) {
        setDefaultLevelSetting(defaultRes.data);
      }

      extraFetched.current = true;
    } finally {
      setExtraLoading(false);
    }
  }, [salesAccount, agentUid]);

  useEffect(() => {
    if (!open) {
      setIsLoading(false);
      setDetail(null);
      extraFetched.current = false;
      setCurrentAccountRebateRule({});
      setDefaultLevelSetting({});
      setIsRoot(false);
      return;
    }

    if (!code || !salesAccount) {
      setDetail(null);
      return;
    }

    let cancelled = false;
    setIsLoading(true);
    setDetail(null);

    (async () => {
      try {
        const [detailRes, categoryRes] = await Promise.all([
          execute(getReferralLinkDetail, code),
          execute(getSalesSymbolCategory),
        ]);

        if (cancelled) return;

        if (categoryRes.success && Array.isArray(categoryRes.data)) {
          setProductCategory(categoryRes.data as ProductCategoryItem[]);
        }

        if (detailRes.success && detailRes.data) {
          const normalized: SalesLinkDetail = {
            ...detailRes.data,
            serviceType:
              detailRes.data.serviceType == null
                ? undefined
                : Number(detailRes.data.serviceType),
          };

          if (
            normalized.summary &&
            Object.keys(normalized.summary).length === 0
          ) {
            setDetail(null);
          } else {
            setDetail(normalized);
          }
        } else {
          setDetail(null);
        }
      } catch {
        if (!cancelled) {
          setDetail(null);
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [open, code, salesAccount, execute]);

  // 当 detail 加载完且是 300 类型时，拉取额外数据
  useEffect(() => {
    if (open && detail?.serviceType === SERVICE_TYPE_IB_BROKER && agentUid && !extraFetched.current) {
      fetchExtraData();
    }
  }, [open, detail, agentUid, fetchExtraData]);

  const productCategoryMap = useMemo(() => {
    const i18nMap = (() => {
      try {
        return tType.raw('productCategory') as Record<string, string>;
      } catch {
        return {};
      }
    })();
    const map = new Map<number, string>();
    productCategory.forEach((item) => {
      const k = Number(item.key ?? item.id);
      if (!Number.isFinite(k)) return;
      const v = item.value ?? item.name;
      if (v)
        map.set(
          k,
          i18nMap[v] ?? i18nMap[v.replace(/\./g, '_')] ?? v
        );
    });
    return map;
  }, [productCategory, tType]);

  const pipOptionMap = useMemo(() => {
    try {
      return tType.raw('pipOptions') as Record<string, string>;
    } catch {
      return {};
    }
  }, [tType]);

  const commissionOptionMap = useMemo(() => {
    try {
      return tType.raw('commissionOptions') as Record<string, string>;
    } catch {
      return {};
    }
  }, [tType]);

  const isAgent = detail?.serviceType === SERVICE_TYPE_BROKER;
  const isIBAgent = detail?.serviceType === SERVICE_TYPE_IB_BROKER;
  const isClient = detail?.serviceType === SERVICE_TYPE_CLIENT;

  const isDataLoading = isLoading || (isIBAgent && extraLoading);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('link.rebateSettings')}</DialogTitle>
          <DialogDescription className="sr-only">
            {t('link.rebateSettings')}
          </DialogDescription>
        </DialogHeader>

        {isDataLoading ? (
          <div className="mt-4 flex flex-col gap-5 animate-pulse">
            <div className="flex items-center gap-6">
              <div className="flex items-center gap-2">
                <div className="h-5 w-5 rounded-full bg-surface-secondary" />
                <div className="h-4 w-8 rounded bg-surface-secondary" />
              </div>
              <div className="flex items-center gap-2">
                <div className="h-5 w-5 rounded-full bg-surface-secondary" />
                <div className="h-4 w-8 rounded bg-surface-secondary" />
              </div>
            </div>
            <hr className="border-border" />
            <div className="h-4 w-24 rounded bg-surface-secondary" />
            {[1, 2].map((i) => (
              <div
                key={i}
                className="flex items-center justify-between rounded-lg border border-border px-4 py-3"
              >
                <div className="flex items-center gap-3">
                  <div className="h-5 w-5 rounded-full bg-surface-secondary" />
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-5 w-16 rounded-md bg-surface-secondary" />
                </div>
                <div className="flex items-center gap-4">
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                </div>
              </div>
            ))}
          </div>
        ) : detail ? (
          <div className="mt-4 flex max-h-[72vh] flex-col gap-5 overflow-y-auto pr-1">
            <div className="flex flex-wrap items-center gap-4 sm:gap-6">
              <label className="flex items-center gap-2 text-sm text-text-primary">
                <span
                  className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${
                    isAgent || isIBAgent ? 'border-primary' : 'border-border'
                  }`}
                >
                  {(isAgent || isIBAgent) && (
                    <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                  )}
                </span>
                {t('link.agent')}
              </label>
              <label className="flex items-center gap-2 text-sm text-text-primary">
                <span
                  className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${
                    isClient ? 'border-primary' : 'border-border'
                  }`}
                >
                  {isClient && (
                    <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                  )}
                </span>
                {t('link.client')}
              </label>
            </div>

            <hr className="border-border" />

            <div className="flex items-start gap-2 text-sm text-text-secondary">
              <span className="mt-1 inline-block h-2 w-2 rounded-full bg-primary" />
              <span className="font-medium text-text-primary">
                {t('link.selectAccountType')}
              </span>
              <span>{t('link.selectAccountType')}</span>
            </div>

            {isAgent && (
              <AgentDetailView
                schemas={detail.summary?.schema ?? []}
                tAccount={tAccount}
                t={t}
                productCategoryMap={productCategoryMap}
                pipOptionMap={pipOptionMap}
                commissionOptionMap={commissionOptionMap}
              />
            )}

            {isIBAgent && (
              <IBAgentDetailView
                schemas={detail.summary?.schema ?? []}
                tAccount={tAccount}
                t={t}
                productCategoryMap={productCategoryMap}
                currentAccountRebateRule={currentAccountRebateRule}
                defaultLevelSetting={defaultLevelSetting}
                isRoot={isRoot}
              />
            )}

            {isClient && (
              <ClientDetailView
                schemas={detail.summary?.allowAccountTypes ?? []}
                tAccount={tAccount}
                t={t}
              />
            )}
          </div>
        ) : (
          <div className="py-8 text-center text-sm text-text-secondary">
            {t('link.noData')}
          </div>
        )}

        <DialogFooter className="mt-5">
          <Button variant="outline" className="w-auto min-w-20 md:w-[120px]" onClick={() => onOpenChange(false)}>
            {t('link.close')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ==================== serviceType=300: 表格视图 ====================

function IBAgentDetailView({
  schemas,
  tAccount,
  t,
  productCategoryMap,
  currentAccountRebateRule,
  defaultLevelSetting,
  isRoot,
}: {
  schemas: SalesLinkSchema[];
  tAccount: ReturnType<typeof useTranslations>;
  t: ReturnType<typeof useTranslations>;
  productCategoryMap: Map<number, string>;
  currentAccountRebateRule: Record<number, Record<number, number>>;
  defaultLevelSetting: SalesDefaultLevelSettingMap;
  isRoot: boolean;
}) {
  if (schemas.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-text-secondary">
        {t('link.noData')}
      </div>
    );
  }

  const calculate = (a: number, b: number): string => {
    if (b > a) return '0';
    const result = a - b;
    return Number.isInteger(result) ? String(result) : String(parseFloat(result.toPrecision(10)));
  };

  return (
    <div className="flex flex-col gap-6">
      {schemas.map((account, index) => {
        const accountItems = currentAccountRebateRule[account.accountType] ?? {};
        const showPips = isRoot && !!account.pips;
        const showCommission = isRoot && !!account.commission;
        const showPercentage = !!(account.pips || account.commission) && (account.percentage ?? 0) !== 0;

        const resolvedDefault: SalesDefaultLevelSettingOption | undefined = (() => {
          const raw = defaultLevelSetting[String(account.accountType)];
          if (!raw?.length) return undefined;
          if (account.optionName) {
            return (
              raw.find(
                (o) =>
                  (o.optionName ?? o.OptionName)?.toLowerCase() ===
                  account.optionName!.toLowerCase()
              ) ?? raw[0]
            );
          }
          return raw[0];
        })();

        return (
          <div key={index}>
            <div className="flex items-center gap-2 px-3 py-2">
              <div className="h-4 w-0.5 bg-primary" />
              <span className="text-base font-medium text-text-primary">
                {tAccount(`accountTypes.${account.accountType}`)}
              </span>
            </div>

            <div className="overflow-x-auto px-3">
              <table className="w-full min-w-[500px] border-collapse text-sm">
                <thead>
                  <tr>
                    <th className="bg-surface-secondary px-2 py-2 text-left font-semibold text-text-primary">
                      {t('link.category')}
                    </th>
                    <th className="bg-surface-secondary px-2 py-2 text-right font-semibold text-text-primary">
                      {t('link.totalRebate')}
                    </th>
                    <th className="bg-surface-secondary px-2 py-2 text-right font-semibold text-text-primary">
                      {t('link.personalRebate')}
                    </th>
                    <th className="bg-surface-secondary px-2 py-2 text-right font-semibold text-text-primary">
                      {t('link.remainRebate')}
                    </th>
                    {showPips && (
                      <th className="bg-surface-secondary px-2 py-2 text-right font-semibold text-text-primary">
                        {t('link.pips')}
                      </th>
                    )}
                    {showCommission && (
                      <th className="bg-surface-secondary px-2 py-2 text-right font-semibold text-text-primary">
                        {t('link.commission')}
                      </th>
                    )}
                    {showPercentage && (
                      <th className="bg-[#0053ad] px-2 py-2 text-right font-semibold text-white">
                        %
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {(account.items ?? []).map((item, idx) => {
                    const catName = productCategoryMap.get(item.cid) ?? String(item.cid);
                    const totalRebate = accountItems[item.cid] ?? 0;
                    const personalRebate = item.r;
                    const remainRebate = calculate(totalRebate, personalRebate);

                    let pipsValue: string | number = '-';
                    if (showPips && resolvedDefault) {
                      pipsValue =
                        resolvedDefault.allowPipSetting?.[String(account.pips!)]?.items?.[String(item.cid)] ?? '-';
                    }

                    let commissionValue: string | number = '-';
                    if (showCommission && resolvedDefault) {
                      commissionValue =
                        resolvedDefault.allowCommissionSetting?.[String(account.commission!)]?.items?.[String(item.cid)] ?? '-';
                    }

                    return (
                      <tr key={idx} className="border-b border-dashed border-border">
                        <td className="px-2 py-2 text-left text-text-primary">{catName}</td>
                        <td className="px-2 py-2 text-right text-text-primary">{totalRebate}</td>
                        <td className="px-2 py-2 text-right text-text-primary">{personalRebate}</td>
                        <td className="px-2 py-2 text-right text-text-primary">{remainRebate}</td>
                        {showPips && (
                          <td className="px-2 py-2 text-right text-text-primary">{pipsValue}</td>
                        )}
                        {showCommission && (
                          <td className="px-2 py-2 text-right text-text-primary">{commissionValue}</td>
                        )}
                        {showPercentage && idx === 0 && (
                          <td
                            className="border-l border-border px-2 py-2 text-right text-text-primary"
                            rowSpan={(account.items ?? []).length}
                          >
                            {account.percentage}
                          </td>
                        )}
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        );
      })}
    </div>
  );
}

// ==================== serviceType=200: 卡片视图 ====================

function AgentDetailView({
  schemas,
  tAccount,
  t,
  productCategoryMap,
  pipOptionMap,
  commissionOptionMap,
}: {
  schemas: SalesLinkSchema[];
  tAccount: ReturnType<typeof useTranslations>;
  t: ReturnType<typeof useTranslations>;
  productCategoryMap: Map<number, string>;
  pipOptionMap: Record<string, string>;
  commissionOptionMap: Record<string, string>;
}) {
  if (schemas.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-text-secondary">
        {t('link.noData')}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      {schemas.map((schema, index) => (
        <div
          key={index}
          className="rounded-xl border border-border px-3 py-3 shadow-[0_1px_4px_rgba(0,0,0,0.12)] sm:px-4"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2">
              <span className="flex h-5 w-5 items-center justify-center rounded-full border-2 border-border">
                <span className="h-2.5 w-2.5 rounded-full bg-primary" />
              </span>
              <span className="text-sm font-medium text-text-primary">
                {tAccount(`accountTypes.${schema.accountType}`)}
              </span>
            </div>
            {schema.optionName && (
              <span className="rounded-md bg-[rgba(128,0,32,0.2)] px-2 py-0.5 text-xs font-semibold text-[#800020]">
                {schema.optionName === 'alpha'
                  ? tAccount(`accountTypes.${schema.accountType}`)
                  : schema.optionName}
              </span>
            )}
          </div>

          {schema.items && schema.items.length > 0 && (
            <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
              {schema.items.map((item, idx) => (
                <div key={idx} className="rounded-md p-2">
                  <div className="truncate text-xs text-text-secondary">
                    {productCategoryMap.get(item.cid) ?? String(item.cid)}
                  </div>
                  <Input
                    value={String(item.r)}
                    disabled
                    className="mt-1 h-8 bg-transparent text-sm"
                  />
                </div>
              ))}
            </div>
          )}

          {schema.allowPips && schema.allowPips.length > 0 && (
            <div className="mt-3">
              <span className="text-xs text-text-secondary">
                {t('rebateEdit.availablePips')}:
              </span>
              <div className="mt-2 flex flex-wrap gap-2">
                {schema.allowPips.map((p, idx) => (
                  <span
                    key={idx}
                    className="inline-block rounded border border-border px-2 py-0.5 text-xs text-text-primary"
                  >
                    {pipOptionMap[String(p)] ?? String(p)}
                  </span>
                ))}
              </div>
            </div>
          )}

          {schema.allowCommissions && schema.allowCommissions.length > 0 && (
            <div className="mt-3">
              <span className="text-xs text-text-secondary">
                {t('rebateEdit.availableCommission')}:
              </span>
              <div className="mt-2 flex flex-wrap gap-2">
                {schema.allowCommissions.map((c, idx) => (
                  <span
                    key={idx}
                    className="inline-block rounded border border-border px-2 py-0.5 text-xs text-text-primary"
                  >
                    {commissionOptionMap[String(c)] ?? String(c)}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>
      ))}
    </div>
  );
}

// ==================== serviceType=400: 客户视图 ====================

function ClientDetailView({
  schemas,
  tAccount,
  t,
}: {
  schemas: SalesLinkSchema[];
  tAccount: ReturnType<typeof useTranslations>;
  t: ReturnType<typeof useTranslations>;
}) {
  if (schemas.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-text-secondary">
        {t('link.noData')}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-3">
      {schemas.map((schema, index) => (
        <div
          key={index}
          className="rounded-lg border border-border px-3 py-3 sm:px-4"
        >
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex flex-wrap items-center gap-3">
              <span
                className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${
                  index === schemas.length - 1
                    ? 'border-primary'
                    : 'border-border'
                }`}
              >
                {index === schemas.length - 1 && (
                  <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                )}
              </span>
              <span className="text-sm font-medium text-text-primary">
                {tAccount(`accountTypes.${schema.accountType}`)}
              </span>
              {schema.optionName && (
                <span className="rounded-md bg-[rgba(128,0,32,0.2)] px-2 py-0.5 text-xs font-semibold text-[#800020]">
                  {schema.optionName === 'alpha'
                    ? tAccount(`accountTypes.${schema.accountType}`)
                    : schema.optionName}
                </span>
              )}
            </div>
            <div className="flex flex-wrap items-center gap-2 text-sm text-text-secondary">
              <span className="rounded bg-surface-secondary px-2 py-1">
                {t('link.pips')} =&gt;{' '}
                {schema.pips == null || schema.pips === 0 ? '0' : schema.pips}
              </span>
              <span className="rounded bg-surface-secondary px-2 py-1">
                {t('link.commission')} =&gt;{' '}
                {schema.commission == null || schema.commission === 0
                  ? '0'
                  : schema.commission}
              </span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
