'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import Image from 'next/image';
import { Button } from '@/components/ui';
// EditIcon 内联组件 (SVGR 需要重启服务器生效)
const EditIcon = ({ className }: { className?: string }) => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" className={className}>
    <path d="M11.705 14.8413H4.07225C2.46425 14.8413 1.15625 13.5333 1.15625 11.9253V4.55174C1.15625 2.94374 2.46425 1.63574 4.07225 1.63574H7.93545C8.24505 1.63574 8.49545 1.88614 8.49545 2.19574C8.49545 2.50534 8.24505 2.75574 7.93545 2.75574H4.07225C3.08185 2.75574 2.27625 3.56134 2.27625 4.55174V11.9253C2.27625 12.9157 3.08185 13.7213 4.07225 13.7213H11.705C12.6954 13.7213 13.501 12.9157 13.501 11.9253V8.27414C13.501 7.96454 13.7514 7.71414 14.061 7.71414C14.3706 7.71414 14.621 7.96454 14.621 8.27414V11.9253C14.621 13.5333 13.313 14.8413 11.705 14.8413Z" />
    <path d="M6.38932 10.6212C6.11812 10.6212 5.85652 10.5116 5.66452 10.3116C5.54929 10.192 5.46563 10.0455 5.42108 9.88548C5.37654 9.72544 5.37249 9.55683 5.40932 9.39484L5.78612 7.71644C5.82772 7.53004 5.92132 7.36044 6.05652 7.22524L11.9477 1.33404C12.3397 0.942039 12.9781 0.942039 13.3701 1.33404L14.7381 2.70204C15.1301 3.09404 15.1301 3.73244 14.7381 4.12444L8.85412 10.0084C8.70941 10.1534 8.52374 10.2506 8.32212 10.2868L6.57092 10.6044C6.50932 10.6164 6.44932 10.6212 6.38932 10.6212ZM6.87092 7.99484L6.53972 9.47244L8.08612 9.19164L13.8637 3.41404L12.6581 2.20844L6.87092 7.99484Z" />
  </svg>
);
import { AddressModal, type AddressFormData } from '../components/AddressModal';
import {
  getAddressList,
  createAddress,
  updateAddress,
  type AddressInfo,
} from '@/actions';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { getDataByCode } from '@/core/data/phonesData';

