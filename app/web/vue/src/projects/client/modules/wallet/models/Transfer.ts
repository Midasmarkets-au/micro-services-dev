import { Matter } from "@/core/models/Matter";
import { ICriteria } from "@/core/models/Criteria";

export interface Transfer {
  id: number;
  matterId: number;
  partyId: number;
  ledgerSide: number;
  senderPartyId: number;
  senderAccountId: number;
  senderAccountType: number;
  receiverPartyId: number;
  receiverAccountId: number;
  receiverAccountType: number;
  currencyId: number;
  amount: number;
  createdOn: string;
  matter: Matter;
}

export interface TransferCriteria extends ICriteria {
  partyId: number | null;
  matterId: number | null;
  from: string | null;
  to: string | null;
  stateId: number | null;
}
