'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getSalesLeads, getSalesLeadDetail, addSalesLeadComment } from '@/actions';
import { useSalesStore } from '@/stores/salesStore';
import { Avatar, Button, Input, Pagination } from '@/components/ui';
import type { SalesLead, SalesLeadDetail } from '@/types/sales';

export default function SalesLeadPage() {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const [data, setData] = useState<SalesLead[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const size = 15;

  const [selectedLead, setSelectedLead] = useState<SalesLeadDetail | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [comment, setComment] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchData = useCallback(
    async (p: number) => {
      if (!salesAccount) return;
      setIsLoading(true);
      const result = await execute(getSalesLeads, salesAccount.uid, { page: p, size });
      if (result.success && result.data) {
        setData(Array.isArray(result.data.data) ? result.data.data : []);
        setTotal(result.data.criteria?.total || 0);
      }
      setIsLoading(false);
    },
    [salesAccount, execute]
  );

  useEffect(() => {
    fetchData(1);
  }, [fetchData]);

  const handleViewDetail = async (leadId: number) => {
    if (!salesAccount) return;
    const result = await execute(getSalesLeadDetail, salesAccount.uid, leadId);
    if (result.success && result.data) {
      setSelectedLead(result.data);
      setShowDetail(true);
    }
  };

  const handleAddComment = async () => {
    if (!salesAccount || !selectedLead || !comment.trim()) return;
    setIsSubmitting(true);
    const result = await execute(addSalesLeadComment, salesAccount.uid, selectedLead.id, {
      content: comment.trim(),
    });
    if (result.success) {
      setComment('');
      const refreshed = await execute(getSalesLeadDetail, salesAccount.uid, selectedLead.id);
      if (refreshed.success && refreshed.data) {
        setSelectedLead(refreshed.data);
      }
    }
    setIsSubmitting(false);
  };

  const getLeadName = (lead: SalesLead) =>
    lead.user?.nativeName || lead.user?.displayName || lead.name || '--';

  return (
    <div className="flex w-full flex-col gap-5">
      {salesAccount && (
        <>
          {showDetail && selectedLead ? (
            <div className="rounded-xl border border-border bg-surface">
              <div className="flex items-center justify-between border-b border-border p-4">
                <h3 className="text-lg font-semibold text-text-primary">{t('lead.details')}</h3>
                <Button size="sm" variant="secondary" onClick={() => setShowDetail(false)}>
                  {t('action.reset')}
                </Button>
              </div>
              <div className="p-5">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.name')}</span>
                    <p className="text-sm text-text-primary">{getLeadName(selectedLead)}</p>
                  </div>
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.email')}</span>
                    <p className="text-sm text-text-primary">{selectedLead.email || '--'}</p>
                  </div>
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.phone')}</span>
                    <p className="text-sm text-text-primary">{selectedLead.phoneNumber || '--'}</p>
                  </div>
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.status')}</span>
                    <p className="text-sm text-text-primary">{selectedLead.status ?? '--'}</p>
                  </div>
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.assigned')}</span>
                    <p className="text-sm text-text-primary">{selectedLead.hasAssignedToSales ? 'Yes' : 'No'}</p>
                  </div>
                  <div>
                    <span className="text-xs text-text-secondary">{t('fields.createdOn')}</span>
                    <p className="text-sm text-text-primary">{new Date(selectedLead.createdOn).toLocaleString()}</p>
                  </div>
                </div>

                <div className="mt-6">
                  <h4 className="mb-3 text-sm font-semibold text-text-primary">{t('lead.comments')}</h4>
                  {selectedLead.comments && selectedLead.comments.length > 0 ? (
                    <div className="flex flex-col gap-3">
                      {selectedLead.comments.map((c) => (
                        <div key={c.id} className="rounded-lg bg-surface-secondary p-3">
                          <p className="text-sm text-text-primary">{c.content}</p>
                          <div className="mt-1 flex gap-2 text-xs text-text-secondary">
                            <span>{c.createdBy}</span>
                            <span>{new Date(c.createdOn).toLocaleString()}</span>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-text-secondary">{t('lead.noComments')}</p>
                  )}

                  <div className="mt-4 flex gap-2">
                    <Input
                      value={comment}
                      onChange={(e) => setComment(e.target.value)}
                      placeholder={t('lead.addComment')}
                      className="flex-1"
                    />
                    <Button
                      size="sm"
                      onClick={handleAddComment}
                      loading={isSubmitting}
                      disabled={!comment.trim()}
                    >
                      {t('action.submit')}
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="rounded-xl border border-border bg-surface">
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-border text-xs text-text-secondary">
                      <th className="px-4 py-3">{t('fields.name')}</th>
                      <th className="px-4 py-3">{t('fields.email')}</th>
                      <th className="px-4 py-3">{t('fields.phone')}</th>
                      <th className="px-4 py-3">{t('fields.status')}</th>
                      <th className="px-4 py-3">{t('fields.assigned')}</th>
                      <th className="px-4 py-3">{t('fields.createdOn')}</th>
                      <th className="px-4 py-3">{t('fields.actions')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoading ? (
                      <tr><td colSpan={7} className="py-12 text-center">
                        <div className="inline-block h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                      </td></tr>
                    ) : data.length === 0 ? (
                      <tr><td colSpan={7} className="py-12 text-center text-text-secondary">{t('lead.noLeads')}</td></tr>
                    ) : (
                      data.map((item, idx) => (
                        <tr key={item.id ?? idx} className="border-b border-border last:border-0 hover:bg-surface-secondary/50">
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              <Avatar src={item.user?.avatar} alt={getLeadName(item)} size="xs" />
                              <span className="text-sm text-text-primary">{getLeadName(item)}</span>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-text-secondary">{item.email || '-'}</td>
                          <td className="px-4 py-3 text-text-secondary">{item.phoneNumber || '-'}</td>
                          <td className="px-4 py-3 text-text-secondary">{item.status ?? '-'}</td>
                          <td className="px-4 py-3 text-text-secondary">{item.hasAssignedToSales ? 'Yes' : 'No'}</td>
                          <td className="px-4 py-3 text-xs text-text-secondary">{new Date(item.createdOn).toLocaleDateString()}</td>
                          <td className="px-4 py-3">
                            <button
                              type="button"
                              onClick={() => handleViewDetail(item.id)}
                              className="text-sm text-primary hover:underline"
                            >
                              {t('action.viewDetails')}
                            </button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
              <div className="px-4 pb-4">
                <Pagination page={page} total={total} size={size} onPageChange={(p) => { setPage(p); fetchData(p); }} />
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
