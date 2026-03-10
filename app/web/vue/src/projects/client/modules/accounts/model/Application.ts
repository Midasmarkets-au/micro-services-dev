import { ICriteria } from "@/core/models/Criteria";

export interface Application {
  id: number;
  partyId: number;
  type: number;
  approvedOn: string | null;
  approvedBy: number | null;
  rejectedOn: string | null;
  rejectedBy: number | null;
  rejectedReason: string | null;
  createdOn: string;
  updatedOn: string;
  supplement: unknown | null;
  user: unknown | null;
}

export interface ApplicationCriteria extends ICriteria {
  partyId: number | null;
  type: number | null;
  from: string | null;
  to: string | null;
  /**
   *     AwaitingApproval = 1,
   *     Approved = 2,
   *     Rejected = 3,
   */
  status: number | null;
}
