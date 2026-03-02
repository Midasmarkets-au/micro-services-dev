using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Charge
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Apcode { get; set; } = null!;

    public string Arcode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Ledger> Ledgers { get; set; } = new List<Ledger>();
}
