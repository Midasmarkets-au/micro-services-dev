export interface Result<T, TC> {
  status: ResultStatus;
  data: T;
  criteria: TC;
  message: string | null;
}

export enum ResultStatus {
  Success = 1,
  Error = 2,
}
