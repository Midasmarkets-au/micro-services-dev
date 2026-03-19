'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/radix/Dialog';
import { Button, Skeleton } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useIBStore } from '@/stores/ibStore';
import { useToast } from '@/hooks/useToast';
import {
  getIBAccountDefaultLevel,
  getIBRebateRuleByAccount,
  updateIBRebateRule,
  getRebateSymbolCategory,
} from '@/actions';
import { AccountRoleTypes } from '@/types/accounts';
import type { IBClientAccount, IBAllowedAccount } from '@/types/ib';

/* eslint-disable @typescript-eslint/no-explicit-any */

interface AccountLevelItem {
  accountType: number;
  optionName: string | null;
  selected: boolean;
  defaultSelected: boolean;
  allowPips: number[];
  allowCommissions: number[];
  percentage: number;
  pips: number | null;
  commission: number | null;
  items: Record<number, number>;
}

interface FormRow {
  cid: number;
  name: string;
  total: number;
  r: number;
}

interface PCSelection {
  schema: Record<number, number>;
  pcDropdown: boolean;
  optionDropdown: boolean;
  selectedPC: 'pips' | 'commission' | '';
  pcValue: number | null;
}

interface RebateRuleEditModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: IBClientAccount | null;
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

export function RebateRuleEditModal({ open, onOpenChange, account, onSuccess }: RebateRuleEditModalProps) {
  const t = useTranslations('ib');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const { showToast } = useToast();

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [productCategories, setProductCategories] = useState<{ key: number; value: string }[]>([]);
  const [accountLevelSettings, setAccountLevelSettings] = useState<Record<number, AccountLevelItem>>({});
  const [formTables, setFormTables] = useState<Record<number, FormRow[]>>({});
  const [percentages, setPercentages] = useState<Record<number, number>>({});
  const [pcSelections, setPcSelections] = useState<Record<number, PCSelection>>({});
  const [defaultLevelSettings, setDefaultLevelSettings] = useState<Record<number, any>>({});
  const [editRebateRuleDetails, setEditRebateRuleDetails] = useState<Record<number, any>>({});
  const [rebateAgentRuleId, setRebateAgentRuleId] = useState(0);
  const [parentIsRoot, setParentIsRoot] = useState(false);
  const mountedRef = useRef(false);

  const initData = useCallback(async () => {
    if (!agentAccount || !account) return;
    setIsLoading(true);

    try {
      const catResult = await execute(getRebateSymbolCategory);
      const categories = catResult.success && catResult.data
        ? catResult.data.map((c: any) => ({ key: c.id ?? c.key, value: c.name ?? c.value }))
        : [];
      setProductCategories(categories);

      const [defaultLevelResult, editRuleResult, parentRuleResult] = await Promise.all([
        execute(getIBAccountDefaultLevel, agentAccount.uid, account.agentUid ?? account.uid),
        execute(getIBRebateRuleByAccount, agentAccount.uid, account.uid),
        execute(getIBRebateRuleByAccount, agentAccount.uid, account.agentUid ?? account.uid),
      ]);

      const rawDefaultLevel = defaultLevelResult.success ? camelCaseKeys(defaultLevelResult.data) : {};
      const rawEditRule = editRuleResult.success ? editRuleResult.data : null;
      const rawParentRule = parentRuleResult.success ? parentRuleResult.data : null;

      if (rawEditRule?.id) setRebateAgentRuleId(rawEditRule.id);
      setParentIsRoot(!!rawParentRule?.isRoot);

      const editSchemaMap: Record<number, any> = {};
      if (rawEditRule?.schema) {
        for (const item of rawEditRule.schema) {
          editSchemaMap[item.accountType] = item;
        }
      }
      setEditRebateRuleDetails(editSchemaMap);

      const allowedAccounts = rawParentRule?.calculatedLevelSetting?.allowedAccounts || [];
      const newSettings: Record<number, AccountLevelItem> = {};
      const newFormTables: Record<number, FormRow[]> = {};
      const newPercentages: Record<number, number> = {};
      const newPcSelections: Record<number, PCSelection> = {};
      const newDefaultLevels: Record<number, any> = {};

      for (const acct of allowedAccounts) {
        const at = acct.accountType;
        const hasEdit = !!editSchemaMap[at];

        newSettings[at] = {
          accountType: at,
          optionName: acct.optionName ?? null,
          selected: hasEdit,
          defaultSelected: hasEdit,
          allowPips: acct.allowPips || [],
          allowCommissions: acct.allowCommissions || [],
          percentage: editSchemaMap[at]?.percentage ?? 100,
          pips: editSchemaMap[at]?.pips ?? null,
          commission: editSchemaMap[at]?.commission ?? null,
          items: {},
        };

        if (acct.items) {
          for (const item of acct.items) {
            newSettings[at].items[item.cid] = item.r;
          }
        }

        let dl = JSON.parse(JSON.stringify(rawDefaultLevel));
        if (dl[at]?.length > 1 && acct.optionName != null) {
          dl = dl[at].find((a: any) => a.optionName === acct.optionName) || dl[at][0];
        } else if (dl[at]) {
          dl = dl[at][0] || dl;
        }
        newDefaultLevels[at] = dl;

        const editItems: Record<number, number> = {};
        if (editSchemaMap[at]?.items) {
          for (const item of editSchemaMap[at].items) {
            editItems[item.cid] = item.r;
          }
        }

        const rows: FormRow[] = categories.map((cat: { key: number; value: string }) => ({
          cid: cat.key,
          name: cat.value,
          total: newSettings[at].items[cat.key] ?? dl?.category?.[cat.key] ?? 0,
          r: hasEdit ? (editItems[cat.key] ?? 0) : 0,
        }));
        newFormTables[at] = rows;
        newPercentages[at] = editSchemaMap[at]?.percentage ?? 100;

        const pcSel: PCSelection = {
          schema: {},
          pcDropdown: false,
          optionDropdown: false,
          selectedPC: '',
          pcValue: null,
        };

        if (hasEdit) {
          const es = editSchemaMap[at];
          if (es.pips != null && (!es.commission || es.commission === 0)) {
            pcSel.selectedPC = 'pips';
            pcSel.pcValue = es.pips;
            pcSel.schema = dl?.allowPipSetting?.[es.pips]?.items || {};
          } else if (es.commission != null && (!es.pips || es.pips === 0)) {
            pcSel.selectedPC = 'commission';
            pcSel.pcValue = es.commission;
            pcSel.schema = dl?.allowCommissionSetting?.[es.commission]?.items || {};
          } else {
            pcSel.selectedPC = 'pips';
            pcSel.pcValue = es.pips;
            pcSel.schema = dl?.allowPipSetting?.[es.pips]?.items || {};
          }
        } else {
          const allowPips = acct.allowPips || [];
          const allowCommissions = acct.allowCommissions || [];
          if (allowPips.length > 0) {
            pcSel.selectedPC = 'pips';
            pcSel.pcValue = allowPips[0];
            pcSel.schema = dl?.allowPipSetting?.[allowPips[0]]?.items || {};
          } else if (allowCommissions.length > 0) {
            pcSel.selectedPC = 'commission';
            pcSel.pcValue = allowCommissions[0];
            pcSel.schema = dl?.allowCommissionSetting?.[allowCommissions[0]]?.items || {};
          }
        }
        newPcSelections[at] = pcSel;
      }

      setAccountLevelSettings(newSettings);
      setFormTables(newFormTables);
      setPercentages(newPercentages);
      setPcSelections(newPcSelections);
      setDefaultLevelSettings(newDefaultLevels);
    } finally {
      setIsLoading(false);
    }
  }, [agentAccount, account, execute]);

  useEffect(() => {
    if (open && !mountedRef.current) {
      mountedRef.current = true;
      initData();
    }
    if (!open) {
      mountedRef.current = false;
    }
  }, [open, initData]);

  const toggleAccountSelected = (accountType: number) => {
    setAccountLevelSettings((prev) => {
      const item = prev[accountType];
      if (!item || item.defaultSelected) return prev;
      return { ...prev, [accountType]: { ...item, selected: !item.selected } };
    });
  };

  const updateFormRowValue = (accountType: number, cidIdx: number, value: number) => {
    setFormTables((prev) => {
      const rows = [...(prev[accountType] || [])];
      rows[cidIdx] = { ...rows[cidIdx], r: value };
      return { ...prev, [accountType]: rows };
    });
  };

  const handleSubmit = async () => {
    if (!agentAccount) return;
    setIsSubmitting(true);

    try {
      const formData: IBAllowedAccount[] = [];

      for (const [atStr, setting] of Object.entries(accountLevelSettings)) {
        if (!setting.selected) continue;
        const at = Number(atStr);
        const rows = formTables[at] || [];
        const pcSel = pcSelections[at];

        let pips: number | null = parentIsRoot ? null : setting.pips;
        let commission: number | null = parentIsRoot ? null : setting.commission;

        if (pcSel?.selectedPC === 'pips') {
          pips = pcSel.pcValue === 0 ? null : pcSel.pcValue;
        } else if (pcSel?.selectedPC === 'commission') {
          commission = pcSel.pcValue === 0 ? null : pcSel.pcValue;
        }

        formData.push({
          optionName: setting.optionName ?? undefined,
          accountType: setting.accountType,
          pips: pips ?? undefined,
          commission: commission ?? undefined,
          percentage: percentages[at] ?? 100,
          items: rows.map((row) => ({ cid: row.cid, r: row.r })),
        });
      }

      const result = await execute(updateIBRebateRule, agentAccount.uid, rebateAgentRuleId, formData);
      if (result.success) {
        showToast({ message: 'Update Success', type: 'success' });
        onOpenChange(false);
        onSuccess?.();
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderAccountSection = (at: number) => {
    const setting = accountLevelSettings[at];
    if (!setting) return null;
    const rows = formTables[at] || [];
    const hasEdit = !!editRebateRuleDetails[at];
    const pcSel = pcSelections[at];
    const showPCColumn = (setting.allowPips.length > 0 || setting.allowCommissions.length > 0) && parentIsRoot;
    const showPercentageColumn = (setting.percentage !== 0 && (setting.pips || setting.commission)) || showPCColumn;

    return (
      <div key={at} className="mb-6">
        {!setting.selected ? null : (
          <>
            <div className="mb-3 flex items-center gap-2">
              <div className="h-4 w-[3px] bg-[#800020]" />
              <span className="text-base font-medium">
                {tAccount(`accountTypes.${at}`)}
              </span>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full min-w-[600px] border-collapse text-center text-sm">
                <thead>
                  <tr className="bg-surface-secondary text-text-secondary">
                    <th className="px-3 py-3 font-medium">{t('rebateEdit.category')}</th>
                    <th className="px-3 py-3 font-medium">{t('rebateEdit.totalRebate')}</th>
                    <th className="px-3 py-3 font-medium">{t('rebateEdit.personalRebate')}</th>
                    <th className="px-3 py-3 font-medium">{t('rebateEdit.remainRebate')}</th>
                    {showPCColumn && (
                      <th className="bg-[#0053ad] px-3 py-3 font-medium text-white">
                        {pcSel?.selectedPC ? t(`rebateEdit.${pcSel.selectedPC}`) : '--'}
                        {pcSel?.pcValue != null ? ` (${pcSel.pcValue})` : ''}
                      </th>
                    )}
                    {showPercentageColumn && (
                      <th className="px-3 py-3 font-medium">
                        {t('rebateEdit.addRatePercentage')}
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {rows.map((row, idx) => {
                    const maxVal = hasEdit ? (editRebateRuleDetails[at]?.items?.find((i: any) => i.cid === row.cid)?.r ?? row.total) : row.total;
                    const remain = row.total < row.r ? 0 : Number((row.total - row.r).toFixed(1));

                    return (
                      <tr key={row.cid} className="border-t border-border">
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
                              updateFormRowValue(at, idx, Number(val.toFixed(1)));
                            }}
                            className="input-field w-24 rounded px-2 py-1 text-center"
                          />
                        </td>
                        <td className="px-3 py-3">{remain}</td>
                        {showPCColumn && (
                          <td className="px-3 py-3">
                            {pcSel?.schema?.[row.cid] ?? '-'}
                          </td>
                        )}
                        {showPercentageColumn && idx === 0 && (
                          <td className="border-l border-border px-3 py-3" rowSpan={rows.length}>
                            <div className="flex items-center justify-center gap-1">
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
                                  if (hasEdit && val > (editRebateRuleDetails[at]?.percentage ?? 100)) {
                                    val = editRebateRuleDetails[at]?.percentage ?? 100;
                                  }
                                  setPercentages((prev) => ({ ...prev, [at]: val }));
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
          </>
        )}
      </div>
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
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
                <span className="mr-3">[A] {t('rebateEdit.editRule1')}</span>
                <span className="mr-3">[B] {t('rebateEdit.editRule2')}</span>
              </div>
              <div>
                <span>{t('rebateEdit.newRule')}: </span>
                <span>{t('rebateEdit.editRule5')}</span>
              </div>
            </div>

            <div className="mb-4 flex flex-wrap gap-4">
              {Object.values(accountLevelSettings).map((setting) => (
                <label key={setting.accountType} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={setting.selected}
                    disabled={setting.defaultSelected}
                    onChange={() => toggleAccountSelected(setting.accountType)}
                    className="h-4 w-4 rounded border-border"
                  />
                  <span className="text-sm">{tAccount(`accountTypes.${setting.accountType}`)}</span>
                </label>
              ))}
            </div>

            {Object.keys(accountLevelSettings).map((atStr) => renderAccountSection(Number(atStr)))}

            <div className="flex justify-center pb-4 pt-2">
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
        <DialogFooter>
          <Button variant="outline" size="sm" className="w-auto min-w-20 md:w-[120px]" onClick={() => onOpenChange(false)}>
            {t('action.close')}
          </Button>
        </DialogFooter>
      </DialogContent>
      
    </Dialog>
  );
}
