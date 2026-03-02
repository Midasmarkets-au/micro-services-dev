using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class SalesRebate
{
    public long Id { get; set; }

    public long TradeRebateId { get; set; }

    public long SalesAccountId { get; set; }

    /// <summary>
    /// 单位是0.01分
    /// </summary>
    public long Amount { get; set; }

    public short TradeAccountType { get; set; }

    public int TradeAccountFundType { get; set; }

    public int TradeAccountCurrencyId { get; set; }

    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string? Note { get; set; }

    public long TradeAccountId { get; set; }

    public long TradeAccountNumber { get; set; }

    public string RebateType { get; set; } = null!;

    public long RebateBase { get; set; }

    public long? WalletAdjustId { get; set; }

    public virtual Account SalesAccount { get; set; } = null!;

    public virtual Account TradeAccount { get; set; } = null!;

    public virtual TradeRebate TradeRebate { get; set; } = null!;

    public virtual WalletAdjust? WalletAdjust { get; set; }
}
