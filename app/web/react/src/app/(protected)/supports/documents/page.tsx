'use client';

import { useMemo } from 'react';
import Image from 'next/image';
import { useTranslations, useLocale } from 'next-intl';
import { getVerificationDocuments } from '@/core/data/bcrDocs';
import { useTheme } from '@/hooks/useTheme';
// 文档卡片组件
function DocumentCard({ 
  title, 
  url,
}: { 
  title: string; 
  url: string; 
}) {
  const t = useTranslations('supports');
  const tDoc = useTranslations('verification.agreements.documentList');
  const { theme } = useTheme();
  const isDark = theme === 'dark';
  const handleDownload = () => {
    if (url) {
      window.open(url, '_blank');
    }
  };

  return (
    <div 
      className="flex items-center gap-5 p-5 md:p-8 lg:p-10 rounded-xl border transition-colors bg-white border-[#EEE] hover:border-[#DDD] dark:bg-[#171717] dark:border-[#333] dark:hover:border-[#444]"
    >
      {/* 文档图标 - 使用 hidden/block 切换日夜模式图标 */}
      <div className="shrink-0">
        <Image
          src={isDark?'/images/icons/file-night.svg':'/images/icons/file-day.svg'}
          alt=""
          width={40}
          height={40}
          className="size-8 md:size-10"
        />
      </div>
      {/* 文档名称 */}
      <p className="flex-1 text-sm font-medium text-[#333] dark:text-white">
        {tDoc(title)}
      </p>
      
      {/* 下载按钮 */}
      <button
        onClick={handleDownload}
        disabled={!url}
        className="shrink-0 flex items-center justify-center gap-2 px-3 py-1.5 rounded text-sm font-medium transition-colors bg-[#f8f8f8] text-[#333] hover:bg-[#efefef] disabled:opacity-50 disabled:cursor-not-allowed dark:bg-[#2e2e2e] dark:text-white dark:hover:bg-[#3e3e3e]"
        style={{ width: '80px' }}
      >
        <Image
          src="/images/icons/download.svg"
          alt=""
          width={20}
          height={20}
          className="size-4 md:size-5 dark:brightness-0 dark:invert"
        />
        <span>{t('documents.download')}</span>
      </button>
    </div>
  );
}

export default function DocumentsPage() {
  const t = useTranslations('supports');
  const locale = useLocale();

  // 获取文档列表
  const documents = useMemo(() => {
    return getVerificationDocuments(locale, false);
  }, [locale]);

  // 如果没有文档
  if (documents.length === 0) {
    return (
      <div className="flex items-center justify-center py-20">
        <p className="text-text-secondary">{t('noDocuments')}</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-5">
      {/* 标题 */}
      <h2 className="text-xl font-semibold text-[#333] dark:text-white">
        {t('documents.title')}
      </h2>

      {/* 文档卡片网格 */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        {documents.map((doc) => (
          <DocumentCard
            key={doc.key}
            title={doc.title}
            url={doc.url}
          />
        ))}
      </div>
    </div>
  );
}
