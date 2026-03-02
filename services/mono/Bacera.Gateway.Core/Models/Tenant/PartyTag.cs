namespace Bacera.Gateway;

public class PartyTag
{
    public long PartyId { get; set; }
    public long TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;

    public static PartyTag Build(long partyId, long tagId)
        => new() { PartyId = partyId, TagId = tagId };
}