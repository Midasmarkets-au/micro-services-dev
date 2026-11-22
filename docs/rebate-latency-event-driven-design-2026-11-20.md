# 返佣链路事件驱动改造设计

状态：**已实施**（分支 `feat/rebate-event-driven-latency`，未提交）
日期：2026-11-20
涉及：`services/scheduler`

三个待确认项已拍板：锁冲突直接 ack 丢弃靠 cron 兜底；三个消费者一并改长轮询；不等业务方确认，先在 staging 验证。

---

## 1. 目标

把"MT 平仓 → 返佣入钱包"的端到端延迟从均值约 **125 秒**降到 **1–2 秒**。

做法是让 calc 和 release 两步都由事件触发，而不是各等一轮 2 分钟的 cron。cron 保留，降级为兜底。

---

## 2. 现状：延迟花在哪

当前链路：

```
MT MySQL ──(轮询 1s)──► TradeMonitor
                             │ publish trade.meta
                             ▼
                        BCR_TRADE ──► TradeHandler
                                          │ INSERT trade_rebate_k8s (status=0)
                                          ▼
                        ┌─ cron 0 0/2 * * * * ─► execute_calculate ─► rebate_k8s + matter(510)
                        │
                        └─ cron 0 0/2 * * * * ─► execute_release   ─► 钱包入账
```

实测各段耗时：

| 阶段 | 耗时 | 来源 |
|---|---|---|
| MT 轮询 | ~1s | `trade_monitor.rs:99` `sleep(1s)` |
| NATS → 写入 `trade_rebate_k8s` | 0–5s | `trade_handler.rs:48` `sleep(5s)` |
| 等 calc cron | 0–120s（均值 60s） | `main.rs:625` `0 0/2 * * * *` |
| 等 release cron | 0–120s（均值 60s） | `main.rs:636` `0 0/2 * * * *` |
| **合计** | **均值 ~125s，最坏 ~245s** | |

两段 cron 等待占了全部延迟的约 **96%**。

### 那 5 秒为什么能消掉

三个消费者（`trade_handler`、`event_trade_handler`、`send_message_handler`）都是同一个写法：

```rust
let mut messages = consumer.fetch().max_messages(10).messages().await?;
// ... 处理 ...
tokio::time::sleep(Duration::from_secs(5)).await;
```

`consumer.fetch()` 是**非阻塞**批量拉取：队列为空时立即返回空批次，于是走到 `sleep(5s)`。这等于把 JetStream 当作轮询对象，放弃了它的推送能力。

改用 `consumer.messages()`（持续长轮询流）后，消息一到即被处理，这 5 秒降到亚秒级。这是独立于本设计的一处改进，三个消费者都适用。

剩下的 1 秒是扫 MT MySQL 的轮询间隔，属于真正的架构下限（除非改成由 MT 侧主动推送）。

---

## 3. 设计

### 3.1 总体

```
TradeHandler ──INSERT trade_rebate_k8s──┬─► publish rebate.calc {tenant_id, trade_rebate_id}
                                        └─► publish trade.event（既有，积分用，条件发布）
                                                │
RebateCalcHandler ◄─────────────────────────────┘
      │ 取 Redis 行级锁 → generate_rebates
      ▼
   publish rebate.release {tenant_id, rebate_id}  （每条新生成的 rebate 一条）
      │
RebateReleaseHandler
      │ 取 Redis 行级锁 → process_rebate → 钱包入账
      ▼
```

cron 原样保留，继续每 2 分钟扫一遍。

### 3.2 为什么不复用 `BCR_EVENT_TRADE`

`trade_handler.rs:113-118` 里那个既有事件是**有条件发布**的：

```rust
let should_publish = rebate.reason != 1        // 非止损
    && rebate.reason != 2                      // 非爆仓
    && hold_duration > chrono::Duration::seconds(60);  // 非剥头皮
```

