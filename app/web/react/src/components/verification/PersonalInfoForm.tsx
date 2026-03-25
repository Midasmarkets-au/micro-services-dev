'use client';

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslations } from 'next-intl';
import { useMemo, useRef, useState, useEffect, useCallback } from 'react';
import {
  Input,
  Textarea,
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
  DatePicker,
  formatDateForApi,
  SearchableSelect,
} from '@/components/ui';
import { getRegionCodes } from '@/core/data/phonesData';
import { SubStepNav, type SubStep } from './SubStepNav';
import { VerificationFormLayout } from './VerificationFormLayout';
import { genderOptions, idTypeOptions, type InfoData } from '@/types/verification';
import { useUserStore } from '@/stores/userStore';
const infoSchema = z.object({
  // 个人信息
  firstName: z.string().min(1, 'required'),
  lastName: z.string().min(1, 'required'),
  gender: z.string().min(1, 'required'),
  birthday: z.date().optional(),
  citizen: z.string().min(1, 'required'),
  address: z.string().min(1, 'required'),
  // 身份证件
  idType: z.number().min(1, 'required'),
  idNumber: z.string().min(1, 'required'),
  idIssuer: z.string().optional(),
  idIssuedOn: z.date().optional(),
  idExpireOn: z.date().optional(),
  // 社交媒体（可选）
  whatsApp: z.string().optional(),
  weChat: z.string().optional(),
  instagram: z.string().optional(),
  telegram: z.string().optional(),
  line: z.string().optional(),
});

type InfoFormData = z.infer<typeof infoSchema>;

interface PersonalInfoFormProps {
  initialData?: Partial<InfoData>;
  onSubmit: (data: Record<string, unknown>) => void;
  onBack: () => void;
  isLoading?: boolean;
}

// 表单区域标题 - 移到组件外部避免重复创建
function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2">
      <div className="h-[18px] w-[3px] rounded-full bg-primary" />
      <h3 className="font-semibold text-base text-text-primary">{children}</h3>
    </div>
  );
}

// 表单字段标签 - 移到组件外部避免重复创建
function FieldLabel({ required, children }: { required?: boolean; children: React.ReactNode }) {
  return (
    <div className="flex items-center px-1 text-sm font-medium">
      {required && <span className="text-primary">*</span>}
      <span className="text-text-secondary">{children}</span>
    </div>
  );
}

