namespace Bacera.Gateway;

public partial class EventShopOrder
{
    public long Id { get; set; }
    public long EventPartyId { get; set; }
    public long EventShopItemId { get; set; }
    public int Quantity { get; set; }
    public long TotalPoint { get; set; }
    public long AddressId { get; set; }
    public short Status { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public long OperatorPartyId { get; set; }
    public string Shipping { get; set; } = "";

    public virtual EventParty EventParty { get; set; } = null!;
    public virtual EventShopItem EventShopItem { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
}