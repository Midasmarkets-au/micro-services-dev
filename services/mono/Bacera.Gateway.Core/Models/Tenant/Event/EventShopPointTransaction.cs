namespace Bacera.Gateway;

public partial class EventShopPointTransaction
{
    public long Id { get; set; }
    public long EventPartyId { get; set; }
    public long Point { get; set; }
    public short SourceType { get; set; }
    public string SourceContent { get; set; } = "{}";
    public long SourceId { get; set; }
    public short Status { get; set; }
    public long? AccountId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual EventParty EventParty { get; set; } = null!;
    public virtual Account? Account { get; set; } = null!;
}