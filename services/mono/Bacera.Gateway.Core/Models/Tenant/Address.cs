namespace Bacera.Gateway;

public partial class Address
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string Name { get; set; } = "";
    public string CCC { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Country { get; set; } = "";
    public string Content { get; set; } = "{}";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }

    public virtual Party Party { get; set; } = null!;
    public virtual ICollection<EventShopOrder> EventShopOrders { get; set; } = new List<EventShopOrder>();
}