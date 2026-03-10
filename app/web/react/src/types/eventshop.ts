export enum EventPartyStatusTypes {
  Applied = 1,
  Approved = 2,
  Rejected = 3,
  Cancelled = 4,
}

export enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Shipped = 2,
  Succeed = 3,
  Cancelled = 4,
}

export enum PointTransactionStatus {
  Pending = 0,
  Success = 1,
  Fail = 2,
}

export enum PointTransactionSource {
  OpenAccount = 0,
  Deposit = 1,
  Trade = 2,
  Adjust = 3,
  Purchase = 4,
}

export enum RewardRebateStatus {
  Pending = 0,
  Processing = 1,
  Succeed = 2,
  Failed = 3,
}

export interface EventDetail {
  hashId: string;
  title: string;
  term: string;
  description: string;
  startOn: string;
  endOn: string;
  status: number;
  instruction?: {
    pointsRule?: {
      all?: string;
      agent?: string;
      client?: string;
      sales?: string;
    };
  };
  [key: string]: unknown;
}

export interface EventUserDetail {
  hashId: string;
  status: EventPartyStatusTypes;
  point: number;
  notavailable: number;
  level: number;
  [key: string]: unknown;
}

export interface ShopCategory {
  hashId: string;
  name: string;
  sortOrder?: number;
}

export interface ShopItem {
  hashId: string;
  title: string;
  name: string;
  images: string[];
  point: number;
  status: number;
  category: number;
  type: number;
  createdOn: string;
  updatedOn: string;
  configuration?: {
    rewardType: number;
    validPeriodInDays: number;
  };
  [key: string]: unknown;
}

export interface OrderAddress {
  hashId: string;
  name: string;
  ccc: string;
  phone: string;
  country: string;
  content: {
    address: string;
    city: string;
    state: string;
    postalCode: string;
    [key: string]: unknown;
  };
  [key: string]: unknown;
}

export interface OrderShipping {
  trackingNumber?: string;
  [key: string]: unknown;
}

export interface ShopOrder {
  hashId: string;
  eventShopItemName: string;
  totalPoint: number;
  quantity: number;
  status: number;
  createdOn: string;
  updatedOn: string;
  shippedOn?: string;
  eventShopItemImages?: string[];
  eventShopItemCategory?: string;
  address?: OrderAddress;
  shipping?: OrderShipping;
  comment?: string;
  description?: string;
  [key: string]: unknown;
}

export interface PointTransaction {
  hashId: string;
  point: number;
  status: number;
  sourceType: number;
  description: string;
  createdOn: string;
  eventShopItemName?: string;
  eventShopItemImages?: string[];
  ticket?: number;
  [key: string]: unknown;
}

export interface RewardRebate {
  hashId: string;
  ticket: number;
  symbol: string;
  amount: number;
  status: number;
  openAt: string;
  closeAt: string;
  createdOn: string;
  [key: string]: unknown;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  size: number;
}

export interface CriteriaParams {
  page?: number;
  size?: number;
  sortField?: string;
  categoryName?: string;
  [key: string]: string | number | undefined;
}
