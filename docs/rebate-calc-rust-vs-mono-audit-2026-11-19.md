# Rebate 计算：scheduler (Rust) vs C# 网关差异审计

日期：2026-11-19（2026-11-20 基准改为 MM-Back）
范围：`services/scheduler/src/jobs/rebate_calc.rs` ↔ `Bacera.Gateway.Infrastructure/Services/RebateService.cs`

## 基准说明

对照基准为 **MM-Back**（`~/code/edge-ark/MM-Back`，业务逻辑的活跃维护分支），而非本仓库的 `services/mono`。

两者在返佣**计算路径**上的代码几乎完全相同——逐行比对 `RebateService.cs`（1076 行 vs 1074 行）只差 2 行，`CurrencyTypes.cs`、`AgentRebateRule.Ext.cs`、`Account.Ext.cs`、`CurrencyConversionHelper.cs`、`RebateDirectSchemaItem.cs`、`RebateClientRule.cs`、`Symbol.cs` 以及 `DTO/Rebate/*` 全部字节一致。所以本报告原先基于 mono 得出的结论**全部原样成立**，改换基准只新增了一条发现（见 P1-0）。

两个分支是平行演进的，各有对方没有的东西：

| | MM-Back | Micro-Service/mono |
|---|---|---|
| 业务逻辑修复 | 较新（如 Symbol.Type 过滤） | 缺 |
| k8s 分区表支持（`RebateK8s` / `TradeRebateK8s`） | 无 | 有 |
| Hangfire 返佣定时任务 | — | 已停用 |

整体 `diff -rq` 有 968 个文件不同，但差异集中在控制器、报表和 k8s 表迁移；返佣计算链路之外的部分不在本报告范围内。

---

## 0. 结论摘要

Rust 移植整体忠实复刻了 mono 的算术结构（截断方向、×100 缩放、remain 扣减顺序、Information JSON 版本号都对得上），但存在 **1 个会让全部汇率失效的 P0 缺陷**，外加 1 个汇率兜底缺失和 4 个会改变金额或分配对象的 P1 差异。

| 级别 | 问题 | 影响 |
|---|---|---|
| P0 | 货币 ID 用了错误的编码体系 | 所有汇率恒为 1.0 |
| P0 | 缺少 ExchangeRate 表兜底 | MT 无报价时静默按 1.0 处理且无日志 |
| P0 | 重算时未合计同一账户的历史返佣行 | 第二次重算起会重复发放 |
| P1 | symbol 分类查询缺 `Type = 300` 过滤 | 可能读到白名单行，分类全错 |
| P1 | Allocation 选 schema 时忽略 AccountType | 取错 rate / percentage |
| P1 | `is_top_level_agent` 少了 Role 判断 | 代理链起点可能不同 |
| P1 | Allocation / LevelPercentage 的 FundType 取错对象 | 仅影响报表标签，不影响资金流向 |
| P1 | `#GER40`、`Bitcoin`、`Ethereum` 的 pip 值两边不同 | 这些品种返佣金额不同 |
| P2 | 6 项行为细节差异 | 见第 4 节 |

**实施状态（2026-11-20，未提交）**：

| 问题 | 状态 |
|---|---|
| P0-1 货币编码 | 已修复（新增 `models/currency.rs`，全量移植 mono 的 `CurrencyTypes`） |
| P0-2 ExchangeRate 兜底 | 已修复（`get_exchange_rate_fallback`） |
| P0-3 重算未合计历史行 | 已修复（`send_rebates` 改为累加），见第 9 节 |
| P1-0 `Type = 300` 过滤 | 已修复 |
| P1-3 FundType | 已修复（对齐 C#：Allocation/LevelPercentage 用客户账户，Direct 用代理） |
| P1-4 pip 值字典 | 已决定保持 Rust 现状，不改代码，依据见 P1-4 节 |
| P1-1 AccountType 选 schema | 待处理 |
| P1-2 `is_top_level_agent` 的 Role 判断 | 待处理 |
| P2 第 5 条静默兜底 | 已修复（`rate_to_decimal`） |
| P2 其余 | 待处理 |

P0 修复已在 k8s staging 用真实数据验证通过，见第 8 节。

关于运行关系：mono 的 Hangfire `Calculate Rebate` 定时任务已被注释停用（`Startup.Hangfire.cs:111-115`）。但 2026-11-20 排查发现，Rust scheduler 此前**也并未部署在现存的唯一集群上**——原 `mm` 命名空间所在的 `charming-synth-dolphin` 集群已被删除，ECR 里 `scheduler:latest` 的最后推送时间停在 2026-07-16，与测试库里返佣数据的终止日期一致。因此 P0 缺陷的"线上生效"范围需要按各环境实际部署情况单独核实，不能一概而论。

