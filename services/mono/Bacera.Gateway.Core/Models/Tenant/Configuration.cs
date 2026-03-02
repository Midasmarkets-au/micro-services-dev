using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Configuration
{
    public long Id { get; set; }

    public long RowId { get; set; }
    public string DataFormat { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Description { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public long UpdatedBy { get; set; }

    public string Name { get; set; } = null!;
    // To Be Removed
}