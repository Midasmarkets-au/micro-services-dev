'use client';

import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  Button,
  Input,
  Switch,
} from '@/components/ui';
import { SearchableSelect } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import {
  getIBRebateRuleDetailFull,
  getIBProductCategory,
  getIBDefaultLevelSettingMap,
  getIBAccountsWithConfig,
  createIBLinkForIB,
  createIBLinkForClient,
} from '@/actions';
import type {
  IBRebateRuleDetailFull,
  IBProductCategory,
  IBDefaultLevelSettingMap,
  IBDefaultLevelSettingOption,
  IBAccountLevelSetting,
} from '@/types/ib';
import { LINK_LANGUAGE_OPTIONS } from '@/core/types/LanguageTypes';

interface AddLinkDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  agentUid: number;
}

interface RebateFormRow {
  cid: number;
  name: string;
  total: number;
  r: number;
}

function AgentRebateTable({
  account,
  productCategory,
  defaultLevelSetting,
  isRoot,
  configLevelSetting,
  onRegister,
  t,
  tAccount,
}: {
  account: IBAccountLevelSetting;
  productCategory: IBProductCategory[];
  defaultLevelSetting: IBDefaultLevelSettingMap;
  isRoot: boolean;
  configLevelSetting: Record<string, IBDefaultLevelSettingOption[]>;
  onRegister: (accountType: number, collectData: () => Record<string, unknown>) => void;
  t: ReturnType<typeof useTranslations>;
  tAccount: ReturnType<typeof useTranslations>;
}) {
  const [formRows, setFormRows] = useState<RebateFormRow[]>([]);
  const [percentage, setPercentage] = useState(100);
  const [pcSelection, setPcSelection] = useState<{
    selectedPC: string;
    pcValue: number | null;
    schema: Record<number, number> | null;
  }>({ selectedPC: '', pcValue: null, schema: null });
  const [batchPercent, setBatchPercent] = useState(0);

  const resolvedDefault = useMemo(() => {
    const raw = defaultLevelSetting[account.accountType];
    if (!raw) return null;
    if (raw.length > 1 && account.optionName) {
      return raw.find(o => (o.optionName ?? o.OptionName) === account.optionName) ?? raw[0];
    }
    return raw[0];
  }, [defaultLevelSetting, account.accountType, account.optionName]);

  useEffect(() => {
    if (!productCategory.length) return;
    const rows: RebateFormRow[] = productCategory.map(cat => ({
      cid: cat.key,
      name: cat.value,
      total: account.items[cat.key] ?? resolvedDefault?.category?.[cat.key] ?? resolvedDefault?.Category?.[cat.key] ?? 0,
      r: 0,
    }));
    setFormRows(rows);

    if (isRoot && (account.allowPips.length > 0 || account.allowCommissions.length > 0)) {
      if (account.allowPips.length > 0) {
        const firstPip = account.allowPips[0];
        setPcSelection({
          selectedPC: 'pips',
          pcValue: firstPip,
          schema: resolvedDefault?.allowPipSetting?.[firstPip]?.items ?? null,
        });
      } else {
        const firstComm = account.allowCommissions[0];
        setPcSelection({
          selectedPC: 'commission',
          pcValue: firstComm,
          schema: resolvedDefault?.allowCommissionSetting?.[firstComm]?.items ?? null,
        });
      }
    }
  }, [productCategory, account, resolvedDefault, isRoot]);

  useEffect(() => {
    if (batchPercent > 0) {
      setFormRows(prev => prev.map(row => ({
        ...row,
        r: Number((row.total * (batchPercent / 100)).toFixed(1)),
      })));
    }
  }, [batchPercent]);

  const handleRChange = (cid: number, val: number) => {
    setFormRows(prev => prev.map(row => row.cid === cid ? { ...row, r: val } : row));
  };

  const handlePCTypeChange = (type: string) => {
    setFormRows(prev => prev.map(row => ({ ...row, r: 0 })));
    if (type === 'pips' && account.allowPips.length > 0) {
      const v = account.allowPips[0];
      setPcSelection({
        selectedPC: 'pips',
        pcValue: v,
        schema: resolvedDefault?.allowPipSetting?.[v]?.items ?? null,
      });
    } else if (account.allowCommissions.length > 0) {
      const v = account.allowCommissions[0];
      setPcSelection({
        selectedPC: 'commission',
        pcValue: v,
        schema: resolvedDefault?.allowCommissionSetting?.[v]?.items ?? null,
      });
    }
  };

  const handlePCValueChange = (val: number) => {
    if (pcSelection.selectedPC === 'pips') {
      setPcSelection(prev => ({ ...prev, pcValue: val, schema: resolvedDefault?.allowPipSetting?.[val]?.items ?? null }));
    } else {
      setPcSelection(prev => ({ ...prev, pcValue: val, schema: resolvedDefault?.allowCommissionSetting?.[val]?.items ?? null }));
    }
  };

  const hasPCColumn = isRoot && (account.allowPips.length > 0 || account.allowCommissions.length > 0);
  const hasPercentageColumn = (account.percentage !== 0 && (account.pips || account.commission)) || hasPCColumn;

  const collectData = useCallback((): Record<string, unknown> => {
    let _p = isRoot ? null : account.pips ?? null;
    let _c = isRoot ? null : account.commission ?? null;
    if (pcSelection.selectedPC === 'pips') {
      _p = pcSelection.pcValue === 0 ? null : pcSelection.pcValue;
    } else if (pcSelection.selectedPC === 'commission') {
      _c = pcSelection.pcValue === 0 ? null : pcSelection.pcValue;
    }
    return {
      optionName: account.optionName,
      accountType: account.accountType,
      pips: _p,
      commission: _c,
      percentage: percentage || 0,
      items: formRows.map(r => ({ cid: r.cid, r: r.r })),
    };
  }, [account, isRoot, pcSelection, percentage, formRows]);

  useEffect(() => {
    onRegister(account.accountType, collectData);
  }, [account.accountType, collectData, onRegister]);

  const calculate = (a: number, b: number) => {
    if (b > a) return 0;
    return Number((a - b).toFixed(1));
  };

  return (
    <div className="mt-4">
      <div className="flex items-center gap-2 mb-3">
        <div className="h-4 w-0.5 bg-primary" />
        <span className="text-base font-medium text-text-primary">
          {tAccount(`accountTypes.${account.accountType}`)}
        </span>
        {Object.keys(configLevelSetting).length > 0 &&
          configLevelSetting[account.accountType]?.length > 1 && (
            <div className="ml-auto flex gap-2">
              {configLevelSetting[account.accountType].map((opt, idx) => (
                <Button key={idx} variant="outline" size="xs" onClick={() => {
                  const cat = opt.category ?? opt.Category ?? {};
                  setFormRows(prev => prev.map(row => ({
                    ...row,
                    total: cat[row.cid] ?? row.total,
                  })));
                }}>
                  {opt.OptionName ?? opt.optionName}
                </Button>
              ))}
            </div>
          )}
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[600px] text-sm border-collapse">
          <thead>
            <tr className="text-center">
              <th className="bg-surface-secondary px-3 py-2 font-semibold text-text-primary">
                {t('addLink.category')}
              </th>
              <th className="bg-surface-secondary px-3 py-2 font-semibold text-text-primary">
                {t('addLink.totalRebate')}
              </th>
              <th className="bg-surface-secondary px-3 py-2 font-semibold text-text-primary">
                <div className="flex flex-col items-center gap-1">
                  <span>{t('addLink.personalRebate')}</span>
                  <div className="flex items-center gap-1">
                    <input
                      type="number"
                      className="w-14 rounded border border-border bg-input-bg px-2 py-0.5 text-center text-xs"
                      min={0}
                      max={100}
                      value={batchPercent}
                      onChange={e => setBatchPercent(Math.min(100, Math.max(0, Number(e.target.value))))}
                    />
                    <span className="text-xs">%</span>
                  </div>
                </div>
              </th>
              <th className="bg-surface-secondary px-3 py-2 font-semibold text-text-primary">
                {t('addLink.remainRebate')}
              </th>
              {hasPCColumn && (
                <th className="bg-[#0053ad] px-3 py-2 font-semibold text-white min-w-[140px]">
                  <div className="flex flex-col items-center gap-2">
                    <select
                      className="rounded border-0 bg-white/20 px-2 py-0.5 text-xs text-white"
                      value={pcSelection.selectedPC}
                      onChange={e => handlePCTypeChange(e.target.value)}
                    >
                      {account.allowPips.length > 0 && <option value="pips">{t('addLink.pips')}</option>}
                      {account.allowCommissions.length > 0 && <option value="commission">{t('addLink.commission')}</option>}
                    </select>
                    <select
                      className="rounded border-0 bg-white/20 px-2 py-0.5 text-xs text-white"
                      value={pcSelection.pcValue ?? ''}
                      onChange={e => handlePCValueChange(Number(e.target.value))}
                    >
                      {(pcSelection.selectedPC === 'pips' ? account.allowPips : account.allowCommissions).map(v => (
                        <option key={v} value={v}>{v}</option>
                      ))}
                    </select>
                  </div>
                </th>
              )}
              {hasPercentageColumn && (
                <th className="bg-surface-secondary px-3 py-2 font-semibold text-text-primary">
                  {t('addLink.addRatePercentage')}
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {formRows.map((row, idx) => (
              <tr key={row.cid} className="text-center border-b border-dashed border-border">
                <td className="px-3 py-2 text-text-primary">{row.name}</td>
                <td className="px-3 py-2 text-text-primary">{row.total}</td>
                <td className="px-3 py-2">
                  <input
                    type="number"
                    className="w-20 rounded border border-border bg-input-bg px-2 py-1 text-center text-sm"
                    min={0}
                    max={row.total}
                    step={0.1}
                    value={row.r}
                    onChange={e => handleRChange(row.cid, Math.min(row.total, Math.max(0, Number(e.target.value))))}
                  />
                </td>
                <td className="px-3 py-2 text-text-primary">
                  {calculate(row.total, row.r)}
                </td>
                {hasPCColumn && (
                  <td className="px-3 py-2 text-text-primary">
                    {pcSelection.schema?.[row.cid] ?? '-'}
                  </td>
                )}
                {hasPercentageColumn && idx === 0 && (
                  <td className="px-3 py-2 border-l border-border" rowSpan={formRows.length}>
                    <div className="flex items-center justify-center gap-1">
                      <input
                        type="number"
                        className="w-14 rounded border border-border bg-input-bg px-2 py-1 text-center text-sm"
                        min={0}
                        max={100}
                        value={percentage}
                        onChange={e => setPercentage(Math.min(100, Math.max(0, Number(e.target.value))))}
                      />
                      <span>%</span>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ClientPCForm({
  account,
  onRegister,
  t,
  tAccount,
}: {
  account: IBAccountLevelSetting;
  onRegister: (accountType: number, collectData: () => Record<string, unknown>) => void;
  t: ReturnType<typeof useTranslations>;
  tAccount: ReturnType<typeof useTranslations>;
}) {
  const [selectedPC, setSelectedPC] = useState('');
  const [pcValue, setPcValue] = useState<number | null>(null);

  useEffect(() => {
    if (account.allowPips.length > 0) {
      setSelectedPC('pips');
      setPcValue(account.allowPips[0]);
    } else if (account.allowCommissions.length > 0) {
      setSelectedPC('commission');
      setPcValue(account.allowCommissions[0]);
    }
  }, [account]);

  useEffect(() => {
    if (selectedPC === 'pips' && account.allowPips.length > 0) {
      setPcValue(account.allowPips[0]);
    } else if (selectedPC === 'commission' && account.allowCommissions.length > 0) {
      setPcValue(account.allowCommissions[0]);
    }
  }, [selectedPC, account]);

  const collectData = useCallback((): Record<string, unknown> => ({
    optionName: account.optionName,
    accountType: account.accountType,
    pips: selectedPC === 'pips' ? pcValue : null,
    commission: selectedPC === 'commission' ? pcValue : null,
  }), [account, selectedPC, pcValue]);

  useEffect(() => {
    onRegister(account.accountType, collectData);
  }, [account.accountType, collectData, onRegister]);

  if (account.allowPips.length === 0 && account.allowCommissions.length === 0) {
    return (
      <div className="mt-3 rounded-full bg-[#ffecec] px-4 py-1.5 text-center text-sm text-[#9f005b]">
        {t('addLink.noPcNeed')}
      </div>
    );
  }

  return (
    <div className="mt-3">
      <div className="flex items-center gap-2 mb-3">
        <div className="h-4 w-0.5 bg-primary" />
        <span className="text-base font-medium text-text-primary">
          {tAccount(`accountTypes.${account.accountType}`)}
        </span>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="mb-1 block text-sm text-text-secondary">{t('addLink.choosePipCommission')}</label>
          <select
            className="input-field w-full"
            value={selectedPC}
            onChange={e => setSelectedPC(e.target.value)}
          >
            {account.allowPips.length > 0 && <option value="pips">{t('addLink.pips')}</option>}
            {account.allowCommissions.length > 0 && <option value="commission">{t('addLink.commission')}</option>}
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm text-text-secondary">{t('addLink.choosePipCommissionValue')}</label>
          <select
            className="input-field w-full"
            value={pcValue ?? ''}
            onChange={e => setPcValue(Number(e.target.value))}
          >
            {(selectedPC === 'pips' ? account.allowPips : account.allowCommissions).map(v => (
              <option key={v} value={v}>{v}</option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
}

export function AddLinkDialog({ isOpen, onClose, onSuccess, agentUid }: AddLinkDialogProps) {
  const t = useTranslations('ib');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const executeRef = useRef(execute);
  executeRef.current = execute;

  const [name, setName] = useState('');
  const [language, setLanguage] = useState('en-us');
  const [serviceType, setServiceType] = useState<number | null>(null);
  const [isAutoCreate, setIsAutoCreate] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const formCollectors = useRef<Record<number, () => Record<string, unknown>>>({});
  const pcFormCollectors = useRef<Record<number, () => Record<string, unknown>>>({});

  const registerFormCollector = useCallback((accountType: number, collectData: () => Record<string, unknown>) => {
    formCollectors.current[accountType] = collectData;
  }, []);

  const registerPCFormCollector = useCallback((accountType: number, collectData: () => Record<string, unknown>) => {
    pcFormCollectors.current[accountType] = collectData;
  }, []);

  const [ruleDetail, setRuleDetail] = useState<IBRebateRuleDetailFull | null>(null);
  const [productCategory, setProductCategory] = useState<IBProductCategory[]>([]);
  const [defaultLevelSetting, setDefaultLevelSetting] = useState<IBDefaultLevelSettingMap>({});
  const [configLevelSetting, setConfigLevelSetting] = useState<Record<string, IBDefaultLevelSettingOption[]>>({});
  const [accountSettings, setAccountSettings] = useState<IBAccountLevelSetting[]>([]);
  const [initLoading, setInitLoading] = useState(false);
  const [initDone, setInitDone] = useState(false);

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (!isOpen) {
      setName('');
      setLanguage('en-us');
      setServiceType(null);
      setIsAutoCreate(false);
      setErrors({});
      setAccountSettings(prev => prev.map(a => ({ ...a, selected: false })));
      setInitDone(false);
    }
  }, [isOpen]);

  const initData = useCallback(async () => {
    if (initDone || initLoading) return;
    setInitLoading(true);
    try {
      const [ruleRes, catRes, defaultRes, accountsRes] = await Promise.all([
        executeRef.current(getIBRebateRuleDetailFull, agentUid),
        executeRef.current(getIBProductCategory),
        executeRef.current(getIBDefaultLevelSettingMap, agentUid),
        executeRef.current(getIBAccountsWithConfig),
      ]);

      if (ruleRes.success && ruleRes.data) setRuleDetail(ruleRes.data);
      if (catRes.success && catRes.data) setProductCategory(catRes.data);

      const defaultData = defaultRes.success && defaultRes.data ? defaultRes.data : {};
      setDefaultLevelSetting(defaultData);

      let configLS: Record<string, IBDefaultLevelSettingOption[]> = {};
      if (accountsRes.success && accountsRes.data) {
        const accounts = accountsRes.data.data || [];
        const myAccount = accounts.find(acc => acc.uid === agentUid);
        const configItem = myAccount?.configurations?.find(c => c.key === 'DefaultRebateLevelSetting');
        if (configItem) {
          try { configLS = JSON.parse(configItem.value); } catch { /* ignore */ }
        }
      }
      setConfigLevelSetting(configLS);

      if (ruleRes.success && ruleRes.data) {
        const allowedAccounts = ruleRes.data.calculatedLevelSetting?.allowedAccounts || [];
        const settings: IBAccountLevelSetting[] = allowedAccounts.map(acc => {
          const items: Record<number, number> = {};
          (acc.items || []).forEach(item => {
            if (Object.keys(configLS).length === 0) {
              items[item.cid] = item.r;
            } else {
              const configAccount = configLS[acc.accountType];
              items[item.cid] = configAccount?.[0]?.category?.[item.cid]
                ?? configAccount?.[0]?.Category?.[item.cid]
                ?? item.r;
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
      }

      setInitDone(true);
    } finally {
      setInitLoading(false);
    }
  }, [agentUid, initDone, initLoading]);

  useEffect(() => {
    if (isOpen && agentUid && !initDone) {
      initData();
    }
  }, [isOpen, agentUid, initDone, initData]);

  const toggleAccountSelected = (accountType: number) => {
    setAccountSettings(prev => prev.map(a =>
      a.accountType === accountType ? { ...a, selected: !a.selected } : a
    ));
    setErrors(prev => {
      const next = { ...prev };
      delete next.account;
      return next;
    });
  };

  const filteredAccounts = useMemo(() =>
    accountSettings.filter(a => a.accountType !== 11),
    [accountSettings]
  );

  const selectedAccounts = useMemo(() =>
    accountSettings.filter(a => a.selected),
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
    if (!validate()) return;
    setSubmitting(true);
    try {
      const allowAccountRequest: Record<string, unknown>[] = [];

      if (serviceType === 300) {
        for (const acc of selectedAccounts) {
          const collector = formCollectors.current[acc.accountType];
          if (collector) allowAccountRequest.push(collector());
        }
      } else if (serviceType === 400) {
        if (ruleDetail?.isRoot) {
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
        setErrors(prev => ({ ...prev, account: t('addLink.mustSelectAccount') }));
        setSubmitting(false);
        return;
      }

      let result;
      if (serviceType === 300) {
        result = await executeRef.current(createIBLinkForIB, agentUid, {
          name,
          language,
          schema: allowAccountRequest,
          isAutoCreatePaymentMethod: isAutoCreate ? 1 : 0,
        } as Record<string, unknown>);
      } else {
        result = await executeRef.current(createIBLinkForClient, agentUid, {
          name,
          language,
          allowAccountTypes: allowAccountRequest,
          isAutoCreatePaymentMethod: isAutoCreate ? 1 : 0,
        } as Record<string, unknown>);
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

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="p-6! max-h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>{t('addLink.title')}</DialogTitle>
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
              <div className="flex flex-col gap-2">
                <div className="h-4 w-24 rounded bg-surface-secondary" />
                {[1, 2].map(i => (
                  <div key={i} className="flex items-center justify-between rounded-lg border border-border px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="h-5 w-5 rounded-full bg-surface-secondary" />
                      <div className="h-4 w-16 rounded bg-surface-secondary" />
                      <div className="h-5 w-16 rounded-md bg-surface-secondary" />
                    </div>
                    <div className="flex gap-4">
                      <div className="h-4 w-16 rounded bg-surface-secondary" />
                      <div className="h-4 w-16 rounded bg-surface-secondary" />
                    </div>
                  </div>
                ))}
              </div>
              <div className="flex flex-col gap-2">
                <div className="h-4 w-28 rounded bg-surface-secondary" />
                <div className="h-5 w-8 rounded-full bg-surface-secondary" />
              </div>
            </div>
          ) : (
            <div className="flex flex-col gap-6">
              {/* Row 1: Link name + Language (two columns) */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="mb-1.5 block text-sm text-text-secondary">
                    <span className="text-primary">*</span> {t('addLink.nameYourLink')}
                  </label>
                  <Input
                    className="w-full"
                    placeholder={t('addLink.pleaseInput')}
                    value={name}
                    onChange={e => {
                      setName(e.target.value);
                      if (errors.name) setErrors(prev => { const n = { ...prev }; delete n.name; return n; });
                    }}
                  />
                  {errors.name && <p className="mt-1 text-xs text-primary">{errors.name}</p>}
                </div>
                <div>
                  <label className="mb-1.5 block text-sm text-text-secondary">
                    <span className="text-primary">*</span> {t('addLink.chooseLanguage')}
                  </label>
                  <SearchableSelect
                    options={LINK_LANGUAGE_OPTIONS}
                    value={language}
                    onChange={val => setLanguage(val as string)}
                    placeholder={t('addLink.chooseLanguage')}
                  />
                  {errors.language && <p className="mt-1 text-xs text-primary">{errors.language}</p>}
                </div>
              </div>

              {/* Row 2: Service type (Agent / Client) */}
              <div>
                <label className="mb-2 block text-sm text-text-secondary">
                  <span className="text-primary">*</span> {t('addLink.selectAccountTypeUnderLink')}
                </label>
                <div className="flex items-center gap-6">
                  <label className="flex items-center gap-2 text-sm text-text-primary cursor-pointer" onClick={() => {
                    setServiceType(300);
                    setErrors(prev => { const n = { ...prev }; delete n.serviceType; return n; });
                  }}>
                    <span className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${isAgent ? 'border-primary' : 'border-border'}`}>
                      {isAgent && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                    </span>
                    {t('addLink.ib')}
                  </label>
                  <label className="flex items-center gap-2 text-sm text-text-primary cursor-pointer" onClick={() => {
                    setServiceType(400);
                    setErrors(prev => { const n = { ...prev }; delete n.serviceType; return n; });
                  }}>
                    <span className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${isClient ? 'border-primary' : 'border-border'}`}>
                      {isClient && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                    </span>
                    {t('addLink.client')}
                  </label>
                </div>
                {errors.serviceType && <p className="mt-1 text-xs text-primary">{errors.serviceType}</p>}
              </div>

              {/* Row 3: Account type selection */}
              {serviceType !== null && (
                <div>
                  <label className="mb-2 block text-sm text-text-secondary">
                    <span className="text-primary">*</span>{t('addLink.selectAccountTypeAndSetRebate')}
                  </label>
                  {errors.account && <p className="mb-2 text-xs text-primary">{errors.account}</p>}

                  {isClient ? (
                    <div className="flex flex-col gap-3">
                      {filteredAccounts.map(acc => (
                        <div
                          key={acc.accountType}
                          className="flex cursor-pointer items-center justify-between rounded-lg border border-border px-4 py-3"
                          onClick={() => toggleAccountSelected(acc.accountType)}
                        >
                          <div className="flex items-center gap-3">
                            <span className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${acc.selected ? 'border-primary' : 'border-border'}`}>
                              {acc.selected && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                            </span>
                            <span className="text-sm font-medium text-text-primary">
                              {tAccount(`accountTypes.${acc.accountType}`)}
                            </span>
                            {acc.optionName && (
                              <span
                                className="rounded-md px-2 py-0.5 text-xs font-semibold"
                                style={{ background: 'rgba(88,168,255,0.1)', color: '#4196f0' }}
                              >
                                {acc.optionName === 'alpha' ? 'Standard' : acc.optionName}
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-4 text-sm text-text-secondary">
                            <span>{t('addLink.pips')} &ge;{acc.pips ?? 0}</span>
                            <span className="text-border">|</span>
                            <span>{t('addLink.commission')} &ge;{acc.commission ?? 0}</span>
                          </div>
                        </div>
                      ))}

                      {ruleDetail?.isRoot && selectedAccounts.map(acc => (
                        <ClientPCForm
                          key={acc.accountType}
                          account={acc}
                          onRegister={registerPCFormCollector}
                          t={t}
                          tAccount={tAccount}
                        />
                      ))}
                    </div>
                  ) : (
                    <>
                      <div className="flex flex-col gap-3">
                        {filteredAccounts.map(acc => (
                          <div
                            key={acc.accountType}
                            className="flex cursor-pointer items-center justify-between rounded-lg border border-border px-4 py-3"
                            onClick={() => toggleAccountSelected(acc.accountType)}
                          >
                            <div className="flex items-center gap-3">
                              <span className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${acc.selected ? 'border-primary' : 'border-border'}`}>
                                {acc.selected && <span className="h-2.5 w-2.5 rounded-full bg-primary" />}
                              </span>
                              <span className="text-sm font-medium text-text-primary">
                                {tAccount(`accountTypes.${acc.accountType}`)}
                              </span>
                              {acc.optionName && (
                                <span
                                  className="rounded-md px-2 py-0.5 text-xs font-semibold"
                                  style={{ background: 'rgba(88,168,255,0.1)', color: '#4196f0' }}
                                >
                                  {acc.optionName === 'alpha' ? 'Standard' : acc.optionName}
                                </span>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>

                      {selectedAccounts.map(acc => (
                        <AgentRebateTable
                          key={acc.accountType}
                          account={acc}
                          productCategory={productCategory}
                          defaultLevelSetting={defaultLevelSetting}
                          isRoot={ruleDetail?.isRoot ?? false}
                          configLevelSetting={configLevelSetting}
                          onRegister={registerFormCollector}
                          t={t}
                          tAccount={tAccount}
                        />
                      ))}
                    </>
                  )}
                </div>
              )}

              {/* Row 4: Auto create account */}
              <div>
                <label className="mb-2 block text-sm text-text-secondary">
                  <span className="text-primary">*</span> {t('addLink.enableAutoCreateAccount')}
                </label>
                <div className="flex items-center gap-3">
                  <Switch
                    checked={isAutoCreate}
                    onChange={setIsAutoCreate}
                  />
                  <span className="text-sm text-text-primary">
                    {isAutoCreate ? t('addLink.yes') : t('addLink.no')}
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="mt-6 flex justify-end gap-3 md:gap-5">
          <Button variant="secondary" onClick={onClose} className="w-auto min-w-20 md:w-[120px]">
            {t('addLink.close')}
          </Button>
          <Button
            onClick={handleSubmit}
            loading={submitting}
            disabled={initLoading}
            className="w-auto min-w-20 md:w-[120px]"
          >
            {t('addLink.submit')}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
