// ============================================
// 入金（Deposit）相关类型定义
// ============================================

// Step 1: 支付渠道
export interface DepositGroup {
  group: string;
  logo: string;
  paymentMethodName: string;
  isActive?: boolean;
  rang?: [number, number];
}

// Step 2: 支付渠道详情
export interface CurrencyRate {
  currencyId: number;
  rate: number;
}

export interface DepositGroupInfo {
  policy: string;
  currencyRates: CurrencyRate[];
  range: [number, number];
  requestKeys: string[];
  requestValues: Record<string, any>;
  hashId: string;
  instruction: string;
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
