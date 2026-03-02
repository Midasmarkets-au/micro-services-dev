namespace Bacera.Gateway;

public partial class EventShopItem
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public short Type { get; set; }
    public short Category { get; set; }

    public string AccessRoles { get; set; } = "[]";
    public string AccessSites { get; set; } = "[]";
    public string Configuration { get; set; } = "{}";
    public long Point { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<EventShopItemLanguage> EventShopItemLanguages { get; set; } =
        new List<EventShopItemLanguage>();

    public virtual ICollection<EventShopOrder> EventShopOrders { get; set; } =
        new List<EventShopOrder>();

    public virtual ICollection<EventShopReward> EventShopRewards { get; set; } = new List<EventShopReward>();
}