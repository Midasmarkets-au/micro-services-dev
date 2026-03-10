'use client';

import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { Button } from '@/components/ui';

// 文件类型枚举
enum FileFormatTypes {
  PDF = 'pdf',
  JPEG = 'jpeg',
  JPG = 'jpg',
  PNG = 'png',
  TXT = 'txt',
  DOC = 'doc',
}

// 文件状态
enum DocumentStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
}

interface FileCardProps {
  media: {
    fileName: string;
    contentType?: string;
    status?: number;
    rejectedReason?: string;
    documentType?: number;
    url?: string;
    guid?: string;
  };
  onClick?: (media: FileCardProps['media']) => void;
}

// 根据 contentType 获取文件类型
function getFileType(contentType?: string): FileFormatTypes {
  const typeMap: Record<string, FileFormatTypes> = {
    'application/pdf': FileFormatTypes.PDF,
    'image/jpeg': FileFormatTypes.JPEG,
    'image/jpg': FileFormatTypes.JPG,
    'image/png': FileFormatTypes.PNG,
    'text/plain': FileFormatTypes.TXT,
  };
  return typeMap[contentType || ''] || FileFormatTypes.DOC;
}

// 根据文件类型获取图标路径
function getFileIcon(fileType: FileFormatTypes, theme: string): string {
  const isImage = [FileFormatTypes.JPEG, FileFormatTypes.JPG, FileFormatTypes.PNG].includes(fileType);
  
  if (fileType === FileFormatTypes.PDF) {
    return '/images/icons/document-file-day.svg';
  }
  if (isImage) {
    return theme === 'dark' ? '/images/icons/file-night.svg' : '/images/icons/file-day.svg';
  }
  return theme === 'dark' ? '/images/icons/document-file-night.svg' : '/images/icons/document-file-day.svg';
}

export function FileCard({ media, onClick }: FileCardProps) {
  const t = useTranslations('profile.files');
  const { theme } = useTheme();
  
  const fileType = getFileType(media.contentType);
  const iconPath = getFileIcon(fileType, theme);
  const isRejected = media.status === DocumentStatus.Rejected;

  const handleClick = () => {
    onClick?.(media);
  };

  return (
    <div 
      className="flex items-center gap-5 p-10 bg-surface border border-border rounded-[12px] overflow-hidden"
    >
      {/* 左侧：文件图标 */}
      <div className="shrink-0 size-10">
        <Image
          src={iconPath}
          alt={media.fileName}
          width={40}
          height={40}
          className="object-contain"
        />
      </div>

      {/* 中间：文件名 */}
      <div className="flex-1 min-w-0 flex items-center gap-2">
        <p 
          className="text-xs text-text-primary truncate"
          title={media.fileName}
        >
          {media.fileName}
        </p>
        
        {/* 拒绝状态标签 */}
        {isRejected && (
          <span className="shrink-0 px-2 py-1 text-responsive-2xs bg-(--color-error-bg) text-(--color-error) rounded">
            {t('rejected')}
          </span>
        )}
      </div>

      {/* 右侧：查看按钮 */}
      <Button
        onClick={handleClick}
        variant="secondary"
        size="xs"
        className="w-20 shrink-0"
      >
        {t('view')}
      </Button>
    </div>
  );
}

export default FileCard;
