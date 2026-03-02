namespace Bacera.Gateway;

public partial class CentralAccount
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public int ServiceId { get; set; }
    public long AccountId { get; set; }
    public long AccountNumber { get; set; }
    public long Uid { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}