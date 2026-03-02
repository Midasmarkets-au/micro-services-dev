using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Verification
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public short Type { get; set; }

    public int Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Note { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual ICollection<VerificationItem> VerificationItems { get; set; } = new List<VerificationItem>();
}
