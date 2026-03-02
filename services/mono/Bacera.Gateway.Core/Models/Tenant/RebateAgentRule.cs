using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateAgentRule
{
    public long Id { get; set; }

    public long? ParentId { get; set; }

    public long AgentAccountId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Schema { get; set; } = null!;

    public string LevelSetting { get; set; } = null!;

    public virtual Account AgentAccount { get; set; } = null!;

    public virtual ICollection<RebateAgentRule> InverseParent { get; set; } = new List<RebateAgentRule>();

    public virtual RebateAgentRule? Parent { get; set; }
}
