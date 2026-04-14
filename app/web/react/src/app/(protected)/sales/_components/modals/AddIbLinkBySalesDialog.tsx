'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  Button,
  Input,
  Switch,
  SimpleSelect,
} from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import {
  getSalesIBRebateRuleDetail,
  getSalesAccountDefaultLevel,
  getSalesIBAccountConfig,
  getIBProductCategory,
  createSalesIBAgentLink,
  createSalesIBClientLink,
} from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { LINK_LANGUAGE_OPTIONS } from '@/core/types/LanguageTypes';
import type {
  IBProductCategory,
  IBDefaultLevelSettingMap,
  IBDefaultLevelSettingOption,
  IBAccountLevelSetting,
} from '@/types/ib';
import { AgentRebateTable, ClientPCForm } from '../../../ib/iblink/AddLinkDialog';

interface SalesIBRuleDetail {
  isRoot: boolean;
  agentAccountUid: number;
  calculatedLevelSetting: {
    distributionType: number;
    allowedAccounts: Array<{
      accountType: number;
      optionName?: string;
      percentage?: number;
      allowPips?: number[];
      allowCommissions?: number[];
      pips?: number;
      commission?: number;
      items?: { cid: number; r: number }[];
    }>;
  };
}

interface AddIbLinkBySalesDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  ibUid: number;
  userName?: string;
}

