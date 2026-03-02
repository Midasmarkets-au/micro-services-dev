using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class NotificationSubscription
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public long NotificationEventId { get; set; }

    public short IsActivated { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual NotificationEvent NotificationEvent { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}
