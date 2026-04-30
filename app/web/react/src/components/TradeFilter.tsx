'use client';

import {
  useState,
  useCallback,
  useMemo,
  useEffect,
  useRef,
  forwardRef,
  useImperativeHandle,
} from 'react';
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
import { convertTradeTime } from '@/lib/utils';

const SERVICE_OPTIONS = [
  { label: 'MT5', value: String(ServiceTypes.MetaTrader5) },
  { label: 'MT4', value: String(ServiceTypes.MetaTrader4) },
];

let symbolCodesCache: string[] | null = null;
let symbolCodesRequest: Promise<string[]> | null = null;

// ====================================================================
// 状态映射 — 对应 Vue StateInfos.ts 中的 simpleXxxToArray
// ====================================================================

const DEPOSIT_STATE_MAP: Record<number, number[]> = {
  300: [300, 305, 306, 310, 330, 335],
  350: [350, 345],
};

const WITHDRAWAL_STATE_MAP: Record<number, number[]> = {
  400: [400, 405, 406, 420, 425],
  450: [450],
};

const TRANSACTION_STATE_MAP: Record<number, number[]> = {
  200: [200, 205, 206, 210, 215, 220],
  250: [250],
};

// ====================================================================
// 时间转换
// ====================================================================

function pad2(n: number): string {
  return String(n).padStart(2, '0');
}

function formatDateStr(d: Date): string {
  return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
}

function getTodayDateRange(): DateRange {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return { from: d, to: d };
}


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
// SumUp 字段清除
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
// 类型定义
// ====================================================================

export type TradeFilterType = 'deposit' | 'withdrawal' | 'trade' | 'transaction';

/** 可在 filterOptions 中声明的字段 */
export type FilterField =
  | 'stateIds'
  | 'isClosed'
  | 'service'
  | 'product'
  | 'account'
  | 'datePicker'
  | 'allHistory'
  | 'pageSize'
  | 'transactionType'
  | 'transactionStatus'
  | 'walletTransactionType'
  | 'walletTransactionStatus'
  | 'order'
  | 'accountUid'
  | 'target';

/** 默认显示的筛选字段 */
const DEFAULT_FILTER_OPTIONS: FilterField[] = ['account', 'datePicker', 'allHistory'];

export interface SelectOption {
  value: string;
  label: string;
}

/** 状态选项 + 映射关系 */
export interface StateConfig {
  options: SelectOption[];
  defaultIndex?: number;
  stateMap?: Record<number, number[]>;
}

/** 筛选字段的值，onChange 对外暴露 */
export interface TradeFilterValues {
  stateIds?: string;
  isClosed?: boolean;
  serviceId?: string;
  symbol?: string;
  account?: string;
  dateRange?: DateRange;
  pageSize?: number;
  transactionType?: string;
  transactionStatus?: string;
  walletTransactionType?: string;
  walletTransactionStatus?: string;
  order?: string;
  accountUid?: string;
  target?: string;
}

/** 触发模式：button=点击搜索按钮触发, change=字段变化即时触发 */
export type TriggerMode = 'button' | 'change';

/** 通过 ref 暴露给外部的方法 */
export interface TradeFilterRef {
  search: () => void;
  reset: () => void;
  getValues: () => TradeFilterValues;
  setValues: (values: Partial<TradeFilterValues>) => void;
}

export interface TradeFilterProps {
  /** 筛选类型，仅在未提供 stateConfig 时用于自动推导状态选项 */
  type: TradeFilterType;
  /** 要显示的筛选字段列表 */
  filterOptions?: FilterField[];
  /** 各字段的默认值 */
  defaultParam?: TradeFilterValues;
  /** 固定透传参数 */
  fixedParams?: Record<string, unknown>;
  /** 页面大小可选项 */
  pageSizeOptions?: number[];
  /** 搜索回调 */
  onSearch: (params: Record<string, unknown>) => void;
  /** 重置回调 */
  onReset?: () => void;
  /** 全部历史回调 */
  onAllHistory?: () => void;
  /** 字段值变化回调 */
  onChange?: (values: TradeFilterValues) => void;
  /** 是否加载中 */
  isLoading?: boolean;

