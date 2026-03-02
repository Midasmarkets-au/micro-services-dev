namespace Bacera.Gateway;

public partial class PaymentServiceAccess
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public long PaymentServiceId { get; set; }

    public int FundType { get; set; }

    public int CurrencyId { get; set; }

    public short CanDeposit { get; set; }

    public short CanWithdraw { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual PaymentService PaymentService { get; set; } = null!;
}
