export interface PublicSetting {
  siteId: number;
  defaultFundType: number;
  fundTypeAvailable: number[];
  accountTypeAvailable: number[];
  currencyAvailable: number[];
  leverageAvailable: number[];
  leverageForWholesaleAvailable: number[];
  tradingPlatformAvailable: number[];
  demoTradingPlatformAvailable: number[];
  ibEnabled: boolean;
  rebateEnabled: boolean;
  smsVerificationEnabled: boolean;
  webTraderEnabled: boolean;
  verificationQuizEnabled: boolean;
  contactInfo: Record<string, string>;
  newYearEvent: boolean;
  twoFactorAuth: any;
}

type General = {
  mode: string;
  primaryColor: string;
  pageWidth: string;
  layout: string;
};

type DefaultHeader = {
  container: string;
  fixed: {
    desktop: boolean;
    mobile: boolean;
  };
  menu: {
    display: boolean;
    iconType: string;
  };
};

type Header = {
  display: boolean;
  default: DefaultHeader;
};

type DefaultSidebar = {
  minimize: {
    desktop: {
      enabled: boolean;
      default: boolean;
      hoverable: boolean;
    };
  };
  menu: {
    iconType: string;
  };
};

type Sidebar = {
  display: boolean;
  default: DefaultSidebar;
};

type DefaultToolbar = {
  desktop: boolean;
  mobile: boolean;
};

type Toolbar = {
  display: boolean;
  container: string;
  fixed: DefaultToolbar;
};

type PageTitle = {
  display: boolean;
  breadcrumb: boolean;
  direction: string;
};

type Content = {
  container: string;
};

type DefaultFooter = {
  desktop: boolean;
  mobile: boolean;
};

type Footer = {
  display: boolean;
  container: string;
  fixed: DefaultFooter;
};

type Illustrations = {
  set: string;
};

type ScrollTop = {
  display: boolean;
};

export type TenantConfigType = {
  general: General;
  header: Header;
  sidebar: Sidebar;
  toolbar: Toolbar;
  pageTitle: PageTitle;
  content: Content;
  footer: Footer;
  illustrations: Illustrations;
  scrolltop: ScrollTop;
  defaultFundType: number;
  fundTypeAvailable: number[];
  accountTypeAvailable: number[];
  currencyAvailable: number[];
  leverageAvailable: number[];
  leverageForWholesaleAvailable: number[];
  tradingPlatformAvailable: number[];
  ibEnabled: boolean;
  rebateEnabled: boolean;
  smsVerificationEnabled: boolean;
};
