use std::collections::HashMap;
use std::sync::OnceLock;

use anyhow::Result;
use rust_decimal::prelude::{FromPrimitive, ToPrimitive};
use rust_decimal::Decimal;
use serde_json::json;
use sqlx::PgPool;
use tracing::{info, warn};

use crate::db::rebate_calc as db;
use crate::models::rebate::{
    AgentAccount, BaseRebate, DirectSchemaItem, NewRebate, TradeRebateRow,
    DIST_ALLOCATION, DIST_DIRECT, DIST_LEVEL_PERCENTAGE, STATUS_COMPLETED, STATUS_HAS_NO_REBATE,
};
use crate::AppContext;

// ── Constants (ported directly from mono RebateService) ──────────────────────

static CONTRACT_SIZE: OnceLock<HashMap<&'static str, i32>> = OnceLock::new();
static PREFIX_PIP_VALUES: OnceLock<HashMap<&'static str, f64>> = OnceLock::new();

fn contract_size() -> &'static HashMap<&'static str, i32> {
    CONTRACT_SIZE.get_or_init(|| {
        let mut m = HashMap::new();
        m.insert("#AAPL", 100); m.insert("#CL", 1000); m.insert("#ES", 50);
        m.insert("#NKD", 5); m.insert("#NQ", 20); m.insert("#YM", 5);
        m.insert("#NG", 10000); m.insert("#Copper", 25000); m.insert("#Soybean", 5000);
        m.insert("#Wheat", 5000); m.insert("#Corn", 5000); m.insert("#MSFT", 100);
        m.insert("#AXP", 100); m.insert("#MCD", 100); m.insert("#INTC", 100);
        m.insert("#IBM", 100); m.insert("#KO", 100); m.insert("#C", 100);
        m.insert("#BAC", 100); m.insert("#DIS", 100); m.insert("#BA", 100);
        m.insert("#QAN.AX", 100); m.insert("#APT.AX", 100); m.insert("#CSL.AX", 100);
        m.insert("#BHP.AX", 100); m.insert("#6501.T", 100); m.insert("#6502.T", 100);
        m.insert("#7201.T", 100); m.insert("#7261.T", 100); m.insert("#8306.T", 100);
        m.insert("#0005.HK", 100); m.insert("#0291.HK", 100); m.insert("#0700.HK", 100);
        m.insert("#0728.HK", 100); m.insert("#0941.HK", 100); m.insert("#1088.HK", 100);
        m.insert("#1810.HK", 100); m.insert("#1928.HK", 100); m.insert("#2628.HK", 100);
        m.insert("#3328.HK", 100); m.insert("#3988.HK", 100);
        m.insert("XTIUSD", 1000); m.insert("XBRUSD", 1000); m.insert("XNGUSD", 10000);
        m.insert("AUDCAD", 100000); m.insert("AUDCHF", 100000); m.insert("AUDJPY", 100000);
        m.insert("AUDNZD", 100000); m.insert("AUDUSD", 100000); m.insert("CADCHF", 100000);
        m.insert("CADJPY", 100000); m.insert("CHFJPY", 100000); m.insert("EURAUD", 100000);
        m.insert("EURCAD", 100000); m.insert("EURCHF", 100000); m.insert("EURGBP", 100000);
        m.insert("EURJPY", 100000); m.insert("EURNZD", 100000); m.insert("EURUSD", 100000);
        m.insert("GBPAUD", 100000); m.insert("GBPCAD", 100000); m.insert("GBPCHF", 100000);
        m.insert("GBPJPY", 100000); m.insert("GBPNZD", 100000); m.insert("GBPUSD", 100000);
        m.insert("XAUUSD", 100); m.insert("XPTUSD", 100); m.insert("XPDUSD", 100);
        m.insert("NZDCAD", 100000); m.insert("NZDCHF", 100000); m.insert("NZDJPY", 100000);
        m.insert("NZDUSD", 100000); m.insert("XAGUSD", 5000); m.insert("XAGUSDmin", 1000);
        m.insert("USDCAD", 100000); m.insert("USDCHF", 100000); m.insert("USDCNH", 100000);
        m.insert("USDJPY", 100000); m.insert("USDMXN", 100000); m.insert("USDNOK", 100000);
        m.insert("USDSEK", 100000); m.insert("USDPLN", 100000); m.insert("USDSGD", 100000);
        m.insert("USDTRY", 100000); m.insert("USDZAR", 100000); m.insert("EURMXN", 100000);
        m.insert("EURNOK", 100000); m.insert("EURPLN", 100000); m.insert("EURSEK", 100000);
        m.insert("EURTRY", 100000); m.insert("GBPMXN", 100000); m.insert("GBPNOK", 100000);
        m.insert("GBPSEK", 100000); m.insert("GBPTRY", 100000);
        m.insert("Bitcoin", 1); m.insert("Ethereum", 5);
        m.insert("#HKG50", 1); m.insert("#JPN225", 1); m.insert("#US500", 1);
        m.insert("#US100", 1); m.insert("#US30", 1); m.insert("#CN300", 1);
        m.insert("#AUS200", 1); m.insert("#GER30", 1); m.insert("#EUSTX50", 1);
        m.insert("#ESP35", 1); m.insert("#FRA40", 1); m.insert("#UK100", 1);
        m.insert("#CHN50", 1); m.insert("#GER40", 1);
        m
    })
}

fn prefix_pip_values() -> &'static HashMap<&'static str, f64> {
    PREFIX_PIP_VALUES.get_or_init(|| {
        let mut m = HashMap::new();
        m.insert("XAUUSD", 1.0); m.insert("XAGUSDmin", 1.0); m.insert("XAGUSD", 50.0);
        m.insert("XPTUSD", 1.0); m.insert("XPDUSD", 1.0); m.insert("XTIUSD", 1.0);
        m.insert("XNGUSD", 1.0); m.insert("#CL", 1.0); m.insert("#Copper", 2.5);
        m.insert("#Corn", 0.5); m.insert("#Wheat", 0.5); m.insert("#Soybean", 0.5);
        m.insert("#ES", 0.5); m.insert("#NQ", 0.2); m.insert("#YM", 5.0);
        m.insert("#CN300", 5.0); m.insert("#NKD", 5.0); m.insert("#MSFT", 1.0);
        m.insert("#AAPL", 1.0); m.insert("#AXP", 1.0); m.insert("#MCD", 1.0);
        m.insert("#INTC", 1.0); m.insert("#IBM", 1.0); m.insert("#KO", 1.0);
        m.insert("#C", 1.0); m.insert("#BAC", 1.0); m.insert("#DIS", 1.0);
        m.insert("#BA", 1.0); m.insert("#QAN.AX", 1.0); m.insert("#APT.AX", 1.0);
        m.insert("#CSL.AX", 1.0); m.insert("#BHP.AX", 1.0); m.insert("#6501.T", 1.0);
        m.insert("#6502.T", 1.0); m.insert("#7201.T", 1.0); m.insert("#7261.T", 1.0);
        m.insert("#8306.T", 1.0); m.insert("#0005.HK", 1.0); m.insert("#0291.HK", 1.0);
        m.insert("#0700.HK", 1.0); m.insert("#0728.HK", 1.0); m.insert("#0941.HK", 1.0);
        m.insert("#1088.HK", 1.0); m.insert("#1810.HK", 1.0); m.insert("#1928.HK", 1.0);
        m.insert("#2628.HK", 1.0); m.insert("#3328.HK", 1.0); m.insert("#3988.HK", 1.0);
        m.insert("Bitcoin", 0.01); m.insert("Ethereum", 0.05);
        m.insert("#AUS200", 1.0); m.insert("#GER30", 1.0); m.insert("#EUSTX50", 1.0);
        m.insert("#ESP35", 1.0); m.insert("#FRA40", 1.0); m.insert("#UK100", 1.0);
        m.insert("#JPN225", 10.0); m.insert("#HKG50", 10.0); m.insert("#CHN50", 1.0);
        m.insert("#US500", 1.0); m.insert("#US100", 1.0); m.insert("#US30", 10.0);
        m.insert("#GER40", 1.0);
        m
    })
}

/// Symbols that use prefix matching (first match wins)
const PREFIX_SYMBOLS: &[&str] = &[
    "#CL", "#NKD", "#Copper", "#Corn", "#Wheat", "#Soybean", "#CN300",
];

// ── Symbol helpers ────────────────────────────────────────────────────────────

/// Mirrors mono's GetTrimmedSymbol.
pub fn get_trimmed_symbol(symbol: &str) -> String {
    let base = if let Some(dot_pos) = symbol.find('.') {
        &symbol[..dot_pos]
    } else {
        symbol
    };

    if base.starts_with("#CL") {
        return "#CL".to_string();
    }
    if base.starts_with("#BRN") {
        return "#BRN".to_string();
    }
    if base == "XAGUSDmin" {
        return base.to_uppercase();
    }
    base.to_string()
}

/// Normalize a symbol using prefix matching (for contract_size / prefix_pip_values lookup).
fn normalize_symbol(code: &str) -> &str {
    for prefix in PREFIX_SYMBOLS.iter() {
        if code.contains(prefix) {
            return prefix;
        }
    }
    code
}

fn get_contract_size(symbol_code: &str) -> i32 {
    let normalized = normalize_symbol(symbol_code);
    *contract_size().get(normalized).unwrap_or(&1)
}

