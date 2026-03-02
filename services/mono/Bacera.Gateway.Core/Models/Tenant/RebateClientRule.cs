using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateClientRule
{
    public long Id { get; set; }

    public long ClientAccountId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public short DistributionType { get; set; }
    public string Schema { get; set; } = null!;

    public long? RebateDirectSchemaId { get; set; }

    public virtual Account ClientAccount { get; set; } = null!;

    public virtual RebateDirectSchema? RebateDirectSchema { get; set; }
}