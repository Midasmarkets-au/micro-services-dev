namespace Bacera.Gateway;

public class AccountRebateView
{
    public long AccountId { get; set; }
    public long Amount { get; set; }
    public int CurrencyId { get; set; }
    public int FundType { get; set; }
    public long TradeAccountId { get; set; }
    public long AccountNumber { get; set; }
    public long Ticket { get; set; }
    public string Symbol { get; set; } = null!;
    public long Volume { get; set; }
    public double Profit { get; set; }
    public long SalesAccountId { get; set; }
    public long AgentAccountId { get; set; }
    public string Group { get; set; } = null!;
    public short Type { get; set; }
    public int SiteId { get; set; }
    public int Level { get; set; }
    public long Uid { get; set; }
    public string ReferPath { get; set; } = null!;
    public DateTime StatedOn { get; set; }
}