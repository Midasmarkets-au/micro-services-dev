import { ReferralServiceTypes } from "@/core/types/ReferralServiceTypes";

export interface TenantWithReferredAccountInfoResponseModel {
  name: string;
  code: string;
  referredCount: number;
  serviceType: ReferralServiceTypes;
  supplement: any;
  accountId: number;
  salesAccountId: number;
  salesName: string;
  accountName: string;
  salesAccountCode: string;
  accountGroupName: string;
}
