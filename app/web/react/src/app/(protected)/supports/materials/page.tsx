'use client';

import { useEffect, useState, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useServerAction } from '@/hooks/useServerAction';
import { Input, Button, SimpleSelect } from '@/components/ui';
import {
  submitMaterialRequest,
  getMyMaterialRequests,
  uploadMaterialAttachment,
  type MaterialRequestItem,
} from '@/actions';

const materialRequestSchema = z.object({
  materialType: z.string().min(1, 'materialTypeRequired'),
  quantity: z.string().optional(),
  description: z.string().min(1, 'descriptionRequired'),
});

type MaterialRequestFormData = z.infer<typeof materialRequestSchema>;

const STATUS = {
  AwaitingReview: 1,
  Approved: 4,
  Rejected: 5,
} as const;

function StatusPill({ status }: { status: number }) {
  const t = useTranslations('supports.materials');

  const style =
    status === STATUS.Approved
      ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
      : status === STATUS.Rejected
        ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
        : 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';

  const label =
    status === STATUS.Approved
      ? t('statusApproved')
      : status === STATUS.Rejected
        ? t('statusRejected')
        : t('statusPending');

  return <span className={`text-xs font-medium px-2.5 py-1 rounded-full whitespace-nowrap ${style}`}>{label}</span>;
}

export default function MaterialsRequestPage() {
  const t = useTranslations('supports.materials');
  const tCommon = useTranslations('common');
  const { execute, isLoading } = useServerAction();

  const [submitSuccess, setSubmitSuccess] = useState(false);
  const [files, setFiles] = useState<File[]>([]);
  const [items, setItems] = useState<MaterialRequestItem[]>([]);
  const [isLoadingList, setIsLoadingList] = useState(true);

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<MaterialRequestFormData>({
    resolver: zodResolver(materialRequestSchema),
    defaultValues: { materialType: '', quantity: '', description: '' },
  });

  const loadItems = useCallback(async () => {
    setIsLoadingList(true);
    const result = await execute(getMyMaterialRequests);
    if (result.success && result.data) {
      setItems(result.data);
    }
    setIsLoadingList(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    loadItems();
  }, [loadItems]);

  const onFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files || []);
    setFiles((prev) => [...prev, ...selected]);
    e.target.value = '';
  };

  const removeFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const onSubmit = async (data: MaterialRequestFormData) => {
    const result = await execute(submitMaterialRequest, {
      materialType: data.materialType,
      description: data.description,
      quantity: data.quantity ? Number(data.quantity) : undefined,
    });

    if (result.success && result.data) {
      const requestId = result.data.id;
      for (const file of files) {
        await execute(uploadMaterialAttachment, requestId, file);
      }

      setSubmitSuccess(true);
      reset();
      setFiles([]);
      loadItems();
      setTimeout(() => setSubmitSuccess(false), 3000);
    }
  };

  return (
    <div className="flex flex-col gap-10">
      <div className="flex flex-col items-center">
        <h2 className="text-xl font-semibold text-text-primary mb-8">{t('newRequest')}</h2>

        {submitSuccess ? (
          <div className="flex flex-col items-center gap-4 py-10">
            <div className="size-16 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
              <svg className="size-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <p className="text-text-primary font-medium">{t('submitSuccess')}</p>
          </div>
        ) : (
          <form onSubmit={handleSubmit(onSubmit)} className="w-full max-w-2xl space-y-5">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-sm text-text-secondary mb-2">
                  <span className="text-primary">*</span>
                  {t('materialType')}
                </label>
                <Controller
                  name="materialType"
                  control={control}
                  render={({ field }) => (
                    <SimpleSelect
                      value={field.value}
                      onChange={field.onChange}
                      placeholder={t('materialTypePlaceholder')}
                      options={[
                        { value: 'banner', label: t('typeBanner') },
                        { value: 'brochure', label: t('typeBrochure') },
                        { value: 'ebook', label: t('typeEbook') },
                        { value: 'other', label: t('typeOther') },
                      ]}
                    />
                  )}
                />
                {errors.materialType && (
                  <p className="mt-1 text-sm text-red-500">{t(`errors.${errors.materialType.message}`)}</p>
                )}
              </div>
              <div>
                <label className="block text-sm text-text-secondary mb-2">{t('quantity')}</label>
                <Input
                  {...register('quantity')}
                  type="number"
                  min={1}
                  placeholder={t('quantityPlaceholder')}
                  className="w-full dark:bg-surface-secondary dark:border-[#333]"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm text-text-secondary mb-2">
                <span className="text-primary">*</span>
                {t('description')}
              </label>
              <textarea
                {...register('description')}
                rows={5}
                placeholder={t('descriptionPlaceholder')}
                className={`input-field h-auto! py-[14px]! dark:bg-surface-secondary dark:border-[#333] ${errors.description ? 'error-border' : ''}`}
              />
              {errors.description && (
                <p className="mt-1 text-sm text-red-500">{t(`errors.${errors.description.message}`)}</p>
              )}
            </div>

            <div>
              <label className="block text-sm text-text-secondary mb-2">{t('attachments')}</label>
              <div className="flex flex-wrap items-center gap-3 p-4 rounded border border-dashed border-border bg-surface-secondary">
                {files.map((file, index) => (
                  <span
                    key={`${file.name}-${index}`}
                    className="flex items-center gap-2 text-xs bg-surface px-3 py-1.5 rounded border border-border"
                  >
                    {file.name}
                    <button type="button" onClick={() => removeFile(index)} className="text-error">
                      ×
                    </button>
                  </span>
                ))}
                <label className="cursor-pointer text-sm text-primary">
                  + {t('addFile')}
                  <input type="file" accept="image/*,.pdf" multiple className="hidden" onChange={onFileSelect} />
                </label>
              </div>
              <p className="mt-1 text-xs text-text-secondary">{t('attachmentsHint')}</p>
            </div>

            <div className="flex justify-center pt-5">
              <Button type="submit" disabled={isSubmitting || isLoading} className="w-full max-w-xs">
                {isSubmitting || isLoading ? tCommon('loading') : tCommon('submit')}
              </Button>
            </div>
          </form>
        )}
      </div>

      <div className="flex flex-col gap-4">
        <h3 className="text-lg font-semibold text-text-primary">{t('myRequests')}</h3>

        {isLoadingList ? (
          <p className="text-text-secondary text-sm">{tCommon('loading')}</p>
        ) : items.length === 0 ? (
          <p className="text-text-secondary text-sm">{t('noRequests')}</p>
        ) : (
          <div className="flex flex-col gap-3">
            {items.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between gap-4 p-4 rounded-lg border border-border bg-surface"
              >
                <div className="flex flex-col gap-1">
                  <span className="font-medium text-text-primary">
                    {item.content.materialType}
                    {item.content.quantity ? ` · ${item.content.quantity}` : ''}
                  </span>
                  <span className="text-xs text-text-secondary">
                    {new Date(item.createdOn).toLocaleDateString()}
                  </span>
                  {item.status === STATUS.Rejected && item.note && (
                    <span className="text-xs text-error">{t('reason')}: {item.note}</span>
                  )}
                </div>
                <StatusPill status={item.status} />
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