export default function AddressPage() {
  const t = useTranslations('profile.address');
  const locale = useLocale();
  const { execute } = useServerAction({ showErrorToast: true });
  const { showSuccess } = useToast();

  // 中文环境下使用字间距，英文环境不使用
  const labelTracking = locale === 'zh' ? 'tracking-[14px]' : '';

  const [loading, setLoading] = useState(true);
  const [addresses, setAddresses] = useState<AddressInfo[]>([]);

  // Modal states
  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'add' | 'edit'>('add');
  const [selectedAddress, setSelectedAddress] = useState<AddressInfo | null>(null);

  // Fetch address list
  const fetchAddresses = useCallback(async () => {
    setLoading(true);
    const result = await execute(getAddressList);
    if (result.success && result.data) {
      setAddresses(result.data);
    }
    setLoading(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    fetchAddresses();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Handle add address
  const handleAddAddress = () => {
    setModalMode('add');
    setSelectedAddress(null);
    setModalOpen(true);
  };

  // Handle edit
  const handleEdit = (address: AddressInfo) => {
    setModalMode('edit');
    setSelectedAddress(address);
    setModalOpen(true);
  };

  // Handle form submit
  const handleFormSubmit = async (formData: AddressFormData) => {
    const payload = {
      name: formData.name,
      ccc: Number(formData.ccc),
      phone: formData.phone,
      country: formData.country,
      content: JSON.stringify(formData.content),
    };

    if (modalMode === 'add') {
      const result = await execute(createAddress, payload);
      if (result.success) {
        showSuccess(t('addSuccess'));
        fetchAddresses();
        setModalOpen(false);
      }
    } else if (selectedAddress) {
      const updatePayload = {
        ...payload,
        hashId: selectedAddress.hashId,
        createdOn: selectedAddress.createdOn,
        updatedOn: selectedAddress.updatedOn,
      };
      const result = await execute(updateAddress, selectedAddress.hashId, updatePayload);
      if (result.success) {
        showSuccess(t('updateSuccess'));
        fetchAddresses();
        setModalOpen(false);
      }
    }
  };

  // Get country name by code
  const getCountryName = (code: string) => {
    const data = getDataByCode(code);
    return data?.name || code.toUpperCase();
  };

  // Render address card
  const renderAddressCard = (address: AddressInfo, index: number) => {
    const content = address.content;
    const countryName = getCountryName(address.country);
    const phoneCode = address.ccc;
    const isFirst = index === 0;

    return (
      <div
        key={address.hashId}
        className={`relative flex flex-col gap-5 rounded-xl border border-border bg-surface p-6 sm:p-10 overflow-hidden ${
          isFirst ? 'shadow-[inset_0px_4px_0px_0px_var(--color-primary)]' : ''
        }`}
      >
        {/* Header: Location info + Edit button */}
        <div className="flex items-start justify-between">
          <div className="flex flex-1 gap-2">
            {/* Location icon */}
            <div className="shrink-0 mt-0.5">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M10 10.625C11.3807 10.625 12.5 9.50571 12.5 8.125C12.5 6.74429 11.3807 5.625 10 5.625C8.61929 5.625 7.5 6.74429 7.5 8.125C7.5 9.50571 8.61929 10.625 10 10.625Z" stroke="var(--color-primary)" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                <path d="M10 1.875C8.34087 1.875 6.74961 2.53385 5.57651 3.70695C4.40341 4.88005 3.74456 6.47131 3.74456 8.13044C3.74456 9.56544 4.05831 10.5017 4.99456 11.6929L10 18.125L15.0054 11.6929C15.9417 10.5017 16.2554 9.56544 16.2554 8.13044C16.2554 6.47131 15.5966 4.88005 14.4235 3.70695C13.2504 2.53385 11.6591 1.875 10 1.875Z" stroke="var(--color-primary)" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </div>
            {/* Location details */}
            <div className="flex flex-col gap-2.5 flex-1 min-w-0">
              {/* Region info */}
              <div className="flex flex-wrap items-center gap-2 text-sm text-text-secondary">
                <span>{countryName}</span>
                {content.state && <span>{content.state}</span>}
                {content.city && <span>{content.city}</span>}
              </div>
              {/* Address */}
              <p className="text-xl font-semibold text-text-primary truncate">
                {content.address}
              </p>
            </div>
          </div>
          {/* Edit button */}
          <Button
            onClick={() => handleEdit(address)}
            size="icon"
            className="rounded-full bg-surface-secondary border-border w-10 h-10 drop-shadow-[0_3.657px_6.857px_rgba(51,51,51,0.20)] text-text-primary"
          >
            <EditIcon className="w-4 h-4" />
          </Button>
        </div>

        {/* Divider */}
        <div className="h-px w-full bg-border" />

        {/* Info section */}
        <div className="flex flex-col gap-2">
          <div className="flex items-center gap-2 text-sm text-text-secondary">
            <span className={labelTracking}>{t('fields.nameLabel')}</span>
            <span>{address.name}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-text-secondary">
            <span className={labelTracking}>{t('fields.phoneLabel')}</span>
            <span>+{phoneCode} {address.phone}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-text-secondary">
            <span className={labelTracking}>{t('fields.codeLabel')}</span>
            <span>{content.postalCode}</span>
          </div>
        </div>
      </div>
    );
  };

  // Render skeleton card
  const renderSkeletonCard = () => (
    <div className="flex flex-col gap-5 rounded-xl border border-border bg-surface p-6 sm:p-10 animate-pulse">
      <div className="flex items-start justify-between">
        <div className="flex flex-1 gap-2">
          <div className="size-5 rounded bg-border" />
          <div className="flex flex-col gap-2.5 flex-1">
            <div className="h-4 w-40 rounded bg-border" />
            <div className="h-6 w-60 rounded bg-border" />
          </div>
        </div>
        <div className="size-8 rounded-full bg-border" />
      </div>
      <div className="h-px w-full bg-border" />
      <div className="flex flex-col gap-2">
        <div className="h-4 w-32 rounded bg-border" />
        <div className="h-4 w-40 rounded bg-border" />
        <div className="h-4 w-24 rounded bg-border" />
      </div>
    </div>
  );

  return (
    <div className="flex flex-col gap-5 py-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <h2 className="text-xl font-semibold text-text-primary">
          {t('title')}
        </h2>
        {/* Action buttons */}
        <div className="flex items-center gap-5 sm:gap-10">
          <Button
            onClick={handleAddAddress}
            variant="primary"
            size="xs"
          >
            <Image
              src="/images/icons/add-plain.svg"
              alt="add"
              width={20}
              height={20}
            />
            <span>{t('addAddress')}</span>
          </Button>
        </div>
      </div>

      {/* Address list - 2 columns grid */}
      {loading ? (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          {[1, 2, 3, 4].map((i) => (
            <div key={i}>{renderSkeletonCard()}</div>
          ))}
        </div>
      ) : addresses.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-text-secondary">
          <Image
            src="/images/data/no-data-day.svg"
            alt="no data"
            width={120}
            height={120}
            className="dark:hidden"
          />
          <Image
            src="/images/data/no-data-night.svg"
            alt="no data"
            width={120}
            height={120}
            className="hidden dark:block"
          />
          <p className="mt-4 text-base">{t('noAddresses')}</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          {addresses.map((address, index) => renderAddressCard(address, index))}
        </div>
      )}

      {/* Address Modal */}
      <AddressModal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={handleFormSubmit}
        initialData={selectedAddress}
        mode={modalMode}
      />
    </div>
  );
}
