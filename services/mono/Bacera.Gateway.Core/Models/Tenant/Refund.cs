using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Refund
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int TargetType { get; set; }
    public long TargetId { get; set; }

    public int CurrencyId { get; set; }
    public int FundType { get; set; }

    public long Amount { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Comment { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;
    public virtual Matter IdNavigation { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}