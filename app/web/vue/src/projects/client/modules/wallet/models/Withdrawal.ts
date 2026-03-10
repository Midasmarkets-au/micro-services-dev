import { Matter } from "@/core/models/Matter";

export interface Withdrawal {
  id: number;
  matterId: number;
  partyId: number;
  walletId: number;
  amount: number;
  matter: Matter;
}
import { ICriteria } from "@/core/models/Criteria";

export interface WithdrawalCriteria extends ICriteria {
  partyId: number | null;
  walletId: number | null;
  from: string | null;
  to: string | null;
  stateId: number | null;
}
