namespace Bacera.Gateway;

public partial class EventShopClientPoint
{
    public long Id { get; set; }
    public long ChildAccountId { get; set; }
    public long ParentAccountId { get; set; }
    public short ParentAccountRole { get; set; }
    public short OpenAccount { get; set; }
    public int Volume { get; set; }
    public long DepositAmount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual Account ChildAccount { get; set; } = null!;
    public virtual Account ParentAccount { get; set; } = null!;
}