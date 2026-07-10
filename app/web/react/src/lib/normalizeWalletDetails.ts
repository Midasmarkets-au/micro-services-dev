import { normalizeAmountList } from '@/lib/utils';
import type { WalletDetailsStat, WalletDetailCategoryStat } from '@/types/sales';

export function normalizeWalletDetails(
  walletDetails?: WalletDetailsStat,
): WalletDetailsStat | undefined {
  if (!walletDetails) return undefined;
  const raw = walletDetails as WalletDetailsStat & { eventShop?: WalletDetailCategoryStat };
  if (raw.eventShop && !raw.rewards) {
    raw.rewards = raw.eventShop;
    delete raw.eventShop;
  }
  const categories = ['refund', 'rebate', 'rewards', 'adjust'] as const;
  const normalized: WalletDetailsStat = {};
  for (const key of categories) {
    const cat = raw[key];
    if (!cat) continue;
    const amounts = cat.amounts ? { ...cat.amounts } : undefined;
    if (amounts) {
      for (const currencyId of Object.keys(amounts)) {
        amounts[currencyId] = normalizeAmountList(amounts[currencyId]) as number;
      }
    }
    normalized[key] = {
      amounts,
      totalUsd: cat.totalUsd != null ? normalizeAmountList(cat.totalUsd) as number : undefined,
    };
  }
  return Object.keys(normalized).length > 0 ? normalized : undefined;
}
