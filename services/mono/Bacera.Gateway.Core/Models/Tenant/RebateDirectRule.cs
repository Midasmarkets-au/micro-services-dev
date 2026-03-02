using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateDirectRule
{
    public long Id { get; set; }

    public long SourceTradeAccountId { get; set; }

    public long TargetAccountId { get; set; }

    public long RebateDirectSchemaId { get; set; }

    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    public long? ConfirmedBy { get; set; }

    public DateTime? ConfirmedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Party? ConfirmedByNavigation { get; set; }

    public virtual Party CreatedByNavigation { get; set; } = null!;

    public virtual RebateDirectSchema RebateDirectSchema { get; set; } = null!;

    public virtual TradeAccount SourceTradeAccount { get; set; } = null!;

    public virtual Account TargetAccount { get; set; } = null!;
}
