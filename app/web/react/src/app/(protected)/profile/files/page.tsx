'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useUserStore } from '@/stores/userStore';
import { isGuestOnly } from '@/lib/rbac';
import { getVerificationStatus, uploadVerificationDocument } from '@/actions';
import { useTheme } from '@/hooks/useTheme';
import { useToast } from '@/hooks/useToast';
import { FileCard } from '@/components/ui/FileCard';
import { 
  Skeleton, 
  Button, 
  FilePreviewModal, 
  UploadFileModal, 
  VerificationDocumentTypes 
} from '@/components/ui';

// 文档类型
interface DocumentMedia {
  fileName: string;
  contentType?: string;
  status?: number;
  rejectedReason?: string;
  documentType?: number;
  url?: string;
  guid?: string;
}

// 验证 Banner 组件 - 仅在 Guest 用户时渲染
function VerificationBanner() {
  const t = useTranslations('profile.files');
  const { theme } = useTheme();

  return (
    <div className="relative h-[180px] md:h-[200px] lg:h-[240px] rounded overflow-hidden verification-banner">
      {/* 背景层 */}
      <div className="absolute inset-0 verification-banner-bg" />
      
      {/* 内容层 */}
      <div className="relative z-10 h-full flex items-center px-[clamp(20px,4.34vw,70px)]">
        <div className="flex-1 max-w-[800px]">
          <h2 className="text-responsive-2xl font-semibold text-white mb-2">
            {t('banner.title')}
          </h2>
          <p className="text-xl font-semibold text-white mb-6">
            {t('banner.subtitle')}
          </p>
          <Link
            href="/verification"
            className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded backdrop-blur-[8.55px] border border-white text-white text-xs font-semibold transition-opacity hover:opacity-90"
            style={{ backgroundColor: 'var(--color-primary)' }}
          >
            <span>{t('banner.startNow')}</span>
            <svg width="7" height="7" viewBox="0 0 7 7" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1 6L6 1M6 1H1M6 1V6" stroke="white" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </Link>
        </div>
        {/* 右侧 3D 图标 */}
        <div className="hidden md:block w-[180px] lg:w-[240px] h-[180px] lg:h-[240px] shrink-0">
          <Image
            src={theme === 'dark' ? '/images/verification/verify-night.svg' : '/images/verification/verify-day.svg'}
            alt="Verification"
            width={260}
            height={260}
            className="object-contain w-full h-full"
            priority
          />
        </div>
      </div>
    </div>
  );
}

// 上传按钮组件
function UploadButton({ 
  disabled, 
  onClick 
}: { 
  disabled: boolean; 
  onClick: () => void; 
}) {
  const t = useTranslations('profile.files');

  return (
    <Button
      onClick={onClick}
      disabled={disabled}
      size="xs"
      className="w-auto"
    >
      <Image src="/images/icons/file-upload.svg"  alt="Upload file" width={16} height={16} />
      <span>{t('uploadFile')}</span>
    </Button>
  );
}

// 空状态组件
function EmptyState() {
  const t = useTranslations('profile.files');
  const { theme } = useTheme();

  return (
    <div className="flex flex-col items-center justify-center py-20">
      <Image
        src={theme === 'dark' ? '/images/data/no-data-night.svg' : '/images/data/no-data-day.svg'}
        alt="No data"
        width={120}
        height={120}
        className="mb-4 opacity-50"
      />
      <p className="text-text-secondary">{t('noFiles')}</p>
    </div>
  );
}

// 加载骨架屏
function LoadingSkeleton() {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
      {Array.from({ length: 4 }).map((_, index) => (
        <div key={index} className="flex items-center gap-5 p-10 bg-surface border border-border rounded-[12px]">
          <Skeleton className="shrink-0 size-10 rounded" />
          <Skeleton className="flex-1 h-4" />
          <Skeleton className="shrink-0 w-20 h-8 rounded" />
        </div>
      ))}
    </div>
  );
}

export default function FilesPage() {
  const t = useTranslations('profile.files');
  const tCommon = useTranslations('common');
  const { user } = useUserStore();
  const { showToast } = useToast();
  
  const [documents, setDocuments] = useState<DocumentMedia[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  // 文件预览弹窗状态
  const [previewOpen, setPreviewOpen] = useState(false);
  const [selectedFile, setSelectedFile] = useState<DocumentMedia | null>(null);
  
  // 文件上传弹窗状态
  const [uploadModalOpen, setUploadModalOpen] = useState(false);

  // 检查是否是 Guest 用户
  const isGuest = isGuestOnly(user?.roles ?? []);
  // 获取文档数据
  useEffect(() => {
    const fetchDocuments = async () => {
      if (isGuest) {
        setIsLoading(false);
        return;
      }

      try {
        const result = await getVerificationStatus();
        console.log('[FilesPage] result:', result);
        if (result.success && result.data) {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const verificationData = result.data as any;
          setDocuments(verificationData.document ?? []);
        }
      } catch (error) {
        console.error('[FilesPage] Failed to fetch documents:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchDocuments();
  }, [isGuest]);

  // 处理文件点击 - 打开预览弹窗
  const handleFileClick = (media: DocumentMedia) => {
    setSelectedFile(media);
    setPreviewOpen(true);
  };

  // 关闭预览弹窗
  const handleClosePreview = () => {
    setPreviewOpen(false);
    setSelectedFile(null);
  };

  // 打开上传弹窗
  const handleOpenUploadModal = () => {
    setUploadModalOpen(true);
  };

  // 关闭上传弹窗
  const handleCloseUploadModal = () => {
    setUploadModalOpen(false);
  };

  // 处理文件上传
  const handleUpload = async (file: File, documentType: VerificationDocumentTypes) => {
    try {
      const result = await uploadVerificationDocument(file, documentType);
      
      if (result.success) {
        showToast({
          type: 'success',
          message: t('uploadSuccess'),
        });
        
        // 重新获取文档列表
        const statusResult = await getVerificationStatus();
        if (statusResult.success && statusResult.data) {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const verificationData = statusResult.data as any;
          setDocuments(verificationData.document ?? []);
        }
      } else {
        showToast({
          type: 'error',
          message: result.error || tCommon('error'),
        });
      }
    } catch (error) {
      console.error('[FilesPage] Upload failed:', error);
      showToast({
        type: 'error',
        message: tCommon('error'),
      });
    }
  };

  return (
    <div className="flex flex-col gap-5 py-5">
      {/* 标题栏 */}
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-text-primary">
          {t('additionalDocuments')}
        </h2>
        <UploadButton disabled={isGuest} onClick={handleOpenUploadModal} />
      </div>

      {/* 验证 Banner - 仅 Guest 用户显示 */}
      {isGuest && <VerificationBanner />}

      {/* 文件列表区域 */}
      {!isGuest && (
        <div className="mt-5">
          {isLoading ? (
            <LoadingSkeleton />
          ) : documents.length === 0 ? (
            <EmptyState />
          ) : (
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
              {documents.map((doc, index) => (
                <FileCard
                  key={doc.guid || index}
                  media={doc}
                  onClick={handleFileClick}
                />
              ))}
            </div>
          )}
        </div>
      )}

      {/* 文件预览弹窗 */}
      <FilePreviewModal
        isOpen={previewOpen}
        onClose={handleClosePreview}
        media={selectedFile}
      />

      {/* 文件上传弹窗 */}
      <UploadFileModal
        isOpen={uploadModalOpen}
        onClose={handleCloseUploadModal}
        onUpload={handleUpload}
        maxFiles={1}
        currentFilesCount={0}
      />
    </div>
  );
}
