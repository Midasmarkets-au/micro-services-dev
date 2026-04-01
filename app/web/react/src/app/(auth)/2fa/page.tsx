'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { AuthIllustration } from '@/components/layout';
import {
  Button,
  Input,
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui';
import { useServerAction } from '@/hooks/useServerAction';
import { verify2FA } from '@/actions';

type DialogType = 'success' | 'error';

export default function TwoFAPage() {
  const router = useRouter();
  const tAuth = useTranslations('auth');
  const tSecurity = useTranslations('profile.security');
  const tCommon = useTranslations('common');
  const { execute, isLoading } = useServerAction({ showErrorToast: false });
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogType, setDialogType] = useState<DialogType>('success');
  const [dialogMessage, setDialogMessage] = useState('');

  const schema = z.object({
    code: z.string().length(6, tAuth('twoFactorRequired')),
  });

  type FormData = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      code: '',
    },
  });

  const onSubmit = async (data: FormData) => {
    const result = await execute(verify2FA, { code: data.code });

    if (result.success) {
      setDialogType('success');
      setDialogMessage(tAuth('loginSuccess'));
      setDialogOpen(true);
      return;
    }

    setDialogType('error');
    setDialogMessage(result.error || tAuth('verificationFailed'));
    setDialogOpen(true);
  };

  const handleDialogConfirm = () => {
    setDialogOpen(false);
    if (dialogType === 'success') {
      router.push('/dashboard');
    }
  };

  return (
    <>
      <div className="card auth-card">
        <div className="auth-illustration-container">
          <AuthIllustration />
        </div>

        <div className="auth-card-form flex flex-col">
          <h1 className="mb-8 text-center text-responsive-2xl font-semibold text-text-primary">
            {tAuth('twoFactorAuthentication')}
          </h1>

          <div className="mb-6 flex justify-center md:hidden">
            <AuthIllustration size={140} />
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
            <Input
              label={tSecurity('twoFactorCode')}
              type="text"
              error={errors.code?.message}
              autoComplete="one-time-code"
              inputSize="md"
              {...register('code')}
            />

            <p className="text-sm text-text-secondary">
              {tAuth('pleaseEnterTwoFactorCode')}
            </p>

            <div className="mt-4">
              <Button type="submit" className="w-full" loading={isSubmitting || isLoading}>
                {tAuth('confirm')}
              </Button>
            </div>
          </form>
        </div>
      </div>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {dialogType === 'success' ? tAuth('loginSuccess') : tAuth('verificationFailed')}
            </DialogTitle>
            <DialogDescription>{dialogMessage}</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button onClick={handleDialogConfirm}>{tCommon('confirm')}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
