'use client';

import { useState, useEffect, useCallback } from 'react';
import { useParams } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { Pagination, Tabs } from '@/components/ui';
import type { TabItem } from '@/components/ui';
import {
  getSalesClientTrades,
  getSalesClientTransactions,
  getSalesAccountDetail,
} from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { formatBalance } from '@/types/accounts';
import { TradeFilter } from '@/components/TradeFilter';

type TabId = 'deposit' | 'withdrawal' | 'transaction' | 'trade';

export default function SalesCustomerDetailPage() {
  const t = useTranslations('sales');
  const { accountId } = useParams<{ accountId: string }>();
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);

  const [activeTab, setActiveTab] = useState<TabId>('trade');
  const [accountDetail, setAccountDetail] = useState<Record<string, unknown> | null>(null);
  const [trades, setTrades] = useState<unknown[]>([]);
  const [transactions, setTransactions] = useState<unknown[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const size = 15;

  const uid = Number(accountId);

  useEffect(() => {
    if (!salesAccount) return;
    execute(getSalesAccountDetail, salesAccount.uid, uid).then((res) => {
      if (res.success) setAccountDetail(res.data as Record<string, unknown>);
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [salesAccount, uid]);

  const fetchTabData = useCallback(
    async (tab: TabId, p: number, params?: Record<string, unknown>) => {
      if (!salesAccount) return;
      setIsLoading(true);
      const queryParams = { page: p, size, ...params };

      if (tab === 'trade') {
        const result = await execute(getSalesClientTrades, salesAccount.uid, uid, queryParams);
        if (result.success && result.data) {
          setTrades(Array.isArray(result.data.data) ? result.data.data : []);
          setTotal(result.data.criteria?.total || 0);
        }
      } else if (tab === 'transaction') {
        const result = await execute(getSalesClientTransactions, salesAccount.uid, uid, queryParams);
        if (result.success && result.data) {
          const d = result.data as { data?: unknown[]; criteria?: { total?: number } };
          setTransactions(Array.isArray(d.data) ? d.data : []);
          setTotal(d.criteria?.total || 0);
        }
      }
      setIsLoading(false);
    },
    [salesAccount, uid, execute]
  );

  useEffect(() => {
    fetchTabData(activeTab, 1);
  }, [activeTab, fetchTabData]);

  const tabs: TabItem<TabId>[] = [
    { key: 'trade', label: t('menu.trade') },
    { key: 'transaction', label: t('menu.transaction') },
  ];

  const user = accountDetail?.user as { displayName?: string; nativeName?: string; email?: string } | undefined;
  const tradeAccount = accountDetail?.tradeAccount as { balanceInCents?: number; currencyId?: number; accountNumber?: number } | undefined;

  return (
    <div className="flex w-full flex-col gap-5">
      {salesAccount && (
        <>
          {accountDetail && (
            <div className="rounded-xl border border-border bg-surface p-5">
              <h3 className="text-lg font-semibold text-text-primary">
                {user?.nativeName || user?.displayName || '--'}
              </h3>
              <p className="text-sm text-text-secondary">{user?.email || '--'}</p>
              {tradeAccount && (
                <div className="mt-3 flex gap-6">
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.accountNo')}</span>
                    <p className="text-sm font-medium text-text-primary">{tradeAccount.accountNumber ?? '--'}</p>
                  </div>
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.balance')}</span>
                    <p className="text-sm font-medium text-text-primary">
                      {formatBalance(tradeAccount.balanceInCents ?? 0, tradeAccount.currencyId ?? 840)}
                    </p>
                  </div>
                </div>
              )}
            </div>
          )}

          <div className="rounded-xl border border-border bg-surface">
            <div className="px-5 pt-3">
              <Tabs
                tabs={tabs}
                activeKey={activeTab}
                onChange={(key) => { setActiveTab(key); setPage(1); }}
                size="sm"
              />
            </div>

            <div className="border-b border-border p-4">
              <TradeFilter
                type="trade"
                translationNamespace="sales"
                filterOptions={['datePicker']}
                onSearch={(params) => { setPage(1); fetchTabData(activeTab, 1, params); }}
                onReset={() => { setPage(1); fetchTabData(activeTab, 1); }}
                isLoading={isLoading}
              />
            </div>

            <div className="overflow-x-auto">
              {activeTab === 'trade' && (
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-border text-xs text-text-secondary">
                      <th className="px-4 py-3">{t('fields.ticket')}</th>
                      <th className="px-4 py-3">{t('fields.symbol')}</th>
                      <th className="px-4 py-3">{t('fields.volume')}</th>
                      <th className="px-4 py-3">{t('fields.openPrice')}</th>
                      <th className="px-4 py-3">{t('fields.closePrice')}</th>
                      <th className="px-4 py-3">{t('fields.profit')}</th>
                      <th className="px-4 py-3">{t('fields.openTime')}</th>
                      <th className="px-4 py-3">{t('fields.closeTime')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoading ? (
                      <tr><td colSpan={8} className="py-12 text-center">
                        <div className="inline-block h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                      </td></tr>
                    ) : trades.length === 0 ? (
                      <tr><td colSpan={8} className="py-12 text-center text-text-secondary">{t('dashboard.noData')}</td></tr>
                    ) : (
                      trades.map((rawItem, idx) => {
                        const item = rawItem as Record<string, unknown>;
                        return (
                          <tr key={(item.id as number) ?? idx} className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
                            <td className="px-4 py-3 text-text-primary">{String(item.ticket ?? '-')}</td>
                            <td className="px-4 py-3 text-text-primary">{String(item.symbol ?? '-')}</td>
                            <td className="px-4 py-3">{Number(item.volume ?? 0).toFixed(2)}</td>
                            <td className="px-4 py-3">{Number(item.openPrice ?? 0).toFixed(2)}</td>
                            <td className="px-4 py-3">{Number(item.closePrice ?? 0).toFixed(2)}</td>
                            <td className="px-4 py-3">{Number(item.profit ?? 0).toFixed(2)}</td>
                            <td className="px-4 py-3 text-xs text-text-secondary">{item.openTime ? new Date(String(item.openTime)).toLocaleString() : '-'}</td>
                            <td className="px-4 py-3 text-xs text-text-secondary">{item.closeTime ? new Date(String(item.closeTime)).toLocaleString() : '-'}</td>
                          </tr>
                        );
                      })
                    )}
                  </tbody>
                </table>
              )}

              {activeTab === 'transaction' && (
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-border text-xs text-text-secondary">
                      <th className="px-4 py-3">{t('fields.type')}</th>
                      <th className="px-4 py-3">{t('fields.amount')}</th>
                      <th className="px-4 py-3">{t('fields.currency')}</th>
                      <th className="px-4 py-3">{t('fields.status')}</th>
                      <th className="px-4 py-3">{t('fields.createdOn')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoading ? (
                      <tr><td colSpan={5} className="py-12 text-center">
                        <div className="inline-block h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                      </td></tr>
                    ) : transactions.length === 0 ? (
                      <tr><td colSpan={5} className="py-12 text-center text-text-secondary">{t('dashboard.noData')}</td></tr>
                    ) : (
                      transactions.map((rawTx, idx) => {
                        const tx = rawTx as Record<string, unknown>;
                        return (
                          <tr key={(tx.id as number) ?? idx} className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
                            <td className="px-4 py-3 text-text-primary">{String(tx.type ?? '-')}</td>
                            <td className="px-4 py-3 text-text-primary">{formatBalance(Number(tx.amount ?? 0), Number(tx.currencyId ?? 840))}</td>
                            <td className="px-4 py-3 text-text-secondary">{String(tx.currencyId ?? '-')}</td>
                            <td className="px-4 py-3 text-text-secondary">{String(tx.stateId ?? '-')}</td>
                            <td className="px-4 py-3 text-xs text-text-secondary">{tx.createdOn ? new Date(String(tx.createdOn)).toLocaleDateString() : '-'}</td>
                          </tr>
                        );
                      })
                    )}
                  </tbody>
                </table>
              )}
            </div>

            <div className="px-4 pb-4">
              <Pagination page={page} total={total} size={size} onPageChange={(p) => { setPage(p); fetchTabData(activeTab, p); }} />
            </div>
          </div>
        </>
      )}
    </div>
  );
}