这是积分业务的规则。返佣必须对**所有**平仓单计算，复用它会让止损单、爆仓单、持仓不足 60 秒的单子永远等不到事件、只能靠 cron 兜底——等于改造对这部分订单完全失效。

所以新开独立的 stream，顺带也隔离了两条链路的积压互相影响。

### 3.3 新增 NATS 定义

在 `nats/client.rs`：

| 常量 | 值 |
|---|---|
| `STREAM_REBATE_CALC` | `BCR_REBATE_CALC` |
| `SUBJECT_REBATE_CALC` | `rebate.calc` |
| `REBATE_CALC_CONSUMER_NAME` | `rebate-calc-handler` |
| `STREAM_REBATE_RELEASE` | `BCR_REBATE_RELEASE` |
| `SUBJECT_REBATE_RELEASE` | `rebate.release` |
| `REBATE_RELEASE_CONSUMER_NAME` | `rebate-release-handler` |

消费者配置沿用现有 `ensure_trade_stream` 的参数：`WorkQueue` 保留策略、`AckPolicy::Explicit`、`max_deliver: -1`、`ack_wait: 300s`。`max_ack_pending` 建议从 10 提到 50，因为返佣计算比写库慢（每笔要查 MT 报价和多级代理链）。

消息体：

```rust
#[derive(Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct RebateCalcMsg { tenant_id: i64, trade_rebate_id: i64 }

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct RebateReleaseMsg { tenant_id: i64, rebate_id: i64 }
```

### 3.4 并发保护（前置条件，必须先做）

**问题**：`generate_rebates` 目前没有任何行级并发保护。没有 `FOR UPDATE`，没有 `SKIP LOCKED`，状态直到全部返佣插入完毕才在 `rebate_calc.rs:1091` 置为 `COMPLETED`。今天之所以安全，仅仅因为只有一个生产者（cron）加一把租户级 Redis 锁。

一旦出现第二条触发路径，同一条 `trade_rebate` 可能被 cron 和事件同时处理：两边都在 `get_existing_rebates` 查到空结果，于是各插一遍完整返佣——**金额翻倍**。

租户级那把锁不能直接复用：粒度是整个租户、TTL 70 分钟，事件撞上 cron 正在跑会走 `Ok(false) => continue` 被静默丢弃，等于事件白发了。

**方案**：每条记录一把 Redis 锁，沿用 `execute_calculate` 已有的 `set_nx` 模式。

```rust
// calc 侧
let lock_key = format!("rebate_calc_lock:{}:{}", tenant_id, trade_rebate_id);
// release 侧
let lock_key = format!("rebate_release_lock:{}:{}", tenant_id, rebate_id);
```

要点：

- TTL 取 120s，略大于单笔计算的最坏耗时（含 MT 报价查询），远小于 `ack_wait` 的 300s。
- cron 路径（`run_for_tenant` 的每次 `generate_rebates` 调用）**也要**取同一把锁，否则保护只有一半。
- 锁冲突时，**事件侧直接 ack 并返回**，不重试——说明另一条路径正在处理这条记录，重投没有意义；真失败了 cron 会兜底。
- 锁必须在处理结束后释放（成功或失败都释放），用和 `execute_calculate:58` 相同的写法。

### 3.5 cron 保留的理由

cron 不删，职责从"唯一驱动"变成"兜底"：

- NATS 消息丢失、stream 被清空、scheduler 重启时窗口内的事件
- 单笔计算抛错（状态仍是 0，下一轮自然重试）
- 锁冲突时被 ack 掉的那些事件
- 历史积压数据（比如这次验证里手工置为 status=5 的记录）

因为状态机本身就是幂等的（status 0/5 → 2），cron 和事件双路径不会重复产出，只要行级锁到位。

间隔维持 2 分钟不变，两个 cron 都保留。

### 3.6 兜底扫描需要配一个部分索引

反直觉的一点：**cron 在事件链路生效之后反而更费资源**。

