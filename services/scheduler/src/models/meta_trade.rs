use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct MetaTrade {
    pub id: i64,
    pub tenant_id: i64,
    pub account_number: i64,
    pub service_id: i32,
    pub ticket: i64,
    pub symbol: String,
    pub cmd: i32,               // action: 0=buy, 1=sell
    pub open_at: Option<chrono::DateTime<chrono::Utc>>,
    pub close_at: Option<chrono::DateTime<chrono::Utc>>,
    pub time_stamp: i64,        // TimeMsc as i64
    pub position: Option<i64>,
    pub digits: i32,
    pub volume: f64,            // volume as float (VolumeClosed / 10000.0)
    pub volume_original: i32,   // VolumeClosedExt / 100
    pub open_price: Option<f64>,
    pub close_price: Option<f64>,
    pub reason: i32,
    pub profit: f64,
    pub commission: f64,
    pub swaps: f64,             // from Storage field
}

#[derive(Debug)]
pub struct NewTradeRebate {
    pub account_id: Option<i64>,
    pub trade_service_id: i32,
    pub ticket: i64,
    pub account_number: i64,
    pub currency_id: i32,
    pub volume: i32,
    pub status: i32,
    pub rule_type: i32,
    pub opened_on: chrono::DateTime<chrono::Utc>,
    pub closed_on: chrono::DateTime<chrono::Utc>,
    pub time_stamp: i64,
    pub action: i32,
    pub deal_id: i64,
    pub symbol: String,
    pub refer_path: String,
    pub commission: f64,
    pub swaps: f64,
    pub open_price: f64,
    pub close_price: f64,
    pub profit: f64,
    pub reason: i32,
}

impl MetaTrade {
    /// 转换为 NewTradeRebate（初始值，handler 后续填充 account_id/currency_id/refer_path）
    pub fn to_new_trade_rebate(&self) -> NewTradeRebate {
        NewTradeRebate {
            account_id: None,
            trade_service_id: self.service_id,
            ticket: self.ticket,
            account_number: self.account_number,
            currency_id: -1,    // CurrencyTypes.Invalid，handler 后续填充
            volume: self.volume_original,
            status: 0,          // TradeRebateStatusTypes.Created
            rule_type: 199,
            opened_on: self.open_at.unwrap_or(chrono::DateTime::<chrono::Utc>::MIN_UTC),
            closed_on: self.close_at.unwrap_or(chrono::DateTime::<chrono::Utc>::MIN_UTC),
            time_stamp: self.time_stamp,
            action: self.cmd,
            deal_id: self.ticket,
            symbol: self.symbol.clone(),
            refer_path: String::new(),   // handler 后续填充
            commission: self.commission,
            swaps: self.swaps,
            open_price: self.open_price.unwrap_or(0.0),
            close_price: self.close_price.unwrap_or(0.0),
            profit: self.profit,
            reason: self.reason,
        }
    }
}
