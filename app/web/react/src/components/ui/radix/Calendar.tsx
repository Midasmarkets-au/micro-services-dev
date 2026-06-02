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
  type MonthCaptionProps,
} from 'react-day-picker';
import { cn } from '@/lib/utils';
import { buttonVariants } from './Button';

type DrillView = 'days' | 'months' | 'years';

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
  const isDrillDown = captionLayout === 'label';

  const [view, setView] = React.useState<DrillView>('days');
  const [viewDate, setViewDate] = React.useState<Date>(
    () => props.month ?? props.defaultMonth ?? new Date()
  );

  React.useEffect(() => {
    if (props.month) setViewDate(props.month);
  }, [props.month]);

  const handleMonthChange = React.useCallback(
    (month: Date) => {
      setViewDate(month);
      props.onMonthChange?.(month);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [props.onMonthChange]
  );

  const containerClass = cn(
    'p-3 [--cell-size:2.25rem] [--cell-radius:var(--radius-sm)]',
    className
  );

  const navBtnClass = cn(
    buttonVariants({ variant: bv }),
    'size-(--cell-size) p-0 select-none disabled:opacity-50',
  );

  // ── 月份 / 年份视图（仅 drill-down 模式）──────────────
  if (isDrillDown && view !== 'days') {
    return (
      <div data-slot="calendar" className={cn('w-fit', containerClass)}>
        {view === 'months' ? (
          <MonthsView
            viewDate={viewDate}
            locale={locale}
            startMonth={props.startMonth}
            endMonth={props.endMonth}
            navBtnClass={navBtnClass}
            onSelect={(monthIdx) => {
              const next = new Date(viewDate.getFullYear(), monthIdx, 1);
              setViewDate(next);
              handleMonthChange(next);
              setView('days');
            }}
            onYearClick={() => setView('years')}
            onYearChange={(delta) => {
              setViewDate(
                (prev) => new Date(prev.getFullYear() + delta, prev.getMonth(), 1)
              );
            }}
          />
        ) : (
          <YearsView
            viewDate={viewDate}
            startMonth={props.startMonth}
            endMonth={props.endMonth}
            navBtnClass={navBtnClass}
            onSelect={(year) => {
              setViewDate(
                (prev) => new Date(year, prev.getMonth(), 1)
              );
              setView('months');
            }}
            onGroupChange={(delta) => {
              setViewDate(
                (prev) => new Date(prev.getFullYear() + delta * 10, prev.getMonth(), 1)
              );
            }}
          />
        )}
      </div>
    );
  }

  // ── 日期视图 ──────────────────────────────────────────
  const drillMonthProps = isDrillDown
    ? { month: viewDate, onMonthChange: handleMonthChange }
    : {};

  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      className={containerClass}
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
        ...(isDrillDown
          ? {
              MonthCaption: (captionProps: MonthCaptionProps) => {
                const { calendarMonth, displayIndex: _idx, ...divProps } = captionProps;
                void _idx;
                const d = calendarMonth?.date ?? viewDate;
                const fmt = new Intl.DateTimeFormat(locale?.code, { year: 'numeric', month: 'long' });
                const parts = fmt.formatToParts(d);
                const yearFirst = parts.findIndex(p => p.type === 'year') < parts.findIndex(p => p.type === 'month');
                const yearLabel = new Intl.DateTimeFormat(locale?.code, { year: 'numeric' }).format(d);
                const monthLabel = new Intl.DateTimeFormat(locale?.code, { month: 'long' }).format(d);
                const btnClass = 'text-sm font-medium text-text-primary hover:text-(--color-primary) cursor-pointer select-none transition-colors';
                const yearBtn = (
                  <button key="y" type="button" onClick={() => setView('years')} className={btnClass}>
                    {yearLabel}
                  </button>
                );
                const monthBtn = (
                  <button key="m" type="button" onClick={() => setView('months')} className={btnClass}>
                    {monthLabel}
                  </button>
                );
                return (
                  <div {...divProps}>
                    <span className="relative z-10 inline-flex items-center gap-1">
                      {yearFirst ? <>{yearBtn}{monthBtn}</> : <>{monthBtn}{yearBtn}</>}
                    </span>
                  </div>
                );
              },
            }
          : {}),
        ...components,
      }}
      {...props}
      {...drillMonthProps}
    />
  );
}

// ─── 月份选择视图 ───────────────────────────────────────

interface MonthsViewProps {
  viewDate: Date;
  locale?: Partial<Locale>;
  startMonth?: Date;
  endMonth?: Date;
  navBtnClass: string;
  onSelect: (monthIdx: number) => void;
  onYearClick: () => void;
  onYearChange: (delta: number) => void;
}

