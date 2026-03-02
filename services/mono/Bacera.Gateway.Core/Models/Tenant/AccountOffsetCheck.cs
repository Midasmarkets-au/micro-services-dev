namespace Bacera.Gateway;

public partial class AccountCheck
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string AccountNumberContent { get; set; } = null!;

    public short Type { get; set; }
    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public long OperatorPartyId { get; set; }

    public virtual Party OperatorParty { get; set; } = null!;
}