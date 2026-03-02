using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateBaseSchema
{
    public long Id { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Note { get; set; } = null!;

    public virtual Party CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<RebateBaseSchemaItem> RebateBaseSchemaItems { get; set; } = new List<RebateBaseSchemaItem>();

    public virtual ICollection<TradeAccount> TradeAccounts { get; set; } = new List<TradeAccount>();
}
