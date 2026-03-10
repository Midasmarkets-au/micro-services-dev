'use client';

import { forwardRef, useState, useRef, useEffect } from 'react';
import Image from 'next/image';

interface SelectOption {
  value: string;
  label: string;
  code?: string; // 可选的代码，如国家代码 'cn', 'us'
}

interface SelectInputProps {
  label?: string;
  labelClassName?: string;
  /** 下拉选择的值 */
  selectValue?: string;
  /** 输入框的值 */
  inputValue?: string;
  /** 下拉选项列表 */
  selectOptions: SelectOption[];
  /** 下拉选择变化回调 */
  onSelectChange?: (value: string) => void;
  /** 输入框变化回调 */
  onInputChange?: (value: string) => void;
  error?: string;
  errorPosition?: 'top' | 'bottom';
  placeholder?: string;
  disabled?: boolean;
  /** 是否显示完整标签，默认 false 只显示 value */
  showFullLabel?: boolean;
  /** 下拉选择器宽度，默认 70px */
  selectWidth?: string;
  /** 输入框类型，默认 text */
  inputType?: 'text' | 'tel' | 'email' | 'number';
  /** 下拉菜单宽度，默认 320px */
  dropdownWidth?: string;
  /** 搜索占位符 */
  searchPlaceholder?: string;
  /** 无匹配结果文本 */
  noResultText?: string;
}

export const SelectInput = forwardRef<HTMLInputElement, SelectInputProps>(
  (
    {
      label,
      labelClassName,
      selectValue = '',
      inputValue = '',
      selectOptions,
      onSelectChange,
      onInputChange,
      error,
      errorPosition = 'bottom',
      placeholder = '',
      disabled = false,
      showFullLabel = false,
      selectWidth = '70px',
      inputType = 'text',
      dropdownWidth = '320px',
      searchPlaceholder = '搜索...',
      noResultText = '无匹配结果',
    },
    ref
  ) => {
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [isFocused, setIsFocused] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const searchInputRef = useRef<HTMLInputElement>(null);

    // 获取当前选中的选项（用于显示）
    const selectedOption = selectOptions.find((opt) => opt.value === selectValue);
    const displayLabel = showFullLabel ? (selectedOption?.label || selectValue) : selectValue;

    // 过滤选项
    const filteredOptions = searchTerm
      ? selectOptions.filter((option) =>
          option.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
          option.value.toLowerCase().includes(searchTerm.toLowerCase())
        )
      : selectOptions;

    // 点击外部关闭下拉框
    useEffect(() => {
      const handleClickOutside = (event: MouseEvent) => {
        if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
          setIsDropdownOpen(false);
          setSearchTerm('');
          setIsFocused(false);
        }
      };

      if (isDropdownOpen) {
        document.addEventListener('mousedown', handleClickOutside);
      }

      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
      };
    }, [isDropdownOpen]);

    return (
      <div className="w-full">
        {/* Label 和错误提示 */}
        {(label || (error && errorPosition === 'top')) && (
          <div className={`mb-2 flex items-center justify-between ${labelClassName || 'text-sm font-normal text-text-secondary'}`}>
            {label && <span>{label}</span>}
            {error && errorPosition === 'top' && (
              <span className="error-text text-sm font-normal">{error}</span>
            )}
          </div>
        )}

        {/* 组合输入框 */}
        <div
          className={`flex h-12 items-center gap-3 rounded bg-input-bg px-5 transition-all ${
            error ? 'error-border border' : 'border border-transparent'
          } ${disabled ? 'cursor-not-allowed opacity-60' : ''}`}
        >
          {/* 下拉选择器（可搜索） */}
          <div className="relative shrink-0" style={{ width: selectWidth }} ref={dropdownRef}>
            <div className="flex items-center gap-2.5">
              <input
                ref={searchInputRef}
                type="text"
                value={isFocused ? searchTerm : displayLabel}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setIsDropdownOpen(true);
                }}
                onFocus={() => {
                  setIsFocused(true);
                  setIsDropdownOpen(true);
                  setSearchTerm('');
                }}
                onBlur={() => {
                  // 延迟以允许点击选项
                  setTimeout(() => {
                    setIsFocused(false);
                    setSearchTerm('');
                  }, 200);
                }}
                disabled={disabled}
                placeholder={searchPlaceholder}
                className="w-full bg-transparent text-sm text-text-secondary focus:outline-none truncate"
              />
              <button
                type="button"
                onClick={() => {
                  if (!disabled) {
                    setIsDropdownOpen(!isDropdownOpen);
                    if (!isDropdownOpen) {
                      searchInputRef.current?.focus();
                    }
                  }
                }}
                disabled={disabled}
                className="shrink-0"
              >
                <Image
                  src="/images/icons/arrow-down.svg"
                  alt="dropdown"
                  width={20}
                  height={20}
                  className="transition-transform"
                  style={{
                    transform: isDropdownOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                  }}
                />
              </button>
            </div>

            {/* 下拉菜单 */}
            {isDropdownOpen && (
              <div 
                className="absolute left-0 top-full z-50 mt-1 max-h-60 overflow-hidden rounded border border-border bg-surface shadow-dropdown"
                style={{ width: dropdownWidth }}
              >
                {/* 选项列表 */}
                <div className="max-h-60 overflow-y-auto">
                  {filteredOptions.length > 0 ? (
                    filteredOptions.map((option, index) => (
                      <button
                        key={option.code || `${option.value}-${index}`}
                        type="button"
                        onClick={() => {
                          onSelectChange?.(option.value);
                          setIsDropdownOpen(false);
                          setSearchTerm('');
                          setIsFocused(false);
                        }}
                        className={`w-full px-4 py-2 text-left text-sm transition-colors ${
                          option.value === selectValue
                            ? 'bg-primary-light text-primary hover:bg-primary-light'
                            : 'text-text-primary hover:bg-[#f5f5f5] cursor-pointer dark:hover:bg-[#2a2a2a]'
                        }`}
                      >
                        {option.label}
                      </button>
                    ))
                  ) : (
                    <div className="px-4 py-3 text-center text-sm text-text-secondary">
                      {noResultText}
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>

          {/* 竖线分隔 */}
          <div className="h-5 w-px shrink-0 bg-[#333333]"></div>

          {/* 输入框 */}
          <input
            ref={ref}
            type={inputType}
            value={inputValue}
            onChange={(e) => onInputChange?.(e.target.value)}
            placeholder={placeholder}
            disabled={disabled}
            className="flex-1 bg-transparent text-sm text-text-primary placeholder:text-text-placeholder focus:outline-none"
            autoComplete={inputType === 'tel' ? 'tel' : 'off'}
          />
        </div>

        {/* 错误提示在底部 */}
        {error && errorPosition === 'bottom' && (
          <p className="error-text mt-1 text-sm">{error}</p>
        )}
      </div>
    );
  }
);

SelectInput.displayName = 'SelectInput';

// 为了向后兼容，保留 PhoneInput 别名
export const PhoneInput = SelectInput;
