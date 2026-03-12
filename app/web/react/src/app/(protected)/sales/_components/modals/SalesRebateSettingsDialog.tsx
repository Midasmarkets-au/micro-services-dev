'use client';

import { useEffect, useMemo, useState } from 'react';
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
import { getSalesLinkDetail, getSalesSymbolCategory } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import type { SalesLinkDetail, SalesLinkSchema } from '@/types/sales';

interface SalesRebateSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  code: string | null;
}

const SERVICE_TYPE_BROKER = 200;
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
}: SalesRebateSettingsDialogProps) {
  const t = useTranslations('sales');
  const tAccount = useTranslations('accounts');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [isLoading, setIsLoading] = useState(false);
  const [detail, setDetail] = useState<SalesLinkDetail | null>(null);
  const [productCategory, setProductCategory] = useState<ProductCategoryItem[]>(
    []
  );

  useEffect(() => {
    if (!open) {
      setIsLoading(false);
      setDetail(null);
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
          execute(getSalesLinkDetail, salesAccount.uid, code),
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

  const productCategoryMap = useMemo(() => {
    const map = new Map<number, string>();
    productCategory.forEach((item) => {
      const k = Number(item.key ?? item.id);
      if (!Number.isFinite(k)) return;
      const v = item.value ?? item.name;
      if (v) map.set(k, v);
    });
    return map;
  }, [productCategory]);

  const pipOptionMap = useMemo(() => {
    try {
      return t.raw('type.pipOptions') as Record<string, string>;
    } catch {
      return {};
    }
  }, [t]);

  const commissionOptionMap = useMemo(() => {
    try {
      return t.raw('type.commissionOptions') as Record<string, string>;
    } catch {
      return {};
    }
  }, [t]);

  const isAgent = detail?.serviceType === SERVICE_TYPE_BROKER;
  const isClient = detail?.serviceType === SERVICE_TYPE_CLIENT;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[calc(100vw-1.5rem)] max-w-[750px] p-4! sm:p-6!">
        <DialogHeader>
          <DialogTitle>{t('link.rebateSettings')}</DialogTitle>
          <DialogDescription className="sr-only">
            {t('link.rebateSettings')}
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
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
                    isAgent ? 'border-primary' : 'border-border'
                  }`}
                >
                  {isAgent && (
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
                  ? t('type.account.4')
                  : schema.optionName}
              </span>
            )}
          </div>

          {schema.items && schema.items.length > 0 && (
            <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
              {schema.items.map((item, idx) => (
                <div key={idx} className="rounded-md bg-surface-secondary p-2">
                  <div className="truncate text-xs text-text-secondary">
                    {productCategoryMap.get(item.cid) ?? String(item.cid)}
                  </div>
                  <Input
                    value={String(item.r)}
                    disabled
                    className="mt-1 h-8 bg-transparent text-center text-sm"
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
                    ? t('type.account.4')
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
