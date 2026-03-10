'use client';

import { useState, useRef, ChangeEvent } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui/radix/Button';
import { SearchableSelect } from '@/components/ui/SearchableSelect';

// 文档类型枚举 (对应后端接口参数)
export enum VerificationDocumentTypes {
  IdFront = 'id_front',
  IdBack = 'id_back',
  Address = 'address',
  Other = 'other',
}

// 文件项接口
interface FileItem {
  id: string;
  file: File;
  preview: string;
  type: string;
}

interface UploadFileModalProps {
  isOpen: boolean;
  onClose: () => void;
  onUpload: (file: File, documentType: VerificationDocumentTypes) => Promise<void>;
  maxFiles?: number; // 最大文件数量限制（可选，不传则不限制）
  currentFilesCount?: number; // 当前已上传的文件数量（从外层传入）
}

// 获取文件图标
const getFileIcon = (fileName: string): string => {
  const ext = fileName.split('.').pop()?.toLowerCase();
  if (ext === 'pdf') {
    return '/images/icons/document-file-day.svg'; // PDF 图标
  }
  return '/images/icons/file-day.svg'; // 默认文件图标
};

export function UploadFileModal({ 
  isOpen, 
  onClose, 
  onUpload, 
  maxFiles,
  currentFilesCount = 0 
}: UploadFileModalProps) {
  const t = useTranslations('profile.files');
  const tCommon = useTranslations('common');
  const tDocTypes = useTranslations('verification.documentTypes');
  const fileInputRef = useRef<HTMLInputElement>(null);

  // 文件类型选项 (使用翻译)
  const fileTypeOptions = [
    { value: VerificationDocumentTypes.IdFront, label: tDocTypes('idFront') },
    { value: VerificationDocumentTypes.IdBack, label: tDocTypes('idBack') },
    { value: VerificationDocumentTypes.Address, label: tDocTypes('addressProof') },
    { value: VerificationDocumentTypes.Other, label: tDocTypes('other') },
  ];
  
  const [selectedType, setSelectedType] = useState<VerificationDocumentTypes>(
    VerificationDocumentTypes.Other
  );
  const [files, setFiles] = useState<FileItem[]>([]);
  const [isUploading, setIsUploading] = useState(false);

  // 计算当前总文件数量
  const totalFilesCount = currentFilesCount + files.length;
  console.log('totalFilesCount', totalFilesCount);
  
  // 是否达到最大数量限制
  const isMaxFilesReached = maxFiles !== undefined && totalFilesCount >= maxFiles;

  // 处理文件选择
  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(e.target.files || []);
    
    // 如果有最大数量限制，计算还能添加多少文件
    let filesToAdd = selectedFiles;
    if (maxFiles !== undefined) {
      const remainingSlots = maxFiles - totalFilesCount;
      if (remainingSlots <= 0) {
        // 已达到最大数量，不添加任何文件
        if (fileInputRef.current) {
          fileInputRef.current.value = '';
        }
        return;
      }
      // 只添加剩余数量的文件
      filesToAdd = selectedFiles.slice(0, remainingSlots);
    }
    
    const newFiles: FileItem[] = filesToAdd.map((file) => ({
      id: `${Date.now()}-${Math.random()}`,
      file,
      preview: URL.createObjectURL(file),
      type: file.type,
    }));
    
    setFiles((prev) => [...prev, ...newFiles]);
    
    // 重置 input 以允许选择相同文件
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  // 删除文件
  const handleRemoveFile = (fileId: string) => {
    setFiles((prev) => {
      const file = prev.find((f) => f.id === fileId);
      if (file?.preview) {
        URL.revokeObjectURL(file.preview);
      }
      return prev.filter((f) => f.id !== fileId);
    });
  };

  // 提交上传
  const handleSubmit = async () => {
    if (files.length === 0) {
      return;
    }
    
    setIsUploading(true);
    try {
      // 逐个上传文件
      for (const fileItem of files) {
        await onUpload(fileItem.file, selectedType);
      }
      
      // 清理并关闭
      files.forEach((f) => {
        if (f.preview) {
          URL.revokeObjectURL(f.preview);
        }
      });
      setFiles([]);
      onClose();
    } catch (error) {
      console.error('Upload failed:', error);
    } finally {
      setIsUploading(false);
    }
  };

  // 关闭时清理
  const handleClose = () => {
    if (!isUploading) {
      files.forEach((f) => {
        if (f.preview) {
          URL.revokeObjectURL(f.preview);
        }
      });
      setFiles([]);
      onClose();
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="p-5!">
        <DialogHeader>
          <DialogTitle>{t('uploadFile')}</DialogTitle>
        </DialogHeader>

        {/* 文件类型选择器 */}
        <div className="flex flex-col gap-10">
          <div className="flex flex-col gap-2">
            <label className="text-sm text-text-secondary">
              <span className="text-(--color-error)">* </span>
              {t('fileType')}
            </label>
            <SearchableSelect
              value={fileTypeOptions.find((opt) => opt.value === selectedType)}
              onChange={(option) => setSelectedType((option as { value: VerificationDocumentTypes })?.value || VerificationDocumentTypes.Other)}
              options={fileTypeOptions}
              placeholder={t('selectFileType')}
              isSearchable={false}
            />
          </div>

          {/* 分隔线 */}
          <div className="h-px w-full bg-border" />

          {/* 已选文件列表 */}
          {files.length > 0 && (
            <div className="flex flex-col gap-5">
              {files.map((fileItem) => (
                <div
                  key={fileItem.id}
                  className="flex items-center gap-5 md:gap-20 bg-input-bg p-5 md:p-10 rounded-[12px]"
                >
                  {/* 左侧：图标 + 文件名 */}
                  <div className="flex items-center gap-3 md:gap-4 flex-1 min-w-0">
                    <div className="cursor-pointer shrink-0 size-8 md:size-10 flex items-center justify-center">
                      <Image
                        src={getFileIcon(fileItem.file.name)}
                        alt={fileItem.file.name}
                        width={40}
                        height={40}
                        className="object-contain cursor-pointer"
                      />
                    </div>
                    <p
                      className="text-xs md:text-sm text-text-primary truncate flex-1"
                      title={fileItem.file.name}
                    >
                      {fileItem.file.name}
                    </p>
                  </div>

                  {/* 右侧：删除按钮 */}
                  <button
                    onClick={() => handleRemoveFile(fileItem.id)}
                    className="cursor-pointer shrink-0 size-5 flex items-center justify-center hover:opacity-70 transition-opacity"
                    aria-label={tCommon('delete')}
                  >
                    <Image
                      src="/images/icons/delete.svg"
                      alt="delete"
                      width={20}
                      height={20}
                      className="object-contain"
                    />
                  </button>
                </div>
              ))}
            </div>
          )}

          {/* 上传区域 - 达到最大数量时隐藏 */}
          {!isMaxFilesReached && (
            <>
              <button
                onClick={() => fileInputRef.current?.click()}
                className="cursor-pointer border-2 border-border border-dashed rounded-[12px] py-8 md:py-10 flex flex-col items-center gap-3 hover:bg-(--color-surface-secondary) transition-colors"
                disabled={isUploading}
              >
                <div className="size-8 md:size-10 flex items-center justify-center">
                  <Image
                    src="/images/icons/add.svg"
                    alt="upload"
                    width={40}
                    height={40}
                    className="object-contain"
                  />
                </div>
                <p className="text-sm text-text-secondary">{t('uploadFileBtn')}</p>
              </button>

              {/* 隐藏的文件输入 */}
              <input
                ref={fileInputRef}
                type="file"
                onChange={handleFileChange}
                multiple
                accept="image/*,application/pdf,.pdf"
                className="hidden"
              />
            </>
          )}
        </div>

        {/* 底部按钮 */}
        <DialogFooter className="flex flex-row justify-end gap-5 mt-5">
          <Button
            variant="secondary"
            onClick={handleClose}
            disabled={isUploading}
            className="w-[120px]"
          >
            {tCommon('cancel')}
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={isUploading}
            disabled={files.length === 0 || isUploading}
            className="w-[120px]"
          >
            {tCommon('submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
