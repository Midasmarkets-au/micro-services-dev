namespace Bacera.Gateway;

public partial class AccountReport
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public int ServiceId { get; set; }
    public long PartyId { get; set; }
    public long AccountId { get; set; }
    public long AccountNumber { get; set; }

    public DateTime Date { get; set; }
    public string DataFile { get; set; } = null!;
    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public DateTime? TryTime { get; set; }
    public int Tries { get; set; }
    public int Type { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;
}