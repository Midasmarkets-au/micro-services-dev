'use client';

import { useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/radix/Dialog';
import { Button } from '@/components/ui/radix/Button';
import { Input } from '@/components/ui/radix/Input';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import { SelectInput } from '@/components/ui/SelectInput';
import { getRegionCodes } from '@/core/data/phonesData';
import type { AddressInfo, AddressContent } from '@/actions/address';

interface AddressModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: AddressFormData) => Promise<void>;
  initialData?: AddressInfo | null;
  mode: 'add' | 'edit';
}

export interface AddressFormData {
  name: string;
  ccc: string;
  phone: string;
  country: string;
  content: AddressContent;
}


export function AddressModal({
  isOpen,
  onClose,
  onSubmit,
  initialData,
  mode,
}: AddressModalProps) {
  const t = useTranslations('profile.address');
  const tCommon = useTranslations('common');

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Form state
  const [formData, setFormData] = useState<AddressFormData>({
    name: '',
    ccc: '61',
    phone: '',
    country: 'au',
    content: {
      address: '',
      city: '',
      postalCode: '',
      socialMediaType: 'whatsApp',
      socialMediaAccount: '',
      state: '',
    },
  });

  // Initialize form data when modal opens
  useEffect(() => {
    if (isOpen) {
      if (mode === 'edit' && initialData) {
        setFormData({
          name: initialData.name,
          ccc: initialData.ccc,
          phone: initialData.phone,
          country: initialData.country,
          content: initialData.content,
        });
      } else {
        // Reset form for add mode
        setFormData({
          name: '',
          ccc: '61',
          phone: '',
          country: 'au',
          content: {
            address: '',
            city: '',
            postalCode: '',
            socialMediaType: 'whatsApp',
            socialMediaAccount: '',
            state: '',
          },
        });
      }
      setErrors({});
    }
  }, [isOpen, mode, initialData]);

  // Get country options for SearchableSelect
  const countryOptions = Object.entries(getRegionCodes()).map(([code, data]) => ({
    value: code,
    label: data.name,
    dialCode: data.dialCode,
  }));

  // Phone code options for SelectInput (value 格式为 "+86")
  const phoneCodeOptions = countryOptions.map((opt) => ({
    value: `+${opt.dialCode}`,
    label: `${opt.label} +${opt.dialCode}`,
    code: opt.value,
  }));

  // Social media type options for SelectInput
  const socialMediaOptions = [
    { value: 'whatsApp', label: 'WhatsApp' },
    { value: 'telegram', label: 'Telegram' },
    { value: 'wechat', label: 'WeChat' },
    { value: 'line', label: 'Line' },
    { value: 'other', label: 'Other' },
  ];

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = t('errors.nameRequired');
    }
    if (!formData.phone.trim()) {
      newErrors.phone = t('errors.phoneRequired');
    }
    if (!formData.content.socialMediaAccount.trim()) {
      newErrors.socialMediaAccount = t('errors.socialMediaRequired');
    }
    if (!formData.country) {
      newErrors.country = t('errors.countryRequired');
    }
    if (!formData.content.state.trim()) {
      newErrors.state = t('errors.stateRequired');
    }
    if (!formData.content.city.trim()) {
      newErrors.city = t('errors.cityRequired');
    }
    if (!formData.content.address.trim()) {
      newErrors.address = t('errors.addressRequired');
    }
    if (!formData.content.postalCode.trim()) {
      newErrors.postalCode = t('errors.postalCodeRequired');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;

    setLoading(true);
    try {
      await onSubmit(formData);
      onClose();
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      onClose();
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader className="flex-row items-center justify-between">
          <DialogTitle className="text-xl font-semibold text-text-primary">
            {mode === 'add' ? t('addAddress') : t('editAddress')}
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-5 pt-10">
          {/* Row 1: Name + Phone (区号+号码组合) */}
          <div className="flex flex-col sm:flex-row gap-5">
            {/* Name - 使用 Input 组件 */}
            <div className="flex-1">
              <Input
                label={t('fields.name')}
                required
                value={formData.name}
                onChange={(e) => setFormData((prev) => ({ ...prev, name: e.target.value }))}
                error={errors.name}
                placeholder={t('placeholders.name')}
              />
            </div>

            {/* Phone - 使用 SelectInput 组件 */}
            <div className="flex-1">
              <SelectInput
                label={t('fields.phone')}
                selectValue={`+${formData.ccc}`}
                inputValue={formData.phone}
                selectOptions={phoneCodeOptions}
                onSelectChange={(value) => setFormData((prev) => ({ ...prev, ccc: value.replace('+', '') }))}
                onInputChange={(value) => setFormData((prev) => ({ ...prev, phone: value }))}
                error={errors.phone}
                placeholder={t('placeholders.phone')}
                selectWidth="110px"
                inputType="tel"
              />
            </div>
          </div>

          {/* Row 2: Social Media (组合输入) + Country */}
          <div className="flex flex-col sm:flex-row gap-5">
            {/* 左侧: 社交媒体 - 使用 SelectInput 组件 */}
            <div className="flex-1">
              <SelectInput
                label={t('fields.socialMedia')}
                selectValue={formData.content.socialMediaType}
                inputValue={formData.content.socialMediaAccount}
                selectOptions={socialMediaOptions}
                onSelectChange={(value) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: { ...prev.content, socialMediaType: value },
                  }))
                }
                onInputChange={(value) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: { ...prev.content, socialMediaAccount: value },
                  }))
                }
                error={errors.socialMediaAccount}
                placeholder={t('placeholders.socialMedia')}
                selectWidth="100px"
                dropdownWidth="200px"
              />
            </div>

            {/* 右侧: 国家 */}
            <div className="flex-1">
              <SearchableSelect
                label={t('fields.country')}
                required
                value={countryOptions.find((opt) => opt.value === formData.country) || null}
                onChange={(option) => {
                  const selected = option as { value: string; label: string } | null;
                  if (selected) {
                    setFormData((prev) => ({ ...prev, country: selected.value }));
                  }
                }}
                options={countryOptions}
                placeholder={t('placeholders.country')}
                error={errors.country}
              />
            </div>
          </div>

          {/* Row 3: State + City - 使用 Input 组件 */}
          <div className="flex flex-col sm:flex-row gap-5">
            <div className="flex-1">
              <Input
                label={t('fields.state')}
                required
                value={formData.content.state}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: { ...prev.content, state: e.target.value },
                  }))
                }
                error={errors.state}
                placeholder={t('placeholders.state')}
              />
            </div>
            <div className="flex-1">
              <Input
                label={t('fields.city')}
                required
                value={formData.content.city}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: { ...prev.content, city: e.target.value },
                  }))
                }
                error={errors.city}
                placeholder={t('placeholders.city')}
              />
            </div>
          </div>

          {/* Row 4: Address + Postal Code - 使用 Input 组件 */}
          <div className="flex flex-col sm:flex-row gap-5">
            <div className="flex-1">
              <Input
                label={t('fields.address')}
                required
                value={formData.content.address}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: { ...prev.content, address: e.target.value },
                  }))
                }
                error={errors.address}
                placeholder={t('placeholders.address')}
              />
            </div>
            <div className="flex-1">
              <Input
                label={t('fields.postalCode')}
                required
                value={formData.content.postalCode}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: { ...prev.content, postalCode: e.target.value },
                  }))
                }
                error={errors.postalCode}
                placeholder={t('placeholders.postalCode')}
              />
            </div>
          </div>
        </div>

        <DialogFooter className="flex flex-row gap-5 justify-end mt-10">
          <Button
            variant="secondary"
            onClick={handleClose}
            disabled={loading}
            className="w-[100px] sm:w-[120px]"
          >
            {tCommon('cancel')}
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={loading}
            className="w-[100px] sm:w-[120px]"
          >
            {tCommon('submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
