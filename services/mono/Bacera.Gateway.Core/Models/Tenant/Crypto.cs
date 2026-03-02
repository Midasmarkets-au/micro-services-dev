namespace Bacera.Gateway;

public partial class Crypto
{
    public long Id { get; set; }
    public string Address { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public short Status { get; set; }
    public long OperatorPartyId { get; set; }
    public long? InUsePaymentId { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public virtual Party OperatorParty { get; set; } = null!;
    public virtual Payment? InUsePayment { get; set; }

    public virtual ICollection<CryptoTransaction> CryptoTransactions { get; set; } = new List<CryptoTransaction>();
}