namespace Bacera.Gateway;

public partial class SalesRebateSchema
{
    public long Id { get; set; }

    public long SalesAccountId { get; set; }

    public long RebateAccountId { get; set; }
    public long OperatorPartyId { get; set; }

    public int Rebate { get; set; }
    public int SalesType { get; set; }

    public string ExcludeAccount { get; set; } = null!;

    public string ExcludeSymbol { get; set; } = null!;

    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string? Note { get; set; }

    public int AlphaRebate { get; set; }
    public int ProRebate { get; set; }

    public short Schedule { get; set; }

    public virtual Account RebateAccount { get; set; } = null!;

    public virtual Account SalesAccount { get; set; } = null!;

    public virtual Party OperatorParty { get; set; } = null!;
}
