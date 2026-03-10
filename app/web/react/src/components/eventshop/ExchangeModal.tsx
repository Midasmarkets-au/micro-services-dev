'use client';

import { useState, useEffect, useCallback } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import {
  getShopItemDetail,
  getMediaUrl,
  purchaseItem,
  getAddressList,
  createAddress,
  updateAddress,
} from '@/actions';
import type { ShopItem } from '@/types/eventshop';
import type { AddressInfo, AddressPayload, UpdateAddressPayload } from '@/actions';
import { ShopPoints } from './ShopPoints';
import Decimal from 'decimal.js';
import {
  Dialog,
  DialogContent,
  DialogTitle,
  Button,
  Checkbox,
  Input,
} from '@/components/ui';

interface ExchangeModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  itemHashId: string | null;
  userPoints: number;
  onSuccess?: () => void;
}

interface AddressFormData {
  name: string;
  ccc: string;
  phone: string;
  country: string;
  state: string;
  city: string;
  address: string;
  postalCode: string;
}

const INITIAL_FORM: AddressFormData = {
  name: '',
  ccc: '61',
  phone: '',
  country: '',
  state: '',
  city: '',
  address: '',
  postalCode: '',
};

export function ExchangeModal({ open, onOpenChange, itemHashId, userPoints, onSuccess }: ExchangeModalProps) {
  const t = useTranslations('eventshop');
  const { execute } = useServerAction({ showErrorToast: true });
  const { showSuccess, showError } = useToast();

  const [item, setItem] = useState<ShopItem | null>(null);
  const [imageUrl, setImageUrl] = useState('');
  const [addresses, setAddresses] = useState<AddressInfo[]>([]);
  const [selectedAddress, setSelectedAddress] = useState<string>('');
  const [quantity, setQuantity] = useState(1);
  const [comment, setComment] = useState('');
  const [agreed, setAgreed] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Address form state
  const [showAddressForm, setShowAddressForm] = useState(false);
  const [editingAddress, setEditingAddress] = useState<AddressInfo | null>(null);
  const [addressForm, setAddressForm] = useState<AddressFormData>(INITIAL_FORM);
  const [addressErrors, setAddressErrors] = useState<Partial<Record<keyof AddressFormData, string>>>({});
  const [isSavingAddress, setIsSavingAddress] = useState(false);

  const displayPoint = item ? new Decimal(item.point).div(10000).toNumber() : 0;
  const totalPoints = displayPoint * quantity;
  const canSubmit = agreed && selectedAddress && totalPoints <= userPoints && !isSubmitting;

  const fetchData = useCallback(async () => {
    if (!itemHashId) return;
    setIsLoading(true);
    setQuantity(1);
    setComment('');
    setAgreed(false);
    setShowAddressForm(false);

    try {
      const [itemResult, addrResult] = await Promise.all([
        execute(getShopItemDetail, itemHashId),
        getAddressList(),
      ]);

      if (itemResult.success && itemResult.data) {
        setItem(itemResult.data);
        const guid = itemResult.data.images?.[0];
        if (guid) {
          const imgResult = await getMediaUrl(guid);
          if (imgResult.success && imgResult.data) setImageUrl(imgResult.data);
        }
      }

      if (addrResult.success && addrResult.data) {
        setAddresses(addrResult.data);
        if (addrResult.data.length > 0) {
          setSelectedAddress(addrResult.data[0].hashId);
        }
      }
    } finally {
      setIsLoading(false);
    }
  }, [itemHashId, execute]);

  useEffect(() => {
    if (open && itemHashId) {
      setItem(null);
      setImageUrl('');
      fetchData();
    }
  }, [open, itemHashId, fetchData]);

  const handleQuantity = (type: 'add' | 'min') => {
    if (type === 'add') setQuantity((q) => q + 1);
    else setQuantity((q) => Math.max(1, q - 1));
  };

  const handleSubmit = async () => {
    if (!item || !selectedAddress) return;
    if (!agreed) return;

    if (totalPoints > userPoints) {
      showError(t('exchange.insufficientPoints'));
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await execute(purchaseItem, {
        shopItemHashId: item.hashId,
        quantity,
        addressHashId: selectedAddress,
        comment,
      });
      if (result.success) {
        showSuccess(t('exchange.success'));
        onSuccess?.();
        onOpenChange(false);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // Address form handlers
  const openAddressForm = (addr?: AddressInfo) => {
    if (addr) {
      setEditingAddress(addr);
      setAddressForm({
        name: addr.name || '',
        ccc: String(addr.ccc) || '61',
        phone: addr.phone || '',
        country: addr.country || '',
        state: addr.content?.state || '',
        city: addr.content?.city || '',
        address: addr.content?.address || '',
        postalCode: addr.content?.postalCode || '',
      });
    } else {
      setEditingAddress(null);
      setAddressForm(INITIAL_FORM);
    }
    setAddressErrors({});
    setShowAddressForm(true);
  };

  const validateAddressForm = (): boolean => {
    const errors: Partial<Record<keyof AddressFormData, string>> = {};
    const requiredMsg = t('exchange.addressRequired');
    if (!addressForm.name.trim()) errors.name = requiredMsg;
    if (!addressForm.phone.trim()) errors.phone = requiredMsg;
    if (!addressForm.country.trim()) errors.country = requiredMsg;
    if (!addressForm.state.trim()) errors.state = requiredMsg;
    if (!addressForm.city.trim()) errors.city = requiredMsg;
    if (!addressForm.address.trim()) errors.address = requiredMsg;
    if (!addressForm.postalCode.trim()) errors.postalCode = requiredMsg;
    setAddressErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSaveAddress = async () => {
    if (!validateAddressForm()) return;
    setIsSavingAddress(true);

    try {
      const contentStr = JSON.stringify({
        address: addressForm.address,
        city: addressForm.city,
        state: addressForm.state,
        postalCode: addressForm.postalCode,
        socialMediaType: '',
        socialMediaAccount: '',
      });

      let result;
      if (editingAddress) {
        const payload: UpdateAddressPayload = {
          hashId: editingAddress.hashId,
          name: addressForm.name,
          ccc: addressForm.ccc,
          phone: addressForm.phone,
          country: addressForm.country,
          content: contentStr,
          createdOn: editingAddress.createdOn,
          updatedOn: editingAddress.updatedOn,
        };
        result = await execute(updateAddress, editingAddress.hashId, payload);
      } else {
        const payload: AddressPayload = {
          name: addressForm.name,
          ccc: addressForm.ccc,
          phone: addressForm.phone,
          country: addressForm.country,
          content: contentStr,
        };
        result = await execute(createAddress, payload);
      }

      if (result.success) {
        showSuccess(t('exchange.addressSaveSuccess'));
        const addrResult = await getAddressList();
        if (addrResult.success && addrResult.data) {
          setAddresses(addrResult.data);
          if (result.data?.hashId) {
            setSelectedAddress(result.data.hashId);
          } else if (addrResult.data.length > 0 && !selectedAddress) {
            setSelectedAddress(addrResult.data[0].hashId);
          }
        }
        setShowAddressForm(false);
      }
    } finally {
      setIsSavingAddress(false);
    }
  };

  const updateField = (field: keyof AddressFormData, value: string) => {
    setAddressForm((prev) => ({ ...prev, [field]: value }));
    if (addressErrors[field]) {
      setAddressErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const selectedAddr = addresses.find((a) => a.hashId === selectedAddress);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[800px] flex flex-col gap-0 p-0 overflow-hidden max-h-[90vh]">
        <DialogTitle className="sr-only">{t('exchange.title')}</DialogTitle>
        {isLoading || !item ? (
          <div className="flex flex-col gap-0 animate-pulse">
            {/* Header skeleton */}
            <div className="flex items-center gap-4 px-5 pt-5 pb-3">
              <div className="h-6 w-28 rounded bg-surface-secondary" />
              <div className="h-4 w-36 rounded bg-surface-secondary" />
            </div>
            {/* Content skeleton */}
            <div className="flex flex-col gap-5 px-5 py-5">
              <div className="flex flex-col sm:flex-row gap-5">
                <div className="shrink-0 size-[168px] rounded-lg bg-surface-secondary" />
                <div className="flex flex-col gap-3 flex-1">
                  <div className="h-5 w-48 rounded bg-surface-secondary" />
                  <div className="h-4 w-full rounded bg-surface-secondary" />
                  <div className="h-4 w-3/4 rounded bg-surface-secondary" />
                  <div className="h-5 w-24 rounded bg-surface-secondary" />
                  <div className="flex flex-col gap-1.5">
                    <div className="h-4 w-12 rounded bg-surface-secondary" />
                    <div className="flex items-center gap-1">
                      <div className="size-7 rounded bg-surface-secondary" />
                      <div className="w-12 h-7 rounded bg-surface-secondary" />
                      <div className="size-7 rounded bg-surface-secondary" />
                    </div>
                  </div>
                </div>
              </div>
              {/* Address skeleton */}
              <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <div className="h-4 w-20 rounded bg-surface-secondary" />
                  <div className="h-4 w-24 rounded bg-surface-secondary" />
                </div>
                <div className="h-16 rounded-lg bg-surface-secondary" />
              </div>
            </div>
            {/* Footer skeleton */}
            <div className="flex justify-end gap-3 px-5 pb-5">
              <div className="h-10 w-24 rounded bg-surface-secondary" />
              <div className="h-10 w-24 rounded bg-surface-secondary" />
            </div>
          </div>
        ) : showAddressForm ? (
          /* ===== Address Form View ===== */
          <div className="flex flex-col overflow-hidden">
            <div className="flex items-center justify-between px-5 pt-5 pb-3">
              <h2 className="text-xl font-bold text-text-primary">
                {editingAddress ? t('exchange.editAddress') : t('exchange.addNewAddress')}
              </h2>
            </div>

            <div className="flex flex-col gap-4 px-5 py-5 overflow-y-auto">
              <Input
                label={t('exchange.addressName')}
                value={addressForm.name}
                onChange={(e) => updateField('name', e.target.value)}
                placeholder={t('exchange.addressNamePlaceholder')}
                error={addressErrors.name}
                required
              />

              <div className="grid grid-cols-[100px_1fr] gap-3">
                <Input
                  label={t('exchange.addressCountryCode')}
                  value={addressForm.ccc}
                  onChange={(e) => updateField('ccc', e.target.value)}
                  placeholder="+61"
                />
                <Input
                  label={t('exchange.addressPhone')}
                  value={addressForm.phone}
                  onChange={(e) => updateField('phone', e.target.value)}
                  placeholder={t('exchange.addressPhonePlaceholder')}
                  error={addressErrors.phone}
                  required
                />
              </div>

              <Input
                label={t('exchange.addressCountry')}
                value={addressForm.country}
                onChange={(e) => updateField('country', e.target.value)}
                placeholder={t('exchange.addressCountryPlaceholder')}
                error={addressErrors.country}
                required
              />

              <div className="grid grid-cols-2 gap-3">
                <Input
                  label={t('exchange.addressState')}
                  value={addressForm.state}
                  onChange={(e) => updateField('state', e.target.value)}
                  placeholder={t('exchange.addressStatePlaceholder')}
                  error={addressErrors.state}
                  required
                />
                <Input
                  label={t('exchange.addressCity')}
                  value={addressForm.city}
                  onChange={(e) => updateField('city', e.target.value)}
                  placeholder={t('exchange.addressCityPlaceholder')}
                  error={addressErrors.city}
                  required
                />
              </div>

              <Input
                label={t('exchange.addressStreet')}
                value={addressForm.address}
                onChange={(e) => updateField('address', e.target.value)}
                placeholder={t('exchange.addressStreetPlaceholder')}
                error={addressErrors.address}
                required
              />

              <Input
                label={t('exchange.addressPostalCode')}
                value={addressForm.postalCode}
                onChange={(e) => updateField('postalCode', e.target.value)}
                placeholder={t('exchange.addressPostalCodePlaceholder')}
                error={addressErrors.postalCode}
                required
              />
            </div>

            <div className="flex justify-end gap-3 px-5 pb-5">
              <Button variant="outline" onClick={() => setShowAddressForm(false)} disabled={isSavingAddress}>
                {t('exchange.cancel')}
              </Button>
              <Button variant="primary" onClick={handleSaveAddress} disabled={isSavingAddress}>
                {isSavingAddress ? t('exchange.addressSaving') : t('exchange.addressSave')}
              </Button>
            </div>
          </div>
        ) : (
          /* ===== Main Exchange View ===== */
          <>
            {/* Header */}
            <div className="flex items-center justify-between px-5 pt-5 pb-3">
              <div className="flex items-center gap-4">
                <h2 className="text-xl font-bold text-text-primary">
                  {t('exchange.title')}
                </h2>
                <span className="text-sm text-amber-500 font-semibold">
                  {t('exchange.currentPoints')}: {userPoints.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                </span>
              </div>
            </div>

            {/* Content */}
            <div className="flex flex-col gap-5 px-5 py-5 overflow-y-auto">
              <div className="flex flex-col sm:flex-row gap-5">
                {/* Image */}
                <div className="shrink-0">
                  <div className="size-[168px] rounded-lg overflow-hidden relative bg-surface-secondary border border-border">
                    {imageUrl ? (
                      <Image src={imageUrl} alt="" fill className="object-cover" unoptimized />
                    ) : (
                      <div className="size-full flex items-center justify-center text-text-tertiary">
                        <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
                          <rect x="3" y="3" width="18" height="18" rx="2"/>
                          <circle cx="8.5" cy="8.5" r="1.5"/>
                          <path d="M21 15l-5-5L5 21"/>
                        </svg>
                      </div>
                    )}
                  </div>
                </div>

                {/* Details */}
                <div className="flex flex-col gap-3 flex-1">
                  <h3 className="text-lg font-semibold text-text-primary leading-7">{item.name}</h3>
                  {item.description && (
                    <div className="text-sm text-text-secondary leading-5" dangerouslySetInnerHTML={{ __html: String(item.description) }} />
                  )}
                  <ShopPoints value={item.point} showIcon={false} className="text-lg font-bold text-amber-500" />

                  {/* Quantity */}
                  <div className="flex flex-col gap-1.5">
                    <span className="text-sm text-text-secondary">{t('exchange.quantity')}</span>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => handleQuantity('min')}
                        disabled={isSubmitting}
                        className="size-7 flex items-center justify-center bg-surface-secondary rounded text-base cursor-pointer hover:bg-border disabled:opacity-50"
                      >
                        -
                      </button>
                      <span className="w-12 h-7 flex items-center justify-center bg-surface-secondary rounded text-sm">
                        {quantity}
                      </span>
                      <button
                        onClick={() => handleQuantity('add')}
                        disabled={isSubmitting}
                        className="size-7 flex items-center justify-center bg-surface-secondary rounded text-base cursor-pointer hover:bg-border disabled:opacity-50"
                      >
                        +
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              {/* Address Section */}
              <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-text-secondary">{t('exchange.address')}</span>
                  <Button
                    variant="ghost"
                    size="xs"
                    className="text-primary text-sm p-0 h-auto"
                    onClick={() => openAddressForm()}
                  >
                    + {t('exchange.addNewAddress')}
                  </Button>
                </div>

                {addresses.length === 0 ? (
                  <div
                    className="flex items-center justify-center py-6 border border-dashed border-border rounded-lg cursor-pointer hover:border-primary transition-colors"
                    onClick={() => openAddressForm()}
                  >
                    <span className="text-sm text-text-tertiary">+ {t('exchange.addNewAddress')}</span>
                  </div>
                ) : (
                  <div className="flex flex-col gap-2">
                    {addresses.map((addr) => {
                      const isSelected = selectedAddress === addr.hashId;
                      const fullAddr = [addr.content?.state, addr.content?.city, addr.content?.address]
                        .filter(Boolean)
                        .join(', ');
                      return (
                        <div
                          key={addr.hashId}
                          onClick={() => setSelectedAddress(addr.hashId)}
                          className={`flex items-start gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                            isSelected
                              ? 'border-primary bg-primary-light'
                              : 'border-border hover:border-text-secondary'
                          }`}
                        >
                          <div className="mt-0.5 shrink-0">
                            <div className={`size-4 rounded-full border-2 flex items-center justify-center ${
                              isSelected ? 'border-primary' : 'border-text-secondary'
                            }`}>
                              {isSelected && <div className="size-2 rounded-full bg-primary" />}
                            </div>
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2">
                              <span className="text-sm font-medium text-text-primary">{addr.name}</span>
                              <span className="text-xs text-text-secondary">+{addr.ccc} {addr.phone}</span>
                            </div>
                            <p className="text-xs text-text-tertiary mt-0.5 truncate">
                              {addr.country && `${addr.country} - `}{fullAddr}
                              {addr.content?.postalCode && ` (${addr.content.postalCode})`}
                            </p>
                          </div>
                          <button
                            onClick={(e) => { e.stopPropagation(); openAddressForm(addr); }}
                            className="shrink-0 text-text-tertiary hover:text-primary cursor-pointer p-1"
                          >
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                            </svg>
                          </button>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>

              {/* Comment */}
              <div className="flex flex-col gap-1.5">
                <span className="text-sm text-text-secondary">{t('exchange.comment')}</span>
                <textarea
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  disabled={isSubmitting}
                  placeholder={t('exchange.commentPlaceholder')}
                  rows={3}
                  className="rounded border border-border bg-surface px-3 py-2 text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-1 focus:ring-primary resize-none"
                />
              </div>
            </div>

            {/* Footer */}
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 px-5 pb-5">
              <div className="flex items-center gap-2">
                <Checkbox
                  checked={agreed}
                  onCheckedChange={(checked) => setAgreed(checked === true)}
                  disabled={isSubmitting}
                />
                <span className="text-sm text-text-secondary">{t('exchange.agreeRules')}</span>
              </div>
              <div className="flex items-center gap-3">
                <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isSubmitting}>
                  {t('exchange.cancel')}
                </Button>
                <Button
                  variant="primary"
                  onClick={handleSubmit}
                  disabled={!canSubmit}
                >
                  {isSubmitting ? '...' : `${t('exchange.redeem')} ${totalPoints.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${t('exchange.points')}`}
                </Button>
              </div>
            </div>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
