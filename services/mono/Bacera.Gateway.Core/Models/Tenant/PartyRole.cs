using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class PartyRole
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int RoleId { get; set; }

    public virtual Party Party { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
