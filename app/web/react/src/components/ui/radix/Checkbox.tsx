'use client';

import * as React from 'react';
import * as CheckboxPrimitive from '@radix-ui/react-checkbox';
import { CheckIcon } from '@radix-ui/react-icons';
import { cn } from '@/lib/utils';

interface CheckboxProps extends React.ComponentPropsWithoutRef<typeof CheckboxPrimitive.Root> {
  label?: React.ReactNode;
  description?: React.ReactNode;
  error?: boolean;
  variant?: 'default' | 'circle';
}

const Checkbox = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  CheckboxProps
>(({ className, label, description, error, variant = 'default', ...props }, ref) => {
  const isCircle = variant === 'circle';

  return (
    <div className="flex items-start gap-3">
      <CheckboxPrimitive.Root
        ref={ref}
        className={cn(
          'peer shrink-0 border-2 transition-colors duration-200',
          'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)] focus-visible:ring-offset-2',
          'disabled:cursor-not-allowed disabled:opacity-50',
          'data-[state=checked]:bg-[var(--color-primary)] data-[state=checked]:border-[var(--color-primary)]',
          isCircle ? 'size-5 rounded-full' : 'size-5 rounded',
          error
            ? 'border-[var(--color-error-border)]'
            : 'border-[var(--color-text-secondary)] data-[state=unchecked]:bg-transparent',
          className
        )}
        {...props}
      >
        <CheckboxPrimitive.Indicator
          className={cn('flex items-center justify-center text-white')}
        >
          <CheckIcon className={cn(isCircle ? 'size-3' : 'size-4')} />
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>

      {(label || description) && (
        <div className="flex flex-col gap-1">
          {label && (
            <label className="text-sm font-medium text-[var(--color-text-primary)] cursor-pointer">
              {label}
            </label>
          )}
          {description && (
            <p className="text-xs text-[var(--color-text-secondary)]">
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
