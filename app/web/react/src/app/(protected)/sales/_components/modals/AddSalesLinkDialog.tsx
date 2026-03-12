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
  SearchableSelect,
} from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import {
  getSalesSymbolCategory,
  getSalesDefaultLevelSetting,
  getSalesAvailableAccountTypes,
  createSalesLinkForIB,
  createSalesLinkForClient,
} from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { LINK_LANGUAGE_OPTIONS } from '@/core/types/LanguageTypes';
const SERVICE_TYPE_IB = 300;
const SERVICE_TYPE_CLIENT = 400;

interface ProductCategory {
  key: number;
  value: string;
}

interface DefaultLevelOption {
  optionName?: string;
  OptionName?: string;
  category?: Record<number, number>;
  Category?: Record<number, number>;
  allowPipOptions?: number[];
  allowCommissionOptions?: number[];
  allowPipSetting?: Record<number, { items: Record<number, number> }>;
  allowCommissionSetting?: Record<number, { items: Record<number, number> }>;
}

interface AccountFormState {
  accountType: string;
  selected: boolean;
  defaultRebateOptions: DefaultLevelOption[];
  selectedDefaultRebateOptions: number;
  items: { cid: number; r: number }[];
  allowPipOptions: number[];
  allowCommissionOptions: number[];
  allowPips: number[];
  allowCommissions: number[];
  pcSelection?: {
    rateOption: number;
    selectedPC: string;
    pcValue: string | null;
  };
}