fn get_prefix_pip_value(symbol_code: &str) -> f64 {
    let normalized = normalize_symbol(symbol_code);
    *prefix_pip_values().get(normalized).unwrap_or(&0.0)
}

/// Mirrors mono's GetSymbolCategoryName for LevelPercentage mode.
pub fn get_symbol_category_name(category: &str) -> &'static str {
    let upper = category.to_uppercase();
    if upper.contains("FOREX") {
        "FOREX"
    } else if upper.contains("GOLD") {
        "GOLD"
    } else {
        "OTHER"
    }
}

// ── Exchange rate ─────────────────────────────────────────────────────────────

/// Mirrors mono's GetMtExchangeRate.
/// Queries MT5 price table: first tries FROM+TO, then TO+FROM (inverted).
/// USC currency is treated as USD/100.
pub async fn get_mt_exchange_rate(
    ctx: &AppContext,
    tenant_pool: &PgPool,
    service_id: i32,
    from_currency_id: i32,
    to_currency_id: i32,
) -> Result<f64> {
    get_mt_exchange_rate_inner(ctx, tenant_pool, service_id, from_currency_id, to_currency_id).await
}

fn get_mt_exchange_rate_inner<'a>(
    ctx: &'a AppContext,
    tenant_pool: &'a PgPool,
    service_id: i32,
    from_currency_id: i32,
    to_currency_id: i32,
) -> std::pin::Pin<Box<dyn std::future::Future<Output = Result<f64>> + Send + 'a>> {
    Box::pin(async move {
        if from_currency_id == to_currency_id {
            return Ok(1.0);
        }

        // CurrencyTypes: USD=1, USC=2
        const USD: i32 = 1;
        const USC: i32 = 2;

        if from_currency_id == USC && to_currency_id == USD {
            return Ok(0.01);
        }
        if from_currency_id == USD && to_currency_id == USC {
            return Ok(100.0);
        }
        if from_currency_id == USC {
            let usd_to_x = get_mt_exchange_rate_inner(ctx, tenant_pool, service_id, USD, to_currency_id).await?;
            return Ok(0.01 * usd_to_x);
        }
        if to_currency_id == USC {
            let x_to_usd = get_mt_exchange_rate_inner(ctx, tenant_pool, service_id, from_currency_id, USD).await?;
            return Ok(x_to_usd * 100.0);
        }

        let from_name = currency_id_to_name(from_currency_id);
        let to_name = currency_id_to_name(to_currency_id);

        let mt5_pool = ctx.mt5_pool(service_id, tenant_pool).await?;

        let symbol = format!("{}{}", from_name, to_name);
        if let Some(price) = db::get_mt5_price(&mt5_pool, &symbol).await? {
            if price.bid != 0.0 {
                return Ok(price.bid);
            }
        }

        let symbol_inv = format!("{}{}", to_name, from_name);
        if let Some(price) = db::get_mt5_price(&mt5_pool, &symbol_inv).await? {
            if price.bid != 0.0 {
                return Ok(1.0 / price.bid);
            }
        }

        Ok(1.0)
    })
}

/// Mirrors mono's GetMtPipValueBySymbolCode.
/// Returns (pip_formula, exchange_rate_price).
pub async fn get_pip_value(
    ctx: &AppContext,
    tenant_pool: &PgPool,
    service_id: i32,
    symbol_code: &str,
) -> Result<(f64, f64)> {
    let mt5_pool = ctx.mt5_pool(service_id, tenant_pool).await?;

    if symbol_code.ends_with("USD") {
        if let Some(price) = db::get_mt5_price(&mt5_pool, symbol_code).await? {
            let contract = get_contract_size(symbol_code) as f64;
            // bid cancels out: 1/10^d/bid * contract * bid = contract/10^d
            // Compute directly to avoid floating-point precision loss
            let value = contract / 10f64.powi(price.digits);
            return Ok((value, price.bid));
        }
        return Ok((0.0, 0.0));
    }

    if symbol_code.starts_with('#') {
        // Index / commodity with special handling
        let index_symbols_eur = ["#AUS200", "#GER30", "#GER40", "#EUSTX50", "#FRA40", "#ESP35", "#UK100"];
        if index_symbols_eur.contains(&symbol_code) {
            let pair = if symbol_code == "#AUS200" {
                "AUDUSD"
            } else if symbol_code == "#UK100" {
                "GBPUSD"
            } else {
                "EURUSD"
            };
            if let Some(price) = db::get_mt5_price(&mt5_pool, pair).await? {
                return Ok((price.bid * get_prefix_pip_value(symbol_code), price.bid));
            }
            return Ok((0.0, 0.0));
        }

        if symbol_code == "#HKG50" {
            if let Some(price) = db::get_mt5_price(&mt5_pool, "USDHKD").await? {
                // Mirror mono: Math.Round(1 / price.Bid, 5)
                let rounded = (1.0 / price.bid * 100000.0).round() / 100000.0;
                let val = rounded * get_prefix_pip_value(symbol_code);
                return Ok((val, price.bid));
            }
            return Ok((0.0, 0.0));
        }

        return Ok((get_prefix_pip_value(symbol_code), 0.0));
    }

    if symbol_code.starts_with("USD") {
        if let Some(price) = db::get_mt5_price(&mt5_pool, symbol_code).await? {
            let contract = get_contract_size(symbol_code) as f64;
            let value = contract / 10f64.powi(price.digits) / price.bid;
            return Ok((value, price.bid));
        }
        return Ok((0.0, 0.0));
    }

    if symbol_code.len() != 6 {
        // Check PREFIX_PIP_VALUES fallback
        let pv = get_prefix_pip_value(symbol_code);
        return Ok((pv, 0.0));
    }

    let last_three = &symbol_code[3..];
    let usd_pair = format!("{}USD", last_three);
    if contract_size().contains_key(usd_pair.as_str()) {
        if let Some(price) = db::get_mt5_price(&mt5_pool, &usd_pair).await? {
            return Ok((price.bid, price.bid));
        }
    }

    let usd_pair_inv = format!("USD{}", last_three);
    if contract_size().contains_key(usd_pair_inv.as_str()) {
        if let Some(price) = db::get_mt5_price(&mt5_pool, &usd_pair_inv).await? {
            let contract = get_contract_size(symbol_code) as f64;
            let value = 1.0 / 10f64.powi(price.digits) / price.bid * contract;
            return Ok((value, price.bid));
        }
    }

    // Fallback to PREFIX_PIP_VALUES
    let pv = get_prefix_pip_value(symbol_code);
    Ok((pv, 0.0))
}

// ── Base rebate calculation ───────────────────────────────────────────────────

/// Pure calculation core — no IO.
/// All rates and pip_formula are pre-resolved by the caller.
/// Mirrors mono's CalculateRatePipCommissionByDirectSchema arithmetic exactly.
///
/// - `source_exchange_rate`: trade currency → USD rate
/// - `pip_formula`: pip value per lot for the symbol (0.0 if symbol has no pip value)
/// - `exchange_rate_price`: raw MT5 bid price stored as metadata in BaseRebate.price
pub(crate) fn calculate_base_rebate_pure(
    schema_item: &DirectSchemaItem,
    volume: i32,
    source_exchange_rate: f64,
    pip_formula: f64,
    exchange_rate_price: f64,
) -> BaseRebate {
    let divided_volume = Decimal::new(volume as i64, 2); // volume / 100
    let ser = Decimal::from_f64(source_exchange_rate).unwrap_or(Decimal::ONE);

    let rate = dec_round_to_zero(schema_item.rate * ser * dec(100) * divided_volume * dec(100));

    if schema_item.pips == Decimal::ZERO && schema_item.commission == Decimal::ZERO {
        return BaseRebate { rate, pip: Decimal::ZERO, commission: Decimal::ZERO, price: 0.0 };
    }

    let commission = dec_round_to_zero(schema_item.commission * ser * dec(100) * divided_volume * dec(100));

    let pip_dec = Decimal::from_f64(pip_formula).unwrap_or(Decimal::ZERO);
    let pip = dec_round_to_zero(schema_item.pips * ser * dec(100) * divided_volume * pip_dec * dec(100));

    BaseRebate { rate, pip, commission, price: exchange_rate_price }
}

/// Mirrors mono's CalculateRatePipCommissionByDirectSchema.
/// Uses Decimal arithmetic throughout to match C# decimal precision.
/// Final round uses MidpointRounding.ToZero (truncate), same as mono.
pub async fn calculate_base_rebate(
    ctx: &AppContext,
    tenant_pool: &PgPool,
    service_id: i32,
    schema_item: &DirectSchemaItem,
    volume: i32,
    source_currency_id: i32,
) -> Result<BaseRebate> {
    const USD: i32 = 1;
    let source_exchange_rate =
        get_mt_exchange_rate(ctx, tenant_pool, service_id, source_currency_id, USD).await?;

    // Early-exit path: if no pips/commission, skip the MT5 price lookup entirely
    if schema_item.pips == Decimal::ZERO && schema_item.commission == Decimal::ZERO {
        return Ok(calculate_base_rebate_pure(schema_item, volume, source_exchange_rate, 0.0, 0.0));
    }

    let (pip_formula, exchange_rate_price) =
        get_pip_value(ctx, tenant_pool, service_id, &schema_item.symbol_code).await?;

    Ok(calculate_base_rebate_pure(schema_item, volume, source_exchange_rate, pip_formula, exchange_rate_price))
}

/// Decimal constant helper.
#[inline]
fn dec(n: i64) -> Decimal {
    Decimal::new(n, 0)
}

