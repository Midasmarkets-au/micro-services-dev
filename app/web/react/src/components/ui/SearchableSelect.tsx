'use client';

import { forwardRef, useId } from 'react';
import Image from 'next/image';
import ReactSelect, { Props as ReactSelectProps, StylesConfig, components } from 'react-select';

interface SearchableSelectProps extends Omit<ReactSelectProps, 'styles'> {
  label?: string;
  labelClassName?: string;
  error?: string;
  errorPosition?: 'top' | 'bottom';
}

// 自定义下拉箭头组件
const DropdownIndicator = (props: any) => {
  return (
    <components.DropdownIndicator {...props}>
      <Image
        src="/images/icons/arrow-down.svg"
        alt="dropdown"
        width={20}
        height={20}
      />
    </components.DropdownIndicator>
  );
};

export const SearchableSelect = forwardRef<any, SearchableSelectProps>(
  ({ label, labelClassName, error, errorPosition = 'top', ...props }, ref) => {
    // 使用 useId 生成稳定的 ID 以避免 SSR 水合错误
    const instanceId = useId();
    
    // 自定义样式以匹配项目设计
    const customStyles: StylesConfig = {
      control: (base, state) => ({
        ...base,
        minHeight: '48px',
        backgroundColor: 'var(--color-input-bg)',
        borderColor: error ? 'var(--color-error-border)' : (state.isFocused ? 'var(--color-primary)' : 'transparent'),
        borderRadius: '4px',
        boxShadow: 'none',
        outline: 'none',
        fontSize: '14px',
        transition: 'border-color 0.2s, box-shadow 0.2s',
        '&:hover': {
          borderColor: error ? 'var(--color-error-border)' : (state.isFocused ? 'var(--color-primary)' : 'var(--color-border)'),
        },
      }),
      menu: (base) => ({
        ...base,
        backgroundColor: 'var(--color-surface)',
        border: '1px solid var(--color-border)',
        boxShadow: 'var(--shadow-dropdown)',
        zIndex: 50,
      }),
      option: (base, state) => ({
        ...base,
        backgroundColor: state.isSelected
          ? 'var(--color-primary-light)'
          : state.isFocused
          ? 'var(--color-surface-secondary)'
          : 'transparent',
        color: state.isSelected ? 'var(--color-primary)' : 'var(--color-text-primary)',
        cursor: 'pointer',
        fontSize: '14px',
        '&:active': {
          backgroundColor: 'var(--color-primary-light)',
        },
      }),
      singleValue: (base, state) => ({
        ...base,
        color: state.isDisabled ? 'var(--color-text-secondary)' : 'var(--color-text-primary)',
      }),
      input: (base) => ({
        ...base,
        color: 'var(--color-text-primary)',
      }),
      placeholder: (base) => ({
        ...base,
        color: 'var(--color-text-placeholder)',
      }),
      dropdownIndicator: (base) => ({
        ...base,
        padding: '8px',
      }),
      indicatorSeparator: () => ({
        display: 'none',
      }),
    };

    return (
      <div className="w-full">
        {/* Label 和错误提示 */}
        {(label || (error && errorPosition === 'top')) && (
          <div className={`mb-2 flex items-center justify-between ${labelClassName || 'text-sm font-normal  text-text-secondary'}`}>
            {label && <span>{label}</span>}
            {error && errorPosition === 'top' && (
              <span className="error-text text-sm font-normal">{error}</span>
            )}
          </div>
        )}
        
        <ReactSelect
          ref={ref}
          instanceId={instanceId}
          styles={customStyles}
          classNamePrefix="react-select"
          components={{
            DropdownIndicator,
            ClearIndicator: () => null, // 移除清空按钮
          }}
          isClearable={false} // 禁用清空功能
          {...props}
        />
        
        {/* 错误提示在底部 */}
        {error && errorPosition === 'bottom' && (
          <p className="error-text mt-1 text-sm">{error}</p>
        )}
      </div>
    );
  }
);

SearchableSelect.displayName = 'SearchableSelect';
