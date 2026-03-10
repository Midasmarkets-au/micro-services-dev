export interface TradeTransaction {
  id: number;
  tradeAccountId: number;
  accountNumber: number;
  serviceId: number;
  ticket: number;
  symbol: string;
  digits: number;
  cmd: number;
  volume: number;
  openAt: string;
  openPrice: number;
  sl: number;
  tp: number;
  closeAt: string | null;
  closePrice: number;
  expiresAt: string | null;
  reason: number;
  convertRate: number;
  convertRate2: number;
  commission: number;
  commissionAgent: number;
  swaps: number;
  profit: number;
  taxes: number;
  comment: string;
  marginRate: number;
  modifiedAt: string;
  createdOn: string;
  updatedOn: string;
}
export interface TradeTransactionCriteria {
  partyId: number | null;
  accountNumber: number | null;
  accountId: number | null;
  serviceId: number | null;
  ticket: number | null;
  symbol: string | null;
  from: string | null;
  to: string | null;
  openedFrom: string | null;
  openedTo: string | null;
  closedFrom: string | null;
  closedTo: string | null;
}
