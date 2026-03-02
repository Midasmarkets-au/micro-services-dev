namespace Bacera.Gateway;

public partial class TradeAccountLoginLog
{
    public long Id { get; set; }

    public long AccountNumber { get; set; }

    public string Ip { get; set; } = null!;

    public DateTime LoginTime { get; set; }
}