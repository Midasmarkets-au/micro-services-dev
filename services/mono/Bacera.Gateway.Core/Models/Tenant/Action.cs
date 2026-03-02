using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Action
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Transition> Transitions { get; set; } = new List<Transition>();
}
