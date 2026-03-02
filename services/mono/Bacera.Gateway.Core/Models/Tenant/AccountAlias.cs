namespace Bacera.Gateway;

public class AccountAlias
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public long AccountId { get; set; }
    public string Alias { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;
}