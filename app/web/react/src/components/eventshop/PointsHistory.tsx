'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { getPointsHistory, getMediaUrl } from '@/actions';
import { PointTransactionStatus, PointTransactionSource } from '@/types/eventshop';
import type { PointTransaction } from '@/types/eventshop';
import { Button, Input, DateDisplay, Tag, Select, SelectTrigger, SelectValue, SelectContent, SelectItem, Tabs } from '@/components/ui';
import type { TagVariant } from '@/components/ui';
import { ShopPoints } from './ShopPoints';

const STATUS_TABS = [
  { key: 'all', value: undefined },
  { key: 'success', value: PointTransactionStatus.Success },
  { key: 'fail', value: PointTransactionStatus.Fail },
  { key: 'pending', value: PointTransactionStatus.Pending },
] as const;

export function PointsHistoryTab() {
  const t = useTranslations('eventshop');
  const { theme } = useTheme();
  const [items, setItems] = useState<PointTransaction[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [activeStatus, setActiveStatus] = useState<number | undefined>(undefined);
  const [searchText, setSearchText] = useState('');
  const [sortOrder, setSortOrder] = useState<'desc' | 'asc'>('desc');
  const [isLoading, setIsLoading] = useState(true);
  const [imageUrls, setImageUrls] = useState<Record<string, string>>({});
  const isLoadedRef = useRef(false);
  const pageSize = 10;

  const loadItems = useCallback(async (p: number, status?: number, search?: string, sort: 'desc' | 'asc' = 'desc') => {
    setIsLoading(true);
    try {
      const criteria: Record<string, string | number> = {
        page: p,
        size: pageSize,
        sortFeild: 'createdOn',
        sortOrder: sort === 'desc' ? 0 : 1,
        eventKey: 'EventShop',
      };
      if (status !== undefined) criteria.status = status;
      if (search) criteria.eventShopItemName = search;
      const result = await getPointsHistory(criteria);
      if (result.success && result.data) {
        setItems(result.data.items);
        setTotal(result.data.total);
        const guids = result.data.items
          .flatMap((item) => item.eventShopItemImages || [])
          .filter(Boolean);
        if (guids.length > 0) loadImages(guids);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  const loadImages = async (guids: string[]) => {
    const urls: Record<string, string> = {};
    await Promise.all(
      guids.map(async (guid) => {
        const result = await getMediaUrl(guid);
        if (result.success && result.data) urls[guid] = result.data;
      })
    );
    setImageUrls((prev) => ({ ...prev, ...urls }));
  };

  useEffect(() => {
    if (isLoadedRef.current) return;
    isLoadedRef.current = true;
    loadItems(1);
  }, [loadItems]);

  const handleStatusChange = (status?: number) => {
    setActiveStatus(status);
    setPage(1);
    loadItems(1, status, searchText, sortOrder);
  };

  const handleSearch = () => {
    setPage(1);
    loadItems(1, activeStatus, searchText, sortOrder);
  };

  const handleReset = () => {
    setSearchText('');
    setActiveStatus(undefined);
    setSortOrder('desc');
    setPage(1);
    loadItems(1);
  };

  const handleSortChange = (value: string) => {
    const sort = value as 'desc' | 'asc';
    setSortOrder(sort);
    setPage(1);
    loadItems(1, activeStatus, searchText, sort);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    loadItems(p, activeStatus, searchText, sortOrder);
  };

  const getSourceLabel = (sourceType: number): string => {
    const map: Record<number, string> = {
      [PointTransactionSource.OpenAccount]: t('pointSource.openAccount'),
      [PointTransactionSource.Deposit]: t('pointSource.deposit'),
      [PointTransactionSource.Trade]: t('pointSource.trade'),
      [PointTransactionSource.Adjust]: t('pointSource.adjust'),
      [PointTransactionSource.Purchase]: t('pointSource.purchase'),
    };
    return map[sourceType] || String(sourceType);
  };

  const getStatusLabel = (status: number): string => {
    const map: Record<number, string> = {
      [PointTransactionStatus.Pending]: t('pointStatus.pending'),
      [PointTransactionStatus.Success]: t('pointStatus.success'),
      [PointTransactionStatus.Fail]: t('pointStatus.fail'),
    };
    return map[status] || String(status);
  };

  const getStatusVariant = (status: number): TagVariant => {
    const map: Record<number, TagVariant> = {
      [PointTransactionStatus.Pending]: 'info',
      [PointTransactionStatus.Success]: 'success',
      [PointTransactionStatus.Fail]: 'danger',
    };
    return map[status] || 'info';
  };

  const renderItemImage = (item: PointTransaction) => {
    if (item.sourceType === PointTransactionSource.Trade) {
      return (
        <div className="shrink-0 size-10 rounded overflow-hidden flex items-center justify-center bg-amber-50 dark:bg-amber-900/20">
          <span className="text-[10px] font-bold text-amber-600 dark:text-amber-400 border border-amber-400 dark:border-amber-500 rounded px-1">Trade</span>
        </div>
      );
    }
    const imgGuid = item.eventShopItemImages?.[0];
    return (
      <div className="shrink-0 size-10 rounded overflow-hidden relative bg-surface-secondary">
        {imgGuid && imageUrls[imgGuid] && (
          <Image src={imageUrls[imgGuid]} alt="" fill className="object-cover" unoptimized />
        )}
      </div>
    );
  };

  const renderItemName = (item: PointTransaction) => {
    if (item.sourceType === PointTransactionSource.Trade) {
      return t('tradePoints');
    }
    return item.eventShopItemName || item.description || '-';
  };

  const renderType = (item: PointTransaction) => {
    if (item.sourceType === PointTransactionSource.Trade && item.ticket) {
      return `${getSourceLabel(item.sourceType)}(${item.ticket})`;
    }
    return getSourceLabel(item.sourceType);
  };

  const getPointColor = (point: number) =>
    point >= 0 ? 'text-[#333] dark:text-text-primary' : 'text-text-secondary';

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="flex-1 bg-surface rounded flex flex-col gap-5 overflow-hidden p-5 min-w-0">
      {/* Status Tabs + Filter Bar */}
      <div className="flex flex-wrap items-end gap-5 border-b border-border pb-0">
        <Tabs
          tabs={STATUS_TABS.map((tab) => ({
            key: tab.key,
            label: tab.key === 'all' ? t('shop.all') : t(`pointStatus.${tab.key}`),
          }))}
          activeKey={STATUS_TABS.find((t) => t.value === activeStatus)?.key || 'all'}
          onChange={(key) => {
            const found = STATUS_TABS.find((t) => t.key === key);
            handleStatusChange(found?.value);
          }}
          size="lg"
          showDivider={false}
        />

        <div className="flex items-center gap-3 mb-3 ml-auto shrink-0">
          <Select value={sortOrder} onValueChange={handleSortChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]! shrink-0 rounded border border-border bg-surface px-3 text-sm">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="desc">{t('filter.newestFirst')}</SelectItem>
              <SelectItem value="asc">{t('filter.oldestFirst')}</SelectItem>
            </SelectContent>
          </Select>
          <div className="w-[200px] shrink-0 relative">
            <Image src="/images/icons/search.svg" alt="" width={16} height={16} className="absolute left-3 top-1/2 -translate-y-1/2 z-10 pointer-events-none" />
            <Input
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              placeholder={t('filter.searchPlaceholder')}
              inputSize="sm"
              className="pl-9!"
            />
          </div>
          <Button size="sm" onClick={handleReset} className="shrink-0 whitespace-nowrap bg-[#000f32] text-white hover:bg-[#000f32]/90">
            {t('filter.reset')}
          </Button>
          <Button variant="primary" size="sm" onClick={handleSearch} className="shrink-0 whitespace-nowrap">
            {t('filter.search')}
          </Button>
        </div>
      </div>

      {/* Scrollable Table */}
      <div className="overflow-x-auto flex-1 min-w-0">
        <div className="min-w-[700px]">
          {/* Column Headers */}
          <div className="grid grid-cols-[2fr_1fr_1fr_1.5fr_1fr] gap-3 text-sm text-text-tertiary px-3">
            <span>{t('columns.itemName')}</span>
            <span>{t('columns.type')}</span>
            <span>{t('columns.points')}</span>
            <span>{t('columns.time')}</span>
            <span>{t('columns.status')}</span>
          </div>
          <div className="h-px bg-border mt-3" />

          {/* Data Rows */}
          {isLoading ? (
            <div className="flex flex-col">
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="grid grid-cols-[2fr_1fr_1fr_1.5fr_1fr] gap-3 items-center px-3 py-4 border-b border-border last:border-b-0 animate-pulse">
                  <div className="flex items-center gap-3">
                    <div className="shrink-0 size-10 rounded bg-surface-secondary" />
                    <div className="h-4 w-24 rounded bg-surface-secondary" />
                  </div>
                  <div className="h-4 w-16 rounded bg-surface-secondary" />
                  <div className="h-4 w-14 rounded bg-surface-secondary" />
                  <div className="h-4 w-28 rounded bg-surface-secondary" />
                  <div className="h-5 w-16 rounded-sm bg-surface-secondary" />
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
                <div key={item.hashId || idx} className="grid grid-cols-[2fr_1fr_1fr_1.5fr_1fr] gap-3 items-center px-3 py-4 border-b border-border last:border-b-0">
                  <div className="flex items-center gap-3 min-w-0">
                    {renderItemImage(item)}
                    <span className="text-sm text-text-primary truncate">{renderItemName(item)}</span>
                  </div>
                  <span className="text-sm text-text-secondary">{renderType(item)}</span>
                  <ShopPoints value={item.point} showSign className={`text-sm font-semibold ${getPointColor(item.point)}`} />
                  <DateDisplay value={item.createdOn} className="text-sm text-text-secondary" />
                  <Tag variant={getStatusVariant(item.status)} soft>
                    {getStatusLabel(item.status)}
                  </Tag>
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
