'use client';

import { useCallback, useMemo } from 'react';
import { useServerAction } from '@/hooks/useServerAction';
import { getIBTradeReports } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { TradeReportTable } from '@/components/TradeReportTable';

export default function IBTradePage() {
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);

  const fetchData = useCallback(
    async (params: Record<string, unknown>) => {
      if (!agentAccount) return null;
      const result = await execute(getIBTradeReports, agentAccount.uid, params);
      if (result.success && result.data) {
        return {
          data: result.data.data,
          criteria: result.data.criteria,
        };
      }
      return null;
    },
    [agentAccount, execute],
  );

  return (
    <div className="flex min-h-full w-full min-w-0 flex-col gap-5 overflow-hidden rounded bg-surface p-5">
      <TradeReportTable
        fetchData={fetchData}
        filterOptions={['isClosed', 'service', 'product', 'account', 'datePicker', 'allHistory']}
        showAccountNumber={true}
        autoFetchKey={agentAccount?.uid}
      />
    </div>
  );
}
