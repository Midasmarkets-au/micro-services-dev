import { type ClassValue, clsx } from 'clsx';
import moment from 'moment';
import { twMerge } from 'tailwind-merge';
import { useUserStore } from '@/stores';
/**
 * 合并 Tailwind CSS 类名
 * 使用 clsx 处理条件类名，twMerge 合并冲突的 Tailwind 类
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * 金额字段归一化（服务端返回值 / divisor）
 * 支持单个数字、单个对象、对象数组
 */
export function normalizeAmountList<T extends Record<string, any>>(
  data: T | T[] | number,
  fields: keyof T | (keyof T)[] = 'amount' as keyof T,
  divisor = 10000
): T | T[] | number {
  if (data == null) return data;

  if (typeof data === 'number') {
    return data / divisor;
  }

  const fieldList = Array.isArray(fields) ? fields : [fields];

  const normalizeObject = (item: T): T => {
    const newItem = { ...item };
    for (const field of fieldList) {
      if (typeof newItem[field] === 'number') {
        newItem[field] = ((newItem[field] as number) / divisor) as any;
      }
    }
    return newItem;
  };

  if (Array.isArray(data)) {
    return data.map((item) => normalizeObject(item));
  }

  return normalizeObject(data);
}

export function buildQuery<T extends object>(params?: T): string {
  if (!params) return '';
  const qs = new URLSearchParams();
  Object.entries(params as Record<string, unknown>).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    if (Array.isArray(value)) {
      value.forEach((v) => qs.append(key, String(v)));
    } else {
      qs.append(key, String(value));
    }
  });
  const str = qs.toString();
  return str ? `?${str}` : '';
}

export function convertTradeTime(from: string | null, to: string | null): [string, string] {
  const isDST = isDateInDST_US();
  const startHour = isDST ? 21 : 22;
  const endHour = isDST ? 20 : 21;

  const timeFormat = `YYYY-MM-DD[T]`;

  const createdFrom = (from ? moment(from) : moment.utc())
    .subtract(1, 'day')
    .format(`${timeFormat}${startHour}:00:00.000[Z]`);

  const createdTo = (to ? moment(to) : moment.utc())
    .format(`${timeFormat}${endHour}:59:59.000[Z]`);

  return [createdFrom, createdTo];
}

function isDateInDST_US(): boolean {
  const siteConfig = useUserStore.getState().siteConfig;
  return siteConfig?.HoursGapForMT5 === 3;
}