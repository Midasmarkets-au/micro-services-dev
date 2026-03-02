namespace Bacera.Gateway;

public partial class WalletDailySnapshot
{
    public long Id { get; set; }

    public long WalletId { get; set; }

    public long Balance { get; set; }

    public DateTime SnapshotDate { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}