/// MidpointRounding.ToZero — truncate toward zero, matching mono's explicit ToZero calls.
fn dec_round_to_zero(v: Decimal) -> Decimal {
    v.trunc()
}


// ── Allocation helpers ────────────────────────────────────────────────────────

/// Mirrors mono's CalculateAllocationRebateBySchema.
/// Uses Decimal arithmetic with MidpointRounding.ToZero (truncate) to match mono exactly.
fn calculate_allocation_rebate(
    remain: &mut BaseRebate,
    volume: i32,
    schema_rate: Decimal,
    take_percentage: Decimal,
    source_exchange_rate: f64,
) -> (Decimal, Decimal) {
    let ser = Decimal::from_f64(source_exchange_rate).unwrap_or(Decimal::ONE);
    // mono: schemaItem.Rate * sourceExchangeRate * volume * 100
    let rate = dec_round_to_zero(schema_rate * ser * dec(volume as i64) * dec(100));
    let schema_rate_value = remain.rate.min(rate);
    remain.rate -= schema_rate_value;

    let pct_abs = take_percentage.abs();
    let mut combined = Decimal::ZERO;

    if remain.pip > Decimal::ZERO {
        // mono: remainBaseRebate.Pip * sourceExchangeRate * |takePercentage| / 100 * 100
        let pip_value = dec_round_to_zero(remain.pip * ser * pct_abs / dec(100) * dec(100));
        let pip_take = remain.pip.min(pip_value);
        combined += pip_take;
        remain.pip -= pip_take;
    }

    if remain.commission > Decimal::ZERO {
        let comm_value = dec_round_to_zero(remain.commission * ser * pct_abs / dec(100) * dec(100));
        let comm_take = remain.commission.min(comm_value);
        combined += comm_take;
        remain.commission -= comm_take;
    }

    (schema_rate_value, combined)
}

/// Mirrors mono's CalculateLastAllocationRebate.
fn calculate_last_allocation_rebate(remain: &mut BaseRebate) -> (Decimal, Decimal) {
    let rate = remain.rate;
    remain.rate = Decimal::ZERO;
    let mut combined = Decimal::ZERO;
    if remain.pip > Decimal::ZERO {
        combined += remain.pip;
        remain.pip = Decimal::ZERO;
    }
    if remain.commission > Decimal::ZERO {
        combined += remain.commission;
        remain.commission = Decimal::ZERO;
    }
    (rate, combined)
}

// ── Mode generators ───────────────────────────────────────────────────────────

/// Direct mode: one rebate per direct rule target account.
async fn generate_direct_rebates(
    ctx: &AppContext,
    pool: &PgPool,
    trade_rebate: &TradeRebateRow,
) -> Result<Vec<NewRebate>> {
    let account_id = match trade_rebate.account_id {
        Some(id) => id,
        None => return Ok(vec![]),
    };

    let trimmed = get_trimmed_symbol(&trade_rebate.symbol);
    let rules = db::get_direct_rules(pool, account_id).await?;
    let mut results = vec![];

    for rule in &rules {
        let schema_item =
            match db::get_direct_schema_item(pool, rule.rebate_direct_schema_id, &trimmed).await? {
                Some(item) => item,
                None => continue,
            };

        let target = match db::get_target_account(pool, rule.target_account_id).await? {
            Some(t) => t,
            None => continue,
        };

        let base = calculate_base_rebate(
            ctx,
            pool,
            trade_rebate.trade_service_id,
            &schema_item,
            trade_rebate.volume,
            trade_rebate.currency_id,
        )
        .await?;

        let total = base.total();
        if total == Decimal::ZERO {
            continue;
        }

        const USD: i32 = 1;
        let target_rate =
            get_mt_exchange_rate(ctx, pool, target.service_id, USD, target.currency_id).await?;
        // mono: (long)Math.Round(itemData.GetTotal() * targetExchangeRate, MidpointRounding.ToZero) * 100
        let ter = Decimal::from_f64(target_rate).unwrap_or(Decimal::ONE);
        let amount = dec_round_to_zero(total * ter) * dec(100);
        if amount == Decimal::ZERO {
            continue;
        }

        results.push(NewRebate {
            party_id: target.party_id,
            account_id: target.id,
            trade_rebate_id: trade_rebate.id,
            currency_id: target.currency_id,
            fund_type: target.fund_type,
            amount,
            information: serde_json::to_string(&json!({
                "BaseRebate": { "Rate": base.rate, "Pip": base.pip, "Commission": base.commission, "Price": base.price },
                "ExchangeRate": target_rate,
                "Version": "v2_scheduler"
            }))?,
        });
    }

    Ok(results)
}

/// Allocation mode: distribute rebate up the agent chain layer by layer.
async fn generate_allocation_rebates(
    ctx: &AppContext,
    pool: &PgPool,
    trade_rebate: &TradeRebateRow,
) -> Result<Vec<NewRebate>> {
    let account_id = match trade_rebate.account_id {
        Some(id) => id,
        None => return Ok(vec![]),
    };

    let trimmed = get_trimmed_symbol(&trade_rebate.symbol);
    let symbol_category_id: i32 = db::get_symbol_category_id(pool, &trimmed).await?.unwrap_or(0);

    // Find the client's RebateClientRule to get the DirectSchemaId
    let rule = db::get_distribution_type(pool, account_id).await?;
    let schema_id = match rule {
        Some((_, Some(sid))) => sid,
        _ => return Ok(vec![]),
    };

    let schema_item = match db::get_direct_schema_item(pool, schema_id, &trimmed).await? {
        Some(item) => item,
        None => return Ok(vec![]),
    };

    let agent_accounts = db::get_sorted_agent_accounts(pool, account_id).await?;
    if agent_accounts.is_empty() {
        return Ok(vec![]);
    }

    const USD: i32 = 1;
    let source_rate = get_mt_exchange_rate(
        ctx,
        pool,
        trade_rebate.trade_service_id,
        trade_rebate.currency_id,
        USD,
    )
    .await?;

    let base = calculate_base_rebate(
        ctx,
        pool,
        trade_rebate.trade_service_id,
        &schema_item,
        trade_rebate.volume,
        trade_rebate.currency_id,
    )
    .await?;

    let mut remain = base.clone();
    let mut results = vec![];

    for (i, agent) in agent_accounts.iter().enumerate() {
        let target_rate =
            get_mt_exchange_rate(ctx, pool, trade_rebate.trade_service_id, USD, agent.currency_id)
                .await?;

        let is_last = i == agent_accounts.len() - 1;
        let next_agent = agent_accounts.get(i + 1);

        let (schema_rate_val, combined_val) = if next_agent.is_none() || is_last {
            calculate_last_allocation_rebate(&mut remain)
        } else {
            // Get schema item for the next agent
            let next = next_agent.unwrap();
            let (schema_item_next, percentage) =
                get_schema_item_and_percentage(next, symbol_category_id);
            match schema_item_next {
                None => continue,
                Some((rate, _)) => {
                    calculate_allocation_rebate(&mut remain, trade_rebate.volume, rate, percentage, source_rate)
                }
            }
        };

        let total = schema_rate_val + combined_val;
        if total == Decimal::ZERO {
            if is_last {
                break;
            }
            continue;
        }

        // mono: (long)Math.Round(allocationRebate.Total * targetExchangeRate, MidpointRounding.ToZero) * 100
        let ter = Decimal::from_f64(target_rate).unwrap_or(Decimal::ONE);
        let amount = dec_round_to_zero(total * ter) * dec(100);
        if amount == Decimal::ZERO {
            if is_last {
                break;
            }
            continue;
        }

        results.push(NewRebate {
            party_id: agent.party_id,
            account_id: agent.id,
            trade_rebate_id: trade_rebate.id,
            currency_id: agent.currency_id,
            fund_type: agent.fund_type,
            amount,
            information: serde_json::to_string(&json!({
                "Depth": i + 1,
                "BaseRebate": { "Rate": base.rate, "Pip": base.pip, "Commission": base.commission },
                "ExchangeRate": target_rate,
                "Version": "v2_scheduler"
            }))?,
        });

        if is_last {
            break;
        }
    }

    Ok(results)
}

/// Parse RebateAgentRule.Schema JSON and return (Rate, Percentage) for a given CategoryId.
/// Schema JSON: [{AccountType, Percentage, Items: [{cid, r}, ...]}]
fn get_schema_item_and_percentage(
    agent: &AgentAccount,
    symbol_category_id: i32,
) -> (Option<(Decimal, i32)>, Decimal) {
    let schema_json = match &agent.rebate_agent_rule_schema {
        Some(s) => s,
        None => return (None, Decimal::ZERO),
    };

    let schemas: Vec<serde_json::Value> = match serde_json::from_str(schema_json) {
        Ok(v) => v,
        Err(_) => return (None, Decimal::ZERO),
    };

    for schema in &schemas {
        let percentage = schema["Percentage"]
            .as_f64()
            .map(|f| Decimal::try_from(f).unwrap_or(Decimal::ZERO))
            .unwrap_or(Decimal::ZERO);

        if let Some(items) = schema["Items"].as_array() {
            for item in items {
                let cid = item["cid"].as_i64().unwrap_or(-1) as i32;
                if cid == symbol_category_id {
                    let rate = item["r"]
                        .as_f64()
                        .map(|f| Decimal::try_from(f).unwrap_or(Decimal::ZERO))
                        .unwrap_or(Decimal::ZERO);
                    return (Some((rate, cid as i32)), percentage);
                }
            }
        }
    }

    (None, Decimal::ZERO)
}

