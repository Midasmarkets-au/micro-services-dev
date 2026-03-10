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
    <div className="relative flex w-full items-center justify-between overflow-x-auto rounded p-3 md:p-5">
      {/* 连接线 */}
      <div
        className="absolute top-8 md:top-10 h-px"
        style={{
          left: 'calc(24px + 16px)',
          right: 'calc(24px + 16px)',
        }}
      >
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
            className="relative z-10 flex w-12 md:w-16 shrink-0 flex-col items-center gap-1"
          >
            <div
              className={`step-circle relative flex size-8 md:size-10 items-center justify-center overflow-hidden rounded-full ${
                isActive ? 'step-circle-active' : 'step-circle-pending'
              }`}
            >
              <div
                className={`step-circle-inner absolute inset-0.5 rounded-full border ${
                  isActive ? 'step-circle-inner-active' : 'step-circle-inner-pending'
                }`}
              />
              {isCompleted ? (
                <CheckIcon className="relative z-10 size-5 md:size-6 step-number-active" />
              ) : (
                <span
                  className={`step-number relative z-10 text-lg md:text-2xl font-bold capitalize leading-none ${
                    isActive ? 'step-number-active' : 'step-number-pending'
                  }`}
                  style={{ fontFamily: 'DIN, sans-serif' }}
                >
                  {step.number}
                </span>
              )}
            </div>

            <span
              className={`step-label text-xs md:text-base whitespace-nowrap ${
                isActive ? 'step-label-active' : 'step-label-pending'
              }`}
            >
              {step.label}
            </span>
          </div>
        );
      })}
    </div>
  );
}
