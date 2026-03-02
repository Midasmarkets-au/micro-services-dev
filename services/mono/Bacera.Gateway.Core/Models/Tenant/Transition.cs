using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Transition
{
    public long Id { get; set; }

    public int ActionId { get; set; }

    public int OnStateId { get; set; }

    public int ToStateId { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Action Action { get; set; } = null!;

    public virtual State OnState { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual State ToState { get; set; } = null!;
}