`get_min_max_id`（`db/rebate_calc.rs:48`）那条 `MIN(id)/MAX(id)` 查询会被 PostgreSQL 改写成按 `Id` 索引有序扫描加 `LIMIT 1`，`status` 只能作为 Filter——`trade_rebate_k8s` 上没有 status 索引。真实库实测计划：

```
Limit → Merge Append → Index Scan using "trade_rebate_k8s_2026_Id_idx"
          Filter: (((status = 0) OR (status = 5)) AND (created_on >= now() - '1 year'))
```

有待处理记录时扫到第一条就停，开销很小。但**一条都没有**时——这正是事件链路正常工作后的常态——必须走完当年和次年两个分区的整个索引才能确认无匹配，且 MIN 和 MAX 是两次独立扫描。

所以要在 `partition.rs` 的建表逻辑里补一个部分索引：

```sql
CREATE INDEX IF NOT EXISTS ix_trade_rebate_k8s_pending
    ON trd.trade_rebate_k8s (id) WHERE status IN (0, 5);
```

这样"无待处理记录"降为 O(1)，既保住 2 分钟的恢复速度，又让常态开销接近零。注意部分索引要建在分区父表上，PostgreSQL 会自动在各子分区上创建对应索引。

`get_pending_ids`（同文件 73 行）用的是相同的过滤条件，一并受益。

---

## 4. 风险与行为变化

### 4.1 返佣金额会变（不是 bug）

返佣金额依赖**计算时点**的 MT 报价——2026-11-20 的验证里已经实证：同一批记录隔了几个月重算，XAUUSD 从 4721.31 变成 3532.63，金额随之变化。

改成即时计算后，用的是更贴近平仓时刻的报价。这在业务上更合理，但金额会与现在的两分钟延迟结果不同。上线前应向业务方说明。

### 4.2 MT 报价查询压力

`db::get_price`（`db/rebate_calc.rs:395`）直连 MySQL，**没有任何缓存**。

但实际压力比预想的小，因为 `get_mt_exchange_rate` 在 `from == to` 时直接短路返回 1.0，一次查询都不发。典型的全 USD 代理链每笔只产生 1–2 次 MySQL 查询（主要是 pip 值那次）；跨币种链路每一级多 1–2 次。

改成即时计算后总查询量不变，只是从"每 2 分钟一批"摊平成逐笔。**暂不需要加缓存**，等批次 3、4 上线后看实测再定。

顺带一提，P0 货币编码修复对查询量基本无影响：USC→USD 从"查 USDUSD 失败 2 次"变成直接短路 0 次（减少），JPY→USD 从"查 USDUSD 失败 2 次"变成"查 JPYUSD / USDJPY 2 次"（持平）。

### 4.3 双路径的可观测性

需要能区分一条返佣是被事件触发还是被 cron 兜底触发的，否则无法判断事件链路是否健康。建议在日志里带上触发源字段，并观察"cron 捡到的笔数"——这个数长期不为零说明事件链路有问题。

---

## 5. 实施步骤

建议按顺序分批，每批可独立上线和回滚：

| 批次 | 内容 | 风险 | 状态 | 落地位置 |
|---|---|---|---|---|
| 1 | 行级 Redis 锁，cron 路径先接入 | 低，纯防御性，无行为变化 | 已完成 | `jobs/row_lock.rs`（新增）、`jobs/rebate.rs`、`jobs/release_rebate.rs` |
| 2 | 三个消费者 `fetch()` → `messages()` | 低，去掉 5s 轮询间隔 | 已完成 | `trade_handler.rs`、`event_trade_handler.rs`、`send_message_handler.rs` |
| 3 | `rebate.calc` 事件：TradeHandler 发布 + 新 handler 消费 | 中，calc 延迟降至亚秒 | 已完成 | `nats/client.rs`、`jobs/rebate_calc_handler.rs`（新增）、`trade_handler.rs`、`main.rs` |
| 4 | `rebate.release` 事件：calc 完成后发布 + 新 handler 消费 | 中，端到端延迟降至 ~1s | 已完成 | `nats/client.rs`、`jobs/rebate_release_handler.rs`（新增）、`jobs/rebate_calc.rs`、`main.rs` |
| 5 | `ix_trade_rebate_k8s_pending` 部分索引（3.6 节） | 低，纯性能 | 已完成 | `db/partition.rs` |

