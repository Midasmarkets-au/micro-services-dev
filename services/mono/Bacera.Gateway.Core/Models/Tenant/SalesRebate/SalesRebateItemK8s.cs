using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("sales_rebate_item_k8s", Schema = "trd")]
public class SalesRebateItemK8s
{
    [Column("id")]                         public long     Id                     { get; set; }
    [Column("sales_rebate_id")]            public long     SalesRebateId          { get; set; }
    [Column("sales_account_id")]           public long     SalesAccountId         { get; set; }
    [Column("trade_rebate_id")]            public long     TradeRebateId          { get; set; }
    [Column("ticket")]                     public long     Ticket                 { get; set; }
    [Column("trade_account_id")]           public long     TradeAccountId         { get; set; }
    [Column("trade_account_number")]       public long     TradeAccountNumber     { get; set; }
    [Column("trade_account_type")]         public short    TradeAccountType       { get; set; }
    [Column("trade_account_fund_type")]    public int      TradeAccountFundType   { get; set; }
    [Column("trade_account_currency_id")]  public int      TradeAccountCurrencyId { get; set; }
    [Column("symbol")]                     public string   Symbol                 { get; set; } = "";
    [Column("volume")]                     public int      Volume                 { get; set; }
    [Column("rebate_base")]                public decimal  RebateBase             { get; set; }
    [Column("rebate_type")]                public string   RebateType             { get; set; } = "";
    [Column("amount")]                     public decimal  Amount                 { get; set; }
    [Column("closed_on")]                  public DateTime ClosedOn               { get; set; }
    [Column("created_on")]                 public DateTime CreatedOn              { get; set; }
    [Column("excluded")]                   public bool     Excluded               { get; set; }
}
