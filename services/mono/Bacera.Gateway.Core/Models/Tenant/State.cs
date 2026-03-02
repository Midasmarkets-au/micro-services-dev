using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class State
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Transition> TransitionOnStates { get; set; } = new List<Transition>();

    public virtual ICollection<Transition> TransitionToStates { get; set; } = new List<Transition>();
}
