using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Ledger
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public long MatterId { get; set; }

    public int ChargeId { get; set; }

    public int CurrencyId { get; set; }

    public long ChargeAmount { get; set; }

    public short LedgerSide { get; set; }

    public long? InvoiceId { get; set; }

    public DateTime TalliedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Charge Charge { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Matter Matter { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}
