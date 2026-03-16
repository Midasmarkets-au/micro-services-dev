'use client';

import { useState, useRef, useEffect, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { Cross2Icon } from '@radix-ui/react-icons';
import { VerificationFormLayout } from './VerificationFormLayout';
import type { DocumentData, DocumentType } from '@/types/verification';

// 文档子步骤类型
type DocumentSubStep = 'documents' | 'address';

// 选择的文件（未上传）
interface SelectedFile {
  file: File;
  preview: string;
}

interface DocumentUploadProps {
  initialData?: DocumentData[];
  onSubmit: (files: Record<DocumentType, File | null>) => void;
  onBack: () => void;
  isLoading?: boolean;
}

// 表单区域标题
function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2">
      <div className="h-[18px] w-[3px] rounded-full bg-primary" />
      <h3 className="font-semibold text-base text-text-primary">{children}</h3>
    </div>
  );
}

// 文档上传步骤导航
function DocumentStepNav({ 
  activeStep, 
  onStepClick 
}: { 
  activeStep: DocumentSubStep;
  onStepClick?: (step: DocumentSubStep) => void;
}) {
  const t = useTranslations('verification');
  
  const steps: { id: DocumentSubStep; number: string; labelKey: string }[] = [
    { id: 'documents', number: '01', labelKey: 'documentsNav' },
    { id: 'address', number: '02', labelKey: 'addressProofNav' },
  ];
  
  const currentIndex = steps.findIndex((s) => s.id === activeStep);

  return (
    <div className="hidden md:flex w-fit shrink-0 gap-7.5">
      {/* 进度条背景 */}
      <div className="relative w-1 rounded bg-text-secondary">
        {/* 高亮进度条 */}
        <div
          className="absolute left-0 top-0 w-1 rounded bg-primary transition-all duration-300"
          style={{
            height: `${((currentIndex + 1) / steps.length) * 100}%`,
          }}
        />
      </div>

      {/* 子步骤列表 */}
      <div className="flex flex-col gap-7">
        {steps.map((step) => {
          const isCurrent = activeStep === step.id;
          const isPast = steps.findIndex((s) => s.id === step.id) < currentIndex;

          return (
            <button
              key={step.id}
              type="button"
              onClick={() => onStepClick?.(step.id)}
              className="flex items-center gap-7 text-left whitespace-nowrap"
            >
              {/* 数字 */}
              <span
                className={`font-bold text-base font-din ${
                  isCurrent || isPast ? 'text-text-secondary' : 'text-text-secondary'
                }`}
              >
                {step.number}
              </span>
              {/* 标签 */}
              <span
                className={`font-semibold text-base transition-colors duration-200 ${
                  isCurrent ? 'text-primary' : 'text-text-secondary'
                }`}
              >
                {t(`documentSteps.${step.labelKey}`)}
              </span>
            </button>
          );
        })}
      </div>
    </div>
  );
}

// 上传框组件
function UploadBox({
  label,
  selected,
  onSelect,
  onRemove,
}: {
  label: string;
  selected: SelectedFile | null;
  onSelect: () => void;
  onRemove: () => void;
}) {
  return (
    <div className="flex flex-col gap-2.5 items-center cursor-pointer">
      {selected?.preview ? (
        <div className="relative w-40 h-30">
          <div className="relative w-full h-full overflow-hidden rounded bg-surface-secondary">
            <Image
              src={selected.preview}
              alt={label}
              fill
              className="object-contain"
            />
          </div>
          {/* 删除按钮 */}
          <div className="absolute -top-2 -right-2">
            <button
              type="button"
              onClick={onRemove}
              className="flex items-center justify-center size-5 rounded-full bg-error text-white hover:bg-error/80"
            >
              <Cross2Icon className="size-3" />
            </button>
          </div>
        </div>
      ) : (
        <button
          type="button"
          onClick={onSelect}
          className="cursor-pointer flex w-40 h-30 flex-col items-center justify-center rounded border border-dashed border-border bg-surface-secondary transition-colors hover:border-primary hover:bg-primary/5 active:bg-primary/10"
        >
          {/* 加号图标 */}
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-primary">
            <path d="M12 5V19M5 12H19" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        </button>
      )}
      <span className="text-base text-text-secondary">{label}</span>
    </div>
  );
}

