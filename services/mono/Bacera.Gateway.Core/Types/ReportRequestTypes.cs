namespace Bacera.Gateway;

public enum ReportRequestTypes
{
    TradeForClient = 1,
    TradeForAgent = 2,
    TradeForSales = 3,
    TradeForTenant = 4,
    Rebate = 5,
    DepositForTenant = 6,
    WithdrawForTenant = 7,
    WithdrawPendingForTenant = 11,
    WithdrawUnionPayPendingForTenant = 21,
    WithdrawUSDTPendingForTenant = 22,
    TransactionForTenant = 8,
    WalletOverviewForTenant = 9,
    WalletTransactionForTenant = 10,
    SalesRebateForTenant = 12,
    SalesRebateSumByAccountForTenant = 13,
    AccountSearchForTenant = 14,
    SalesReportForTenant = 15,
    SalesWeeklyReportForTenant = 16,
    IbReportForTenant = 17,
    IbMonthlyReportForClient = 18,
    WalletDailySnapshot = 19,
    DemoAccount = 20,
    DailyEquity = 23,
    DailyEquityMonthly = 24,
}