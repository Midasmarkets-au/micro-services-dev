'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import Image from 'next/image';
import { Button, ConfirmDialog } from '@/components/ui';
import { BankAccountModal, type BankAccountFormData, type BankFormData, type USDTFormData } from '../components/BankAccountModal';
import {
  getPaymentInfoList,
  deletePaymentInfo,
  createPaymentInfo,
  updatePaymentInfo,
  type PaymentInfo,
} from '@/actions';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { getDataByCode } from '@/core/data/phonesData';

export default function BankInfosPage() {
  const t = useTranslations('profile.bankInfos');
  const tCommon = useTranslations('common');
  const { execute, isLoading } = useServerAction({ showErrorToast: true });
  const { showSuccess, showError } = useToast();

  const [loading, setLoading] = useState(true);
  const [paymentInfos, setPaymentInfos] = useState<PaymentInfo[]>([]);
  
  // Modal states
  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'add' | 'edit'>('add');
  const [modalPlatform, setModalPlatform] = useState<100 | 240>(100);
  const [selectedInfo, setSelectedInfo] = useState<PaymentInfo | null>(null);

  // Confirm dialog state
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  // Fetch payment info list
  const fetchPaymentInfos = useCallback(async () => {
    setLoading(true);
    const result = await execute(getPaymentInfoList);
    if (result.success && result.data) {
      setPaymentInfos(result.data);
    }
    setLoading(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    fetchPaymentInfos();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Handle add account
  const handleAddAccount = (platform: 100 | 240) => {
    setModalMode('add');
    setModalPlatform(platform);
    setSelectedInfo(null);
    setModalOpen(true);
  };

  // Handle edit
  const handleEdit = (info: PaymentInfo) => {
    console.log('handleEdit info', info);
    setModalMode('edit');
    setModalPlatform(info.paymentPlatform as 100 | 240);
    setSelectedInfo(info);
    setModalOpen(true);
  };

  // Handle delete confirm
  const handleDeleteClick = (id: number) => {
    setDeletingId(id);
    setConfirmOpen(true);
  };

  // Handle delete
  const handleDelete = async () => {
    if (!deletingId) return;

    const result = await execute(deletePaymentInfo, deletingId);
    if (result.success) {
      showSuccess(t('deleteSuccess'));
      fetchPaymentInfos();
      setConfirmOpen(false);
    }
  };

  // Handle form submit
  const handleFormSubmit = async (formData: BankAccountFormData) => {
    const isBankAccount = 'holder' in formData;
    const payload = {
      paymentPlatform: modalPlatform,
      name: formData.name,
      info: isBankAccount
        ? {
            name: formData.name,
            holder: (formData as BankFormData).holder,
            bankName: (formData as BankFormData).bankName,
            branchName: (formData as BankFormData).branchName,
            state: (formData as BankFormData).state,
            city: (formData as BankFormData).city,
            accountNo: (formData as BankFormData).accountNo,
            confirmAccountNo: (formData as BankFormData).confirmAccountNo,
            bankCountry: (formData as BankFormData).bankCountry,
          }
        : {
            name: formData.name,
            walletAddress: (formData as USDTFormData).walletAddress,
          },
    };

    const result =
      modalMode === 'add'
        ? await execute(createPaymentInfo, payload)
        : await execute(updatePaymentInfo, selectedInfo!.id, payload);
    
    if (result.success) {
      showSuccess(modalMode === 'add' ? t('addSuccess') : t('updateSuccess'));
      fetchPaymentInfos();
      setModalOpen(false);
    }
  };

  // Copy to clipboard
  const handleCopy = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
      showSuccess(t('copySuccess'));
    } catch {
      showError(t('errors.copyFailed'));
    }
  };

  // Render account card
  const renderCard = (info: PaymentInfo) => {
    const isBankAccount = info.paymentPlatform === 100;
    const accountNo = isBankAccount && 'accountNo' in info.info ? info.info.accountNo : '';
    const holder = isBankAccount && 'holder' in info.info ? info.info.holder : '';
    const bankCountry = isBankAccount && 'bankCountry' in info.info ? info.info.bankCountry : 'cn';
    const bankName = isBankAccount && 'bankName' in info.info ? info.info.bankName : '';
    const branchName = isBankAccount && 'branchName' in info.info ? info.info.branchName : '';
    const walletAddress = !isBankAccount && 'walletAddress' in info.info ? info.info.walletAddress : '';
    
    const countryData = getDataByCode(bankCountry);
    const flagPath = `/images/flags/${bankCountry}.svg`;

    return (
      <div
        key={info.id}
        className="bg-surface border border-border rounded-[12px] p-5 md:p-10 flex flex-col gap-5"
      >
        {/* Top section */}
        <div className="flex gap-3 sm:gap-5 items-start">
          {/* Country flag / USDT icon */}
          <div className="shrink-0 size-8 sm:size-10 rounded-full bg-[#f5f5f5] dark:bg-[#2a2a2a] flex items-center justify-center overflow-hidden">
            {isBankAccount ? (
              <Image
                src={flagPath}
                alt={countryData.name || ''}
                width={40}
                height={40}
                className="object-cover"
                // onError={(e) => {
                //   (e.target as HTMLImageElement).src = '/images/default-avatar.svg';
                // }}
              />
            ) : (
              <Image
                src={'/images/wallet/TRC20.png'}
                alt="TRC20"
                width={40}
                height={40}
                className="object-cover"
                // onError={(e) => {
                //   (e.target as HTMLImageElement).src = '/images/default-avatar.svg';
                // }}
              />
            )}
          </div>

          {/* Info */}
          <div className="flex-1 min-w-0 flex flex-col gap-2">
            {/* Name */}
            <h3 className="text-xl font-semibold text-text-primary">
              {info.name}
            </h3>

            {/* Bank name or wallet label */}
            <div className="flex items-center gap-2 text-sm text-text-secondary min-w-0">
              <span className="shrink-0">{isBankAccount ? t('bankNameLabel') : t('walletLabel')}</span>
              <span className="text-text-secondary truncate">
                {isBankAccount
                  ? `${bankName}${branchName ? ` (${branchName})` : ''}`
                  : t('usdtWallet')}
              </span>
            </div>

            {/* Account number / Wallet address */}
            <div className="flex items-center gap-2 min-w-0">
              <span className="text-sm text-text-secondary shrink-0">
                {isBankAccount ? t('accountLabel') : t('addressLabel')}
              </span>
              <span className="text-sm text-text-secondary truncate">
                {isBankAccount ? accountNo : walletAddress}
              </span>
              <button
                onClick={() => handleCopy(isBankAccount ? accountNo : walletAddress)}
                className="shrink-0"
              >
                <Image
                  src="/images/icons/document-file-day.svg"
                  alt="copy"
                  width={20}
                  height={20}
                  className="cursor-pointer dark:hidden"
                />
                <Image
                  src="/images/icons/document-file-night.svg"
                  alt="copy"
                  width={20}
                  height={20}
                  className="cursor-pointer hidden dark:block"
                />
              </button>
            </div>
          </div>
        </div>

        {/* Divider */}
        <div className="h-px bg-border" />
        {/* Bottom section */}
        <div className="flex flex-col gap-5">
          {/* Holder & Country */}
          {isBankAccount && (
            <div className="flex items-center justify-between gap-3 sm:gap-5">
              <div className="flex flex-col items-center gap-1 flex-1 min-w-0">
                <span className="text-sm text-text-secondary">
                  {t('holder')}
                </span>
                <span className="text-base font-semibold text-text-primary truncate max-w-full">
                  {holder}
                </span>
              </div>
              <div className="flex flex-col items-center gap-1 flex-1 min-w-0">
                <span className="text-sm text-text-secondary text-center">
                  {t('bankLocation')}
                </span>
                <span className="text-base font-semibold text-text-primary truncate max-w-full">
                  {countryData.name || bankCountry}
                </span>
              </div>
            </div>
          )}

          {/* Action buttons */}
          <div className="flex gap-3 sm:gap-5">
            <Button
              variant="secondary"
              size="sm"
              onClick={() => handleDeleteClick(info.id)}
              className="flex-1"
            >
              {t('unlinkAccount')}
            </Button>
            <Button
              variant="secondary"
              size="sm"
              onClick={() => handleEdit(info)}
              className="flex-1"
            >
              {tCommon('edit')}
            </Button>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="flex flex-col gap-5">
      {/* Action buttons - Right aligned */}
      <div className="flex flex-col sm:flex-row items-stretch sm:items-center justify-end mt-5 gap-3 sm:gap-5 md:gap-10">
        {/* Add Bank Account Button */}
        <Button
          onClick={() => handleAddAccount(100)}
          size="xs"
          className="w-full sm:w-auto bg-[#000f32] dark:bg-[#1a2744] hover:bg-[#001a47] dark:hover:bg-[#243558]"
        >
          <Image
            src="/images/icons/add-plain.svg"
            alt="add"
            width={20}
            height={20}
          />
          <span className="truncate">{t('addBankAccount')}</span>
        </Button>
        
        {/* Add USDT Wallet Button - 数据加载完成且列表中没有 USDT 钱包时才显示 */}
        {!loading && !paymentInfos.some((info) => info.paymentPlatform === 240) && (
          <Button
            onClick={() => handleAddAccount(240)} 
            variant="primary"
            size="xs"
            className="w-full sm:w-auto"
          >
            <Image
              src="/images/icons/add-plain.svg"
              alt="add"
              width={20}
              height={20}
            />
            <span className="truncate">{t('addUSDTWallet')}</span>
          </Button>
        )}
      </div>

      {/* Content */}
      {loading ? (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          {[1, 2, 3, 4].map((i) => (
            <div
              key={i}
              className="bg-surface border border-border rounded-[12px] p-5 md:p-10 flex flex-col gap-5 animate-pulse"
            >
              {/* Top section skeleton */}
              <div className="flex gap-5 items-start">
                <div className="shrink-0 size-10 rounded-full bg-border" />
                <div className="flex-1 space-y-3">
                  <div className="h-5 bg-border rounded w-3/4" />
                  <div className="h-4 bg-border rounded w-full" />
                  <div className="h-4 bg-border rounded w-2/3" />
                </div>
              </div>
              {/* Divider */}
              <div className="h-px bg-border" />
              {/* Bottom section skeleton */}
              <div className="space-y-4">
                <div className="flex gap-4">
                  <div className="flex-1 h-12 bg-border rounded" />
                  <div className="flex-1 h-12 bg-border rounded" />
                </div>
                <div className="flex gap-5">
                  <div className="flex-1 h-10 bg-border rounded" />
                  <div className="flex-1 h-10 bg-border rounded" />
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : paymentInfos.length === 0 ? (
        <div className="flex items-center justify-center py-20">
          <div className="text-base text-text-secondary">
            {t('noAccounts')}
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          {paymentInfos.map(renderCard)}
        </div>
      )}

      {/* Modals */}
      <BankAccountModal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={handleFormSubmit}
        paymentPlatform={modalPlatform}
        initialData={selectedInfo}
        mode={modalMode}
      />

      <ConfirmDialog
        isOpen={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        onConfirm={handleDelete}
        title={t('confirmDelete.title')}
        description={t('confirmDelete.description')}
        confirmText={tCommon('confirm')}
        cancelText={tCommon('cancel')}
        loading={isLoading}
      />
    </div>
  );
}
