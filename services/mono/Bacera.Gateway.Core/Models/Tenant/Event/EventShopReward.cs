namespace Bacera.Gateway;

public partial class EventShopReward
{
    public long Id { get; set; }
    public long EventPartyId { get; set; }
    public long EventShopItemId { get; set; }
    public long TotalPoint { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime EffectiveTo { get; set; }
    public long OperatorPartyId { get; set; }
    public virtual EventParty EventParty { get; set; } = null!;
    public virtual EventShopItem EventShopItem { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;

    public virtual ICollection<EventShopRewardRebate> EventShopRewardRebates { get; set; } =
        new List<EventShopRewardRebate>();
}