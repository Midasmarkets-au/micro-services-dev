using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Application
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public short Type { get; set; }

    public short Status { get; set; }

    public long ReferenceId { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? RejectedOn { get; set; }

    public long? RejectedBy { get; set; }

    public string? RejectedReason { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public long? CompletedBy { get; set; }

    public string? Supplement { get; set; }

    public virtual Party Party { get; set; } = null!;
}
