import { ICriteria } from "@/core/models/Criteria";
import { Matter } from "@/core/models/Matter";

export interface WalletTransfer {
  id: number;
  walletId: number;
  invoiceId: number | null;
  matterId: number;
  prevBalance: number;
  amount: number;
  createdOn: string;
  updatedOn: string;
  matter: Matter | null;
}

export interface WalletTransferCriteria extends ICriteria {
  partyId: number | null;
  walletId: number | null;
  matterId: number | null;
  from: string | null;
  to: string | null;
}
