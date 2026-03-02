using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Symbol
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Category { get; set; } = null!;
    public int CategoryId { get; set; }

    public int Type { get; set; }

    public DateTime CreatedOn { get; set; }
    public long OperatorPartyId { get; set; }

    public virtual Party OperatorParty { get; set; } = null!;
}
