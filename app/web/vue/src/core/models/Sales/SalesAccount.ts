import { UserBasicViewModel } from "@/core/models/UserBasicViewModel";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { FundTypes } from "@/core/types/FundTypes";

export interface SalesAccount {
  uid: number;
  accountNumber: number;
  createdOn: string;
  role: AccountRoleTypes;
  user: UserBasicViewModel;
  currencyId: CurrencyTypes;
  fundType: FundTypes;
  balanceInCents: number;
}
