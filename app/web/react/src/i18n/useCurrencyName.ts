'use client';

import { useCallback } from 'react';
import { useTranslations } from 'next-intl';

export function useCurrencyName() {
  const tCurrency = useTranslations('common.currencyName');

  return useCallback(
    (currencyId?: number | string | null) => {
      const key =
        currencyId === undefined || currencyId === null
          ? 'undefined'
          : String(currencyId);

      if (tCurrency.has(key)) return tCurrency(key);
      if (tCurrency.has('840')) return tCurrency('840');
      return 'USD';
    },
    [tCurrency]
  );
}
