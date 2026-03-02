using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class WalletTransaction
{
    public long Id { get; set; }

    public long WalletId { get; set; }

    public long? InvoiceId { get; set; }

    public long MatterId { get; set; }

    public long PrevBalance { get; set; }

    public long Amount { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Matter Matter { get; set; } = null!;

    public virtual Wallet Wallet { get; set; } = null!;
}
