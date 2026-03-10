import { Pagination } from "@/core/types/Pagination";

export interface Criteria extends Pagination {
  id: number;
  ids: number[];
  page: number;
  size: number;
  total: number;
  pageCount: number;
  hasMore: boolean;
  sortField: string | null;
  sortFlag: boolean;
}
