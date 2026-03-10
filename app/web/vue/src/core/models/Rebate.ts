import { CurrencyTypes } from "../types/CurrencyTypes";
import { AccountBasicViewModel } from "@/core/models/AccountBasicViewModel";
import { TenantTradeTransactionForRebateResponseModel } from "@/core/models/TradeTransaction";
import { Criteria } from "@/core/types/Criteria";

export interface TenantRebateViewModel {
  amount: number;
  partyId: number;
  postedOn: string;
  currencyId: CurrencyTypes;
  targetAccount: AccountBasicViewModel;
  trade: TenantTradeTransactionForRebateResponseModel;
}

export interface TenantRebateCriteria extends Criteria {
  partyId: number | null;
  accountId: number | null;
  accountUid: number | null;
  salesUid: number | null;
  agentUid: number | null;
  ticketNumber: number | null;
  sourceAccountUid: number | null;
  targetAccountUid: number | null;
  currencyId: CurrencyTypes | null;
  createdFrom: string | null;
  createdTo: string | null;
  from: string | null;
  to: string | null;
}
