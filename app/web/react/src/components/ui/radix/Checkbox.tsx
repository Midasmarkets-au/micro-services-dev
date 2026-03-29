'use client';

import * as React from 'react';
import * as CheckboxPrimitive from '@radix-ui/react-checkbox';
import { CheckIcon } from '@radix-ui/react-icons';
import { cn } from '@/lib/utils';

interface CheckboxProps extends React.ComponentPropsWithoutRef<typeof CheckboxPrimitive.Root> {
  label?: React.ReactNode;
  description?: React.ReactNode;
  error?: boolean;
  variant?: 'default' | 'circle' | 'radio';
}

const Checkbox = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  CheckboxProps
>(({ className, label, description, error, variant = 'default', ...props }, ref) => {
  const isCircle = variant === 'circle';
  const isRadio = variant === 'radio';

  return (
    <div className="flex gap-3 items-center">
      <CheckboxPrimitive.Root
        ref={ref}
        className={cn(
          'peer shrink-0 border-2 transition-colors duration-200',
          isRadio
            ? 'focus-visible:outline-none'
            : 'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-(--color-primary) focus-visible:ring-offset-2',
          'disabled:cursor-not-allowed disabled:opacity-50',
          isRadio
            ? [
                'size-4 rounded-full border-2',
                'data-[state=unchecked]:bg-transparent border-(--color-text-secondary)',
                'data-[state=checked]:border-[#800020] data-[state=checked]:bg-transparent',
                'dark:data-[state=checked]:border-[#004EFF] dark:data-[state=checked]:bg-[#111111]',
              ]
            : 'data-[state=checked]:bg-(--color-primary) data-[state=checked]:border-(--color-primary)',
          isCircle || isRadio ? 'rounded-full' : 'rounded',
          isRadio ? '' : 'size-5',
          error
            ? 'border-(--color-error-border)'
            : isRadio
              ? ''
              : 'border-(--color-text-secondary) data-[state=unchecked]:bg-transparent',
          className
        )}
        {...props}
      >
        <CheckboxPrimitive.Indicator
          className={cn('flex items-center justify-center text-white')}
        >
          {isRadio ? (
            <span className="size-2 rounded-full bg-[#800020] dark:bg-[#004EFF]" />
          ) : (
            <CheckIcon className={cn(isCircle ? 'size-3' : 'size-4')} />
          )}
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>

      {(label || description) && (
        <div className="flex flex-col gap-1">
          {label && (
            <label className="cursor-pointer text-sm font-medium text-(--color-text-primary)">
              {label}
            </label>
          )}
          {description && (
            <p className="text-xs text-(--color-text-secondary)">
              {description}
            </p>
          )}
        </div>
      )}
    </div>
  );
});
Checkbox.displayName = CheckboxPrimitive.Root.displayName;

export { Checkbox };
