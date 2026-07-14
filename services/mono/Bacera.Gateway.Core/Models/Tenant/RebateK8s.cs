using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("rebate_k8s", Schema = "trd")]
public partial class RebateK8s
{
    [Column("id")] public long Id { get; set; }
    [Column("party_id")] public long PartyId { get; set; }
    [Column("account_id")] public long AccountId { get; set; }
    [Column("fund_type")] public int FundType { get; set; }
    [Column("currency_id")] public int CurrencyId { get; set; }
    [Column("amount")] public decimal Amount { get; set; }
    [Column("trade_rebate_id")] public long? TradeRebateId { get; set; }
    [Column("hold_until_on")] public DateTime? HoldUntilOn { get; set; }
    [Column("information")] public string Information { get; set; } = null!;
    [Column("created_on")] public DateTime CreatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;
    public virtual MatterK8s Matter { get; set; } = null!;
}
