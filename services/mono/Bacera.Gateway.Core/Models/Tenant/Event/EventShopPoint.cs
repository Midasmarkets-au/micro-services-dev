namespace Bacera.Gateway;

public partial class EventShopPoint
{
    public long Id { get; set; }
    public long EventPartyId { get; set; }
    public long Point { get; set; }
    public long TotalPoint { get; set; }
    public long FrozenPoint { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public virtual EventParty EventParty { get; set; } = null!;
}