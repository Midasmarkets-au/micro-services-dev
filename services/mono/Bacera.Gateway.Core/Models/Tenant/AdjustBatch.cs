namespace Bacera.Gateway;

public partial class AdjustBatch
{
    public long Id { get; set; }
    public int ServiceId { get; set; }
    public short Type { get; set; }
    public long OperatorPartyId { get; set; }
    public string File { get; set; } = "";
    public string Note { get; set; } = "";
    public string Result { get; set; } = "{}";
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual ICollection<AdjustRecord> AdjustRecords { get; set; } = new List<AdjustRecord>();
    public virtual Party OperatorParty { get; set; } = null!;
}