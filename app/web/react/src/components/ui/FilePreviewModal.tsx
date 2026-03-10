'use client';

import { useState, useEffect, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui';
import { getMediaUrl } from '@/actions';

// 文件类型
enum FileType {
  IMAGE = 'image',
  PDF = 'pdf',
  OTHER = 'other',
}

interface FilePreviewModalProps {
  isOpen: boolean;
  onClose: () => void;
  media: {
    fileName: string;
    contentType?: string;
    guid?: string;
  } | null;
}

// 根据 contentType 判断文件类型
function getFileType(contentType?: string): FileType {
  if (!contentType) return FileType.OTHER;
  
  if (contentType.startsWith('image/')) {
    return FileType.IMAGE;
  }
  if (contentType === 'application/pdf') {
    return FileType.PDF;
  }
  return FileType.OTHER;
}

export function FilePreviewModal({ isOpen, onClose, media }: FilePreviewModalProps) {
  const t = useTranslations('profile.files');
  const [isLoading, setIsLoading] = useState(true);
  const [imageSrc, setImageSrc] = useState<string | null>(null);
  const [scale, setScale] = useState(1);
  const [rotate, setRotate] = useState(0);
  const [error, setError] = useState<string | null>(null);

  // 加载文件
  useEffect(() => {
    if (!isOpen || !media?.guid) {
      setImageSrc(null);
      setIsLoading(false);
      return;
    }

    const loadFile = async () => {
      setIsLoading(true);
      setError(null);
      setScale(1);
      setRotate(0);

      try {
        // 通过 action 获取带 token 的媒体 URL
        const result = await getMediaUrl(media.guid!);
        
        if (!result.success || !result.data) {
          throw new Error(result.error || 'Failed to get media URL');
        }

        // 直接使用带 token 的 URL
        setImageSrc(result.data);
      } catch (err) {
        console.error('[FilePreviewModal] Load error:', err);
        setError(t('loadError') || 'Failed to load file');
      } finally {
        setIsLoading(false);
      }
    };

    loadFile();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen, media?.guid]);

  // 键盘事件处理
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (!isOpen) return;
      
      if (e.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  // 缩放
  const handleZoomIn = useCallback(() => {
    setScale((prev) => Math.min(prev + 0.2, 3));
  }, []);

  const handleZoomOut = useCallback(() => {
    setScale((prev) => Math.max(prev - 0.2, 0.5));
  }, []);

  // 旋转
  const handleRotate = useCallback(() => {
    setRotate((prev) => (prev + 90) % 360);
  }, []);

  // 下载
  const handleDownload = useCallback(() => {
    if (imageSrc && media?.fileName) {
      const link = document.createElement('a');
      link.href = imageSrc;
      link.download = media.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }, [imageSrc, media?.fileName]);

  if (!isOpen) return null;

  const fileType = getFileType(media?.contentType);

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70"
      onClick={onClose}
    >
      {/* 内容区域 */}
      <div 
        className="relative max-w-[90vw] max-h-[90vh]"
        onClick={(e) => e.stopPropagation()}
      >
        {/* 加载状态 */}
        {isLoading && (
          <div className="flex items-center justify-center w-[400px] h-[300px]">
            <div className="flex flex-col items-center gap-4">
              <div className="size-10 border-4 border-white/30 border-t-white rounded-full animate-spin" />
              <p className="text-white text-sm">{t('loading') || 'Loading...'}</p>
            </div>
          </div>
        )}

        {/* 错误状态 */}
        {error && !isLoading && (
          <div className="flex items-center justify-center w-[400px] h-[300px]">
            <p className="text-white text-sm">{error}</p>
          </div>
        )}

        {/* 图片预览 */}
        {!isLoading && !error && imageSrc && fileType === FileType.IMAGE && (
          <div 
            className="transition-transform duration-200"
            style={{
              transform: `scale(${scale}) rotate(${rotate}deg)`,
            }}
          >
            <Image
              src={imageSrc}
              alt={media?.fileName || 'Preview'}
              width={800}
              height={600}
              className="max-w-[80vw] max-h-[80vh] object-contain"
              unoptimized
            />
          </div>
        )}

        {/* PDF 预览 - 使用 iframe */}
        {!isLoading && !error && imageSrc && fileType === FileType.PDF && (
          <iframe
            src={imageSrc}
            className="w-[80vw] h-[80vh] bg-white rounded"
            title={media?.fileName || 'PDF Preview'}
          />
        )}

        {/* 其他文件类型 - 显示下载提示 */}
        {!isLoading && !error && fileType === FileType.OTHER && (
          <div className="flex flex-col items-center justify-center w-[400px] h-[300px] gap-4">
            <p className="text-white text-base">{media?.fileName}</p>
            <p className="text-white/70 text-sm">{t('cannotPreview') || 'This file type cannot be previewed'}</p>
            <Button variant="primary" size="sm" onClick={handleDownload}>
              {t('download') || 'Download'}
            </Button>
          </div>
        )}
      </div>

      {/* 工具栏 - 底部 */}
      {!isLoading && !error && fileType === FileType.IMAGE && (
        <div className="fixed bottom-8 left-1/2 -translate-x-1/2 flex items-center gap-2 px-4 py-2 bg-black/50 rounded-full">
          <button
            onClick={handleZoomOut}
            className="p-2 text-white hover:bg-white/20 rounded-full transition-colors"
            title={t('zoomOut') || 'Zoom out'}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
          </button>
          <span className="text-white text-sm min-w-[50px] text-center">
            {Math.round(scale * 100)}%
          </span>
          <button
            onClick={handleZoomIn}
            className="p-2 text-white hover:bg-white/20 rounded-full transition-colors"
            title={t('zoomIn') || 'Zoom in'}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
          </button>
          <div className="w-px h-6 bg-white/30 mx-2" />
          <button
            onClick={handleRotate}
            className="p-2 text-white hover:bg-white/20 rounded-full transition-colors"
            title={t('rotate') || 'Rotate'}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M21 12a9 9 0 11-9-9c2.52 0 4.93 1 6.74 2.74L21 8" />
              <path d="M21 3v5h-5" />
            </svg>
          </button>
          <div className="w-px h-6 bg-white/30 mx-2" />
          <button
            onClick={handleDownload}
            className="p-2 text-white hover:bg-white/20 rounded-full transition-colors"
            title={t('download') || 'Download'}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4" />
              <polyline points="7 10 12 15 17 10" />
              <line x1="12" y1="15" x2="12" y2="3" />
            </svg>
          </button>
        </div>
      )}

      {/* 关闭按钮 - 右上角 */}
      <button
        onClick={onClose}
        className="fixed top-6 right-6 p-3 bg-black/50 text-white hover:bg-black/70 rounded-full transition-colors"
        title={t('close') || 'Close'}
      >
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      </button>

      {/* 文件名 - 顶部 */}
      <div className="fixed top-6 left-6 max-w-[60%]">
        <p className="text-white text-sm truncate">
          {media?.fileName}
        </p>
      </div>
    </div>
  );
}

export default FilePreviewModal;
