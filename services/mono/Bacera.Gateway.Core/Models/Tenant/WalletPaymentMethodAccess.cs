namespace Bacera.Gateway;

public class WalletPaymentMethodAccess
{
    public long WalletId { get; set; }
    public long PaymentMethodId { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public long OperatedPartyId { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    public virtual Party OperatedParty { get; set; } = null!;
}