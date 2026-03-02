namespace Bacera.Gateway;

public partial class CryptoTransaction
{
    public long Id { get; set; }
    public long CryptoId { get; set; }
    public bool Confirmed { get; set; }
    public long? PaymentId { get; set; }

    public long Amount { get; set; }
    public short Status { get; set; }

    public string FromAddress { get; set; } = null!;
    public string Data { get; set; } = null!;
    public DateTime CreatedOn { get; set; }

    public string TransactionHash { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
    public virtual Crypto Crypto { get; set; } = null!;
}