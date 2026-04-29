// ============================================
// 入金（Deposit）相关类型定义
// ============================================

// Step 1: 支付渠道
export interface DepositGroup {
  group: string;
  logo: string;
  paymentMethodName: string;
  isActive?: boolean;
  range?: [number, number];
  /** 渠道类型标识，例如 "ExLinkGlobal"。后端按类型驱动差异化逻辑 */
  type?: string;
}

// Step 2: 支付渠道详情
export interface CurrencyRate {
  currencyId: number;
  rate: number;
}

/** 按币种维度的渠道配置（ExLinkGlobal 等以币种为单位维护 hashId / range / 名称） */
export interface PaymentMethodConfig {
  currencyId: number;
  hashId: string;
  range: [number, number];
  /** 该币种对应的子渠道展示名（ExLinkGlobal 用它替代 selectedGroup.paymentMethodName） */
  paymentMethodName?: string;
}

export interface DepositGroupInfo {
  policy: string;
  currencyRates: CurrencyRate[];
  range: [number, number];
  requestKeys: string[];
  requestValues: Record<string, any>;
  hashId: string;
  instruction: string;
  /** 仅 ExLinkGlobal 等按币种独立配置的渠道返回 */
  paymentMethods?: PaymentMethodConfig[];
}

// ExLink Global 币种
export interface ExLinkCurrency {
  currencyId: number;
  name?: string;
  [key: string]: unknown;
}

// ExLink Global 汇率
export interface ExLinkExchangeRate {
  sourceCoinId: number;
  marketInPrice: number;
  [key: string]: unknown;
}

export interface ExLinkExchangeRatesResponse {
  marketPriceList: ExLinkExchangeRate[];
}

// Step 4: 提交入金请求参数
export interface DepositRequest {
  hashId: string;
  amount: number;
  request: Record<string, any>;
}

// Step 4: 提交入金响应
export enum DepositActions {
  Redirect = 'Redirect',
  Post = 'Post',
  QrCode = 'QrCode',
  PayPal = 'PayPal',
}

export interface DepositResponse {
  isSuccess: boolean;
  action: string;
  redirectUrl?: string;
  endPoint?: string;
  form?: Record<string, string>;
  instruction?: string;
  textForQrCode?: string;
  message?: number;
  error?: string;
}

// 贯穿全流程的数据容器
export interface PaymentRequireData {
  account: {
    uid: number;
    currencyId: number;
  };
  group: string;
  logo: string;
  paymentMethodName: string;
  groupInfo: DepositGroupInfo | null;
  request: Record<string, any>;
  targetAmount: number;
}
