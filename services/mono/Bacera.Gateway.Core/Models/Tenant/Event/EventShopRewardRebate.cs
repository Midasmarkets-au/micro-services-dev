namespace Bacera.Gateway;

public partial class EventShopRewardRebate
{
    public long Id { get; set; }
    public long EventShopRewardId { get; set; }
    public long Ticket { get; set; }
    public long Amount { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual EventShopReward EventShopReward { get; set; } = null!;
}