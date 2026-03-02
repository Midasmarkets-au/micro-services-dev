using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class ReportRequest
{
    public long Id { get; set; }

    public int Type { get; set; }

    public long PartyId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? GeneratedOn { get; set; }

    public DateTime? ExpireOn { get; set; }

    public string Name { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string Query { get; set; } = null!;

    public int IsFromApi { get; set; } = 0;

    public virtual Party Party { get; set; } = null!;
}
