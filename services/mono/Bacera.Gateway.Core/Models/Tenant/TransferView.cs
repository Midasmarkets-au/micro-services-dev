namespace Bacera.Gateway;

public partial class TransferView
{
    public long Id { get; set; }

    public int? Type { get; set; }

    public int? StateId { get; set; }

    public DateTime? PostedOn { get; set; }
    public DateTime? StatedOn { get; set; }

    public long? Amount { get; set; }

    public int? CurrencyId { get; set; }

    public long? PartyId { get; set; }

    public int? LedgerSide { get; set; }

    public int? FundType { get; set; }

    public TransactionAccountTypes? SourceAccountType { get; set; }

    public long? SourceAccountId { get; set; }

    public TransactionAccountTypes? TargetAccountType { get; set; }

    public long? TargetAccountId { get; set; }

    public long? PrevBalance { get; set; }
}