  // ---- 以下为新增外部化配置 ----

  /** 外部自定义状态选项 & 映射，优先级高于 type 推导 */
  stateConfig?: StateConfig;
  /** 触发模式，默认 'button' */
  trigger?: TriggerMode;
  /** 交易类型选项（transactionType 字段用） */
  transactionTypeOptions?: SelectOption[];
  /** 交易状态选项（transactionStatus 字段用） */
  transactionStatusOptions?: SelectOption[];
  /** 钱包交易类型选项 */
  walletTransactionTypeOptions?: SelectOption[];
  /** 钱包交易状态选项 */
  walletTransactionStatusOptions?: SelectOption[];
  /** 订单类型选项 */
  orderOptions?: SelectOption[];
}

// ====================================================================
// 内置默认选项（仅在外部未提供时用作 fallback）
// ====================================================================

function getBuiltinStateConfig(type: TradeFilterType, t: (key: string) => string): StateConfig {
  if (type === 'deposit') {
    return {
      options: [
        { value: '300', label: t('deposit.incompleteDeposit') },
        { value: '350', label: t('deposit.completedDeposit') },
      ],
      defaultIndex: 1,
      stateMap: DEPOSIT_STATE_MAP,
    };
  }
  if (type === 'withdrawal') {
    return {
      options: [
        { value: '400', label: t('withdrawal.incompleteWithdrawal') },
        { value: '450', label: t('withdrawal.completedWithdrawal') },
      ],
      defaultIndex: 1,
      stateMap: WITHDRAWAL_STATE_MAP,
    };
  }
  if (type === 'transaction') {
    return {
      options: [
        { value: '200', label: t('transaction.incompleteTransaction') },
        { value: '250', label: t('transaction.completedTransaction') },
      ],
      defaultIndex: 1,
      stateMap: TRANSACTION_STATE_MAP,
    };
  }
  return { options: [], stateMap: {} };
}

// ====================================================================
// 组件
// ====================================================================

