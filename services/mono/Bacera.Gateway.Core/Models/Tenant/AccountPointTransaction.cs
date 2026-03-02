using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class AccountPointTransaction
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public int Type { get; set; }

    public DateTime CreatedOn { get; set; }

    public long PreviousBalance { get; set; }

    public long Point { get; set; }

    public string Note { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;
}
