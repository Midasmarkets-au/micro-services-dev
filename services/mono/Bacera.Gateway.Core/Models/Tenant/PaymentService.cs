using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class PaymentService
{
    public long Id { get; set; }

    public int Platform { get; set; }

    public int CurrencyId { get; set; }

    public int Sequence { get; set; }

    public short IsActivated { get; set; }

    public short CanDeposit { get; set; }

    public short CanWithdraw { get; set; }

    public long InitialValue { get; set; }

    public long MinValue { get; set; }

    public long MaxValue { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string Configuration { get; set; } = null!;

    public short IsHighDollarEnabled { get; set; }
    public short IsAutoDepositEnabled { get; set; }

    public string CommentCode { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PaymentInfo> PaymentInfos { get; set; } = new List<PaymentInfo>();

    public virtual ICollection<PaymentServiceAccess> PaymentServiceAccesses { get; set; } =
        new List<PaymentServiceAccess>();

    // DEPRECATED: Payment now uses PaymentMethod.Payments collection instead
    // Commenting this out to prevent EF from creating shadow FK column (PaymentServiceId1)
    // public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<FundType> FundTypes { get; set; } = new List<FundType>();
}