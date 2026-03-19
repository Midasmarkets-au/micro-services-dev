'use client';

import { useState, useCallback, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Skeleton ,Button} from '@/components/ui';
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

interface AccountRebateRelationModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}

interface RelationItem {
  uid: number;
  role: number;
  upperAcc: number | null;
  belowAcc: number | null;
}

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

function convertArrayToObject(rules: any[]): Record<string, any> {
  return (rules || []).reduce((acc: any, cur: any) => {
    acc[cur.accountType] = cur;
    return acc;
  }, {});
}

function adjustRebateInfo(data: any[]): Record<number, any> {
  data.forEach((acc: any) => {
    acc.schema = convertArrayToObject(acc.schema);
    if (acc.levelSetting?.allowedAccounts) {
      acc.levelSetting.allowedAccounts = convertArrayToObject(
        acc.levelSetting.allowedAccounts
      );
    }
    if (acc.calculatedLevelSetting?.allowedAccounts) {
      acc.calculatedLevelSetting.allowedAccounts = convertArrayToObject(
        acc.calculatedLevelSetting.allowedAccounts
      );
    }
  });

  return data.reduce((acc: any, cur: any) => {
    acc[cur.agentAccountUid] = cur;
    return acc;
  }, {});
}

function adjustDefaultRebateRate(
  data: any,
  rebateInfo: Record<number, any>
): any {
  const topIbRebateInfo: any = Object.values(rebateInfo).find(
    (acc: any) => acc.isRoot
  );

  Object.keys(data).forEach((acc) => {
    data[acc] =
      data[acc].find(
        (option: any) =>
          option.optionName ===
          topIbRebateInfo?.levelSetting?.allowedAccounts?.[acc]?.optionName
      ) ?? data[acc][0];

    if (data[acc]?.allowPipOptions) {
      data[acc].allowPipOptions = data[acc].allowPipOptions.map(
        (option: any) => ({ label: option, value: option })
      );
    }
    if (data[acc]?.allowCommissionOptions) {
      data[acc].allowCommissionOptions = data[acc].allowCommissionOptions.map(
        (option: any) => ({ label: option, value: option })
      );
    }
  });

  return data;
}

