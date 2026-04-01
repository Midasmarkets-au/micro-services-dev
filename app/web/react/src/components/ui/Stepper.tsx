'use client';

import { CheckIcon } from '@radix-ui/react-icons';

export interface StepItem {
  id: string;
  label: string;
  number: number;
}

export type StepStatus = 'completed' | 'current' | 'pending';

interface StepperProps {
  steps: StepItem[];
  currentStep: string;
  completedSteps: string[];
}

export function Stepper({ steps, currentStep, completedSteps }: StepperProps) {
  const getStepStatus = (stepId: string): StepStatus => {
    if (completedSteps.includes(stepId)) return 'completed';
    if (currentStep === stepId) return 'current';
    return 'pending';
  };

  const currentIndex = steps.findIndex((s) => s.id === currentStep);

  return (
    <div className="w-full rounded p-3 md:p-5">
      {/* 圆圈行：连接线相对此行垂直居中，不依赖外部布局 */}
      <div className="relative flex items-center justify-between">
        <div className="absolute left-6 right-6 top-1/2 h-px -translate-y-1/2 md:left-8 md:right-8">
          <div className="absolute inset-0 bg-border" />
          <div
            className="absolute left-0 top-0 h-full bg-primary transition-all duration-300"
            style={{
              width:
                currentIndex >= steps.length - 1
                  ? '100%'
                  : `${(currentIndex / (steps.length - 1)) * 100}%`,
            }}
          />
        </div>

        {steps.map((step) => {
          const status = getStepStatus(step.id);
          const isCompleted = status === 'completed';
          const isCurrent = status === 'current';
          const isActive = isCompleted || isCurrent;

          return (
            <div
              key={step.id}
              className="relative z-10 flex w-12 shrink-0 justify-center md:w-16"
            >
              <div
                className={`step-circle relative flex size-8 items-center justify-center overflow-hidden rounded-full md:size-10 ${
                  isActive ? 'step-circle-active' : 'step-circle-pending'
                }`}
              >
                <div
                  className={`step-circle-inner absolute inset-0.5 rounded-full border ${
                    isActive ? 'step-circle-inner-active' : 'step-circle-inner-pending'
                  }`}
                />
                {isCompleted ? (
                  <CheckIcon className="step-number-active relative z-10 size-5 md:size-6" />
                ) : (
                  <span
                    className={`step-number relative z-10 text-lg font-bold capitalize leading-none md:text-2xl ${
                      isActive ? 'step-number-active' : 'step-number-pending'
                    }`}
                    style={{ fontFamily: 'DIN, sans-serif' }}
                  >
                    {step.number}
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* 标签行 */}
      <div className="mt-1 flex justify-between">
        {steps.map((step) => {
          const status = getStepStatus(step.id);
          const isActive = status === 'completed' || status === 'current';

          return (
            <span
              key={step.id}
              className={`step-label w-12 shrink-0 text-center text-xs whitespace-nowrap md:w-16 md:text-base ${
                isActive ? 'step-label-active' : 'step-label-pending'
              }`}
            >
              {step.label}
            </span>
          );
        })}
      </div>
    </div>
  );
}
