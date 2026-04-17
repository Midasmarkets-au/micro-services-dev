'use client';

import { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { getShopOrderList, getMediaUrl } from '@/actions';
import { Button, Input, Tag, Select, SelectTrigger, SelectValue, SelectContent, SelectItem, Tabs, DataTable, Pagination } from '@/components/ui';
import type { TagVariant, DataTableColumn } from '@/components/ui';
import { OrderStatus } from '@/types/eventshop';
import type { ShopOrder } from '@/types/eventshop';
import { ShopPoints } from './ShopPoints';
import { OrderDetailModal } from './OrderDetailModal';
import { TimeShow } from '@/components/TimeShow';
const STATUS_TABS = [
  { key: 'all', value: undefined },
  { key: 'pending', value: OrderStatus.Pending },
  { key: 'processing', value: OrderStatus.Processing },
  { key: 'shipped', value: OrderStatus.Shipped },
  { key: 'succeed', value: OrderStatus.Succeed },
  { key: 'cancelled', value: OrderStatus.Cancelled },
] as const;

export function OrderHistory() {
  const t = useTranslations('eventshop');
  const [orders, setOrders] = useState<ShopOrder[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [activeStatus, setActiveStatus] = useState<number | undefined>(undefined);
  const [searchText, setSearchText] = useState('');
  const [sortOrder, setSortOrder] = useState<'desc' | 'asc'>('desc');
  const [isLoading, setIsLoading] = useState(true);
  const [imageUrls, setImageUrls] = useState<Record<string, string>>({});
  const [detailOrderId, setDetailOrderId] = useState<string | null>(null);
  const [detailOpen, setDetailOpen] = useState(false);
  const isLoadedRef = useRef(false);
  const pageSize = 10;

  const loadOrders = useCallback(async (p: number, status?: number, search?: string, sort: 'desc' | 'asc' = 'desc') => {
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
      if (search) criteria.eventShopItemName = search;
      const result = await getShopOrderList(criteria);
      if (result.success && result.data) {
        setOrders(result.data.items);
        setTotal(result.data.total);
        const guids = result.data.items
          .flatMap((o) => o.eventShopItemImages || [])
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
    loadOrders(1);
  }, [loadOrders]);

  const handleStatusChange = (status?: number) => {
    setActiveStatus(status);
    setPage(1);
    loadOrders(1, status, searchText, sortOrder);
  };

  const handleSearch = () => {
    setPage(1);
    loadOrders(1, activeStatus, searchText, sortOrder);
  };

  const handleReset = () => {
    setSearchText('');
    setActiveStatus(undefined);
    setSortOrder('desc');
    setPage(1);
    loadOrders(1);
  };

  const handleSortChange = (value: string) => {
    const sort = value as 'desc' | 'asc';
    setSortOrder(sort);
    setPage(1);
    loadOrders(1, activeStatus, searchText, sort);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    loadOrders(p, activeStatus, searchText, sortOrder);
  };

  const getStatusLabel = (status: number) => {
    const map: Record<number, string> = {
      [OrderStatus.Pending]: t('orderStatus.pending'),
      [OrderStatus.Processing]: t('orderStatus.processing'),
      [OrderStatus.Shipped]: t('orderStatus.shipped'),
      [OrderStatus.Succeed]: t('orderStatus.succeed'),
      [OrderStatus.Cancelled]: t('orderStatus.cancelled'),
    };
    return map[status] || status;
  };

  const getStatusVariant = (status: number): TagVariant => {
    const map: Record<number, TagVariant> = {
      [OrderStatus.Pending]: 'info',
      [OrderStatus.Processing]: 'danger',
      [OrderStatus.Shipped]: 'success',
      [OrderStatus.Succeed]: 'success',
      [OrderStatus.Cancelled]: 'info',
    };
    return map[status] || 'info';
  };

  const columns = useMemo<DataTableColumn<ShopOrder>[]>(() => [
    {
      key: 'itemName',
      title: t('columns.itemName'),
      skeletonWidth: 'w-24',
      skeletonRender: () => (
        <div className="flex items-center gap-3">
          <div className="shrink-0 size-10 rounded bg-surface-secondary" />
          <div className="h-4 w-24 rounded bg-surface-secondary" />
        </div>
      ),
      render: (order) => {
        const imgGuid = order.eventShopItemImages?.[0];
        return (
          <div className="flex items-center gap-3 min-w-0">
            <div className="shrink-0 size-10 rounded overflow-hidden relative bg-surface-secondary">
              {imgGuid && imageUrls[imgGuid] && (
                <Image src={imageUrls[imgGuid]} alt="" fill className="object-cover" unoptimized />
              )}
            </div>
            <span className="text-sm text-text-primary truncate">{order.eventShopItemName}</span>
          </div>
        );
      },
    },
    {
      key: 'orderNumber',
      title: t('columns.orderNumber'),
      skeletonWidth: 'w-20',
      render: (order) => <span className="text-sm text-text-secondary">{order.hashId}</span>,
    },
    {
      key: 'points',
      title: t('columns.points'),
      skeletonWidth: 'w-14',
      render: (order) => <ShopPoints value={order.totalPoint} className="text-sm font-semibold text-text-primary" />,
    },
    {
      key: 'quantity',
      title: t('columns.quantity'),
      skeletonWidth: 'w-8',
      render: (order) => <span className="text-sm text-text-secondary">{order.quantity}</span>,
    },
    {
      key: 'time',
      title: t('columns.time'),
      skeletonWidth: 'w-28',
      render: (order) =><TimeShow dateIsoString={order.createdOn} type="customCSS" />,
    },
    {
      key: 'status',
      title: t('columns.status'),
      skeletonWidth: 'w-16',
      render: (order) => (
        <Tag variant={getStatusVariant(order.status)} soft>
          {getStatusLabel(order.status)}
        </Tag>
      ),
    },
    {
      key: 'action',
      title: t('columns.action'),
      skeletonWidth: 'w-10',
      render: (order) => (
        <Button
          variant="ghost"
          size="xs"
          onClick={() => { setDetailOrderId(order.hashId); setDetailOpen(true); }}
          className="text-sm text-primary hover:underline w-fit p-0 h-auto"
        >
          {t('notification.view')}
        </Button>
      ),
    },
  ], [t, imageUrls]); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div className="flex-1 bg-surface rounded flex flex-col gap-5 overflow-hidden p-5 min-w-0">
      {/* Status Tabs + Filter Bar */}
      <div className="flex flex-wrap items-end gap-5 border-b border-border pb-0">
        <Tabs
          tabs={STATUS_TABS.map((tab) => ({
            key: tab.key,
            label: tab.key === 'all' ? t('shop.all') : t(`orderStatus.${tab.key}`),
          }))}
          activeKey={STATUS_TABS.find((t) => t.value === activeStatus)?.key || 'all'}
          onChange={(key) => {
            const found = STATUS_TABS.find((t) => t.key === key);
            handleStatusChange(found?.value);
          }}
          size="lg"
          showDivider={false}
          tabsRowClassName="!gap-5 md:!gap-6"
        />

        <div className="flex items-center gap-3 mb-3 ml-auto shrink-0">
          <Select value={sortOrder} onValueChange={handleSortChange}>
            <SelectTrigger triggerSize="sm" className="w-[120px]! shrink-0 rounded border border-border bg-surface px-3 text-sm">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="desc">{t('filter.newestFirst')}</SelectItem>
              <SelectItem value="asc">{t('filter.oldestFirst')}</SelectItem>
            </SelectContent>
          </Select>
          <div className="w-[clamp(140px,calc(-3px+10.56vw),200px)] shrink-0 relative">
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

      <DataTable<ShopOrder>
        columns={columns}
        data={orders}
        rowKey={(item, idx) => item.hashId || idx}
        loading={isLoading}
        skeletonRows={5}
      />

      <Pagination page={page} total={total} size={pageSize} onPageChange={handlePageChange} />

      <OrderDetailModal
        open={detailOpen}
        onOpenChange={setDetailOpen}
        orderHashId={detailOrderId}
        onRefresh={() => loadOrders(page, activeStatus, searchText)}
      />
    </div>
  );
}