/// LevelPercentage mode: distribute rebate by percentage up the agent chain.
async fn generate_level_percentage_rebates(
    ctx: &AppContext,
    pool: &PgPool,
    trade_rebate: &TradeRebateRow,
) -> Result<Vec<NewRebate>> {
    let account_id = match trade_rebate.account_id {
        Some(id) => id,
        None => return Ok(vec![]),
    };

    let trimmed = get_trimmed_symbol(&trade_rebate.symbol);

    // Determine symbol category
    let raw_category = db::get_symbol_category(pool, &trimmed)
        .await?
        .unwrap_or_else(|| "OTHER".to_string());
    let symbol_category = get_symbol_category_name(&raw_category);

    // Find the client's RebateClientRule DirectSchemaId
    let rule = db::get_distribution_type(pool, account_id).await?;
    let schema_id = match rule {
        Some((_, Some(sid))) => sid,
        _ => return Ok(vec![]),
    };

    let schema_item = match db::get_direct_schema_item(pool, schema_id, &trimmed).await? {
        Some(item) => item,
        None => return Ok(vec![]),
    };

    let agent_accounts = db::get_sorted_agent_accounts(pool, account_id).await?;
    if agent_accounts.is_empty() {
        return Ok(vec![]);
    }

    let top_agent = &agent_accounts[0];

    let base = calculate_base_rebate(
        ctx,
        pool,
        trade_rebate.trade_service_id,
        &schema_item,
        trade_rebate.volume,
        trade_rebate.currency_id,
    )
    .await?;

    let total_rebate = base.total();
    if total_rebate == Decimal::ZERO {
        return Ok(vec![]);
    }

    const USD: i32 = 1;
    let source_rate = get_mt_exchange_rate(
        ctx,
        pool,
        trade_rebate.trade_service_id,
        trade_rebate.currency_id,
        USD,
    )
    .await?;

    // mono uses decimal throughout for remain_amount to avoid float accumulation
    let volume_divided = Decimal::new(trade_rebate.volume as i64, 2); // volume / 100
    let source_rate_dec = Decimal::from_f64(source_rate).unwrap_or(Decimal::ONE);
    let mut remain_amount = total_rebate;
    let mut results = vec![];

    // Level2+ agents (skip top agent, iterate from bottom)
    if agent_accounts.len() > 1 {
        let level2_agent = &agent_accounts[1];
        let level_setting_json = match &level2_agent.rebate_agent_rule_level_setting {
            Some(s) => s.clone(),
            None => return Ok(vec![]),
        };

        let level_setting: serde_json::Value = match serde_json::from_str(&level_setting_json) {
            Ok(v) => v,
            Err(_) => return Ok(vec![]),
        };

        let settings = match level_setting["PercentageSetting"][symbol_category].as_array() {
            Some(arr) => arr.clone(),
            None => return Ok(vec![]),
        };

        if settings.is_empty() {
            return Ok(vec![]);
        }

        // Reverse the settings (bottom-up)
        let mut percentages: Vec<Decimal> = settings
            .iter()
            .filter_map(|v| v.as_f64())
            .filter_map(|f| Decimal::from_f64(f))
            .collect();
        percentages.reverse();
        let mut queue = std::collections::VecDeque::from(percentages);

        let n = agent_accounts.len();
        let mut i = n as i64 - 1;
        while remain_amount > Decimal::ZERO && i > 0 && queue.len() > 1 {
            let agent = &agent_accounts[i as usize];
            // mono: percentage = queue.Dequeue() * 100
            let percentage = queue.pop_front().unwrap_or(Decimal::ZERO) * dec(100);

            let target_rate_f64 =
                get_mt_exchange_rate(ctx, pool, trade_rebate.trade_service_id, USD, agent.currency_id)
                    .await?;
            let target_rate_dec = Decimal::from_f64(target_rate_f64).unwrap_or(Decimal::ONE);

            // mono: Math.Round(volume * percentage * sourceExchangeRate * targetExchangeRate, MidpointRounding.ToZero)
            let amount = dec_round_to_zero(volume_divided * percentage * source_rate_dec * target_rate_dec);
            if amount == Decimal::ZERO {
                break;
            }

            let amount_dec = amount.min(remain_amount);
            remain_amount -= amount_dec;

            // mono: Amount = (long)amount * 100
            let rebate_amount = amount_dec * dec(100);
            results.push(NewRebate {
                party_id: agent.party_id,
                account_id: agent.id,
                trade_rebate_id: trade_rebate.id,
                currency_id: agent.currency_id,
                fund_type: agent.fund_type,
                amount: rebate_amount,
                information: serde_json::to_string(&json!({
                    "Depth": i + 1,
                    "BaseRebate": { "Rate": base.rate, "Pip": base.pip, "Commission": base.commission },
                    "ExchangeRate": target_rate_f64,
                    "RemainRebate": remain_amount.to_f64(),
                    "Version": "v3_level_percentage_scheduler"
                }))?,
            });

            i -= 1;
        }
    }

    // Top agent gets the remainder
    if remain_amount > Decimal::ZERO {
        let target_rate_f64 =
            get_mt_exchange_rate(ctx, pool, trade_rebate.trade_service_id, USD, top_agent.currency_id)
                .await?;
        let target_rate_dec = Decimal::from_f64(target_rate_f64).unwrap_or(Decimal::ONE);
        // mono: (long)Math.Round(remainRebateAmount * targetExchangeRate, MidpointRounding.ToZero) * 100
        let amount = dec_round_to_zero(remain_amount * target_rate_dec) * dec(100);
        if amount > Decimal::ZERO {
            results.push(NewRebate {
                party_id: top_agent.party_id,
                account_id: top_agent.id,
                trade_rebate_id: trade_rebate.id,
                currency_id: top_agent.currency_id,
                fund_type: top_agent.fund_type,
                amount,
                information: serde_json::to_string(&json!({
                    "Depth": 1,
                    "BaseRebate": { "Rate": base.rate, "Pip": base.pip, "Commission": base.commission },
                    "ExchangeRate": target_rate_f64,
                    "RemainRebate": remain_amount.to_f64(),
                    "Version": "v3_level_percentage_mib_scheduler"
                }))?,
            });
        }
    }

    Ok(results)
}

// ── RebateDb trait ────────────────────────────────────────────────────────────

/// Abstracts all DB calls needed by the generate/send rebate state machine.
/// The real implementation delegates to `crate::db::rebate_calc`.
/// Tests supply a `MockRebateDb` that records calls and returns canned data.
#[async_trait::async_trait]
pub(crate) trait RebateDb: Send + Sync {
    async fn get_trade_rebate(&self, table: &str, id: i64) -> Result<Option<TradeRebateRow>>;
    async fn is_account_active(&self, account_id: i64) -> Result<bool>;
    async fn get_distribution_type(&self, account_id: i64) -> Result<Option<(i16, Option<i64>)>>;
    async fn get_existing_rebates(&self, year: i32, trade_rebate_id: i64) -> Result<Vec<(i64, Decimal)>>;
    async fn get_target_account(&self, account_id: i64) -> Result<Option<crate::models::rebate::TargetAccount>>;
    async fn insert_rebate(&self, year: i32, rebate: &NewRebate) -> Result<i64>;
    async fn update_trade_rebate_status(&self, table: &str, id: i64, status: i32) -> Result<()>;
}

/// Production implementation — thin wrapper around `crate::db::rebate_calc`.
pub(crate) struct PgRebateDb<'a>(pub &'a PgPool);

#[async_trait::async_trait]
impl RebateDb for PgRebateDb<'_> {
    async fn get_trade_rebate(&self, table: &str, id: i64) -> Result<Option<TradeRebateRow>> {
        db::get_trade_rebate(self.0, table, id).await
    }
    async fn is_account_active(&self, account_id: i64) -> Result<bool> {
        db::is_account_active(self.0, account_id).await
    }
    async fn get_distribution_type(&self, account_id: i64) -> Result<Option<(i16, Option<i64>)>> {
        db::get_distribution_type(self.0, account_id).await
    }
    async fn get_existing_rebates(&self, year: i32, trade_rebate_id: i64) -> Result<Vec<(i64, Decimal)>> {
        db::get_existing_rebates(self.0, year, trade_rebate_id).await
    }
    async fn get_target_account(&self, account_id: i64) -> Result<Option<crate::models::rebate::TargetAccount>> {
        db::get_target_account(self.0, account_id).await
    }
    async fn insert_rebate(&self, year: i32, rebate: &NewRebate) -> Result<i64> {
        db::insert_rebate(self.0, year, rebate).await
    }
    async fn update_trade_rebate_status(&self, table: &str, id: i64, status: i32) -> Result<()> {
        db::update_trade_rebate_status(self.0, table, id, status).await
    }
}

// ── Main entry point ──────────────────────────────────────────────────────────

/// Main entry: generate and persist rebates for a single TradeRebate record.
/// Mirrors mono's GenerateRebatesByTradeRebateId + SendRebates.
pub async fn generate_rebates(
    ctx: &AppContext,
    pool: &PgPool,
    trade_rebate_id: i64,
    table: &str,
    year: i32,
) -> Result<()> {
    generate_rebates_with_db(ctx, &PgRebateDb(pool), pool, trade_rebate_id, table, year).await
}

