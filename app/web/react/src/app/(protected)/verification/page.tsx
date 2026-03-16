'use client';

import { useState, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import {
  VerificationBanner,
  VerificationStepper,
  PersonalInfoForm,
  FinancialInfoForm,
  AgreementForm,
  DocumentUpload,
  VerificationComplete,
  type MainStep,
} from '@/components/verification';
import type {
  VerificationData,
  InfoData,
  FinancialData,
  AgreementData,
  DocumentType,
} from '@/types/verification';
import { useServerAction } from '@/hooks/useServerAction';
import { useApiClient } from '@/hooks/useApiClient';
import { PageLoading } from '@/components/ui';
import { useUserStore } from '@/stores/userStore';
import {
  getVerificationStatus,
  savePersonalInfo,
  saveFinancialInfo,
  saveAgreement,
  submitDocument,
} from '@/actions';
import {
  prepareChunkedUpload,
  createChunkFormData,
  createMergeFormData,
} from '@/lib/utils/fileUpload';

export default function VerificationPage() {
  useTranslations('verification'); // 预加载翻译
  const { execute } = useServerAction();
  const { upload } = useApiClient(); // 保留用于文件上传
  const user = useUserStore((s) => s.user);
  const [currentStep, setCurrentStep] = useState<MainStep>('info');
  const [completedSteps, setCompletedSteps] = useState<MainStep[]>([]);
  const [verificationData, setVerificationData] = useState<VerificationData | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isInitialLoading, setIsInitialLoading] = useState(true);
  
  // 防止重复请求
  const isInitialized = useRef(false);

  // 获取验证状态（只在首次加载时执行）
  useEffect(() => {
    if (isInitialized.current) return;
    isInitialized.current = true;
    
    const fetchVerificationStatus = async () => {
      // 使用 Server Action
      const result = await execute(getVerificationStatus);
      
      if (result.success && result.data) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const data = result.data as any as VerificationData;
        setVerificationData(data);
        
        // 根据已有数据设置已完成的步骤
        const completed: MainStep[] = [];
        if (data?.info) completed.push('info');
        // if (data?.financial) completed.push('financial');
        if (data?.agreement) completed.push('agreement');
        if (data?.document && data.document.length > 0) completed.push('document');
        
        setCompletedSteps(completed);
        
        // 设置当前步骤
        if (data?.status === 2 || data?.status === 3) {
          setCurrentStep('complete');
        } else if (!data?.info) {
          setCurrentStep('info');
        // } else if (!data?.financial) {
        //   setCurrentStep('financial');
        // }
        }else if (!data?.agreement) {
          setCurrentStep('agreement');
        } else if (!data?.document || data.document.length === 0) {
          setCurrentStep('document');
        } else {
          setCurrentStep('complete');
        }
      }
      
      setIsInitialLoading(false);
    };
    
    fetchVerificationStatus();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // 保存个人信息
  const handleInfoSubmit = async (data: Partial<InfoData>) => {
    setIsLoading(true);
    
    // 转换社交媒体数据
    const socialMedium = [
      { name: 'whatsApp', account: (data as Record<string, string>).whatsApp || '' },
      { name: 'weChat', account: (data as Record<string, string>).weChat || '' },
      { name: 'instagram', account: (data as Record<string, string>).instagram || '' },
      { name: 'telegram', account: (data as Record<string, string>).telegram || '' },
      { name: 'line', account: (data as Record<string, string>).line || '' },
    ];

    // 使用 Server Action
    const result = await execute(savePersonalInfo, { ...data, socialMedium });

    if (result.success) {
      setCompletedSteps((prev) => [...prev, 'info']);
      setCurrentStep('financial');
    }
    
    setIsLoading(false);
  };

  // 保存财务信息
  const handleFinancialSubmit = async (data: Partial<FinancialData>) => {
    setIsLoading(true);
    
    // 使用 Server Action
    const result = await execute(saveFinancialInfo, data);

    if (result.success) {
      setCompletedSteps((prev) => [...prev, 'financial']);
      setCurrentStep('agreement');
    }
    
    setIsLoading(false);
  };

  // 保存协议信息
  const handleAgreementSubmit = async (data: Partial<AgreementData>) => {
    setIsLoading(true);
    
    // 使用 Server Action
    const result = await execute(saveAgreement, data);

    if (result.success) {
      setCompletedSteps((prev) => [...prev, 'agreement']);
      setCurrentStep('document');
    }
    
    setIsLoading(false);
  };

  // 切片上传单个文件（保留使用 useApiClient 的 upload）
  const uploadFileWithChunks = async (file: File, type: DocumentType): Promise<{ guid?: string; data?: unknown }> => {
    const userId = user?.uid?.toString();
    const chunks = prepareChunkedUpload(file, type, userId);
    
    if (chunks.length === 0) {
      throw new Error('No chunks to upload');
    }

    const { fieldId, fileName, contentType, totalChunks } = chunks[0];

    // 上传所有切片
    for (const chunkInfo of chunks) {
      const formData = createChunkFormData(chunkInfo);
      const result = await upload('/api/verification/upload/chunk', formData, {
        showErrorToast: false,
      });
      
      if (!result.success) {
        throw new Error(`Chunk ${chunkInfo.chunkIndex} upload failed`);
      }
    }

    // 合并文件
    const mergeFormData = createMergeFormData(
      fieldId,
      fileName,
      contentType,
      type,
      totalChunks
    );

    const mergeResult = await upload<{ guid?: string; data?: unknown }>(
      '/api/verification/upload/merge',
      mergeFormData,
      { showErrorToast: false }
    );

    if (!mergeResult.success || !mergeResult.data) {
      throw new Error('File merge failed');
    }

    return mergeResult.data;
  };

  // 提交文档 - 接收选择的文件，统一切片上传
  const handleDocumentSubmit = async (files: Record<DocumentType, File | null>) => {
    setIsLoading(true);
    
    try {
      const media: Record<string, unknown>[] = [];

      // 遍历所有文件，依次切片上传
      for (const [type, file] of Object.entries(files)) {
        if (!file) continue;
        
        const result = await uploadFileWithChunks(file, type as DocumentType);
        // merge API 返回 { data: { id, hashId, guid, ... } }，需要解开一层
        const docData = (result as Record<string, unknown>).data || result;
        media.push(docData as Record<string, unknown>);
      }

      // API 期望扁平数组: [{ id, hashId, guid, type, ... }, ...]
      const result = await execute(submitDocument, { media });

      if (result.success) {
        setCompletedSteps((prev) => [...prev, 'document']);
        setCurrentStep('complete');
      }
    } catch (error) {
      console.error('Document upload error:', error);
    }
    
    setIsLoading(false);
  };

  // 返回上一步
  const handleBack = () => {
    const stepOrder: MainStep[] = ['info', 'financial', 'agreement', 'document'];
    const currentIndex = stepOrder.indexOf(currentStep);
    if (currentIndex > 0) {
      setCurrentStep(stepOrder[currentIndex - 1]);
    }
  };

  // 获取验证状态类型
  const getCompletionStatus = (): 'pending' | 'approved' | 'rejected' => {
    if (!verificationData) return 'pending';
    switch (verificationData.status) {
      case 2:
        return 'pending';
      case 3:
        return 'approved';
      default:
        return 'pending';
    }
  };

  if (isInitialLoading) {
    return <PageLoading fullscreen={false} className="py-20" />;
  }

  return (
    <div className="flex w-full flex-col gap-3 md:gap-5">
      {/* Banner */}
      <VerificationBanner />

      {/* 步骤指示器 - 完成页面也显示 */}
      <VerificationStepper
        currentStep={currentStep}
        completedSteps={currentStep === 'complete' ? ['info', 'financial', 'agreement', 'document', 'complete'] : completedSteps}
      />

      {/* 表单内容 */}
      <div className="rounded bg-surface p-3 md:p-5">
        {currentStep === 'info' && (
          <PersonalInfoForm
            initialData={verificationData?.info}
            onSubmit={handleInfoSubmit}
            onBack={handleBack}
            isLoading={isLoading}
          />
        )}

        {/* {currentStep === 'financial' && (
          <FinancialInfoForm
            initialData={verificationData?.financial}
            onSubmit={handleFinancialSubmit}
            onBack={handleBack}
            isLoading={isLoading}
          />
        )} */}

        {currentStep === 'agreement' && (
          <AgreementForm
            initialData={verificationData?.agreement}
            onSubmit={handleAgreementSubmit}
            onBack={handleBack}
            isLoading={isLoading}
          />
        )}

        {currentStep === 'document' && (
          <DocumentUpload
            initialData={verificationData?.document}
            onSubmit={handleDocumentSubmit}
            onBack={handleBack}
            isLoading={isLoading}
          />
        )}

        {currentStep === 'complete' && (
          <VerificationComplete status={getCompletionStatus()} />
        )}
      </div>
    </div>
  );
}