---

## 1. 两套实现的运行关系

两者并不是读写同一批表：

| | mono `RebateService` | scheduler `rebate_calc` |
|---|---|---|
| 读 | `trd."_TradeRebate"` | `trd.trade_rebate_k8s` |
| 写 | `trd."_Rebate"` + Matter | `trd.rebate_k8s` + `core.matter_k8s` |
| 触发 | Hangfire（已停用）、gRPC `TriggerCalculateRebate`、Tenant 后台预览 | cron `*/2 * * * *` |
| 锁 | `calculate_rebate_lock_key_tid:{id}` | `scheduler_calculate_rebate_lock_tid:{id}` |

所以两边不会互相覆盖，但 mono 的 `TradeRebateCheckForTenantAsync`（租户后台「试算」接口）仍然走 C# 逻辑。**租户在后台看到的试算金额和 scheduler 实际发放的金额会对不上**，这是目前最容易被业务方发现的外部症状。

金额单位：两边一致，最终 Amount 的单位是 **1e-6 USD（微美元）**。推导：`Rate(USD/lot) × 100 × lots × 100` 得到 1e-4 USD，末尾再 `× 100` 得到 1e-6。与 `report/daily_equity.rs` 里的 `/ 1000000.0` 吻合。

---

## 2. P0 问题

### P0-1 货币 ID 用了错误的编码体系 → 所有汇率恒为 1.0

mono 的 `CurrencyTypes` 是 **ISO-4217 数字代码**：

```129:130:services/mono/Bacera.Gateway.Core/Types/CurrencyTypes.cs
    USD = 840,
    USC = 841, // US Cent
```

（另有 `AUD = 036`、`HKD = 344`、`JPY = 392`、`EUR = 978`。）

数据库里 `trd."_Account"."CurrencyId"` 和 `trd.trade_rebate_k8s.currency_id` 存的就是这个值——`_TradeRebate.CurrencyId` 上有指向 `_Currency.Id` 的外键，且 C# 全程直接 `(CurrencyTypes)tradeRebate.CurrencyId` 强转。Rust 侧 `currency_id` 的来源是 `find_account_by_number` 直接 SELECT 的 `"CurrencyId"`（`db/tenant.rs:318`），没有任何重映射。

但 `rebate_calc.rs` 自己定义了一套从 1 开始的序号：

```184:205:services/scheduler/src/jobs/rebate_calc.rs
        // CurrencyTypes: USD=1, USC=2
        const USD: i32 = 1;
        const USC: i32 = 2;

        if from_currency_id == USC && to_currency_id == USD {
            return Ok(0.01);
        }
        // ...
        let from_name = currency_id_to_name(from_currency_id);
        let to_name = currency_id_to_name(to_currency_id);
```

`currency_id_to_name` 同样按 1..20 映射，且对未知值兜底返回 `"USD"`：

```1081:1103:services/scheduler/src/jobs/rebate_calc.rs
pub(crate) fn currency_id_to_name(currency_id: i32) -> &'static str {
    match currency_id {
        1 => "USD",
        2 => "USC", // cents
        3 => "EUR",
        // ...
        _ => "USD", // fallback
    }
}
```

`const USD: i32 = 1` 在 `generate_direct_rebates`、`generate_allocation_rebates`、`generate_level_percentage_rebates`、`calculate_base_rebate` 共 4 处重复定义，全部是 1。

**执行链路推演**（以 USC 账户 `currency_id = 841` 求 source rate 为例）：

1. `from(841) == to(1)`？否，短路不触发
2. `from == USC(2) && to == USD(1)`？否——841 ≠ 2，USC 特判全部不触发
3. `currency_id_to_name(841)` → 落 `_` 分支 → `"USD"`；`currency_id_to_name(1)` → `"USD"`
4. 查 MT 报价 `"USDUSD"` → 不存在
5. 反向查 `"USDUSD"` → 不存在
6. 走到函数末尾 `Ok(1.0)`

**结论：`get_mt_exchange_rate` 在任何输入下都返回 1.0。**

这是 `rebate_calc.rs` 的孤立错误，不是全局约定不同——同一个 crate 里其他模块用的是正确编码：

- `jobs/sales_rebate.rs:17` → `const USC_CURRENCY_ID: i32 = 841;`
- `report/daily_equity.rs` → `"CurrencyId" = 840`、`= 841`
- `report/deposit.rs:445` → `COALESCE(sa."CurrencyId", 840)`

