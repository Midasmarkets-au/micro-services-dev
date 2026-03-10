'use client';

import { useState, useEffect, useMemo } from 'react';
import Link from 'next/link';
import { useTranslations, useLocale } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { QRCodeSVG } from 'qrcode.react';
import { getIBLinks } from '@/actions';
import { useIBStore } from '@/stores/ibStore';
import { useUserStore } from '@/stores/userStore';
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from '@/components/ui';
import type { IBLink } from '@/types/ib';

function buildInviteUrl(code: string, siteId: number, locale: string): string {
  const origin = typeof window !== 'undefined' ? window.location.origin : '';
  return `${origin}/sign-up?code=${code}&siteId=${siteId}&lang=${locale}`;
}

export function IBLinksWidget() {
  const t = useTranslations('ib.dashboard');
  const locale = useLocale();
  const { execute } = useServerAction({ showErrorToast: true });
  const agentAccount = useIBStore((s) => s.agentAccount);
  const siteConfig = useUserStore((s) => s.siteConfig);

  const [links, setLinks] = useState<IBLink[]>([]);
  const [selectedCode, setSelectedCode] = useState<string>('');
  const [loadedUid, setLoadedUid] = useState<number | null>(null);
  const [copied, setCopied] = useState(false);

  const isLoading = !agentAccount || agentAccount.uid !== loadedUid;

  useEffect(() => {
    if (!agentAccount) return;
    let cancelled = false;

    const load = async () => {
      const result = await execute(getIBLinks, agentAccount.uid, { page: 1, size: 20 });
      if (cancelled) return;
      if (result.success && result.data?.data) {
        const items = Array.isArray(result.data.data) ? result.data.data : [];
        setLinks(items);
        const defaultLink = items.find((l) => l.isDefault) || items[0];
        if (defaultLink) setSelectedCode(defaultLink.code);
      }
      setLoadedUid(agentAccount.uid);
    };

    load();
    return () => { cancelled = true; };
  }, [agentAccount, execute]);

  const activeLink = useMemo(
    () => links.find((l) => l.code === selectedCode) || links[0],
    [links, selectedCode]
  );

  const siteId = siteConfig?.siteId ?? 1;
  const linkUrl = activeLink ? buildInviteUrl(activeLink.code, siteId, locale) : '';

  const handleCopy = async () => {
    if (!linkUrl) return;
    try {
      await navigator.clipboard.writeText(linkUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // fallback ignored
    }
  };

  return (
    <div className="flex flex-col rounded-xl border border-border bg-surface p-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-xl font-semibold text-text-primary">{t('ibLinks')}</h3>
        <Link href="/ib/iblink" className="text-base text-text-secondary hover:text-primary">
          {t('viewMore')} &gt;
        </Link>
      </div>

      {/* Divider */}
      <div className="my-5 h-px w-full bg-border" />

      {isLoading ? (
        <div className="flex flex-col items-center gap-5">
          <div className="h-8 w-full animate-pulse rounded bg-border" />
          <div className="size-[150px] animate-pulse rounded bg-border" />
          <div className="h-5 w-3/4 animate-pulse rounded bg-border" />
        </div>
      ) : links.length === 0 ? (
        <div className="flex min-h-36 flex-col items-center justify-center gap-2 py-6">
          <div className="no-data-icon" />
          <span className="text-xs text-text-secondary">{t('noData')}</span>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-5">
          {/* Code selector - Radix Select */}
          <Select value={selectedCode} onValueChange={setSelectedCode}>
            <SelectTrigger className="w-full">
              <SelectValue placeholder={activeLink?.code || '--'} />
            </SelectTrigger>
            <SelectContent>
              {links.map((link, idx) => (
                <SelectItem key={link.code || link.id} value={link.code}>
                  {link.name && link.name.length > 0 ? link.name : link.code || `Invite Link ${idx + 1}`}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* QR Code */}
          {linkUrl && (
            <div className="rounded-lg bg-white p-2">
              <QRCodeSVG
                value={linkUrl}
                size={150}
                level="H"
                bgColor="#ffffff"
                fgColor="#000000"
              />
            </div>
          )}

          {/* Link URL + Copy icon */}
          {linkUrl && (
            <div className="flex w-full items-center gap-5">
              <span className="min-w-0 flex-1 truncate text-sm text-text-secondary">
                {linkUrl}
              </span>
              <button
                onClick={handleCopy}
                className="shrink-0 text-text-secondary transition-opacity hover:opacity-70"
                title={copied ? t('copied') : t('copy')}
              >
                {copied ? (
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M20 6L9 17l-5-5" />
                  </svg>
                ) : (
                  <div className="copy-icon" />
                )}
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