/// Outcome of the guard phase — either an early exit or the data needed for mode dispatch.
pub(crate) enum GuardOutcome {
    /// A status update was written and processing is done.
    EarlyExit,
    /// Guards passed; caller should dispatch the appropriate rebate mode.
    Proceed { trade_rebate: TradeRebateRow, dist_type: i16 },
}

/// Guard phase only — no IO beyond the RebateDb trait.
/// Validates trade_rebate existence, account_id, active status, and distribution type.
/// Tests exercise this function directly to cover all early-exit branches.
pub(crate) async fn run_generate_guards(
    rdb: &dyn RebateDb,
    trade_rebate_id: i64,
    table: &str,
) -> Result<GuardOutcome> {
    let trade_rebate = match rdb.get_trade_rebate(table, trade_rebate_id).await? {
        Some(tr) => tr,
        None => {
            warn!("CalculateRebate: TradeRebate id={} not found in {}", trade_rebate_id, table);
            return Ok(GuardOutcome::EarlyExit);
        }
    };

    let account_id = match trade_rebate.account_id {
        Some(id) => id,
        None => {
            rdb.update_trade_rebate_status(table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
            return Ok(GuardOutcome::EarlyExit);
        }
    };

    if !rdb.is_account_active(account_id).await? {
        rdb.update_trade_rebate_status(table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
        return Ok(GuardOutcome::EarlyExit);
    }

    let dist_type = match rdb.get_distribution_type(account_id).await? {
        Some((dt, _)) => dt,
        None => {
            rdb.update_trade_rebate_status(table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
            return Ok(GuardOutcome::EarlyExit);
        }
    };

    Ok(GuardOutcome::Proceed { trade_rebate, dist_type })
}

/// Testable core: accepts a `RebateDb` trait object so tests can inject a mock.
pub(crate) async fn generate_rebates_with_db(
    ctx: &AppContext,
    rdb: &dyn RebateDb,
    pool: &PgPool,
    trade_rebate_id: i64,
    table: &str,
    year: i32,
) -> Result<()> {
    let (trade_rebate, dist_type) = match run_generate_guards(rdb, trade_rebate_id, table).await? {
        GuardOutcome::EarlyExit => return Ok(()),
        GuardOutcome::Proceed { trade_rebate, dist_type } => (trade_rebate, dist_type),
    };

    let rebates = match dist_type {
        DIST_DIRECT => generate_direct_rebates(ctx, pool, &trade_rebate).await?,
        DIST_ALLOCATION => generate_allocation_rebates(ctx, pool, &trade_rebate).await?,
        DIST_LEVEL_PERCENTAGE => generate_level_percentage_rebates(ctx, pool, &trade_rebate).await?,
        _ => vec![],
    };

    send_rebates_with_db(rdb, &trade_rebate, rebates, table, year).await
}

/// Testable core of SendRebates — accepts a `RebateDb` trait object.
pub(crate) async fn send_rebates_with_db(
    rdb: &dyn RebateDb,
    trade_rebate: &TradeRebateRow,
    rebates: Vec<NewRebate>,
    table: &str,
    year: i32,
) -> Result<()> {
    let trade_rebate_id = trade_rebate.id;

    let existing = rdb.get_existing_rebates(year, trade_rebate_id).await?;

    if rebates.is_empty() && existing.is_empty() {
        rdb.update_trade_rebate_status(table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
        return Ok(());
    }

    if !existing.is_empty() {
        let existing_map: HashMap<i64, Decimal> = existing.into_iter().collect();
        let new_map: HashMap<i64, Decimal> = rebates.iter().map(|r| (r.account_id, r.amount)).collect();

        for (account_id, existing_amount) in &existing_map {
            if !new_map.contains_key(account_id) {
                let compensating = NewRebate {
                    party_id: 0,
                    account_id: *account_id,
                    trade_rebate_id,
                    currency_id: -1,
                    fund_type: 0,
                    amount: -*existing_amount,
                    information: serde_json::to_string(&json!({"Note": "compensating_adjustment", "Version": "v2_scheduler"}))?,
                };
                if let Some(target) = rdb.get_target_account(*account_id).await? {
                    let comp = NewRebate {
                        party_id: target.party_id,
                        currency_id: target.currency_id,
                        fund_type: target.fund_type,
                        ..compensating
                    };
                    rdb.insert_rebate(year, &comp).await?;
                }
            }
        }

        let mut to_insert = vec![];
        for rebate in &rebates {
            if let Some(&existing_amount) = existing_map.get(&rebate.account_id) {
                if rebate.amount != existing_amount {
                    to_insert.push(NewRebate { amount: rebate.amount - existing_amount, ..rebate.clone() });
                }
            } else {
                to_insert.push(rebate.clone());
            }
        }

        for rebate in to_insert {
            rdb.insert_rebate(year, &rebate).await?;
        }
    } else {
        for rebate in &rebates {
            rdb.insert_rebate(year, rebate).await?;
        }
    }

    rdb.update_trade_rebate_status(table, trade_rebate_id, STATUS_COMPLETED).await?;

    info!(
        "CalculateRebate: completed trade_rebate_id={} rebates={}",
        trade_rebate_id,
        rebates.len()
    );

    Ok(())
}

// ── Currency helpers ──────────────────────────────────────────────────────────

/// Maps CurrencyId to ISO currency code string.
/// Based on mono's CurrencyTypes enum — extend as needed.
pub(crate) fn currency_id_to_name(currency_id: i32) -> &'static str {
    match currency_id {
        1 => "USD",
        2 => "USC", // cents
        3 => "EUR",
        4 => "GBP",
        5 => "JPY",
        6 => "AUD",
        7 => "CAD",
        8 => "CHF",
        9 => "NZD",
        10 => "HKD",
        11 => "SGD",
        12 => "CNY",
        13 => "MXN",
        14 => "NOK",
        15 => "SEK",
        16 => "PLN",
        17 => "TRY",
        18 => "ZAR",
        19 => "MYR",
        20 => "THB",
        _ => "USD", // fallback
    }
}

// ── Unit tests ────────────────────────────────────────────────────────────────

#[cfg(test)]
mod tests {
    use super::*;
    use rust_decimal_macros::dec;

    // ── get_trimmed_symbol ────────────────────────────────────────────────────

    #[test]
    fn trimmed_symbol_strips_suffix() {
        assert_eq!(get_trimmed_symbol("EURUSD.raw"), "EURUSD");
        assert_eq!(get_trimmed_symbol("XAUUSD.ECN"), "XAUUSD");
        assert_eq!(get_trimmed_symbol("GBPUSD.pro"), "GBPUSD");
    }

    #[test]
    fn trimmed_symbol_no_suffix_unchanged() {
        assert_eq!(get_trimmed_symbol("EURUSD"), "EURUSD");
        assert_eq!(get_trimmed_symbol("XAUUSD"), "XAUUSD");
        assert_eq!(get_trimmed_symbol("Bitcoin"), "Bitcoin");
    }

    #[test]
    fn trimmed_symbol_cl_prefix_normalised() {
        // Any #CL variant (futures month codes) → "#CL"
        assert_eq!(get_trimmed_symbol("#CLM25"), "#CL");
        assert_eq!(get_trimmed_symbol("#CLZ24"), "#CL");
        assert_eq!(get_trimmed_symbol("#CL"), "#CL");
    }

    #[test]
    fn trimmed_symbol_brn_prefix_normalised() {
        assert_eq!(get_trimmed_symbol("#BRNM25"), "#BRN");
        assert_eq!(get_trimmed_symbol("#BRN"), "#BRN");
    }

    #[test]
    fn trimmed_symbol_xagusdmin_uppercased() {
        // Special case: XAGUSDmin → "XAGUSDMIN"
        assert_eq!(get_trimmed_symbol("XAGUSDmin"), "XAGUSDMIN");
    }

    #[test]
    fn trimmed_symbol_hash_index_unchanged() {
        assert_eq!(get_trimmed_symbol("#US500"), "#US500");
        assert_eq!(get_trimmed_symbol("#HKG50"), "#HKG50");
        assert_eq!(get_trimmed_symbol("#GER40"), "#GER40");
    }

    // ── get_symbol_category_name ──────────────────────────────────────────────

    #[test]
    fn category_name_forex_variants() {
        assert_eq!(get_symbol_category_name("Forex"), "FOREX");
        assert_eq!(get_symbol_category_name("FOREX"), "FOREX");
        assert_eq!(get_symbol_category_name("forex group"), "FOREX");
        assert_eq!(get_symbol_category_name("Major Forex"), "FOREX");
    }

    #[test]
    fn category_name_gold_variants() {
        assert_eq!(get_symbol_category_name("Gold"), "GOLD");
        assert_eq!(get_symbol_category_name("GOLD"), "GOLD");
        assert_eq!(get_symbol_category_name("gold spot"), "GOLD");
    }

    #[test]
    fn category_name_other_fallback() {
        assert_eq!(get_symbol_category_name("Crypto"), "OTHER");
        assert_eq!(get_symbol_category_name("Index"), "OTHER");
        assert_eq!(get_symbol_category_name("Oil"), "OTHER");
        assert_eq!(get_symbol_category_name(""), "OTHER");
    }

    // ── BaseRebate::total ─────────────────────────────────────────────────────

    #[test]
    fn base_rebate_total_sums_all_fields() {
        let br = BaseRebate {
            rate: dec!(100),
            pip: dec!(50),
            commission: dec!(25),
            price: 1.23,
        };
        assert_eq!(br.total(), dec!(175));
    }

    #[test]
    fn base_rebate_total_zero_when_all_zero() {
        let br = BaseRebate::default();
        assert_eq!(br.total(), Decimal::ZERO);
    }

    #[test]
    fn base_rebate_total_ignores_price() {
        // price is an f64 metadata field, not included in total
        let br = BaseRebate { rate: dec!(10), pip: dec!(0), commission: dec!(0), price: 999.0 };
        assert_eq!(br.total(), dec!(10));
    }

    // ── dec_round_to_zero (truncate toward zero) ──────────────────────────────

    #[test]
    fn round_to_zero_truncates_positive() {
        assert_eq!(dec_round_to_zero(dec!(1.9)), dec!(1));
        assert_eq!(dec_round_to_zero(dec!(99.999)), dec!(99));
        assert_eq!(dec_round_to_zero(dec!(0.5)), dec!(0));
    }

    #[test]
    fn round_to_zero_truncates_negative() {
        // ToZero means toward zero, not floor — so -1.9 → -1, not -2
        assert_eq!(dec_round_to_zero(dec!(-1.9)), dec!(-1));
        assert_eq!(dec_round_to_zero(dec!(-0.5)), dec!(0));
    }

    #[test]
    fn round_to_zero_integer_unchanged() {
        assert_eq!(dec_round_to_zero(dec!(42)), dec!(42));
        assert_eq!(dec_round_to_zero(dec!(0)), dec!(0));
    }

    // ── calculate_last_allocation_rebate ─────────────────────────────────────

    #[test]
    fn last_allocation_drains_all_remain() {
        let mut remain = BaseRebate {
            rate: dec!(300),
            pip: dec!(120),
            commission: dec!(80),
            price: 0.0,
        };
        let (rate_val, combined) = calculate_last_allocation_rebate(&mut remain);

        assert_eq!(rate_val, dec!(300));
        assert_eq!(combined, dec!(200)); // pip + commission
        // remain must be fully zeroed
        assert_eq!(remain.rate, Decimal::ZERO);
        assert_eq!(remain.pip, Decimal::ZERO);
        assert_eq!(remain.commission, Decimal::ZERO);
    }

    #[test]
    fn last_allocation_zero_pip_and_commission() {
        let mut remain = BaseRebate { rate: dec!(50), pip: dec!(0), commission: dec!(0), price: 0.0 };
        let (rate_val, combined) = calculate_last_allocation_rebate(&mut remain);
        assert_eq!(rate_val, dec!(50));
        assert_eq!(combined, dec!(0));
    }

    #[test]
    fn last_allocation_zero_remain() {
        let mut remain = BaseRebate::default();
        let (rate_val, combined) = calculate_last_allocation_rebate(&mut remain);
        assert_eq!(rate_val, dec!(0));
        assert_eq!(combined, dec!(0));
    }

    // ── calculate_allocation_rebate ───────────────────────────────────────────

    #[test]
    fn allocation_rebate_rate_capped_by_remain() {
        // remain.rate=100, schema_rate would compute to 200 → capped at 100
        let mut remain = BaseRebate { rate: dec!(100), pip: dec!(0), commission: dec!(0), price: 0.0 };
        // schema_rate=2.0, ser=1.0, volume=100 → rate = trunc(2.0 * 1.0 * 100 * 100) = 20000 > 100
        let (rate_val, _combined) = calculate_allocation_rebate(&mut remain, 100, dec!(2), Decimal::ZERO, 1.0);
        assert_eq!(rate_val, dec!(100)); // capped at remain.rate
        assert_eq!(remain.rate, dec!(0));
    }

    #[test]
    fn allocation_rebate_partial_rate_taken() {
        // remain.rate=1000, schema computes to 500 → takes 500, leaves 500
        let mut remain = BaseRebate { rate: dec!(1000), pip: dec!(0), commission: dec!(0), price: 0.0 };
        // schema_rate=0.05, ser=1.0, volume=100 → rate = trunc(0.05 * 1.0 * 100 * 100) = 500
        let (rate_val, _) = calculate_allocation_rebate(&mut remain, 100, dec!(0.05), Decimal::ZERO, 1.0);
        assert_eq!(rate_val, dec!(500));
        assert_eq!(remain.rate, dec!(500));
    }

    #[test]
    fn allocation_rebate_pip_taken_by_percentage() {
        // remain.pip=200, take_percentage=50 → pip_value = trunc(200 * 1.0 * 50 / 100 * 100) = 10000 → capped at 200
        let mut remain = BaseRebate { rate: dec!(0), pip: dec!(200), commission: dec!(0), price: 0.0 };
        let (_rate_val, combined) = calculate_allocation_rebate(&mut remain, 0, Decimal::ZERO, dec!(50), 1.0);
        assert_eq!(combined, dec!(200));
        assert_eq!(remain.pip, dec!(0));
    }

    #[test]
    fn allocation_rebate_negative_percentage_uses_abs() {
        // take_percentage=-30 should behave same as +30
        let mut remain_neg = BaseRebate { rate: dec!(0), pip: dec!(100), commission: dec!(0), price: 0.0 };
        let mut remain_pos = BaseRebate { rate: dec!(0), pip: dec!(100), commission: dec!(0), price: 0.0 };
        let (_, combined_neg) = calculate_allocation_rebate(&mut remain_neg, 0, Decimal::ZERO, dec!(-30), 1.0);
        let (_, combined_pos) = calculate_allocation_rebate(&mut remain_pos, 0, Decimal::ZERO, dec!(30), 1.0);
        assert_eq!(combined_neg, combined_pos);
    }

    // ── get_schema_item_and_percentage ────────────────────────────────────────

    fn make_agent_with_schema(schema_json: &str) -> AgentAccount {
        AgentAccount {
            id: 1,
            uid: 1,
            party_id: 1,
            currency_id: 1,
            fund_type: 0,
            level: 1,
            agent_account_id: None,
            rebate_agent_rule_schema: Some(schema_json.to_string()),
            rebate_agent_rule_level_setting: None,
        }
    }

    #[test]
    fn schema_item_found_for_matching_category() {
        let json = r#"[{"Percentage": 50, "Items": [{"cid": 3, "r": 0.5}]}]"#;
        let agent = make_agent_with_schema(json);
        let (item, pct) = get_schema_item_and_percentage(&agent, 3);
        assert!(item.is_some());
        let (rate, cid) = item.unwrap();
        assert_eq!(rate, dec!(0.5));
        assert_eq!(cid, 3);
        assert_eq!(pct, dec!(50));
    }

    #[test]
    fn schema_item_not_found_for_missing_category() {
        let json = r#"[{"Percentage": 50, "Items": [{"cid": 3, "r": 0.5}]}]"#;
        let agent = make_agent_with_schema(json);
        let (item, pct) = get_schema_item_and_percentage(&agent, 99);
        assert!(item.is_none());
        assert_eq!(pct, Decimal::ZERO);
    }

    #[test]
    fn schema_item_no_schema_returns_none() {
        let agent = AgentAccount {
            id: 1, uid: 1, party_id: 1, currency_id: 1, fund_type: 0,
            level: 1, agent_account_id: None,
            rebate_agent_rule_schema: None,
            rebate_agent_rule_level_setting: None,
        };
        let (item, pct) = get_schema_item_and_percentage(&agent, 3);
        assert!(item.is_none());
        assert_eq!(pct, Decimal::ZERO);
    }

    #[test]
    fn schema_item_invalid_json_returns_none() {
        let agent = make_agent_with_schema("not valid json {{");
        let (item, _) = get_schema_item_and_percentage(&agent, 3);
        assert!(item.is_none());
    }

    // ── calculate_base_rebate_pure ────────────────────────────────────────────
    //
    // Formula (mirrors mono):
    //   rate       = trunc(schema.rate       * ser * 100 * (volume/100) * 100)
    //   commission = trunc(schema.commission * ser * 100 * (volume/100) * 100)
    //   pip        = trunc(schema.pips       * ser * 100 * (volume/100) * pip_formula * 100)
    //
    // where ser = source_exchange_rate, volume is in units of 1/100 lot (e.g. 100 = 1 lot).

    fn make_schema(rate: &str, pips: &str, commission: &str) -> DirectSchemaItem {
        use std::str::FromStr;
        DirectSchemaItem {
            rate: Decimal::from_str(rate).unwrap(),
            pips: Decimal::from_str(pips).unwrap(),
            commission: Decimal::from_str(commission).unwrap(),
            symbol_code: "EURUSD".to_string(),
        }
    }

    #[test]
    fn base_rebate_rate_only_usd_account_1lot() {
        // 1 lot = volume 100, USD account (ser=1.0), rate=0.5, no pips/commission
        // rate = trunc(0.5 * 1.0 * 100 * 1.0 * 100) = trunc(5000) = 5000
        let schema = make_schema("0.5", "0", "0");
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 0.0, 0.0);
        assert_eq!(br.rate, dec!(5000));
        assert_eq!(br.pip, dec!(0));
        assert_eq!(br.commission, dec!(0));
    }

    #[test]
    fn base_rebate_rate_only_early_exit_skips_pip_calc() {
        // pips=0, commission=0 → pip_formula ignored even if non-zero
        let schema = make_schema("1.0", "0", "0");
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 999.0, 1.23);
        assert_eq!(br.rate, dec!(10000));
        assert_eq!(br.pip, dec!(0));
        assert_eq!(br.commission, dec!(0));
        // price is NOT stored in early-exit path (pip_formula not consulted)
        assert_eq!(br.price, 0.0);
    }

    #[test]
    fn base_rebate_pip_calculation_eurusd() {
        // EURUSD: pip_formula = 1.0 (1 pip = $1 per standard lot)
        // 1 lot, ser=1.0, pips=0.3
        // pip = trunc(0.3 * 1.0 * 100 * 1.0 * 1.0 * 100) = trunc(3000) = 3000
        use std::str::FromStr;
        let schema = DirectSchemaItem {
            rate: Decimal::ZERO,
            pips: Decimal::from_str("0.3").unwrap(),
            commission: Decimal::ZERO,
            symbol_code: "EURUSD".to_string(),
        };
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 1.0, 1.08);
        assert_eq!(br.pip, dec!(3000));
        assert_eq!(br.rate, dec!(0));
        assert_eq!(br.commission, dec!(0));
        assert_eq!(br.price, 1.08);
    }

    #[test]
    fn base_rebate_commission_calculation() {
        // commission=0.5, 1 lot, ser=1.0
        // commission = trunc(0.5 * 1.0 * 100 * 1.0 * 100) = 5000
        use std::str::FromStr;
        let schema = DirectSchemaItem {
            rate: Decimal::ZERO,
            pips: Decimal::ZERO,
            commission: Decimal::from_str("0.5").unwrap(),
            symbol_code: "EURUSD".to_string(),
        };
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 0.0, 0.0);
        assert_eq!(br.commission, dec!(5000));
    }

    #[test]
    fn base_rebate_usc_currency_halved_by_exchange_rate() {
        // USC account: ser=0.01 (USC→USD)
        // rate=0.5, 1 lot → rate = trunc(0.5 * 0.01 * 100 * 1.0 * 100) = trunc(50) = 50
        let schema = make_schema("0.5", "0", "0");
        let br = calculate_base_rebate_pure(&schema, 100, 0.01, 0.0, 0.0);
        assert_eq!(br.rate, dec!(50));
    }

    #[test]
    fn base_rebate_fractional_volume_half_lot() {
        // 0.5 lot = volume 50
        // rate=1.0, ser=1.0 → rate = trunc(1.0 * 1.0 * 100 * 0.5 * 100) = trunc(5000) = 5000
        let schema = make_schema("1.0", "0", "0");
        let br = calculate_base_rebate_pure(&schema, 50, 1.0, 0.0, 0.0);
        assert_eq!(br.rate, dec!(5000));
    }

    #[test]
    fn base_rebate_truncates_not_rounds() {
        // rate=0.333, 1 lot, ser=1.0
        // rate = trunc(0.333 * 1.0 * 100 * 1.0 * 100) = trunc(3330) = 3330
        use std::str::FromStr;
        let schema = DirectSchemaItem {
            rate: Decimal::from_str("0.333").unwrap(),
            pips: Decimal::ZERO,
            commission: Decimal::ZERO,
            symbol_code: "EURUSD".to_string(),
        };
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 0.0, 0.0);
        assert_eq!(br.rate, dec!(3330));
    }

    #[test]
    fn base_rebate_all_fields_combined() {
        // rate=0.5, pips=0.3, commission=0.2, 1 lot, ser=1.0, pip_formula=1.0
        use std::str::FromStr;
        let schema = DirectSchemaItem {
            rate: Decimal::from_str("0.5").unwrap(),
            pips: Decimal::from_str("0.3").unwrap(),
            commission: Decimal::from_str("0.2").unwrap(),
            symbol_code: "EURUSD".to_string(),
        };
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 1.0, 1.08);
        assert_eq!(br.rate, dec!(5000));
        assert_eq!(br.pip, dec!(3000));
        assert_eq!(br.commission, dec!(2000));
        assert_eq!(br.total(), dec!(10000));
    }

    #[test]
    fn base_rebate_zero_schema_returns_all_zero() {
        let schema = make_schema("0", "0", "0");
        let br = calculate_base_rebate_pure(&schema, 100, 1.0, 1.0, 1.0);
        assert_eq!(br.rate, dec!(0));
        assert_eq!(br.pip, dec!(0));
        assert_eq!(br.commission, dec!(0));
        assert_eq!(br.total(), dec!(0));
    }

    // ── currency_id_to_name ───────────────────────────────────────────────────

    #[test]
    fn currency_id_known_values() {
        assert_eq!(currency_id_to_name(1), "USD");
        assert_eq!(currency_id_to_name(2), "USC");
        assert_eq!(currency_id_to_name(3), "EUR");
        assert_eq!(currency_id_to_name(10), "HKD");
        assert_eq!(currency_id_to_name(12), "CNY");
    }

    #[test]
    fn currency_id_unknown_falls_back_to_usd() {
        assert_eq!(currency_id_to_name(0), "USD");
        assert_eq!(currency_id_to_name(999), "USD");
    }

    // ── Layer 3: generate_rebates_with_db / send_rebates_with_db (mock DB) ───
    //
    // These tests cover the state-machine branches in generate_rebates and the
    // PendingResend diff logic in send_rebates without touching a real database.

    use std::sync::Mutex;
    use crate::models::rebate::{TargetAccount, TradeRebateRow, STATUS_HAS_NO_REBATE, STATUS_COMPLETED};
    use chrono::Utc;

    // ── MockRebateDb ──────────────────────────────────────────────────────────

    /// Canned responses + call recorder for RebateDb.
    struct MockRebateDb {
        trade_rebate: Option<TradeRebateRow>,
        account_active: bool,
        distribution_type: Option<(i16, Option<i64>)>,
        existing_rebates: Vec<(i64, Decimal)>,
        target_account: Option<TargetAccount>,
        /// Recorded (table, id, status) calls to update_trade_rebate_status
        status_updates: Mutex<Vec<(String, i64, i32)>>,
        /// Recorded rebates passed to insert_rebate
        inserted_rebates: Mutex<Vec<NewRebate>>,
        /// Auto-incrementing id counter for insert_rebate
        next_id: Mutex<i64>,
    }

    impl MockRebateDb {
        fn new() -> Self {
            Self {
                trade_rebate: None,
                account_active: true,
                distribution_type: None,
                existing_rebates: vec![],
                target_account: None,
                status_updates: Mutex::new(vec![]),
                inserted_rebates: Mutex::new(vec![]),
                next_id: Mutex::new(1),
            }
        }

        fn with_trade_rebate(mut self, tr: TradeRebateRow) -> Self {
            self.trade_rebate = Some(tr);
            self
        }
        fn with_account_active(mut self, active: bool) -> Self {
            self.account_active = active;
            self
        }
        fn with_distribution_type(mut self, dt: i16, schema_id: Option<i64>) -> Self {
            self.distribution_type = Some((dt, schema_id));
            self
        }
        fn with_existing_rebates(mut self, existing: Vec<(i64, Decimal)>) -> Self {
            self.existing_rebates = existing;
            self
        }
        fn with_target_account(mut self, ta: TargetAccount) -> Self {
            self.target_account = Some(ta);
            self
        }

        fn status_updates(&self) -> Vec<(String, i64, i32)> {
            self.status_updates.lock().unwrap().clone()
        }
        fn inserted_rebates(&self) -> Vec<NewRebate> {
            self.inserted_rebates.lock().unwrap().clone()
        }
    }

    #[async_trait::async_trait]
    impl RebateDb for MockRebateDb {
        async fn get_trade_rebate(&self, table: &str, _id: i64) -> Result<Option<TradeRebateRow>> {
            Ok(self.trade_rebate.as_ref().map(|tr| {
                let mut t = tr.clone();
                // store the table name in refer_path for assertion convenience
                t.refer_path = table.to_string();
                t
            }))
        }
        async fn is_account_active(&self, _account_id: i64) -> Result<bool> {
            Ok(self.account_active)
        }
        async fn get_distribution_type(&self, _account_id: i64) -> Result<Option<(i16, Option<i64>)>> {
            Ok(self.distribution_type)
        }
        async fn get_existing_rebates(&self, _year: i32, _trade_rebate_id: i64) -> Result<Vec<(i64, Decimal)>> {
            Ok(self.existing_rebates.clone())
        }
        async fn get_target_account(&self, _account_id: i64) -> Result<Option<TargetAccount>> {
            Ok(self.target_account.clone())
        }
        async fn insert_rebate(&self, _year: i32, rebate: &NewRebate) -> Result<i64> {
            let id = {
                let mut n = self.next_id.lock().unwrap();
                let id = *n;
                *n += 1;
                id
            };
            self.inserted_rebates.lock().unwrap().push(rebate.clone());
            Ok(id)
        }
        async fn update_trade_rebate_status(&self, table: &str, id: i64, status: i32) -> Result<()> {
            self.status_updates.lock().unwrap().push((table.to_string(), id, status));
            Ok(())
        }
    }

    fn make_trade_rebate(id: i64, account_id: Option<i64>) -> TradeRebateRow {
        TradeRebateRow {
            id,
            account_id,
            trade_service_id: 1,
            ticket: 1000 + id,
            account_number: 100,
            currency_id: 1,
            volume: 100,
            symbol: "EURUSD".to_string(),
            refer_path: "".to_string(),
            closed_on: Utc::now(),
            opened_on: Utc::now(),
            status: 0,
        }
    }

    fn make_target_account(id: i64) -> TargetAccount {
        TargetAccount { id, party_id: 99, currency_id: 1, fund_type: 0, service_id: 1 }
    }

    // ── send_rebates_with_db tests ────────────────────────────────────────────

    #[tokio::test]
    async fn send_rebates_no_rebates_no_existing_marks_has_no_rebate() {
        let db = MockRebateDb::new();
        let tr = make_trade_rebate(1, Some(10));
        send_rebates_with_db(&db, &tr, vec![], "trd.\"_TradeRebate_2025\"", 2025)
            .await
            .unwrap();

        let updates = db.status_updates();
        assert_eq!(updates.len(), 1);
        assert_eq!(updates[0].2, STATUS_HAS_NO_REBATE);
        assert!(db.inserted_rebates().is_empty());
    }

    #[tokio::test]
    async fn send_rebates_fresh_inserts_all_rebates_and_marks_completed() {
        let db = MockRebateDb::new();
        let tr = make_trade_rebate(2, Some(10));
        let rebates = vec![
            NewRebate { party_id: 1, account_id: 101, trade_rebate_id: 2, currency_id: 1, fund_type: 0, amount: dec!(5000), information: "{}".into() },
            NewRebate { party_id: 2, account_id: 102, trade_rebate_id: 2, currency_id: 1, fund_type: 0, amount: dec!(3000), information: "{}".into() },
        ];

        send_rebates_with_db(&db, &tr, rebates, "trd.\"_TradeRebate_2025\"", 2025)
            .await
            .unwrap();

        let inserted = db.inserted_rebates();
        assert_eq!(inserted.len(), 2);
        assert_eq!(inserted[0].amount, dec!(5000));
        assert_eq!(inserted[1].amount, dec!(3000));

        let updates = db.status_updates();
        assert_eq!(updates.len(), 1);
        assert_eq!(updates[0].2, STATUS_COMPLETED);
    }

    #[tokio::test]
    async fn send_rebates_pending_resend_inserts_diff_for_changed_amount() {
        // existing: account 101 had amount 5000; new calc gives 7000 → diff = +2000
        let db = MockRebateDb::new()
            .with_existing_rebates(vec![(101, dec!(5000))]);
        let tr = make_trade_rebate(3, Some(10));
        let rebates = vec![
            NewRebate { party_id: 1, account_id: 101, trade_rebate_id: 3, currency_id: 1, fund_type: 0, amount: dec!(7000), information: "{}".into() },
        ];

        send_rebates_with_db(&db, &tr, rebates, "trd.\"_TradeRebate_2025\"", 2025)
            .await
            .unwrap();

        let inserted = db.inserted_rebates();
        assert_eq!(inserted.len(), 1);
        assert_eq!(inserted[0].amount, dec!(2000)); // diff only
        assert_eq!(inserted[0].account_id, 101);

        let updates = db.status_updates();
        assert_eq!(updates[0].2, STATUS_COMPLETED);
    }

    #[tokio::test]
    async fn send_rebates_pending_resend_skips_unchanged_amount() {
        // existing amount == new amount → no insert, still marks completed
        let db = MockRebateDb::new()
            .with_existing_rebates(vec![(101, dec!(5000))]);
        let tr = make_trade_rebate(4, Some(10));
        let rebates = vec![
            NewRebate { party_id: 1, account_id: 101, trade_rebate_id: 4, currency_id: 1, fund_type: 0, amount: dec!(5000), information: "{}".into() },
        ];

        send_rebates_with_db(&db, &tr, rebates, "trd.\"_TradeRebate_2025\"", 2025)
            .await
            .unwrap();

        assert!(db.inserted_rebates().is_empty());
        assert_eq!(db.status_updates()[0].2, STATUS_COMPLETED);
    }

    #[tokio::test]
    async fn send_rebates_pending_resend_inserts_compensating_for_removed_account() {
        // existing: account 101 had 5000; new rebates have no entry for 101 → compensating -5000
        let db = MockRebateDb::new()
            .with_existing_rebates(vec![(101, dec!(5000))])
            .with_target_account(make_target_account(101));
        let tr = make_trade_rebate(5, Some(10));

        send_rebates_with_db(&db, &tr, vec![], "trd.\"_TradeRebate_2025\"", 2025)
            .await
            .unwrap();

        let inserted = db.inserted_rebates();
        assert_eq!(inserted.len(), 1);
        assert_eq!(inserted[0].amount, dec!(-5000));
        assert_eq!(inserted[0].account_id, 101);
        assert_eq!(inserted[0].party_id, 99); // from target_account
    }

    #[tokio::test]
    async fn send_rebates_pending_resend_new_account_inserted_in_full() {
        // existing: account 101; new rebates: account 101 (same) + account 102 (new)
        let db = MockRebateDb::new()
            .with_existing_rebates(vec![(101, dec!(5000))]);
        let tr = make_trade_rebate(6, Some(10));
        let rebates = vec![
            NewRebate { party_id: 1, account_id: 101, trade_rebate_id: 6, currency_id: 1, fund_type: 0, amount: dec!(5000), information: "{}".into() },
            NewRebate { party_id: 2, account_id: 102, trade_rebate_id: 6, currency_id: 1, fund_type: 0, amount: dec!(3000), information: "{}".into() },
        ];

        send_rebates_with_db(&db, &tr, rebates, "trd.\"_TradeRebate_2025\"", 2025)
            .await
            .unwrap();

        let inserted = db.inserted_rebates();
        // 101 unchanged → skipped; 102 is new → inserted in full
        assert_eq!(inserted.len(), 1);
        assert_eq!(inserted[0].account_id, 102);
        assert_eq!(inserted[0].amount, dec!(3000));
    }

    // ── run_generate_guards state-machine tests ───────────────────────────────
    //
    // These tests cover all early-exit branches in the guard phase without
    // needing AppContext or a real database connection.

    #[tokio::test]
    async fn guards_trade_rebate_not_found_returns_early_exit_silently() {
        let db = MockRebateDb::new(); // trade_rebate = None
        let outcome = run_generate_guards(&db, 999, "trd.\"_TradeRebate_2025\"")
            .await
            .unwrap();
        assert!(matches!(outcome, GuardOutcome::EarlyExit));
        assert!(db.status_updates().is_empty()); // no status written for missing record
    }

    #[tokio::test]
    async fn guards_null_account_id_marks_has_no_rebate() {
        let db = MockRebateDb::new()
            .with_trade_rebate(make_trade_rebate(10, None)); // account_id = None
        let outcome = run_generate_guards(&db, 10, "trd.\"_TradeRebate_2025\"")
            .await
            .unwrap();
        assert!(matches!(outcome, GuardOutcome::EarlyExit));
        let updates = db.status_updates();
        assert_eq!(updates.len(), 1);
        assert_eq!(updates[0].1, 10);
        assert_eq!(updates[0].2, STATUS_HAS_NO_REBATE);
    }

    #[tokio::test]
    async fn guards_inactive_account_marks_has_no_rebate() {
        let db = MockRebateDb::new()
            .with_trade_rebate(make_trade_rebate(11, Some(50)))
            .with_account_active(false);
        let outcome = run_generate_guards(&db, 11, "trd.\"_TradeRebate_2025\"")
            .await
            .unwrap();
        assert!(matches!(outcome, GuardOutcome::EarlyExit));
        assert_eq!(db.status_updates()[0].2, STATUS_HAS_NO_REBATE);
    }

    #[tokio::test]
    async fn guards_no_distribution_rule_marks_has_no_rebate() {
        let db = MockRebateDb::new()
            .with_trade_rebate(make_trade_rebate(12, Some(50)))
            .with_account_active(true);
        // distribution_type = None (no RebateClientRule for this account)
        let outcome = run_generate_guards(&db, 12, "trd.\"_TradeRebate_2025\"")
            .await
            .unwrap();
        assert!(matches!(outcome, GuardOutcome::EarlyExit));
        assert_eq!(db.status_updates()[0].2, STATUS_HAS_NO_REBATE);
    }

    #[tokio::test]
    async fn guards_active_account_with_rule_returns_proceed() {
        let db = MockRebateDb::new()
            .with_trade_rebate(make_trade_rebate(13, Some(50)))
            .with_account_active(true)
            .with_distribution_type(1, Some(7)); // DIST_DIRECT=1
        let outcome = run_generate_guards(&db, 13, "trd.\"_TradeRebate_2025\"")
            .await
            .unwrap();
        match outcome {
            GuardOutcome::Proceed { trade_rebate, dist_type } => {
                assert_eq!(trade_rebate.id, 13);
                assert_eq!(dist_type, 1);
            }
            GuardOutcome::EarlyExit => panic!("expected Proceed"),
        }
        assert!(db.status_updates().is_empty()); // no early-exit status written
    }

    #[tokio::test]
    async fn guards_unknown_dist_type_still_proceeds_dispatch_handles_it() {
        // The guard phase does NOT reject unknown dist_type — it passes through.
        // The mode dispatch in generate_rebates_with_db produces vec![] for unknown types,
        // which then causes send_rebates to mark HAS_NO_REBATE.
        let db = MockRebateDb::new()
            .with_trade_rebate(make_trade_rebate(14, Some(50)))
            .with_account_active(true)
            .with_distribution_type(99, None);
        let outcome = run_generate_guards(&db, 14, "trd.\"_TradeRebate_2025\"")
            .await
            .unwrap();
        assert!(matches!(outcome, GuardOutcome::Proceed { dist_type: 99, .. }));
    }
}
