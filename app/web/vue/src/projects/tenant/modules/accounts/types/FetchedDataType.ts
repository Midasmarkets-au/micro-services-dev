import { UserType } from "@/core/types/UserType";
import { SupplementType } from "./SupplementType";

export interface FetchedDataType {
  approvedBy: string | null;
  approvedOn: string | null;
  createdOn: string;
  id: number;
  partyId: number;
  rejectedBy: string | null;
  rejectedOn: string | null;
  rejectedReason: string | null;
  supplement: SupplementType;
  type: number;
  updatedOn: string;
  user: UserType;
}
