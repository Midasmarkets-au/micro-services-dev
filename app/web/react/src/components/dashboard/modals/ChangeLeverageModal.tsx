'use client';

import { useEffect, useState } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/radix/Select';
import { useServerAction } from '@/hooks/useServerAction';
import { getAccountLeverageConfig, changeLeverage } from '@/actions';
import { useToast } from '@/hooks/useToast';

interface ChangeLeverageModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  accountUid: number;
  accountNumber: number;
  currentLeverage: number;
}

export function ChangeLeverageModal({
  open,
  onOpenChange,
  onSuccess,
  accountUid,
  accountNumber,
  currentLeverage,
}: ChangeLeverageModalProps) {
  const t = useTranslations('accounts');
  const { showSuccess } = useToast();
  const { execute, isLoading } = useServerAction({ showErrorToast: true });

  const [leverageOptions, setLeverageOptions] = useState<number[]>([]);
  const [selectedLeverage, setSelectedLeverage] = useState<string>('');
  const [isLoadingConfig, setIsLoadingConfig] = useState(false);

  // 加载杠杆配置
  useEffect(() => {
    if (!open) return;

    const loadConfig = async () => {
      setIsLoadingConfig(true);
      try {
        const result = await execute(getAccountLeverageConfig, accountNumber);
        if (result.success && result.data) {
          setLeverageOptions(result.data.leverages || []);
          // 设置默认选中当前杠杆或第一个选项
          const defaultLeverage = result.data.leverages?.find(l => l !== currentLeverage) || result.data.leverages?.[0];
          if (defaultLeverage) {
            setSelectedLeverage(String(defaultLeverage));
          }
        }
      } finally {
        setIsLoadingConfig(false);
      }
    };

    loadConfig();
  }, [open, accountNumber, currentLeverage, execute]);

  const handleSubmit = async () => {
    if (!selectedLeverage) return;

    const result = await execute(changeLeverage, accountUid, accountNumber, Number(selectedLeverage));
    if (result.success) {
      showSuccess(t('toast.leverageChanged'));
      onSuccess();
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent onOpenAutoFocus={(e) => e.preventDefault()}>
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold text-text-primary">
            {t('modal.changeLeverage')}
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-6 py-6">
          {/* 当前杠杆 */}
          <div className="text-base text-text-secondary">
            {t('modal.currentLeverage', { accountNumber, leverage: currentLeverage })}
          </div>

          {/* 分隔线 */}
          <div className="h-px bg-border" />

          {/* 新杠杆选择 */}
          <div className="flex flex-col gap-2">
            <label className="flex items-center text-sm font-medium text-text-secondary">
              <span className="text-primary mr-1">*</span>
              {t('fields.newLeverage')}
            </label>
            <Select 
              value={selectedLeverage} 
              onValueChange={setSelectedLeverage} 
              disabled={isLoadingConfig}
            >
              <SelectTrigger className="w-full h-12 bg-input-bg border-border">
                <SelectValue placeholder={t('placeholder.selectLeverage')} />
              </SelectTrigger>
              <SelectContent>
                {leverageOptions.map((lev) => (
                  <SelectItem key={lev} value={String(lev)}>
                    {lev}:1
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <DialogFooter className="flex flex-row gap-5 justify-end pt-5 border-t border-border">
          <Button
            variant="secondary"
            onClick={() => onOpenChange(false)}
            disabled={isLoading}
            className="w-[120px]"
          >
            {t('action.close')}
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={isLoading}
            disabled={isLoading || isLoadingConfig || !selectedLeverage}
            className="w-[120px]"
          >
            {t('action.submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
