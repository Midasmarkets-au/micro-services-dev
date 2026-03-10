'use client';

import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui';
import type { RefObject, ReactNode } from 'react';

interface VerificationFormLayoutProps {
  /** 左侧导航内容 - 由各页面自定义 */
  stepNav: ReactNode;
  /** 右侧表单内容 */
  children: ReactNode;
  /** 返回按钮回调 */
  onBack: () => void;
  /** 表单提交（通过 form 的 onSubmit 处理） */
  isLoading?: boolean;
  /** 提交按钮文字，默认"下一步" */
  submitLabel?: string;
  /** 加载中按钮文字 */
  loadingLabel?: string;
  /** 滚动容器 ref（用于滚动同步高亮） */
  scrollContainerRef?: RefObject<HTMLDivElement | null>;
  /** 是否为 form 表单（默认 true） */
  isForm?: boolean;
  /** 非表单模式下的提交回调 */
  onSubmit?: () => void;
  /** 提交按钮是否禁用 */
  submitDisabled?: boolean;
}

export function VerificationFormLayout({
  stepNav,
  children,
  onBack,
  isLoading = false,
  submitLabel,
  loadingLabel,
  scrollContainerRef,
  isForm = true,
  onSubmit,
  submitDisabled = false,
}: VerificationFormLayoutProps) {
  const t = useTranslations('verification');

  const finalSubmitLabel = submitLabel || t('nextStep');
  const finalLoadingLabel = loadingLabel || t('saving');

  // 操作按钮区域
  const actionButtons = (
    <div className="flex flex-col-reverse sm:flex-row justify-end gap-3 sm:gap-5 pt-4 md:pt-6">
      <Button
        type="button"
        variant="outline"
        onClick={onBack}
        className="w-full sm:w-30 h-10 md:h-12"
      >
        {t('previousStep')}
      </Button>
      <Button
        type={isForm ? 'submit' : 'button'}
        onClick={isForm ? undefined : onSubmit}
        disabled={isLoading || submitDisabled}
        className="w-full sm:w-30 h-10 md:h-12"
      >
        {isLoading ? finalLoadingLabel : finalSubmitLabel}
      </Button>
    </div>
  );

  // 内容区域
  const contentArea = (
    <div
      ref={scrollContainerRef}
      className="flex flex-1 flex-col gap-10 md:gap-15 md:max-h-[calc(100vh-200px)] md:overflow-y-auto md:pr-4 scroll-smooth"
    >
      {children}
      {actionButtons}
    </div>
  );

  return (
    <div className="flex flex-col md:flex-row gap-6 md:gap-12 lg:gap-50">
      {/* 左侧账户申请标题和步骤导航 - 桌面端显示 */}
      <div className="flex flex-col gap-6 md:gap-10 md:sticky md:top-5 md:self-start">
        <h2 className="font-semibold text-xl text-text-primary">
          {t('accountApplication')}
        </h2>
        {stepNav}
      </div>

      {/* 右侧表单内容 */}
      {contentArea}
    </div>
  );
}

/**
 * 单步骤导航组件 - 用于只有一个步骤的页面（如协议、财务信息、文件上传）
 */
interface SingleStepNavProps {
  /** 步骤序号，如 "01" */
  stepNumber: string;
  /** 步骤标签 */
  label: string;
}

export function SingleStepNav({ stepNumber, label }: SingleStepNavProps) {
  return (
    <div className="hidden md:flex items-center gap-6 md:gap-8">
      {/* 左侧竖线指示器 */}
      <div className="relative h-[50px] w-1">
        <div className="absolute inset-0 bg-text-primary" />
        <div className="absolute inset-0 bg-primary" />
      </div>
      {/* 步骤内容 */}
      <div className="flex items-center gap-6 md:gap-7">
        <span className="font-bold text-base text-text-secondary font-din">{stepNumber}</span>
        <span className="font-semibold text-base text-primary">{label}</span>
      </div>
    </div>
  );
}
