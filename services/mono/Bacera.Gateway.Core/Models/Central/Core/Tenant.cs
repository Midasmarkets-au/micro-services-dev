namespace Bacera.Gateway;

public partial class Tenant
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }
    public bool ApiLogEnable { get; set; }

    public virtual ICollection<Domain> Domains { get; set; } = new List<Domain>();
    public virtual ICollection<CentralAccount> CentralAccounts { get; set; } = new List<CentralAccount>();

    public virtual ICollection<CentralReferralCode> CentralReferralCodes { get; set; } =
        new List<CentralReferralCode>();

    public virtual ICollection<MetaTrade> CentralTrades { get; set; } = new List<MetaTrade>();
}