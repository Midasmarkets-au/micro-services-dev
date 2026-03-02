using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PartyRole> PartyRoles { get; set; } = new List<PartyRole>();

    public virtual ICollection<Transition> Transitions { get; set; } = new List<Transition>();
}
