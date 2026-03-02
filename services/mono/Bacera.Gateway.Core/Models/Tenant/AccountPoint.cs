using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class AccountPoint
{
    public long AccountId { get; set; }

    public long Balance { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