批次 1 是硬前置，没有它批次 3、4 都会引入资金风险。

### 实施中的两处设计细化

**`send_rebates_with_db` 改为返回已插入的 rebate id 列表。** 批次 4 需要知道该为哪些 rebate 发布 release 事件，而这个函数原本只返回 `()`。改成返回 `Vec<i64>` 后，由 `generate_rebates`（持有 `AppContext`，能拿到 jetstream）统一发布，测试侧的 mock 不受影响。两个新测试 `send_rebates_returns_inserted_ids_for_release` 和 `send_rebates_returns_no_ids_when_nothing_is_written` 锁定了这个语义——漏掉一条 id 不会报错，只会让那条返佣悄悄退回等 cron。

**`generate_rebates` 新增 `tenant_id` 参数。** release 消息需要它来定位租户库，而原签名只有 `pool`。两个调用方（cron 的 `run_for_tenant`、事件 handler）本来就持有 `tenant_id`。

### 错误处理的统一取舍

两个新 handler 在所有分支上都 ack，包括计算失败：记录的 `status`/matter 状态不变，cron 下一轮会用新鲜状态重试，比让消息无限重投更可控。只有 payload 解析失败走 `AckKind::Term`——这种消息永远不可能解析成功，留着会占用 ack-pending 槽位并阻塞队列头。

---

## 6. 验证计划

沿用 2026-11-20 那次 staging 验证的环境（`mm` 命名空间，`portal_tenant_bvi`）。

1. **并发安全**：批次 1 上线后，手工把若干记录置为 status=0，同时触发 cron 和（模拟的）事件，确认 `rebate_k8s` 没有同一 `(trade_rebate_id, account_id)` 的重复行。
2. **延迟**：在 MT 测试服造一笔平仓，记录 `trade_rebate_k8s.created_on` 与对应 `rebate_k8s.created_on`、钱包流水时间的差值。目标 < 2s。
3. **兜底有效**：停掉 NATS consumer，造单，确认 cron 仍能在 2 分钟内补上。
4. **金额一致**：同一批记录分别走事件路径和 cron 路径计算，在报价不变的前提下金额应完全相同。

---

## 7. 待确认

1. 行级锁冲突时直接 ack 丢弃事件、依赖 cron 兜底——这个取舍是否接受？另一个选项是 `nak_with_delay` 延迟重投，延迟更低但实现更复杂，且要防重投风暴。
2. 批次 2 会改动 `send_message_handler`（发信）和 `event_trade_handler`（积分）这两条与返佣无关的链路。它们的延迟同样会从 5s 降到亚秒——是收益，但属于本次改造的范围外影响，需要确认可以一并改。
3. 即时计算导致返佣金额与现在不同（4.1 节），是否需要业务方书面确认后再上批次 3。

---

## 附录：与 MM-Back 的 staging 实测对比（2026-11-20）

### 方法

staging MT MySQL（`13.212.107.58/metatrader4`）的数据止于 2026-07-17，其中 MM-Back
与 Rust 都处理过的最近一段是 **2026-07-01 ~ 07-06 共 21 笔**（service 20 / MT4）。

- **MM-Back 基线**：直接取 staging 库里它已有的结果，耗时用
  `trd."_TradeRebate"."CreatedOn"` → `core."_Matter"."PostedOn"`（`_TradeRebate.UpdatedOn`
  建行时即写死、完成时不更新，不能用）。
- **Rust**：删掉这 21 笔在 `*_k8s` 侧的全部派生数据（21 trade_rebate / 39 rebate /
  39 matter / 39 activity / 39 wallet_transaction，已备份至 `/tmp/rebate-cmp/`），
  把 Redis 游标 `trade:monitor:last_time:20` 回拨到 2026-07-01 并清空
  `trade:queue:dedup`，让 TradeMonitor 从 MySQL 重新拉取，走完整的事件化链路。

