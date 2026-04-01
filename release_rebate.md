Release Rebate — 流程共识与实现记录

 流程共识

 触发链

 每2分钟 cron（scheduler）
   → 检查 Redis "ReleaseRebateDisabledKey"（存在则整体跳过）
   → 并发处理每个 tenant
       → 检查 core._Configuration RebateEnabled（false 则跳过）
       → 查询 StateId IN (510, 520) 的 rebate → 顺序逐条处理

 单条 Rebate 处理逻辑

 ① 读 trd._Account → IsClosed!=0 OR Status!=0 → 写 StateId=505，退出
 ② 读 _AccountWithTag + core._Tag (PauseReleaseRebate) → 有标签 → 写 StateId=505，退出
 ③ 解析 wallet_id（Account.WalletId 或从 acct._Wallet 查找；null 时回填 trd._Account）
 ④ BEGIN TRANSACTION
    读 acct._Wallet FOR UPDATE（行锁，prev_balance）
    读 acct._WalletTransaction WHERE MatterId=rebate_id（幂等检查）
      → 已存在：只写 StateId=550，COMMIT
    计算 new_balance = prev_balance + amount（整数 cents，无精度问题）
    写 acct._Wallet.Balance / INSERT _WalletTransaction / INSERT core._Activity
    写 core._Matter StateId=550, StatedOn=NOW()
    COMMIT

 读写表

 ┌────────────────────┬────────┬────────────────────────────┬─────────────────────┐
 │         表         │ Schema │             读             │         写          │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Configuration     │ core   │ ✓ RebateEnabled            │                     │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Rebate            │ trd    │ ✓ Amount, AccountId        │                     │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Matter            │ core   │ ✓ StateId                  │ ✓ StateId, StatedOn │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Account           │ trd    │ ✓ WalletId/Status/IsClosed │ ✓ WalletId（回填）  │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _AccountWithTag    │ trd    │ ✓ 暂停标签                 │                     │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Tag               │ core   │ ✓ Name                     │                     │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Wallet            │ acct   │ ✓ Balance FOR UPDATE       │ ✓ Balance           │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _WalletTransaction │ acct   │ ✓ 幂等检查                 │ ✓ INSERT            │
 ├────────────────────┼────────┼────────────────────────────┼─────────────────────┤
 │ _Activity          │ core   │                            │ ✓ INSERT 审计       │
 └────────────────────┴────────┴────────────────────────────┴─────────────────────┘

 状态流

 510/520 → 550 RebateCompleted（成功）
 510/520 → 505 RebateCanceled（账户无效或暂停标签）

 ---
 Context（实现背景）

 目前 ReleaseRebate 由 Rust scheduler 每 2 分钟通过 gRPC 触发 mono 的 Hangfire job 执行。
 需求：将完整的 ReleaseRebate 逻辑迁移到 scheduler 中直接实现（直接操作 DB），
 mono 代码保持不变（停止触发 gRPC 即可，mono 的 Hangfire job 就不会再被调用）。

 ---
 目标数据库操作

 涉及表（tenant DB）：
 - core."_Configuration" — 检查 RebateEnabled toggle
 - trd."_Rebate" + core."_Matter" — 查询待释放 rebate
 - trd."_Account" + trd."_AccountWithTag" + core."_Tag" — 账户验证 + 暂停标签
 - acct."_WalletTransaction" — 幂等性检查（是否已释放）
 - acct."_Wallet" — 余额更新（SELECT ... FOR UPDATE）
 - core."_Activity" — 状态变更审计记录
 - core."_Matter" — 更新 StateId

 状态常量：
 - 510 = RebateOnHold（待释放）
 - 520 = RebateReleased（已标记释放）
 - 550 = RebateCompleted（最终态，释放完成）
 - 505 = RebateCanceled（取消，账户无效 or 暂停标签）
 - operatorPartyId = 1（系统用户）

 ---
 实现步骤

 Step 1: 新建 services/scheduler/src/db/rebate.rs

 包含以下 SQL 函数（全部使用 sqlx query）：

 a. is_rebate_enabled(pool) -> Result<bool>
 SELECT "Value" FROM core."_Configuration"
 WHERE "Category" = 'Public' AND "Key" = 'RebateEnabled'
   AND "RowId" = 0
 LIMIT 1
 解析 JSON {"Value":true} 取布尔值；若无记录默认返回 false。

 b. get_pending_rebate_ids(pool) -> Result<Vec<i64>>
 SELECT r."Id"
 FROM trd."_Rebate" r
 INNER JOIN core."_Matter" m ON r."Id" = m."Id"
 WHERE m."StateId" IN (510, 520)
 ORDER BY r."Id"

 c. struct RebateRow (Id, AccountId, Amount, current StateId)
 SELECT r."Id", r."AccountId", r."Amount", m."StateId"
 FROM trd."_Rebate" r
 INNER JOIN core."_Matter" m ON r."Id" = m."Id"
 WHERE r."Id" = $1

 d. struct AccountRow (Id, PartyId, WalletId, Status, IsClosed, CurrencyId, FundType, has_pause_tag)
 SELECT a."Id", a."PartyId", a."WalletId", a."Status", a."IsClosed",
        a."CurrencyId", a."FundType",
        EXISTS(
          SELECT 1 FROM trd."_AccountWithTag" awt
          INNER JOIN core."_Tag" t ON awt."TagId" = t."Id"
          WHERE awt."AccountId" = a."Id" AND t."Name" = 'PauseReleaseRebate'
        ) AS has_pause_tag
 FROM trd."_Account" a WHERE a."Id" = $1

 e. process_rebate(pool, rebate_id) -> Result<()> — 核心事务逻辑：

 BEGIN TRANSACTION
   1. 查询 RebateRow (rebate + matter)
   2. 查询 AccountRow (account + pause tag)
   3. 账户验证：
      - account 不存在 OR IsClosed != 0 OR Status != 0 (Activate)
        → 取消 (StateId=505)，INSERT Activity，UPDATE Matter，COMMIT，return
   4. 若 has_pause_tag = true
        → 取消 (StateId=505)，INSERT Activity，UPDATE Matter，COMMIT，return
   5. 解析 wallet_id：
      - account.WalletId 有值则直接使用
      - 否则：SELECT acct._Wallet WHERE PartyId+CurrencyId+FundType，UPDATE trd._Account.WalletId
   6. SELECT acct._Wallet WHERE Id=$wallet_id FOR UPDATE（行锁）
   7. 检查幂等：SELECT EXISTS acct._WalletTransaction WHERE MatterId=$rebate_id
      - 若已存在：只 transit 到 550，不更新余额，COMMIT，return
   8. 计算 new_balance = prev_balance + rebate.amount
   9. UPDATE acct._Wallet SET Balance=$new_balance WHERE Id=$wallet_id
   10. INSERT acct._WalletTransaction (WalletId, MatterId, PrevBalance, Amount, CreatedOn, UpdatedOn)
   11. INSERT core._Activity (MatterId=rebate_id, PartyId=1, PerformedOn=NOW(), OnStateId=current, ActionId=0,
 ToStateId=550, Data="Release By System")
   12. UPDATE core._Matter SET StateId=550, StatedOn=NOW() WHERE Id=$rebate_id
 COMMIT

 Step 2: 新建 services/scheduler/src/jobs/release_rebate.rs

 pub async fn execute(ctx: AppContext) -> Result<()> {
     // 1. 检查 Redis 禁用标志 "ReleaseRebateDisabledKey"
     if ctx.cache.get_string("ReleaseRebateDisabledKey").await?.is_some() {
         info!("ReleaseRebate is disabled via cache key, skipping");
         return Ok(());
     }

     // 2. 获取所有 tenant IDs（复用 db::tenant::get_all_tenant_ids）
     let tenant_ids = db::tenant::get_all_tenant_ids(&ctx.central_pool).await?;

     // 3. 并发处理各 tenant（tokio::join 或 futures::join_all）
     let tasks: Vec<_> = tenant_ids.into_iter().map(|tid| {
         let ctx = ctx.clone();
         tokio::spawn(async move { process_tenant(ctx, tid).await })
     }).collect();
     futures::future::join_all(tasks).await;

     Ok(())
 }

 async fn process_tenant(ctx: AppContext, tenant_id: i64) -> Result<()> {
     let pool = ctx.tenant_pool(tenant_id).await?;

     // 检查 RebateEnabled toggle
     if !db::rebate::is_rebate_enabled(&pool).await? {
         return Ok(());
     }

     let ids = db::rebate::get_pending_rebate_ids(&pool).await?;
     for rebate_id in ids {
         if let Err(e) = db::rebate::process_rebate(&pool, rebate_id).await {
             tracing::error!(tenant_id, rebate_id, error = %e, "ReleaseRebate: failed to process rebate");
         }
     }
     Ok(())
 }

 Step 3: 修改 services/scheduler/src/jobs/rebate.rs

 将 execute_release() 改为调用 release_rebate::execute() 而非 gRPC：

 pub async fn execute_release(ctx: AppContext) -> Result<()> {
     info!("ReleaseRebateJob: executing directly");
     release_rebate::execute(ctx).await
 }

 Step 4: 修改 services/scheduler/src/db/mod.rs

 pub mod rebate;  // 新增

 Step 5: 修改 services/scheduler/src/jobs/mod.rs

 pub mod release_rebate;  // 新增

 ---
 关键文件

 ┌───────────────────────────────────────────────┬──────────────────────────────────────────────────┐
 │                     文件                      │                       操作                       │
 ├───────────────────────────────────────────────┼──────────────────────────────────────────────────┤
 │ services/scheduler/src/db/rebate.rs           │ 新建 — 所有 SQL 查询                             │
 ├───────────────────────────────────────────────┼──────────────────────────────────────────────────┤
 │ services/scheduler/src/jobs/release_rebate.rs │ 新建 — job 逻辑                                  │
 ├───────────────────────────────────────────────┼──────────────────────────────────────────────────┤
 │ services/scheduler/src/db/mod.rs              │ 新增 pub mod rebate;                             │
 ├───────────────────────────────────────────────┼──────────────────────────────────────────────────┤
 │ services/scheduler/src/jobs/mod.rs            │ 新增 pub mod release_rebate;                     │
 ├───────────────────────────────────────────────┼──────────────────────────────────────────────────┤
 │ services/scheduler/src/jobs/rebate.rs         │ execute_release() 改调 release_rebate::execute() │
 └───────────────────────────────────────────────┴──────────────────────────────────────────────────┘

 可复用的现有函数：
 - db::tenant::get_all_tenant_ids(&pool) — services/scheduler/src/db/tenant.rs:273
 - ctx.tenant_pool(tenant_id) — services/scheduler/src/main.rs:104
 - ctx.cache.get_string(key) — Redis 缓存读取
