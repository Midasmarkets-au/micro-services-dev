using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Deposit
{
    public long Id { get; set; }

    public int Type { get; set; }

    public long PartyId { get; set; }

    public long PaymentId { get; set; }

    public int FundType { get; set; }

    public int CurrencyId { get; set; }

    public long Amount { get; set; }

    public long? TargetAccountId { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;

    public virtual Matter IdNavigation { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;

    public virtual Account? TargetAccount { get; set; }
}