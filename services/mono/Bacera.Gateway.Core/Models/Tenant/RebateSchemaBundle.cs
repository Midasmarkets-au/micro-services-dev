using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateSchemaBundle
{
    public long Id { get; set; }

    public int Type { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Note { get; set; } = null!;

    public string Data { get; set; } = null!;

    public virtual Party CreatedByNavigation { get; set; } = null!;
}