export function PersonalInfoForm({ initialData, onSubmit, onBack, isLoading }: PersonalInfoFormProps) {
  const t = useTranslations('verification');
  const storeUser = useUserStore((state) => state.user);
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const sectionRefs = useRef<Record<SubStep, HTMLDivElement | null>>({
    personal: null,
    identity: null,
    social: null,
  });
  
  // 当前激活的子步骤（滚动同步）
  const [activeSubStep, setActiveSubStep] = useState<SubStep>('personal');

  // 获取国家列表
  const regionsData = useMemo(() => getRegionCodes(), []);
  const countryOptions = useMemo(
    () =>
      Object.entries(regionsData).map(([code, country]) => ({
        value: code,
        label: country.name,
      })),
    [regionsData]
  );

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<InfoFormData>({
    resolver: zodResolver(infoSchema),
    defaultValues: {
      firstName: initialData?.firstName || storeUser?.firstName || '',
      lastName: initialData?.lastName || storeUser?.lastName || '',
      gender: initialData?.gender || '',
      birthday: initialData?.birthday ? new Date(initialData.birthday) : undefined,
      citizen: initialData?.citizen || storeUser?.countryCode || '',
      address: initialData?.address || '',
      idType: initialData?.idType || 1,
      idNumber: initialData?.idNumber || '',
      idIssuer: initialData?.idIssuer || '',
      idIssuedOn: initialData?.idIssuedOn ? new Date(initialData.idIssuedOn) : undefined,
      idExpireOn: initialData?.idExpireOn ? new Date(initialData.idExpireOn) : undefined,
      whatsApp: initialData?.socialMedium?.find(s => s.name === 'whatsApp')?.account || '',
      weChat: initialData?.socialMedium?.find(s => s.name === 'weChat')?.account || '',
      instagram: initialData?.socialMedium?.find(s => s.name === 'instagram')?.account || '',
      telegram: initialData?.socialMedium?.find(s => s.name === 'telegram')?.account || '',
      line: initialData?.socialMedium?.find(s => s.name === 'line')?.account || '',
    },
  });

  // 滚动同步高亮：监听滚动事件，计算哪个区域最靠近顶部
  useEffect(() => {
    const scrollContainer = scrollContainerRef.current;
    if (!scrollContainer) return;
    
    const subStepOrder: SubStep[] = ['personal', 'identity', 'social'];
    
    const handleScroll = () => {
      const containerTop = scrollContainer.scrollTop;
      let activeStep: SubStep = 'personal';
      let minDistance = Infinity;
      
      subStepOrder.forEach((subStep) => {
        const element = sectionRefs.current[subStep];
        if (!element) return;
        
        // 计算元素顶部相对于滚动容器的位置
        const elementTop = element.offsetTop - scrollContainer.offsetTop;
        const distance = Math.abs(elementTop - containerTop);
        
        // 选择最靠近当前滚动位置的区域
        // 如果元素顶部在滚动位置之上或刚好在可视区域内，优先选择
        if (elementTop <= containerTop + 100 && distance < minDistance) {
          minDistance = distance;
          activeStep = subStep;
        }
      });
      
      setActiveSubStep(activeStep);
    };
    
    // 初始化时执行一次
    handleScroll();
    
    // 监听滚动事件
    scrollContainer.addEventListener('scroll', handleScroll, { passive: true });
    
    return () => {
      scrollContainer.removeEventListener('scroll', handleScroll);
    };
  }, []);

  // 点击导航滚动到对应区域
  const handleSubStepClick = useCallback((subStep: SubStep) => {
    const element = sectionRefs.current[subStep];
    const container = scrollContainerRef.current;
    if (element && container) {
      // 计算元素相对于容器的位置
      const elementTop = element.offsetTop - container.offsetTop;
      container.scrollTo({ top: elementTop, behavior: 'smooth' });
    }
  }, []);

  // 处理表单提交，格式化日期为 YYYY-MM-DD
  const handleFormSubmit = (data: InfoFormData) => {
    const formattedData = {
      ...data,
      birthday: formatDateForApi(data.birthday),
      idIssuedOn: formatDateForApi(data.idIssuedOn),
      idExpireOn: formatDateForApi(data.idExpireOn),
    };
    onSubmit(formattedData);
  };

  // 左侧导航
  const stepNav = (
    <SubStepNav activeSubStep={activeSubStep} onSubStepClick={handleSubStepClick} />
  );

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)}>
      <VerificationFormLayout
        stepNav={stepNav}
        onBack={onBack}
        isLoading={isLoading}
        scrollContainerRef={scrollContainerRef}
      >
        <div className="flex flex-col gap-8 md:gap-10">
          {/* 01 个人信息 */}
          <div
            ref={(el) => { sectionRefs.current.personal = el; }}
            className="flex flex-col gap-6 md:gap-10"
          >
            <SectionTitle>{t('subSteps.personalInfo')}</SectionTitle>

            <div className="flex flex-col gap-4 md:gap-5">
              {/* 名/姓 */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.firstName')}</FieldLabel>
                  <Input
                    placeholder={t('placeholders.firstName')}
                    error={errors.firstName?.message ? t('errors.required') : undefined}
                    errorPosition="bottom"
                    {...register('firstName')}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.lastName')}</FieldLabel>
                  <Input
                    placeholder={t('placeholders.lastName')}
                    error={errors.lastName?.message ? t('errors.required') : undefined}
                    errorPosition="bottom"
                    {...register('lastName')}
                  />
                </div>
              </div>

              {/* 性别/出生年月 */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.gender')}</FieldLabel>
                  <Controller
                    name="gender"
                    control={control}
                    render={({ field }) => (
                      <Select value={field.value} onValueChange={field.onChange}>
                        <SelectTrigger error={!!errors.gender}>
                          <SelectValue placeholder={t('placeholders.selectGender')} />
                        </SelectTrigger>
                        <SelectContent>
                          {genderOptions.map((opt) => (
                            <SelectItem key={opt.value} value={String(opt.value)}>
                              {t(`genders.${opt.value === '0' ? 'male' : 'female'}`)}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                  {errors.gender && (
                    <p className="text-sm error-text">{t('errors.required')}</p>
                  )}
                </div>
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.birthday')}</FieldLabel>
                  <Controller
                    name="birthday"
                    control={control}
                    render={({ field }) => (
                      <DatePicker
                        value={field.value}
                        onChange={field.onChange}
                        placeholder={t('placeholders.birthday')}
                        error={!!errors.birthday}
                      />
                    )}
                  />
                  {errors.birthday && (
                    <p className="text-sm error-text">{t('errors.required')}</p>
                  )}
                </div>
              </div>

              {/* 国家 */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.country')}</FieldLabel>
                  <Controller
                    name="citizen"
                    control={control}
                    render={({ field }) => (
                      <SearchableSelect
                        options={countryOptions}
                        value={countryOptions.find((opt) => opt.value === field.value) || null}
                        onChange={(option: unknown) => {
                          const selected = option as { value: string; label: string } | null;
                          field.onChange(selected?.value || '');
                        }}
                        error={errors.citizen?.message ? t('errors.required') : undefined}
                        errorPosition="bottom"
                        placeholder={t('placeholders.selectCountry')}
                      />
                    )}
                  />
                </div>
                <div className="hidden md:block" /> {/* 占位 - 仅桌面端显示 */}
              </div>

              {/* 地址 */}
              <div className="flex flex-col gap-2">
                <FieldLabel required>{t('fields.address')}</FieldLabel>
                <Textarea
                  placeholder={t('placeholders.address')}
                  error={errors.address?.message ? t('errors.required') : undefined}
                  errorPosition="bottom"
                  className="h-50"
                  {...register('address')}
                />
              </div>
            </div>
          </div>

          {/* 02 身份证件 */}
          <div
            ref={(el) => { sectionRefs.current.identity = el; }}
            className="flex flex-col gap-6 md:gap-10"
          >
            <SectionTitle>{t('subSteps.identityDocument')}</SectionTitle>

            <div className="flex flex-col gap-4 md:gap-5">
              {/* 证件类型/证件号码 */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.idType')}</FieldLabel>
                  <Controller
                    name="idType"
                    control={control}
                    render={({ field }) => (
                      <Select
                        value={String(field.value)}
                        onValueChange={(val) => field.onChange(Number(val))}
                      >
                        <SelectTrigger error={!!errors.idType}>
                          <SelectValue placeholder={t('placeholders.selectIdType')} />
                        </SelectTrigger>
                        <SelectContent>
                          {idTypeOptions.map((opt) => (
                            <SelectItem key={opt.value} value={String(opt.value)}>
                              {t(`idTypes.${opt.value}`)}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                  {errors.idType && (
                    <p className="text-sm error-text">{t('errors.required')}</p>
                  )}
                </div>
                <div className="flex flex-col gap-2">
                  <FieldLabel required>{t('fields.idNumber')}</FieldLabel>
                  <Input
                    placeholder={t('placeholders.idNumber')}
                    error={errors.idNumber?.message ? t('errors.required') : undefined}
                    errorPosition="bottom"
                    {...register('idNumber')}
                  />
                </div>
              </div>

              {/* 签发机关/证件有效日期 */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel>{t('fields.idIssuer')}</FieldLabel>
                  <Input
                    placeholder={t('placeholders.idIssuer')}
                    {...register('idIssuer')}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <FieldLabel>{t('fields.idValidDate')}</FieldLabel>
                  <div className="grid grid-cols-2 gap-2">
                    <Controller
                      name="idIssuedOn"
                      control={control}
                      render={({ field }) => (
                        <DatePicker
                          value={field.value}
                          onChange={field.onChange}
                          placeholder={t('placeholders.issuedDate')}
                        />
                      )}
                    />
                    <Controller
                      name="idExpireOn"
                      control={control}
                      render={({ field }) => (
                        <DatePicker
                          value={field.value}
                          onChange={field.onChange}
                          placeholder={t('placeholders.expireDate')}
                        />
                      )}
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* 03 社交媒体 */}
          <div
            ref={(el) => { sectionRefs.current.social = el; }}
            className="flex flex-col gap-6 md:gap-10"
          >
            <SectionTitle>{t('subSteps.socialMedia')}</SectionTitle>

            <div className="flex flex-col gap-4 md:gap-5">
              {/* WhatsApp/微信 */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel>WhatsApp</FieldLabel>
                  <Input
                    placeholder={t('placeholders.whatsApp')}
                    {...register('whatsApp')}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <FieldLabel>{t('fields.weChat')}</FieldLabel>
                  <Input
                    placeholder={t('placeholders.weChat')}
                    {...register('weChat')}
                  />
                </div>
              </div>

              {/* Instagram/Telegram */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel>Instagram</FieldLabel>
                  <Input
                    placeholder={t('placeholders.instagram')}
                    {...register('instagram')}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <FieldLabel>Telegram</FieldLabel>
                  <Input
                    placeholder={t('placeholders.telegram')}
                    {...register('telegram')}
                  />
                </div>
              </div>

              {/* Line */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-5">
                <div className="flex flex-col gap-2">
                  <FieldLabel>Line</FieldLabel>
                  <Input
                    placeholder={t('placeholders.line')}
                    {...register('line')}
                  />
                </div>
                <div className="hidden md:block" /> {/* 占位 - 仅桌面端显示 */}
              </div>
            </div>
          </div>

          {/* 提示文本 */}
          <div className="flex items-start gap-2 text-sm">
            <span className="font-semibold text-primary">*</span>
            <p className="text-text-secondary">
              {t('submitDisclaimer')}
            </p>
          </div>
        </div>
      </VerificationFormLayout>
    </form>
  );
}
