import { Matter } from "@/core/models/Matter";
import { ICriteria } from "@/core/models/Criteria";

export interface Deposit {
  id: number;
  matterId: number;
  type: number;
  partyId: number;
  paymentId: number | null;
  currencyId: number;
  amount: number;
  matter: Matter | null;
}
export interface DepositCriteria extends ICriteria {
  partyId: number | null;
  matterId: number | null;
  from: string | null;
  to: string | null;
  stateId: number | null;
  currencyId: number | null;
}
