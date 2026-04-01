  Rebate Calculate 计算共识



  一、整体流程


  MT5 交易关闭
    → TradeMonitor 扫 mt5_deals（每秒）→ 发布 NATS BCR_TRADE
    → TradeHandler 消费 → 写入 trd."_TradeRebate_{year}"（Status=0 Created）
    → CalculateRebate Job（每2分钟）→ 读 Status IN (0,5) 的记录
    → 生成 Rebate → 写入 core."_Matter_{year}" + trd."_Rebate_{year}"
    → 更新 TradeRebate.Status = 2(Completed) 或 -2(HasNoRebate)


  ────────────────────────────────────────



  二、前置条件（任一不满足 → HasNoRebate）

  1. RebateEnabled = true（core."_Configuration"）
  2. TradeRebate.AccountId 不为 NULL
  3. _Account.Status = 0（Active）
  4. _RebateClientRule 存在，且 DistributionType 有效
  5. _RebateDirectSchemaItem 中存在该 symbol 的配置（Rate/Pip/Commission 至少一个 > 0）
  6. Agent 链存在（_Account.ReferPath 中的 ID 都在 _Account 表中）


  ────────────────────────────────────────



  三、Symbol 标准化（`get_trimmed_symbol`）


  ┌─────────────┬─────────────┐
  │ 原始 symbol │ 标准化结果  │
  ├─────────────┼─────────────┤
  │ XAUUSD.s    │ XAUUSD      │
  │ XAGUSDmin   │ XAGUSDMIN   │
  │ #CLxxx      │ #CL         │
  │ #BRNxxx     │ #BRN        │
  │ 其他        │ 去掉 . 后缀 │
  └─────────────┴─────────────┘


  ────────────────────────────────────────



  四、三种分配模式（`_RebateClientRule.DistributionType`）



  1. Direct（=1）

  • 读 _RebateDirectRule（SourceTradeAccountId = client），每条规则对应一个 TargetAccount
  • 每个 target 独立计算 base rebate，互不影响
  • 公式：Amount = trunc(BaseTotal × TargetExchangeRate) × 100



  2. Allocation（=2）

  • 读 _RebateClientRule.RebateDirectSchemaId → 找 schema item
  • 读 agent 链（_Account.ReferPath 解析，按深度排序，过滤有 _RebateAgentRule 的）
  • 从 client 往上 逐层分配，每层从剩余 remain 中取走自己的份额
    • 非最后层：rate_take = min(remain.rate, schema_rate × volume × source_rate)，pip/commission_take =
      min(remain.pip/comm, remain × |percentage| / 100)
    • 最后层（top agent）：拿走全部剩余 remain
  • 公式：Amount = trunc((rate_take + pip_take + comm_take) × TargetExchangeRate) × 100



  3. LevelPercentage（=3）

  • 读 _RebateClientRule.RebateDirectSchemaId → 计算总 base rebate 作为 remain_amount
  • 读 level2 agent 的 _RebateAgentRule.LevelSetting JSON：


    { "PercentageSetting": { "FOREX": [0.3, 0.5, ...], "GOLD": [...], "OTHER": [...] } }

  • 从 底层往上 逐层按百分比分配（percentages 倒序）：
    • amount = trunc(volume × percentage × sourceRate × targetRate)
    • amount_dec = min(amount, remain_amount)，remain_amount -= amount_dec
    • rebate_amount = amount_dec × 100
  • Top agent 拿走剩余：trunc(remain × targetRate) × 100


  ────────────────────────────────────────



  五、Base Rebate 计算（`calculate_base_rebate`）


  divided_volume = volume / 100   (TradeRebate.Volume 单位是手×100)
  source_rate    = ExchangeRate(trade_currency → USD)
  rate       = trunc(schema.Rate       × source_rate × 100 × divided_volume × 100)
  commission = trunc(schema.Commission × source_rate × 100 × divided_volume × 100)
  pip        = trunc(schema.Pips       × source_rate × 100 × divided_volume × pip_formula × 100)
  BaseTotal  = rate + pip + commission


  ────────────────────────────────────────



  六、Pip Formula（`get_pip_value`）


  ┌─────────────────────────────────┬─────────────────────────────────────────────┐
  │ Symbol 类型                     │ pip_formula                                 │
  ├─────────────────────────────────┼─────────────────────────────────────────────┤
  │ ends_with("USD")（如 XAUUSD）   │ contract / 10^digits                        │
  │ starts_with("USD")（如 USDJPY） │ contract / 10^digits / bid                  │
  │ #AUS200, #UK100, EUR 系指数     │ AUDUSD/GBPUSD/EURUSD.bid × prefix_pip_value │
  │ #HKG50                          │ round(1/USDHKD.bid, 5) × 10.0               │
  │ 其他 # 开头                     │ prefix_pip_value（固定值）                  │
  │ 6位货币对（如 EURJPY）          │ 取后3位组成 JPYUSD 或 USDJPY，查 bid        │
  └─────────────────────────────────┴─────────────────────────────────────────────┘


  ────────────────────────────────────────



  七、汇率转换（`get_mt_exchange_rate`）

  • 查 mt5_prices 表（实时价格）
  • 先尝试 FROM+TO（如 AUDUSD），取 BidLast
  • 再尝试 TO+FROM（如 USDAUD），取 1/BidLast
  • USC（分）特殊处理：USC→USD = 0.01，USD→USC = 100


  ────────────────────────────────────────



  八、金额存储规则

  • 单位：Amount = 计算结果 × 100（即分的1/100，保留2位小数精度）
  • 类型：NUMERIC（PostgreSQL），Decimal（Rust）
  • 舍入：全程使用 trunc()（向零截断，对应 C# MidpointRounding.ToZero）
  • `_Rebate.Id` = _Matter.Id（从 _Matter_{year} BIGSERIAL 获取，起始值 10000）


  ────────────────────────────────────────



  九、PendingResend（Status=5）处理

  已有 rebate 记录时（重算场景）：
  • 新账号有、旧没有 → 直接插入
  • 旧账号有、新没有 → 插入负数补偿记录
  • 两边都有但金额不同 → 插入差值（new - old）
  • 金额相同 → 跳过