（`jobs/event_trade_handler.rs:21` 的 `CURRENCY_USC = 16` 是第三套取值，属于活动模块的独立问题，不在本次范围内，但建议顺手核实。）

#### 影响分级

source rate 和 target rate 都塌成 1.0，**当交易账户与收款账户币种相同时两者会互相抵消**，这正是该缺陷一直没被发现的原因。只要跨币种就会出现实打实的金额偏差：

| 场景 | C# source | C# target | Rust | Rust 相对 C# |
|---|---|---|---|---|
| USD 交易账户 → USD 代理 | 1 | 1 | 1 / 1 | 一致 |
| USC 交易账户 → USC 代理 | 0.01 | 100 | 1 / 1 | 基本抵消，仅截断位置不同 |
| EUR 交易账户 → EUR 代理 | ~1.08 | ~0.926 | 1 / 1 | 基本抵消 |
| **USC 交易账户 → USD 代理** | 0.01 | 1 | 1 / 1 | **放大 100 倍** |
| **USD 交易账户 → USC 代理** | 1 | 100 | 1 / 1 | **缩小 100 倍** |
| **EUR 交易账户 → USD 代理** | ~1.08 | 1 | 1 / 1 | **少约 7.4%** |
| **USD 交易账户 → JPY 代理** | 1 | ~157 | 1 / 1 | **缩小约 157 倍** |

Allocation 模式还有一层额外影响。`calculate_allocation_rebate` 里 source rate 被用来缩放 schema rate 和 pip/commission 的提取比例：

```403:414:services/scheduler/src/jobs/rebate_calc.rs
    let rate = dec_round_to_zero(schema_rate * ser * dec(volume as i64) * dec(100));
    let schema_rate_value = remain.rate.min(rate);
    remain.rate -= schema_rate_value;
    // ...
        let pip_value = dec_round_to_zero(remain.pip * ser * pct_abs / dec(100) * dec(100));
```

这个 `ser` 是非对称使用的（只影响 remain 的扣减速度，不在末尾被 target rate 还原）。所以即便总额碰巧抵消，**代理链上各层之间的分配比例也会和 C# 不同**。

顺带一提：`remain.pip * ser` 这个写法在 mono 原始实现里就很可疑——remain 已经是 USD 口径，再乘一次汇率在量纲上说不通。Rust 忠实复刻了它。修完 P0 之后这个二次缩放会第一次真正生效，**需要同时决定要不要连带修正 mono 的这处逻辑**，否则修 P0 反而会引入新的金额跳变。

### P0-2 缺少 ExchangeRate 表兜底

C# 在 MT 报价查不到时有两级回退，最后才打 warning 返回 1：

```725:741:services/mono/Bacera.Gateway.Infrastructure/Services/RebateService.cs
        // Fallback: tenant ExchangeRate table when MT prices are unavailable
        var rate = await ctx.ExchangeRates
            .AsNoTracking()
            .Where(x => x.FromCurrencyId == (int)from && x.ToCurrencyId == (int)to)
            .Select(x => x.BuyingRate)
            .FirstOrDefaultAsync();
        if (rate > 0) return (double)rate;
        // ... 反向查 SellingRate ...
        logger.LogWarning("No exchange rate found for {From} -> {To}, defaulting to 1", from, to);
        return 1;
```

Rust 版本 `get_mt_exchange_rate_inner` 直接 `Ok(1.0)` 收尾，既没有查 `trd."_ExchangeRate"`，也没有任何日志。修 P0-1 之后这条路径才会被真正走到（届时周末/休市 MT 无报价会变成常态），所以这两个必须一起修，否则只是把"恒定 1.0"换成"静默 1.0"。

---

## 3. P1 问题

### P1-0 symbol 分类查询缺 `Type = 300` 过滤（改换基准后新增）

`trd."_Symbol"` 用 `Type` 列区分两份清单，对应 mono 的 `RebateSymbolListTypes`：

```7:14:MM-Back/src/Bacera.Gateway.Core/Types/RebateSymbolListTypes.cs
public enum RebateSymbolListTypes
{
    /// <summary>Rebate category list used for symbol → category lookup e.g.生成推荐码的时候看到的分类</summary>
    Categories = 300,

    /// <summary>Schema symbol whitelist (symbols allowed in rebate schemas).</summary>
    SchemaWhitelist = 400,
}
```

