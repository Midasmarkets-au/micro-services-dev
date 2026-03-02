using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class MatterType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Matter> Matters { get; set; } = new List<Matter>();
}
