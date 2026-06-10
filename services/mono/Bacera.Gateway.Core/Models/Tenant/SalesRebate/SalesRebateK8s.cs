using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("sales_rebate_k8s", Schema = "trd")]
public class SalesRebateK8s
{
    [Column("id")]               public long     Id             { get; set; }
    [Column("sales_account_id")] public long     SalesAccountId { get; set; }
    [Column("period_start")]     public DateTime PeriodStart    { get; set; }
    [Column("period_end")]       public DateTime PeriodEnd      { get; set; }
    [Column("schedule_type")]    public short    ScheduleType   { get; set; }
    [Column("total_amount")]          public decimal  TotalAmount          { get; set; }
    [Column("trade_count")]           public int      TradeCount           { get; set; }
    [Column("status")]                public short    Status               { get; set; }
    [Column("wallet_transaction_id")] public long?    WalletTransactionId  { get; set; }
    [Column("matter_id")]             public long?    MatterId             { get; set; }
    [Column("created_on")]            public DateTime CreatedOn            { get; set; }
}
