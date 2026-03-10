'use client';

import { useState, useCallback, useEffect, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button, Skeleton, DataTable, Pagination, Icon } from '@/components/ui';
import type { DataTableColumn } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useSalesStore } from '@/stores/salesStore';
import { useToast } from '@/hooks/useToast';
import { getSalesLinks, updateSalesLink } from '@/actions';
import type { SalesClientAccount, SalesLink } from '@/types/sales';

/* eslint-disable @typescript-eslint/no-explicit-any */

interface IbLinksModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: SalesClientAccount | null;
}

export function IbLinksModal({ open, onOpenChange, account }: IbLinksModalProps) {
  const t = useTranslations('sales');
  const { execute } = useServerAction({ showErrorToast: true });
  const salesAccount = useSalesStore((s) => s.salesAccount);
  const { showToast } = useToast();

  const [isLoading, setIsLoading] = useState(false);
  const [links, setLinks] = useState<SalesLink[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState('');

  const fetchLinks = useCallback(async (p: number = 1) => {
    if (!salesAccount || !account) return;
    setIsLoading(true);
    try {
      const result = await execute(getSalesLinks, salesAccount.uid, {
        page: p,
        size: 10,
        agentUid: account.uid,
      } as any);
      if (result.success && result.data) {
        setLinks(Array.isArray(result.data.data) ? result.data.data : []);
        setTotal(result.data.criteria?.total ?? 0);
        setPage(p);
      }
    } finally {
      setIsLoading(false);
    }
  }, [salesAccount, account, execute]);

  useEffect(() => {
    if (open && account) {
      fetchLinks(1);
    }
  }, [open, account, fetchLinks]);

  const handleToggleStatus = async (link: SalesLink) => {
    if (!salesAccount) return;
    const newStatus = link.status === 1 ? 0 : 1;
    const result = await execute(updateSalesLink, salesAccount.uid, link.id, { status: newStatus });
    if (result.success) {
      fetchLinks(page);
    }
  };

  const handleUpdateName = async (link: SalesLink) => {
    if (!salesAccount || !editName.trim()) return;
    const result = await execute(updateSalesLink, salesAccount.uid, link.id, { name: editName.trim() });
    if (result.success) {
      setEditingId(null);
      fetchLinks(page);
    }
  };

  const handleCopy = (text: string) => {
    navigator.clipboard.writeText(text).then(() => {
      showToast({ message: t('ibLinks.copied'), type: 'success' });
    });
  };

  const userName = account?.user?.displayName || account?.user?.nativeName || '';

  const columns = useMemo<DataTableColumn<SalesLink>[]>(() => [
    {
      key: 'name',
      title: t('ibLinks.linkName'),
      skeletonWidth: 'w-24',
      render: (link) => {
        if (editingId === link.id) {
          return (
            <div className="flex items-center gap-1">
              <input
                type="text"
                value={editName}
                onChange={(e) => setEditName(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleUpdateName(link)}
                className="input-field w-32 rounded px-2 py-1 text-sm"
                autoFocus
              />
              <Button size="xs" variant="primary" onClick={() => handleUpdateName(link)}>
                <Icon name="check" size={14} />
              </Button>
              <Button size="xs" variant="ghost" onClick={() => setEditingId(null)}>
                <Icon name="close" size={14} />
              </Button>
            </div>
          );
        }
        return (
          <div className="flex items-center gap-1">
            <span className="text-sm">{link.name || '-'}</span>
            <button
              type="button"
              onClick={() => { setEditingId(link.id); setEditName(link.name || ''); }}
              className="text-text-secondary hover:text-primary"
            >
              <Icon name="edit-line" size={14} />
            </button>
          </div>
        );
      },
    },
    {
      key: 'code',
      title: t('ibLinks.referCode'),
      skeletonWidth: 'w-20',
      render: (link) => (
        <div className="flex items-center gap-1">
          <span className="text-sm font-mono">{link.code}</span>
          <button
            type="button"
            onClick={() => handleCopy(link.code)}
            className="text-text-secondary hover:text-primary"
          >
            <Icon name="file-copy-line" size={14} />
          </button>
        </div>
      ),
    },
    {
      key: 'accountType',
      title: t('ibLinks.accountType'),
      skeletonWidth: 'w-12',
      render: (link) => <span className="text-sm">{link.accountType ?? '-'}</span>,
    },
    {
      key: 'linkType',
      title: t('ibLinks.linkType'),
      skeletonWidth: 'w-16',
      render: (link) => (
        <span className="text-sm">
          {link.role === 300 ? t('ibLinks.agent') : t('ibLinks.client')}
        </span>
      ),
    },
    {
      key: 'status',
      title: t('ibLinks.status'),
      skeletonWidth: 'w-16',
      render: (link) => (
        <span className={`rounded-full px-2 py-0.5 text-xs ${
          link.status === 1 ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
        }`}>
          {link.status === 1 ? t('ibLinks.active') : t('ibLinks.inactive')}
        </span>
      ),
    },
    {
      key: 'actions',
      title: t('fields.actions'),
      skeletonWidth: 'w-20',
      render: (link) => (
        <div className="flex items-center gap-2">
          {link.url && (
            <button
              type="button"
              onClick={() => handleCopy(link.url!)}
              className="text-sm text-primary hover:underline"
            >
              {t('ibLinks.copyLink')}
            </button>
          )}
          <button
            type="button"
            onClick={() => handleToggleStatus(link)}
            className={`text-sm hover:underline ${link.status === 1 ? 'text-red-500' : 'text-green-600'}`}
          >
            {link.status === 1 ? t('ibLinks.inactive') : t('ibLinks.active')}
          </button>
        </div>
      ),
    },
  ], [t, editingId, editName]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[900px]">
        <DialogHeader>
          <DialogTitle>{t('ibLinks.title')} - {userName}</DialogTitle>
        </DialogHeader>

        <div className="max-h-[60vh] overflow-auto">
          {isLoading && links.length === 0 ? (
            <div className="space-y-3 py-4">
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-full" />
            </div>
          ) : links.length === 0 ? (
            <div className="py-12 text-center text-text-secondary">{t('ibLinks.noLinks')}</div>
          ) : (
            <>
              <DataTable<SalesLink>
                columns={columns}
                data={links}
                rowKey={(item) => item.id}
                loading={isLoading}
                skeletonRows={3}
              />
              <div className="mt-4">
                <Pagination
                  page={page}
                  total={total}
                  size={10}
                  onPageChange={(p) => fetchLinks(p)}
                />
              </div>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
