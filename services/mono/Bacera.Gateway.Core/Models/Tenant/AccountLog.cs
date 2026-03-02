namespace Bacera.Gateway;

public partial class AccountLog
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long OperatorPartyId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Before { get; set; } = string.Empty;
    public string After { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual Party OperatorParty { get; set; } = null!;
}