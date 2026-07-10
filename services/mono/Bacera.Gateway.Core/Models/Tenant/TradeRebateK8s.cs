using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("trade_rebate_k8s", Schema = "trd")]
public partial class TradeRebateK8s
{
    [Column("id")] public long Id { get; set; }
    [Column("account_id")] public long? AccountId { get; set; }
    [Column("trade_service_id")] public int TradeServiceId { get; set; }
    [Column("ticket")] public long Ticket { get; set; }
    [Column("account_number")] public long AccountNumber { get; set; }
    [Column("currency_id")] public int CurrencyId { get; set; }
    [Column("volume")] public int Volume { get; set; }
    [Column("status")] public int Status { get; set; }
    [Column("rule_type")] public int RuleType { get; set; }
    [Column("created_on")] public DateTime CreatedOn { get; set; }
    [Column("updated_on")] public DateTime UpdatedOn { get; set; }
    [Column("closed_on")] public DateTime ClosedOn { get; set; }
    [Column("opened_on")] public DateTime OpenedOn { get; set; }
    [Column("time_stamp")] public long TimeStamp { get; set; }
    [Column("action")] public int Action { get; set; }
    [Column("deal_id")] public long DealId { get; set; }
    [Column("symbol")] public string Symbol { get; set; } = null!;
    [Column("refer_path")] public string ReferPath { get; set; } = null!;
    [Column("commission")] public decimal Commission { get; set; }
    [Column("swaps")] public decimal Swaps { get; set; }
    [Column("open_price")] public decimal OpenPrice { get; set; }
    [Column("close_price")] public decimal ClosePrice { get; set; }
    [Column("profit")] public decimal Profit { get; set; }
    [Column("reason")] public int Reason { get; set; }

    public virtual Account? Account { get; set; }
}
