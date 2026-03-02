using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Payment
{
    public long Id { get; set; }

    public long? Pid { get; set; }

    public long PartyId { get; set; }

    public short LedgerSide { get; set; }

    public long PaymentServiceId { get; set; }

    public int CurrencyId { get; set; }

    public long Amount { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public short Status { get; set; }

    public string Number { get; set; } = null!;

    public string ReferenceNumber { get; set; } = null!;
    public string CallbackBody { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Payment> InversePidNavigation { get; set; } = new List<Payment>();

    public virtual Party Party { get; set; } = null!;

    // Note: PaymentServiceId actually references PaymentMethod.Id (legacy naming)
    // PaymentService table is deprecated and no longer used
    // public virtual PaymentService PaymentService { get; set; } = null!;
    
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual Payment? PidNavigation { get; set; }

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();

    public virtual Crypto? InUseCrypto { get; set; }

    public virtual CryptoTransaction? CryptoTransaction { get; set; }
}