同一个 `Code` 会同时出现在两份清单里，而两行的 `Category` / `CategoryId` 并不相同。MM-Back 在 2026-07-20 的 commit `3cdd4d7`（"fix: filtering the symbol category data only with Symbol.Type == 300."）给两处分类查询都加上了过滤。本仓库的 mono 没有这个修复，Rust 也没有——两处查询都是裸的 `WHERE "Code" = $1 LIMIT 1`，既不过滤 `Type` 也不排序，读到哪一行取决于物理存储顺序。

影响按模式区分：

- **Allocation**：`symbol_category_id` 取错 → `get_schema_item_and_percentage` 找不到匹配的 `cid` → 每一层代理都 `continue` → **整笔不产生任何返佣**。若错误的 id 恰好命中 schema 里另一个 `cid`，则按错误费率计算。
- **LevelPercentage**：`Category` 字符串取错 → FOREX / GOLD / OTHER 归类错误 → 套用错误的百分比阶梯。

这条的实际触发概率取决于 `_Symbol` 里是否真的存在跨 Type 的重复 `Code`。MM-Back 专门为此发过修复，说明生产数据里确实存在。

### P1-1 Allocation 选 schema 时忽略 AccountType

C# 先按下单客户的账户类型定位 schema，再在该 schema 内找品类：

```12:19:services/mono/Bacera.Gateway.Core/Models/Tenant/_Extensions/AgentRebateRule.Ext.cs
    public (RebateLevelSchemaItem?, decimal) GetSchemaItemAndPercentage(AccountTypes accountType, long symbolCategoryId)
    {
        var schema = GetSchema();
        var accountTypeSchema = schema.FirstOrDefault(x => x.AccountType == accountType);
        if (accountTypeSchema == null) return (null, 0);

        var schemaItem = accountTypeSchema.Items.FirstOrDefault(x => x.CategoryId == symbolCategoryId);
        return schemaItem == null ? (null, 0) : (schemaItem, accountTypeSchema.Percentage);
```

调用处传的是 `(AccountTypes)tradeRebate.Account.Type`（`RebateService.cs:374`）。

Rust 的 `get_schema_item_and_percentage` 完全不看 `AccountType`，遍历所有 schema 返回第一个 cid 命中的项，连带 percentage 也取自那个 schema。代理若为多种账户类型（标准户/ECN 等）配置了不同费率，就会取错。

根因之一是 `TradeRebateRow` 结构体里根本没有 account type 字段，`db/rebate_calc.rs:97` 的 SELECT 也没查。修复需要同时扩展查询、模型和函数签名。另外 C# 里 `AccountType` 来自**下单客户**账户，不是代理账户，容易写反。

### P1-2 `is_top_level_agent` 少了 Role 判断

```24:25:services/mono/Bacera.Gateway.Core/Models/Tenant/_Extensions/Account.Ext.cs
    public bool IsTopLevelAgent() =>
        Role is (short)AccountRoleTypes.Agent && (AgentAccountId == Id || AgentAccountId == null);
```

```73:78:services/scheduler/src/models/rebate.rs
    pub fn is_top_level_agent(&self) -> bool {
        match self.agent_account_id {
            None => true,
            Some(aid) => aid == self.id,
        }
    }
```

Rust 丢掉了 `Role == Agent` 这个条件，`AgentAccount` 结构体和 `get_sorted_agent_accounts` 的 SELECT 里也没有 `Role` 字段。

这个判断用于 `TakeWhile(!IsTopLevelAgent).Count()` 算出要跳过的前缀长度。Rust 若过早判定某账户为 top-level，`skip_count` 会偏小，**代理链会多带上前面若干个账户**，直接改变返佣发放对象。实际影响取决于数据：需要查一下 refer path 上是否存在 `Role != Agent` 但 `AgentAccountId IS NULL` 且挂了 `_RebateAgentRule` 的账户。

### P1-3 Allocation / LevelPercentage 的 FundType 取错对象

C# 在这两个模式里用的是**下单客户**账户的 FundType：

```225:226:services/mono/Bacera.Gateway.Infrastructure/Services/RebateService.cs
                    CurrencyId = account.CurrencyId,
                    FundType = tradeRebate.Account.FundType,
```

Rust 用的是收款代理账户的 `agent.fund_type`。Direct 模式两边都用 target account，一致。

**已查明：这个字段不影响资金流向。** 派发环节两边都不读 `Rebate.FundType`，而是从收款账户自己的 `FundType` 重新定位钱包：

- C# `AcctService.Rebate.cs:53` — `x.CurrencyId == receiverAccount.CurrencyId && x.FundType == receiverAccount.FundType`
- Rust `db/rebate.rs:183` — 同样 bind 收款账户的 `fund_type`

