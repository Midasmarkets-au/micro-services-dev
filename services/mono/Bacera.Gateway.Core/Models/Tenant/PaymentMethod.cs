namespace Bacera.Gateway;

public partial class PaymentMethod
{
    public long Id { get; set; }

    public int Platform { get; set; }

    public string MethodType { get; set; } = null!;

    public int CurrencyId { get; set; }

    public int Percentage { get; set; }

    public long InitialValue { get; set; }

    public long MinValue { get; set; }

    public long MaxValue { get; set; }

    public string Name { get; set; } = null!;

    public string Configuration { get; set; } = null!;
    public string AvailableCurrencies { get; set; } = null!;
    
    public string CommentCode { get; set; } = null!;

    public short IsHighDollarEnabled { get; set; }

    public short IsAutoDepositEnabled { get; set; }

    public string Group { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string Note { get; set; } = null!;

    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }

    public long OperatorPartyId { get; set; }

    public int Sort { get; set; }

    public bool IsDeleted { get; set; }


    // public virtual ICollection<PaymentMethodAccess> PaymentMethodAccess { get; set; } = new List<PaymentMethodAccess>();
    public virtual Party OperatorParty { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<PayoutRecord> PayoutRecords { get; set; } = new List<PayoutRecord>();

    public virtual ICollection<AccountPaymentMethodAccess> AccountPaymentMethodAccesses { get; set; } =
        new List<AccountPaymentMethodAccess>();

    public virtual ICollection<WalletPaymentMethodAccess> WalletPaymentMethodAccesses { get; set; } =
        new List<WalletPaymentMethodAccess>();
}