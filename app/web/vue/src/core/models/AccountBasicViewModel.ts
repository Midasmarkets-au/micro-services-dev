import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { UserBasicViewModel } from "@/core/models/UserBasicViewModel";

export interface AccountBasicViewModel {
  id: number;
  uid: number;
  partyId: number;
  hasComment: boolean | null;
  role: AccountRoleTypes;
  code: string;
  group: string;
  accountNumber: number;
  user: UserBasicViewModel;
}
