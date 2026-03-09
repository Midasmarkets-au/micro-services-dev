export enum ApplicationStatusType {
  AwaitingApproval = 1,
  Approved = 2,
  Rejected = 3,
  Completed = 4,
}

export enum ApplicationType {
  Empty = -1,
  Account = 1,
  SalesAccount = 10,
  IbAccount = 20,
  TradeAccount = 100,
  TradeAccountChangePassword = 102,
  TradeAccountChangeLeverage = 103,
  TradeDemoAccount = 110,
  WholeSaleAccount = 101,
  WholesaleReferral = 104,
}

export const getApplicationStatusType = () => {
  const reversed = Object.entries(ApplicationStatusType)
    .filter(([key, value]) => typeof value === "number")
    .reduce((obj, [key, value]) => ({ ...obj, [value]: key }), {});
  return reversed;
};
