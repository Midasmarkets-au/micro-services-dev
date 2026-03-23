'use client';

import { useState, useCallback, useMemo, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getAllSymbols } from '@/actions';
import { useUserStore } from '@/stores';
import { ServiceTypes } from '@/types/accounts';
import {
  Button,
  Input,
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
  DatePicker,
  Icon,
  Switch,
} from '@/components/ui';
import type { DateRange } from '@/components/ui';

const SERVICE_OPTIONS = [
  { label: 'MT5', value: String(ServiceTypes.MetaTrader5) },
  { label: 'MT4', value: String(ServiceTypes.MetaTrader4) },
];

// ====================================================================
// 状态映射 — 完全对应 Vue StateInfos.ts 中的 simpleXxxToArray
// ====================================================================

/**
 * 入金: simpleDepositSelections → simpleDepositToArray
 *   DepositCreated(300) → "未完成" → 包含 [300,305,306,310,330,335]
 *   DepositCompleted(350) → "已完成" → 包含 [350,345]
 */
const DEPOSIT_STATE_MAP: Record<number, number[]> = {
  300: [300, 305, 306, 310, 330, 335],
  350: [350, 345],
};

/**
 * 出金: simpleWithdrawalSelections → simpleWithdrawalToArray
 *   WithdrawalCreated(400) → "未完成" → 包含 [400,405,406,420,425]
 *   WithdrawalCompleted(450) → "已完成" → 包含 [450,430]  (Vue 只有 [450])
 */
const WITHDRAWAL_STATE_MAP: Record<number, number[]> = {
  400: [400, 405, 406, 420, 425],
  450: [450, 430],
};

// ====================================================================
// 时间转换 — 对应 Vue helpers.ts 中的 convertTradeTime / handleCriteriaTradeTime
// MT 服务器日内边界根据 DST 偏移：
//   DST(夏令时): 一天 = 前一天 21:00 UTC ~ 当天 20:59:59 UTC
//   非DST(冬令时): 一天 = 前一天 22:00 UTC ~ 当天 21:59:59 UTC
// ====================================================================

function isDateInDST_US(): boolean {
  const now = new Date();
  const jan = new Date(now.getFullYear(), 0, 1);
  const jul = new Date(now.getFullYear(), 6, 1);
  const stdOffset = Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
  return now.getTimezoneOffset() < stdOffset;
}

function pad2(n: number): string {
  return String(n).padStart(2, '0');
}

function formatDateStr(d: Date): string {
  return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
}

/**
 * 对应 Vue convertTradeTime(from, to)
 * 返回 [createdFrom, createdTo]
 */
function convertTradeTime(from: string | null, to: string | null): [string | null, string | null] {
  const isDST = isDateInDST_US();
  const startHour = isDST ? 21 : 22;
  const endHour = isDST ? 20 : 21;

  let createdFrom: string | null = null;
  if (from) {
    const d = new Date(from);
    d.setDate(d.getDate() - 1);
    createdFrom = `${formatDateStr(d)}T${pad2(startHour)}:00:00.000Z`;
  }

  let createdTo: string | null = null;
  if (to) {
    const d = new Date(to);
    createdTo = `${formatDateStr(d)}T${pad2(endHour)}:59:59.000Z`;
  }

  return [createdFrom, createdTo];
}

/**
 * 对应 Vue handleCriteriaTradeTime(periodVal, criteria, false)
 * deposit/withdrawal 使用 defualtTime=false，from 和 to 独立处理
 */
function buildTimeCriteria(
  from: string | null,
  to: string | null,
): { from?: string; to?: string } {
  const result: { from?: string; to?: string } = {};

  if (from) {
    const [createdFrom] = convertTradeTime(from, null);
    if (createdFrom) result.from = createdFrom;
  }

  if (to) {
    const [, createdTo] = convertTradeTime(from, to);
    if (createdTo) result.to = createdTo;
  }

  return result;
}

// ====================================================================
// SumUp 字段清除 — 对应 Vue fetchDataWithClearSumUp
// ====================================================================

const SUM_FIELDS_TO_CLEAR = [
  'total', 'totalVolume', 'totalProfit', 'totalCommission',
  'totalSwap', 'totalAmount',
];

