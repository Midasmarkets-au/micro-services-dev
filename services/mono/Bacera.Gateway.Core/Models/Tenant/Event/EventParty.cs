namespace Bacera.Gateway;

public partial class EventParty
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public long PartyId { get; set; }
    public short Status { get; set; }
    public string Settings { get; set; } = "[]";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public long OperatorPartyId { get; set; }
    public virtual Event Event { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
    public virtual EventShopPoint EventShopPoint { get; set; } = null!;

    public virtual ICollection<EventShopPointTransaction> EventShopPointTransactions { get; set; } =
        new List<EventShopPointTransaction>();

    public virtual ICollection<EventShopOrder> EventShopOrders { get; set; } = new List<EventShopOrder>();
    public virtual ICollection<EventShopReward> EventShopRewards { get; set; } = new List<EventShopReward>();
}