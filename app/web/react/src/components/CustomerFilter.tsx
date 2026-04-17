'use client';

import {
  useState,
  useCallback,
  useImperativeHandle,
  forwardRef,
  useRef,
  useEffect,
} from 'react';
import { useTranslations } from 'next-intl';
import moment from 'moment';
import {
  Button,
  Input,
  DatePicker,
  Icon,
  SimpleSelect,
} from '@/components/ui';
import type { DateRange } from '@/components/ui';

// ── Types ──────────────────────────────────────────────────────────────────

export interface CustomerFilterOption {
  value: string;
  label: string;
}

export interface CustomerFilterValues {
  searchText?: string;
  multiLevel?: boolean;
  dateRange?: DateRange;
  sortOrder?: string;
}

export interface CustomerFilterParams {
  searchText?: string;
  multiLevel?: boolean;
  from?: string;
  to?: string;
  sortOrder?: string;
}

export interface CustomerFilterRef {
  search: () => void;
  reset: () => void;
  getValues: () => CustomerFilterValues;
  setValues: (values: Partial<CustomerFilterValues>) => void;
}

export interface CustomerFilterProps {
  /** 显示直属 / 全层级选择器（默认 false） */
  showMultiLevel?: boolean;
  /** 显示排序选择器（默认 false） */
  showSortOrder?: boolean;
  /** 排序选项，showSortOrder=true 时必传 */
  sortOptions?: CustomerFilterOption[];
  /** 显示日期选择器（client tab 时传 true） */
  showDatePicker?: boolean;
  /** 初始值 */
  defaultValues?: CustomerFilterValues;
  /** 搜索回调，from/to 已处理为 ISO 字符串 */
  onSearch: (params: CustomerFilterParams) => void;
  /** 重置回调（CustomerFilter 内部已重置状态，此处做页面级处理） */
  onReset?: () => void;
  /** 是否加载中 */
  isLoading?: boolean;
  /** 搜索框 placeholder */
  searchPlaceholder?: string;
}

// ── Component ───────────────────────────────────────────────────────────────

