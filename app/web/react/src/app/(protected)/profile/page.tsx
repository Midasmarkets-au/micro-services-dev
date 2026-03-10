'use client';

import { useState, useRef, useEffect, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useUserStore } from '@/stores/userStore';
import { useServerAction } from '@/hooks/useServerAction';
import { useToast } from '@/hooks/useToast';
import { uploadAvatar, updatePhoneNumber } from '@/actions';
import { Button, Input, SearchableSelect, SelectInput, Avatar } from '@/components/ui';
import { getRegionCodes } from '@/core/data/phonesData';

export default function ProfilePage() {
  const t = useTranslations('profile');
  const { execute, isLoading } = useServerAction();
  const { showSuccess, showError } = useToast();
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  // 从 store 获取用户信息
  const user = useUserStore((state) => state.user);
  const setUser = useUserStore((state) => state.setUser);
  
  const [isUploading, setIsUploading] = useState(false);
  
  // 手机号编辑状态
  const [phoneCode, setPhoneCode] = useState('+86');
  const [phoneNumber, setPhoneNumber] = useState('');

  // 地区数据（与注册页面一致）
  const regionsData = useMemo(() => getRegionCodes(), []);
  
  // 国家/地区选项（与注册页面一致）
  const countryOptions = useMemo(
    () => Object.values(regionsData).map((country) => ({
      value: country.code,
      label: country.name,
    })),
    [regionsData]
  );

  // 手机区号选项（与注册页面一致）
  const phoneCodeOptions = useMemo(
    () => Object.values(regionsData).map((country) => ({
      value: `+${country.dialCode}`,
      label: `${country.name} +${country.dialCode}`,
      code: country.code,
    })),
    [regionsData]
  );

  // 初始化手机号
  useEffect(() => {
    if (user) {
      setPhoneCode(user.ccc ? `+${user.ccc}` : '+86');
      setPhoneNumber(user.phoneNumber || '');
    }
  }, [user]);

  // 处理头像上传
  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // 验证文件类型
    const allowedTypes = ['image/png', 'image/jpg', 'image/jpeg'];
    if (!allowedTypes.includes(file.type)) {
      showError(t('errors.invalidFileType'));
      return;
    }

    // 验证文件大小（最大 5MB）
    if (file.size > 5 * 1024 * 1024) {
      showError(t('errors.fileTooLarge'));
      return;
    }

    setIsUploading(true);
    try {
      const result = await execute(() => uploadAvatar(file));
      if (result.success && result.data) {
        // 更新 store 中的头像
        if (user) {
          setUser({ ...user, avatar: result.data.party.avatar });
        }
        //showSuccess(t('messages.avatarUpdated'));
      }
    } finally {
      setIsUploading(false);
      // 清空 input 以允许重复上传同一文件
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  // 重置表单
  const handleReset = () => {
    // 重置手机号为原始值
    if (user) {
      setPhoneCode(user.ccc ? `+${user.ccc}` : '+86');
      setPhoneNumber(user.phoneNumber || '');
    }
  };

  // 保存表单
  const handleSave = async () => {
    // 提取区号数字（去掉+号）
    const regionCode = phoneCode.replace('+', '');
    
    // 检查手机号是否有变化
    const originalCcc = user?.ccc || '86';
    const originalPhone = user?.phoneNumber || '';
    
    if (regionCode === originalCcc && phoneNumber === originalPhone) {
      showSuccess(t('messages.saved'));
      return;
    }

    // 验证手机号
    if (!phoneNumber) {
      showError(t('errors.phoneRequired'));
      return;
    }

    // 调用接口更新手机号（不需要验证码）
    const result = await execute(() => updatePhoneNumber(regionCode, phoneNumber));
    if (result.success) {
      // 更新 store 中的手机号
      if (user) {
        setUser({ ...user, ccc: regionCode, phoneNumber });
      }
      showSuccess(t('messages.profileUpdated'));
    }
  };

  return (
    <div className="flex flex-col items-center gap-10 md:gap-[80px] bg-surface rounded-xl px-4 py-10 md:px-[40px] md:py-[80px]">
      {/* 头像和表单区域 */}
      <div className="flex flex-col items-center gap-5 w-full">
        {/* 头像组件 */}
        <div className="flex flex-col items-center gap-3">
          {/* 头像容器 */}
          <div className="flex flex-col items-center pb-3">
            {/* 头像 */}
            <div className="mb-[-12px]">
              <Avatar
                src={user?.avatar}
                alt={user?.name || 'User'}
                size="md"
                onClick={handleAvatarClick}
                loading={isUploading}
                skeleton={!user}
                clickable
              />
            </div>
            
            {/* 更换头像按钮 */}
            <button
              type="button"
              onClick={handleAvatarClick}
              className="bg-primary z-10 text-white text-xs font-normal px-2 py-0.5 rounded-[54px] mb-[-12px]"
            >
              {t('changeAvatar')}
            </button>
          </div>
          
          {/* 隐藏的文件输入 */}
          <input
            ref={fileInputRef}
            type="file"
            accept=".png,.jpg,.jpeg"
            onChange={handleFileChange}
            className="hidden"
          />
        </div>
        
        {/* 文件类型提示 */}
        <p className="text-sm text-text-secondary text-center">
          {t('avatarHint')}
        </p>

        {/* 表单字段 - 响应式双列布局 */}
        {/* 移动端: 单列100%宽度, 桌面端: 双列 335px + 20px + 335px = 690px */}
        <div className="flex flex-col gap-5 w-full md:w-auto">
          {/* 第一行：电子邮件、姓名 */}
          <div className="flex flex-col md:flex-row gap-5">
            <div className="w-full md:w-[335px]">
              <label className="block text-sm font-normal text-text-secondary mb-2 px-1">
                <span className="text-primary">*</span>{t('fields.email')}
              </label>
              <Input
                value={user?.email || ''}
                disabled
                className="w-full"
              />
            </div>
            <div className="w-full md:w-[335px]">
              <label className="block text-sm font-normal text-text-secondary mb-2 px-1">
                <span className="text-primary">*</span>{t('fields.nickname')}
              </label>
              <Input
                value={user?.name || user?.nativeName || ''}
                disabled
                className="w-full"
              />
            </div>
          </div>

          {/* 第二行：姓、名 */}
          <div className="flex flex-col md:flex-row gap-5">
            <div className="w-full md:w-[335px]">
              <label className="block text-sm font-normal text-text-secondary mb-2 px-1">
                <span className="text-primary">*</span>{t('fields.lastName')}
              </label>
              <Input
                value={user?.lastName || ''}
                disabled
                className="w-full"
              />
            </div>
            <div className="w-full md:w-[335px]">
              <label className="block text-sm font-normal text-text-secondary mb-2 px-1">
                <span className="text-primary">*</span>{t('fields.firstName')}
              </label>
              <Input
                value={user?.firstName || ''}
                disabled
                className="w-full"
              />
            </div>
          </div>

          {/* 第三行：国家和地区、手机号（与注册页面一致） */}
          <div className="flex flex-col md:flex-row gap-5">
            {/* 国家/地区 - 只读显示 */}
            <div className="flex flex-col gap-2 w-full md:w-[335px]">
              {/* 标签 - 与其他字段一致 */}
              <div className="flex items-center px-1 text-sm font-normal">
                <span className="text-primary">*</span>
                <span className="text-text-secondary">{t('fields.country')}</span>
              </div>
              <SearchableSelect
                options={countryOptions}
                placeholder={t('fields.countryPlaceholder')}
                isSearchable
                value={countryOptions.find(opt => opt.value === user?.countryCode) || null}
                isDisabled
              />
            </div>
            
            {/* 手机号 - 可编辑（与注册页面一致） */}
            <div className="flex flex-col gap-2 w-full md:w-[335px]">
              {/* 标签 - 与其他字段一致 */}
              <div className="flex items-center px-1 text-sm font-normal">
                <span className="text-primary">*</span>
                <span className="text-text-secondary">{t('fields.phone')}</span>
              </div>
              <SelectInput
                selectValue={phoneCode}
                inputValue={phoneNumber}
                selectOptions={phoneCodeOptions}
                onSelectChange={(value) => setPhoneCode(value)}
                onInputChange={(value) => setPhoneNumber(value)}
                placeholder={t('changePhone.phonePlaceholder')}
                inputType="tel"
              />
            </div>
          </div>
        </div>
      </div>

      {/* 底部按钮 - 响应式 */}
      <div className="flex items-center justify-center gap-4 md:gap-5 w-full md:w-auto">
        <Button
          variant="secondary"
          onClick={handleReset}
          className="flex-1 md:flex-none md:w-[120px] h-11"
        >
          {t('buttons.reset')}
        </Button>
        <Button
          variant="primary"
          onClick={handleSave}
          loading={isLoading}
          className="flex-1 md:flex-none md:w-[120px] h-11"
        >
          {t('buttons.save')}
        </Button>
      </div>
    </div>
  );
}