所以 `Rebate.FundType` 只是返佣记录上的冗余标签，差异仅影响按 FundType 分组的报表。原先担心的"资金流向错误"不成立。

**处置（已实施）**：严格对齐 C#——Allocation 和 LevelPercentage 改用下单客户账户的 FundType，Direct 维持收款代理的。通过在 `get_trade_rebate` 里 LEFT JOIN `trd."_Account"` 取得，对应 mono 的 `.Include(x => x.Account)` 预加载。`run_generate_guards` 已要求账户存在且处于激活状态，因此 JOIN 必然命中；`client_fund_type` 在落空时直接报错而非编造默认值。

测试库实测：81 条返佣记录的客户与代理 FundType 全部相同（均为 2），且无孤儿记录，该改动在现有数据上是无操作。

### P1-4 pip 值字典的两处不一致

**`#GER40`**：Rust 的 `PREFIX_PIP_VALUES` 有 `"#GER40" → 1.0`，C# 的 `PrefixPipValues` 字典里没有这个 key（已全库确认，`RebateService.cs` 里唯一出现 `#GER40` 的地方是 638 行的索引列表）。两边都把 `#GER40` 归进了走 EURUSD 的那组索引，于是：

- C#：`price.Bid * GetPrefixPipValue("#GER40")` = `bid × 0` = **0**，该品种 pip 返佣恒为零
- Rust：`bid × 1.0`

**长度 ≠ 6 的品种**：C# 直接截断返回，后面那段 `PrefixPipValues` 检查是**死代码**：

```674:694:services/mono/Bacera.Gateway.Infrastructure/Services/RebateService.cs
        if (symbolCode.Length != 6)
            return (0d, 0d);
        // ...
        if (PrefixPipValues.ContainsKey(symbolCode))   // 永远到不了
        {
            return (GetPrefixPipValue(symbolCode), 0d);
        }
```

Rust 在这里回落到了 `get_prefix_pip_value`，所以 `Bitcoin`（0.01）和 `Ethereum`（0.05）在 Rust 下有 pip 值，C# 下是 0。

这两条本质上是 Rust 顺手"修好"了 mono 的疏漏。

**处置（已决定）：保持 Rust 现状，不对齐 C#。** 依据：

1. 三个品种在测试库 `trd."_Symbol"` 里都配了 `Type=300`（分类）和 `Type=400`（白名单）两条记录，业务上是刻意启用的，不是遗留配置。
2. `Bitcoin` 在 `trd.trade_rebate_k8s` 里有真实成交（10 手），不是纸面问题。
3. `#GER40` 的缺失是明显的漏配而非有意为零——它被明确写进了 638 行走 EURUSD 的索引清单，而前身 `#GER30` 在两个字典里都有且值为 1（DAX 2021 年从 30 只成分股扩至 40 只）。
4. C# 里 `Bitcoin` / `Ethereum` 的字典项（0.01 / 0.05）和 `ContractSize` 项（1 / 5）都写了确定的数值，作者本意是启用，只是被 `Length != 6` 提前拦截。

**副产品**：`#GER40` 在 `Type=300` 下 `Category='Commodities'`，在 `Type=400` 下却是 `'Index'`——这正是 P1-0 的 `Type = 300` 过滤所要消除的歧义在真实数据上的实例。

---

## 4. P2 问题（行为差异，影响较小）

1. **schema item 过滤条件不一致**。Rust 的 `get_direct_schema_item` 对三种模式统一加了 `("Rate" > 0 OR "Pips" > 0 OR "Commission" > 0)`；C# 只在 Direct 模式加。实测两边最终都产出空结果（C# 会走完循环但每项 total 为 0），落到同一个 `HasNoRebate` 状态，影响可忽略。

2. **零额记录的处理**。C# 在 Allocation 非末位代理和 LevelPercentage 顶级代理处，即使算出 `Amount == 0` 仍会插入 Rebate + Matter 行；Rust 会 `continue` / 跳过。Rust 行为更干净，但对账时记录条数会对不上。

3. **状态守卫**。C# `SendRebates` 会重新读取 TradeRebate 并要求 status ∈ {Created, PendingResend}，还会复查账户是否 active；Rust 依赖分页查询时的 `status = 0 OR status = 5` 预筛，之后不再复查。存在一个很窄的竞态窗口。

4. **Direct 模式的 serviceId**。C# base rebate 用 `tradeRebate.Account.ServiceId`，Rust 用 `trade_rebate.trade_service_id`。正常情况两者相同，账户跨 trade service 迁移时会分叉。

