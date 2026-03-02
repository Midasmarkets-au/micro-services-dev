namespace Bacera.Gateway;

public partial class WalletAdjust
{
    public long Id { get; set; }
    public long WalletId { get; set; }
    public short SourceType { get; set; }
    public long Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string Comment { get; set; } = "";
    public virtual Wallet Wallet { get; set; } = null!;
    public virtual Matter IdNavigation { get; set; } = null!;

    public virtual ICollection<SalesRebate> SalesRebates { get; set; } = new List<SalesRebate>();
}