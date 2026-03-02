using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class CopyTrade
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int RuleNumber { get; set; }

    public int Value { get; set; }

    public long SourceAccountId { get; set; }

    public long SourceAccountNumber { get; set; }

    public long TargetAccountId { get; set; }

    public long TargetAccountNumber { get; set; }

    public string Mode { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Party Party { get; set; } = null!;

    public virtual TradeAccount SourceAccount { get; set; } = null!;

    public virtual TradeAccount TargetAccount { get; set; } = null!;
}
