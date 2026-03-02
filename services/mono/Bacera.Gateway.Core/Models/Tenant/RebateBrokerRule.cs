using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateBrokerRule
{
    public long Id { get; set; }

    public long BrokerAccountId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string AllowAccountRoles { get; set; } = null!;

    public string AllowAccountTypes { get; set; } = null!;

    public string Schema { get; set; } = null!;

    public virtual Account BrokerAccount { get; set; } = null!;
}
