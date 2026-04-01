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
    let divided_volume = Decimal::new(volume as i64, 2); // volume / 100
    let source_exchange_rate_f64 =
        get_mt_exchange_rate(ctx, tenant_pool, service_id, source_currency_id, USD).await?;
    let source_exchange_rate = Decimal::from_f64(source_exchange_rate_f64).unwrap_or(Decimal::ONE);

    let rate = dec_round_to_zero(schema_item.rate * source_exchange_rate * dec(100) * divided_volume * dec(100));

    if schema_item.pips == Decimal::ZERO && schema_item.commission == Decimal::ZERO {
        return Ok(BaseRebate {
            rate,
            pip: Decimal::ZERO,
            commission: Decimal::ZERO,
            price: 0.0,
        });
    }

    let commission = dec_round_to_zero(schema_item.commission * source_exchange_rate * dec(100) * divided_volume * dec(100));

    let (pip_formula_f64, exchange_rate_price) =
        get_pip_value(ctx, tenant_pool, service_id, &schema_item.symbol_code).await?;
    let pip_formula = Decimal::from_f64(pip_formula_f64).unwrap_or(Decimal::ZERO);

    let pip = dec_round_to_zero(schema_item.pips * source_exchange_rate * dec(100) * divided_volume * pip_formula * dec(100));

    Ok(BaseRebate {
        rate,
        pip,
        commission,
        price: exchange_rate_price,
    })
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
    let trade_rebate = match db::get_trade_rebate(pool, table, trade_rebate_id).await? {
        Some(tr) => tr,
        None => {
            warn!("CalculateRebate: TradeRebate id={} not found in {}", trade_rebate_id, table);
            return Ok(());
        }
    };

    let account_id = match trade_rebate.account_id {
        Some(id) => id,
        None => {
            db::update_trade_rebate_status(pool, table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
            return Ok(());
        }
    };

    // Check account is active
    if !db::is_account_active(pool, account_id).await? {
        db::update_trade_rebate_status(pool, table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
        return Ok(());
    }

    // Get distribution type
    let dist_type = match db::get_distribution_type(pool, account_id).await? {
        Some((dt, _)) => dt,
        None => {
            db::update_trade_rebate_status(pool, table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
            return Ok(());
        }
    };

    // Generate rebates based on distribution type
    let rebates = match dist_type {
        DIST_DIRECT => generate_direct_rebates(ctx, pool, &trade_rebate).await?,
        DIST_ALLOCATION => generate_allocation_rebates(ctx, pool, &trade_rebate).await?,
        DIST_LEVEL_PERCENTAGE => generate_level_percentage_rebates(ctx, pool, &trade_rebate).await?,
        _ => vec![],
    };

    // Send / persist rebates (mirrors mono's SendRebates)
    send_rebates(ctx, pool, &trade_rebate, rebates, table, year).await
}

/// Mirrors mono's SendRebates: handles PendingResend diff adjustment and writes to DB.
async fn send_rebates(
    _ctx: &AppContext,
    pool: &PgPool,
    trade_rebate: &TradeRebateRow,
    rebates: Vec<NewRebate>,
    table: &str,
    year: i32,
) -> Result<()> {
    let trade_rebate_id = trade_rebate.id;

    // Check if no rebates generated
    let existing = db::get_existing_rebates(pool, year, trade_rebate_id).await?;

    if rebates.is_empty() && existing.is_empty() {
        db::update_trade_rebate_status(pool, table, trade_rebate_id, STATUS_HAS_NO_REBATE).await?;
        return Ok(());
    }

    // PendingResend: diff adjustment
    if !existing.is_empty() {
        // Build map of existing rebates by account_id
        let existing_map: HashMap<i64, Decimal> = existing.into_iter().collect();
        let new_map: HashMap<i64, Decimal> = rebates.iter().map(|r| (r.account_id, r.amount)).collect();

        // For accounts in existing but not in new → insert negative adjustment
        for (account_id, existing_amount) in &existing_map {
            if !new_map.contains_key(account_id) {
                // Find the original rebate to get party_id / currency_id / fund_type
                // We need to re-fetch — use a simplified approach: insert compensating rebate
                let compensating = NewRebate {
                    party_id: 0, // will be looked up below
                    account_id: *account_id,
                    trade_rebate_id,
                    currency_id: -1,
                    fund_type: 0,
                    amount: -existing_amount,
                    information: serde_json::to_string(&json!({"Note": "compensating_adjustment", "Version": "v2_scheduler"}))?,
                };
                // Get account info for compensation
                if let Some(target) = db::get_target_account(pool, *account_id).await? {
                    let comp = NewRebate {
                        party_id: target.party_id,
                        currency_id: target.currency_id,
                        fund_type: target.fund_type,
                        ..compensating
                    };
                    db::insert_rebate(pool, year, &comp).await?;
                }
            }
        }

        // For accounts in both: insert diff if amount changed
        let mut to_insert = vec![];
        for rebate in &rebates {
            if let Some(&existing_amount) = existing_map.get(&rebate.account_id) {
                if rebate.amount != existing_amount {
                    let diff = NewRebate {
                        amount: rebate.amount - existing_amount,
                        ..rebate.clone()
                    };
                    to_insert.push(diff);
                }
                // If equal, skip (no change)
            } else {
                to_insert.push(rebate.clone());
            }
        }

        for rebate in to_insert {
            db::insert_rebate(pool, year, &rebate).await?;
        }
    } else {
        // Fresh insert
        for rebate in &rebates {
            db::insert_rebate(pool, year, rebate).await?;
        }
    }

    db::update_trade_rebate_status(pool, table, trade_rebate_id, STATUS_COMPLETED).await?;

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
fn currency_id_to_name(currency_id: i32) -> &'static str {
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
