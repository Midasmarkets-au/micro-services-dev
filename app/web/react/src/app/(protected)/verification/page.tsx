'use client';

import { useState, useEffect, useRef, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import {
  VerificationBanner,
  VerificationStepper,
  StartedInfoForm,
  PersonalInfoForm,
  AgreementForm,
  DocumentUpload,
  VerificationComplete,
  type MainStep,
} from '@/components/verification';
import type {
  VerificationData,
  InfoData,
  AgreementData,
  DocumentType,
} from '@/types/verification';
import type { StartedSubmitData } from '@/components/verification/StartedInfoForm';
import { useServerAction } from '@/hooks/useServerAction';
import { useApiClient } from '@/hooks/useApiClient';
import { useToast } from '@/hooks/useToast';
import { PageLoading } from '@/components/ui';
import type { SelectOption } from '@/components/ui';
import { useUserStore } from '@/stores/userStore';
import {
  getVerificationStatus,
  saveStartedInfo,
  checkClientAnswer,
  getMyReferralCode,
  getReferralInfoByReferralCode,
  getConfiguration,
  savePersonalInfo,
  saveAgreement,
  submitDocument,
  logout,
} from '@/actions';
import {
  prepareChunkedUpload,
  createChunkFormData,
  createMergeFormData,
} from '@/lib/utils/fileUpload';
import { accountTypeOptions, currencyOptions, leverageOptions, platformOptions } from '@/types/verification';
import { AccountRoleTypes, getPlatformName } from '@/types/accounts';
import { getTenancy, Tenancies } from '@/core/types/TenantTypes';
import { useCurrencyName } from '@/i18n/useCurrencyName';

export default function VerificationPage() {
  const t = useTranslations('verification');
  const { execute } = useServerAction();
  const { upload } = useApiClient();
  const { showWarning, showError } = useToast();
  const router = useRouter();
  const user = useUserStore((s) => s.user);
  const siteConfig = useUserStore((s) => s.siteConfig);
  const tAccounts = useTranslations('accounts');
  const getCurrencyName = useCurrencyName();
  const [currentStep, setCurrentStep] = useState<MainStep>('started');
  const [completedSteps, setCompletedSteps] = useState<MainStep[]>([]);
  const [verificationData, setVerificationData] = useState<VerificationData | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isInitialLoading, setIsInitialLoading] = useState(true);
  const [referralCode, setReferralCode] = useState('');
  const [showIbDocuments, setShowIbDocuments] = useState(false);
  const [quizEnabled, setQuizEnabled] = useState(false);
  const [accountTypeSelections, setAccountTypeSelections] = useState<SelectOption[]>([]);
  const [isAccountTypeRestricted, setIsAccountTypeRestricted] = useState(false);

  const isInitialized = useRef(false);
  const tenancy = getTenancy();

  const defaultAccountTypeOptions = useMemo<SelectOption[]>(() => {
    const available = siteConfig?.accountTypeAvailable ?? [];
    if (available.length > 0) {
      return available.map((id) => ({
        value: String(id),
        label: tAccounts(`accountTypes.${id}`),
      }));
    }
    return accountTypeOptions.map((item) => ({
      value: String(item.value),
      label: item.label,
    }));
  }, [siteConfig?.accountTypeAvailable, tAccounts]);

  const defaultCurrencyOptions = useMemo<SelectOption[]>(() => {
    const available = siteConfig?.currencyAvailable ?? [];
    if (available.length > 0) {
      return available.map((id) => ({
        value: String(id),
        label: getCurrencyName(id),
      }));
    }
    return currencyOptions.map((item) => ({
      value: String(item.value),
      label: item.label,
    }));
  }, [getCurrencyName, siteConfig?.currencyAvailable]);

  const defaultLeverageOptions = useMemo<SelectOption[]>(() => {
    const available = siteConfig?.leverageAvailable ?? [];
    if (available.length > 0) {
      return available.map((lev) => ({
        value: String(lev),
        label: `${lev}:1`,
      }));
    }
    return leverageOptions.map((item) => ({
      value: String(item.value),
      label: item.label,
    }));
  }, [siteConfig?.leverageAvailable]);

  const defaultPlatformOptions = useMemo<SelectOption[]>(() => {
    const available = siteConfig?.tradingPlatformAvailable ?? [];
    if (available.length > 0) {
      return available.map((id) => ({
        value: String(id),
        label: getPlatformName(id),
      }));
    }
    return platformOptions.map((item) => ({
      value: String(item.value),
      label: item.label,
    }));
  }, [siteConfig?.tradingPlatformAvailable]);

  useEffect(() => {
    if (!isAccountTypeRestricted) {
      setAccountTypeSelections(defaultAccountTypeOptions);
    }
  }, [defaultAccountTypeOptions, isAccountTypeRestricted]);

  const addCompletedStep = (step: MainStep) => {
    setCompletedSteps((prev) => (prev.includes(step) ? prev : [...prev, step]));
  };

  const getStepOrder = (): MainStep[] => ['started', 'info', 'agreement', 'document'];

  const isStepDone = (data: VerificationData, step: MainStep) => {
    switch (step) {
      case 'started':
        return !!data.started;
      case 'info':
        return !!data.info;
      case 'agreement':
        return !!data.agreement;
      case 'document':
        return !!data.document && data.document.length > 0;
      default:
        return false;
    }
  };

  const resolveCurrentStep = (data: VerificationData): MainStep => {
    if (data.status !== 0) {
      return 'complete';
    }

    const stepOrder = getStepOrder();
    for (const step of stepOrder) {
      if (!isStepDone(data, step)) {
        return step;
      }
    }
    return 'complete';
  };

  const resolveCompletedSteps = (data: VerificationData): MainStep[] => {
    const stepOrder = getStepOrder();
    return stepOrder.filter((step) => isStepDone(data, step));
  };

  const fetchReferralData = async () => {
    const referralCodeResult = await execute(getMyReferralCode);
    if (!referralCodeResult.success || !referralCodeResult.data?.referCode) {
      setIsAccountTypeRestricted(false);
      setAccountTypeSelections(defaultAccountTypeOptions);
      return;
    }

    const code = referralCodeResult.data.referCode;
    setReferralCode(code);

    const referralInfoResult = await execute(getReferralInfoByReferralCode, code);
    if (!referralInfoResult.success || !referralInfoResult.data) {
      setIsAccountTypeRestricted(false);
      setAccountTypeSelections(defaultAccountTypeOptions);
      return;
    }

    const referralData = referralInfoResult.data.data || referralInfoResult.data;
    const serviceType = referralData.serviceType || 0;
    setShowIbDocuments(
      serviceType === AccountRoleTypes.IB || serviceType === 200
    );

    const allowAccountTypes = referralData.summary?.allowAccountTypes || [];
    const schemaAccountTypes = referralData.summary?.schema || [];
    const sourceList = allowAccountTypes.length > 0 ? allowAccountTypes : schemaAccountTypes;

    if (sourceList.length > 0) {
      setIsAccountTypeRestricted(true);
      setAccountTypeSelections(
        sourceList.map((item) => ({
          value: String(item.accountType),
          label: tAccounts(`accountTypes.${item.accountType}`),
        }))
      );
      return;
    }

    setIsAccountTypeRestricted(false);
    setAccountTypeSelections(defaultAccountTypeOptions);
  };

  useEffect(() => {
    if (isInitialized.current) return;
    isInitialized.current = true;

    const initializePage = async () => {
      const configResult = await execute(getConfiguration);
      if (configResult.success) {
        setQuizEnabled(Boolean(configResult.data?.verificationQuizEnabled));
      }

      await fetchReferralData();

      const verificationResult = await execute(getVerificationStatus);
      if (verificationResult.success && verificationResult.data) {
        const data = verificationResult.data as unknown as VerificationData;
        setVerificationData(data);
        setCompletedSteps(resolveCompletedSteps(data));
        setCurrentStep(resolveCurrentStep(data));
      }

      setIsInitialLoading(false);
    };

    initializePage();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleStartedSubmit = async (data: StartedSubmitData) => {
    setIsLoading(true);

    const hasWrongAnswer =
      quizEnabled &&
      Object.values(data.questions || {}).some((item) => item === false);

    if (hasWrongAnswer) {
      await execute(checkClientAnswer, {
        quiz1: data.questions?.q1,
        quiz2: data.questions?.q2,
        quiz3: data.questions?.q3,
        quiz4: data.questions?.q4,
        answerw: 4,
      });

      showWarning(t('started.cantProcess'));
      await execute(logout);
      router.push('/sign-in');
      setIsLoading(false);
      return;
    }

    const payload = {
      ...data,
      platform: data.serviceId === 30 ? 30 : 20,
      referral: referralCode || undefined,
    };

    const result = await execute(saveStartedInfo, payload);
    if (result.success) {
      addCompletedStep('started');
      setCurrentStep('info');
      setVerificationData((prev) => (prev ? { ...prev, started: payload } : prev));
    }

    setIsLoading(false);
  };

  const handleInfoSubmit = async (data: Partial<InfoData>) => {
    setIsLoading(true);

    const socialMedium = [
      { name: 'whatsApp', account: (data as Record<string, string>).whatsApp || '' },
      { name: 'weChat', account: (data as Record<string, string>).weChat || '' },
      { name: 'instagram', account: (data as Record<string, string>).instagram || '' },
      { name: 'telegram', account: (data as Record<string, string>).telegram || '' },
      { name: 'line', account: (data as Record<string, string>).line || '' },
    ];

    const result = await execute(savePersonalInfo, { ...data, socialMedium });

    if (result.success) {
      addCompletedStep('info');
      setCurrentStep('agreement');
      setVerificationData((prev) => (prev ? { ...prev, info: { ...data, socialMedium } as InfoData } : prev));
    }
    setIsLoading(false);
  };

  const handleAgreementSubmit = async (data: Partial<AgreementData>) => {
    if (tenancy === Tenancies.sea && data.consent_2 === false) {
      showError('electronicIdentityVerificationRequired');
      return;
    }

    setIsLoading(true);
    const result = await execute(saveAgreement, data);

    if (result.success) {
      addCompletedStep('agreement');
      setCurrentStep('document');
      setVerificationData((prev) => (prev ? { ...prev, agreement: data as AgreementData } : prev));
    }
    setIsLoading(false);
  };

  const uploadFileWithChunks = async (
    file: File,
    type: DocumentType
  ): Promise<{ guid?: string; data?: unknown }> => {
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

  const handleDocumentSubmit = async (files: Record<DocumentType, File | null>) => {
    setIsLoading(true);

    try {
      const media: Record<string, unknown>[] = [];

      for (const [type, file] of Object.entries(files)) {
        if (!file) continue;

        const result = await uploadFileWithChunks(file, type as DocumentType);
        const docData = (result as Record<string, unknown>).data || result;
        media.push(docData as Record<string, unknown>);
      }

      const result = await execute(submitDocument, { media });

      if (result.success) {
        addCompletedStep('document');
        setCurrentStep('complete');
      }
    } catch (error) {
      console.error('Document upload error:', error);
    }
    setIsLoading(false);
  };

  const handleBack = () => {
    const stepOrder: MainStep[] = getStepOrder();
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
      <VerificationBanner />

      <VerificationStepper
        currentStep={currentStep}
        completedSteps={
          currentStep === 'complete'
            ? ['started', 'info', 'agreement', 'document']
            : completedSteps
        }
      />

      <div className="rounded bg-surface p-3 md:p-5">
        {currentStep === 'started' && (
          <StartedInfoForm
            initialData={verificationData?.started}
            accountTypeOptions={accountTypeSelections}
            currencyOptions={defaultCurrencyOptions}
            leverageOptions={defaultLeverageOptions}
            platformOptions={defaultPlatformOptions}
            onSubmit={handleStartedSubmit}
            isLoading={isLoading}
          />
        )}

        {currentStep === 'info' && (
          <PersonalInfoForm
            initialData={verificationData?.info}
            onSubmit={handleInfoSubmit}
            onBack={handleBack}
            isLoading={isLoading}
          />
        )}

        {currentStep === 'agreement' && (
          <AgreementForm
            initialData={verificationData?.agreement}
            onSubmit={handleAgreementSubmit}
            onBack={handleBack}
            isLoading={isLoading}
            showIbDocuments={showIbDocuments}
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
