export interface Paged<T, TC> {
  data: T[];
  criteria: TC;
  message: string;
  status: number;
}

interface IPagination<T> {
  data: T[];
  pageable: Pageable;
}

interface IPageable {
  page: number;
  size: number;
  total: number;
  totalPage: number;
}

export class Pageable implements IPageable {
  page: number;
  size: number;
  total: number;
  totalPage: number;

  constructor() {
    this.page = 1;
    this.size = 10;
    this.total = 0;
    this.totalPage = 0;
  }

  of(_p: unknown): Pageable {
    const p = _p as Pageable;
    this.page = p?.page || 0;
    this.size = p?.size || 0;
    this.total = p?.total || 0;
    this.totalPage = p?.totalPage || 0;
    return this;
  }
}

export class Pagination<T> implements IPagination<T> {
  data: T[];
  pageable: Pageable;

  constructor() {
    this.data = new Array<T>();
    this.pageable = new Pageable();
  }

  of(_p: unknown): Pagination<T> {
    const p = _p as Pagination<T>;
    this.data = p.data || [];
    this.pageable = new Pageable().of(p.pageable);
    return this;
  }
}
