'use client';

import { useCallback, useMemo } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesTradeReports } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { TradeReportTable } from '@/components/TradeReportTable';

export default function SalesTradePage() {
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const todayRange = useMemo(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return { from: d, to: d };
  }, []);

  const defaultParam = useMemo(() => ({
    isClosed: true,
    dateRange: todayRange,
  }), [todayRange]);

  const fetchData = useCallback(
    async (params: Record<string, unknown>) => {
      if (!salesAccount) return null;
      const result = await execute(getSalesTradeReports, salesAccount.uid, params);
      if (result.success && result.data) {
        return {
          data: result.data.data,
          criteria: result.data.criteria,
        };
      }
      return null;
    },
    [salesAccount, execute],
  );

  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 rounded bg-surface p-5">
      <TradeReportTable
        fetchData={fetchData}
        filterOptions={['isClosed', 'service', 'product', 'account', 'datePicker', 'allHistory']}
        defaultParam={defaultParam}
        showAccountNumber={true}
        autoFetchKey={salesAccount?.uid}
      />
    </div>
  );
}
