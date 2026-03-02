using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Activity
{
    public long Id { get; set; }

    public long MatterId { get; set; }

    public int ActionId { get; set; }

    public int OnStateId { get; set; }

    public int ToStateId { get; set; }

    public long PartyId { get; set; }

    public DateTime PerformedOn { get; set; }

    public string Data { get; set; } = null!;

    public virtual Matter Matter { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}
