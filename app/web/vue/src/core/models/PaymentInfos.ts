import { PaymentPlatformTypes } from "@/core/types/PaymentTypes";
import { Criteria } from "@/core/types/Criteria";

export type PaymentInfoTenantModal = {
  id: number;
  partyId: number;
  paymentPlatform: PaymentPlatformTypes;
  paymentServiceId: number | null;
  createdOn: string;
  updatedOn: string;
  name: string;
  info: any;
};

export type PaymentInfoCriteria = Criteria & {
  platform: PaymentPlatformTypes | null;
  partyId: number | null;
  keyword: string | null;
};
