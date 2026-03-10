'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { getEventDetail, getEventUserDetail } from '@/actions';
import { useUserStore } from '@/stores/userStore';
import { RegistrationCard } from '@/components/eventshop/RegistrationCard';
import { EventShopContent } from '@/components/eventshop/EventShopContent';
import { EventShopSkeleton } from '@/components/eventshop/EventShopSkeleton';
import { EventPartyStatusTypes } from '@/types/eventshop';
import type { EventDetail, EventUserDetail } from '@/types/eventshop';

export default function EventShopPage() {
  const t = useTranslations('eventshop');
  const { execute } = useServerAction({ showErrorToast: true });

  const [isLoading, setIsLoading] = useState(true);
  const [eventDetail, setEventDetail] = useState<EventDetail | null>(null);
  const [userDetail, setUserDetail] = useState<EventUserDetail | null>(null);
  const [step, setStep] = useState(0);
  const isLoadedRef = useRef(false);

  const fetchData = useCallback(async (forceRegistered = false) => {
    setIsLoading(true);
    try {
      const eventResult = await execute(getEventDetail, 'EventShop');
      if (eventResult.success && eventResult.data) {
        setEventDetail(eventResult.data);
      }

      const hasEventShopRole =
        forceRegistered || useUserStore.getState().hasRole('EventShop');

      if (hasEventShopRole) {
        setStep(EventPartyStatusTypes.Applied);

        const userResult = await getEventUserDetail();
        if (userResult.success && userResult.data) {
          setUserDetail(userResult.data);
          if (userResult.data.status !== undefined) {
            setStep(userResult.data.status);
          }
        }
      } else {
        setStep(0);
      }
    } finally {
      setIsLoading(false);
    }
  }, [execute]);

  useEffect(() => {
    if (isLoadedRef.current) return;
    isLoadedRef.current = true;
    fetchData();
  }, [fetchData]);

  const handleRegistered = () => {
    isLoadedRef.current = false;
    fetchData(true);
  };

  if (isLoading) {
    return (
      <div className="flex flex-col gap-6 w-full">
        <EventShopSkeleton />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 w-full">
      {step === 0 && eventDetail ? (
        <RegistrationCard
          eventDetail={eventDetail}
          onRegistered={handleRegistered}
        />
      ) : step !== 0 && eventDetail ? (
        <EventShopContent
          eventDetail={eventDetail}
          userDetail={userDetail}
          step={step}
        />
      ) : (
        <div className="flex items-center justify-center py-20">
          <p className="text-text-secondary">{t('shop.loading')}</p>
        </div>
      )}
    </div>
  );
}
