export interface RebateInterface {
  sid: string;
  r: number;
  p: number;
  c: number;
}

export interface RebateRuleInterface {
  accountId: number | null;
  rate: number;
  pipe: number;
  commission: number;
  rules: Array<RebateInterface>;
}
