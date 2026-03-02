namespace Bacera.Gateway;

public partial class Tag
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;

    public virtual ICollection<PartyTag> PartyTags { get; set; } = new List<PartyTag>();

    public virtual ICollection<Party> Parties { get; set; } = new List<Party>();
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}