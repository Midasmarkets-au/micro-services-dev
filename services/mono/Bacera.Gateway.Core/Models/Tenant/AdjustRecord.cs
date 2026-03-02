namespace Bacera.Gateway;

public partial class AdjustRecord
{
    public long Id { get; set; }
    public long AccountNumber { get; set; }
    public long Amount { get; set; }
    public string Comment { get; set; } = "";

    public long? AdjustBatchId { get; set; }
    public long? AccountId { get; set; }
    public short Type { get; set; }
    public long Ticket { get; set; }
    public short Status { get; set; }
    public long OperatorPartyId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public virtual Account? Account { get; set; }
    public virtual AdjustBatch? AdjustBatch { get; set; }
    public virtual Party OperatorParty { get; set; } = null!;
}