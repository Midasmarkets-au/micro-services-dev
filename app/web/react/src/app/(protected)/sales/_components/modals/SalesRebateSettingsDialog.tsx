'use client';

import { useState, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  Button,
} from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesLinkDetail } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import type { SalesLinkDetail, SalesLinkSchema } from '@/types/sales';

interface SalesRebateSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  code: string | null;
}

const SERVICE_TYPE_BROKER = 200;
const SERVICE_TYPE_CLIENT = 400;

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

  const handleOpenChange = useCallback(
    async (nextOpen: boolean) => {
      if (nextOpen && code && salesAccount) {
        setIsLoading(true);
        setDetail(null);
        try {
          const result = await execute(getSalesLinkDetail, salesAccount.uid, code);
          if (result.success && result.data) {
            const d = result.data;
            if (d.serviceType) {
              d.serviceType = Number(d.serviceType);
            }
            if (d.summary && Object.keys(d.summary).length === 0) {
              setDetail(null);
            } else {
              setDetail(d);
            }
          }
        } catch {
          setDetail(null);
        } finally {
          setIsLoading(false);
        }
      }
      onOpenChange(nextOpen);
    },
    [code, salesAccount, execute, onOpenChange]
  );

  const isAgent = detail?.serviceType === SERVICE_TYPE_BROKER;
  const isClient = detail?.serviceType === SERVICE_TYPE_CLIENT;

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="p-6!">
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
          <div className="mt-4 flex max-h-[60vh] flex-col gap-5 overflow-y-auto">
            <div className="flex items-center gap-6">
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

            <p className="text-sm text-text-secondary">
              <span className="text-primary">*</span>
              {t('link.selectAccountType')}
            </p>

            {isAgent && (
              <AgentDetailView
                schemas={detail.summary?.schema ?? []}
                tAccount={tAccount}
                t={t}
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
          <Button variant="primary" onClick={() => onOpenChange(false)}>
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
    <div className="flex flex-col gap-4">
      {schemas.map((schema, index) => (
        <div key={index}>
          <div className="flex items-center gap-2 rounded-lg border border-border px-4 py-3">
            <span className="flex h-5 w-5 items-center justify-center rounded-full border-2 border-border">
              <span className="h-2.5 w-2.5 rounded-full bg-primary" />
            </span>
            <span className="text-sm font-medium text-text-primary">
              {tAccount(`accountTypes.${schema.accountType}`)}
            </span>
            {schema.optionName && (
              <span
                className="rounded-md px-2 py-0.5 text-xs font-semibold"
                style={{
                  background: 'rgba(128,0,32,0.2)',
                  color: '#800020',
                }}
              >
                {schema.optionName === 'alpha'
                  ? 'Standard'
                  : schema.optionName}
              </span>
            )}
          </div>

          {schema.items && schema.items.length > 0 && (
            <div className="mt-2 overflow-x-auto px-4">
              <div className="flex flex-wrap gap-3">
                {schema.items.map((item, idx) => (
                  <div
                    key={idx}
                    className="flex items-center gap-2 rounded bg-surface-secondary px-3 py-1.5 text-sm"
                  >
                    <span className="text-text-secondary">
                      {item.cid}
                    </span>
                    <span className="text-text-primary font-medium">
                      {item.r}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {schema.allowPips && schema.allowPips.length > 0 && (
            <div className="mt-2 px-4">
              <span className="text-xs text-text-secondary">
                {t('link.pips')}:{' '}
              </span>
              {schema.allowPips.map((p, idx) => (
                <span
                  key={idx}
                  className="mr-1 inline-block rounded bg-green-100 px-2 py-0.5 text-xs text-green-700"
                >
                  {p}
                </span>
              ))}
            </div>
          )}

          {schema.allowCommissions && schema.allowCommissions.length > 0 && (
            <div className="mt-1 px-4">
              <span className="text-xs text-text-secondary">
                {t('link.commission')}:{' '}
              </span>
              {schema.allowCommissions.map((c, idx) => (
                <span
                  key={idx}
                  className="mr-1 inline-block rounded bg-blue-100 px-2 py-0.5 text-xs text-blue-700"
                >
                  {c}
                </span>
              ))}
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
          className="flex items-center justify-between rounded-lg border border-border px-4 py-3"
        >
          <div className="flex items-center gap-3">
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
              <span
                className="rounded-md px-2 py-0.5 text-xs font-semibold"
                style={{
                  background: 'rgba(128,0,32,0.2)',
                  color: '#800020',
                }}
              >
                {schema.optionName === 'alpha'
                  ? 'Standard'
                  : schema.optionName}
              </span>
            )}
          </div>
          <div className="flex items-center gap-4 text-sm text-text-secondary">
            <span>
              {t('link.pips')} ≥
              {schema.pips == null || schema.pips === 0 ? '0' : schema.pips}
            </span>
            <span className="text-border">|</span>
            <span>
              {t('link.commission')} ≥
              {schema.commission == null || schema.commission === 0
                ? '0'
                : schema.commission}
            </span>
          </div>
        </div>
      ))}
    </div>
  );
}