export function DocumentUpload({
  initialData,
  onSubmit,
  onBack,
  isLoading,
}: DocumentUploadProps) {
  const t = useTranslations('verification');
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const sectionRefs = useRef<Record<DocumentSubStep, HTMLDivElement | null>>({
    documents: null,
    address: null,
  });
  
  const [activeStep, setActiveStep] = useState<DocumentSubStep>('documents');
  const [error, setError] = useState<string | null>(null);
  
  const fileInputRefs = useRef<Record<DocumentType, HTMLInputElement | null>>({
    id_front: null,
    id_back: null,
    address: null,
  });

  // 只存储选择的文件，不立即上传
  const [selectedFiles, setSelectedFiles] = useState<Record<DocumentType, SelectedFile | null>>(() => {
    const initial: Record<DocumentType, SelectedFile | null> = {
      id_front: null,
      id_back: null,
      address: null,
    };
    
    // 如果有初始数据（已上传的文件），显示预览
    if (initialData) {
      initialData.forEach((doc) => {
        if (doc.type && doc.url) {
          initial[doc.type as DocumentType] = {
            file: new File([], doc.fileName || 'uploaded'),
            preview: doc.url,
          };
        }
      });
    }
    
    return initial;
  });

  // 滚动同步高亮
  useEffect(() => {
    const scrollContainer = scrollContainerRef.current;
    if (!scrollContainer) return;
    
    const handleScroll = () => {
      const containerTop = scrollContainer.scrollTop;
      const steps: DocumentSubStep[] = ['documents', 'address'];
      let activeSubStep: DocumentSubStep = 'documents';
      
      steps.forEach((step) => {
        const element = sectionRefs.current[step];
        if (!element) return;
        
        const elementTop = element.offsetTop - scrollContainer.offsetTop;
        if (elementTop <= containerTop + 100) {
          activeSubStep = step;
        }
      });
      
      setActiveStep(activeSubStep);
    };
    
    handleScroll();
    scrollContainer.addEventListener('scroll', handleScroll, { passive: true });
    
    return () => {
      scrollContainer.removeEventListener('scroll', handleScroll);
    };
  }, []);

  // 点击导航滚动
  const handleStepClick = useCallback((step: DocumentSubStep) => {
    const element = sectionRefs.current[step];
    const container = scrollContainerRef.current;
    if (element && container) {
      const elementTop = element.offsetTop - container.offsetTop;
      container.scrollTo({ top: elementTop, behavior: 'smooth' });
    }
  }, []);

  // 选择文件（不立即上传）
  const handleFileSelect = (type: DocumentType, file: File) => {
    setError(null);
    
    // 验证文件类型
    if (!file.type.startsWith('image/') && file.type !== 'application/pdf') {
      setError(t('errors.invalidFileType'));
      return;
    }

    // 验证文件大小 (最大 20MB)
    if (file.size > 20 * 1024 * 1024) {
      setError(t('errors.fileTooLarge'));
      return;
    }

    // 创建预览
    const preview = URL.createObjectURL(file);
    setSelectedFiles((prev) => ({
      ...prev,
      [type]: { file, preview },
    }));
  };

  // 移除文件
  const handleRemove = (type: DocumentType) => {
    const current = selectedFiles[type];
    if (current?.preview && !current.preview.startsWith('http')) {
      URL.revokeObjectURL(current.preview);
    }
    setSelectedFiles((prev) => ({
      ...prev,
      [type]: null,
    }));
  };

  // 提交 - 将文件传递给父组件处理上传
  const handleSubmit = () => {
    const files: Record<DocumentType, File | null> = {
      id_front: selectedFiles.id_front?.file || null,
      id_back: selectedFiles.id_back?.file || null,
      address: selectedFiles.address?.file || null,
    };
    
    onSubmit(files);
  };

  // 检查是否所有必需文件已选择
  const allSelected = selectedFiles.id_front && 
                      selectedFiles.id_back && 
                      selectedFiles.address;

  // 左侧导航
  const stepNav = (
    <DocumentStepNav activeStep={activeStep} onStepClick={handleStepClick} />
  );

  return (
    <VerificationFormLayout
      stepNav={stepNav}
      onBack={onBack}
      isLoading={isLoading}
      isForm={false}
      onSubmit={handleSubmit}
      submitDisabled={!allSelected}
      scrollContainerRef={scrollContainerRef}
    >
      <div className="flex flex-col gap-10 md:gap-10">
        {/* 错误提示 */}
        {error && (
          <div className="p-3 rounded bg-error/10 text-error text-sm">
            {error}
          </div>
        )}

        {/* 01 文件区域 */}
        <div
          ref={(el) => { sectionRefs.current.documents = el; }}
          className="flex flex-col gap-10"
        >
          <SectionTitle>{t('documentSteps.documentsNav')}</SectionTitle>
          
          <div className="flex flex-col gap-5">
            {/* 副标题 */}
            <p className="text-base text-text-secondary">
              {t('documentUpload.idDescription')}
            </p>
            
            {/* 说明列表 */}
            <ul className="flex flex-col gap-5 text-base text-text-secondary list-disc list-inside">
              <li>{t('documentUpload.requirement1')}</li>
              <li>{t('documentUpload.requirement2')}</li>
              <li>{t('documentUpload.requirement3')}</li>
              <li>{t('documentUpload.requirement4')}</li>
            </ul>
            
            {/* 上传框 */}
            <div className="flex flex-wrap gap-10 mt-2.5">
              <UploadBox
                label={t('documentUpload.idFront')}
                selected={selectedFiles.id_front}
                onSelect={() => fileInputRefs.current.id_front?.click()}
                onRemove={() => handleRemove('id_front')}
              />
              <UploadBox
                label={t('documentUpload.idBack')}
                selected={selectedFiles.id_back}
                onSelect={() => fileInputRefs.current.id_back?.click()}
                onRemove={() => handleRemove('id_back')}
              />
            </div>
          </div>
        </div>

        {/* 02 地址证明区域 */}
        <div
          ref={(el) => { sectionRefs.current.address = el; }}
          className="flex flex-col gap-10"
        >
          <SectionTitle>{t('documentSteps.addressProofNav')}</SectionTitle>
          
          <div className="flex flex-col gap-5">
            {/* 说明列表 */}
            <ul className="flex flex-col gap-5 text-base text-text-secondary list-disc list-inside">
              <li>{t('documentUpload.addressRequirement1')}</li>
              <li>{t('documentUpload.addressRequirement2')}</li>
            </ul>
            
            {/* 上传框 */}
            <div className="flex flex-col gap-2.5 items-start">
              <UploadBox
                label={t('documentUpload.addressFile')}
                selected={selectedFiles.address}
                onSelect={() => fileInputRefs.current.address?.click()}
                onRemove={() => handleRemove('address')}
              />
              
              {/* 提示文字 */}
              <p className="text-sm text-primary mt-2">
                {t('documentUpload.uploadLaterNote')}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* 隐藏的文件输入 */}
      <input
        ref={(el) => { fileInputRefs.current.id_front = el; }}
        type="file"
        accept="image/*,.pdf"
        className="hidden"
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) handleFileSelect('id_front', file);
          e.target.value = '';
        }}
      />
      <input
        ref={(el) => { fileInputRefs.current.id_back = el; }}
        type="file"
        accept="image/*,.pdf"
        className="hidden"
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) handleFileSelect('id_back', file);
          e.target.value = '';
        }}
      />
      <input
        ref={(el) => { fileInputRefs.current.address = el; }}
        type="file"
        accept="image/*,.pdf"
        className="hidden"
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) handleFileSelect('address', file);
          e.target.value = '';
        }}
      />
    </VerificationFormLayout>
  );
}
