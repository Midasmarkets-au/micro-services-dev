using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateNew
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public int FundType { get; set; }

    public int CurrencyId { get; set; }

    public long Amount { get; set; }

    public long TradeRebateId { get; set; }

    public string Information { get; set; } = null!;

    public DateTime PostedOn { get; set; }

    public int StateId { get; set; }

    public DateTime StatedOn { get; set; }
}