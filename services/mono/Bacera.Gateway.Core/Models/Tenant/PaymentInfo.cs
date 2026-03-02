using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class PaymentInfo
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int PaymentPlatform { get; set; }

    public long? PaymentServiceId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Info { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual PaymentService? PaymentService { get; set; }
}
