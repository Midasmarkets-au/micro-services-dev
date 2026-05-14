'use client';

import * as React from 'react';
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronDownIcon,
} from '@radix-ui/react-icons';
import {
  DayPicker,
  getDefaultClassNames,
  type DayButton,
  type Locale,
} from 'react-day-picker';
import { cn } from '@/lib/utils';
import { buttonVariants } from './Button';

export type CalendarProps = React.ComponentProps<typeof DayPicker> & {
  buttonVariant?: 'ghost' | 'outline' | 'secondary';
};

function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  captionLayout = 'label',
  buttonVariant: bv = 'ghost',
  formatters,
  components,
  locale,
  ...props
}: CalendarProps) {
  const defaultClassNames = getDefaultClassNames();

  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      className={cn(
        'p-3 [--cell-size:2.25rem] [--cell-radius:var(--radius-sm)]',
        className
      )}
      captionLayout={captionLayout}
      locale={locale}
      formatters={{
        formatMonthDropdown: (date) =>
          date.toLocaleString(locale?.code, { month: 'short' }),
        ...formatters,
      }}
      classNames={{
        root: cn('w-fit', defaultClassNames.root),
        months: cn('relative flex flex-col gap-4 md:flex-row', defaultClassNames.months),
        month: cn('flex w-full flex-col gap-4', defaultClassNames.month),
        nav: cn(
          'absolute inset-x-0 top-0 flex w-full items-center justify-between',
          defaultClassNames.nav
        ),
        button_previous: cn(
          buttonVariants({ variant: bv }),
          'size-(--cell-size) p-0 select-none aria-disabled:opacity-50',
          defaultClassNames.button_previous
        ),
        button_next: cn(
          buttonVariants({ variant: bv }),
          'size-(--cell-size) p-0 select-none aria-disabled:opacity-50',
          defaultClassNames.button_next
        ),
        month_caption: cn(
          'flex h-(--cell-size) w-full items-center justify-center px-(--cell-size)',
          defaultClassNames.month_caption
        ),
        dropdowns: cn(
          'flex h-(--cell-size) w-full items-center justify-center gap-1.5 text-sm font-medium',
          defaultClassNames.dropdowns
        ),
        dropdown_root: cn(
          'calendar-dropdown-root relative rounded border border-border shadow-xs',
          defaultClassNames.dropdown_root
        ),
        dropdown: cn(
          'calendar-dropdown absolute inset-0 opacity-0 cursor-pointer',
          defaultClassNames.dropdown
        ),
        caption_label: cn(
          'text-sm font-medium text-text-primary select-none',
          captionLayout !== 'label' && 'flex h-8 items-center gap-1 rounded px-2 pr-1 text-sm',
          defaultClassNames.caption_label
        ),
        table: 'w-full border-collapse',
        weekdays: cn('flex', defaultClassNames.weekdays),
        weekday: cn(
          'flex-1 text-xs font-normal text-text-secondary select-none text-center',
          defaultClassNames.weekday
        ),
        week: cn('mt-1 flex w-full', defaultClassNames.week),
        day: cn(
          'group/day relative aspect-square h-full w-full p-0 text-center select-none',
          defaultClassNames.day
        ),
        range_start: cn('rounded-l-[--cell-radius] bg-primary/10', defaultClassNames.range_start),
        range_middle: cn('rounded-none bg-primary/10', defaultClassNames.range_middle),
        range_end: cn('rounded-r-[--cell-radius] bg-primary/10', defaultClassNames.range_end),
        today: cn(
          'rounded-[--cell-radius] data-[selected=true]:rounded-none',
          defaultClassNames.today
        ),
        outside: cn(
          'text-text-placeholder opacity-40 aria-selected:text-text-placeholder',
          defaultClassNames.outside
        ),
        disabled: cn('text-text-placeholder opacity-30', defaultClassNames.disabled),
        hidden: cn('invisible', defaultClassNames.hidden),
        ...classNames,
      }}
      components={{
        Root: ({ className: rootCn, rootRef, ...rootProps }) => (
          <div data-slot="calendar" ref={rootRef} className={cn(rootCn)} {...rootProps} />
        ),
        Chevron: ({ className: chevCn, orientation, ...chevProps }) => {
          if (orientation === 'left')
            return <ChevronLeftIcon className={cn('size-4', chevCn)} {...chevProps} />;
          if (orientation === 'right')
            return <ChevronRightIcon className={cn('size-4', chevCn)} {...chevProps} />;
          return <ChevronDownIcon className={cn('size-4', chevCn)} {...chevProps} />;
        },
        DayButton: (dayBtnProps) => <CalendarDayButton locale={locale} {...dayBtnProps} />,
        ...components,
      }}
      {...props}
    />
  );
}

function CalendarDayButton({
  className,
  day,
  modifiers,
  locale,
  ...props
}: React.ComponentProps<typeof DayButton> & { locale?: Partial<Locale> }) {
  const defaultClassNames = getDefaultClassNames();
  const ref = React.useRef<HTMLButtonElement>(null);

  React.useEffect(() => {
    if (modifiers.focused) ref.current?.focus();
  }, [modifiers.focused]);

  return (
    <button
      ref={ref}
      type="button"
      data-day={day.date.toLocaleDateString(locale?.code)}
      data-selected-single={
        modifiers.selected &&
        !modifiers.range_start &&
        !modifiers.range_end &&
        !modifiers.range_middle
      }
      data-range-start={modifiers.range_start}
      data-range-end={modifiers.range_end}
      data-range-middle={modifiers.range_middle}
      data-today={modifiers.today}
      data-outside={day.outside}
      data-disabled={modifiers.disabled}
      className={cn(
        // base — calendar-day-btn class handles surface-secondary bg via globals.css
        'calendar-day-btn',
        'inline-flex items-center justify-center w-full aspect-square min-w-(--cell-size) rounded-[--cell-radius]',
        'text-sm text-text-primary cursor-pointer transition-colors',
        'hover:bg-(--color-primary) hover:text-white',
        // focused ring
        'group-data-[focused=true]/day:relative group-data-[focused=true]/day:z-10',
        'group-data-[focused=true]/day:ring-2 group-data-[focused=true]/day:ring-primary/50',
        // selected single
        'data-[selected-single=true]:bg-(--color-primary) data-[selected-single=true]:text-white',
        // range start/end
        'data-[range-start=true]:bg-(--color-primary) data-[range-start=true]:text-white data-[range-start=true]:rounded-r-none',
        'data-[range-end=true]:bg-(--color-primary) data-[range-end=true]:text-white data-[range-end=true]:rounded-l-none',
        // range middle
        'data-[range-middle=true]:bg-(--color-primary-light) data-[range-middle=true]:text-(--color-primary) data-[range-middle=true]:rounded-none',
        // today — border highlight
        'data-[today=true]:ring-1 data-[today=true]:ring-(--color-primary) data-[today=true]:font-semibold',
        // outside days — transparent, no bg
        'data-[outside=true]:bg-transparent data-[outside=true]:text-(--color-text-placeholder) data-[outside=true]:opacity-40 data-[outside=true]:hover:bg-transparent',
        // disabled
        'data-[disabled=true]:bg-transparent data-[disabled=true]:text-(--color-text-placeholder) data-[disabled=true]:opacity-30 data-[disabled=true]:cursor-not-allowed data-[disabled=true]:hover:bg-transparent',
        defaultClassNames.day,
        className
      )}
      {...props}
    />
  );
}

Calendar.displayName = 'Calendar';

export { Calendar, CalendarDayButton };
