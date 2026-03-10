'use client';

import { useState, useRef, useEffect } from 'react';
import { useLocale } from 'next-intl';
import Image from 'next/image';
import { LanguageTypes, type ILanguage } from '@/core/types/LanguageTypes';
import { useServerAction } from '@/hooks/useServerAction';
import { setLocale, updateUserLanguage } from '@/actions';
import { useUserStore } from '@/stores';

// 语言代码映射：LanguageTypes code -> i18n locale
const localeMap: Record<string, string> = {
  'en-us': 'en',
  'zh-cn': 'zh',
  'zh-tw': 'zh-tw',
  'vi-vn': 'vi',
  'th-th': 'th',
  'jp-jp': 'jp',
  'id-id': 'id',
  'ms-my': 'ms',
  'ko-kr': 'ko',
  'km-kh': 'km',
  'es-es': 'es',
};

// 反向映射：i18n locale -> LanguageTypes code
const reverseLocaleMap: Record<string, string> = Object.entries(localeMap).reduce(
  (acc, [key, value]) => ({ ...acc, [value]: key }),
  {}
);

export function LanguageToggle() {
  const currentLocale = useLocale();
  const [isPending, setIsPending] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { execute } = useServerAction({ showErrorToast: false });

  // 获取当前语言（从 i18n locale 转换为 LanguageTypes code）
  const currentLangCode = reverseLocaleMap[currentLocale] || 'en-us';
  const currentLang = LanguageTypes.all.find((l) => l.code === currentLangCode) || LanguageTypes.enUS;

  // 点击外部关闭下拉框
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = async (lang: ILanguage) => {
    setIsOpen(false);
    
    const newLocale = localeMap[lang.code] || 'en';
    
    if (newLocale === currentLocale) return;

    setIsPending(true);

    await updateUserLanguage(lang.code).catch(() => {});

    const user = useUserStore.getState().user;
    if (user) {
      useUserStore.getState().setUser({ ...user, language: lang.code });
    }

    await execute(setLocale, { locale: newLocale });

    window.location.reload();
  };

  return (
    <div className="relative" ref={dropdownRef}>
      {/* 触发按钮 */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        disabled={isPending}
        className="flex items-center gap-2 text-text-secondary transition-colors hover:text-text-primary disabled:opacity-50"
      >
        <div className="relative h-[18px] w-[32px] overflow-hidden rounded-sm">
          <Image
            src={currentLang.flag}
            alt={currentLang.name}
            fill
            className="object-cover"
          />
        </div>
        <span className="text-sm font-medium cursor-pointer">{currentLang.name}</span>
        <svg
          className={`h-5 w-5 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`}
          viewBox="0 0 20 20"
          fill="currentColor"
        >
          <path
            fillRule="evenodd"
            d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
            clipRule="evenodd"
          />
        </svg>
      </button>

      {/* 下拉菜单 */}
      {isOpen && (
        <div 
          className="lang-dropdown absolute right-0 top-full z-50 mt-2 w-[220px] overflow-hidden rounded-lg"
        >
          <div className="flex flex-col gap-5 p-5 max-h-[400px] overflow-y-auto">
            {LanguageTypes.all.map((lang, index) => (
              <div key={lang.code}>
                <button
                  onClick={() => handleSelect(lang)}
                  disabled={isPending}
                  className="flex w-full items-center justify-between disabled:opacity-50 cursor-pointer"
                >
                  {/* 左侧：国旗 + 语言名称 */}
                  <div className="flex items-center gap-2">
                    <div className="relative h-[18px] w-[32px] overflow-hidden">
                      <Image
                        src={lang.flag}
                        alt={lang.name}
                        fill
                        className="object-cover"
                      />
                    </div>
                    <span className="text-sm font-medium text-text-primary cursor-pointer">
                      {lang.name}
                    </span>
                  </div>
                  
                  {/* 右侧：选择指示器 */}
                  <div className="h-5 w-5 flex items-center justify-center">
                    {lang.code === currentLang.code ? (
                      // 选中状态 - 填充的圆圈带勾
                      <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                        <circle 
                          cx="10" 
                          cy="10" 
                          r="10" 
                          className="lang-radio-selected"
                        />
                        <path 
                          d="M6 10L9 13L14 7" 
                          stroke="white" 
                          strokeWidth="2" 
                          strokeLinecap="round" 
                          strokeLinejoin="round"
                        />
                      </svg>
                    ) : (
                      // 未选中状态 - 空心圆圈
                      <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                        <circle 
                          cx="10" 
                          cy="10" 
                          r="9" 
                          className="lang-radio-unselected"
                          strokeWidth="1"
                          fill="none"
                        />
                      </svg>
                    )}
                  </div>
                </button>
                
                {/* 分割线（最后一项不显示） */}
                {index < LanguageTypes.all.length - 1 && (
                  <div className="lang-divider mt-5 h-[0.5px] w-full" />
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
