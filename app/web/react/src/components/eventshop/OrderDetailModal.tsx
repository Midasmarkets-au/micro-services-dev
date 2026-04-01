'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getOrderDetail, getMediaUrl, confirmDelivery } from '@/actions';
import { OrderStatus } from '@/types/eventshop';
import type { ShopOrder } from '@/types/eventshop';
import { ShopPoints } from './ShopPoints';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogTitle,
  Button,
  formatDateValue,
} from '@/components/ui';

interface OrderDetailModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  orderHashId: string | null;
  onRefresh?: () => void;
}

export function OrderDetailModal({ open, onOpenChange, orderHashId, onRefresh }: OrderDetailModalProps) {
  const t = useTranslations('eventshop');
  const { execute } = useServerAction({ showErrorToast: true });
  const [order, setOrder] = useState<ShopOrder | null>(null);
  const [imageUrl, setImageUrl] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isConfirming, setIsConfirming] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (!open || !orderHashId) return;
    setOrder(null);
    setImageUrl('');
    setCopied(false);

    const fetchData = async () => {
      setIsLoading(true);
      try {
        const result = await execute(getOrderDetail, orderHashId);
        if (result.success && result.data) {
          setOrder(result.data);
          const guid = result.data.eventShopItemImages?.[0];
          if (guid) {
            const imgResult = await getMediaUrl(guid);
            if (imgResult.success && imgResult.data) {
              setImageUrl(imgResult.data);
            }
          }
        }
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, [open, orderHashId, execute]);

  const handleConfirmDelivery = async () => {
    if (!order) return;
    setIsConfirming(true);
    try {
      const result = await execute(confirmDelivery, order.hashId);
      if (result.success) {
        onRefresh?.();
        onOpenChange(false);
      }
    } finally {
      setIsConfirming(false);
    }
  };

  const handleCopyTracking = (text: string) => {
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const getStatusLabel = (status: number) => {
    const map: Record<number, string> = {
      [OrderStatus.Pending]: t('orderStatus.pending'),
      [OrderStatus.Processing]: t('orderStatus.processing'),
      [OrderStatus.Shipped]: t('orderStatus.shipped'),
      [OrderStatus.Succeed]: t('orderStatus.succeed'),
      [OrderStatus.Cancelled]: t('orderStatus.cancelled'),
    };
    return map[status] || String(status);
  };

  const buildFullAddress = () => {
    if (!order?.address) return '-';
    const addr = order.address;
    const parts = [addr.country, addr.content?.state, addr.content?.city, addr.content?.address].filter(Boolean);
    return parts.join('-') || '-';
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[800px] flex flex-col gap-10 p-5">
        <DialogTitle className="sr-only">{t('orderDetail.title')}</DialogTitle>
        {isLoading || !order ? (
          <div className="flex flex-col gap-10 animate-pulse">
            {/* Header skeleton */}
            <div className="flex items-center justify-between">
              <div className="h-6 w-32 rounded bg-surface-secondary" />
              <div className="h-5 w-16 rounded bg-surface-secondary" />
            </div>
            {/* Product skeleton */}
            <div className="flex flex-col gap-5">
              <div className="h-4 w-20 rounded bg-surface-secondary" />
              <div className="flex gap-3 items-center">
                <div className="shrink-0 size-20 rounded-lg bg-surface-secondary" />
                <div className="flex flex-col gap-3 flex-1">
                  <div className="h-4 w-40 rounded bg-surface-secondary" />
                  <div className="flex items-center gap-5">
                    <div className="h-4 w-20 rounded bg-surface-secondary" />
                    <div className="h-4 w-8 rounded bg-surface-secondary" />
                  </div>
                </div>
              </div>
              <div className="h-px bg-border" />
              {/* Basic Info skeleton */}
              <div className="flex flex-col gap-5">
                <div className="h-4 w-24 rounded bg-surface-secondary" />
                {Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="flex items-center justify-between">
                    <div className="h-4 w-20 rounded bg-surface-secondary" />
                    <div className="h-4 w-32 rounded bg-surface-secondary" />
                  </div>
                ))}
              </div>
              <div className="h-px bg-border" />
              {/* Order Info skeleton */}
              <div className="flex flex-col gap-5">
                <div className="h-4 w-24 rounded bg-surface-secondary" />
                {Array.from({ length: 5 }).map((_, i) => (
                  <div key={i} className="flex items-center justify-between">
                    <div className="h-4 w-20 rounded bg-surface-secondary" />
                    <div className="h-4 w-36 rounded bg-surface-secondary" />
                  </div>
                ))}
              </div>
              <div className="h-px bg-border" />
            </div>
            {/* Footer skeleton */}
            <div className="flex justify-end">
              <div className="h-10 w-[120px] rounded bg-surface-secondary" />
            </div>
          </div>
        ) : (
          <>
            {/* Header */}
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-semibold text-text-primary">
                {t('orderDetail.title')}
              </h2>
              <span className="text-base text-text-secondary dark:text-text-tertiary">{getStatusLabel(order.status)}</span>
            </div>

            {/* Product Info */}
            <div className="flex flex-col gap-5">
              {order.eventShopItemCategory && (
                <span className="text-sm text-[#333] dark:text-text-primary px-1">{order.eventShopItemCategory}</span>
              )}
              <div className="flex gap-3 items-center">
                <div className="shrink-0 size-20 rounded-lg overflow-hidden relative bg-surface-secondary">
                  {imageUrl && (
                    <Image src={imageUrl} alt="" fill className="object-cover" unoptimized />
                  )}
                </div>
                <div className="flex flex-col justify-between flex-1 min-h-[52px]">
                  <span className="text-sm text-text-primary">{order.eventShopItemName}</span>
                  <div className="flex items-center gap-5">
                    <ShopPoints value={order.totalPoint} className="text-sm font-bold text-text-primary" />
                    <span className="text-sm text-text-primary">x{order.quantity}</span>
                  </div>
                </div>
              </div>

              <div className="h-px bg-border" />

              {/* Basic Info */}
              <div className="flex flex-col gap-5">
                <span className="text-sm font-semibold text-[#333] dark:text-text-primary">{t('orderDetail.basicInfo')}</span>
                <InfoRow label={t('orderDetail.recipient')} value={order.address?.name || '-'} />
                <InfoRow label={t('orderDetail.address')} value={buildFullAddress()} />
                <InfoRow label={t('orderDetail.postalCode')} value={order.address?.content?.postalCode || '-'} />
                <InfoRow label={t('orderDetail.phone')} value={order.address ? `${order.address.ccc} ${order.address.phone}` : '-'} />
              </div>

              <div className="h-px bg-border" />

              {/* Order Info */}
              <div className="flex flex-col gap-5">
                <span className="text-sm font-semibold text-[#333] dark:text-text-primary">{t('orderDetail.orderInfo')}</span>
                <InfoRow label={t('orderDetail.shippedTime')} value={order.shippedOn ? formatDateValue(order.shippedOn) : '-'} />
                <InfoRow label={t('orderDetail.exchangeTime')} value={order.updatedOn ? formatDateValue(order.updatedOn) : '-'} />
                <InfoRow label={t('orderDetail.createdTime')} value={order.createdOn ? formatDateValue(order.createdOn) : '-'} />
                <InfoRow label={t('orderDetail.orderNumber')} value={order.hashId} />
                <div className="flex items-center justify-between">
                  <span className="text-sm text-[#666] dark:text-text-secondary w-[117px] shrink-0">{t('orderDetail.trackingNumber')}</span>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-text-secondary dark:text-text-tertiary">
                      {order.shipping?.trackingNumber || t('orderDetail.waitingForShipment')}
                    </span>
                    {order.shipping?.trackingNumber && (
                      <button
                        onClick={() => handleCopyTracking(order.shipping!.trackingNumber!)}
                        className="text-text-tertiary hover:text-text-primary cursor-pointer"
                        title={copied ? t('orderDetail.copied') : 'Copy'}
                      >
                        {copied ? (
                          <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M13 4L6 12L3 9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                          </svg>
                        ) : (
                          <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <rect x="7" y="7" width="10" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.5"/>
                            <path d="M13 7V4.5C13 3.67 12.33 3 11.5 3H4.5C3.67 3 3 3.67 3 4.5V11.5C3 12.33 3.67 13 4.5 13H7" stroke="currentColor" strokeWidth="1.5"/>
                          </svg>
                        )}
                      </button>
                    )}
                  </div>
                </div>
              </div>

              <div className="h-px bg-border" />
            </div>

            {/* Footer */}
            <DialogFooter>
              <div className="flex justify-end gap-5">
                {order.status === OrderStatus.Shipped && (
                  <Button
                    variant="outline"
                    onClick={handleConfirmDelivery}
                    disabled={isConfirming}
                  >
                    {isConfirming ? '...' : t('orderDetail.confirmDelivered')}
                  </Button>
                )}
                <Button variant="outline" className="w-[120px]" onClick={() => onOpenChange(false)}>
                  {t('orderDetail.close')}
                </Button>
              </div>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between">
      <span className="text-sm text-[#666] dark:text-text-secondary w-[117px] shrink-0">{label}</span>
      <span className="text-sm text-text-secondary dark:text-text-tertiary">{value}</span>
    </div>
  );
}
