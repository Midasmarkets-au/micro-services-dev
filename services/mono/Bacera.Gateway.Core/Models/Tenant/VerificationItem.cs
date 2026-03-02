using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class VerificationItem
{
    public long Id { get; set; }

    public long VerificationId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Category { get; set; } = null!;

    public string Content { get; set; } = null!;

    public virtual Verification Verification { get; set; } = null!;
}
