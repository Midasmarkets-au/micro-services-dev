using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Withdrawal
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public long PaymentId { get; set; }

    public int FundType { get; set; }

    public int CurrencyId { get; set; }

    public long Amount { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public long? SourceAccountId { get; set; }
    public long? SourceWalletId { get; set; }

    public decimal ExchangeRate { get; set; }

    public DateTime ApprovedOn { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;

    public virtual Matter IdNavigation { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;

    public virtual Account? SourceAccount { get; set; }

    public virtual Wallet? SourceWallet { get; set; }
}