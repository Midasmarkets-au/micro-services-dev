using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("wallet_transaction_k8s", Schema = "acct")]
public class WalletTransactionK8s
{
    [Column("id")]           public long     Id          { get; set; }
    [Column("wallet_id")]    public long     WalletId    { get; set; }
    [Column("invoice_id")]   public long?    InvoiceId   { get; set; }
    [Column("matter_id")]    public long     MatterId    { get; set; }
    [Column("prev_balance")] public decimal  PrevBalance { get; set; }
    [Column("amount")]       public decimal  Amount      { get; set; }
    [Column("created_on")]   public DateTime CreatedOn   { get; set; }
    [Column("updated_on")]   public DateTime UpdatedOn   { get; set; }
}
