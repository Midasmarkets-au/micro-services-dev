namespace Bacera.Gateway;

public class AccountComment
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long OperatorPartyId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public virtual Account Account { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
}