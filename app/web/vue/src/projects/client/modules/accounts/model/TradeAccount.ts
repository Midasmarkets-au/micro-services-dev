import { ICriteria } from "@/core/models/Criteria";

export enum PlatformTypes {
  MetaTrader4Co = 10,
  MetaTrader4CoDemo = 11,
  MetaTrader4 = 20,
  MetaTrader4Demo = 21,
  MetaTrader5 = 30,
  MetaTrader5Demo = 31,
  CTrader = 40,
}

export class TradeServiceTypes {
  public static MT4Co: TradeService = {
    id: 10,
    platform: PlatformTypes.MetaTrader4Co,
    name: "MetaTrader 4",
    description: "MetaTrader 4",
  };
  public static MT4CoDemo: TradeService = {
    id: 11,
    platform: PlatformTypes.MetaTrader4CoDemo,
    name: "MetaTrader 4 Demo",
    description: "MetaTrader 4 Demo",
  };
  public static MT4: TradeService = {
    id: 20,
    platform: PlatformTypes.MetaTrader4,
    name: "MetaTrader 4",
    description: "MetaTrader 4",
  };
  public static MT4Demo: TradeService = {
    id: 21,
    platform: PlatformTypes.MetaTrader4Demo,
    name: "MetaTrader 4 Demo",
    description: "MetaTrader 4 Demo",
  };
  public static MT5: TradeService = {
    id: 30,
    platform: PlatformTypes.MetaTrader5,
    name: "MetaTrader 5",
    description: "MetaTrader 5",
  };
  public static MT5Demo: TradeService = {
    id: 31,
    platform: PlatformTypes.MetaTrader5Demo,
    name: "MetaTrader 5 Demo",
    description: "MetaTrader 5 Demo",
  };
  public static CTrader: TradeService = {
    id: 40,
    platform: PlatformTypes.CTrader,
    name: "CTrader",
    description: "CTrader",
  };
  public static All: TradeService[] = [
    this.MT4Co,
    this.MT4CoDemo,
    this.MT4,
    this.MT4Demo,
    this.MT5,
    this.MT5Demo,
    this.CTrader,
  ];
  public static Active: TradeService[] = [this.MT4, this.MT4Demo];
}

export interface TradeService {
  id: number;
  platform: number;
  name: string;
  description: string;
}

export interface Account {
  id: number;
  partyId: number;
  type: number;
  aid: number;
  referrerAccountId: number | null;
  salesAccountId: number | null;
  hasTradeAccount: boolean;
  name: string;
  code: string;
  group: string;
  referCode: string | null;
  createdOn: string;
  updatedOn: string;
  referPath: string;
  tradeAccount: TradeAccount | null;
}

export interface TradeAccount {
  id: number;
  serviceId: number;
  accountNumber: number;
  currencyId: number;
  lastSyncedOn: string | null;
  createdOn: string;
  updatedOn: string;
  referPath: string;
  currency: string;
  tradeAccountStatus: TradeAccountStatus | null;
}

export interface TradeAccountStatus {
  id: number;
  lastLoginOn: string | null;
  leverage: number;
  agentAccount: number;
  balance: number;
  prevMonthBalance: number;
  prevBalance: number;
  credit: number;
  interestRate: number;
  taxes: number;
  equity: number;
  margin: number;
  marginLevel: number;
  marginFree: number;
  currency: string;
  modifiedOn: string | null;
  createdOn: string;
  updatedOn: string;
}

export interface TradeDemoAccount {
  id: number;
  partyId: number;
  serviceId: number;
  accountNumber: number;
  expireOn: string;
  createdOn: string;
  updatedOn: string;
}

export interface AccountCriteria extends ICriteria {
  partyId?: number;
  referrerAccountId?: number;
  salesAccountId?: number;
  hasTradeAccount?: boolean;
  keyword?: string;
  type?: number;
  roles?: number[];
  salesUid?: number;
  agentUid?: number;
}

export interface TradeAccountCriteria extends ICriteria {
  partyId?: number;
  serviceId?: number;
  accountNumber?: number;
}
export interface TradeDemoAccountCriteria extends ICriteria {
  partyId?: number;
  serviceId?: number;
  accountNumber?: number;
}
