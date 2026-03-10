'use client';

import { usePathname } from 'next/navigation';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useTheme } from '@/hooks/useTheme';
import { useUserStore } from '@/stores/userStore';
import { PageLoading, Tabs } from '@/components/ui';
import type { TabItem } from '@/components/ui';
import { useNoticesStore } from '@/stores';
import DOMPurify from 'dompurify';

// 导航配置
const navItems = [
  { key: 'contact', path: '/supports', label: 'tabs.contact' },
  { key: 'announcements', path: '/supports/notices', label: 'tabs.announcements' },
  { key: 'documents', path: '/supports/documents', label: 'tabs.documents' },
];

// 横幅内容组件 - 默认（联系我们）
// 设计稿@1600: 标题left:80px,top:30px; 社交left:272px,top:30px,w:335px; 电话left:980px,top:30px; 邮件left:1248px,top:31px
function DefaultBannerContent() {
  const t = useTranslations('supports');
  const siteConfig = useUserStore((s) => s.siteConfig);
  
  const contactInfo = siteConfig?.contactInfo;
  
  // 社交媒体配置 - 总是显示4个图标，没有URL时显示空状态
  const socialIcons = [
    { name: 'Facebook', icon: 'facebook', url: contactInfo?.socialMedia?.facebook?.url || '' },
    { name: 'X', icon: 'x', url: contactInfo?.socialMedia?.twitter?.url || '' },
    // { name: 'Linkendin', icon: 'linkedin', url: '' },
    { name: 'Instagram', icon: 'instagram', url: contactInfo?.socialMedia?.instagram?.url || '' },
  ];

  const renderSocialIcon = (icon: string) => {
    const iconClass = "size-4 lg:size-5";
    switch (icon) {
      case 'facebook':
        return (
          <svg className={iconClass} fill="currentColor" viewBox="0 0 24 24">
            <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
          </svg>
        );
      case 'x':
        return (
          <svg className={iconClass} fill="currentColor" viewBox="0 0 24 24">
            <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z"/>
          </svg>
        );
      case 'linkedin':
        return (
          <svg className={iconClass} fill="currentColor" viewBox="0 0 24 24">
            <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/>
          </svg>
        );
      case 'instagram':
        return (
          <svg className={iconClass} fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 0C8.74 0 8.333.015 7.053.072 5.775.132 4.905.333 4.14.63c-.789.306-1.459.717-2.126 1.384S.935 3.35.63 4.14C.333 4.905.131 5.775.072 7.053.012 8.333 0 8.74 0 12s.015 3.667.072 4.947c.06 1.277.261 2.148.558 2.913.306.788.717 1.459 1.384 2.126.667.666 1.336 1.079 2.126 1.384.766.296 1.636.499 2.913.558C8.333 23.988 8.74 24 12 24s3.667-.015 4.947-.072c1.277-.06 2.148-.262 2.913-.558.788-.306 1.459-.718 2.126-1.384.666-.667 1.079-1.335 1.384-2.126.296-.765.499-1.636.558-2.913.06-1.28.072-1.687.072-4.947s-.015-3.667-.072-4.947c-.06-1.277-.262-2.149-.558-2.913-.306-.789-.718-1.459-1.384-2.126C21.319 1.347 20.651.935 19.86.63c-.765-.297-1.636-.499-2.913-.558C15.667.012 15.26 0 12 0zm0 2.16c3.203 0 3.585.016 4.85.071 1.17.055 1.805.249 2.227.415.562.217.96.477 1.382.896.419.42.679.819.896 1.381.164.422.36 1.057.413 2.227.057 1.266.07 1.646.07 4.85s-.015 3.585-.074 4.85c-.061 1.17-.256 1.805-.421 2.227-.224.562-.479.96-.899 1.382-.419.419-.824.679-1.38.896-.42.164-1.065.36-2.235.413-1.274.057-1.649.07-4.859.07-3.211 0-3.586-.015-4.859-.074-1.171-.061-1.816-.256-2.236-.421-.569-.224-.96-.479-1.379-.899-.421-.419-.69-.824-.9-1.38-.165-.42-.359-1.065-.42-2.235-.045-1.26-.061-1.649-.061-4.844 0-3.196.016-3.586.061-4.861.061-1.17.255-1.814.42-2.234.21-.57.479-.96.9-1.381.419-.419.81-.689 1.379-.898.42-.166 1.051-.361 2.221-.421 1.275-.045 1.65-.06 4.859-.06l.045.03zm0 3.678c-3.405 0-6.162 2.76-6.162 6.162 0 3.405 2.76 6.162 6.162 6.162 3.405 0 6.162-2.76 6.162-6.162 0-3.405-2.757-6.162-6.162-6.162zM12 16c-2.21 0-4-1.79-4-4s1.79-4 4-4 4 1.79 4 4-1.79 4-4 4zm7.846-10.405c0 .795-.646 1.44-1.44 1.44-.795 0-1.44-.646-1.44-1.44 0-.794.646-1.439 1.44-1.439.793-.001 1.44.645 1.44 1.439z"/>
          </svg>
        );
      default:
        return null;
    }
  };

  return (
    <div className="w-full h-full relative z-10">
      {/* 桌面端布局 - 绝对定位匹配设计稿 */}
      <div className="hidden lg:block w-full h-full">
        {/* 标题 - 设计稿@1600: left:80px(5%), top:30px(17.86%) */}
        <h1 
          className="absolute font-semibold text-white whitespace-nowrap leading-[1.32]"
          style={{ 
            fontSize: 'clamp(18px, 1.75vw, 28px)',
            left: '4%',
            top: '17.86%',
          }}
        >
          {t('title')}
        </h1>
        
        {/* 社交媒体图标 - 设计稿@1600: left:272px(17%), top:30px, width:335px(20.9%) */}
        <div 
          className="absolute flex items-center justify-between"
          style={{ 
            left: '17%',
            top: '17.86%',
            width: '20.9%',
          }}
        >
          {socialIcons.map((social) => {
            const IconWrapper = social.url ? 'a' : 'div';
            const wrapperProps = social.url ? {
              href: social.url,
              target: '_blank',
              rel: 'noopener noreferrer',
            } : {};
            
            return (
              <IconWrapper
                key={social.name}
                {...wrapperProps}
                className="flex flex-col items-center gap-0.5 flex-1"
              >
                <div 
                  className="rounded-full border border-white/40 flex items-center justify-center text-white hover:bg-white/10 transition-colors"
                  style={{ width: 'clamp(32px, 2.5vw, 40px)', height: 'clamp(32px, 2.5vw, 40px)' }}
                >
                  {renderSocialIcon(social.icon)}
                </div>
                <span 
                  className="text-text-secondary whitespace-nowrap text-center"
                  style={{ fontSize: 'clamp(10px, 0.75vw, 12px)' }}
                >
                  {social.name}
                </span>
              </IconWrapper>
            );
          })}
        </div>

        {/* 电话 - 设计稿@1600: left:980px(61.25%), top:30px */}
        {contactInfo?.phone && (
          <div 
            className="absolute flex items-center"
            style={{ 
              left: '61.25%',
              top: '17.86%',
              gap: 'clamp(12px, 1vw, 16px)',
            }}
          >
            <div 
              className="rounded-full border border-white/40 flex items-center justify-center text-white shrink-0"
              style={{ width: 'clamp(32px, 2.5vw, 40px)', height: 'clamp(32px, 2.5vw, 40px)' }}
            >
              <svg className="size-4 lg:size-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
              </svg>
            </div>
            <a 
              href={`tel:${contactInfo.phone}`} 
              className="underline hover:no-underline whitespace-nowrap text-white"
              style={{ fontSize: 'clamp(12px, 1vw, 16px)' }}
            >
              {contactInfo.phone}
            </a>
          </div>
        )}

        {/* 邮件联系方式 - 设计稿@1600: left:1248px(78%), top:31px */}
        <div 
          className="absolute flex items-start"
          style={{ 
            right: '4%',
            top: '18.45%',
            gap: 'clamp(12px, 1vw, 16px)',
          }}
        >
          <div 
            className="rounded-full border border-white/40 flex items-center justify-center text-white shrink-0"
            style={{ width: 'clamp(32px, 2.5vw, 40px)', height: 'clamp(32px, 2.5vw, 40px)' }}
          >
            <svg className="size-4 lg:size-5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
            </svg>
          </div>
          <div 
            className="flex flex-col text-white"
            style={{ 
              fontSize: 'clamp(12px, 1vw, 16px)',
              gap: 'clamp(8px, 0.75vw, 12px)',
            }}
          >
            {contactInfo?.department?.generalInformation && (
              <div className="flex items-center gap-1 whitespace-nowrap">
                <span>{t('contactInfo.general')}:</span>
                <a 
                  href={contactInfo.department.generalInformation.includes('@') 
                    ? `mailto:${contactInfo.department.generalInformation}` 
                    : `tel:${contactInfo.department.generalInformation}`} 
                  className="underline hover:no-underline"
                >
                  {contactInfo.department.generalInformation}
                </a>
              </div>
            )}
            {contactInfo?.department?.marketingDepartment && (
              <div className="flex items-center whitespace-nowrap">
                <span className="tracking-[5.12px]">{t('contactInfo.marketing')}:</span>
                <a 
                  href={contactInfo.department.marketingDepartment.includes('@') 
                    ? `mailto:${contactInfo.department.marketingDepartment}` 
                    : `tel:${contactInfo.department.marketingDepartment}`} 
                  className="underline hover:no-underline"
                >
                  {contactInfo.department.marketingDepartment}
                </a>
              </div>
            )}
            {contactInfo?.department?.complianceDepartment && (
              <div className="flex items-center gap-1 whitespace-nowrap">
                <span>{t('contactInfo.compliance')}:</span>
                <a 
                  href={contactInfo.department.complianceDepartment.includes('@') 
                    ? `mailto:${contactInfo.department.complianceDepartment}` 
                    : `tel:${contactInfo.department.complianceDepartment}`} 
                  className="underline hover:no-underline"
                >
                  {contactInfo.department.complianceDepartment}
                </a>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* 移动端和平板布局 - 响应式流式布局（标题隐藏，空间不够） */}
      <div 
        className="flex lg:hidden flex-col gap-3 w-full h-full justify-center"
        style={{ padding: 'clamp(12px, 3vw, 20px) clamp(16px, 4vw, 24px)' }}
      >
        {/* 社交媒体图标 - 水平排列 */}
        <div className="flex items-center gap-4">
          {socialIcons.filter(s => s.url).map((social) => (
            <a
              key={social.name}
              href={social.url}
              target="_blank"
              rel="noopener noreferrer"
              className="flex flex-col items-center gap-0.5"
            >
              <div className="size-8 rounded-full border border-white/40 flex items-center justify-center text-white hover:bg-white/10 transition-colors">
                {renderSocialIcon(social.icon)}
              </div>
              <span className="text-responsive-2xs text-text-secondary">{social.name}</span>
            </a>
          ))}
        </div>

        {/* 联系方式 - 垂直排列 */}
        <div className="flex flex-col gap-2 text-white text-sm">
          {contactInfo?.phone && (
            <div className="flex items-center gap-2">
              <div className="size-6 rounded-full border border-white/40 flex items-center justify-center shrink-0">
                <svg className="size-3" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
                </svg>
              </div>
              <a href={`tel:${contactInfo.phone}`} className="underline">{contactInfo.phone}</a>
            </div>
          )}
          {contactInfo?.department?.generalInformation && (
            <div className="flex items-center gap-2">
              <div className="size-6 rounded-full border border-white/40 flex items-center justify-center shrink-0">
                <svg className="size-3" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
                </svg>
              </div>
              <span>{t('contactInfo.general')}: </span>
              <a 
                href={contactInfo.department.generalInformation.includes('@') 
                  ? `mailto:${contactInfo.department.generalInformation}` 
                  : `tel:${contactInfo.department.generalInformation}`} 
                className="underline"
              >
                {contactInfo.department.generalInformation}
              </a>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// 横幅内容组件 - 公告
function NoticesBannerContent() {
  const t = useTranslations('supports');
  const latestNotice = useNoticesStore((state) => state.latestNotice);
  
  // 从第一条公告获取描述，如果没有则使用默认文案
  const description = latestNotice?.content 
    ? DOMPurify.sanitize(latestNotice.content, { ALLOWED_TAGS: [] }) // 移除所有HTML标签，只保留纯文本
    : t('banner.description');
  
  return (
    <>
      {/* 左侧3D图形 - 设计稿@1920: 283x283px, top:-62px, left:-49px (占比: 14.7vw) */}
      <div 
        className="absolute pointer-events-none"
        style={{ 
          width: 'clamp(120px, 14.7vw, 283px)',
          height: 'clamp(120px, 14.7vw, 283px)',
          top: 'clamp(-25px, -3.2vw, -62px)',
          left: 'clamp(-20px, -2.5vw, -49px)',
        }}
      >
        <Image 
          src="/images/banner/Generated3D-left.png" 
          alt="" 
          fill
          className="object-contain"
        />
      </div>
      
      {/* 右侧装饰图形1 - 设计稿@1920: 188x188px, top:71px, right:113px, rotate:-35.28° */}
      <div 
        className="absolute pointer-events-none hidden md:block z-10"
        style={{ 
          width: 'clamp(80px, 9.8vw, 188px)',
          height: 'clamp(80px, 9.8vw, 188px)',
          top: 'clamp(30px, 3.7vw, 71px)',
          right: 'clamp(50px, 5.9vw, 113px)',
          transform: 'rotate(-35.28deg)',
        }}
      >
        <Image 
          src="/images/banner/Generated3D-right1.png" 
          alt="" 
          fill
          className="object-contain"
        />
      </div>
      
      {/* 右侧装饰图形2（金色圆环） - 设计稿@1920: 247x247px, top:9px, right:-21px, rotate:10.86° */}
      <div 
        className="absolute pointer-events-none hidden sm:block z-20"
        style={{ 
          width: 'clamp(100px, 12.9vw, 247px)',
          height: 'clamp(100px, 12.9vw, 247px)',
          top: 'clamp(5px, 0.5vw, 9px)',
          right: 'clamp(-15px, -1.1vw, -21px)',
          transform: 'rotate(10.86deg)',
        }}
      >
        <Image 
          src="/images/banner/Generated3D-right2.png" 
          alt="" 
          fill
          className="object-contain"
        />
      </div>
      
      {/* 内容区域 - 设计稿@1600: 标题y:23, 描述y:71, 按钮y:117, 左侧271px */}
      <div 
        className="flex flex-col w-full h-full relative z-10"
        style={{
          paddingTop: 'clamp(14px, 1.44vw, 23px)',
          paddingBottom: 'clamp(12px, 1.3vw, 21px)',
          paddingLeft: 'clamp(100px, 16.9vw, 271px)',
          paddingRight: 'clamp(100px, 12vw, 200px)',
          gap: 'clamp(6px, 0.69vw, 11px)',
        }}
      >
        {/* 标题 - 设计稿: 28px字号, 37px高 */}
        <h1 
          className="font-semibold text-white leading-[1.32]"
          style={{ fontSize: 'clamp(16px, 1.75vw, 28px)' }}
        >
          MIDAS MARKET {t('banner.latestNews')}
          <span 
            className="bg-clip-text bg-linear-to-r from-(--color-primary) to-[#F17B7D] dark:to-[#7bd4f1]"
            style={{ WebkitTextFillColor: 'transparent' }}
          >
            {t('banner.realTime')}
          </span>
        </h1>
        
        {/* 描述文字 - 显示第一条公告内容 */}
        <p 
          className="text-text-secondary hidden md:block line-clamp-2"
          style={{ 
            fontSize: 'clamp(10px, 0.75vw, 12px)',
            lineHeight: '1.42',
            maxWidth: 'clamp(300px, 59vw, 943px)' 
          }}
        >
          {description}
        </p>
        
        {/* Deposit Now 按钮 - 设计稿: 107x30px, 11px字号, gap:12px */}
        <button 
          className="flex items-center gap-1 bg-primary text-white rounded w-fit border border-white/50 hover:bg-primary/90 transition-colors whitespace-nowrap backdrop-blur-sm"
          style={{
            fontSize: 'clamp(9px, 0.69vw, 11px)',
            padding: 'clamp(5px, 0.47vw, 7.5px) clamp(10px, 0.8vw, 13px)',
          }}
        >
          <span>Deposit Now</span>
          <svg className="size-2.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
          </svg>
        </button>
      </div>
    </>
  );
}

// 横幅内容组件 - 文档（设计稿@1920: 标题left:198px,top:28px; 副标题left:198px,top:100px; 图标left:1248px,top:-33px,230x230px）
function DocumentsBannerContent() {
  const t = useTranslations('supports');
  return (
    <div className="w-full h-full relative z-10">
      {/* 渐变标题 - 设计稿@1920: left:198px(10.3%), top:28px(16.67%), 40px字号 */}
      <h1 
        className="absolute font-semibold leading-[1.32] bg-clip-text bg-linear-to-r from-(--color-primary) to-[#F17B7D] dark:to-[#7bd4f1]"
        style={{ 
          fontSize: 'clamp(24px, 2.08vw, 40px)',
          left: 'clamp(20px, 10.3vw, 198px)',
          top: 'clamp(15px, 1.46vw, 28px)',
          WebkitTextFillColor: 'transparent'
        }}
      >
        {t('documents.bannerTitle')}
      </h1>
      
      {/* 副标题 - 设计稿@1920: left:198px(10.3%), top:100px(59.5%), 16px字号 */}
      <p 
        className="absolute text-text-secondary"
        style={{ 
          fontSize: 'clamp(12px, 0.83vw, 16px)',
          left: 'clamp(20px, 10.3vw, 198px)',
          top: 'clamp(55px, 5.2vw, 100px)',
        }}
      >
        {t('documents.bannerSubtitle')}
      </p>
      
      {/* 右侧文件夹图标 - 设计稿@1920: left:1248px(65%), top:-33px, 230x230px(12%) */}
      <div 
        className="absolute pointer-events-none hidden md:block"
        style={{ 
          width: 'clamp(120px, 12vw, 230px)',
          height: 'clamp(120px, 12vw, 230px)',
          top: 'clamp(-17px, -1.72vw, -33px)',
          right: 'clamp(122px, 0vw, 442px)',
        }}
      >
        {/* 日间模式图标 */}
        <Image 
          src="/images/banner/file-day.svg" 
          alt="" 
          fill
          className="object-contain dark:hidden"
        />
        {/* 夜间模式图标 */}
        <Image 
          src="/images/banner/file-night.svg" 
          alt="" 
          fill
          className="object-contain hidden dark:block"
        />
      </div>
    </div>
  );
}

export default function SupportsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const t = useTranslations('supports');
  const pathname = usePathname();
  const { theme } = useTheme();
  const userLoading = useUserStore((s) => s.isLoading);
  const isInitialized = useUserStore((s) => s.isInitialized);
  
  const isDark = theme === 'dark';
  
  // 检查是否为详情页（不显示 banner 和 tab）
  const isDetailPage = pathname.startsWith('/supports/notices/') && pathname !== '/supports/notices';

  if (!isInitialized || userLoading) {
    return <PageLoading fullscreen={false} className="py-20" />;
  }

  // 根据路由选择横幅内容
  const renderBannerContent = () => {
    if (pathname === '/supports/notices') {
      return <NoticesBannerContent />;
    }
    if (pathname === '/supports/documents') {
      return <DocumentsBannerContent />;
    }
    return <DefaultBannerContent />;
  };

  // 详情页：只显示内容区域，不显示 banner 和 tab
  if (isDetailPage) {
    return (
      <div className="flex flex-1 flex-col">
        <div className="flex flex-1 flex-col rounded-lg bg-surface p-5">
          {children}
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-1 flex-col gap-5">
      {/* 顶部横幅 - 渐变背景，isolate 确保圆角裁剪子元素 */}
      {/* 设计稿@1600: 高度168px, border-radius:4px, 背景渐变 182deg */}
      <div 
        className="overflow-hidden relative isolate"
        style={{
          height: 'clamp(120px, 10.5vw, 168px)',
          borderRadius: '4px',
          background: isDark 
            ? 'linear-gradient(182deg, #001d5e -39.96%, #000 99.54%)' 
            : 'linear-gradient(182deg, #67001A -39.96%, #000 99.54%)'
        }}
      >
        {renderBannerContent()}
      </div>

      {/* 导航和内容区域 */}
      <div className="flex flex-1 flex-col rounded-lg bg-surface p-5">
        {/* 导航 */}
        <Tabs
          tabs={navItems.map((item) => ({
            key: item.key,
            label: t(item.label),
            href: item.path,
          } as TabItem))}
          activeKey={navItems.find((item) => pathname === item.path)?.key || 'contact'}
          onChange={() => {}}
          size="base"
        />

        {/* 子页面内容 */}
        <div className="flex-1 pt-5">
          {children}
        </div>
      </div>
    </div>
  );
}
