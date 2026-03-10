'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { getShopItems, getShopCategories, getMediaUrl } from '@/actions';
import { Button, Tabs } from '@/components/ui';
import type { ShopItem } from '@/types/eventshop';
import { ShopPoints } from './ShopPoints';
import { ExchangeModal } from './ExchangeModal';

interface CategoryEntry {
  id: string;
  name: string;
}

interface ProductGridProps {
  userPoints?: number;
  onExchangeSuccess?: () => void;
}

export function ProductGrid({ userPoints = 0, onExchangeSuccess }: ProductGridProps) {
  const t = useTranslations('eventshop');
  const { theme } = useTheme();
  const [categories, setCategories] = useState<CategoryEntry[]>([]);
  const [items, setItems] = useState<ShopItem[]>([]);
  const [activeCategory, setActiveCategory] = useState<string>('');
  const [isLoading, setIsLoading] = useState(true);
  const [imageUrls, setImageUrls] = useState<Record<string, string>>({});
  const [exchangeItemId, setExchangeItemId] = useState<string | null>(null);
  const [exchangeOpen, setExchangeOpen] = useState(false);
  const isLoadedRef = useRef(false);

  const loadCategories = useCallback(async () => {
    const result = await getShopCategories();
    if (result.success && result.data) {
      const entries = Object.entries(result.data).map(([id, name]) => ({ id, name }));
      setCategories(entries);
    }
  }, []);

  const loadItems = useCallback(async (category?: string) => {
    setIsLoading(true);
    try {
      const criteria: Record<string, string | number> = {
        type: 0,
        page: 1,
        size: 50,
        sortField: 'createdOn',
        eventKey: 'EventShop',
      };
      if (category) {
        criteria.category = category;
      }
      const result = await getShopItems(criteria);
      if (result.success && result.data?.items) {
        setItems(result.data.items);
        const guids = result.data.items
          .flatMap((item) => item.images || [])
          .filter(Boolean);
        loadImages(guids);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  const loadImages = async (guids: string[]) => {
    const urls: Record<string, string> = {};
    await Promise.all(
      guids.map(async (guid) => {
        if (!guid) return;
        const result = await getMediaUrl(guid);
        if (result.success && result.data) {
          urls[guid] = result.data;
        }
      })
    );
    setImageUrls((prev) => ({ ...prev, ...urls }));
  };

  useEffect(() => {
    if (isLoadedRef.current) return;
    isLoadedRef.current = true;
    loadCategories();
    loadItems();
  }, [loadCategories, loadItems]);

  const handleCategoryChange = (category: string) => {
    setActiveCategory(category);
    loadItems(category || undefined);
  };

  return (
    <div className="bg-surface rounded flex-1 flex flex-col gap-5 overflow-hidden p-5">
      {/* 分类 Tabs */}
      <Tabs
        tabs={[
          { key: '', label: t('shop.all') },
          ...categories.map((cat) => ({ key: cat.id, label: cat.name })),
        ]}
        activeKey={activeCategory}
        onChange={handleCategoryChange}
        size="lg"
      />

      {/* 商品列表 */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="flex gap-3 items-center border border-border rounded-xl overflow-hidden p-5 md:p-10">
              <div className="animate-pulse rounded-2xl bg-surface-secondary shrink-0 size-[100px] md:size-[150px]" />
              <div className="flex flex-col justify-between flex-1 py-2 md:py-5 min-h-[100px] md:min-h-[150px]">
                <div className="animate-pulse rounded bg-surface-secondary h-4 w-3/4" />
                <div className="flex items-center justify-between">
                  <div className="animate-pulse rounded bg-surface-secondary h-6 w-20" />
                  <div className="animate-pulse rounded bg-surface-secondary h-8 w-20" />
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-1 flex-col items-center justify-center gap-2.5 py-24">
          <Image
            src={theme === 'dark' ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
            alt=""
            width={150}
            height={150}
          />
          <p className="text-base text-text-tertiary">{t('shop.noItems')}</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {items.map((item) => (
            <div
              key={item.hashId}
              className="border border-border rounded-xl overflow-hidden p-5 md:p-10"
            >
              <div className="flex gap-3 items-center w-full">
                <div className="bg-surface rounded-2xl shrink-0 size-[100px] md:size-[150px] overflow-hidden relative">
                  {item.images?.[0] && imageUrls[item.images[0]] ? (
                    <Image
                      src={imageUrls[item.images[0]]}
                      alt={item.name}
                      fill
                      className="object-cover"
                      unoptimized
                    />
                  ) : (
                    <div className="size-full bg-surface-secondary" />
                  )}
                </div>
                <div className="flex flex-1 flex-col h-full justify-between py-2 md:py-5 min-h-[100px] md:min-h-[150px]">
                  <p className="text-sm font-medium text-text-primary leading-normal line-clamp-2">
                    {item.name}
                  </p>
                  <div className="flex items-center justify-between w-full">
                    <ShopPoints value={item.point} showIcon={false} className="text-xl font-bold text-text-primary" />
                    <Button
                      variant="primary"
                      size="xs"
                      className="w-20 rounded"
                      onClick={() => { setExchangeItemId(item.hashId); setExchangeOpen(true); }}
                    >
                      {t('shop.exchange')}
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <ExchangeModal
        open={exchangeOpen}
        onOpenChange={setExchangeOpen}
        itemHashId={exchangeItemId}
        userPoints={userPoints}
        onSuccess={() => { loadItems(activeCategory || undefined); onExchangeSuccess?.(); }}
      />
    </div>
  );
}
