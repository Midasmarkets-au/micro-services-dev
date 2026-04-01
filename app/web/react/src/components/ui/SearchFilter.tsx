'use client';

import { useEffect, useMemo, useState } from 'react';
import { SearchableSelect } from './SearchableSelect';

type SearchRecord = Record<string, unknown>;

type SearchOption = {
  value: string;
  label: string;
  raw: SearchRecord;
};

export interface SearchFilterResult {
  ids: number[];
  criteria?: Record<string, unknown>;
  data: SearchRecord[];
}

interface SearchFilterProps {
  className?: string;
  placeholder?: string;
  disabled?: boolean;
  multipleSelection?: boolean;
  searchTrigger?: number | boolean;
  minKeywordLength?: number;
  customSearchHandler: (keyword: string) => Promise<unknown>;
  onResultsChange: (result: SearchFilterResult) => void;
}

function parseNumber(value: unknown): number | null {
  if (typeof value === 'number' && Number.isFinite(value)) return value;
  if (typeof value === 'string') {
    const n = Number(value);
    return Number.isFinite(n) ? n : null;
  }
  return null;
}

function getFirstNumber(record: SearchRecord, keys: string[]): number | null {
  for (const key of keys) {
    const parsed = parseNumber(record[key]);
    if (parsed !== null) return parsed;
  }
  return null;
}

function getFirstText(record: SearchRecord, keys: string[]): string | null {
  for (const key of keys) {
    const value = record[key];
    if (typeof value === 'string' && value.trim()) return value.trim();
  }
  return null;
}

function readUser(record: SearchRecord): SearchRecord | null {
  const user = record.user;
  if (typeof user === 'object' && user !== null) return user as SearchRecord;
  return null;
}

function buildOptionLabel(record: SearchRecord): string {
  const user = readUser(record);
  const name =
    (user && getFirstText(user, ['nativeName', 'displayName', 'name', 'firstName'])) ||
    getFirstText(record, ['nativeName', 'displayName', 'name']) ||
    '-';
  const email =
    (user && getFirstText(user, ['email'])) || getFirstText(record, ['email']) || '';
  const accountNo = getFirstNumber(record, ['accountNumber', 'tradeAccountNumber']);
  const uid = getFirstNumber(record, ['accountUid', 'uid', 'id']);

  const idText = accountNo !== null ? `#${accountNo}` : uid !== null ? `UID:${uid}` : '';
  const tail = [email, idText].filter(Boolean).join(' / ');
  return tail ? `${name} (${tail})` : name;
}

function extractList(payload: unknown): SearchRecord[] {
  if (Array.isArray(payload)) {
    return payload.filter((item) => typeof item === 'object' && item !== null) as SearchRecord[];
  }
  if (typeof payload === 'object' && payload !== null) {
    const root = payload as SearchRecord;
    if (Array.isArray(root.data)) {
      return root.data.filter((item) => typeof item === 'object' && item !== null) as SearchRecord[];
    }
  }
  return [];
}

function extractCriteria(payload: unknown): Record<string, unknown> | undefined {
  if (typeof payload === 'object' && payload !== null) {
    const root = payload as SearchRecord;
    if (typeof root.criteria === 'object' && root.criteria !== null) {
      return root.criteria as Record<string, unknown>;
    }
  }
  return undefined;
}

export function SearchFilter({
  className,
  placeholder,
  disabled = false,
  multipleSelection = false,
  searchTrigger,
  minKeywordLength = 2,
  customSearchHandler,
  onResultsChange,
}: SearchFilterProps) {
  const [keyword, setKeyword] = useState('');
  const [options, setOptions] = useState<SearchOption[]>([]);
  const [selected, setSelected] = useState<SearchOption[] | SearchOption | null>(multipleSelection ? [] : null);
  const [isLoading, setIsLoading] = useState(false);
  const [lastCriteria, setLastCriteria] = useState<Record<string, unknown> | undefined>(undefined);

  useEffect(() => {
    setKeyword('');
    setOptions([]);
    setSelected(multipleSelection ? [] : null);
  }, [searchTrigger, multipleSelection]);

  useEffect(() => {
    const trimmed = keyword.trim();
    if (!trimmed || trimmed.length < minKeywordLength) {
      setOptions([]);
      return;
    }

    let isCancelled = false;
    const timer = setTimeout(async () => {
      setIsLoading(true);
      try {
        const payload = await customSearchHandler(trimmed);
        if (isCancelled) return;

        const list = extractList(payload);
        const criteria = extractCriteria(payload);
        setLastCriteria(criteria);

        const builtOptions: SearchOption[] = list.map((item, idx) => {
          const uid =
            getFirstNumber(item, ['accountUid', 'uid', 'id']) ??
            getFirstNumber(item, ['accountNumber', 'tradeAccountNumber']) ??
            idx;
          return {
            value: String(uid),
            label: buildOptionLabel(item),
            raw: item,
          };
        });
        setOptions(builtOptions);
      } finally {
        if (!isCancelled) setIsLoading(false);
      }
    }, 300);

    return () => {
      isCancelled = true;
      clearTimeout(timer);
    };
  }, [keyword, minKeywordLength, customSearchHandler]);

  const selectValue = useMemo(() => selected, [selected]);

  return (
    <div className={className}>
      <SearchableSelect
        isMulti={multipleSelection}
        isDisabled={disabled}
        isLoading={isLoading}
        options={options}
        value={selectValue}
        placeholder={placeholder}
        noOptionsMessage={() => (keyword.trim().length < minKeywordLength ? '' : 'No options')}
        onInputChange={(value, actionMeta) => {
          if (actionMeta.action === 'input-change') setKeyword(value);
          return value;
        }}
        onChange={(value) => {
          setSelected(value as SearchOption[] | SearchOption | null);
          const items = Array.isArray(value) ? value : value ? [value] : [];
          const ids = items
            .map((item) => getFirstNumber((item as SearchOption).raw, ['accountUid', 'uid', 'id']))
            .filter((id): id is number => id !== null);
          onResultsChange({
            ids,
            criteria: lastCriteria,
            data: items.map((item) => (item as SearchOption).raw),
          });
        }}
      />
    </div>
  );
}
