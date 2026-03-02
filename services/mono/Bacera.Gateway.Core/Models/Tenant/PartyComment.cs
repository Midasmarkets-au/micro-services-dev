namespace Bacera.Gateway;

public class PartyComment
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public long OperatorPartyId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public virtual Party Party { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
}