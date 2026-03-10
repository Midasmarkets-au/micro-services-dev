import { FundTypes } from "@/core/types/FundTypes";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";

export interface CreateByTenantSpec {
  partyId: number;
  amount: number;
  fundType: FundTypes;
  currencyId: CurrencyTypes;
  paymentServiceId: number;
  targetTradeAccountUid: number;
  note: string;
  referenceNumber: string;
  request: any;
}
