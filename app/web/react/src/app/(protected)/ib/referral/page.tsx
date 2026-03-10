'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBReferralCodes } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { Button } from '@/components/ui';
import type { IBReferralCode } from '@/types/ib';

export default function IBReferralPage() {
  const t = useTranslations('ib');
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const [data, setData] = useState<IBReferralCode[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const fetchData = useCallback(async () => {
    if (!agentAccount) return;
    setIsLoading(true);
    const result = await execute(getIBReferralCodes, agentAccount.uid, {
      status: 0,
    });
    if (result.success && result.data) {
      const list = Array.isArray(result.data)
        ? result.data
        : (result.data as { data?: IBReferralCode[] })?.data ?? [];
      setData(list);
    }
    setIsLoading(false);
  }, [agentAccount, execute]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleCopyUrl = (url: string) => {
    navigator.clipboard.writeText(url);
    // Could add toast notification here if available
  };

  return (
    <div className="flex w-full flex-col gap-5">
      {agentAccount && (
        <div className="rounded-xl border border-border bg-surface">
          <div className="border-b border-border p-4">
            <Button size="sm">{t('action.create')}</Button>
          </div>
          <div className="p-4">
            {isLoading ? (
              <div className="flex flex-col gap-3 py-12">
                {[1, 2, 3].map((i) => (
                  <div
                    key={i}
                    className="h-24 animate-pulse rounded-lg bg-border"
                  />
                ))}
              </div>
            ) : data.length === 0 ? (
              <div className="py-12 text-center text-text-secondary">
                {t('dashboard.noData')}
              </div>
            ) : (
              <div className="flex flex-col gap-4">
                {data.map((item, idx) => (
                  <div
                    key={item.id ?? idx}
                    className="rounded-lg border border-border bg-surface-secondary p-4"
                  >
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div className="flex flex-col gap-1">
                        <div className="flex items-center gap-2">
                          <span className="rounded-full bg-(--color-primary)/10 px-2 py-0.5 text-xs font-medium text-primary">
                            {item.code}
                          </span>
                          {item.isDefault && (
                            <span className="rounded-full bg-(--color-primary)/10 px-2 py-0.5 text-xs text-primary">
                              {t('fields.default')}
                            </span>
                          )}
                        </div>
                        <span className="text-sm font-medium text-text-primary">
                          {item.name || '-'}
                        </span>
                        {item.url && (
                          <div className="flex items-center gap-2">
                            <code className="max-w-md truncate rounded bg-input-bg px-2 py-1 text-xs text-text-secondary">
                              {item.url}
                            </code>
                            <Button
                              size="xs"
                              variant="outline"
                              onClick={() => handleCopyUrl(item.url!)}
                            >
                              {t('action.copy')}
                            </Button>
                          </div>
                        )}
                      </div>
                      <div className="flex flex-col items-end gap-1">
                        <span
                          className={`rounded-full px-2 py-0.5 text-xs ${
                            item.status === 1
                              ? 'bg-green-100 text-green-700'
                              : 'bg-gray-100 text-gray-700'
                          }`}
                        >
                          {item.status === 1 ? 'Active' : 'Inactive'}
                        </span>
                        <span className="text-xs text-text-secondary">
                          {item.createdOn
                            ? new Date(item.createdOn).toLocaleDateString()
                            : '-'}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
