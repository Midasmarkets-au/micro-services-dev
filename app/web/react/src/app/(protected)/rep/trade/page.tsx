'use client';

import { useCallback, useMemo } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { useServerAction } from '@/hooks/useServerAction';
import { getRepTradeReports } from '@/actions';
import { useRepStore } from '@/stores/repStore';
import { TradeReportTable } from '@/components/TradeReportTable';

export default function RepTradePage() {
  const { execute } = useServerAction({ showErrorToast: true });
  const repAccount = useRepStore((s) => s.repAccount);
  const searchParams = useSearchParams();
  const router = useRouter();

  const accountFromQuery = searchParams.get('accountNumber');

  const defaultParam = useMemo(() => {
    const param: Record<string, unknown> = {
      isClosed: false,
    };
    if (accountFromQuery) {
      param.account = accountFromQuery;
      setTimeout(() => router.replace('/rep/trade'), 3000);
    }
    return param;
  }, [accountFromQuery, router]);

  const fetchData = useCallback(
    async (params: Record<string, unknown>) => {
      if (!repAccount) return null;
      const result = await execute(getRepTradeReports, repAccount.uid, params);
      if (result.success && result.data) {
        return {
          data: result.data.data,
          criteria: result.data.criteria,
        };
      }
      return null;
    },
    [repAccount, execute],
  );

  return (
    <div className="flex flex-1 min-w-0 flex-col gap-5 rounded bg-surface p-5">
      <TradeReportTable
        fetchData={fetchData}
        filterOptions={['isClosed', 'service', 'product', 'account', 'datePicker', 'allHistory']}
        defaultParam={defaultParam}
        showAccountNumber={true}
        autoFetchKey={repAccount?.uid}
      />
    </div>
  );
}
