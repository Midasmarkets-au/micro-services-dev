import { Matter } from "@/core/models/Matter";

export interface Rebate {
  id: number;
  matterId: number;
  partyId: number;
  accountId: number;
  type: number;
  rowId: number;
  currencyId: number;
  amount: number;
  unfreezeOn: string | null;
  matter: Matter | null;
}

export interface RebateDailyReport {
  date: string;
  currencyId: number;
  amount: number;
  count: number;
  stateId: number;
}
