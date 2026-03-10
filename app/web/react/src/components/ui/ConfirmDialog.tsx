'use client';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui/radix/Button';

interface ConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description: string;
  confirmText?: string;
  cancelText?: string;
  loading?: boolean;
  maxWidth?: string; // 自定义最大宽度，如 '400px', '500px' 等
}

export function ConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  title,
  description,
  confirmText = '确认',
  cancelText = '取消',
  loading = false,
  maxWidth = '400px',
}: ConfirmDialogProps) {
  const handleConfirm = () => {
    onConfirm();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent 
        className="!p-5 sm:!w-auto" 
        style={{ maxWidth }}
      >
        <DialogHeader className="flex flex-col items-center gap-10">
          <DialogTitle className="text-xl font-semibold text-text-primary text-center">
            {title}
          </DialogTitle>
          <DialogDescription className="text-xl text-text-secondary text-center">
            {description}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="flex flex-row gap-5 mt-10 justify-center">
          <Button
            variant="secondary"
            onClick={onClose}
            disabled={loading}
            className="w-[120px]"
          >
            {cancelText}
          </Button>
          <Button
            variant="primary"
            onClick={handleConfirm}
            loading={loading}
            disabled={loading}
            className="w-[120px]"
          >
            {confirmText}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
