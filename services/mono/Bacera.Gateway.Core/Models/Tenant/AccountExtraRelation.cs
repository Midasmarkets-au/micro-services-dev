namespace Bacera.Gateway;

public class AccountExtraRelation
{
    public long ParentAccountId { get; set; }
    public long ChildAccountId { get; set; }
    public long OperatorPartyId { get; set; }
    public DateTime CreatedOn { get; set; }
    public virtual Account ParentAccount { get; set; } = null!;
    public virtual Account ChildAccount { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
}