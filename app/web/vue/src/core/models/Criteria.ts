export interface IPagination {
  page: number | null;
  size: number | null;
  total: number | null;
  pageCount: number | null;
  totalPage: number | null;
}

export interface ICriteria extends IPagination {
  id?: any | null;
  ids?: any[] | null;
  sortField?: string | null;
  sortFlag?: boolean | null;
  status?: number | null;
}
