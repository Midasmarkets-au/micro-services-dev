'use client';

import { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { getRewardRebateList } from '@/actions';
import { RewardRebateStatus } from '@/types/eventshop';
import type { RewardRebate } from '@/types/eventshop';
import { DateDisplay, Tag, Select, SelectTrigger, SelectValue, SelectContent, SelectItem, Tabs, DataTable, Pagination } from '@/components/ui';
import type { TagVariant, DataTableColumn } from '@/components/ui';

const STATUS_TABS = [
  { key: 'all', value: undefined },
  { key: 'succeed', value: RewardRebateStatus.Succeed },
  { key: 'processing', value: RewardRebateStatus.Processing },
  { key: 'pending', value: RewardRebateStatus.Pending },
  { key: 'failed', value: RewardRebateStatus.Failed },
] as const;

export function RewardDetails() {
  const t = useTranslations('eventshop');
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

  const columns = useMemo<DataTableColumn<RewardRebate>[]>(() => [
    {
      key: 'ticket',
      title: t('columns.ticket'),
      skeletonWidth: 'w-12',
      render: (item) => <span className="text-sm text-text-primary">{item.ticket}</span>,
    },
    {
      key: 'symbol',
      title: t('columns.symbol'),
      skeletonWidth: 'w-16',
      render: (item) => <span className="text-sm text-text-secondary">{item.symbol}</span>,
    },
    {
      key: 'lot',
      title: t('columns.lot'),
      skeletonWidth: 'w-10',
      render: (item) => <span className="text-sm text-text-secondary">{getLot(item.amount)}</span>,
    },
    {
      key: 'openTime',
      title: t('columns.openTime'),
      skeletonWidth: 'w-28',
      render: (item) => <DateDisplay value={item.openAt} className="text-sm text-text-secondary" />,
    },
    {
      key: 'closeTime',
      title: t('columns.closeTime'),
      skeletonWidth: 'w-28',
      render: (item) => <DateDisplay value={item.closeAt} className="text-sm text-text-secondary" />,
    },
    {
      key: 'status',
      title: t('columns.status'),
      skeletonWidth: 'w-16',
      render: (item) => (
        <Tag variant={getStatusVariant(item.status)} soft>
          {getStatusLabel(item.status)}
        </Tag>
      ),
    },
    {
      key: 'amount',
      title: t('columns.amount'),
      skeletonWidth: 'w-16',
      render: (item) => (
        <span className="text-sm font-semibold text-text-primary">
          US${typeof item.amount === 'number' ? item.amount.toLocaleString(undefined, { minimumFractionDigits: 2 }) : item.amount}
        </span>
      ),
    },
    {
      key: 'createdOn',
      title: t('columns.createdOn'),
      skeletonWidth: 'w-28',
      render: (item) => <DateDisplay value={item.createdOn} className="text-sm text-text-secondary" />,
    },
  ], [t]);

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

      <DataTable<RewardRebate>
        columns={columns}
        data={items}
        rowKey={(item, idx) => item.hashId || idx}
        loading={isLoading}
        skeletonRows={5}
      />

      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />
    </div>
  );
}
