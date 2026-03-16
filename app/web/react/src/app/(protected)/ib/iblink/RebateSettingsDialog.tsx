'use client';

import { useEffect, useState, useRef, useCallback } from 'react';
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
  getIBProductCategory,
  getIBDefaultLevelSettingMap,
  getIBRebateRuleDetailFull,
} from '@/actions';
import type {
  IBReferralSupplement,
  IBProductCategory,
  IBDefaultLevelSettingMap,
  IBDefaultLevelSettingOption,
  IBAccountLevelSetting,
} from '@/types/ib';

interface RebateSettingsDialogProps {
  isOpen: boolean;
  onClose: () => void;
  data: IBReferralSupplement | null;
  loading?: boolean;
  agentUid: number;
}

export function RebateSettingsDialog({
  isOpen,
  onClose,
  data,
  loading,
  agentUid,
}: RebateSettingsDialogProps) {
  const t = useTranslations('ib');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const isClient = data?.serviceType === 400;
  const isAgent = data?.serviceType === 300;
  const isPercentageMode = data?.summary?.distributionType === 3;

  const [productCategory, setProductCategory] = useState<IBProductCategory[]>([]);
  const [defaultLevelSetting, setDefaultLevelSetting] = useState<IBDefaultLevelSettingMap>({});
  const [currentAccountRebateRule, setCurrentAccountRebateRule] = useState<Record<number, IBAccountLevelSetting>>({});
  const [isRoot, setIsRoot] = useState(false);
  const [extraLoading, setExtraLoading] = useState(false);
  const extraFetched = useRef(false);

  const fetchExtraData = useCallback(async () => {
    if (!agentUid || extraFetched.current) return;
    setExtraLoading(true);
    try {
      const [catRes, defaultRes, ruleRes] = await Promise.all([
        executeRef.current(getIBProductCategory),
        executeRef.current(getIBDefaultLevelSettingMap, agentUid),
        executeRef.current(getIBRebateRuleDetailFull, agentUid),
      ]);

      if (catRes.success && catRes.data) setProductCategory(catRes.data);
      if (defaultRes.success && defaultRes.data) setDefaultLevelSetting(defaultRes.data);
      if (ruleRes.success && ruleRes.data) {
        setIsRoot(ruleRes.data.isRoot);
        const allowed = ruleRes.data.calculatedLevelSetting?.allowedAccounts || [];
        const ruleMap: Record<number, IBAccountLevelSetting> = {};
        allowed.forEach(acc => {
          const items: Record<number, number> = {};
          (acc.items || []).forEach(item => { items[item.cid] = item.r; });
          ruleMap[acc.accountType] = {
            accountType: acc.accountType,
            optionName: acc.optionName,
            percentage: acc.percentage ?? 100,
            allowPips: acc.allowPips || [],
            allowCommissions: acc.allowCommissions || [],
            pips: acc.pips,
            commission: acc.commission,
            items,
            selected: false,
          };
        });
        setCurrentAccountRebateRule(ruleMap);
      }
      extraFetched.current = true;
    } finally {
      setExtraLoading(false);
    }
  }, [agentUid]);

  useEffect(() => {
    if (isOpen && data && isAgent && !isPercentageMode && !extraFetched.current) {
      fetchExtraData();
    }
  }, [isOpen, data, isAgent, isPercentageMode, fetchExtraData]);

  useEffect(() => {
    if (!isOpen) {
      extraFetched.current = false;
    }
  }, [isOpen]);

  const calculate = (a: number, b: number): string => {
    if (b > a) return '0';
    return (a - b).toFixed(1).replace(/\.0$/, '');
  };

  const isDataLoading = loading || (isAgent && !isPercentageMode && extraLoading);

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="p-6!">
        <DialogHeader>
          <DialogTitle>{t('link.rebateSettingsTitle')}</DialogTitle>
          <DialogDescription className="sr-only">
            {t('link.rebateSettings')}
          </DialogDescription>
        </DialogHeader>

        {isDataLoading ? (
          <div className="mt-4 flex flex-col gap-5 animate-pulse">
            {/* Radio indicators skeleton */}
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
            {/* Account rows skeleton */}
            {[1, 2, 3].map(i => (
              <div key={i} className="flex items-center justify-between rounded-lg border border-border px-4 py-3">
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
        ) : data ? (
          <div className="mt-4 flex flex-col gap-5 max-h-[60vh] overflow-y-auto">

            {/* ==================== Mode 1: distributionType === 3 (percentage) ==================== */}
            {isPercentageMode ? (
              <PercentageView data={data} t={t} />
            ) : (
              <>
                {/* Common header: Agent/Client radio + divider + selectAccountType */}
                <div className="flex items-center gap-6">
                  <label className="flex items-center gap-2 text-sm text-text-primary">
                    <span className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${isAgent ? 'border-primary' : 'border-border'}`}>
                      {isAgent && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                    </span>
                    {t('link.agent')}
                  </label>
                  <label className="flex items-center gap-2 text-sm text-text-primary">
                    <span className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${isClient ? 'border-primary' : 'border-border'}`}>
                      {isClient && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                    </span>
                    {t('link.client')}
                  </label>
                </div>

                <hr className="border-border" />

                <p className="text-sm text-text-secondary">
                  <span className="text-primary">*</span>
                  {t('link.selectAccountType')}
                </p>

                {/* ==================== Mode 2: Agent ==================== */}
                {isAgent && (
                  <AgentView
                    data={data}
                    productCategory={productCategory}
                    defaultLevelSetting={defaultLevelSetting}
                    currentAccountRebateRule={currentAccountRebateRule}
                    isRoot={isRoot}
                    tAccount={tAccount}
                    t={t}
                    calculate={calculate}
                  />
                )}

                {/* ==================== Mode 3: Client ==================== */}
                {isClient && (
                  <ClientView data={data} tAccount={tAccount} t={t} />
                )}
              </>
            )}
          </div>
        ) : null}

        <DialogFooter className="mt-5">
          <Button variant="primary" onClick={onClose}>
            {t('link.close')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function PercentageView({
  data,
  t,
}: {
  data: IBReferralSupplement;
  t: ReturnType<typeof useTranslations>;
}) {
  const ps = data.summary?.percentageSchema;
  return (
    <div className="flex flex-col gap-4 p-2">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="mb-1 block text-sm font-medium text-text-secondary">
            {t('link.referCode')}
          </label>
          <Input value={data.code} disabled className="w-full" />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-text-secondary">
            {t('link.linkName')}
          </label>
          <Input value={data.summary?.name ?? ''} disabled className="w-full" />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="mb-1 block text-sm font-medium text-text-secondary">
            {t('link.language')}
          </label>
          <Input value={data.summary?.language ?? ''} disabled className="w-full" />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-text-secondary">
            {t('link.recordName')}
          </label>
          <Input value={ps?.optionName ?? ''} disabled className="w-full" />
        </div>
      </div>

      {data.serviceType === 300 && ps?.percentageSetting && (
        <>
          <hr className="border-border" />
          <div className="flex flex-col gap-3">
            {ps.percentageSetting.map((val, idx) => (
              <div key={idx} className="flex items-center gap-3">
                <label className="w-20 text-sm font-medium text-text-secondary">
                  Level {idx + 1}
                </label>
                <div className="flex items-center gap-2">
                  <Input value={String(val)} disabled className="w-[200px]" />
                  <span className="text-sm text-text-secondary">%</span>
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}

function AgentView({
  data,
  productCategory,
  defaultLevelSetting,
  currentAccountRebateRule,
  isRoot,
  tAccount,
  t,
  calculate,
}: {
  data: IBReferralSupplement;
  productCategory: IBProductCategory[];
  defaultLevelSetting: IBDefaultLevelSettingMap;
  currentAccountRebateRule: Record<number, IBAccountLevelSetting>;
  isRoot: boolean;
  tAccount: ReturnType<typeof useTranslations>;
  t: ReturnType<typeof useTranslations>;
  calculate: (a: number, b: number) => string;
}) {
  const schemas = data.summary?.schema ?? [];
  const categoryNameMap = (() => {
    try {
      return t.raw('type.productCategory') as Record<string, string>;
    } catch {
      return {};
    }
  })();

  if (schemas.length === 0) {
    return <div className="py-8 text-center text-sm text-text-secondary">{t('common.noData')}</div>;
  }

  return (
    <div className="flex flex-col gap-6">
      {schemas.map((account, index) => {
        const accountRule = currentAccountRebateRule[account.accountType];
        const showPips = isRoot && !!account.pips;
        const showCommission = isRoot && !!account.commission;
        const showPercentage = (account.pips || account.commission) && account.percentage !== 0;

        const resolvedDefault: IBDefaultLevelSettingOption | undefined = (() => {
          const raw = defaultLevelSetting[account.accountType];
          if (!raw?.length) return undefined;
          if (account.optionName) {
            return raw.find(o => (o.optionName ?? o.OptionName) === account.optionName) ?? raw[0];
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
                  <tr className="text-center">
                    <th className="bg-surface-secondary px-2 py-2 font-semibold text-text-primary">
                      {t('link.detailCategory')}
                    </th>
                    <th className="bg-surface-secondary px-2 py-2 font-semibold text-text-primary">
                      {t('link.detailTotalRebate')}
                    </th>
                    <th className="bg-surface-secondary px-2 py-2 font-semibold text-text-primary">
                      {t('link.detailPersonalRebate')}
                    </th>
                    <th className="bg-surface-secondary px-2 py-2 font-semibold text-text-primary">
                      {t('link.detailRemainRebate')}
                    </th>
                    {showPips && (
                      <th className="bg-surface-secondary px-2 py-2 font-semibold text-text-primary">
                        {t('link.pips')}
                      </th>
                    )}
                    {showCommission && (
                      <th className="bg-surface-secondary px-2 py-2 font-semibold text-text-primary">
                        {t('link.commission')}
                      </th>
                    )}
                    {showPercentage && (
                      <th className="bg-[#0053ad] px-2 py-2 font-semibold text-white">
                        %
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {(account.items ?? []).map((item, idx) => {
                    const rawCatName = productCategory.find(c => c.key === item.cid)?.value ?? String(item.cid);
                    const catName = categoryNameMap[rawCatName] ?? categoryNameMap[rawCatName.replace(/\./g, '_')] ?? rawCatName;
                    const totalRebate = accountRule?.items?.[item.cid] ?? 0;
                    const personalRebate = item.r;
                    const remainRebate = calculate(totalRebate, personalRebate);

                    let pipsValue: string | number = '-';
                    if (showPips && resolvedDefault) {
                      pipsValue = resolvedDefault.allowPipSetting?.[account.pips!]?.items?.[item.cid] ?? '-';
                    }

                    let commissionValue: string | number = '-';
                    if (showCommission && resolvedDefault) {
                      commissionValue = resolvedDefault.allowCommissionSetting?.[account.commission!]?.items?.[item.cid] ?? '-';
                    }

                    return (
                      <tr key={idx} className="text-center border-b border-dashed border-border">
                        <td className="px-2 py-2 text-text-primary">{catName}</td>
                        <td className="px-2 py-2 text-text-primary">{totalRebate}</td>
                        <td className="px-2 py-2 text-text-primary">{personalRebate}</td>
                        <td className="px-2 py-2 text-text-primary">{remainRebate}</td>
                        {showPips && (
                          <td className="px-2 py-2 text-text-primary">{pipsValue}</td>
                        )}
                        {showCommission && (
                          <td className="px-2 py-2 text-text-primary">{commissionValue}</td>
                        )}
                        {showPercentage && idx === 0 && (
                          <td
                            className="px-2 py-2 text-text-primary border-l border-border"
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

function ClientView({
  data,
  tAccount,
  t,
}: {
  data: IBReferralSupplement;
  tAccount: ReturnType<typeof useTranslations>;
  t: ReturnType<typeof useTranslations>;
}) {
  const schemas = data.summary?.allowAccountTypes ?? [];

  return (
    <div className="flex flex-col gap-3">
      {schemas.map((schema, index) => (
        <div
          key={index}
          className="flex items-center justify-between rounded-lg border border-border px-4 py-3"
        >
          <div className="flex items-center gap-3">
            <span
              className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${
                index === schemas.length - 1 ? 'border-primary' : 'border-border'
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
              <span
                className="rounded-md px-2 py-0.5 text-xs font-semibold"
                style={{ background: 'rgba(88,168,255,0.1)', color: '#4196f0' }}
              >
                {schema.optionName === 'alpha' ? 'Standard' : schema.optionName}
              </span>
            )}
          </div>
          <div className="flex items-center gap-4 text-sm text-text-secondary">
            <span>{t('link.pips')} ≥{schema.pips ?? 0}</span>
            <span>{t('link.commission')} ≥{schema.commission ?? 0}</span>
          </div>
        </div>
      ))}
    </div>
  );
}
