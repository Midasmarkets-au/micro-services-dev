import { ICriteria } from "@/core/models/Criteria";

export interface Wallet {
  id: number;
  partyId: number;
  type: number;
  currencyId: number;
  balance: number;
  talliedOn: string;
}

export interface WalletCriteria extends ICriteria {
  partyId?: number;
  type?: number;
  currencyId?: number;
}