function clearSumUpFields(params: Record<string, unknown>): Record<string, unknown> {
  const cleaned = { ...params };
  SUM_FIELDS_TO_CLEAR.forEach((key) => {
    delete cleaned[key];
  });
  return cleaned;
}

// ====================================================================
// Component Types
// ====================================================================

export type TradeFilterType = 'deposit' | 'withdrawal' | 'trade';

/** 可在 filterOptions 中声明的字段 */
export type FilterField = 'status' | 'isClosed' | 'service' | 'product' | 'account' | 'datePicker' | 'allHistory';

/** 默认显示的筛选字段 */
const DEFAULT_FILTER_OPTIONS: FilterField[] = ['account', 'datePicker', 'allHistory'];

export interface StatusOption {
  /** 状态 ID 值，如 "300"(DepositCreated), "350"(DepositCompleted) */
  value: string;
  label: string;
}

export interface TradeFilterProps {
  /** 筛选类型，决定状态映射和时间转换逻辑 */
  type: TradeFilterType;
  /** i18n namespace，默认 'ib'，sales 页面传 'sales' */
  translationNamespace?: string;
  /** 状态下拉选项（对应 simpleDepositSelections / simpleWithdrawalSelections） */
  statusOptions?: StatusOption[];
  /** 状态字段标签 */
  statusLabel?: string;
  /** 日期选择器默认值，不传则为空 */
  defaultDateRange?: DateRange;
  /**
   * 要显示的筛选字段列表，默认 ['account', 'datePicker', 'allHistory']
   * 可选值: 'status' | 'isClosed' | 'service' | 'product' | 'account' | 'datePicker' | 'allHistory'
   */
  filterOptions?: FilterField[];
  /**
   * 搜索回调 — 参数为完整的查询条件
   * StateIds 已映射为数组，时间已转换为 GMT
   */
  onSearch: (params: Record<string, unknown>) => void;
  /** 重置回调（会同时触发 onSearch） */
  onReset?: () => void;
  /** 全部历史回调 */
  onAllHistory?: () => void;
  /** isClosed 切换回调（切换时自动触发搜索） */
  onIsClosedChange?: (isClosed: boolean) => void;
  /** 是否加载中 */
  isLoading?: boolean;
}

// ====================================================================
// Component
// ====================================================================

