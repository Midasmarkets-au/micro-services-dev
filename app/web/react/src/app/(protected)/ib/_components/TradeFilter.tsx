'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { Button, Input, SimpleSelect } from '@/components/ui';

interface TradeFilterProps {
  onSearch: (params: { from?: string; to?: string; searchText?: string; size?: number }) => void;
  onReset: () => void;
  isLoading?: boolean;
  showSearch?: boolean;
  showDateRange?: boolean;
  showPageSize?: boolean;
  pageSizes?: number[];
  defaultPageSize?: number;
}

export function TradeFilter({
  onSearch,
  onReset,
  isLoading = false,
  showSearch = true,
  showDateRange = true,
  showPageSize = true,
  pageSizes = [10, 15, 20, 25, 50],
  defaultPageSize = 15,
}: TradeFilterProps) {
  const t = useTranslations('ib');
  const [searchText, setSearchText] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [pageSize, setPageSize] = useState(defaultPageSize);

  const handleSearch = () => {
    onSearch({
      from: startDate || undefined,
      to: endDate || undefined,
      searchText: searchText || undefined,
      size: pageSize,
    });
  };

  const handleReset = () => {
    setSearchText('');
    setStartDate('');
    setEndDate('');
    setPageSize(defaultPageSize);
    onReset();
  };

  return (
    <div className="flex flex-wrap items-end gap-3">
      {showSearch && (
        <div className="min-w-48">
          <Input
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            placeholder={t('tip.searchKeyWords')}
            disabled={isLoading}
            className="h-10"
          />
        </div>
      )}
      {showDateRange && (
        <>
          <div className="min-w-36">
            <Input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              disabled={isLoading}
              className="h-10"
              label={t('fields.startDate')}
            />
          </div>
          <div className="min-w-36">
            <Input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              disabled={isLoading}
              className="h-10"
              label={t('fields.endDate')}
            />
          </div>
        </>
      )}
      {showPageSize && (
        <div className="flex items-center gap-2">
          <label className="text-sm text-text-secondary">{t('fields.pageSize')}</label>
          <SimpleSelect
            value={String(pageSize)}
            onChange={(value) => setPageSize(Number(value))}
            disabled={isLoading}
            triggerSize="sm"
            className="min-w-20"
            options={pageSizes.map((s) => ({
              value: String(s),
              label: String(s),
            }))}
          />
        </div>
      )}
      <div className="flex gap-2">
        <Button size="sm" onClick={handleSearch} disabled={isLoading}>
          {t('action.search')}
        </Button>
        <Button size="sm" variant="secondary" onClick={handleReset} disabled={isLoading}>
          {t('action.reset')}
        </Button>
      </div>
    </div>
  );
}
