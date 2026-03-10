'use client';

import { useTranslations } from 'next-intl';

export type SubStep = 'personal' | 'identity' | 'social';

interface SubStepNavProps {
  /** 当前激活的子步骤（用于滚动同步高亮） */
  activeSubStep: SubStep;
  onSubStepClick?: (subStep: SubStep) => void;
}

const subSteps: { id: SubStep; number: string; labelKey: string }[] = [
  { id: 'personal', number: '01', labelKey: 'personalInfo' },
  { id: 'identity', number: '02', labelKey: 'identityDocument' },
  { id: 'social', number: '03', labelKey: 'socialMedia' },
];

export function SubStepNav({ activeSubStep, onSubStepClick }: SubStepNavProps) {
  const t = useTranslations('verification');

  const currentIndex = subSteps.findIndex((s) => s.id === activeSubStep);

  return (
    <div className="hidden md:flex w-fit shrink-0 gap-7.5">
      {/* 进度条背景 */}
      <div className="relative w-1 rounded bg-surface-secondary">
        {/* 高亮进度条 */}
        <div
          className="absolute left-0 top-0 w-1 rounded bg-primary transition-all duration-300"
          style={{
            height: `${((currentIndex + 1) / subSteps.length) * 100}%`,
          }}
        />
      </div>

      {/* 子步骤列表 */}
      <div className="flex flex-col gap-7">
        {subSteps.map((subStep) => {
          const isCurrent = activeSubStep === subStep.id;
          const isPast = subSteps.findIndex((s) => s.id === subStep.id) < currentIndex;

          return (
            <button
              key={subStep.id}
              type="button"
              onClick={() => onSubStepClick?.(subStep.id)}
              className="flex items-center gap-4 text-left whitespace-nowrap"
            >
              {/* 数字 */}
              <span
                className={`font-bold text-base ${
                  isCurrent || isPast ? 'text-text-secondary' : 'text-text-secondary'
                }`}
                style={{ fontFamily: 'DIN, sans-serif' }}
              >
                {subStep.number}
              </span>
              {/* 标签 */}
              <span
                className={`font-semibold text-base transition-colors duration-200 ${
                  isCurrent ? 'text-primary' : 'text-text-secondary'
                }`}
              >
                {t(`subSteps.${subStep.labelKey}`)}
              </span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