5. **报价缺失时的行为**。C# `GetMtPrice` 查不到时返回 `Bid=0, Digit=0`，后续 `1/10^0/0*contract*0` 得到 `NaN`（`EndsWith("USD")` 分支）或 `+∞`（`StartsWith("USD")` 分支），随后 `(decimal)double.NaN` 抛 `OverflowException`，整个 job 崩。Rust 返回 `(0.0, 0.0)`，pip 记 0 继续。Rust 更稳健但结果不同。另外 Rust 的 `Decimal::from_f64(...).unwrap_or(Decimal::ONE)` 会把 NaN/∞ 静默变成 1.0，这是一个会掩盖故障的兜底，建议改成显式报错。

6. **浮点细节**。`EndsWith("USD")` 分支 Rust 做了代数化简（`contract / 10^digits`，约掉了 bid），与 C# 的 `1/10^d/bid*contract*bid` 数学等价，浮点相对误差约 1e-16，截断后基本无差异。`#HKG50` 分支 Rust 用 `(x*1e5).round()/1e5` 模拟 `Math.Round(1/bid, 5)`，但 C# 默认是银行家舍入（ToEven）而 Rust 的 `f64::round` 是四舍五入远离零，仅在第 6 位恰好为 5 时分叉。两条都可忽略。

7. **`get_distribution_type` 无 ORDER BY**。`LIMIT 1` 不带排序，同一账户存在多条 `_RebateClientRule` 时取值不确定；C# 是按 DistributionType 过滤后 `SingleOrDefault`。`_ClientRebateRule_pk2` 是 `ClientAccountId` 上的唯一索引，所以实际不会触发。

8. **Information JSON 的 depth**。C# 的 `depth++` 只在实际产出返佣时递增，Rust 直接用循环下标 `i + 1`。仅影响元数据，不影响金额。

---

## 5. 已核对一致的部分

以下逐行比对后确认无差异，修复时不要误动：

- `get_trimmed_symbol` ↔ `GetTrimmedSymbol`（`.` 截断、`#CL` / `#BRN` 前缀归一、`XAGUSDmin` 大写）
- `get_symbol_category_name` ↔ LevelPercentage 的 FOREX/GOLD/OTHER 归类
- `ContractSize` 字典（除 Rust 多一个 `#GER40`，而该 key 对 `#` 开头品种不参与计算）
- `calculate_base_rebate_pure` 的三项公式与 `MidpointRounding.ToZero` 截断语义
- `calculate_last_allocation_rebate` ↔ `CalculateLastAllocationRebate`
- LevelPercentage 的 `setting.Reverse()` + 队列消费 + `queue.Count > 1` 循环条件
- `send_rebates_with_db` 的 PendingResend 差额补偿逻辑（新增全额、变更补差额、消失则冲正）
- `get_sorted_agent_accounts` 的 ReferPath 解析与 `ORDER BY Level`（除 P1-2 的 Role 判断）
- Matter 状态固定为 `RebateOnHold`、`hold_until_on = NOW()`

---

## 6. 建议的修复顺序

**第 1 步（P0，必须一起上）—— 已完成**

1. ~~把 `rebate_calc.rs` 里 4 处 `const USD: i32 = 1` 改为 840，`USC` 改为 841。~~
   已改为统一从新模块 `models/currency.rs` 导入 `USD` / `USC`，`sales_rebate.rs` 原先自带的 `USC_CURRENCY_ID` 也改为复用同一份定义，消除再次漂移的可能。
2. ~~`currency_id_to_name` 换成 ISO-4217 映射，去掉 `_ => "USD"` 兜底。~~
   已删除该函数，替换为 `currency::id_to_name() -> Option<&'static str>`，完整移植 mono `CurrencyTypes` 的全部约 170 个条目（对齐 `Enum.GetName` 的行为，避免冷门币种从"回落查表"退化成硬报错）。未知 id 现在返回 `None`，调用方转成 `anyhow!("unknown currency id {}")` 错误。
3. ~~补上 `trd."_ExchangeRate"` 两级兜底。~~
   已在 `db/rebate_calc.rs` 新增 `get_exchange_rate_fallback`（正向 `BuyingRate` → 反向 `SellingRate` 取倒数，只认正值），兜底未命中时按 mono 的做法打 warning 再返回 1。
4. **未做，仍需决策**：Allocation 里 `remain.pip * ser` 的二次缩放保持原样（继续与 mono 一致）。它在修复前因 `ser` 恒为 1.0 而处于休眠状态，修复后会首次真正生效。

