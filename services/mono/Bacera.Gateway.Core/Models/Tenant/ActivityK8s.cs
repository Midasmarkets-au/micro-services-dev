using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway;

[Table("activity_k8s", Schema = "core")]
public partial class ActivityK8s
{
    [Column("id")] public long Id { get; set; }
    [Column("matter_id")] public long MatterId { get; set; }
    [Column("party_id")] public long PartyId { get; set; }
    [Column("performed_on")] public DateTime PerformedOn { get; set; }
    [Column("on_state_id")] public int OnStateId { get; set; }
    [Column("action_id")] public int ActionId { get; set; }
    [Column("to_state_id")] public int ToStateId { get; set; }
    [Column("data")] public string Data { get; set; } = string.Empty;

    public virtual MatterK8s Matter { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;
}
