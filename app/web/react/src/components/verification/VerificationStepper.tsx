'use client';

import { useTranslations } from 'next-intl';
import { Stepper } from '@/components/ui/Stepper';
import type { StepItem } from '@/components/ui/Stepper';

export type MainStep = 'started' | 'info' | 'agreement' | 'document' | 'complete';

const mainStepDefs: { id: MainStep; labelKey: string; number: number }[] = [
  { id: 'started', labelKey: 'gettingStarted', number: 1 },
  { id: 'info', labelKey: 'personalInfo', number: 2 },
  { id: 'agreement', labelKey: 'agreement', number: 3 },
  { id: 'document', labelKey: 'documents', number: 4 },
];

interface VerificationStepperProps {
  currentStep: MainStep;
  completedSteps: MainStep[];
}

export function VerificationStepper({ currentStep, completedSteps }: VerificationStepperProps) {
  const t = useTranslations('verification');
  const normalizedCurrentStep = currentStep === 'complete' ? 'document' : currentStep;

  const steps: StepItem[] = mainStepDefs.map((s) => ({
    id: s.id,
    label: t(`mainSteps.${s.labelKey}`),
    number: s.number,
  }));

  return (
    <Stepper
      steps={steps}
      currentStep={normalizedCurrentStep}
      completedSteps={completedSteps}
    />
  );
}