附带修掉的 P2 第 5 条：所有 `Decimal::from_f64(rate).unwrap_or(Decimal::ONE)` 替换为新的 `rate_to_decimal()`，NaN / 无穷 / 超范围一律报错。`calculate_base_rebate_pure` 和 `calculate_allocation_rebate` 的汇率参数类型随之从 `f64` 收紧为 `Decimal`，让"不会静默变成 1:1"成为类型层面的保证而不是注释约定。

遗留未动（归入 P1/P2 批次）：`Decimal::from_f64(pip_formula).unwrap_or(Decimal::ZERO)` 仍是静默兜底，报价 bid 为 0 时 pip 会悄悄算成 0。

新增测试：`models::currency` 3 个（含一条专门断言旧序号 1/2 不再解析的回归测试）、`rebate_calc` 3 个（`rate_to_decimal` 正常值 / 非有限值，以及常量为 840/841 的回归断言）。`cargo test -p scheduler` 59 passed，`cargo clippy -p scheduler --all-targets` 无告警。

**第 1.5 步（P1-0）—— 已完成**

`db/rebate_calc.rs` 的 `get_symbol_category` 和 `get_symbol_category_id` 均补上 `AND "Type" = $2`，绑定新常量 `models::rebate::SYMBOL_TYPE_CATEGORIES = 300`。这条是纯对齐 MM-Back，不需要业务决策。

**第 2 步（P1 其余项，逐条确认后再改）**

P1-1 和 P1-2 需要先扩展 SQL 查询和模型结构（补 account type、Role 字段）。P1-3 已按"对齐 C#"实施，P1-4 已决定保持 Rust 现状。

**第 3 步** P2 按需处理，其中第 5 条（`unwrap_or(Decimal::ONE)` 掩盖 NaN）建议随 P0 一起改掉。

### 验证计划

- `rebate_calc.rs` 现有的单测覆盖的是纯函数层，`currency_id_to_name` 那几个断言（1504-1514 行）本身就是在固化错误行为，需要一并改写。
- 建议补一个对拍脚本：挑一批历史 `trade_rebate` 记录，分别跑 C# `GenerateRebatesByTradeRebateId`（`shouldSend = false`，只试算不落库）和 Rust 的计算核心，逐笔比对 Amount。重点覆盖 USC 账户、非 USD 代理、多层代理链、`#GER40` / `Bitcoin` 这几类。
- 上线前需要评估**存量数据**：P0 存在期间跨币种发放的返佣金额是错的，要确认影响的租户范围和笔数，并决定是否需要冲正。`trd.rebate_k8s.information` 里记录了当时用的 `exchangeRate`（全是 1.0），可以作为筛选依据。

---

## 7. 待确认的开放问题

1. ~~`#GER40` / `Bitcoin` / `Ethereum` 的 pip 返佣业务上是否应该发放？~~ → 已决定保持 Rust 现状（发放），见 P1-4。
2. ~~Allocation / LevelPercentage 的 FundType 应取客户账户还是代理账户？~~ → 已决定对齐 C#（取客户账户），见 P1-3。
3. Allocation 里 `remain.pip × sourceExchangeRate` 的二次缩放是设计还是 mono 的历史 bug？P0 修复前 `ser` 恒为 1.0，这段逻辑处于休眠状态；修复后已被激活——第 8 节 JPY 那笔的三级代理净额呈非等比分布即由此而来。
4. mono 的 `RebateService` 是否还需要维持和 scheduler 的一致性？租户后台试算接口仍在走它，且读的是旧表 `trd."_TradeRebate"`，存在"试算与实发不一致"的问题。
5. `jobs/event_trade_handler.rs:21` 的 `CURRENCY_USC = 16` 是第三套取值，是否也是同类缺陷？

---

## 8. k8s staging 验证记录（2026-11-20）

### 环境

原 `mm` 命名空间所在集群已不存在，在现存集群 `charming-alternative-potato`（ap-southeast-2，节点全 arm64）上重建：

| 组件 | 说明 |
|---|---|
| 命名空间 | `mm` |
| `scheduler-env` Secret | 由本地 `.env.scheduler` 生成，追加 `NATS_URL` / `IDGEN_GRPC_URL` / `MONO_GRPC_URL` |
| redis / nats / idgen | 按 `deployment/k8s/mm/` 清单部署；`AppContext::new` 对 Postgres、Redis、NATS 是启动硬依赖，idgen 在写入时才调用 |
| scheduler | 镜像 `scheduler:rebate-fix-20261119` |

数据库 `mm-postgre-db...ap-southeast-1` 与 MT MySQL `13.212.107.58:3306` 均在新加坡，已用集群内 Pod 实测可达。租户仅 `10000 → portal_tenant_bvi`，124 条 `trade_rebate_k8s`、53 条 `rebate_k8s`，数据止于 2026-07-16。

