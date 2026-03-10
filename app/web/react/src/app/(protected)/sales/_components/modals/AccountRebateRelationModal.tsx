'use client';

import { useState, useCallback, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Skeleton } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import {
  getSalesReferralPath,
  getSalesSymbolCategory,
  getSalesDefaultLevelSetting,
  getSalesLevelAccounts,
  getSalesAgentRules,
} from '@/actions';
import type { SalesClientAccount } from '@/types/sales';
import { AccountRoleTypes } from '@/types/accounts';

/* eslint-disable @typescript-eslint/no-explicit-any */

interface ReferralPathItem {
  uid: number;
  role: number;
  nativeName?: string;
  displayName?: string;
  firstName?: string;
  lastName?: string;
  allowedAccounts?: any[];
}

interface CategoryItem {
  id: number;
  name: string;
}

interface AccountRebateRelationModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}

function getPathUserName(item: ReferralPathItem): string {
  if (item.nativeName?.trim()) return item.nativeName;
  if (item.displayName?.trim()) return item.displayName;
  if (item.firstName?.trim() && item.lastName?.trim()) return `${item.firstName} ${item.lastName}`;
  return 'No Name';
}

function getRoleName(role: number): string {
  switch (role) {
    case AccountRoleTypes.Sales: return 'Sales';
    case AccountRoleTypes.IB: return 'IB';
    case AccountRoleTypes.Client: return 'Client';
    default: return String(role);
  }
}

interface RelationRow {
  uid: number;
  role: number;
  name: string;
  accountTypes: number[];
  pips?: Record<number, number | null>;
  commission?: Record<number, number | null>;
  percentage?: Record<number, number>;
  categories?: Record<number, Record<number, number>>;
}

