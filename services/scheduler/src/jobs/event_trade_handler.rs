/// EventTradeHandler: consumes BCR_EVENT_TRADE NATS stream and processes trade points.
///
/// Mirrors mono's EventService.ProcessTradeSourceAsync logic:
///   1. Load TradeRebate from trd.trade_rebate_k8s
///   2. Skip if reason==1/2 (stop-loss/margin-call) or hold duration <=1 min
///   3. Calculate and award points to client and direct agent
///   4. Update EventShopClientPoint.Volume for all upstream parents
///   5. Reward rebate (stub — requires MT API, deferred)
use anyhow::Result;
use chrono::Duration;
use futures::StreamExt;
use serde::Deserialize;
use tracing::{error, info, warn};

use crate::db::event;
use crate::nats::client::{EVENT_CONSUMER_NAME, STREAM_EVENT};
use crate::AppContext;

/// USC currency ID — mirrors CurrencyTypes.USC in mono
const CURRENCY_USC: i32 = 16;
/// AccountRoleTypes.Agent
const ROLE_AGENT: i16 = 200;

/// Redis dedup key format mirrors mono's MQSource.ToRedisKey():
///   event_shop_mq_src_tid:{TenantId}_sourceType:{SourceType}_rowId{RowId}
fn dedup_redis_key(tenant_id: i64, trade_rebate_id: i64) -> String {
    format!(
        "event_shop_mq_src_tid:{}_sourceType:2_rowId{}",
        tenant_id, trade_rebate_id
    )
}

/// Source content JSON mirrors what C# builds for Trade source:
///   { "AccountNumber": ..., "Ticket": ..., "Volume": ..., "ServiceId": ... }
fn build_source_content(account_number: i64, ticket: i64, volume: i32, service_id: i32) -> String {
    serde_json::json!({
        "AccountNumber": account_number,
        "Ticket": ticket,
        "Volume": volume,
        "ServiceId": service_id,
    })
    .to_string()
}

/// Mirrors TradeRebate.ClosedLessThanOneMinute()
fn closed_less_than_one_minute(opened: chrono::DateTime<chrono::Utc>, closed: chrono::DateTime<chrono::Utc>) -> bool {
    closed - opened <= Duration::minutes(1)
}

/// Incoming NATS message body (mirrors C# EventShopPointTransaction.MQSource)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct MqSource {
    source_type: u8,
    row_id: i64,
    tenant_id: i64,
}

/// Entry point: runs the NATS pull consumer loop indefinitely.
pub async fn run(ctx: AppContext) -> Result<()> {
    let stream = ctx
        .jetstream
        .get_stream(STREAM_EVENT)
        .await
        .map_err(|e| anyhow::anyhow!("EventTradeHandler: failed to get stream {}: {}", STREAM_EVENT, e))?;

    let consumer = stream
        .get_consumer::<async_nats::jetstream::consumer::pull::Config>(EVENT_CONSUMER_NAME)
        .await
        .map_err(|e| {
            anyhow::anyhow!(
                "EventTradeHandler: failed to get consumer {}: {}",
                EVENT_CONSUMER_NAME,
                e
            )
        })?;

    info!("EventTradeHandler: listening on NATS stream {}", STREAM_EVENT);

    loop {
        let mut messages = match consumer.fetch().max_messages(10).messages().await {
            Ok(m) => m,
            Err(e) => {
                warn!("EventTradeHandler: fetch error (will retry): {:#}", e);
                tokio::time::sleep(std::time::Duration::from_secs(1)).await;
                continue;
            }
        };

        while let Some(msg_result) = messages.next().await {
            match msg_result {
                Ok(msg) => {
                    if let Err(e) = process_message(&ctx, msg).await {
                        error!("EventTradeHandler: failed to process message: {:#}", e);
                    }
                }
                Err(e) => error!("EventTradeHandler: message error: {:#}", e),
            }
        }

        tokio::time::sleep(std::time::Duration::from_secs(5)).await;
    }
}