export const CustomerFilter = forwardRef<CustomerFilterRef, CustomerFilterProps>(
  function CustomerFilter(
    {
      showMultiLevel = false,
      showSortOrder = false,
      sortOptions = [],
      showDatePicker = false,
      defaultValues,
      onSearch,
      onReset,
      isLoading = false,
      searchPlaceholder,
    },
    ref,
  ) {
    const t = useTranslations('sales');

    const multiLevelOptions: CustomerFilterOption[] = [
      { value: 'false', label: t('fields.directLevel') },
      { value: 'true', label: t('fields.allLevels') },
    ];

    const [searchText, setSearchText] = useState(defaultValues?.searchText ?? '');
    const [multiLevel, setMultiLevel] = useState(defaultValues?.multiLevel ?? false);
    const [dateRange, setDateRange] = useState<DateRange | undefined>(defaultValues?.dateRange);
    const [sortOrder, setSortOrder] = useState(defaultValues?.sortOrder ?? sortOptions[0]?.value ?? '');
    const [mobileOpen, setMobileOpen] = useState(false);

    const onSearchRef = useRef(onSearch);
    useEffect(() => { onSearchRef.current = onSearch; }, [onSearch]);
    const onResetRef = useRef(onReset);
    useEffect(() => { onResetRef.current = onReset; }, [onReset]);

    const buildParams = useCallback(
      (overrides?: Partial<CustomerFilterValues>): CustomerFilterParams => {
        const text = overrides && 'searchText' in overrides ? overrides.searchText : searchText;
        const ml = overrides && 'multiLevel' in overrides ? overrides.multiLevel : multiLevel;
        const dr = overrides && 'dateRange' in overrides ? overrides.dateRange : dateRange;
        const so = overrides && 'sortOrder' in overrides ? overrides.sortOrder : sortOrder;

        const params: CustomerFilterParams = {
          searchText: text || undefined,
          multiLevel: ml,
          sortOrder: so || undefined,
        };

        if (showDatePicker) {
          if (dr?.from) params.from = moment(dr.from).startOf('day').toISOString();
          if (dr?.to) params.to = moment(dr.to).endOf('day').toISOString();
        }

        return params;
      },
      [searchText, multiLevel, dateRange, sortOrder, showDatePicker],
    );

    const handleSearch = useCallback(() => {
      onSearchRef.current(buildParams());
    }, [buildParams]);

    const handleReset = useCallback(() => {
      setSearchText('');
      setMultiLevel(false);
      setDateRange(undefined);
      setSortOrder(sortOptions[0]?.value ?? '');
      onResetRef.current?.();
    }, [sortOptions]);

    useImperativeHandle(ref, () => ({
      search: handleSearch,
      reset: handleReset,
      getValues: () => ({ searchText, multiLevel, dateRange, sortOrder }),
      setValues: (vals) => {
        if (vals.searchText !== undefined) setSearchText(vals.searchText);
        if (vals.multiLevel !== undefined) setMultiLevel(vals.multiLevel);
        if (vals.dateRange !== undefined) setDateRange(vals.dateRange);
        if (vals.sortOrder !== undefined) setSortOrder(vals.sortOrder);
      },
    }));

    const filterControls = (
      <>
        {/* 排序 */}
        {showSortOrder && sortOptions.length > 0 && (
          <div className="shrink-0">
            <SimpleSelect
              value={sortOrder}
              onChange={setSortOrder}
              options={sortOptions}
              triggerSize="sm"
              className="w-auto! min-w-24 bg-input-bg"
            />
          </div>
        )}

        {/* 层级 */}
        {showMultiLevel && (
          <div className="shrink-0">
            <SimpleSelect
              value={String(multiLevel)}
              onChange={(val) => setMultiLevel(val === 'true')}
              options={multiLevelOptions}
              triggerSize="sm"
              className="w-auto! min-w-20 bg-input-bg"
            />
          </div>
        )}

        {/* 日期（client tab） */}
        {showDatePicker && (
          <div className="shrink-0">
            <DatePicker
              mode="range"
              size="sm"
              value={dateRange}
              onChange={setDateRange}
              className="w-auto"
            />
          </div>
        )}

        {/* 搜索框 */}
        <div className="relative shrink-0">
          <Icon
            name="search-line"
            className="pointer-events-none absolute left-3 top-1/2 z-10 -translate-y-1/2 text-text-secondary"
          />
          <Input
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            placeholder={searchPlaceholder ?? t('customers.searchPlaceholder')}
            inputSize="sm"
            className="w-[160px] pl-9!"
          />
        </div>

        {/* 重置 */}
        <Button
          variant="secondary"
          size="sm"
          onClick={handleReset}
          disabled={isLoading}
          className="shrink-0 bg-[#000f32] text-white hover:bg-[#000f32]/90"
        >
          <Icon name="reset-line" />
          {t('action.reset')}
        </Button>

        {/* 搜索 */}
        <Button
          variant="primary"
          size="sm"
          onClick={handleSearch}
          disabled={isLoading}
          className="shrink-0"
        >
          <Icon name="search-line" />
          {t('action.search')}
        </Button>
      </>
    );

    return (
      <>
        {/* Desktop */}
        <div className="hidden md:flex md:flex-wrap md:items-center md:gap-2 lg:gap-3">
          {filterControls}
        </div>

        {/* Mobile */}
        <div className="md:hidden">
          <div className="flex justify-end">
            <button
              type="button"
              onClick={() => setMobileOpen((v) => !v)}
              className="flex items-center gap-1.5 rounded-lg border border-border px-3 py-2 text-sm text-text-secondary transition-colors hover:bg-surface-secondary"
            >
              <Icon name="filter" size={14} />
              {t('action.search')}
            </button>
          </div>

          {mobileOpen && (
            <div className="mt-3 flex flex-wrap items-center gap-3 rounded-lg border border-border bg-surface p-4">
              {filterControls}
            </div>
          )}
        </div>
      </>
    );
  },
);