export function AccountRebateRelationModal({
  open,
  onOpenChange,
  account,
}: AccountRebateRelationModalProps) {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [isLoading, setIsLoading] = useState(false);
  const [rebateInfo, setRebateInfo] = useState<Record<number, any>>({});
  const [selectedAccountType, setSelectedAccountType] = useState<string>('0');
  const [accountInfo, setAccountInfo] = useState<Record<number, any>>({});
  const [relationList, setRelationList] = useState<RelationItem[]>([]);
  const [allAccountType, setAllAccountType] = useState<string[]>([]);
  const [productCategory, setProductCategory] = useState<any[]>([]);
  const [availableAccountType, setAvailableAccountType] = useState<string[]>(
    []
  );

  const buildRelationForm = useCallback(
    async (referPath: number[]) => {
      if (!salesAccount) return;

      setIsLoading(true);
      setRelationList([]);
      setSelectedAccountType('0');

      try {
        const [
          getCategoryResult,
          getDefaultLevelSettingResult,
          queryAccountsResult,
          getAgentRulesResult,
        ] = await Promise.all([
          execute(getSalesSymbolCategory),
          execute(getSalesDefaultLevelSetting, salesAccount.uid),
          execute(
            getSalesLevelAccounts,
            salesAccount.uid,
            referPath[referPath.length - 1]
          ),
          execute(getSalesAgentRules, salesAccount.uid, {
            agentUids: referPath,
          }),
        ]);

        // accountInfo: array → map by uid
        const queryAccounts =
          queryAccountsResult.success &&
          Array.isArray(queryAccountsResult.data)
            ? queryAccountsResult.data
            : [];
        const newAccountInfo: Record<number, any> = queryAccounts.reduce(
          (acc: any, cur: any) => {
            acc[cur.uid] = cur;
            return acc;
          },
          {}
        );
        setAccountInfo(newAccountInfo);

        // productCategory
        const cats =
          getCategoryResult.success && Array.isArray(getCategoryResult.data)
            ? getCategoryResult.data
            : [];
        console.log('cats', cats);
        setProductCategory(cats);

        // rebateInfo: adjustRebateInfo converts arrays to objects keyed by accountType
        const agentRulesRaw = getAgentRulesResult.success
          ? getAgentRulesResult.data
          : [];
        const agentRulesData = Array.isArray(agentRulesRaw)
          ? agentRulesRaw
          : (agentRulesRaw as any)?.data ?? [];
        const newRebateInfo = adjustRebateInfo(
          JSON.parse(JSON.stringify(agentRulesData))
        );
        setRebateInfo(newRebateInfo);

        // defaultRebateRate
        const rawDefaultLevel = getDefaultLevelSettingResult.success
          ? camelCaseKeys(
              JSON.parse(
                JSON.stringify(getDefaultLevelSettingResult.data)
              )
            )
          : {};
        const newDefaultRebateRate = adjustDefaultRebateRate(
          rawDefaultLevel,
          newRebateInfo
        );
        setAllAccountType(Object.keys(newDefaultRebateRate));

        // relationList
        const newRelationList: RelationItem[] = referPath.map((uid, index) => ({
          uid,
          role: newAccountInfo[uid]?.role ?? 0,
          upperAcc: referPath[index - 1] ?? null,
          belowAcc: referPath[index + 1] ?? null,
        }));
        setRelationList(newRelationList);

        // availableAccountType: 基于最后一个账户的角色
        const lastAccount = newRelationList[newRelationList.length - 1];
        let newAvailableAccountType: string[] = [];

        if (lastAccount) {
          const roleToAvailableMap: Record<number, string[]> = {
            [AccountRoleTypes.Sales]: Object.keys(newDefaultRebateRate),
            [AccountRoleTypes.IB]: Object.keys(
              newRebateInfo[lastAccount.uid]?.calculatedLevelSetting
                ?.allowedAccounts ?? {}
            ),
            [AccountRoleTypes.Client]: [
              String(newAccountInfo[lastAccount.uid]?.type),
            ],
          };
          newAvailableAccountType =
            roleToAvailableMap[lastAccount.role] ?? [];
        }
        setAvailableAccountType(newAvailableAccountType);

        // 选中第一个可用的账户类型
        let newSelectedAccountType = '0';
        if (newAvailableAccountType.length > 0) {
          newSelectedAccountType = newAvailableAccountType[0];
        }
        setSelectedAccountType(newSelectedAccountType);
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
      const pathResult = await execute(
        getSalesReferralPath,
        salesAccount.uid,
        account.uid
      );

      if (pathResult.success && pathResult.data) {
        const referPath: number[] = Array.isArray(pathResult.data)
          ? pathResult.data
          : [];
        if (referPath.length > 0) {
          await buildRelationForm(referPath);
          return;
        }
      }
      setRelationList([]);
    } catch {
      // error handled by useServerAction
    }

    setIsLoading(false);
  }, [salesAccount, account, execute, buildRelationForm]);

  useEffect(() => {
    if (!open || !account) return;
    let cancelled = false;
    (async () => {
      if (!cancelled) await initData();
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, account]);

  const userName =
    account?.user?.displayName || account?.user?.nativeName || '';

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{userName}</DialogTitle>
        </DialogHeader>

        <div className="max-h-[70vh] overflow-auto">
          {isLoading ? (
            <div className="space-y-4 py-4">
              <Skeleton className="h-6 w-48" />
              <Skeleton className="h-40 w-full" />
            </div>
          ) : relationList.length === 0 ? (
            <div className="py-12 text-center text-text-secondary">
              {t('rebateRelation.noData')}
            </div>
          ) : (
            <>
              {/* 账户类型切换按钮 */}
              <div className="mb-5 mt-5 flex">
                <div className="flex">
                  {allAccountType.map((accountType) =>
                    availableAccountType.includes(accountType) ? (
                      <button
                        key={accountType}
                        type="button"
                        onClick={() => setSelectedAccountType(accountType)}
                        className={`mr-4 w-[100px] cursor-pointer rounded-[5px] border px-[15px] py-[10px] text-center text-sm transition-colors ${
                          selectedAccountType === accountType
                            ? 'border-[#000f32] bg-[#000f32] text-white'
                            : 'border-[#ecedf4] hover:border-[#000f32] hover:bg-[#000f32] hover:text-white'
                        }`}
                      >
                        {t(`type.account.${accountType}` as any)}
                      </button>
                    ) : null
                  )}
                </div>
              </div>

              {/* 数据表格 */}
              <table className="w-full table-auto border-collapse align-middle">
                <tbody className="text-sm font-semibold text-gray-600">
                  {/* 第一行：账户所有者信息 */}
                  <tr className="border-b border-[#ecedf4] text-center">
                    <td />
                    {relationList.map(({ uid }) => (
                      <td key={uid} className="relative px-3 py-3">
                        <div className="mb-2">
                          <strong>
                            {t(
                              `type.roleType.${accountInfo[uid]?.role}` as any
                            )}
                          </strong>
                          : {accountInfo[uid]?.nativeName}
                        </div>

                        <span className="inline-block rounded-[5px] bg-gray-100 px-2 py-0.5 text-xs font-normal">
                          {uid}
                        </span>

                        {rebateInfo[uid]?.calculatedLevelSetting
                          ?.allowedAccounts && (
                          <div className="mt-2">
                            {Object.values(
                              rebateInfo[uid].calculatedLevelSetting
                                .allowedAccounts
                            ).map((acc: any, index: number) => (
                              <span
                                key={`type_${index}`}
                                className={`me-1 inline-block rounded-lg px-2 py-0.5 text-[10px] font-bold ${
                                  [
                                    'bg-[rgba(88,168,255,0.1)] text-[#4196f0]',
                                    'bg-[rgba(255,164,0,0.15)] text-[#ffa400]',
                                    'bg-[rgba(123,97,255,0.1)] text-[#7b61ff]',
                                  ][index % 3]
                                }`}
                              >
                                {t(
                                  `type.account.${acc.accountType}` as any
                                )}
                              </span>
                            ))}
                          </div>
                        )}
                      </td>
                    ))}
                  </tr>

                  {/* 第二行：Pips 和 Commission */}
                  <tr className="border-b border-[#ecedf4] text-center">
                    <td />
                    {relationList.map(({ uid }) => (
                      <td key={uid} className="px-3 py-3">
                        {rebateInfo[uid]?.calculatedLevelSetting
                          ?.allowedAccounts?.[selectedAccountType] ? (
                          rebateInfo[uid].isRoot ? (
                            <div>
                              <div>
                                <div>{t('rebateRelation.allowPips')}</div>
                                <div className="mt-2 flex justify-center">
                                  {rebateInfo[
                                    uid
                                  ]?.levelSetting?.allowedAccounts?.[
                                    selectedAccountType
                                  ]?.allowPips?.map(
                                    (item: any, index: number) => (
                                      <div
                                        key={index}
                                        className="me-2 flex h-[22px] w-[22px] items-center justify-center rounded-[5px] bg-white text-xs shadow-[0_1px_4px_rgba(0,0,0,0.16)]"
                                      >
                                        {item}
                                      </div>
                                    )
                                  )}
                                </div>
                              </div>
                              <div className="mt-3">
                                <div>
                                  {t('rebateRelation.allowCommissions')}
                                </div>
                                <div className="mt-2 flex justify-center">
                                  {rebateInfo[
                                    uid
                                  ]?.levelSetting?.allowedAccounts?.[
                                    selectedAccountType
                                  ]?.allowCommissions?.map(
                                    (item: any, index: number) => (
                                      <div
                                        key={index}
                                        className="me-2 flex h-[22px] w-[22px] items-center justify-center rounded-[5px] bg-white text-xs shadow-[0_1px_4px_rgba(0,0,0,0.16)]"
                                      >
                                        {item}
                                      </div>
                                    )
                                  )}
                                </div>
                              </div>
                            </div>
                          ) : accountInfo[uid]?.role ===
                            AccountRoleTypes.IB ? (
                            <div className="flex flex-col items-center justify-center">
                              <div className="flex items-center">
                                <div className="me-3">
                                  Pips:{' '}
                                  {rebateInfo[uid]?.schema?.[
                                    selectedAccountType
                                  ]?.pips ?? '--'}
                                </div>
                              </div>
                              <div className="mt-3 flex items-center justify-center">
                                <div className="me-3">
                                  Com:{' '}
                                  {rebateInfo[uid]?.schema?.[
                                    selectedAccountType
                                  ]?.commission ?? '--'}
                                </div>
                              </div>
                            </div>
                          ) : null
                        ) : null}
                      </td>
                    ))}
                  </tr>

                  {/* 第三行："我拿多少" 百分比 */}
                  <tr className="border-b border-[#ecedf4] text-center">
                    <td />
                    {relationList.map(({ uid, belowAcc }) => (
                      <td key={uid} className="px-3 py-3">
                        {accountInfo[uid]?.role === AccountRoleTypes.IB &&
                        belowAcc &&
                        rebateInfo[belowAcc]?.schema?.[
                          selectedAccountType
                        ] ? (
                          <div className="flex flex-col items-center justify-center">
                            <div>{t('rebateRelation.howMuchTake')}</div>
                            <div className="mt-[5px] w-[80px] rounded-[10px] bg-white shadow-[0_1px_4px_rgba(0,0,0,0.16)]">
                              {
                                rebateInfo[belowAcc].schema[
                                  selectedAccountType
                                ].percentage
                              }
                              %
                            </div>
                          </div>
                        ) : null}
                      </td>
                    ))}
                  </tr>

                  {/* 产品品类行 */}
                  {productCategory.map((category) => (
                    <tr
                      key={category.key ?? category.id}
                      className="border-b border-[#ecedf4] text-center"
                    >
                      <td className="px-3 py-3 text-left">
                        {category.value ?? category.name}
                      </td>
                      {relationList.map(({ uid, belowAcc }) => {
                        const catKey = category.key ?? category.id;

                        if (
                          belowAcc &&
                          accountInfo[uid]?.role === AccountRoleTypes.Sales &&
                          rebateInfo[belowAcc]?.levelSetting
                            ?.allowedAccounts?.[selectedAccountType]
                        ) {
                          const val = rebateInfo[
                            belowAcc
                          ].levelSetting.allowedAccounts[
                            selectedAccountType
                          ]?.items?.find(
                            (acc: any) => acc.cid === catKey
                          )?.r;
                          return (
                            <td key={uid} className="px-3 py-3">
                              <input
                                className="w-[100px] rounded border border-gray-300 bg-gray-50 px-2 py-1 text-center text-sm"
                                value={val ?? ''}
                                disabled
                                readOnly
                              />
                            </td>
                          );
                        }

                        if (
                          belowAcc &&
                          accountInfo[uid]?.role === AccountRoleTypes.IB &&
                          accountInfo[belowAcc]?.role ===
                            AccountRoleTypes.IB &&
                          rebateInfo[belowAcc]?.schema?.[selectedAccountType]
                        ) {
                          const val = rebateInfo[belowAcc].schema[
                            selectedAccountType
                          ]?.items?.find(
                            (acc: any) => acc.cid === catKey
                          )?.r;
                          return (
                            <td key={uid} className="px-3 py-3">
                              <input
                                className="w-[100px] rounded border border-gray-300 bg-gray-50 px-2 py-1 text-center text-sm"
                                value={val ?? ''}
                                disabled
                                readOnly
                              />
                            </td>
                          );
                        }

                        if (
                          accountInfo[uid]?.role === AccountRoleTypes.IB &&
                          (!belowAcc ||
                            (belowAcc &&
                              accountInfo[belowAcc]?.role ===
                                AccountRoleTypes.Client))
                        ) {
                          const val =
                            rebateInfo[
                              uid
                            ]?.calculatedLevelSetting?.allowedAccounts?.[
                              selectedAccountType
                            ]?.items?.find(
                              (acc: any) => acc.cid === catKey
                            )?.r;
                          return (
                            <td key={uid} className="px-3 py-3">
                              <span>{val ?? ''}</span>
                            </td>
                          );
                        }

                        return <td key={uid} className="px-3 py-3" />;
                      })}
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}
        </div>
        <DialogFooter>
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
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