export function AddIbLinkBySalesDialog({
  isOpen,
  onClose,
  onSuccess,
  ibUid,
  userName,
}: AddIbLinkBySalesDialogProps) {
  const t = useTranslations('ib');
  const tType = useTranslations('type');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [name, setName] = useState('');
  const [language, setLanguage] = useState('en-us');
  const [serviceType, setServiceType] = useState<number | null>(null);
  const [isAutoCreate, setIsAutoCreate] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [initLoading, setInitLoading] = useState(false);
  const [initDone, setInitDone] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const formCollectors = useRef<Record<number, () => Record<string, unknown>>>({});
  const pcFormCollectors = useRef<Record<number, () => Record<string, unknown>>>({});

  const registerFormCollector = useCallback(
    (accountType: number, collectData: () => Record<string, unknown>) => {
      formCollectors.current[accountType] = collectData;
    },
    []
  );

  const registerPCFormCollector = useCallback(
    (accountType: number, collectData: () => Record<string, unknown>) => {
      pcFormCollectors.current[accountType] = collectData;
    },
    []
  );

  const [ruleDetail, setRuleDetail] = useState<SalesIBRuleDetail | null>(null);
  const [productCategory, setProductCategory] = useState<IBProductCategory[]>([]);
  const [defaultLevelSetting, setDefaultLevelSetting] = useState<IBDefaultLevelSettingMap>({});
  const [configLevelSetting, setConfigLevelSetting] = useState<Record<string, IBDefaultLevelSettingOption[]>>({});
  const [accountSettings, setAccountSettings] = useState<IBAccountLevelSetting[]>([]);

  useEffect(() => {
    if (!isOpen) {
      setName('');
      setLanguage('en-us');
      setServiceType(null);
      setIsAutoCreate(false);
      setErrors({});
      setAccountSettings((prev) => prev.map((a) => ({ ...a, selected: false })));
      setInitDone(false);
    }
  }, [isOpen]);

  const initData = useCallback(async () => {
    if (initDone || initLoading || !salesAccount || !ibUid) return;
    setInitLoading(true);
    try {
      const [ruleRes, catRes] = await Promise.all([
        executeRef.current(getSalesIBRebateRuleDetail, salesAccount.uid, ibUid),
        executeRef.current(getIBProductCategory),
      ]);

      if (!ruleRes.success || !ruleRes.data) return;

      const rule = ruleRes.data as unknown as SalesIBRuleDetail;
      setRuleDetail(rule);

      if (catRes.success && catRes.data) setProductCategory(catRes.data);

      const [defaultRes, configRes] = await Promise.all([
        executeRef.current(getSalesAccountDefaultLevel, salesAccount.uid, rule.agentAccountUid),
        executeRef.current(getSalesIBAccountConfig, salesAccount.uid, ibUid),
      ]);

      const defaultData = (
        defaultRes.success && defaultRes.data ? defaultRes.data : {}
      ) as unknown as IBDefaultLevelSettingMap;
      setDefaultLevelSetting(defaultData);

      let configLS: Record<string, IBDefaultLevelSettingOption[]> = {};
      if (configRes.success && configRes.data) {
        // 后端返回数组格式: [{key, value}]，与 Vue 中 getIBAccountConfiguration 相同
        const configs = configRes.data as unknown as Array<{ key: string; value: string }>;
        if (
          Array.isArray(configs) &&
          configs.length > 0 &&
          configs[0].key !== 'AutoCreateTradeAccountEnabled'
        ) {
          try {
            configLS = JSON.parse(configs[0].value);
          } catch {
            // ignore
          }
        }
      }
      setConfigLevelSetting(configLS);

      const allowedAccounts = rule.calculatedLevelSetting?.allowedAccounts || [];
      const settings: IBAccountLevelSetting[] = allowedAccounts.map((acc) => {
        const items: Record<number, number> = {};
        (acc.items || []).forEach((item) => {
          if (Object.keys(configLS).length === 0) {
            items[item.cid] = item.r;
          } else {
            const configAccount = configLS[acc.accountType];
            items[item.cid] =
              configAccount?.[0]?.category?.[item.cid] ??
              configAccount?.[0]?.Category?.[item.cid] ??
              item.r;
          }
        });
        return {
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
      setAccountSettings(settings);
      setInitDone(true);
    } finally {
      setInitLoading(false);
    }
  }, [salesAccount, ibUid, initDone, initLoading]);

  useEffect(() => {
    if (isOpen && salesAccount && ibUid && !initDone) {
      initData();
    }
  }, [isOpen, salesAccount, ibUid, initDone, initData]);

  const toggleAccountSelected = (accountType: number) => {
    setAccountSettings((prev) =>
      prev.map((a) => (a.accountType === accountType ? { ...a, selected: !a.selected } : a))
    );
    setErrors((prev) => {
      const next = { ...prev };
      delete next.account;
      return next;
    });
  };

  const filteredAccounts = useMemo(
    () => accountSettings.filter((a) => a.accountType !== 11),
    [accountSettings]
  );

  const selectedAccounts = useMemo(
    () => accountSettings.filter((a) => a.selected),
    [accountSettings]
  );

  const validate = (): boolean => {
    const errs: Record<string, string> = {};
    if (!name.trim()) errs.name = t('addLink.nameRequired');
    if (!language) errs.language = t('addLink.languageRequired');
    if (!serviceType) errs.serviceType = t('addLink.serviceTypeRequired');
    if (selectedAccounts.length === 0) errs.account = t('addLink.mustSelectAccount');
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate() || !salesAccount || !ruleDetail) return;
    setSubmitting(true);
    try {
      const allowAccountRequest: Record<string, unknown>[] = [];
      const agentAccountUid = ruleDetail.agentAccountUid;

      if (serviceType === 300) {
        for (const acc of selectedAccounts) {
          const collector = formCollectors.current[acc.accountType];
          if (collector) allowAccountRequest.push(collector());
        }
      } else if (serviceType === 400) {
        if (ruleDetail.isRoot) {
          for (const acc of selectedAccounts) {
            const collector = pcFormCollectors.current[acc.accountType];
            if (collector) allowAccountRequest.push(collector());
          }
        } else {
          for (const acc of selectedAccounts) {
            allowAccountRequest.push({
              optionName: acc.optionName,
              accountType: acc.accountType,
              pips: acc.pips,
              commission: acc.commission,
            });
          }
        }
      }

      if (allowAccountRequest.length === 0) {
        setErrors((prev) => ({ ...prev, account: t('addLink.mustSelectAccount') }));
        setSubmitting(false);
        return;
      }

      let result;
      if (serviceType === 300) {
        result = await executeRef.current(
          createSalesIBAgentLink,
          salesAccount.uid,
          agentAccountUid,
          {
            name,
            language,
            schema: allowAccountRequest,
            isAutoCreatePaymentMethod: isAutoCreate ? 1 : 0,
          }
        );
      } else {
        result = await executeRef.current(
          createSalesIBClientLink,
          salesAccount.uid,
          agentAccountUid,
          {
            name,
            language,
            allowAccountTypes: allowAccountRequest,
            isAutoCreatePaymentMethod: isAutoCreate ? 1 : 0,
          }
        );
      }

      if (result.success) {
        onSuccess();
        onClose();
      }
    } finally {
      setSubmitting(false);
    }
  };

  const isAgent = serviceType === 300;
  const isClient = serviceType === 400;
  const isMLM = ruleDetail?.calculatedLevelSetting?.distributionType === 3;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="p-6! h-[700px]! overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>
            {t('addLink.title')}
            {userName ? ` - ${userName}` : ''}
          </DialogTitle>
          <DialogDescription className="sr-only">{t('addLink.title')}</DialogDescription>
        </DialogHeader>

        <div className="flex-1 overflow-y-auto mt-4">
          {initLoading ? (
            <div className="flex flex-col gap-6 animate-pulse">
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-2">
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-10 w-full rounded bg-surface-secondary" />
                </div>
                <div className="flex flex-col gap-2">
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-10 w-full rounded bg-surface-secondary" />
                </div>
              </div>
              <div className="flex flex-col gap-2">
                <div className="h-4 w-32 rounded bg-surface-secondary" />
                <div className="flex gap-6">
                  <div className="h-5 w-12 rounded bg-surface-secondary" />
                  <div className="h-5 w-12 rounded bg-surface-secondary" />
                </div>
              </div>
              {[1, 2].map((i) => (
                <div
                  key={i}
                  className="flex items-center justify-between rounded-lg border border-border px-4 py-3"
                >
                  <div className="flex items-center gap-3">
                    <div className="h-5 w-5 rounded-full bg-surface-secondary" />
                    <div className="h-4 w-16 rounded bg-surface-secondary" />
                  </div>
                </div>
              ))}
            </div>
          ) : isMLM ? (
            <div className="flex flex-col items-center justify-center gap-3 py-11 text-center">
              <p className="text-base text-text-primary">{t('addLink.isMLM')}</p>
              <p className="text-sm text-text-secondary">{t('addLink.referralCodeNotAvailable')}</p>
            </div>
          ) : (
            <div className="flex flex-col gap-6">
              {/* 链接名称 + 语言 */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="mb-1.5 block text-sm text-text-secondary">
                    <span className="text-primary">*</span> {t('addLink.nameYourLink')}
                  </label>
                  <Input
                    className="w-full"
                    placeholder={t('addLink.pleaseInput')}
                    value={name}
                    onChange={(e) => {
                      setName(e.target.value);
                      if (errors.name)
                        setErrors((prev) => {
                          const n = { ...prev };
                          delete n.name;
                          return n;
                        });
                    }}
                  />
                  {errors.name && <p className="mt-1 text-xs text-primary">{errors.name}</p>}
                </div>
                <div>
                  <label className="mb-1.5 block text-sm text-text-secondary">
                    <span className="text-primary">*</span> {t('addLink.chooseLanguage')}
                  </label>
                  <SimpleSelect
                    options={LINK_LANGUAGE_OPTIONS}
                    value={language}
                    onChange={setLanguage}
                    placeholder={t('addLink.chooseLanguage')}
                    triggerSize="sm"
                  />
                  {errors.language && (
                    <p className="mt-1 text-xs text-primary">{errors.language}</p>
                  )}
                </div>
              </div>

              {/* 服务类型 */}
              <div>
                <label className="mb-2 block text-sm text-text-secondary">
                  <span className="text-primary">*</span>{' '}
                  {t('addLink.selectAccountTypeUnderLink')}
                </label>
                <div className="flex items-center gap-6">
                  <label
                    className="flex items-center gap-2 text-sm text-text-primary cursor-pointer"
                    onClick={() => {
                      setServiceType(300);
                      setErrors((prev) => {
                        const n = { ...prev };
                        delete n.serviceType;
                        return n;
                      });
                    }}
                  >
                    <span
                      className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${isAgent ? 'border-primary' : 'border-border'}`}
                    >
                      {isAgent && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                    </span>
                    {t('addLink.ib')}
                  </label>
                  <label
                    className="flex items-center gap-2 text-sm text-text-primary cursor-pointer"
                    onClick={() => {
                      setServiceType(400);
                      setErrors((prev) => {
                        const n = { ...prev };
                        delete n.serviceType;
                        return n;
                      });
                    }}
                  >
                    <span
                      className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${isClient ? 'border-primary' : 'border-border'}`}
                    >
                      {isClient && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                    </span>
                    {t('addLink.client')}
                  </label>
                </div>
                {errors.serviceType && (
                  <p className="mt-1 text-xs text-primary">{errors.serviceType}</p>
                )}
              </div>

              {/* 账户类型选择 */}
              {serviceType !== null && (
                <div>
                  <label className="mb-2 block text-sm text-text-secondary">
                    <span className="text-primary">*</span>
                    {t('addLink.selectAccountTypeAndSetRebate')}
                  </label>
                  {errors.account && (
                    <p className="mb-2 text-xs text-primary">{errors.account}</p>
                  )}

                  {isClient ? (
                    <div className="flex flex-col gap-3">
                      {filteredAccounts.map((acc) => (
                        <div
                          key={acc.accountType}
                          className="flex cursor-pointer items-center justify-between rounded-lg border border-border px-4 py-3"
                          onClick={() => toggleAccountSelected(acc.accountType)}
                        >
                          <div className="flex items-center gap-3">
                            <span
                              className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${acc.selected ? 'border-primary' : 'border-border'}`}
                            >
                              {acc.selected && (
                                <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                              )}
                            </span>
                            <span className="text-sm font-medium text-text-primary">
                              {tAccount(`accountTypes.${acc.accountType}`)}
                            </span>
                            {acc.optionName && (
                              <span
                                className="rounded-md px-2 py-0.5 text-xs font-semibold"
                                style={{ background: 'rgba(88,168,255,0.1)', color: '#4196f0' }}
                              >
                                {acc.optionName === 'alpha'
                                  ? tAccount(`accountTypes.4`)
                                  : acc.optionName}
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-4 text-sm text-text-secondary">
                            <span>
                              {t('addLink.pips')} &ge;{acc.pips ?? 0}
                            </span>
                            <span className="text-border">|</span>
                            <span>
                              {t('addLink.commission')} &ge;{acc.commission ?? 0}
                            </span>
                          </div>
                        </div>
                      ))}

                      {ruleDetail?.isRoot &&
                        selectedAccounts.map((acc) => (
                          <ClientPCForm
                            key={acc.accountType}
                            account={acc}
                            onRegister={registerPCFormCollector}
                            t={t}
                            tType={tType}
                            tAccount={tAccount}
                          />
                        ))}
                    </div>
                  ) : (
                    <>
                      <div className="flex flex-col gap-3">
                        {filteredAccounts.map((acc) => (
                          <div
                            key={acc.accountType}
                            className="flex cursor-pointer items-center justify-between rounded-lg border border-border px-4 py-3"
                            onClick={() => toggleAccountSelected(acc.accountType)}
                          >
                            <div className="flex items-center gap-3">
                              <span
                                className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${acc.selected ? 'border-primary' : 'border-border'}`}
                              >
                                {acc.selected && (
                                  <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                                )}
                              </span>
                              <span className="text-sm font-medium text-text-primary">
                                {tAccount(`accountTypes.${acc.accountType}`)}
                              </span>
                              {acc.optionName && (
                                <span
                                  className="rounded-md px-2 py-0.5 text-xs font-semibold"
                                  style={{ background: 'rgba(88,168,255,0.1)', color: '#4196f0' }}
                                >
                                  {acc.optionName === 'alpha'
                                    ? tAccount(`accountTypes.4`)
                                    : acc.optionName}
                                </span>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>

                      {selectedAccounts.map((acc) => (
                        <AgentRebateTable
                          key={acc.accountType}
                          account={acc}
                          productCategory={productCategory}
                          defaultLevelSetting={defaultLevelSetting}
                          isRoot={ruleDetail?.isRoot ?? false}
                          configLevelSetting={configLevelSetting}
                          onRegister={registerFormCollector}
                          t={t}
                          tType={tType}
                          tAccount={tAccount}
                        />
                      ))}
                    </>
                  )}
                </div>
              )}

              {/* 自动创建账户 */}
              <div>
                <label className="mb-2 block text-sm text-text-secondary">
                  <span className="text-primary">*</span>{' '}
                  {t('addLink.enableAutoCreateAccount')}
                </label>
                <div className="flex items-center gap-3">
                  <Switch checked={isAutoCreate} onChange={setIsAutoCreate} />
                  <span className="text-sm text-text-primary">
                    {isAutoCreate ? t('addLink.yes') : t('addLink.no')}
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>

        <DialogFooter className="mt-6">
          <div className="flex justify-end gap-3 md:gap-5">
            <Button variant="outline" onClick={onClose} className="w-auto min-w-20 md:w-[120px]">
              {t('addLink.close')}
            </Button>
            {!isMLM && (
              <Button
                onClick={handleSubmit}
                loading={submitting}
                disabled={initLoading}
                className="w-auto min-w-20 md:w-[120px]"
              >
                {t('addLink.submit')}
              </Button>
            )}
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
