import {
  AccountRoleTypes,
  AccountStatusTypes,
  AccountTypes,
} from "@/core/types/AccountInfos";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { FundTypes } from "@/core/types/FundTypes";
import { Criteria } from "@/core/types/Criteria";

export interface Account {}

export interface AccountCriteria extends Criteria {
  uid: number | null;
  uids: number[] | null;
  code: string | null;
  partyId: number | null;
  keywords: string | null;
  type: AccountTypes | null;
  hasTradeAccount: boolean | null;
  role: AccountRoleTypes | null;
  roles: number[] | null;
  referrerAccountId: number | null;
  status: AccountStatusTypes | null;
  group: string | null;
  currencyId: CurrencyTypes | null;
  fundType: FundTypes | null;
  groupId: number | null;
  salesId: number | null;
  salesUid: number | null;
  agentId: number | null;
  agentUid: number | null;
  repId: number | null;
  repUid: number | null;
}