export const TradeFilter = forwardRef<TradeFilterRef, TradeFilterProps>(function TradeFilter(
  {
    type,
    filterOptions = DEFAULT_FILTER_OPTIONS,
    defaultParam,
    fixedParams,
    pageSizeOptions = [10, 15, 20, 25, 30, 50, 100],
    onSearch,
    onReset,
    onAllHistory,
    onChange,
    isLoading = false,
    stateConfig: externalStateConfig,
    trigger = 'button',
    transactionTypeOptions,
    transactionStatusOptions,
    walletTransactionTypeOptions,
    walletTransactionStatusOptions,
    orderOptions,
  },
  ref,
) {
  const t = useTranslations('ib');
  const siteConfig = useUserStore((s) => s.siteConfig);

  // ---- 状态选项 & 映射（外部优先，否则按 type 自动推导） ----

  const resolvedStateConfig = useMemo<StateConfig>(() => {
    if (externalStateConfig) return externalStateConfig;
    return getBuiltinStateConfig(type, t);
  }, [externalStateConfig, type, t]);

  const { options: statusOptions, stateMap: statusStateMap } = resolvedStateConfig;

  const visibleFields = useMemo(() => new Set(filterOptions), [filterOptions]);

  const serviceOptions = useMemo(() => {
    if (!visibleFields.has('service')) return SERVICE_OPTIONS;
    const available = siteConfig?.tradingPlatformAvailable;
    if (!available?.length) return SERVICE_OPTIONS;
    return SERVICE_OPTIONS.filter((s) => available.includes(Number(s.value)));
  }, [siteConfig, visibleFields]);

  const defaultStatusValue = useMemo(
    () => {
      const idx = resolvedStateConfig.defaultIndex ?? 1;
      return statusOptions?.[idx]?.value ?? statusOptions?.[0]?.value ?? '';
    },
    [statusOptions, resolvedStateConfig.defaultIndex],
  );

  const defaultServiceValue = useMemo(
    () => serviceOptions?.[0]?.value ?? '',
    [serviceOptions],
  );

  // ---- 内部状态 ----

  const [status, setStatus] = useState<string | undefined>(defaultParam?.stateIds);
  const [isClosed, setIsClosed] = useState(defaultParam?.isClosed ?? false);
  const [serviceId, setServiceId] = useState<string>(defaultParam?.serviceId ?? defaultServiceValue);
  const [symbol, setSymbol] = useState(defaultParam?.symbol ?? '');
  const [accountNumber, setAccountNumber] = useState(defaultParam?.account ?? '');
  const [dateRange, setDateRange] = useState<DateRange | undefined>(defaultParam?.dateRange);
  const [pageSize, setPageSize] = useState<number>(defaultParam?.pageSize ?? 25);
  const [transactionType, setTransactionType] = useState(defaultParam?.transactionType ?? '');
  const [transactionStatus, setTransactionStatus] = useState(defaultParam?.transactionStatus ?? '');
  const [walletTxType, setWalletTxType] = useState(defaultParam?.walletTransactionType ?? '');
  const [walletTxStatus, setWalletTxStatus] = useState(defaultParam?.walletTransactionStatus ?? '');
  const [order, setOrder] = useState(defaultParam?.order ?? '');
  const [accountUid, setAccountUid] = useState(defaultParam?.accountUid ?? '');
  const [target, setTarget] = useState(defaultParam?.target ?? '');

  // defaultParam 外部变更时同步内部状态
  const [prevDefaultParam, setPrevDefaultParam] = useState(defaultParam);
  const [defaultParamSyncVersion, setDefaultParamSyncVersion] = useState(0);
  if (defaultParam !== prevDefaultParam) {
    const dp = defaultParam || {};
    const prev = prevDefaultParam || {};
    setPrevDefaultParam(defaultParam);
    let synced = false;
    if (dp.dateRange !== prev.dateRange) { setDateRange(dp.dateRange); synced = true; }
    if (dp.isClosed !== prev.isClosed) { setIsClosed(dp.isClosed ?? false); synced = true; }
    if (dp.pageSize !== prev.pageSize) { setPageSize(dp.pageSize ?? 25); synced = true; }
    if (dp.serviceId !== prev.serviceId) { setServiceId(dp.serviceId ?? defaultServiceValue); synced = true; }
    if (dp.symbol !== prev.symbol) { setSymbol(dp.symbol ?? ''); synced = true; }
    if (dp.account !== prev.account) { setAccountNumber(dp.account ?? ''); synced = true; }
    if (dp.stateIds !== prev.stateIds) { setStatus(dp.stateIds); synced = true; }
    if (dp.transactionType !== prev.transactionType) { setTransactionType(dp.transactionType ?? ''); synced = true; }
    if (dp.transactionStatus !== prev.transactionStatus) { setTransactionStatus(dp.transactionStatus ?? ''); synced = true; }
    if (dp.walletTransactionType !== prev.walletTransactionType) { setWalletTxType(dp.walletTransactionType ?? ''); synced = true; }
    if (dp.walletTransactionStatus !== prev.walletTransactionStatus) { setWalletTxStatus(dp.walletTransactionStatus ?? ''); synced = true; }
    if (dp.order !== prev.order) { setOrder(dp.order ?? ''); synced = true; }
    if (dp.accountUid !== prev.accountUid) { setAccountUid(dp.accountUid ?? ''); synced = true; }
    if (dp.target !== prev.target) { setTarget(dp.target ?? ''); synced = true; }
    if (synced) setDefaultParamSyncVersion((v) => v + 1);
  }

  const { execute } = useServerAction({ showErrorToast: false });
  const executeRef = useRef(execute);
  useEffect(() => { executeRef.current = execute; }, [execute]);

  const onSearchRef = useRef(onSearch);
  useEffect(() => { onSearchRef.current = onSearch; }, [onSearch]);

  const onChangeRef = useRef(onChange);
  useEffect(() => { onChangeRef.current = onChange; }, [onChange]);

  // ---- 品种自动补全 ----

  const [symbolCodes, setSymbolCodes] = useState<string[]>([]);
  const [showSymbolDropdown, setShowSymbolDropdown] = useState(false);
  const symbolDropdownRef = useRef<HTMLDivElement>(null);
  const hasProductField = filterOptions.includes('product');

  useEffect(() => {
    if (!hasProductField) return;
    let cancelled = false;
    (async () => {
      if (symbolCodesCache) {
        if (!cancelled) setSymbolCodes(symbolCodesCache);
        return;
      }
      if (!symbolCodesRequest) {
        symbolCodesRequest = (async () => {
          const result = await executeRef.current(getAllSymbols);
          if (!result.success || !Array.isArray(result.data)) return [];
          const codes = result.data
            .map((s) => s.code)
            .filter((code) => code && code !== 'UNKNOWN');
          symbolCodesCache = codes;
          return codes;
        })().finally(() => { symbolCodesRequest = null; });
      }
      const codes = await symbolCodesRequest;
      if (!cancelled) setSymbolCodes(codes);
    })();
    return () => { cancelled = true; };
  }, [hasProductField]);

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

  // ---- 状态值推导 ----

  const resolvedStatus = useMemo(() => {
    if (!status) return defaultStatusValue;
    if (!statusOptions?.length) return status;
    return statusOptions.some((opt) => opt.value === status)
      ? status
      : defaultStatusValue;
  }, [status, statusOptions, defaultStatusValue]);

  // ---- 收集当前所有值 ----

  const collectValues = useCallback(
    (overrides?: Partial<TradeFilterValues>): TradeFilterValues => {
      const o = overrides || {};
      const values: TradeFilterValues = {};
      if (visibleFields.has('stateIds')) values.stateIds = 'stateIds' in o ? o.stateIds : (status ?? resolvedStatus);
      if (visibleFields.has('isClosed')) values.isClosed = 'isClosed' in o ? o.isClosed : isClosed;
      if (visibleFields.has('service')) values.serviceId = 'serviceId' in o ? o.serviceId : serviceId;
      if (visibleFields.has('product')) values.symbol = 'symbol' in o ? o.symbol : symbol;
      if (visibleFields.has('account')) values.account = 'account' in o ? o.account : accountNumber;
      if (visibleFields.has('datePicker')) values.dateRange = 'dateRange' in o ? o.dateRange : dateRange;
      if (visibleFields.has('pageSize')) values.pageSize = 'pageSize' in o ? o.pageSize : pageSize;
      if (visibleFields.has('transactionType')) values.transactionType = 'transactionType' in o ? o.transactionType : transactionType;
      if (visibleFields.has('transactionStatus')) values.transactionStatus = 'transactionStatus' in o ? o.transactionStatus : transactionStatus;
      if (visibleFields.has('walletTransactionType')) values.walletTransactionType = 'walletTransactionType' in o ? o.walletTransactionType : walletTxType;
      if (visibleFields.has('walletTransactionStatus')) values.walletTransactionStatus = 'walletTransactionStatus' in o ? o.walletTransactionStatus : walletTxStatus;
      if (visibleFields.has('order')) values.order = 'order' in o ? o.order : order;
      if (visibleFields.has('accountUid')) values.accountUid = 'accountUid' in o ? o.accountUid : accountUid;
      if (visibleFields.has('target')) values.target = 'target' in o ? o.target : target;
      return values;
    },
    [visibleFields, status, resolvedStatus, isClosed, serviceId, symbol, accountNumber, dateRange, pageSize, transactionType, transactionStatus, walletTxType, walletTxStatus, order, accountUid, target],
  );

  const fireChange = useCallback(
    (overrides: Partial<TradeFilterValues>) => {
      onChangeRef.current?.(collectValues(overrides));
    },
    [collectValues],
  );

  // ---- 状态 ID 映射 ----

  const resolveStateIds = useCallback(
    (statusValue: string): number[] | undefined => {
      if (!statusValue || statusValue === 'all') return undefined;
      const id = Number(statusValue);
      const map = statusStateMap;
      return map?.[id];
    },
    [statusStateMap],
  );

  // ---- 构建查询参数 ----

  const buildParams = useCallback(
    (overrides?: { fromDate?: string; clearStatus?: boolean; isClosed?: boolean; dateRange?: DateRange }) => {
      const params: Record<string, unknown> = {};

      if (visibleFields.has('stateIds') && !overrides?.clearStatus) {
        const stateIds = resolveStateIds(resolvedStatus);
        if (stateIds) params.stateIds = stateIds;
      }

      if (visibleFields.has('isClosed')) {
        params.isClosed = overrides?.isClosed ?? isClosed;
      }
      if (visibleFields.has('service') && serviceId) {
        params.serviceId = Number(serviceId);
      }
      if (visibleFields.has('product') && symbol.trim()) params.symbol = symbol.trim();
      if (visibleFields.has('account') && accountNumber.trim()) params.accountNumber = accountNumber.trim();
      if (visibleFields.has('pageSize')) params.size = pageSize;
      if (visibleFields.has('transactionType') && transactionType) params.ledgerSide = Number(transactionType);
      if (visibleFields.has('transactionStatus') && transactionStatus) params.stateId = Number(transactionStatus);
      if (visibleFields.has('walletTransactionType') && walletTxType) params.matterType = Number(walletTxType);
      if (visibleFields.has('walletTransactionStatus') && walletTxStatus) params.walletStatus = Number(walletTxStatus);
      if (visibleFields.has('order') && order) params.cmds = order;
      if (visibleFields.has('accountUid') && accountUid.trim()) params.uid = accountUid.trim();
      if (visibleFields.has('target') && target.trim()) params.target = target.trim();

      if (visibleFields.has('datePicker')) {
        let fromStr: string | null = null;
        let toStr: string | null = null;
        const effectiveDateRange = (overrides && 'dateRange' in overrides)
          ? overrides.dateRange
          : dateRange;

        if (overrides?.fromDate) {
          fromStr = overrides.fromDate;
        } else if (effectiveDateRange?.from) {
          fromStr = formatDateStr(effectiveDateRange.from);
        }
        if (effectiveDateRange?.to) {
          toStr = formatDateStr(effectiveDateRange.to);
        }

        const timeCriteria = buildTimeCriteria(fromStr, toStr);
        if (timeCriteria.from) params.from = timeCriteria.from;
        if (timeCriteria.to) params.to = timeCriteria.to;
        if (fromStr && toStr) params.period = `${fromStr},${toStr}`;
      }

      return clearSumUpFields({ ...params, ...(fixedParams || {}) });
    },
    [resolvedStatus, isClosed, serviceId, symbol, accountNumber, pageSize, dateRange, resolveStateIds, visibleFields, fixedParams, transactionType, transactionStatus, walletTxType, walletTxStatus, order, accountUid, target],
  );

  // ---- 触发搜索的统一入口 ----

  const doSearch = useCallback(
    (overrides?: Parameters<typeof buildParams>[0]) => {
      onSearchRef.current(buildParams(overrides));
    },
    [buildParams],
  );

  // 挂载时自动触发初始搜索
  const isFirstRender = useRef(true);
  useEffect(() => {
    if (!isFirstRender.current) return;
    isFirstRender.current = false;
    onSearchRef.current(buildParams());
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // defaultParam 外部变更同步后自动搜索
  useEffect(() => {
    if (defaultParamSyncVersion === 0) return;
    onSearchRef.current(buildParams());
  }, [defaultParamSyncVersion]); // eslint-disable-line react-hooks/exhaustive-deps

  // ---- trigger='change' 时的即时搜索 ----

  const searchOnChange = trigger === 'change';

  // ---- 字段变化处理器 ----

  const handleStatusChange = (val: string) => {
    setStatus(val);
    fireChange({ stateIds: val });
    if (searchOnChange) doSearch();
  };

  const handleIsClosedToggle = (newVal: boolean) => {
    setIsClosed(newVal);
    if (newVal && visibleFields.has('datePicker')) {
      const todayRange = getTodayDateRange();
      setDateRange(todayRange);
      fireChange({ isClosed: newVal, dateRange: todayRange });
      doSearch({ isClosed: newVal, dateRange: todayRange });
      return;
    }
    if (!newVal && visibleFields.has('datePicker')) {
      setDateRange(undefined);
      fireChange({ isClosed: newVal, dateRange: undefined });
      doSearch({ isClosed: newVal, dateRange: undefined });
      return;
    }
    fireChange({ isClosed: newVal });
    doSearch({ isClosed: newVal });
  };

  const handleServiceChange = (val: string) => {
    setServiceId(val);
    fireChange({ serviceId: val });
    if (searchOnChange) doSearch();
  };

  const handleSymbolChange = (val: string) => {
    setSymbol(val);
    fireChange({ symbol: val });
  };

  const handleAccountChange = (val: string) => {
    setAccountNumber(val);
    fireChange({ account: val });
  };

  const handleDateRangeChange = (val: DateRange | undefined) => {
    setDateRange(val);
    fireChange({ dateRange: val });
  };

  const handlePageSizeChange = (val: number) => {
    setPageSize(val);
    fireChange({ pageSize: val });
    if (searchOnChange) doSearch();
  };

  const handleTransactionTypeChange = (val: string) => {
    setTransactionType(val);
    fireChange({ transactionType: val });
    if (searchOnChange) doSearch();
  };

  const handleTransactionStatusChange = (val: string) => {
    setTransactionStatus(val);
    fireChange({ transactionStatus: val });
    if (searchOnChange) doSearch();
  };

  const handleWalletTxTypeChange = (val: string) => {
    setWalletTxType(val);
    fireChange({ walletTransactionType: val });
    if (searchOnChange) doSearch();
  };

  const handleWalletTxStatusChange = (val: string) => {
    setWalletTxStatus(val);
    fireChange({ walletTransactionStatus: val });
    if (searchOnChange) doSearch();
  };

  const handleOrderChange = (val: string) => {
    setOrder(val);
    fireChange({ order: val });
    if (searchOnChange) doSearch();
  };

  const handleAccountUidChange = (val: string) => {
    setAccountUid(val);
    fireChange({ accountUid: val });
  };

  const handleTargetChange = (val: string) => {
    setTarget(val);
    fireChange({ target: val });
  };

  // ---- 按钮动作 ----

  const handleSearch = () => {
    doSearch();
  };

  const defaultIsClosed = defaultParam?.isClosed ?? false;
  const defaultPageSize = defaultParam?.pageSize ?? 25;

  const handleReset = () => {
    setStatus(defaultParam?.stateIds);
    setIsClosed(defaultIsClosed);
    setServiceId(defaultParam?.serviceId ?? defaultServiceValue);
    setSymbol(defaultParam?.symbol ?? '');
    setAccountNumber(defaultParam?.account ?? '');
    setDateRange(defaultParam?.dateRange);
    setPageSize(defaultPageSize);
    setTransactionType(defaultParam?.transactionType ?? '');
    setTransactionStatus(defaultParam?.transactionStatus ?? '');
    setWalletTxType(defaultParam?.walletTransactionType ?? '');
    setWalletTxStatus(defaultParam?.walletTransactionStatus ?? '');
    setOrder(defaultParam?.order ?? '');
    setAccountUid(defaultParam?.accountUid ?? '');
    setTarget(defaultParam?.target ?? '');

    const resetParams: Record<string, unknown> = {};
    if (visibleFields.has('stateIds')) {
      const defaultStateIds = resolveStateIds(defaultParam?.stateIds ?? defaultStatusValue);
      if (defaultStateIds) resetParams.stateIds = defaultStateIds;
    }
    if (visibleFields.has('isClosed')) resetParams.isClosed = defaultIsClosed;
    if (visibleFields.has('service')) {
      const svcId = defaultParam?.serviceId ?? defaultServiceValue;
      if (svcId) resetParams.serviceId = Number(svcId);
    }
    if (visibleFields.has('pageSize')) resetParams.size = defaultPageSize;

    const resetValues = collectValues({
      stateIds: defaultParam?.stateIds,
      isClosed: defaultIsClosed,
      serviceId: defaultParam?.serviceId ?? defaultServiceValue,
      symbol: defaultParam?.symbol ?? '',
      account: defaultParam?.account ?? '',
      dateRange: defaultParam?.dateRange,
      pageSize: defaultPageSize,
      transactionType: defaultParam?.transactionType ?? '',
      transactionStatus: defaultParam?.transactionStatus ?? '',
      walletTransactionType: defaultParam?.walletTransactionType ?? '',
      walletTransactionStatus: defaultParam?.walletTransactionStatus ?? '',
      order: defaultParam?.order ?? '',
      accountUid: defaultParam?.accountUid ?? '',
      target: defaultParam?.target ?? '',
    });
    onChangeRef.current?.(resetValues);
    onReset?.();
    onSearch(clearSumUpFields({ ...resetParams, ...(fixedParams || {}) }));
  };

  const handleAllHistory = () => {
    setIsClosed(true);

    const allHistoryFrom = new Date('2025-10-01T00:00:00');
    setDateRange({ from: allHistoryFrom, to: undefined });

    const params: Record<string, unknown> = {};
    if (visibleFields.has('stateIds')) {
      const stateIds = resolveStateIds(resolvedStatus);
      if (stateIds) params.stateIds = stateIds;
    }
    if (visibleFields.has('isClosed')) params.isClosed = true;
    if (visibleFields.has('service') && serviceId) params.serviceId = Number(serviceId);

    const timeCriteria = buildTimeCriteria('2025-10-01', null);
    if (timeCriteria.from) params.from = timeCriteria.from;
    if (visibleFields.has('pageSize')) params.size = pageSize;

    fireChange({ isClosed: true, dateRange: { from: allHistoryFrom, to: undefined } });
    onAllHistory?.();
    onSearch(clearSumUpFields({ ...params, ...(fixedParams || {}) }));
  };

  // ---- forwardRef / useImperativeHandle ----

  useImperativeHandle(ref, () => ({
    search: handleSearch,
    reset: handleReset,
    getValues: () => collectValues(),
    setValues: (vals: Partial<TradeFilterValues>) => {
      if (vals.stateIds !== undefined) setStatus(vals.stateIds);
      if (vals.isClosed !== undefined) setIsClosed(vals.isClosed);
      if (vals.serviceId !== undefined) setServiceId(vals.serviceId);
      if (vals.symbol !== undefined) setSymbol(vals.symbol);
      if (vals.account !== undefined) setAccountNumber(vals.account);
      if (vals.dateRange !== undefined) setDateRange(vals.dateRange);
      if (vals.pageSize !== undefined) setPageSize(vals.pageSize);
      if (vals.transactionType !== undefined) setTransactionType(vals.transactionType);
      if (vals.transactionStatus !== undefined) setTransactionStatus(vals.transactionStatus);
      if (vals.walletTransactionType !== undefined) setWalletTxType(vals.walletTransactionType);
      if (vals.walletTransactionStatus !== undefined) setWalletTxStatus(vals.walletTransactionStatus);
      if (vals.order !== undefined) setOrder(vals.order);
      if (vals.accountUid !== undefined) setAccountUid(vals.accountUid);
      if (vals.target !== undefined) setTarget(vals.target);
      fireChange(vals);
    },
  }));

  // ---- 渲染 ----

  const [mobileFilterOpen, setMobileFilterOpen] = useState(false);

  const filterControls = (
    <>
      {/* 状态 */}
      {visibleFields.has('stateIds') && statusOptions.length > 0 && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.status')}</span>
          <Select value={resolvedStatus} onValueChange={handleStatusChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {statusOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 开/平仓 */}
      {visibleFields.has('isClosed') && (
        <div className="flex shrink-0 items-center gap-1">
          <Switch checked={isClosed} onChange={handleIsClosedToggle} />
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {isClosed ? t('trade.closedOrder') : t('trade.openOrder')}
          </span>
        </div>
      )}

      {/* 服务（MT4/MT5） */}
      {visibleFields.has('service') && serviceOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('trade.service')}</span>
          <Select value={serviceId} onValueChange={handleServiceChange}>
            <SelectTrigger triggerSize="sm" className="w-[80px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {serviceOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 交易类型 */}
      {visibleFields.has('transactionType') && transactionTypeOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.transactionType')}</span>
          <Select value={transactionType} onValueChange={handleTransactionTypeChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">{t('fields.all')}</SelectItem>
              {transactionTypeOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 交易状态 */}
      {visibleFields.has('transactionStatus') && transactionStatusOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.transactionStatus')}</span>
          <Select value={transactionStatus} onValueChange={handleTransactionStatusChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">{t('fields.all')}</SelectItem>
              {transactionStatusOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 钱包交易类型 */}
      {visibleFields.has('walletTransactionType') && walletTransactionTypeOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.transactionType')}</span>
          <Select value={walletTxType} onValueChange={handleWalletTxTypeChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">{t('fields.all')}</SelectItem>
              {walletTransactionTypeOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 钱包交易状态 */}
      {visibleFields.has('walletTransactionStatus') && walletTransactionStatusOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.transactionStatus')}</span>
          <Select value={walletTxStatus} onValueChange={handleWalletTxStatusChange}>
            <SelectTrigger triggerSize="sm" className="w-[140px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">{t('fields.all')}</SelectItem>
              {walletTransactionStatusOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 订单类型 */}
      {visibleFields.has('order') && orderOptions && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.orderType')}</span>
          <Select value={order} onValueChange={handleOrderChange}>
            <SelectTrigger triggerSize="sm" className="w-[150px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">{t('fields.all')}</SelectItem>
              {orderOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 品种 */}
      {visibleFields.has('product') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {t('trade.product')}
          </span>
          <div className="relative" ref={symbolDropdownRef}>
            <Input
              value={symbol}
              onChange={(e) => {
                handleSymbolChange(e.target.value);
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
                      handleSymbolChange(code);
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

      {/* 账号 */}
      {visibleFields.has('account') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('trade.account')}</span>
          <Input
            value={accountNumber}
            onChange={(e) => handleAccountChange(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            inputSize="sm"
            className="w-[160px]"
          />
        </div>
      )}

      {/* 账户UID */}
      {visibleFields.has('accountUid') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.accountUid')}</span>
          <Input
            value={accountUid}
            onChange={(e) => handleAccountUidChange(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            inputSize="sm"
            className="w-[130px]"
          />
        </div>
      )}

      {/* 目标账户 */}
      {visibleFields.has('target') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">{t('fields.accountNo')}</span>
          <Input
            value={target}
            onChange={(e) => handleTargetChange(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            inputSize="sm"
            className="w-[130px]"
          />
        </div>
      )}

      {/* 日期选择器 */}
      {visibleFields.has('datePicker') && (
        <div className="shrink-0">
          <DatePicker
            mode="range"
            size="sm"
            value={dateRange}
            onChange={handleDateRangeChange}
            className="w-auto"
          />
        </div>
      )}

      {/* 页面大小 */}
      {visibleFields.has('pageSize') && (
        <div className="flex shrink-0 items-center gap-2">
          <span className="whitespace-nowrap text-sm text-text-secondary">
            {t('fields.pageSize')}
          </span>
          <Select value={String(pageSize)} onValueChange={(v) => handlePageSizeChange(Number(v))}>
            <SelectTrigger triggerSize="sm" className="w-[90px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {pageSizeOptions.map((size) => (
                <SelectItem key={size} value={String(size)}>
                  {size}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* 重置按钮 */}
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

      {/* 搜索按钮 */}
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

      {/* 全部历史 */}
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
});
