namespace Bacera.Gateway;

public class CentralReferralCode
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public long TenantId { get; set; }
    public long AccountId { get; set; }

    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}