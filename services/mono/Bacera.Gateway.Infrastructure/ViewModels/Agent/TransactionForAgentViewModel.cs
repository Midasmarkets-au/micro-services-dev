using Bacera.Gateway.ViewModels.Tenant;

namespace Bacera.Gateway.Agent;

public class TransactionForAgentViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public FundTypes FundType { get; set; }
    public AccountBasicViewModel SourceAccount { get; set; } = new();
    public AccountBasicViewModel TargetAccount { get; set; } = new();
    public int CurrencyId { get; set; }
    public long Amount { get; set; }
    public int Status { get; set; }
    public StateTypes StateId { get; set; }
    public DateTime CreatedOn { get; set; }
    public ParentUserBasicModel? User { get; set; }
}