实际回放只完成了 07-01 的 13 笔，原因见下方「TradeHandler 吞吐瓶颈」。

### 耗时

| | MM-Back (C#) | Rust（事件化） |
|---|---|---|
| 样本 | 35 笔（2026-07） | 13 笔 |
| 中位数 | **48.3 s** | 首笔 **3.44 s** |
| 最小 / 最大 | 3.9 s / 118.3 s | 3.44 s / 25.1 s |

Rust 这 13 笔在 23 秒内集中到达，calc 串行处理（每笔约 3.3 s），所以延迟从 3.44 s
线性涨到 25.1 s。**无竞争时的真实值是 3.44 s**；即使是排队最久的一笔也低于 C# 的中位数。
release 稳定在 calc 完成后约 1.2 s。

### 金额

13 笔逐笔对比：**5 笔完全一致，8 笔偏差 +0.06% ~ +0.12%**，偏差全部落在需要实时
交叉汇率的 CHF 品种上；EURUSD、USDJPY 分文不差。

取 ticket 12857678（EURCHF）的 `information` 对照，两边唯一不同的输入是 `price`：

| | rate | depth | exchangeRate | price | pip | total |
|---|---|---|---|---|---|---|
| MM-Back | 120000 | 1 | 1.0 | 0.80873 | 61825 | 181825 |
| Rust | 120000 | 1 | 1.0 | 0.80733 | 61932 | 181932 |

0.80873 / 0.80733 = 1.001734，61932 / 61825 = 1.001731 —— 偏差完全由 MM-Back 在
7 月取价、Rust 在今天取价造成，算法一致。

### 并发安全

13 笔产生 13 条返佣、13 个不同 `(trade_rebate_id, account_id)`，**零重复**。
期间 cron 与事件确有竞争，日志里既有被锁挡下的
（`locked by another worker, leaving to it`），也有一笔 `7529434852706222080`
被先后算了两次（事件一次、cron 一次）——行锁只阻止**并发**重入，不阻止 cron
拿着过期快照在锁释放后**顺序**重跑。这一笔之所以没有多发，是靠 P0-3 的差额逻辑
算出 0 后不写入。建议在锁内补一次 status 复查，省掉这次无用计算。

### 顺带发现的两个问题

**1. TradeHandler 吞吐瓶颈 —— 未命中不缓存**

`get_account_tenant_id`（`jobs/trade_handler.rs`）只在找到租户时写 Redis，
返回 `None` 时不写。MT 服务器上绝大多数成交属于未注册账户，于是每条消息都要重跑
一遍「查全部租户 → 逐个租户查账户」，跨区（scheduler 在 ap-southeast-2、RDS 在
ap-southeast-1）每笔约 370 ms，实测吞吐仅 **2.7 笔/秒**。日志中同一个账户号
（如 1362509267）反复出现、每次都付全价。本次回放 32,110 条消息因此无法在合理
时间内跑完，这也是只拿到 13 笔的原因。建议对未命中也写一个短 TTL 的负缓存。

**2. 单条脏数据会永久卡死行情摄取**

`MT4_TRADES` 里 ticket 12927466（`#JPN225`，LOGIN 0）的 `CLOSE_TIME` 是
**2038-01-19 03:14:07**，即 int32 epoch 溢出值。TradeMonitor 按 CLOSE_TIME 升序推进
游标，吃到这一行后 `trade:monitor:last_time:20` 被推到 2038，此后
`CLOSE_TIME > 2038` 永远查不到任何数据，摄取彻底停摆。游标键 30 天 TTL 且只在
「本轮有数据」时刷新，所以会在 30 天后自愈——但这 30 天里一笔都进不来。
建议在 `poll_closed_trades` 加上 `CLOSE_TIME <= NOW()` 的上界。
