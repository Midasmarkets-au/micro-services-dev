'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { registerEvent, getUserInfo } from '@/actions';
import { useUserStore } from '@/stores/userStore';
import { Button, Checkbox } from '@/components/ui';
import type { UserInfo } from '@/types/user';
import type { EventDetail } from '@/types/eventshop';

interface RegistrationCardProps {
  eventDetail: EventDetail;
  onRegistered: () => void;
}

export function RegistrationCard({ eventDetail, onRegistered }: RegistrationCardProps) {
  const t = useTranslations('eventshop');
  const { execute } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();
  const setUser = useUserStore((s) => s.setUser);
  const [agreed, setAgreed] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);

  const handleRegister = async () => {
    if (!agreed) return;
    setIsRegistering(true);
    try {
      const result = await execute(registerEvent, 'EventShop');
      if (result.success) {
        showSuccess(t('registration.success'));
        const userResult = await execute(getUserInfo);
        if (userResult.success && userResult.data) {
          setUser(userResult.data as UserInfo);
        }
        onRegistered();
      }
    } finally {
      setIsRegistering(false);
    }
  };

  return (
    <div className="flex flex-col gap-5 w-full">
      {eventDetail.title && (
        <h2 className="text-xl font-semibold text-text-primary">
          {eventDetail.title}
        </h2>
      )}

      {eventDetail.term && (
        <div className="bg-surface rounded border border-border p-5 md:p-8">
          <div
            className="max-w-none text-text-primary text-sm leading-relaxed [&_a]:text-primary [&_a]:underline [&_h1]:text-xl [&_h1]:font-semibold [&_h1]:mb-3 [&_h2]:text-lg [&_h2]:font-semibold [&_h2]:mb-2 [&_h3]:text-base [&_h3]:font-medium [&_h3]:mb-2 [&_p]:mb-2 [&_ul]:list-disc [&_ul]:pl-5 [&_ol]:list-decimal [&_ol]:pl-5 [&_li]:mb-1 [&_table]:w-full [&_table]:border-collapse [&_td]:border [&_td]:border-border [&_td]:p-2 [&_th]:border [&_th]:border-border [&_th]:p-2 [&_th]:bg-surface-secondary"
            dangerouslySetInnerHTML={{ __html: eventDetail.term }}
          />
        </div>
      )}

      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 bg-surface rounded border border-border p-5">
        <label className="flex items-center gap-2 cursor-pointer select-none">
          <Checkbox
            checked={agreed}
            onCheckedChange={(checked) => setAgreed(checked === true)}
          />
          <span className="text-sm text-text-primary">
            {t('registration.agree')}
          </span>
        </label>

        <Button
          variant="primary"
          size="md"
          disabled={!agreed}
          loading={isRegistering}
          onClick={handleRegister}
          className="w-full sm:w-auto min-w-32"
        >
          {isRegistering ? t('registration.registering') : t('registration.register')}
        </Button>
      </div>
    </div>
  );
}