function MonthsView({
  viewDate,
  locale,
  startMonth,
  endMonth,
  navBtnClass,
  onSelect,
  onYearClick,
  onYearChange,
}: MonthsViewProps) {
  const year = viewDate.getFullYear();
  const now = new Date();
  const currentMonth = now.getMonth();
  const currentYear = now.getFullYear();

  const canPrev = !startMonth || year > startMonth.getFullYear();
  const canNext = !endMonth || year < endMonth.getFullYear();

  const yearLabel = new Intl.DateTimeFormat(locale?.code, { year: 'numeric' }).format(
    new Date(year, 0)
  );

  return (
    <div className="flex flex-col gap-4" style={{ minWidth: 'calc(7 * var(--cell-size))' }}>
      <div className="relative flex h-(--cell-size) items-center justify-between">
        <button
          type="button"
          disabled={!canPrev}
          onClick={() => onYearChange(-1)}
          className={navBtnClass}
        >
          <ChevronLeftIcon className="size-4" />
        </button>
        <button
          type="button"
          onClick={onYearClick}
          className="text-sm font-medium text-text-primary hover:text-(--color-primary) cursor-pointer select-none transition-colors"
        >
          {yearLabel}
        </button>
        <button
          type="button"
          disabled={!canNext}
          onClick={() => onYearChange(1)}
          className={navBtnClass}
        >
          <ChevronRightIcon className="size-4" />
        </button>
      </div>

      <div className="grid grid-cols-4 gap-y-1 gap-x-1">
        {Array.from({ length: 12 }, (_, i) => {
          const label = new Date(year, i, 1).toLocaleString(locale?.code, {
            month: 'short',
          });
          const isDisabled =
            (startMonth != null && new Date(year, i + 1, 0) < startMonth) ||
            (endMonth != null && new Date(year, i, 1) > endMonth);
          const isCurrent = year === currentYear && i === currentMonth;

          return (
            <button
              key={i}
              type="button"
              disabled={isDisabled}
              onClick={() => onSelect(i)}
              className={cn(
                'rounded-md py-2.5 text-sm transition-colors cursor-pointer',
                'hover:bg-(--color-surface-secondary)',
                isCurrent && 'font-semibold text-(--color-primary)',
                isDisabled && 'opacity-30 cursor-not-allowed hover:bg-transparent',
              )}
            >
              {label}
            </button>
          );
        })}
      </div>
    </div>
  );
}

// ─── 年份选择视图 ───────────────────────────────────────

interface YearsViewProps {
  viewDate: Date;
  startMonth?: Date;
  endMonth?: Date;
  navBtnClass: string;
  onSelect: (year: number) => void;
  onGroupChange: (delta: number) => void;
}

function YearsView({
  viewDate,
  startMonth,
  endMonth,
  navBtnClass,
  onSelect,
  onGroupChange,
}: YearsViewProps) {
  const year = viewDate.getFullYear();
  const decadeStart = Math.floor(year / 10) * 10;
  const currentYear = new Date().getFullYear();

  const canPrev = !startMonth || decadeStart > startMonth.getFullYear();
  const canNext = !endMonth || decadeStart + 9 < endMonth.getFullYear();

  return (
    <div className="flex flex-col gap-4" style={{ minWidth: 'calc(7 * var(--cell-size))' }}>
      <div className="relative flex h-(--cell-size) items-center justify-between">
        <button
          type="button"
          disabled={!canPrev}
          onClick={() => onGroupChange(-1)}
          className={navBtnClass}
        >
          <ChevronLeftIcon className="size-4" />
        </button>
        <span className="text-sm font-medium text-text-primary select-none">
          {decadeStart} - {decadeStart + 9}
        </span>
        <button
          type="button"
          disabled={!canNext}
          onClick={() => onGroupChange(1)}
          className={navBtnClass}
        >
          <ChevronRightIcon className="size-4" />
        </button>
      </div>

      <div className="grid grid-cols-4 gap-y-1 gap-x-1">
        {Array.from({ length: 12 }, (_, i) => {
          const y = decadeStart + i;
          if (i >= 10) return <div key={i} />;

          const isDisabled =
            (startMonth != null && y < startMonth.getFullYear()) ||
            (endMonth != null && y > endMonth.getFullYear());
          const isCurrent = y === currentYear;

          return (
            <button
              key={i}
              type="button"
              disabled={isDisabled}
              onClick={() => onSelect(y)}
              className={cn(
                'rounded-md py-2.5 text-sm transition-colors cursor-pointer',
                'hover:bg-(--color-surface-secondary)',
                isCurrent && 'font-semibold text-(--color-primary)',
                isDisabled && 'opacity-30 cursor-not-allowed hover:bg-transparent',
              )}
            >
              {y}
            </button>
          );
        })}
      </div>
    </div>
  );
}

// ─── CalendarDayButton ──────────────────────────────────

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
        'data-[today=true]:relative data-[today=true]:z-[1] data-[today=true]:ring-1 data-[today=true]:ring-(--color-primary) data-[today=true]:font-semibold',
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
