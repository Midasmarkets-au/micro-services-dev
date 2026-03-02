using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Invoice
{
    public long Id { get; set; }

    public short LedgerSide { get; set; }

    public long Recipient { get; set; }

    public long Sender { get; set; }

    public int CurrencyId { get; set; }

    public long Amount { get; set; }

    public long DueBalance { get; set; }

    public DateTime InvoicedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Number { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Party RecipientNavigation { get; set; } = null!;

    public virtual Party SenderNavigation { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
