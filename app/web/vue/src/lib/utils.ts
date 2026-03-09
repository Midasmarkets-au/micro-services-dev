import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function normalizeAmountList<T extends Record<string, any>>(
  data: T | T[] | number,
  fields: keyof T | (keyof T)[] = "amount", // 可以是单个字段，也可以是多个字段
  divisor = 10000
): T | T[] | number {
  if (data == null) return data;

  if (typeof data === "number") {
    return data / divisor;
  }

  const fieldList = Array.isArray(fields) ? fields : [fields];

  const normalizeObject = (item: T): T => {
    const newItem = { ...item };
    for (const field of fieldList) {
      if (typeof newItem[field] === "number") {
        newItem[field] = ((newItem[field] as number) / divisor) as any;
      }
    }
    return newItem;
  };

  if (Array.isArray(data)) {
    return data.map((item) => normalizeObject(item));
  }

  // 单个对象
  return normalizeObject(data);
}

export type ObjectValues<T> = T[keyof T];
