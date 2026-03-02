namespace Bacera.Gateway;

public partial class Domain
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string DomainName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
