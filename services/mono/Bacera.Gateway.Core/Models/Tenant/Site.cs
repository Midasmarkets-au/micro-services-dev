using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Site
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Party> Parties { get; set; } = new List<Party>();
}