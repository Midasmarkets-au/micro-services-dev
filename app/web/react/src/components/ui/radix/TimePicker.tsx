'use client';

import * as React from 'react';
import { ClockIcon, ChevronUpIcon, ChevronDownIcon } from '@radix-ui/react-icons';
import * as Popover from '@radix-ui/react-popover';
import { cn } from '@/lib/utils';

interface TimePickerProps {
  value?: string; // HH:mm 格式
  onChange?: (time: string) => void;
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  use24Hour?: boolean;
  className?: string;
}

export function TimePicker({
  value,
  onChange,
  placeholder = '选择时间',
  disabled = false,
  error = false,
  use24Hour = true,
  className,
}: TimePickerProps) {
  const [open, setOpen] = React.useState(false);
  
  // 解析当前时间值
  const parseTime = (timeStr?: string) => {
    if (!timeStr) {
      const now = new Date();
      return {
        hour: now.getHours(),
        minute: now.getMinutes(),
      };
    }
    const [h, m] = timeStr.split(':').map(Number);
    return { hour: h || 0, minute: m || 0 };
  };

  const [selectedTime, setSelectedTime] = React.useState(() => parseTime(value));

  React.useEffect(() => {
    if (value) {
      setSelectedTime(parseTime(value));
    }
  }, [value]);

  // 格式化显示时间
  const formatTime = (hour: number, minute: number) => {
    if (use24Hour) {
      return `${hour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')}`;
    }
    const period = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour % 12 || 12;
    return `${displayHour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')} ${period}`;
  };

  // 更新时间
  const updateTime = (hour: number, minute: number) => {
    const newHour = Math.max(0, Math.min(23, hour));
    const newMinute = Math.max(0, Math.min(59, minute));
    setSelectedTime({ hour: newHour, minute: newMinute });
  };

  // 确认选择
  const handleConfirm = () => {
    const timeStr = `${selectedTime.hour.toString().padStart(2, '0')}:${selectedTime.minute.toString().padStart(2, '0')}`;
    onChange?.(timeStr);
    setOpen(false);
  };

  // 滚轮调整
  const handleWheel = (e: React.WheelEvent, type: 'hour' | 'minute') => {
    e.preventDefault();
    const delta = e.deltaY > 0 ? -1 : 1;
    if (type === 'hour') {
      updateTime(selectedTime.hour + delta, selectedTime.minute);
    } else {
      updateTime(selectedTime.hour, selectedTime.minute + delta);
    }
  };

  return (
    <Popover.Root open={open} onOpenChange={setOpen}>
      <Popover.Trigger asChild>
        <button
          type="button"
          disabled={disabled}
          className={cn(
            'flex h-12 w-full items-center gap-2 rounded-xl border px-3 py-2 text-left text-sm',
            'bg-input-bg text-text-primary',
            'border-border focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary',
            'disabled:cursor-not-allowed disabled:opacity-50',
            'transition-colors duration-200',
            error && 'border-error-border',
            !value && 'text-text-placeholder',
            className
          )}
        >
          <ClockIcon className="size-5 shrink-0 text-text-secondary" />
          <span className="flex-1 truncate">
            {value ? formatTime(selectedTime.hour, selectedTime.minute) : placeholder}
          </span>
        </button>
      </Popover.Trigger>

      <Popover.Portal>
        <Popover.Content
          className={cn(
            'z-50 w-[280px] rounded-xl border p-4',
            'bg-surface border-border',
            'shadow-dropdown',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[side=bottom]:slide-in-from-top-2 data-[side=top]:slide-in-from-bottom-2'
          )}
          sideOffset={4}
          align="start"
        >
          {/* 时间滚轮选择器 */}
          <div className="flex items-center justify-center gap-4">
            {/* 小时选择 */}
            <div className="flex flex-col items-center">
              <span className="mb-2 text-xs text-text-secondary">时</span>
              <div
                className="flex flex-col items-center"
                onWheel={(e) => handleWheel(e, 'hour')}
              >
                <button
                  type="button"
                  onClick={() => updateTime(selectedTime.hour + 1, selectedTime.minute)}
                  className="flex size-10 items-center justify-center rounded-lg text-text-secondary hover:bg-surface-secondary transition-colors"
                >
                  <ChevronUpIcon className="size-5" />
                </button>
                
                <div className="relative flex h-16 w-20 items-center justify-center">
                  <div className="absolute inset-0 rounded-xl bg-primary/10" />
                  <span className="relative z-10 text-2xl font-semibold text-text-primary">
                    {selectedTime.hour.toString().padStart(2, '0')}
                  </span>
                </div>

                <button
                  type="button"
                  onClick={() => updateTime(selectedTime.hour - 1, selectedTime.minute)}
                  className="flex size-10 items-center justify-center rounded-lg text-text-secondary hover:bg-surface-secondary transition-colors"
                >
                  <ChevronDownIcon className="size-5" />
                </button>
              </div>
            </div>

            {/* 分隔符 */}
            <div className="flex h-16 items-center">
              <span className="text-2xl font-semibold text-text-primary">:</span>
            </div>

            {/* 分钟选择 */}
            <div className="flex flex-col items-center">
              <span className="mb-2 text-xs text-text-secondary">分</span>
              <div
                className="flex flex-col items-center"
                onWheel={(e) => handleWheel(e, 'minute')}
              >
                <button
                  type="button"
                  onClick={() => updateTime(selectedTime.hour, selectedTime.minute + 1)}
                  className="flex size-10 items-center justify-center rounded-lg text-text-secondary hover:bg-surface-secondary transition-colors"
                >
                  <ChevronUpIcon className="size-5" />
                </button>
                
                <div className="relative flex h-16 w-20 items-center justify-center">
                  <div className="absolute inset-0 rounded-xl bg-primary/10" />
                  <span className="relative z-10 text-2xl font-semibold text-text-primary">
                    {selectedTime.minute.toString().padStart(2, '0')}
                  </span>
                </div>

                <button
                  type="button"
                  onClick={() => updateTime(selectedTime.hour, selectedTime.minute - 1)}
                  className="flex size-10 items-center justify-center rounded-lg text-text-secondary hover:bg-surface-secondary transition-colors"
                >
                  <ChevronDownIcon className="size-5" />
                </button>
              </div>
            </div>
          </div>

          {/* 快捷时间选项 */}
          <div className="mt-4 flex flex-wrap gap-2">
            {['00:00', '06:00', '12:00', '18:00'].map((time) => (
              <button
                key={time}
                type="button"
                onClick={() => {
                  const [h, m] = time.split(':').map(Number);
                  updateTime(h, m);
                }}
                className={cn(
                  'rounded-lg px-3 py-1.5 text-xs transition-colors',
                  'bg-surface-secondary text-text-secondary',
                  'hover:bg-primary/20 hover:text-primary'
                )}
              >
                {time}
              </button>
            ))}
          </div>

          {/* 操作按钮 */}
          <div className="mt-4 flex gap-2">
            <button
              type="button"
              onClick={() => setOpen(false)}
              className={cn(
                'flex-1 rounded-lg py-2 text-sm font-medium transition-colors',
                'bg-surface-secondary text-text-primary',
                'hover:bg-surface-secondary/80'
              )}
            >
              取消
            </button>
            <button
              type="button"
              onClick={handleConfirm}
              className={cn(
                'flex-1 rounded-lg py-2 text-sm font-medium transition-colors',
                'bg-primary text-white',
                'hover:bg-primary-hover'
              )}
            >
              确认
            </button>
          </div>
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}
