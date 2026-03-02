namespace Bacera.Gateway;

public partial class AccountTag
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}