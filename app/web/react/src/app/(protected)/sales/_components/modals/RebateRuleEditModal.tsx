'use client';

import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, Skeleton } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import {
  getSalesSymbolCategory,
  getSalesAccountDefaultLevel,
  getSalesDefaultLevelSetting,
  getSalesAvailableAccountTypes,
  getSalesIBRebateRuleDetail,
  updateSalesIBRebateRule,
  updateSalesTopIBRebateRule,
} from '@/actions';
import { AccountRoleTypes } from '@/types/accounts';
import type { SalesClientAccount } from '@/types/sales';

/* eslint-disable @typescript-eslint/no-explicit-any */

interface RebateRuleEditModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
  context: {
    parentRole: number;
    parentUid: number;
    editUid: number;
  } | null;
  onSuccess?: () => void;
}

function camelCaseKeys(obj: any): any {
  if (Array.isArray(obj)) return obj.map(camelCaseKeys);
  if (typeof obj === 'object' && obj !== null) {
    return Object.keys(obj).reduce((acc: any, key) => {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      acc[camelKey] = camelCaseKeys(obj[key]);
      return acc;
    }, {});
  }
  return obj;
}

export function RebateRuleEditModal({
  open,
  onOpenChange,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  account,
  context,
  onSuccess,
}: RebateRuleEditModalProps) {
  const t = useTranslations('sales');
  const tType = useTranslations('sales.type');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const { showToast } = useToast();

  const isTopAgent = context?.parentRole === AccountRoleTypes.Sales;

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [rebateAgentRuleId, setRebateAgentRuleId] = useState(0);
  const [productCategories, setProductCategories] = useState<
    { key: number; value: string }[]
  >([]);

  // Sales 模式专用状态
  const [filledAccountSchema, setFilledAccountSchema] = useState<
    Record<number, any>
  >({});
  const [salesDefaultLevel, setSalesDefaultLevel] = useState<
    Record<number, any>
  >({});
  const [editRebateRuleMap, setEditRebateRuleMap] = useState<
    Record<number, any>
  >({});

  // IB 模式专用状态
  const [currentAccountLevelSetting, setCurrentAccountLevelSetting] = useState<
    Record<number, any>
  >({});
  const [defaultLevelSetting, setDefaultLevelSetting] = useState<any>({});
  const [parentIsRoot, setParentIsRoot] = useState(false);
  const [formTables, setFormTables] = useState<Record<number, any[]>>({});
  const [percentages, setPercentages] = useState<Record<number, number>>({});
  const [pcSelections, setPcSelections] = useState<Record<number, any>>({});
  const [editRebateRuleItems, setEditRebateRuleItems] = useState<
    Record<number, Record<number, number>>
  >({});

  const mountedRef = useRef(false);
  const productCategoryNameMap = useMemo(() => {
    try {
      return t.raw('type.productCategory') as Record<string, string>;
    } catch {
      return {};
    }
  }, [t]);

  // =============================================
  // Sales 模式初始化 (对应 SalesEditTopAgentForm.vue)
  // =============================================
  const initSalesMode = useCallback(async () => {
    if (!salesAccount || !context) return;

    const [catResult, availableResult, editRuleResult, defaultResult] =
      await Promise.all([
        execute(getSalesSymbolCategory),
        execute(getSalesAvailableAccountTypes, salesAccount.uid),
        execute(getSalesIBRebateRuleDetail, salesAccount.uid, context.editUid),
        execute(getSalesDefaultLevelSetting, salesAccount.uid),
      ]);

    const categories =
      catResult.success && catResult.data
        ? (catResult.data as any[]).map((c: any) => ({
            key: c.id ?? c.key,
            value: c.name ?? c.value,
          }))
        : [];
    setProductCategories(categories);

    const availableAccounts: number[] = availableResult.success
      ? ((availableResult.data as number[]) || [])
      : [];

    const rawEditRule = editRuleResult.success ? editRuleResult.data : null;
    if ((rawEditRule as any)?.id)
      setRebateAgentRuleId((rawEditRule as any).id);

    const editMap: Record<number, any> = {};
    const levelSettingAccounts =
      (rawEditRule as any)?.levelSetting?.allowedAccounts || [];
    for (const item of levelSettingAccounts) {
      editMap[item.accountType] = item;
    }
    setEditRebateRuleMap(editMap);

    const rawDefault = defaultResult.success
      ? camelCaseKeys(defaultResult.data)
      : {};
    setSalesDefaultLevel(rawDefault);

    const schema: Record<number, any> = {};

    for (const accountType of availableAccounts) {
      if (rawDefault[accountType] === undefined) continue;

      const editAccountRule = editMap[accountType];
      const acct: any = { accountType };

      if (editAccountRule) {
        acct.selected = true;
        acct.defaultSelected = true;
        acct.allowPips = [...(editAccountRule.allowPips || [])];
        acct.allowCommissions = [...(editAccountRule.allowCommissions || [])];

        const index = rawDefault[accountType].findIndex(
          (item: any) => item.optionName === editAccountRule?.optionName
        );
        const defaultAccount =
          rawDefault[accountType][index >= 0 ? index : 0];
        acct.selectedDefaultRebateOptions = index >= 0 ? index : 0;
        acct.optionName =
          index === -1 ? null : (defaultAccount?.optionName ?? null);
        acct.allowPipOptions =
          index === -1
            ? editAccountRule?.allowPips
            : (defaultAccount?.allowPipOptions || []);
        acct.allowCommissionOptions =
          index === -1
            ? editAccountRule?.allowCommissions
            : (defaultAccount?.allowCommissionOptions || []);
        acct.items = categories.map((cat) => ({
          cid: cat.key,
          r:
            index === -1
              ? (editAccountRule?.items?.find(
                  (i: any) => i.cid === cat.key
                )?.r ?? 0)
              : (defaultAccount?.category?.[cat.key] ?? 0),
        }));
        acct.defaultRebateOptions = rawDefault[accountType];
      } else {
        acct.selected = false;
        acct.defaultSelected = false;
        acct.allowPips = [];
        acct.allowCommissions = [];

        const defaultAccount = rawDefault[accountType][0];
        acct.selectedDefaultRebateOptions = 0;
        acct.optionName = defaultAccount?.optionName ?? null;
        acct.allowPipOptions = defaultAccount?.allowPipOptions || [];
        acct.allowCommissionOptions =
          defaultAccount?.allowCommissionOptions || [];
        acct.items = categories.map((cat) => ({
          cid: cat.key,
          r: defaultAccount?.category?.[cat.key] ?? 0,
        }));
        acct.defaultRebateOptions = rawDefault[accountType];
      }

      acct.defaultAllowPips = [...acct.allowPips];
      acct.defaultAllowCommissions = [...acct.allowCommissions];

      schema[accountType] = acct;
    }
    console.log('schema', schema);
    setFilledAccountSchema(schema);
  }, [salesAccount, context, execute]);

  // =============================================
  // IB 模式初始化 (对应 EditAgentRuleForm.vue + BaseRebateForm.vue)
  // =============================================
  const initIBMode = useCallback(async () => {
    if (!salesAccount || !context) return;

    const [catResult, defaultResult, editRuleResult, parentRuleResult] =
      await Promise.all([
        execute(getSalesSymbolCategory),
        execute(
          getSalesAccountDefaultLevel,
          salesAccount.uid,
          context.parentUid
        ),
        execute(
          getSalesIBRebateRuleDetail,
          salesAccount.uid,
          context.editUid
        ),
        execute(
          getSalesIBRebateRuleDetail,
          salesAccount.uid,
          context.parentUid
        ),
      ]);

    const categories =
      catResult.success && catResult.data
        ? (catResult.data as any[]).map((c: any) => ({
            key: c.id ?? c.key,
            value: c.name ?? c.value,
          }))
        : [];
    setProductCategories(categories);

    const rawDefaultLevel = defaultResult.success
      ? camelCaseKeys(defaultResult.data)
      : {};
    setDefaultLevelSetting(rawDefaultLevel);

    const rawEditRule = editRuleResult.success ? editRuleResult.data : null;
    if ((rawEditRule as any)?.id)
      setRebateAgentRuleId((rawEditRule as any).id);

    const editSchemaMap: Record<number, any> = {};
    if ((rawEditRule as any)?.schema) {
      for (const item of (rawEditRule as any).schema) {
        editSchemaMap[item.accountType] = item;
      }
    }

    const rawParentRule = parentRuleResult.success
      ? parentRuleResult.data
      : null;
    setParentIsRoot(!!(rawParentRule as any)?.isRoot);

    const allowedAccounts =
      (rawParentRule as any)?.calculatedLevelSetting?.allowedAccounts || [];
    const settings: Record<number, any> = {};
    const tables: Record<number, any[]> = {};
    const pcts: Record<number, number> = {};
    const pcSels: Record<number, any> = {};
    const editItemsMap: Record<number, Record<number, number>> = {};

    for (const acct of allowedAccounts) {
      const at = acct.accountType;
      const hasEdit = !!editSchemaMap[at];

      const setting: any = {
        accountType: at,
        optionName: acct.optionName,
        selected: hasEdit,
        defaultSelected: hasEdit,
        allowPips: acct.allowPips || [],
        allowCommissions: acct.allowCommissions || [],
        percentage: editSchemaMap[at]?.percentage ?? 100,
        pips: editSchemaMap[at]?.pips ?? null,
        commission: editSchemaMap[at]?.commission ?? null,
        items: {} as Record<number, number>,
      };

      if (acct.items) {
        for (const item of acct.items) {
          setting.items[item.cid] = item.r;
        }
      }
      settings[at] = setting;

      let dl = JSON.parse(JSON.stringify(rawDefaultLevel));
      if (dl[at]?.length > 1 && acct.optionName != null) {
        dl =
          dl[at].find((a: any) => a.optionName === acct.optionName) ||
          dl[at][0];
      } else if (dl[at]) {
        dl = dl[at][0] || dl;
      }

      const editItems: Record<number, number> = {};
      if (editSchemaMap[at]?.items) {
        for (const item of editSchemaMap[at].items) {
          editItems[item.cid] = item.r;
        }
      }
      editItemsMap[at] = editItems;

      const rows = categories.map((cat) => ({
        cid: cat.key,
        name: cat.value,
        total: setting.items[cat.key] ?? dl?.category?.[cat.key] ?? 0,
        r: hasEdit ? (editItems[cat.key] ?? 0) : 0,
      }));
      tables[at] = rows;
      pcts[at] = editSchemaMap[at]?.percentage ?? 100;

      const pcSel: any = {
        schema: {},
        pcDropdown: false,
        optionDropdown: false,
        selectedPC: '',
        pcValue: null,
      };

      if (hasEdit) {
        const es = editSchemaMap[at];
        if (
          es.pips != null &&
          (es.commission === 0 || es.commission == null)
        ) {
          pcSel.selectedPC = 'pips';
          pcSel.pcValue = es.pips;
          pcSel.schema = dl?.allowPipSetting?.[es.pips]?.items || {};
        } else if (
          es.commission != null &&
          (es.pips === 0 || es.pips == null)
        ) {
          pcSel.selectedPC = 'commission';
          pcSel.pcValue = es.commission;
          pcSel.schema =
            dl?.allowCommissionSetting?.[es.commission]?.items || {};
        } else if (es.pips === 0 && es.commission === 0) {
          pcSel.selectedPC = 'pips';
          pcSel.pcValue = es.pips;
          pcSel.schema = dl?.allowPipSetting?.[es.pips]?.items || {};
        }
      } else {
        if ((acct.allowPips || []).length > 0) {
          pcSel.selectedPC = 'pips';
          pcSel.pcValue = acct.allowPips[0];
          pcSel.schema =
            dl?.allowPipSetting?.[acct.allowPips[0]]?.items || {};
        } else if ((acct.allowCommissions || []).length > 0) {
          pcSel.selectedPC = 'commission';
          pcSel.pcValue = acct.allowCommissions[0];
          pcSel.schema =
            dl?.allowCommissionSetting?.[acct.allowCommissions[0]]?.items ||
            {};
        }
      }
      pcSels[at] = pcSel;
    }

    setCurrentAccountLevelSetting(settings);
    setFormTables(tables);
    setPercentages(pcts);
    setPcSelections(pcSels);
    setEditRebateRuleItems(editItemsMap);
  }, [salesAccount, context, execute]);

  // =============================================
  // 初始化入口
  // =============================================
  useEffect(() => {
    if (open && !mountedRef.current && context) {
      mountedRef.current = true;
      setIsLoading(true);
      const initFn =
        context.parentRole === AccountRoleTypes.Sales
          ? initSalesMode
          : initIBMode;
      initFn().finally(() => setIsLoading(false));
    }
    if (!open) {
      mountedRef.current = false;
    }
  }, [open, context, initSalesMode, initIBMode]);

  // =============================================
  // Sales 模式: 切换 optionName 方案
  // =============================================
  const setAccountRule = useCallback(
    (accountType: number, index: number) => {
      setFilledAccountSchema((prev) => {
        const acct = { ...prev[accountType] };
        const editAccountRule = editRebateRuleMap[accountType];
        const defaultAccount = salesDefaultLevel[accountType]?.[index];

        acct.selectedDefaultRebateOptions = index;
        acct.optionName =
          index === -1 ? null : (defaultAccount?.optionName ?? null);
        acct.allowPipOptions =
          index === -1
            ? editAccountRule?.allowPips
            : (defaultAccount?.allowPipOptions || []);
        acct.allowCommissionOptions =
          index === -1
            ? editAccountRule?.allowCommissions
            : (defaultAccount?.allowCommissionOptions || []);
        acct.items = productCategories.map((cat) => ({
          cid: cat.key,
          r:
            index === -1
              ? (editAccountRule?.items?.find(
                  (i: any) => i.cid === cat.key
                )?.r ?? 0)
              : (defaultAccount?.category?.[cat.key] ?? 0),
        }));

        return { ...prev, [accountType]: acct };
      });
    },
    [editRebateRuleMap, salesDefaultLevel, productCategories]
  );

  // =============================================
  // IB 模式: 切换 PC 类型 (pips/commission)
  // =============================================
  const findDLForAccount = useCallback(
    (at: number) => {
      const setting = currentAccountLevelSetting[at];
      let dl = JSON.parse(JSON.stringify(defaultLevelSetting));
      if (dl[at]?.length > 1 && setting?.optionName != null) {
        dl =
          dl[at].find((a: any) => a.optionName === setting.optionName) ||
          dl[at][0];
      } else if (dl[at]) {
        dl = dl[at][0] || dl;
      }
      return dl;
    },
    [currentAccountLevelSetting, defaultLevelSetting]
  );

  const selectPC = useCallback(
    (accountType: number, pc: 'pips' | 'commission') => {
      setFormTables((prev) => {
        const rows = (prev[accountType] || []).map((r: any) => ({
          ...r,
          r: 0,
        }));
        return { ...prev, [accountType]: rows };
      });

      setPcSelections((prev) => {
        const sel = { ...prev[accountType] };
        sel.selectedPC = pc;
        sel.pcDropdown = false;

        const setting = currentAccountLevelSetting[accountType];
        const dl = findDLForAccount(accountType);

        if (pc === 'pips') {
          sel.pcValue = setting?.allowPips?.[0] ?? null;
          sel.schema =
            dl?.allowPipSetting?.[setting?.allowPips?.[0]]?.items || {};
        } else {
          sel.pcValue = setting?.allowCommissions?.[0] ?? null;
          sel.schema =
            dl?.allowCommissionSetting?.[setting?.allowCommissions?.[0]]
              ?.items || {};
        }

        return { ...prev, [accountType]: sel };
      });
    },
    [currentAccountLevelSetting, findDLForAccount]
  );

  const selectVal = useCallback(
    (accountType: number, val: number) => {
      setPcSelections((prev) => {
        const sel = { ...prev[accountType] };
        sel.pcValue = val;
        sel.optionDropdown = false;

        const dl = findDLForAccount(accountType);

        if (sel.selectedPC === 'pips') {
          sel.schema = dl?.allowPipSetting?.[val]?.items || {};
        } else {
          sel.schema = dl?.allowCommissionSetting?.[val]?.items || {};
        }

        return { ...prev, [accountType]: sel };
      });
    },
    [findDLForAccount]
  );

  // =============================================
  // IB 模式: 切换账户类型 checkbox
  // =============================================
  const toggleIBAccountSelected = (at: number) => {
    setCurrentAccountLevelSetting((prev) => {
      const item = prev[at];
      if (!item || item.defaultSelected) return prev;
      return { ...prev, [at]: { ...item, selected: !item.selected } };
    });
  };

  // =============================================
  // IB 模式: 更新单元格值
  // =============================================
  const updateFormRowValue = (at: number, cidIdx: number, value: number) => {
    setFormTables((prev) => {
      const rows = [...(prev[at] || [])];
      rows[cidIdx] = { ...rows[cidIdx], r: value };
      return { ...prev, [at]: rows };
    });
  };

  // =============================================
  // IB 模式: 百分比自动填充
  // =============================================
  const applyPersonCentage = (at: number, pct: number) => {
    if (pct > 0) {
      setFormTables((prev) => {
        const rows = (prev[at] || []).map((r: any) => ({
          ...r,
          r: Number((r.total * (pct / 100)).toFixed(1)),
        }));
        return { ...prev, [at]: rows };
      });
    }
  };

  // =============================================
  // 提交
  // =============================================
  const handleSubmit = async () => {
    if (!salesAccount || !context) return;
    setIsSubmitting(true);

    try {
      if (isTopAgent) {
        // Sales 模式: putTopIBRebateRule(editUid, ruleId, { schema: [...] })
        const schema: any[] = [];
        for (const acct of Object.values(filledAccountSchema)) {
          if (acct.selected) {
            schema.push({
              accountType: acct.accountType,
              optionName: acct.optionName,
              items: acct.items,
              allowPips: acct.allowPips,
              allowCommissions: acct.allowCommissions,
              pips: null,
              commission: null,
              percentage: 100,
            });
          }
        }
        const result = await execute(
          updateSalesTopIBRebateRule,
          salesAccount.uid,
          context.editUid,
          rebateAgentRuleId,
          { schema } as any
        );
        if (result.success) {
          showToast({ message: 'Update Success', type: 'success' });
          onOpenChange(false);
          onSuccess?.();
        }
      } else {
        // IB 模式: putIBRebateRule(parentUid, ruleId, formData[])
        const formData: any[] = [];
        for (const [atStr, setting] of Object.entries(
          currentAccountLevelSetting
        )) {
          if (!setting.selected) continue;
          const at = Number(atStr);
          const rows = formTables[at] || [];
          const pcSel = pcSelections[at];

          let pips: number | null = parentIsRoot ? null : setting.pips;
          let commission: number | null = parentIsRoot
            ? null
            : setting.commission;

          if (pcSel?.selectedPC === 'pips') {
            pips = pcSel.pcValue === 0 ? null : pcSel.pcValue;
          } else {
            commission = pcSel.pcValue === 0 ? null : pcSel.pcValue;
          }

          formData.push({
            optionName: setting.optionName,
            accountType: setting.accountType,
            pips,
            commission,
            percentage:
              percentages[at] === undefined || percentages[at] === null
                ? 0
                : percentages[at],
            items: rows.map((row: any) => ({ cid: row.cid, r: row.r })),
          });
        }

        const result = await execute(
          updateSalesIBRebateRule,
          salesAccount.uid,
          context.parentUid,
          rebateAgentRuleId,
          formData as any
        );
        if (result.success) {
          showToast({ message: 'Update Success', type: 'success' });
          onOpenChange(false);
          onSuccess?.();
        }
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // =============================================
  // Sales 模式渲染 (对应 SalesEditTopAgentForm.vue)
  // =============================================
  const renderSalesForm = () => (
    <div className="mt-4">
      <div className="relative space-y-4">
        {Object.values(filledAccountSchema).map((acct: any) => (
          <div
            key={acct.accountType}
            className="rounded-lg p-4 shadow-[0_1px_4px_rgba(0,0,0,0.16)]"
          >
            {acct.selected &&
              (acct.defaultRebateOptions || []).length > 1 && (
                <div className="mb-2 flex justify-end gap-2">
                  {acct.defaultRebateOptions.map(
                    (option: any, idx: number) => (
                      <button
                        key={idx}
                        className={`rounded border-0 px-3 py-1 text-sm ${
                          acct.selectedDefaultRebateOptions === idx
                            ? 'bg-primary/20 font-medium text-primary'
                            : 'bg-surface-secondary text-text-secondary hover:bg-surface-hover'
                        }`}
                        onClick={() =>
                          setAccountRule(acct.accountType, idx)
                        }
                      >
                        {option.optionName}
                      </button>
                    )
                  )}
                </div>
              )}

            <div className="flex items-center gap-3">
              <input
                type="checkbox"
                checked={acct.selected}
                disabled={acct.defaultSelected}
                onChange={() => {
                  setFilledAccountSchema((prev) => ({
                    ...prev,
                    [acct.accountType]: {
                      ...prev[acct.accountType],
                      selected: !acct.selected,
                    },
                  }));
                }}
                className="h-4 w-4 rounded border-border"
              />
              <span className="text-base">
                {tType.has(`account.${acct.accountType}`)
                  ? tType(`account.${acct.accountType}`)
                  : String(acct.accountType)}
              </span>
            </div>

            {acct.selected && (
              <>
                <hr className="my-3 border-border" />
                <div className="grid grid-cols-2 gap-2 lg:grid-cols-6">
                  {(acct.items || []).map((item: any) => {
                    const rawCatName =
                      productCategories.find((c) => c.key === item.cid)
                        ?.value ?? String(item.cid);
                    const catName =
                      productCategoryNameMap[rawCatName] ??
                      productCategoryNameMap[
                        rawCatName.replace(/\./g, '_')
                      ] ??
                      rawCatName;
                    return (
                      <div key={item.cid} className="mb-1">
                        <div className="text-xs text-text-secondary">
                          {catName}
                        </div>
                        <input
                          type="text"
                          value={item.r}
                          disabled
                          className="input-field mt-1 h-9 w-full rounded px-2 text-sm opacity-60"
                        />
                      </div>
                    );
                  })}
                </div>

                {(acct.allowPipOptions || []).length > 0 && (
                  <div className="mb-2 mt-5">
                    <label className="text-sm font-medium">
                      {t('rebateEdit.availablePips')}
                    </label>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {acct.allowPipOptions.map((p: number) => {
                        const checked = acct.allowPips.includes(p);
                        const disabled =
                          acct.defaultAllowPips.includes(p);
                        return (
                          <label
                            key={`p_${p}`}
                            className={`flex cursor-pointer items-center gap-1.5 rounded border px-3 py-1.5 text-sm ${
                              checked
                                ? 'border-primary bg-primary/10'
                                : 'border-border'
                            } ${disabled ? 'cursor-not-allowed opacity-60' : ''}`}
                          >
                            <input
                              type="checkbox"
                              checked={checked}
                              disabled={disabled}
                              onChange={() => {
                                setFilledAccountSchema((prev) => {
                                  const a = {
                                    ...prev[acct.accountType],
                                  };
                                  if (checked) {
                                    a.allowPips = a.allowPips.filter(
                                      (v: number) => v !== p
                                    );
                                  } else {
                                    a.allowPips = [...a.allowPips, p];
                                  }
                                  return {
                                    ...prev,
                                    [acct.accountType]: a,
                                  };
                                });
                              }}
                              className="h-3.5 w-3.5 rounded border-border"
                            />
                            {tType.has(`pipOptions.${p}`)
                              ? tType(`pipOptions.${p}`)
                              : String(p)}
                          </label>
                        );
                      })}
                    </div>
                  </div>
                )}

                {(acct.allowCommissionOptions || []).length > 0 && (
                  <div className="mb-3 mt-3">
                    <label className="text-sm font-medium">
                      {t('rebateEdit.availableCommission')}
                    </label>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {acct.allowCommissionOptions.map((c: number) => {
                        const checked =
                          acct.allowCommissions.includes(c);
                        const disabled =
                          acct.defaultAllowCommissions.includes(c);
                        return (
                          <label
                            key={`c_${c}`}
                            className={`flex cursor-pointer items-center gap-1.5 rounded border px-3 py-1.5 text-sm ${
                              checked
                                ? 'border-primary bg-primary/10'
                                : 'border-border'
                            } ${disabled ? 'cursor-not-allowed opacity-60' : ''}`}
                          >
                            <input
                              type="checkbox"
                              checked={checked}
                              disabled={disabled}
                              onChange={() => {
                                setFilledAccountSchema((prev) => {
                                  const a = {
                                    ...prev[acct.accountType],
                                  };
                                  if (checked) {
                                    a.allowCommissions =
                                      a.allowCommissions.filter(
                                        (v: number) => v !== c
                                      );
                                  } else {
                                    a.allowCommissions = [
                                      ...a.allowCommissions,
                                      c,
                                    ];
                                  }
                                  return {
                                    ...prev,
                                    [acct.accountType]: a,
                                  };
                                });
                              }}
                              className="h-3.5 w-3.5 rounded border-border"
                            />
                            {tType.has(`commissionOptions.${c}`)
                              ? tType(`commissionOptions.${c}`)
                              : String(c)}
                          </label>
                        );
                      })}
                    </div>
                  </div>
                )}
              </>
            )}
          </div>
        ))}
      </div>
    </div>
  );

  // =============================================
  // IB 模式: 单个账户类型区域渲染 (对应 BaseRebateForm.vue)
  // =============================================
  const renderIBAccountSection = (at: number) => {
    const setting = currentAccountLevelSetting[at];
    if (!setting?.selected) return null;

    const rows = formTables[at] || [];
    const editMode = setting.defaultSelected;
    const pcSel = pcSelections[at];
    const showPCColumn =
      (setting.allowPips.length > 0 ||
        setting.allowCommissions.length > 0) &&
      parentIsRoot;
    const showPercentageColumn =
      (setting.percentage !== 0 && (setting.pips || setting.commission)) ||
      (parentIsRoot &&
        (setting.allowPips.length > 0 ||
          setting.allowCommissions.length > 0));

    return (
      <div key={at} className="mb-6">
        <div className="mb-3 flex items-center gap-2">
          <div className="h-4 w-[3px] bg-[#800020]" />
          <span className="text-base font-medium">
            {tType.has(`account.${at}`)
              ? tType(`account.${at}`)
              : String(at)}
          </span>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[600px] border-collapse text-center text-sm">
            <thead>
              <tr>
                <td className="whitespace-nowrap bg-[#f8f9fa] px-3 py-3 font-semibold">
                  {t('rebateEdit.category')}
                </td>
                <td className="whitespace-nowrap bg-[#f8f9fa] px-3 py-3 font-semibold">
                  {t('rebateEdit.totalRebate')}
                </td>
                <td className="whitespace-nowrap bg-[#f8f9fa] px-3 py-3 font-semibold">
                  <div className="flex flex-col items-center">
                    <span>{t('rebateEdit.personalRebate')}</span>
                    <span className="mt-1">
                      <input
                        type="number"
                        step={1}
                        min={0}
                        max={100}
                        className="input-field w-14 rounded px-1 py-0.5 text-center text-xs"
                        placeholder="0"
                        title={t('rebateEdit.setPercentageTooltip')}
                        onChange={(e) => {
                          const val = parseInt(e.target.value) || 0;
                          applyPersonCentage(at, val);
                        }}
                      />
                      <span className="ml-1">%</span>
                    </span>
                  </div>
                </td>
                <td className="whitespace-nowrap bg-[#f8f9fa] px-3 py-3 font-semibold">
                  {t('rebateEdit.remainRebate')}
                </td>

                {showPCColumn && (
                  <td className="min-w-[140px] bg-[#0053ad] px-3 py-3 font-semibold text-white">
                    {editMode ? (
                      <>
                        <div className="mb-2 flex items-center justify-center">
                          <span>
                            {pcSel?.selectedPC
                              ? t(`rebateEdit.${pcSel.selectedPC}`)
                              : t('rebateEdit.none')}
                          </span>
                        </div>
                        <div className="flex items-center justify-center">
                          <span>
                            {t('rebateEdit.options')} (
                            {pcSel?.pcValue ?? '--'})
                          </span>
                        </div>
                      </>
                    ) : (
                      <>
                        <div className="relative mb-2">
                          <div
                            className="flex cursor-pointer items-center justify-center gap-1"
                            onClick={() => {
                              setPcSelections((prev) => ({
                                ...prev,
                                [at]: {
                                  ...prev[at],
                                  pcDropdown: !prev[at].pcDropdown,
                                  optionDropdown: false,
                                },
                              }));
                            }}
                          >
                            <span>
                              {t(
                                `rebateEdit.${pcSel?.selectedPC || 'pips'}`
                              )}
                            </span>
                            <i className="fa-solid fa-angle-down ml-2" />
                          </div>
                          {pcSel?.pcDropdown && (
                            <div
                              className="absolute right-2 z-50 mt-1 overflow-hidden rounded-lg bg-white text-black shadow-lg"
                              style={{ width: 172 }}
                            >
                              {setting.allowPips.length > 0 && (
                                <div
                                  className="flex h-10 cursor-pointer items-center justify-center border border-[#f5f7fa] hover:bg-[#f5f7fa]"
                                  onClick={() => selectPC(at, 'pips')}
                                >
                                  {t('rebateEdit.pips')}
                                </div>
                              )}
                              {setting.allowCommissions.length > 0 && (
                                <div
                                  className="flex h-10 cursor-pointer items-center justify-center border border-[#f5f7fa] hover:bg-[#f5f7fa]"
                                  onClick={() =>
                                    selectPC(at, 'commission')
                                  }
                                >
                                  {t('rebateEdit.commission')}
                                </div>
                              )}
                            </div>
                          )}
                        </div>

                        <div className="relative">
                          <div
                            className="flex cursor-pointer items-center justify-center gap-1"
                            onClick={() => {
                              setPcSelections((prev) => ({
                                ...prev,
                                [at]: {
                                  ...prev[at],
                                  optionDropdown:
                                    !prev[at].optionDropdown,
                                  pcDropdown: false,
                                },
                              }));
                            }}
                          >
                            <span>
                              {t('rebateEdit.options')} (
                              {pcSel?.pcValue ?? '--'})
                            </span>
                            <i className="fa-solid fa-angle-down ml-2" />
                          </div>
                          {pcSel?.optionDropdown && (
                            <div
                              className="absolute right-2 z-50 mt-1 overflow-hidden rounded-lg bg-white text-black shadow-lg"
                              style={{ width: 172 }}
                            >
                              {pcSel.selectedPC === 'pips'
                                ? (setting.allowPips || []).map(
                                    (p: number) => (
                                      <div
                                        key={p}
                                        className="flex h-10 cursor-pointer items-center justify-center border border-[#f5f7fa] hover:bg-[#f5f7fa]"
                                        onClick={() =>
                                          selectVal(at, p)
                                        }
                                      >
                                        {tType.has(`pipOptions.${p}`)
                                          ? tType(`pipOptions.${p}`)
                                          : String(p)}
                                      </div>
                                    )
                                  )
                                : (setting.allowCommissions || []).map(
                                    (c: number) => (
                                      <div
                                        key={c}
                                        className="flex h-10 cursor-pointer items-center justify-center border border-[#f5f7fa] hover:bg-[#f5f7fa]"
                                        onClick={() =>
                                          selectVal(at, c)
                                        }
                                      >
                                        {tType.has(
                                          `commissionOptions.${c}`
                                        )
                                          ? tType(
                                              `commissionOptions.${c}`
                                            )
                                          : String(c)}
                                      </div>
                                    )
                                  )}
                            </div>
                          )}
                        </div>
                      </>
                    )}
                  </td>
                )}

                {showPercentageColumn && (
                  <td className="whitespace-nowrap bg-[#f8f9fa] px-3 py-3 font-semibold">
                    {t('rebateEdit.addRate')} {t('rebateEdit.percentage')}
                  </td>
                )}
              </tr>
            </thead>
            <tbody>
              {rows.map((row: any, idx: number) => {
                const maxVal = editMode
                  ? (editRebateRuleItems[at]?.[row.cid] ?? row.total)
                  : row.total;
                const remain =
                  row.total < row.r
                    ? 0
                    : Number((row.total - row.r).toFixed(1));

                return (
                  <tr
                    key={row.cid}
                    className="border-b border-dashed border-[#e4e6ef]"
                  >
                    <td className="px-3 py-3">{row.name}</td>
                    <td className="px-3 py-3">{row.total}</td>
                    <td className="px-3 py-3">
                      <input
                        type="number"
                        step="0.1"
                        min={0}
                        max={maxVal}
                        value={row.r}
                        onChange={(e) => {
                          let val = parseFloat(e.target.value);
                          if (isNaN(val)) val = 0;
                          if (val > maxVal) val = maxVal;
                          if (val < 0) val = 0;
                          updateFormRowValue(
                            at,
                            idx,
                            Number(val.toFixed(1))
                          );
                        }}
                        className="input-field w-24 rounded px-2 py-1 text-center"
                      />
                    </td>
                    <td className="px-3 py-3">{remain}</td>
                    {showPCColumn && (
                      <td className="w-[150px] px-3 py-3">
                        {pcSel?.schema?.[row.cid] ?? '-'}
                      </td>
                    )}
                    {showPercentageColumn && idx === 0 && (
                      <td
                        className="border-l border-[#f5f5f5] px-3 py-3"
                        rowSpan={rows.length}
                      >
                        <div className="flex items-center justify-center gap-1 p-2">
                          <input
                            type="number"
                            step={1}
                            min={0}
                            max={100}
                            value={percentages[at] ?? 100}
                            onChange={(e) => {
                              let val = parseInt(e.target.value);
                              if (isNaN(val)) val = 0;
                              if (val > 100) val = 100;
                              if (val < 0) val = 0;
                              if (editMode) {
                                const maxPct =
                                  setting.percentage ?? 100;
                                if (val > maxPct) val = maxPct;
                              }
                              setPercentages((prev) => ({
                                ...prev,
                                [at]: val,
                              }));
                            }}
                            className="input-field w-16 rounded px-2 py-1 text-center"
                          />
                          <span>%</span>
                        </div>
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
  };

  // =============================================
  // IB 模式渲染 (对应 EditAgentRuleForm.vue)
  // =============================================
  const renderIBForm = () => (
    <div className="mt-4">
      <div className="mb-4 flex flex-wrap gap-4">
        {Object.values(currentAccountLevelSetting).map((setting: any) => (
          <label
            key={setting.accountType}
            className="flex cursor-pointer items-center gap-2"
          >
            <input
              type="checkbox"
              checked={setting.selected}
              disabled={setting.defaultSelected}
              onChange={() => toggleIBAccountSelected(setting.accountType)}
              className="h-4 w-4 rounded border-border"
            />
            <span className="text-sm">
              {tType.has(`account.${setting.accountType}`)
                ? tType(`account.${setting.accountType}`)
                : String(setting.accountType)}
            </span>
          </label>
        ))}
      </div>

      {Object.keys(currentAccountLevelSetting).map((atStr) =>
        renderIBAccountSection(Number(atStr))
      )}
    </div>
  );

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>{t('rebateEdit.title')}</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4 py-4">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-40 w-full" />
          </div>
        ) : (
          <div className="max-h-[70vh] overflow-auto">
            <div className="mb-5 rounded-lg bg-[#ffecec] px-4 py-3 text-sm text-[#9f005b]">
              <div>
                <span>{t('rebateEdit.existRule')}: </span>
                <span className="mr-3">
                  [A] {t('rebateEdit.editRule1')}
                </span>
                <span className="mr-3">
                  [B] {t('rebateEdit.editRule2')}
                </span>
              </div>
              <div>
                <span>{t('rebateEdit.newRule')}: </span>
                <span>{t('rebateEdit.editRule5')}</span>
              </div>
            </div>

            {isTopAgent ? renderSalesForm() : renderIBForm()}

            <div className="flex justify-center pb-4 pt-5">
              <Button
                variant="primary"
                size="md"
                loading={isSubmitting}
                onClick={handleSubmit}
                className="w-60"
              >
                {t('rebateEdit.updateRebateRule')}
              </Button>
            </div>
          </div>
        )}
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
  );
}