export function TradeFilter({
  type,
  translationNamespace = 'ib',
  statusOptions,
  statusLabel,
  defaultDateRange,
  filterOptions = DEFAULT_FILTER_OPTIONS,
  onSearch,
  onReset,
  onAllHistory,
  onIsClosedChange,
  isLoading = false,
}: TradeFilterProps) {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const t = useTranslations(translationNamespace as any);
  const siteConfig = useUserStore((s) => s.siteConfig);

  const visibleFields = useMemo(() => new Set(filterOptions), [filterOptions]);

  const serviceOptions = useMemo(() => {
    if (!visibleFields.has('service')) return SERVICE_OPTIONS;
    const available = siteConfig?.tradingPlatformAvailable;
    if (!available?.length) return SERVICE_OPTIONS;
    return SERVICE_OPTIONS.filter((s) => available.includes(Number(s.value)));
  }, [siteConfig, visibleFields]);

  const defaultStatusValue = useMemo(
    () => statusOptions?.[1]?.value ?? statusOptions?.[0]?.value ?? '',
    [statusOptions],
  );

  const defaultServiceValue = useMemo(
    () => serviceOptions?.[0]?.value ?? '',
    [serviceOptions],
  );

  const [status, setStatus] = useState<string | undefined>(undefined);
  const [isClosed, setIsClosed] = useState(false);
  const [serviceId, setServiceId] = useState<string>(defaultServiceValue);
  const [symbol, setSymbol] = useState('');
  const [accountNumber, setAccountNumber] = useState('');
  const [dateRange, setDateRange] = useState<DateRange | undefined>(defaultDateRange);

  const { execute } = useServerAction({ showErrorToast: false });
  const [symbolCodes, setSymbolCodes] = useState<string[]>([]);
  const [showSymbolDropdown, setShowSymbolDropdown] = useState(false);
  const symbolDropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!visibleFields.has('product')) return;
    let cancelled = false;
    (async () => {
      const result = await execute(getAllSymbols);
      if (!cancelled && result.success && Array.isArray(result.data)) {
        setSymbolCodes(
          result.data
            .map((s) => s.code)
            .filter((code) => code && code !== 'UNKNOWN'),
        );
      }
    })();
    return () => { cancelled = true; };
  }, [visibleFields]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (symbolDropdownRef.current && !symbolDropdownRef.current.contains(e.target as Node)) {
        setShowSymbolDropdown(false);
      }
    };
    if (showSymbolDropdown) document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [showSymbolDropdown]);

  const filteredSymbols = useMemo(() => {
    if (!symbol.trim()) return symbolCodes;
    const q = symbol.trim().toLowerCase();
    return symbolCodes.filter((code) => code.toLowerCase().startsWith(q));
  }, [symbol, symbolCodes]);

  const resolvedStatus = useMemo(() => {
    if (!status) return defaultStatusValue;
    if (!statusOptions?.length) return status;
    return statusOptions.some((opt) => opt.value === status)
      ? status
      : defaultStatusValue;
  }, [status, statusOptions, defaultStatusValue]);

  const resolveStateIds = useCallback(
    (statusValue: string): number[] | undefined => {
      if (!statusValue || statusValue === 'all') return undefined;
      const id = Number(statusValue);
      if (type === 'deposit') return DEPOSIT_STATE_MAP[id];
      if (type === 'withdrawal') return WITHDRAWAL_STATE_MAP[id];
      return undefined;
    },
    [type],
  );

  const buildParams = useCallback(
    (overrides?: { fromDate?: string; clearStatus?: boolean; isClosed?: boolean }) => {
      const params: Record<string, unknown> = {};

      if (!overrides?.clearStatus) {
        const stateIds = resolveStateIds(resolvedStatus);
        if (stateIds) params.StateIds = stateIds;
      }

      if (visibleFields.has('isClosed')) {
        params.isClosed = overrides?.isClosed ?? isClosed;
      }

      if (visibleFields.has('service') && serviceId) {
        params.serviceId = Number(serviceId);
      }

      if (symbol.trim()) params.symbol = symbol.trim();
      if (accountNumber.trim()) params.searchText = accountNumber.trim();

      let fromStr: string | null = null;
      let toStr: string | null = null;

      if (overrides?.fromDate) {
        fromStr = overrides.fromDate;
      } else if (dateRange?.from) {
        fromStr = formatDateStr(dateRange.from);
      }

      if (dateRange?.to) {
        toStr = formatDateStr(dateRange.to);
      }

      const timeCriteria = buildTimeCriteria(fromStr, toStr);
      if (timeCriteria.from) params.from = timeCriteria.from;
      if (timeCriteria.to) params.to = timeCriteria.to;

      return clearSumUpFields(params);
    },
    [resolvedStatus, isClosed, serviceId, symbol, accountNumber, dateRange, resolveStateIds, visibleFields],
  );

  // ---- Actions ----

  const handleSearch = () => {
    onSearch(buildParams());
  };

  const handleIsClosedToggle = (newVal: boolean) => {
    setIsClosed(newVal);
    onIsClosedChange?.(newVal);
    onSearch(buildParams({ isClosed: newVal }));
  };

  const handleReset = () => {
    setStatus(undefined);
    setIsClosed(false);
    setServiceId(defaultServiceValue);
    setSymbol('');
    setAccountNumber('');
    setDateRange(undefined);

    const resetParams: Record<string, unknown> = {};
    const defaultStateIds = resolveStateIds(defaultStatusValue);
    if (defaultStateIds) resetParams.StateIds = defaultStateIds;
    if (visibleFields.has('isClosed')) resetParams.isClosed = false;
    if (visibleFields.has('service') && defaultServiceValue) {
      resetParams.serviceId = Number(defaultServiceValue);
    }

    onIsClosedChange?.(false);
    onReset?.();
    onSearch(clearSumUpFields(resetParams));
  };

  const handleAllHistory = () => {
    setIsClosed(true);

    const allHistoryFrom = new Date('2025-10-01T00:00:00');
    setDateRange({ from: allHistoryFrom, to: undefined });

    const params: Record<string, unknown> = {};

    const stateIds = resolveStateIds(resolvedStatus);
    if (stateIds) params.StateIds = stateIds;

    if (visibleFields.has('isClosed')) params.isClosed = true;
    if (visibleFields.has('service') && serviceId) {
      params.serviceId = Number(serviceId);
    }

    const timeCriteria = buildTimeCriteria('2025-10-01', null);
    if (timeCriteria.from) params.from = timeCriteria.from;

    onIsClosedChange?.(true);
    onAllHistory?.();
    onSearch(clearSumUpFields(params));
  };

  const [mobileFilterOpen, setMobileFilterOpen] = useState(false);

  const filterControls = (
    <>
      {visibleFields.has('status') && statusOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {statusLabel || t('fields.status')}
          </span>
          <Select value={resolvedStatus} onValueChange={setStatus}>
            <SelectTrigger triggerSize="sm" className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {statusOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {visibleFields.has('isClosed') && (
        <div className="flex shrink-0 items-center gap-1">
          <Switch checked={isClosed} onChange={handleIsClosedToggle} />
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {isClosed ? t('trade.closedOrder') : t('trade.openOrder')}
          </span>
        </div>
      )}

      {visibleFields.has('service') && serviceOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {t('trade.service')}
          </span>
          <Select value={serviceId} onValueChange={setServiceId}>
            <SelectTrigger triggerSize="sm" className="w-[80px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {serviceOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {visibleFields.has('product') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {t('trade.product')}
          </span>
          <div className="relative" ref={symbolDropdownRef}>
            <Input
              value={symbol}
              onChange={(e) => {
                setSymbol(e.target.value);
                setShowSymbolDropdown(true);
              }}
              onFocus={() => setShowSymbolDropdown(true)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  setShowSymbolDropdown(false);
                  handleSearch();
                }
              }}
              inputSize="sm"
              className="w-[160px]"
            />
            {showSymbolDropdown && filteredSymbols.length > 0 && (
              <div className="absolute left-0 top-full z-50 mt-1 max-h-48 w-[200px] overflow-y-auto rounded border border-border bg-surface shadow-lg">
                {filteredSymbols.map((code) => (
                  <button
                    key={code}
                    type="button"
                    onClick={() => {
                      setSymbol(code);
                      setShowSymbolDropdown(false);
                    }}
                    className="w-full cursor-pointer px-3 py-1.5 text-left text-sm text-text-primary hover:bg-surface-secondary"
                  >
                    {code}
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      {visibleFields.has('account') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {t('trade.account')}
          </span>
          <Input
            value={accountNumber}
            onChange={(e) => setAccountNumber(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            inputSize="sm"
            className="w-[160px]"
          />
        </div>
      )}

      {visibleFields.has('datePicker') && (
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

      {visibleFields.has('allHistory') && (
        <Button
          variant="secondary"
          size="sm"
          onClick={handleAllHistory}
          disabled={isLoading}
          className="shrink-0 bg-[#000f32] text-white hover:bg-[#000f32]/90"
        >
          {t('trade.allHistory')}
        </Button>
      )}
    </>
  );

  return (
    <div>
      {/* Desktop */}
      <div className="hidden md:flex md:items-center">
        <div className="flex w-full flex-wrap items-center gap-2 lg:gap-3">
          {filterControls}
        </div>
      </div>

      {/* Mobile */}
      <div className="md:hidden">
        <div className="flex justify-end">
          <button
            type="button"
            onClick={() => setMobileFilterOpen((v) => !v)}
            className="flex items-center gap-1.5 rounded-lg border border-border px-3 py-2 text-sm text-text-secondary transition-colors hover:bg-(--color-surface-secondary)"
          >
            <Icon name="filter" size={14} />
            {t('action.search')}
          </button>
        </div>

        {mobileFilterOpen && (
          <div className="mt-3 flex flex-wrap items-center gap-3 rounded-lg border border-border bg-surface p-4">
            {filterControls}
          </div>
        )}
      </div>
    </div>
  );
}