interface AddSalesLinkDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function AddSalesLinkDialog({
  isOpen,
  onClose,
  onSuccess,
}: AddSalesLinkDialogProps) {
  const t = useTranslations('sales');
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

  const [productCategory, setProductCategory] = useState<ProductCategory[]>([]);
  const [defaultLevelSetting, setDefaultLevelSetting] = useState<
    Record<string, DefaultLevelOption[]>
  >({});
  const [availableAccounts, setAvailableAccounts] = useState<string[]>([]);
  const [schemaForm, setSchemaForm] = useState<Record<string, AccountFormState>>({});

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
      setInitDone(false);
      setSchemaForm({});
    }
  }, [isOpen]);

  const initData = useCallback(async () => {
    if (initDone || initLoading || !salesAccount) return;
    setInitLoading(true);
    try {
      const [catRes, defaultRes, accountsRes] = await Promise.all([
        executeRef.current(getSalesSymbolCategory),
        executeRef.current(getSalesDefaultLevelSetting, salesAccount.uid),
        executeRef.current(getSalesAvailableAccountTypes, salesAccount.uid),
      ]);

      let categories: ProductCategory[] = [];
      if (catRes.success && catRes.data) {
        categories = catRes.data as unknown as ProductCategory[];
        setProductCategory(categories);
      }

      let defaults: Record<string, DefaultLevelOption[]> = {};
      if (defaultRes.success && defaultRes.data) {
        defaults = processKeysToCamelCase(defaultRes.data) as Record<
          string,
          DefaultLevelOption[]
        >;
        setDefaultLevelSetting(defaults);
      }

      let accounts: string[] = [];
      if (accountsRes.success && accountsRes.data) {
        accounts = accountsRes.data as unknown as string[];
        setAvailableAccounts(accounts);
      }

      const form: Record<string, AccountFormState> = {};
      accounts.forEach((accountType) => {
        if (defaults[accountType] === undefined) return;
        const defaultOption = defaults[accountType][0];
        const items = categories.map((cat) => ({
          cid: cat.key,
          r: defaultOption?.category?.[cat.key] ?? defaultOption?.Category?.[cat.key] ?? 0,
        }));

        form[accountType] = {
          accountType,
          selected: false,
          defaultRebateOptions: defaults[accountType],
          selectedDefaultRebateOptions: 0,
          items,
          allowPipOptions: defaultOption?.allowPipOptions ?? [],
          allowCommissionOptions: defaultOption?.allowCommissionOptions ?? [],
          allowPips: [],
          allowCommissions: [],
          pcSelection: {
            rateOption: 0,
            selectedPC: 'pips',
            pcValue:
              (defaultOption?.allowPipOptions?.[0]?.toString() ?? null),
          },
        };
      });
      setSchemaForm(form);
      setInitDone(true);
    } finally {
      setInitLoading(false);
    }
  }, [salesAccount, initDone, initLoading]);

  useEffect(() => {
    if (isOpen && salesAccount && !initDone) {
      initData();
    }
  }, [isOpen, salesAccount, initDone, initData]);

  const setAccountRule = useCallback(
    (accountType: string, index: number) => {
      setSchemaForm((prev) => {
        const acc = prev[accountType];
        if (!acc) return prev;
        const option = acc.defaultRebateOptions[index];
        if (!option) return prev;
        const items = productCategory.map((cat) => ({
          cid: cat.key,
          r: option.category?.[cat.key] ?? option.Category?.[cat.key] ?? 0,
        }));
        return {
          ...prev,
          [accountType]: {
            ...acc,
            selectedDefaultRebateOptions: index,
            items,
            allowPipOptions: option.allowPipOptions ?? [],
            allowCommissionOptions: option.allowCommissionOptions ?? [],
          },
        };
      });
    },
    [productCategory]
  );

  const toggleAccountSelected = (accountType: string) => {
    setSchemaForm((prev) => ({
      ...prev,
      [accountType]: {
        ...prev[accountType],
        selected: !prev[accountType].selected,
      },
    }));
    setErrors((prev) => {
      const next = { ...prev };
      delete next.account;
      return next;
    });
  };

  const updatePcSelection = (
    accountType: string,
    field: string,
    value: string | number
  ) => {
    setSchemaForm((prev) => {
      const acc = prev[accountType];
      if (!acc) return prev;
      const pc = { ...acc.pcSelection!, [field]: value };
      if (field === 'selectedPC') {
        if (value === 'pips') {
          pc.pcValue = acc.allowPipOptions[0]?.toString() ?? null;
        } else {
          pc.pcValue = acc.allowCommissionOptions[0]?.toString() ?? null;
        }
      }
      if (field === 'rateOption') {
        setAccountRule(accountType, Number(value));
      }
      return { ...prev, [accountType]: { ...acc, pcSelection: pc } };
    });
  };

  const filteredAccounts = useMemo(
    () => Object.values(schemaForm),
    [schemaForm]
  );

  const selectedAccounts = useMemo(
    () => filteredAccounts.filter((a) => a.selected),
    [filteredAccounts]
  );

  const validate = (): boolean => {
    const errs: Record<string, string> = {};
    if (!name.trim()) errs.name = t('link.nameRequired');
    if (!language) errs.language = t('link.languageRequired');
    if (!serviceType) errs.serviceType = t('link.serviceTypeRequired');
    if (selectedAccounts.length === 0)
      errs.account = t('link.mustSelectAccount');
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate() || !salesAccount) return;
    setSubmitting(true);
    try {
      if (serviceType === SERVICE_TYPE_IB) {
        const schema = selectedAccounts.map((acc) => ({
          optionName:
            acc.defaultRebateOptions[acc.selectedDefaultRebateOptions]
              ?.optionName ??
            acc.defaultRebateOptions[acc.selectedDefaultRebateOptions]
              ?.OptionName,
          accountType: acc.accountType,
          items: acc.items,
          allowPips: acc.allowPips,
          allowCommissions: acc.allowCommissions,
          pips: null,
          commission: null,
          percentage: 0,
        }));
        const result = await execute(
          createSalesLinkForIB,
          salesAccount.uid,
          {
            name,
            language,
            schema,
            isAutoCreatePaymentMethod: isAutoCreate ? 1 : 0,
          } as Record<string, unknown>
        );
        if (result.success) {
          onSuccess();
          onClose();
        }
      } else if (serviceType === SERVICE_TYPE_CLIENT) {
        const allowAccountTypes = selectedAccounts.map((acc) => {
          const pc = acc.pcSelection;
          let pips: number | null = null;
          let commission: number | null = null;
          if (pc?.selectedPC === 'pips' && pc.pcValue) {
            pips = Number(pc.pcValue);
          } else if (pc?.selectedPC === 'commission' && pc.pcValue) {
            commission = Number(pc.pcValue);
          }
          return {
            optionName:
              acc.defaultRebateOptions[
                pc?.rateOption ?? acc.selectedDefaultRebateOptions
              ]?.optionName ??
              acc.defaultRebateOptions[
                pc?.rateOption ?? acc.selectedDefaultRebateOptions
              ]?.OptionName,
            accountType: acc.accountType,
            pips,
            commission,
          };
        });
        const result = await execute(
          createSalesLinkForClient,
          salesAccount.uid,
          {
            name,
            language,
            allowAccountTypes,
            isAutoCreatePaymentMethod: isAutoCreate ? 1 : 0,
          } as Record<string, unknown>
        );
        if (result.success) {
          onSuccess();
          onClose();
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  const isAgent = serviceType === SERVICE_TYPE_IB;
  const isClient = serviceType === SERVICE_TYPE_CLIENT;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="h-[700px]! overflow-hidden flex flex-col p-6!">
        <DialogHeader>
          <DialogTitle>{t('link.addNewLink')}</DialogTitle>
          <DialogDescription className="sr-only">
            {t('link.addNewLink')}
          </DialogDescription>
        </DialogHeader>

        <div className="mt-4 flex-1 overflow-y-auto">
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
          ) : (
            <div className="flex flex-col gap-6">
              {/* Step 1 + 2: Link name + Language */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="mb-1.5 block text-sm text-text-secondary">
                    <span className="text-primary">*</span>{' '}
                    {t('link.nameYourLink')}
                  </label>
                  <Input
                    className="w-full"
                    placeholder={t('link.pleaseInput')}
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
                  {errors.name && (
                    <p className="mt-1 text-xs text-primary">{errors.name}</p>
                  )}
                </div>
                <div>
                  <label className="mb-1.5 block text-sm text-text-secondary">
                    <span className="text-primary">*</span>{' '}
                    {t('link.chooseLanguage')}
                  </label>
                  <SearchableSelect
                    options={LINK_LANGUAGE_OPTIONS}
                    value={language}
                    onChange={(val) => setLanguage(val as string)}
                    placeholder={t('link.chooseLanguage')}
                  />
                  {errors.language && (
                    <p className="mt-1 text-xs text-primary">
                      {errors.language}
                    </p>
                  )}
                </div>
              </div>

              {/* Step 3: Service type */}
              <div>
                <label className="mb-2 block text-sm text-text-secondary">
                  <span className="text-primary">*</span>{' '}
                  {t('link.selectLinkType')}
                </label>
                <div className="flex items-center gap-6">
                  <label
                    className="flex cursor-pointer items-center gap-2 text-sm text-text-primary"
                    onClick={() => {
                      setServiceType(SERVICE_TYPE_IB);
                      setErrors((prev) => {
                        const n = { ...prev };
                        delete n.serviceType;
                        return n;
                      });
                    }}
                  >
                    <span
                      className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${
                        isAgent ? 'border-primary' : 'border-border'
                      }`}
                    >
                      {isAgent && (
                        <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                      )}
                    </span>
                    {t('link.agent')}
                  </label>
                  <label
                    className="flex cursor-pointer items-center gap-2 text-sm text-text-primary"
                    onClick={() => {
                      setServiceType(SERVICE_TYPE_CLIENT);
                      setErrors((prev) => {
                        const n = { ...prev };
                        delete n.serviceType;
                        return n;
                      });
                    }}
                  >
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
                {errors.serviceType && (
                  <p className="mt-1 text-xs text-primary">
                    {errors.serviceType}
                  </p>
                )}
              </div>

              {/* Step 4: Account type selection */}
              {serviceType !== null && (
                <div>
                  <label className="mb-2 block text-sm text-text-secondary">
                    <span className="text-primary">*</span>
                    {t('link.selectAccountTypeAndSetRebate')}
                  </label>
                  {errors.account && (
                    <p className="mb-2 text-xs text-primary">
                      {errors.account}
                    </p>
                  )}

                  <div className="flex flex-col gap-3">
                    {filteredAccounts.map((acc) => (
                      <div key={acc.accountType}>
                        <div
                          className="flex cursor-pointer items-center justify-between rounded-lg border border-border px-4 py-3"
                          onClick={() =>
                            toggleAccountSelected(acc.accountType)
                          }
                        >
                          <div className="flex items-center gap-3">
                            <span
                              className={`flex h-5 w-5 items-center justify-center rounded-full border-2 ${
                                acc.selected
                                  ? 'border-primary'
                                  : 'border-border'
                              }`}
                            >
                              {acc.selected && (
                                <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                              )}
                            </span>
                            <span className="text-sm font-medium text-text-primary">
                              {tAccount(
                                `accountTypes.${acc.accountType}`
                              )}
                            </span>
                          </div>

                          {isAgent &&
                            acc.selected &&
                            acc.defaultRebateOptions.length > 1 && (
                              <div className="flex gap-2">
                                {acc.defaultRebateOptions.map((opt, idx) => (
                                  <Button
                                    key={idx}
                                    variant={
                                      acc.selectedDefaultRebateOptions === idx
                                        ? 'primary'
                                        : 'outline'
                                    }
                                    size="xs"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      setAccountRule(acc.accountType, idx);
                                    }}
                                  >
                                    {(opt.optionName ?? opt.OptionName) ===
                                    'alpha'
                                      ? 'Standard'
                                      : (opt.optionName ?? opt.OptionName)}
                                  </Button>
                                ))}
                              </div>
                            )}
                        </div>

                        {/* IB Mode: show rebate items */}
                        {isAgent && acc.selected && (
                          <div className="mt-2 rounded-lg border border-dashed border-border p-4">
                            <div className="flex flex-wrap gap-3">
                              {acc.items.map((item) => {
                                const catName =
                                  productCategory.find(
                                    (c) => c.key === item.cid
                                  )?.value ?? String(item.cid);
                                return (
                                  <div
                                    key={item.cid}
                                    className="flex items-center gap-2 rounded bg-surface-secondary px-3 py-1.5 text-sm"
                                  >
                                    <span className="text-text-secondary">
                                      {catName}
                                    </span>
                                    <Input
                                      value={String(item.r)}
                                      disabled
                                      className="h-7 w-16 text-center"
                                    />
                                  </div>
                                );
                              })}
                            </div>

                            {acc.allowPipOptions.length > 0 && (
                              <div className="mt-3">
                                <label className="mb-1 block text-xs text-text-secondary">
                                  {t('link.pips')}
                                </label>
                                <div className="flex flex-wrap gap-2">
                                  {acc.allowPipOptions.map((p) => {
                                    const checked = acc.allowPips.includes(p);
                                    return (
                                      <label
                                        key={p}
                                        className={`flex cursor-pointer items-center gap-1.5 rounded border px-3 py-1 text-sm ${
                                          checked
                                            ? 'border-primary bg-primary/10 text-primary'
                                            : 'border-border text-text-secondary'
                                        }`}
                                        onClick={() => {
                                          setSchemaForm((prev) => {
                                            const a = prev[acc.accountType];
                                            const pips = checked
                                              ? a.allowPips.filter(
                                                  (v) => v !== p
                                                )
                                              : [...a.allowPips, p];
                                            return {
                                              ...prev,
                                              [acc.accountType]: {
                                                ...a,
                                                allowPips: pips,
                                              },
                                            };
                                          });
                                        }}
                                      >
                                        {p}
                                      </label>
                                    );
                                  })}
                                </div>
                              </div>
                            )}

                            {acc.allowCommissionOptions.length > 0 && (
                              <div className="mt-3">
                                <label className="mb-1 block text-xs text-text-secondary">
                                  {t('link.commission')}
                                </label>
                                <div className="flex flex-wrap gap-2">
                                  {acc.allowCommissionOptions.map((c) => {
                                    const checked =
                                      acc.allowCommissions.includes(c);
                                    return (
                                      <label
                                        key={c}
                                        className={`flex cursor-pointer items-center gap-1.5 rounded border px-3 py-1 text-sm ${
                                          checked
                                            ? 'border-primary bg-primary/10 text-primary'
                                            : 'border-border text-text-secondary'
                                        }`}
                                        onClick={() => {
                                          setSchemaForm((prev) => {
                                            const a = prev[acc.accountType];
                                            const comms = checked
                                              ? a.allowCommissions.filter(
                                                  (v) => v !== c
                                                )
                                              : [...a.allowCommissions, c];
                                            return {
                                              ...prev,
                                              [acc.accountType]: {
                                                ...a,
                                                allowCommissions: comms,
                                              },
                                            };
                                          });
                                        }}
                                      >
                                        {c}
                                      </label>
                                    );
                                  })}
                                </div>
                              </div>
                            )}
                          </div>
                        )}

                        {/* Client Mode: show BaseRebatePCForm equivalent */}
                        {isClient && acc.selected && (
                          <ClientPCFormInline
                            acc={acc}
                            onUpdate={updatePcSelection}
                            t={t}
                          />
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Step 5: Auto create account */}
              <div>
                <label className="mb-2 block text-sm text-text-secondary">
                  {t('link.enableAutoCreateAccount')}
                </label>
                <div className="flex items-center gap-3">
                  <Switch checked={isAutoCreate} onChange={setIsAutoCreate} />
                  <span className="text-sm text-text-primary">
                    {isAutoCreate ? t('link.yes') : t('link.no')}
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="mt-6 flex justify-end gap-3 md:gap-5">
          <Button
            variant="secondary"
            onClick={onClose}
            className="w-auto min-w-20 md:w-[120px]"
          >
            {t('link.close')}
          </Button>
          <Button
            onClick={handleSubmit}
            loading={submitting}
            disabled={initLoading}
            className="w-auto min-w-20 md:w-[120px]"
          >
            {t('link.submit')}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

function ClientPCFormInline({
  acc,
  onUpdate,
  t,
}: {
  acc: AccountFormState;
  onUpdate: (accountType: string, field: string, value: string | number) => void;
  t: ReturnType<typeof useTranslations>;
}) {
  const hasPipOrCommission =
    acc.allowPipOptions.length > 0 ||
    acc.allowCommissionOptions.length > 0 ||
    acc.defaultRebateOptions.length > 1;

  if (!hasPipOrCommission) {
    return (
      <div className="mt-3 rounded-full bg-[#ffecec] px-4 py-1.5 text-center text-sm text-[#9f005b]">
        {t('link.noPcNeed')}
      </div>
    );
  }

  const pc = acc.pcSelection!;

  return (
    <div className="mt-2 rounded-lg border border-dashed border-border p-4">
      <div className="grid grid-cols-3 gap-4">
        {acc.defaultRebateOptions.length > 1 && (
          <div>
            <label className="mb-1 block text-xs text-text-secondary">
              {t('link.rateOption')}
            </label>
            <select
              className="input-field w-full"
              value={pc.rateOption}
              onChange={(e) =>
                onUpdate(acc.accountType, 'rateOption', e.target.value)
              }
            >
              {acc.defaultRebateOptions.map((opt, idx) => (
                <option key={idx} value={idx}>
                  {opt.optionName ?? opt.OptionName}
                </option>
              ))}
            </select>
          </div>
        )}

        <div>
          <label className="mb-1 block text-xs text-text-secondary">
            {t('link.choosePipCommission')}
          </label>
          <select
            className="input-field w-full"
            value={pc.selectedPC}
            onChange={(e) =>
              onUpdate(acc.accountType, 'selectedPC', e.target.value)
            }
          >
            {acc.allowPipOptions.length > 0 && (
              <option value="pips">{t('link.pips')}</option>
            )}
            {acc.allowCommissionOptions.length > 0 && (
              <option value="commission">{t('link.commission')}</option>
            )}
          </select>
        </div>

        <div>
          <label className="mb-1 block text-xs text-text-secondary">
            {t('link.choosePipCommissionValue')}
          </label>
          {pc.selectedPC === 'pips' && acc.allowPipOptions.length > 0 ? (
            <select
              className="input-field w-full"
              value={pc.pcValue ?? ''}
              onChange={(e) =>
                onUpdate(acc.accountType, 'pcValue', e.target.value)
              }
            >
              {acc.allowPipOptions.map((v) => (
                <option key={v} value={v}>
                  {v}
                </option>
              ))}
            </select>
          ) : pc.selectedPC === 'commission' &&
            acc.allowCommissionOptions.length > 0 ? (
            <select
              className="input-field w-full"
              value={pc.pcValue ?? ''}
              onChange={(e) =>
                onUpdate(acc.accountType, 'pcValue', e.target.value)
              }
            >
              {acc.allowCommissionOptions.map((v) => (
                <option key={v} value={v}>
                  {v}
                </option>
              ))}
            </select>
          ) : (
            <select className="input-field w-full" disabled>
              <option>{t('link.noData')}</option>
            </select>
          )}
        </div>
      </div>
    </div>
  );
}

function processKeysToCamelCase(obj: unknown): unknown {
  if (obj === null || obj === undefined) return obj;
  if (Array.isArray(obj)) return obj.map(processKeysToCamelCase);
  if (typeof obj === 'object') {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(obj as Record<string, unknown>)) {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      result[camelKey] = processKeysToCamelCase(value);
      if (camelKey !== key) {
        result[key] = processKeysToCamelCase(value);
      }
    }
    return result;
  }
  return obj;
}
