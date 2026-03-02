using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Group
{
    public long Id { get; set; }

    public long OwnerAccountId { get; set; }

    public int Type { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual Account OwnerAccount { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