export function AccountRebateRelationModal({ open, onOpenChange, account }: AccountRebateRelationModalProps) {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [isLoading, setIsLoading] = useState(false);
  const [categories, setCategories] = useState<CategoryItem[]>([]);
  const [rows, setRows] = useState<RelationRow[]>([]);
  const [accountTypes, setAccountTypes] = useState<number[]>([]);
  const [selectedAccountType, setSelectedAccountType] = useState<number | null>(null);

  const initData = useCallback(async () => {
    if (!salesAccount || !account) return;
    setIsLoading(true);
    try {
      const [pathResult, catResult, defaultLevelResult] = await Promise.all([
        execute(getSalesReferralPath, salesAccount.uid, account.uid),
        execute(getSalesSymbolCategory),
        execute(getSalesDefaultLevelSetting, salesAccount.uid),
      ]);

      const cats: CategoryItem[] = catResult.success && Array.isArray(catResult.data)
        ? (catResult.data as any[]).map((c) => ({ id: c.id ?? c.key, name: c.name ?? c.value }))
        : [];
      setCategories(cats);

      if (!pathResult.success || !pathResult.data) {
        setRows([]);
        return;
      }

      const referPath = Array.isArray(pathResult.data)
        ? pathResult.data as ReferralPathItem[]
        : (pathResult.data as any).data ?? [];

      if (referPath.length === 0) {
        setRows([]);
        return;
      }

      const allAccountTypes = new Set<number>();
      for (const item of referPath) {
        if (item.allowedAccounts) {
          for (const aa of item.allowedAccounts) {
            allAccountTypes.add(aa.accountType ?? aa);
          }
        }
      }
      const atArr = Array.from(allAccountTypes).sort();
      setAccountTypes(atArr);
      if (atArr.length > 0) setSelectedAccountType(atArr[0]);

      const agentUids = referPath
        .filter((p) => p.role === AccountRoleTypes.IB || p.role === AccountRoleTypes.Sales)
        .map((p) => p.uid);

      let levelAccountsMap: Record<number, any> = {};
      let agentRulesMap: Record<number, any> = {};

      if (agentUids.length > 0) {
        const [levelResult, rulesResult] = await Promise.all([
          execute(getSalesLevelAccounts, salesAccount.uid, account.uid),
          execute(getSalesAgentRules, salesAccount.uid),
        ]);

        if (levelResult.success && levelResult.data) {
          const raw = levelResult.data as any;
          if (Array.isArray(raw)) {
            for (const item of raw) {
              if (item.uid) levelAccountsMap[item.uid] = item;
            }
          } else if (typeof raw === 'object') {
            levelAccountsMap = raw;
          }
        }

        if (rulesResult.success && rulesResult.data) {
          const raw = rulesResult.data as any;
          if (Array.isArray(raw)) {
            for (const item of raw) {
              if (item.agentUid) agentRulesMap[item.agentUid] = item;
            }
          } else if (typeof raw === 'object') {
            agentRulesMap = raw;
          }
        }
      }

      const defaultLevel = defaultLevelResult.success ? defaultLevelResult.data : null;

      const relationRows: RelationRow[] = referPath.map((pathItem) => {
        const row: RelationRow = {
          uid: pathItem.uid,
          role: pathItem.role,
          name: getPathUserName(pathItem),
          accountTypes: atArr,
          pips: {},
          commission: {},
          percentage: {},
          categories: {},
        };

        const rule = agentRulesMap[pathItem.uid];
        if (rule?.schema) {
          for (const schema of rule.schema) {
            const at = schema.accountType;
            if (row.pips) row.pips[at] = schema.pips ?? null;
            if (row.commission) row.commission[at] = schema.commission ?? null;
            if (row.percentage) row.percentage[at] = schema.percentage ?? 100;
            if (schema.items && row.categories) {
              row.categories[at] = {};
              for (const item of schema.items) {
                row.categories[at][item.cid] = item.r ?? 0;
              }
            }
          }
        }

        if (pathItem.role === AccountRoleTypes.Sales && defaultLevel) {
          for (const at of atArr) {
            const dl = (defaultLevel as any)[at];
            if (dl && Array.isArray(dl) && dl[0]?.category) {
              if (!row.categories) row.categories = {};
              if (!row.categories[at]) {
                row.categories[at] = {};
                for (const cat of cats) {
                  row.categories[at][cat.id] = dl[0].category[cat.id] ?? 0;
                }
              }
            }
          }
        }

        return row;
      });

      setRows(relationRows);
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute]);

  useEffect(() => {
    if (open && account) {
      initData();
    }
  }, [open, account, initData]);

  const userName = account?.user?.displayName || account?.user?.nativeName || '';

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[1000px]">
        <DialogHeader>
          <DialogTitle>{t('rebateRelation.title')} - {userName}</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4 py-4">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-40 w-full" />
          </div>
        ) : rows.length === 0 ? (
          <div className="py-12 text-center text-text-secondary">{t('rebateRelation.noData')}</div>
        ) : (
          <div className="max-h-[70vh] overflow-auto">
            {accountTypes.length > 1 && (
              <div className="mb-4 flex flex-wrap gap-2">
                {accountTypes.map((at) => (
                  <button
                    key={at}
                    type="button"
                    onClick={() => setSelectedAccountType(at)}
                    className={`rounded px-3 py-1.5 text-sm transition-colors ${
                      selectedAccountType === at
                        ? 'bg-primary text-white'
                        : 'bg-surface-secondary text-text-secondary hover:bg-surface-secondary/80'
                    }`}
                  >
                    {at}
                  </button>
                ))}
              </div>
            )}

            <div className="overflow-x-auto">
              <table className="w-full min-w-[700px] border-collapse text-sm">
                <thead>
                  <tr className="bg-surface-secondary text-text-secondary">
                    <th className="px-3 py-3 text-left font-medium">{t('rebateRelation.role')}</th>
                    <th className="px-3 py-3 text-left font-medium">{t('rebateRelation.name')}</th>
                    <th className="px-3 py-3 text-left font-medium">{t('rebateRelation.accountUid')}</th>
                    <th className="px-3 py-3 text-center font-medium">{t('rebateRelation.pips')}</th>
                    <th className="px-3 py-3 text-center font-medium">{t('rebateRelation.commission')}</th>
                    <th className="px-3 py-3 text-center font-medium">{t('rebateRelation.percentage')}</th>
                    {categories.map((cat) => (
                      <th key={cat.id} className="px-3 py-3 text-center font-medium">{cat.name}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {rows.map((row) => (
                    <tr key={row.uid} className="border-t border-border">
                      <td className="px-3 py-3">
                        <span className={`rounded-full px-2 py-0.5 text-xs ${
                          row.role === AccountRoleTypes.Sales ? 'bg-red-100 text-red-700'
                          : row.role === AccountRoleTypes.IB ? 'bg-green-100 text-green-700'
                          : 'bg-blue-100 text-blue-700'
                        }`}>
                          {getRoleName(row.role)}
                        </span>
                      </td>
                      <td className="px-3 py-3 font-medium">{row.name}</td>
                      <td className="px-3 py-3 text-text-secondary">{row.uid}</td>
                      <td className="px-3 py-3 text-center">
                        {selectedAccountType != null && row.pips?.[selectedAccountType] != null
                          ? row.pips[selectedAccountType]
                          : '-'}
                      </td>
                      <td className="px-3 py-3 text-center">
                        {selectedAccountType != null && row.commission?.[selectedAccountType] != null
                          ? row.commission[selectedAccountType]
                          : '-'}
                      </td>
                      <td className="px-3 py-3 text-center">
                        {selectedAccountType != null && row.percentage?.[selectedAccountType] != null
                          ? `${row.percentage[selectedAccountType]}%`
                          : '-'}
                      </td>
                      {categories.map((cat) => (
                        <td key={cat.id} className="px-3 py-3 text-center">
                          {selectedAccountType != null && row.categories?.[selectedAccountType]?.[cat.id] != null
                            ? row.categories[selectedAccountType][cat.id]
                            : '-'}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
