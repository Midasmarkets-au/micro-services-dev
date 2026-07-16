using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("matter_k8s", Schema = "core")]
public partial class MatterK8s
{
    [Column("id")] public long Id { get; set; }
    [Column("pid")] public long? Pid { get; set; }
    [Column("type")] public int Type { get; set; }
    [Column("state_id")] public int StateId { get; set; }
    [Column("posted_on")] public DateTime PostedOn { get; set; }
    [Column("stated_on")] public DateTime StatedOn { get; set; }

    public virtual ICollection<WalletTransactionK8s> WalletTransactions { get; set; } = new List<WalletTransactionK8s>();
    public virtual ICollection<ActivityK8s> Activities { get; set; } = new List<ActivityK8s>();
}
