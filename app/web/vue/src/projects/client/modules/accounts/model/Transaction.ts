import { Matter } from "@/core/models/Matter";

export interface Transaction {
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
  matter: Matter | null;
}
