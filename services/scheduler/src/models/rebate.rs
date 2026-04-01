use chrono::{DateTime, Utc};
use rust_decimal::Decimal;

/// MatterType::Rebate = 500
pub const MATTER_TYPE_REBATE: i32 = 500;
/// StateType::RebateOnHold = 510
pub const STATE_REBATE_ON_HOLD: i32 = 510;

/// TradeRebate status codes
#[allow(dead_code)]
pub const STATUS_CREATED: i32 = 0;
pub const STATUS_COMPLETED: i32 = 2;
pub const STATUS_HAS_NO_REBATE: i32 = -2;
#[allow(dead_code)]
pub const STATUS_PENDING_RESEND: i32 = 5;

/// RebateClientRule DistributionType
pub const DIST_DIRECT: i16 = 1;
pub const DIST_ALLOCATION: i16 = 2;
pub const DIST_LEVEL_PERCENTAGE: i16 = 3;

/// Account status — AccountStatusTypes::Activate = 0, Pause = 1, Inactivated = 2
#[allow(dead_code)]
pub const ACCOUNT_STATUS_ACTIVATE: i16 = 0;

/// Mirrors trd."_TradeRebate_{year}" — fields needed for rebate calculation.
#[derive(Debug, Clone, sqlx::FromRow)]
#[allow(dead_code)]
pub struct TradeRebateRow {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "AccountId")]
    pub account_id: Option<i64>,
    #[sqlx(rename = "TradeServiceId")]
    pub trade_service_id: i32,
    #[sqlx(rename = "Ticket")]
    pub ticket: i64,
    #[sqlx(rename = "AccountNumber")]
    pub account_number: i64,
    #[sqlx(rename = "CurrencyId")]
    pub currency_id: i32,
    #[sqlx(rename = "Volume")]
    pub volume: i32,
    #[sqlx(rename = "Symbol")]
    pub symbol: String,
    #[sqlx(rename = "ReferPath")]
    pub refer_path: String,
    #[sqlx(rename = "ClosedOn")]
    pub closed_on: DateTime<Utc>,
    #[sqlx(rename = "OpenedOn")]
    pub opened_on: DateTime<Utc>,
    #[sqlx(rename = "Status")]
    pub status: i32,
}

/// Agent account with rebate rule info — used for Allocation and LevelPercentage modes.
#[derive(Debug, Clone, sqlx::FromRow)]
#[allow(dead_code)]
pub struct AgentAccount {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "Uid")]
    pub uid: i64,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "CurrencyId")]
    pub currency_id: i32,
    #[sqlx(rename = "FundType")]
    pub fund_type: i32,
    #[sqlx(rename = "Level")]
    pub level: i32,
    #[sqlx(rename = "AgentAccountId")]
    pub agent_account_id: Option<i64>,
    /// JSON: List<RebateLevelSchemaItem> — [{cid, r}, ...]
    #[sqlx(rename = "RuleSchema")]
    pub rebate_agent_rule_schema: Option<String>,
    /// JSON: RebateLevelSetting — {PercentageSetting: {FOREX: [...], GOLD: [...], OTHER: [...]}}
    #[sqlx(rename = "RuleLevelSetting")]
    pub rebate_agent_rule_level_setting: Option<String>,
}

impl AgentAccount {
    /// Mirrors Account.IsTopLevelAgent():
    /// Role == Agent && (AgentAccountId == Id || AgentAccountId == null)
    pub fn is_top_level_agent(&self) -> bool {
        match self.agent_account_id {
            None => true,
            Some(aid) => aid == self.id,
        }
    }
}

/// Direct rule — maps a source trade account to a target account + schema.
#[derive(Debug, Clone, sqlx::FromRow)]
pub struct DirectRule {
    #[sqlx(rename = "RebateDirectSchemaId")]
    pub rebate_direct_schema_id: i64,
    #[sqlx(rename = "TargetAccountId")]
    pub target_account_id: i64,
}

/// Direct schema item — Rate/Pips/Commission per symbol.
#[derive(Debug, Clone, sqlx::FromRow)]
pub struct DirectSchemaItem {
    #[sqlx(rename = "Rate")]
    pub rate: Decimal,
    #[sqlx(rename = "Pips")]
    pub pips: Decimal,
    #[sqlx(rename = "Commission")]
    pub commission: Decimal,
    #[sqlx(rename = "SymbolCode")]
    pub symbol_code: String,
}

/// Target account info for Direct mode rebate calculation.
#[derive(Debug, Clone, sqlx::FromRow)]
pub struct TargetAccount {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "CurrencyId")]
    pub currency_id: i32,
    #[sqlx(rename = "FundType")]
    pub fund_type: i32,
    #[sqlx(rename = "ServiceId")]
    pub service_id: i32,
}

/// MT price row from mt5_prices / mt4_prices.
#[derive(Debug, Clone)]
pub struct MtPrice {
    pub bid: f64,
    pub digits: i32,
}

/// Intermediate rebate calculation result (Rate + Pip + Commission in units of 分×100).
#[derive(Debug, Clone, Default)]
pub struct BaseRebate {
    pub rate: rust_decimal::Decimal,
    pub pip: rust_decimal::Decimal,
    pub commission: rust_decimal::Decimal,
    pub price: f64,
}

impl BaseRebate {
    pub fn total(&self) -> rust_decimal::Decimal {
        self.rate + self.pip + self.commission
    }
}

/// New rebate record to insert into trd."_Rebate_{year}".
/// Id is assigned from core."_Matter_{year}" RETURNING "Id".
#[derive(Debug, Clone)]
pub struct NewRebate {
    pub party_id: i64,
    pub account_id: i64,
    pub trade_rebate_id: i64,
    pub currency_id: i32,
    pub fund_type: i32,
    /// Amount stored as NUMERIC in DB — preserves full decimal precision
    pub amount: rust_decimal::Decimal,
    /// JSON string with calculation details
    pub information: String,
}
