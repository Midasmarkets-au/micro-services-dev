namespace Bacera.Gateway;

public class AccountWithdrawalView
{
    public long WithdrawalId { get; set; }
    public int FundType { get; set; }
    public int CurrencyId { get; set; }
    public long Amount { get; set; }
    public DateTime PostedOn { get; set; }
    public DateTime StatedOn { get; set; }
    public long AccountId { get; set; }
    public long PartyId { get; set; }
    public long Uid { get; set; }
    public long AccountNumber { get; set; }
    public int Level { get; set; }
    public string ReferPath { get; set; } = string.Empty;
    public int Type { get; set; }
    public long? AgentAccountId { get; set; }
    public long? SalesAccountId { get; set; }
    public string Group { get; set; } = string.Empty;
}