async fn process_message(ctx: &AppContext, msg: async_nats::jetstream::Message) -> Result<()> {
    // 1. Parse MqSource
    let src: MqSource = match serde_json::from_slice(&msg.payload) {
        Ok(s) => s,
        Err(e) => {
            warn!("EventTradeHandler: invalid message payload: {:#}", e);
            msg.ack().await.ok();
            return Ok(());
        }
    };

    if src.source_type != 2 {
        warn!("EventTradeHandler: unexpected source_type={}, skipping", src.source_type);
        msg.ack().await.ok();
        return Ok(());
    }

    let trade_rebate_id = src.row_id;
    let tenant_id = src.tenant_id;

    // 2. Redis dedup (TTL 2h, mirrors mono PollEventTradeHandler)
    let dedup_key = dedup_redis_key(tenant_id, trade_rebate_id);
    if ctx.cache.get_string(&dedup_key).await?.is_some() {
        info!(
            "EventTradeHandler: duplicate message trade_rebate_id={} tenant={}, skipping",
            trade_rebate_id, tenant_id
        );
        msg.ack().await.ok();
        return Ok(());
    }
    ctx.cache
        .set_string(&dedup_key, "1", std::time::Duration::from_secs(7200))
        .await?;

    let pool = ctx.tenant_pool(tenant_id).await?;

    // 3. Load TradeRebate
    let trade = match event::get_trade_rebate(&pool, trade_rebate_id).await? {
        Some(t) => t,
        None => {
            warn!(
                "EventTradeHandler: trade_rebate_id={} not found for tenant={}",
                trade_rebate_id, tenant_id
            );
            msg.ack().await.ok();
            return Ok(());
        }
    };

    let account_id = match trade.account_id {
        Some(id) if id != 0 => id,
        _ => {
            info!(
                "EventTradeHandler: trade_rebate_id={} has no valid account_id, skipping",
                trade_rebate_id
            );
            msg.ack().await.ok();
            return Ok(());
        }
    };

    // 4. Secondary filters (mirrors ProcessTradeSourceAsync)
    //    reason 1=stop-loss, 2=margin-call → skip
    if trade.reason == 1 || trade.reason == 2 {
        info!(
            "EventTradeHandler: trade_rebate_id={} reason={} skipped (stop-loss/margin-call)",
            trade_rebate_id, trade.reason
        );
        msg.ack().await.ok();
        return Ok(());
    }
    if closed_less_than_one_minute(trade.opened_on, trade.closed_on) {
        info!(
            "EventTradeHandler: trade_rebate_id={} hold<=1min skipped",
            trade_rebate_id
        );
        msg.ack().await.ok();
        return Ok(());
    }

    // 5. TODO(ProcessTradeRewardAsync): 积分商城「交易返现奖励」功能 — 暂未实现
    //
    // 背景：用户在积分商城用积分兑换 EventShopReward（ClientReward / AgentReward / SalesReward）后，
    // 每次平仓自动按手数向 MT 账户或 Wallet 返现真实资金。
    //
    // 涉及两个子流程：
    //
    // A) ProcessTradeRewardForClientAsync — 客户返现
    //    1. 查 event._EventShopReward (Status=Active, Type=ClientReward, EffectiveTo > now)
    //    2. 按 RewardType (Point1000/3000/5000) + AccountType (Alpha/AlphaPlus vs 普通) 定单价(cents/100 lots)
    //    3. rewardAmount = rewardUnitInCents * Volume / 100
    //    4. INSERT event._EventShopRewardRebate
    //    5. UPDATE event._EventShopReward.TotalPoint += rewardAmount
    //    6. tradingApiService.ChangeBalance(serviceId, accountNumber, amount, comment)
    //       → 需要 MT gRPC ChangeBalance 接口（scheduler 目前不具备）
    //
    // B) ProcessTradeRewardForParentAsync — Agent/Sales 返现
    //    1. 遍历客户的 AgentAccountId + SalesAccountId
    //    2. 查各自 EventShopReward (Status=Active, Type=AgentReward/SalesReward, EffectiveTo > now)
    //    3. 固定金额（per trade）按 RewardType + Alpha 判断
    //    4. INSERT event._EventShopRewardRebate
    //    5. 汇率换算: rebateService.GetMtExchangeRate(serviceId, USD, targetCurrency)
    //    6. accountingService.WalletChangeBalanceAsync(walletId, walletAdjustId, actualAmount)
    //       → 需要 WalletAdjust + accounting Wallet 写入逻辑
    //
    // 实现前提：
    //   - 在 scheduler 中添加 MT gRPC ChangeBalance 客户端
    //   - 或通过 mono callback gRPC 转发（MonoCallbackGrpcService 扩展）
    //   - 参考实现：MM-Back EventService.ProcessTradeRewardAsync (L293–L454)
    //
    // 当前影响范围：仅对持有有效 EventShopReward 的用户有影响，普通交易不受影响。

    // 6. Compute multipliers
    // USC currency: volume unit is 0.01 USD → point_multiplier=0.01; others: 1.0
    let point_multiplier: f64 = if trade.currency_id == CURRENCY_USC { 0.01 } else { 1.0 };

    let source_content = build_source_content(
        trade.account_number,
        trade.ticket,
        trade.volume,
        trade.trade_service_id,
    );

    // 7. Client self points
    //    Mirrors C#: (trade.Volume * 100).ToScaledFromCents() * pointMultiplier
    //    = Volume * 100 * 10_000 * multiplier = Volume * 1_000_000 * multiplier
    if let Some(self_event_party_id) = event::get_event_party_id(&pool, account_id).await? {
        if event::check_point_transaction_exists(&pool, self_event_party_id, account_id, &source_content).await? {
            info!(
                "EventTradeHandler: self point transaction already exists trade_rebate_id={} account_id={}",
                trade_rebate_id, account_id
            );
        } else {
            let point_scaled = (trade.volume as i64 * 100 * 10_000) as f64 * point_multiplier;
            let point_scaled = point_scaled as i64;
            if point_scaled > 0 {
                event::change_point(
                    &pool,
                    self_event_party_id,
                    point_scaled,
                    account_id,
                    trade_rebate_id,
                    &source_content,
                )
                .await?;
                info!(
                    "EventTradeHandler: awarded {} points to account_id={} trade_rebate_id={}",
                    point_scaled, account_id, trade_rebate_id
                );
            }
        }
    }

    // 8. Agent points (30% of client points)
    //    Mirrors C#: only awarded when directAgentId != 0 && role == Agent
    let account = event::get_account_for_trade(&pool, account_id).await?;
    if let Some(acc) = &account {
        if let Some(agent_account_id) = acc.agent_account_id {
            // Verify the agent's own role record (mirrors GetAccountRoleByIdAsync check)
            let agent_acc = event::get_account_for_trade(&pool, agent_account_id).await?;
            let is_agent = agent_acc.map(|a| a.role == ROLE_AGENT).unwrap_or(false);

            if is_agent {
                if let Some(agent_event_party_id) =
                    event::get_event_party_id(&pool, agent_account_id).await?
                {
                    // Ensure ClientPoint relationship exists
                    event::ensure_client_point_exists(
                        &pool,
                        account_id,
                        agent_account_id,
                        ROLE_AGENT,
                    )
                    .await?;

                    if event::check_point_transaction_exists(
                        &pool,
                        agent_event_party_id,
                        agent_account_id,
                        &source_content,
                    )
                    .await?
                    {
                        info!(
                            "EventTradeHandler: agent point transaction already exists agent_id={} trade_rebate_id={}",
                            agent_account_id, trade_rebate_id
                        );
                    } else {
                        // Mirrors C#: (trade.Volume * 100).ToScaledFromCents() * 0.3 * pointMultiplier
                        let agent_point_scaled =
                            (trade.volume as i64 * 100 * 10_000) as f64 * point_multiplier * 0.3;
                        let agent_point_scaled = agent_point_scaled as i64;
                        if agent_point_scaled > 0 {
                            event::change_point(
                                &pool,
                                agent_event_party_id,
                                agent_point_scaled,
                                agent_account_id,
                                trade_rebate_id,
                                &source_content,
                            )
                            .await?;
                            info!(
                                "EventTradeHandler: awarded {} points to agent_id={} trade_rebate_id={}",
                                agent_point_scaled, agent_account_id, trade_rebate_id
                            );
                        }
                    }

                    // Update agent's upstream ClientPoint Volume
                    update_parent_volumes(&pool, agent_account_id, trade.volume, trade.currency_id).await?;
                }
            }
        }
    }

    // 9. Update ClientPoint Volume for client's direct parents (ProcessTradeForParentAsync)
    update_parent_volumes(&pool, account_id, trade.volume, trade.currency_id).await?;

    msg.ack().await.ok();
    info!(
        "EventTradeHandler: processed trade_rebate_id={} tenant={}",
        trade_rebate_id, tenant_id
    );
    Ok(())
}

/// Update Volume on all valid upstream EventShopClientPoint rows for a given child account.
/// Mirrors: ProcessTradeForParentAsync → AddTradePointByIdAsync
async fn update_parent_volumes(pool: &sqlx::PgPool, child_account_id: i64, volume: i32, currency_id: i32) -> Result<()> {
    let ids = event::get_valid_client_point_ids(pool, child_account_id).await?;
    if ids.is_empty() {
        return Ok(());
    }
    let volume_multiplier: f64 = if currency_id == CURRENCY_USC { 0.01 } else { 1.0 };
    let adjusted_volume = (volume as f64 * volume_multiplier) as i64;
    for id in ids {
        event::add_trade_volume_to_client_point(pool, id, adjusted_volume).await?;
    }
    Ok(())
}
