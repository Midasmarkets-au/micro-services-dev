using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Supplement
{
    public long Id { get; set; }

    public long RowId { get; set; }

    public int Type { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Data { get; set; } = null!;
}
