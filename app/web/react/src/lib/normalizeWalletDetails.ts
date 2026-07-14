import { normalizeAmountList } from '@/lib/utils';
import type { WalletDetailsStat, WalletDetailCategoryStat } from '@/types/sales';

type RawWalletDetailCategory = WalletDetailCategoryStat & {
  Amounts?: Record<string, number>;
  TotalUsd?: number;
};

type RawWalletDetails = WalletDetailsStat & {
  eventShop?: WalletDetailCategoryStat;
  EventShop?: WalletDetailCategoryStat;
  Refund?: RawWalletDetailCategory;
  Rebate?: RawWalletDetailCategory;
  Rewards?: RawWalletDetailCategory;
  Adjust?: RawWalletDetailCategory;
};

function pickCategory(
  raw: RawWalletDetails,
  camelKey: keyof WalletDetailsStat,
  pascalKey: keyof RawWalletDetails,
): RawWalletDetailCategory | undefined {
  const camel = raw[camelKey] as RawWalletDetailCategory | undefined;
  if (camel) return camel;
  const pascal = raw[pascalKey] as RawWalletDetailCategory | undefined;
  if (!pascal) return undefined;
  return {
    amounts: pascal.amounts ?? pascal.Amounts,
    totalUsd: pascal.totalUsd ?? pascal.TotalUsd,
  };
}

export function normalizeWalletDetails(
  walletDetails?: WalletDetailsStat,
): WalletDetailsStat | undefined {
  if (!walletDetails) return undefined;
  const raw = walletDetails as RawWalletDetails;

  const categories = ['refund', 'rebate', 'rewards', 'adjust'] as const;
  const pascalKeys = ['Refund', 'Rebate', 'Rewards', 'Adjust'] as const;
  const normalized: WalletDetailsStat = {};

  for (let i = 0; i < categories.length; i++) {
    const key = categories[i];
    let cat = pickCategory(raw, key, pascalKeys[i]);

    if (key === 'rewards' && !cat) {
      cat = raw.eventShop ?? raw.EventShop;
    }

    if (!cat) continue;

    const amountsSource = cat.amounts ?? cat.Amounts;
    const amounts = amountsSource ? { ...amountsSource } : undefined;
    if (amounts) {
      for (const currencyId of Object.keys(amounts)) {
        amounts[currencyId] = normalizeAmountList(amounts[currencyId]) as number;
      }
    }

    const totalUsdRaw = cat.totalUsd ?? cat.TotalUsd;
    normalized[key] = {
      amounts,
      totalUsd: totalUsdRaw != null ? normalizeAmountList(totalUsdRaw) as number : undefined,
    };
  }

  return Object.keys(normalized).length > 0 ? normalized : undefined;
}
