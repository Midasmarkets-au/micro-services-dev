/**
 * Menu item
 */
declare interface AccountType {
  id: number;
  accountNumber: string;
  platform: string;
  tradeAccountStatus: TradeAccountStatus;
}
declare interface TradeAccountStatus {
  lastLoginOn: string;
  leverage: number;
  balance: number;
  prevMonthBalance: number;
  prevBalance: number;
  credit: number;
  interestRate: number;
  taxes: number;
  equity: number;
  margin: number;
  marginLevel: number;
  marginFree: number;
  currency: string;
}
declare interface AccountDetailType {
  account_number: string;
  platform: string;
  balance: number;
  levearge: number;
  profit: number;
  lots: number;
}
export { AccountType, AccountDetailType };