### 方法

把已完成（status=2）的记录置为 `PendingResend`（status=5）触发重算。该路径只写入新算金额与既有返佣的**差额**，差额行本身即为 before/after 对照。验证前已用 `\copy` 导出两张表作为基线。

### 结果

| 交易币种 | 笔数 | 修复前合计 | 修复后 | 变化 |
|---|---|---|---|---|
| USC (841) | 1 | 270,000 | 2,700 | 精确 ×0.01 |
| JPY (392) | 1 | 270,000 | 1,600 | ≈ ×0.0059 |
| USD (840) | 43 | 129,063,200 | 129,009,900 | −0.04%，与修复无关 |

最干净的证据是 USC 那笔：新记录的 `information` 里 `rate` 从 2200 变 22、`pip` 从 500 变 5、金额从 270000 变 2700，三者精确除以 100，即 USC→USD 应有的 0.01 汇率——此前被压成了 1.0。全部 53 条历史返佣的 `exchangeRate` 均为 1.0，是 P0 缺陷的实证。

USD 那 43 笔出现的 −0.04% 差异来自 MT 实时报价变动（XAUUSD 4721.31 → 3532.63，EURGBP 1.3422 → 1.34547），是 PendingResend 重算机制的固有性质，与本次改动无关。代码层面亦可证明：同币种走 `from_currency_id == to_currency_id` 短路分支，该分支不依赖任何货币常量，改动前后均返回 1.0。

### 遗留

验证产生的 28 条冲正记录保留在测试库中。scheduler 已缩至 0 副本，redis / nats / idgen 保留。基线 CSV 在 `/tmp/rebate-verify/`。

---

## 9. P0-3 重算时未合计同一账户的历史返佣行（2026-11-20 新发现）

本次事件化改造的 staging 验证中，对同一笔 `trade_rebate` 做第三次重算时发现金额被重复发放。该缺陷与事件化改造无关，是 PendingResend 路径一直存在的问题。

### 现象

交易 `7480885066181115904`（账户 10018）在测试库中的返佣记录：

| 时间 | 金额 | 累计 | 说明 |
|---|---|---|---|
| 2026-07-10 | 187,100 | 187,100 | 原始发放 |
| 2026-11-20 05:00 | +100 | 187,200 | 第 8 节 P0 验证写入的差额，正确 |
| 2026-11-20 06:06 | +187,100 | **374,300** | 第三次重算，**错误** |

第三次重算的应得金额仍是 187,200，差额应为 0，却又发了一遍 187,100。

### 根因

`send_rebates` 把 `get_existing_rebates` 返回的逐行记录收进 `HashMap`：

```rust
let existing_map: HashMap<i64, Decimal> = existing.into_iter().collect();
```

`get_existing_rebates`（`db/rebate_calc.rs`）执行的是
`SELECT account_id, amount FROM rebate_k8s WHERE trade_rebate_id = $1`，
不带 `GROUP BY`。一个账户每次重算都会新增一行（原始行 + 每次重算的差额行），
因此同一 `account_id` 可能对应多行；`collect()` 在键冲突时只保留最后一行。

上例中 `existing_map[10018]` 取到的是 100 而不是 187,100 + 100 = 187,200，
于是差额算成 187,200 − 100 = 187,100。

**规律**：首次重算总是正确（只有一行历史记录），从第二次重算开始必然重复发放。

### 修复

改为按账户累加，并补了两个单元测试（等额重算写 0 行、超额重算只补差额）：

```rust
let mut existing_map: HashMap<i64, Decimal> = HashMap::new();
for (account_id, amount) in existing {
    *existing_map.entry(account_id).or_insert(Decimal::ZERO) += amount;
}
```

### 验证

修复后对同一笔数据重算，写入 **−187,100** 的冲正行，合计回到正确的 187,200，
状态置为 2；再触发一次重算，行数与合计均不变（4 行 / 187,200），幂等成立。
即修复本身同时完成了这条错误数据的订正。

### 对存量修复计划的影响

P0-1 的历史数据订正需要对大量 `trade_rebate` 触发重算。若其中任何一笔此前已被
重算过（已有两行以上历史记录），在修好本缺陷之前重跑都会多发一笔。
**存量订正必须在本修复上线之后进行。**

生产库是否已存在受影响数据，可用以下查询确认：

```sql
SELECT trade_rebate_id, account_id, count(*), sum(amount)
FROM trd.rebate_k8s
GROUP BY trade_rebate_id, account_id
HAVING count(*) > 2;
```
