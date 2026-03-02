namespace Bacera.Gateway;

public class AccountTradeView
{
    public long AccountId { get; set; }
    public string ReferPath { get; set; } = null!;
    public int CurrencyId { get; set; }
    public string Symbol { get; set; } = null!;
    public long Volume { get; set; }
    public long Amount { get; set; }
    public DateTime OpenedOn { get; set; }
    public DateTime ClosedOn { get; set; }
}