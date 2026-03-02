using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Rebate
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public long AccountId { get; set; }

    public int FundType { get; set; }

    public int CurrencyId { get; set; }

    public long Amount { get; set; }

    public long? TradeRebateId { get; set; }

    public DateTime? HoldUntilOn { get; set; }

    public string Information { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;

    public virtual Matter IdNavigation { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual TradeRebate? TradeRebate { get; set; }
}
