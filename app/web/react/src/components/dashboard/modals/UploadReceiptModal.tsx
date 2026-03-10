'use client';

import { useState, useCallback, useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import {
  getDepositGuide,
  getDepositReceiptFiles,
  uploadDepositReceipt,
  getMediaUrl,
} from '@/actions';

interface UploadReceiptModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accountUid: number;
  depositHashId: string;
  paymentMethodName?: string;
  onSuccess?: () => void;
}

export function UploadReceiptModal({
  open,
  onOpenChange,
  accountUid,
  depositHashId,
  paymentMethodName,
  onSuccess,
}: UploadReceiptModalProps) {
  const t = useTranslations('accounts');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [guideInfo, setGuideInfo] = useState<{
    paymentMethodName: string;
    instruction: string;
    info: Record<string, string>;
  } | null>(null);
  const [guideLoading, setGuideLoading] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [existingReceiptUrl, setExistingReceiptUrl] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);

  const loadGuideAndReceipt = useCallback(async () => {
    if (!accountUid || !depositHashId) return;
    setGuideLoading(true);

    const [guideRes, receiptRes] = await Promise.all([
      execute(() => getDepositGuide(accountUid, depositHashId)),
      execute(() => getDepositReceiptFiles(accountUid, depositHashId)),
    ]);

    if (guideRes?.success && guideRes.data) {
      setGuideInfo(guideRes.data);
    }

    if (receiptRes?.success && receiptRes.data && receiptRes.data.length > 0) {
      const lastGuid = receiptRes.data[receiptRes.data.length - 1];
      const mediaRes = await execute(() => getMediaUrl(lastGuid));
      if (mediaRes?.success && mediaRes.data) {
        setExistingReceiptUrl(mediaRes.data);
      }
    }

    setGuideLoading(false);
  }, [accountUid, depositHashId, execute]);

  useEffect(() => {
    if (open) {
      setSelectedFile(null);
      setPreviewUrl(null);
      setExistingReceiptUrl(null);
      setGuideInfo(null);
      loadGuideAndReceipt();
    }
  }, [open, loadGuideAndReceipt]);

  const handleFileChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setSelectedFile(file);

    const reader = new FileReader();
    reader.onload = () => {
      setPreviewUrl(reader.result as string);
    };
    reader.readAsDataURL(file);
  }, []);

  const handleUpload = useCallback(async () => {
    if (!selectedFile || !accountUid || !depositHashId) return;
    setUploading(true);

    const reader = new FileReader();
    reader.onload = async () => {
      const base64 = (reader.result as string).split(',')[1];
      const res = await execute(() =>
        uploadDepositReceipt(accountUid, depositHashId, {
          name: selectedFile.name,
          type: selectedFile.type,
          data: base64,
        })
      );

      if (res?.success) {
        showSuccess(t('tip.depositUploadSuccess'));
        onOpenChange(false);
        onSuccess?.();
      }
      setUploading(false);
    };
    reader.readAsDataURL(selectedFile);
  }, [selectedFile, accountUid, depositHashId, execute, showSuccess, t, onOpenChange, onSuccess]);

  const displayImage = previewUrl || existingReceiptUrl;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('action.uploadReceipt')}</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4 py-4">
          {guideLoading ? (
            <div className="flex items-center justify-center py-8">
              <div className="size-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
            </div>
          ) : (
            <>
              {guideInfo?.instruction && (
                <div
                  className="rounded-lg border border-border bg-surface-secondary p-4 text-sm text-text-secondary"
                  dangerouslySetInnerHTML={{ __html: guideInfo.instruction }}
                />
              )}

              <div className="flex flex-col items-center gap-4">
                <div
                  className="relative flex min-h-[200px] w-full cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-border bg-surface-secondary transition-colors hover:border-primary"
                  onClick={() => fileInputRef.current?.click()}
                >
                  {displayImage ? (
                    <div className="relative size-full min-h-[200px]">
                      <Image
                        src={displayImage}
                        alt="Receipt"
                        fill
                        className="rounded-lg object-contain p-2"
                      />
                    </div>
                  ) : (
                    <div className="flex flex-col items-center gap-2 py-8">
                      <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" className="text-text-tertiary">
                        <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                        <polyline points="17,8 12,3 7,8" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                        <line x1="12" y1="3" x2="12" y2="15" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                      </svg>
                      <span className="text-sm text-text-tertiary">
                        {t('action.uploadReceipt')}
                      </span>
                    </div>
                  )}
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*,.pdf"
                    onChange={handleFileChange}
                    className="hidden"
                  />
                </div>

                {selectedFile && (
                  <span className="text-xs text-text-secondary">{selectedFile.name}</span>
                )}
              </div>
            </>
          )}
        </div>

        <DialogFooter className="gap-2">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={uploading}
          >
            {t('action.cancel')}
          </Button>
          <Button
            onClick={handleUpload}
            disabled={!selectedFile || uploading || guideLoading}
          >
            {uploading ? (
              <span className="flex items-center gap-2">
                <span className="size-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                {t('action.submit')}
              </span>
            ) : (
              t('action.submit')
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
