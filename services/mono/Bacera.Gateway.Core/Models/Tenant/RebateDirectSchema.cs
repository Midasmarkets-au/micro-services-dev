using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateDirectSchema
{
    public long Id { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public long? ConfirmedBy { get; set; }

    public DateTime? ConfirmedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Note { get; set; } = null!;

    public virtual Party? ConfirmedByNavigation { get; set; }

    public virtual Party CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<RebateClientRule> RebateClientRules { get; set; } = new List<RebateClientRule>();

    public virtual ICollection<RebateDirectRule> RebateDirectRules { get; set; } = new List<RebateDirectRule>();

    public virtual ICollection<RebateDirectSchemaItem> RebateDirectSchemaItems { get; set; } = new List<RebateDirectSchemaItem>();
}
