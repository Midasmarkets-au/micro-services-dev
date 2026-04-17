// 验证步骤类型
export type VerificationStep = 'started' | 'info' | 'financial' | 'agreement' | 'document' | 'complete';

// 验证状态
export type VerificationStatus = 0 | 1 | 2 | 3; // 0=未开始, 1=进行中, 2=已提交, 3=已通过

// 推荐历史中的 verification 字段（IB / Sales 共用）
// status: 0=未完成 1=等待审核 2=审核中 3=等待批准 4=已批准 5=已拒绝
export interface ReferralHistoryVerification {
  isEmpty: boolean;
  status: number;
}

// 账户信息 (Started)
export interface StartedData {
  currency: number;
  accountType: number;
  platform: number;
  serviceId: number;
  leverage: number;
  referral?: string;
  questions?: {
    q1: boolean;
    q2: boolean;
    q3: boolean;
    q4: boolean;
  };
}

// 社交媒体
export interface SocialMedium {
  name: string;
  account: string;
}

// 个人信息 (Info)
export interface InfoData {
  firstName: string;
  lastName: string;
  priorName?: string;
  birthday: string;
  gender: string; // '0' = 男, '1' = 女
  citizen: string; // 国家代码
  ccc: string; // 电话区号
  phone: string;
  email: string;
  address: string;
  idType: number; // 1=身份证, 2=驾照, 3=护照
  idNumber: string;
  idIssuer?: string;
  idIssuedOn?: string;
  idExpireOn?: string;
  socialMedium?: SocialMedium[];
}

// 财务信息 (Financial) - 可选
export interface FinancialData {
  annualIncome?: string;
  netWorth?: string;
  sourceOfFunds?: string;
  employmentStatus?: string;
  occupation?: string;
}

// 协议信息 (Agreement)
export interface AgreementData {
  consent_1: boolean;
  consent_2: boolean;
  consent_3: boolean;
}

// 文档类型
export type DocumentType = 'id_front' | 'id_back' | 'address';

// 文档状态
export type DocumentStatus = 0 | 1 | 2; // 0=待审核, 1=已通过, 2=已拒绝

// 文档信息
export interface DocumentData {
  status: DocumentStatus;
  documentType: DocumentType;
  approvedOn?: string;
  rejectedOn?: string;
  rejectedReason?: string;
  id: number;
  guid: string;
  url?: string;
  type: string;
  fileName: string;
  contentType: string;
  context?: string;
}

// 验证数据完整结构
export interface VerificationData {
  id: number;
  partyId: number;
  status: VerificationStatus;
  createdOn: string;
  updatedOn: string;
  operator?: string;
  started?: StartedData;
  info?: InfoData;
  financial?: FinancialData;
  quiz?: unknown;
  agreement?: AgreementData;
  document?: DocumentData[];
  siteId: number;
  user: {
    id: number;
    uid: number;
    partyId: number;
    email: string;
    avatar: string;
    language: string;
    idNumber: string;
    phone: string;
    lastLoginIp: string;
    status: number;
    hasComment: boolean;
    isInIpBlackList: boolean;
    isInUserBlackList: boolean;
    partyTags: string[];
    nativeName: string;
    firstName: string;
    lastName: string;
    displayName: string;
  };
  comments: unknown[];
  hasComment: boolean;
}

// 验证响应
export interface VerificationResponse {
  settings: VerificationStep[];
  data: VerificationData;
}

// 选项类型
export interface SelectOption {
  value: string | number;
  label: string;
}

// 货币选项
export const currencyOptions: SelectOption[] = [
  { value: 840, label: 'USD' },
  { value: 978, label: 'EUR' },
  { value: 156, label: 'CNY' },
  { value: 36, label: 'AUD' },
];

// 账户类型选项
export const accountTypeOptions: SelectOption[] = [
  { value: 4, label: 'Standard' },
  { value: 1, label: 'Pro' },
  { value: 2, label: 'VIP' },
];

// 平台选项
export const platformOptions: SelectOption[] = [
  { value: 30, label: 'MT5' },
  { value: 20, label: 'MT4' },
];

// 杠杆选项
export const leverageOptions: SelectOption[] = [
  { value: 1000, label: '1000:1' },
  { value: 500, label: '500:1' },
  { value: 200, label: '200:1' },
  { value: 100, label: '100:1' },
];

// 证件类型选项
export const idTypeOptions: SelectOption[] = [
  { value: 1, label: '身份证' },
  { value: 2, label: '驾照' },
  { value: 3, label: '护照' },
];

// 性别选项
export const genderOptions: SelectOption[] = [
  { value: '0', label: '男' },
  { value: '1', label: '女' },
];
