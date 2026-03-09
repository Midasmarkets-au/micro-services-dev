import { ICriteria } from "@/core/models/Criteria";

export interface VerificationCriteria extends ICriteria {
  partyId: number | null;
  type: string | null;
}

export type AccountInfoCriteria = ICriteria & {
  status: number | null;
  code: string | null;
  role: number | null;
  group: string | null;
  partyId: number | null;
};

export type ApplicationCriteria = ICriteria & {
  referenceId: number | null;
};
