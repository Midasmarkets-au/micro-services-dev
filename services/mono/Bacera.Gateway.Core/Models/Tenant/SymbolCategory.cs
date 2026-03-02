using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class SymbolCategory
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Symbol> Symbols { get; set; } = new List<Symbol>();
}
