'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { getRewardRebateList } from '@/actions';
import { RewardRebateStatus } from '@/types/eventshop';
import type { RewardRebate } from '@/types/eventshop';
import { DateDisplay, Tag, Select, SelectTrigger, SelectValue, SelectContent, SelectItem, Tabs } from '@/components/ui';
import type { TagVariant } from '@/components/ui';

const STATUS_TABS = [
  { key: 'all', value: undefined },
  { key: 'succeed', value: RewardRebateStatus.Succeed },
  { key: 'processing', value: RewardRebateStatus.Processing },
  { key: 'pending', value: RewardRebateStatus.Pending },
  { key: 'failed', value: RewardRebateStatus.Failed },
] as const;

export function RewardDetails() {
  const t = useTranslations('eventshop');
  const { theme } = useTheme();
  const [items, setItems] = useState<RewardRebate[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [activeStatus, setActiveStatus] = useState<number | undefined>(undefined);
  const [sortOrder, setSortOrder] = useState<'desc' | 'asc'>('desc');
  const [isLoading, setIsLoading] = useState(true);
  const isLoadedRef = useRef(false);
  const pageSize = 10;

  const loadItems = useCallback(async (p: number, status?: number, sort: 'desc' | 'asc' = 'desc') => {
    setIsLoading(true);
    try {
      const criteria: Record<string, string | number> = {
        page: p,
        size: pageSize,
        sortField: 'createdOn',
        sortOrder: sort === 'desc' ? 0 : 1,
        eventKey: 'EventShop',
      };
      if (status !== undefined) criteria.status = status;
      const result = await getRewardRebateList(criteria);
      if (result.success && result.data) {
        setItems(result.data.items);
        setTotal(result.data.total);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (isLoadedRef.current) return;
    isLoadedRef.current = true;
    loadItems(1);
  }, [loadItems]);

  const handleStatusChange = (status?: number) => {
    setActiveStatus(status);
    setPage(1);
    loadItems(1, status, sortOrder);
  };

  const handleSortChange = (value: string) => {
    const sort = value as 'desc' | 'asc';
    setSortOrder(sort);
    setPage(1);
    loadItems(1, activeStatus, sort);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    loadItems(p, activeStatus, sortOrder);
  };

  const getStatusLabel = (status: number): string => {
    const map: Record<number, string> = {
      [RewardRebateStatus.Pending]: t('rebateStatus.pending'),
      [RewardRebateStatus.Processing]: t('rebateStatus.processing'),
      [RewardRebateStatus.Succeed]: t('rebateStatus.succeed'),
      [RewardRebateStatus.Failed]: t('rebateStatus.failed'),
    };
    return map[status] || String(status);
  };

  const getStatusVariant = (status: number): TagVariant => {
    const map: Record<number, TagVariant> = {
      [RewardRebateStatus.Pending]: 'info',
      [RewardRebateStatus.Processing]: 'danger',
      [RewardRebateStatus.Succeed]: 'success',
      [RewardRebateStatus.Failed]: 'danger',
    };
    return map[status] || 'info';
  };

  const getLot = (amount: number) => {
    return (amount / 100).toFixed(1);
  };

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="flex-1 bg-surface rounded flex flex-col gap-5 overflow-hidden p-5 min-w-0">
      {/* Status Tabs + Sort */}
      <div className="flex flex-wrap items-end gap-5 border-b border-border pb-0">
        <Tabs
          tabs={STATUS_TABS.map((tab) => ({
            key: tab.key,
            label: tab.key === 'all' ? t('shop.all') : t(`rebateStatus.${tab.key}`),
          }))}
          activeKey={STATUS_TABS.find((t) => t.value === activeStatus)?.key || 'all'}
          onChange={(key) => {
            const found = STATUS_TABS.find((t) => t.key === key);
            handleStatusChange(found?.value);
          }}
          size="lg"
          showDivider={false}
        />
        <div className="mb-3 ml-auto">
          <Select value={sortOrder} onValueChange={handleSortChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]! shrink-0 rounded border border-border bg-surface px-3 text-sm">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="desc">{t('filter.newestFirst')}</SelectItem>
              <SelectItem value="asc">{t('filter.oldestFirst')}</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Scrollable Table */}
      <div className="overflow-x-auto flex-1 min-w-0">
        <div className="min-w-[900px]">
          {/* Column Headers */}
          <div className="grid grid-cols-[0.8fr_1fr_0.6fr_1.2fr_1.2fr_0.8fr_1fr_1.2fr] gap-3 text-sm text-text-tertiary px-3">
            <span>{t('columns.ticket')}</span>
            <span>{t('columns.symbol')}</span>
            <span>{t('columns.lot')}</span>
            <span>{t('columns.openTime')}</span>
            <span>{t('columns.closeTime')}</span>
            <span>{t('columns.status')}</span>
            <span>{t('columns.amount')}</span>
            <span>{t('columns.createdOn')}</span>
          </div>
          <div className="h-px bg-border mt-3" />

          {/* Data Rows */}
          {isLoading ? (
            <div className="flex flex-col">
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="grid grid-cols-[0.8fr_1fr_0.6fr_1.2fr_1.2fr_0.8fr_1fr_1.2fr] gap-3 items-center px-3 py-4 border-b border-border last:border-b-0 animate-pulse">
                  <div className="h-4 w-12 rounded bg-surface-secondary" />
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-4 w-10 rounded bg-surface-secondary" />
                  <div className="h-4 w-28 rounded bg-surface-secondary" />
                  <div className="h-4 w-28 rounded bg-surface-secondary" />
                  <div className="h-5 w-16 rounded-sm bg-surface-secondary" />
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-4 w-28 rounded bg-surface-secondary" />
                </div>
              ))}
            </div>
          ) : items.length === 0 ? (
            <div className="flex flex-1 flex-col items-center justify-center gap-2.5 py-24">
              <Image
                src={theme === 'dark' ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
                alt="" width={150} height={150}
              />
              <p className="text-base text-text-tertiary">{t('noData')}</p>
            </div>
          ) : (
            <div className="flex flex-col">
              {items.map((item, idx) => (
                <div key={item.hashId || idx} className="grid grid-cols-[0.8fr_1fr_0.6fr_1.2fr_1.2fr_0.8fr_1fr_1.2fr] gap-3 items-center px-3 py-4 border-b border-border last:border-b-0">
                  <span className="text-sm text-text-primary">{item.ticket}</span>
                  <span className="text-sm text-text-secondary">{item.symbol}</span>
                  <span className="text-sm text-text-secondary">{getLot(item.amount)}</span>
                  <DateDisplay value={item.openAt} className="text-sm text-text-secondary" />
                  <DateDisplay value={item.closeAt} className="text-sm text-text-secondary" />
                  <Tag variant={getStatusVariant(item.status)} soft>
                    {getStatusLabel(item.status)}
                  </Tag>
                  <span className="text-sm font-semibold text-text-primary">
                    US${typeof item.amount === 'number' ? item.amount.toLocaleString(undefined, { minimumFractionDigits: 2 }) : item.amount}
                  </span>
                  <DateDisplay value={item.createdOn} className="text-sm text-text-secondary" />
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-end items-center gap-1 pt-3">
          <button
            disabled={page <= 1}
            onClick={() => handlePageChange(page - 1)}
            className="size-8 flex items-center justify-center rounded border border-border text-sm disabled:opacity-40 cursor-pointer"
          >
            &lt;
          </button>
          {Array.from({ length: Math.min(totalPages, 5) }, (_, i) => i + 1).map((p) => (
            <button
              key={p}
              onClick={() => handlePageChange(p)}
              className={`size-8 flex items-center justify-center rounded text-sm cursor-pointer ${
                p === page ? 'bg-primary text-white' : 'border border-border text-text-secondary'
              }`}
            >
              {p}
            </button>
          ))}
          {totalPages > 5 && <span className="px-1 text-text-tertiary">...</span>}
          {totalPages > 5 && (
            <button
              onClick={() => handlePageChange(totalPages)}
              className={`size-8 flex items-center justify-center rounded text-sm cursor-pointer border border-border text-text-secondary`}
            >
              {totalPages}
            </button>
          )}
          <button
            disabled={page >= totalPages}
            onClick={() => handlePageChange(page + 1)}
            className="size-8 flex items-center justify-center rounded border border-border text-sm disabled:opacity-40 cursor-pointer"
          >
            &gt;
          </button>
        </div>
      )}
    </div>
  );
}
