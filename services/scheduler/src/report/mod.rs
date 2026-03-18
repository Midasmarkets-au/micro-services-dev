pub mod account;
pub mod csv;
pub mod daily_equity;
pub mod deposit;
pub mod equity;
pub mod rebate;
pub mod request;
pub mod sales;
pub mod trade;

use serde_repr::{Deserialize_repr, Serialize_repr};

/// Mirrors Bacera.Gateway.ReportRequestTypes enum
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize_repr, Deserialize_repr)]
#[repr(i32)]
pub enum ReportRequestType {
    TradeForClient = 1,
    TradeForAgent = 2,
    TradeForSales = 3,
    TradeForTenant = 4,
    Rebate = 5,
    DepositForTenant = 6,
    WithdrawForTenant = 7,
    TransactionForTenant = 8,
    WalletOverviewForTenant = 9,
    WalletTransactionForTenant = 10,
    WithdrawPendingForTenant = 11,
    SalesRebateForTenant = 12,
    SalesRebateSumByAccountForTenant = 13,
    AccountSearchForTenant = 14,
    SalesReportForTenant = 15,
    SalesWeeklyReportForTenant = 16,
    IbReportForTenant = 17,
    IbMonthlyReportForClient = 18,
    WalletDailySnapshot = 19,
    DemoAccount = 20,
    WithdrawUnionPayPendingForTenant = 21,
    WithdrawUSDTPendingForTenant = 22,
    DailyEquity = 23,
}

impl TryFrom<i32> for ReportRequestType {
    type Error = anyhow::Error;

    fn try_from(v: i32) -> Result<Self, Self::Error> {
        match v {
            1 => Ok(Self::TradeForClient),
            2 => Ok(Self::TradeForAgent),
            3 => Ok(Self::TradeForSales),
            4 => Ok(Self::TradeForTenant),
            5 => Ok(Self::Rebate),
            6 => Ok(Self::DepositForTenant),
            7 => Ok(Self::WithdrawForTenant),
            8 => Ok(Self::TransactionForTenant),
            9 => Ok(Self::WalletOverviewForTenant),
            10 => Ok(Self::WalletTransactionForTenant),
            11 => Ok(Self::WithdrawPendingForTenant),
            12 => Ok(Self::SalesRebateForTenant),
            13 => Ok(Self::SalesRebateSumByAccountForTenant),
            14 => Ok(Self::AccountSearchForTenant),
            15 => Ok(Self::SalesReportForTenant),
            16 => Ok(Self::SalesWeeklyReportForTenant),
            17 => Ok(Self::IbReportForTenant),
            18 => Ok(Self::IbMonthlyReportForClient),
            19 => Ok(Self::WalletDailySnapshot),
            20 => Ok(Self::DemoAccount),
            21 => Ok(Self::WithdrawUnionPayPendingForTenant),
            22 => Ok(Self::WithdrawUSDTPendingForTenant),
            23 => Ok(Self::DailyEquity),
            _ => Err(anyhow::anyhow!("Unknown ReportRequestType: {}", v)),
        }
    }
}
